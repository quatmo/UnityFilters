
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Filters {

	public enum GlowFilterOutput : int {
		Release, DebugEncoding, DebugGlow
	}

	public class GlowRenderer : BaseFilterRenderer
	{
		private enum BloomScreenBlendMode : int {
			Screen = 0,
			Add = 1,
		}

		private GlowSettings settings;

		public Shader screenBlendShader;
		private Material screenBlend;

		public Shader blurAndFlaresShader;
		private Material blurAndFlaresMaterial;

		public Shader brightPassFilterShader;
		private Material brightPassFilterMaterial;

		private Material glowEncodingMat;
		private Material blendModesMat;

		private BloomScreenBlendMode screenBlendMode = BloomScreenBlendMode.Add;

		public GlowRenderer (GlowSettings settings) : base(settings)
		{
			this.settings = settings;	
		}

		protected override void CheckResources() {
			brightPassFilterShader = Shader.Find("Hidden/BrightPassFilter2");
			brightPassFilterMaterial = CheckShaderAndCreateMaterial(brightPassFilterShader, brightPassFilterMaterial);

			screenBlendShader = Shader.Find("Hidden/BlendForBloom");
			screenBlend = CheckShaderAndCreateMaterial(screenBlendShader, screenBlend);

			blurAndFlaresShader = Shader.Find("Hidden/BlurAndFlares");
			blurAndFlaresMaterial = CheckShaderAndCreateMaterial (blurAndFlaresShader, blurAndFlaresMaterial);

			Shader glowEncodingShader = Shader.Find("Hidden/Filters/GlowEncoder"); 
			glowEncodingMat = new Material(glowEncodingShader);
			
			Shader blendShader = Shader.Find("Hidden/Filters/BlendModes");
			blendModesMat = new Material(blendShader);

		}

		public void RenderGlowFilter(List<GlowFilter> subscribers, Camera filterCamera, RenderTexture source, RenderTexture destination) {
			if(subscribers.Count == 0) return;

			// Set handy values
			int screenW = (int) filterCamera.pixelWidth;
			int screenH = (int) filterCamera.pixelHeight;
			
			// Grab render textures and acquire graphics state
			RenderTexture filterEncodingRT = RenderTexture.GetTemporary( screenW, screenH ); 
			RenderTexture glowRT = RenderTexture.GetTemporary( screenW, screenH ); 
			Graphics.SetRenderTarget(filterEncodingRT);
			GL.Clear(true, true, new Color(0x0,0x0,0x0,0x0));
			
			// Encode subscribers
			subscribers.ForEach( subscriber => EncodeGlowFilterSubscriber(subscriber) );
			
			// Send to filter rendering
			RenderBlur(filterEncodingRT, glowRT);
			
			switch(settings.output) {
				case GlowFilterOutput.DebugEncoding :
					Graphics.Blit(filterEncodingRT, destination);
					break;
				case GlowFilterOutput.DebugGlow :
					Graphics.Blit(glowRT, destination);
					break;
				case GlowFilterOutput.Release :
					blendModesMat.SetTexture("_ColorBuffer", glowRT);
					Graphics.Blit(source, destination, blendModesMat, 0);
					break;
			}
			
			// Release render textures and free graphics state
			Graphics.SetRenderTarget(null);
			RenderTexture.ReleaseTemporary(filterEncodingRT);
			RenderTexture.ReleaseTemporary(glowRT);
		}


		private void EncodeGlowFilterSubscriber(GlowFilter subscriber) {
			
			// The rgb channels are used for the glow color
			Color c = subscriber.color;
			// The first 4 bits of the alpha channel is used for storing strength
			c.a = subscriber.strength;
			// The last 4 bits are currently free
			glowEncodingMat.SetColor("_MainColor", c);
			
			// Pass is activated after(!) setting the material properties. Otherwise, they don't seem to update properly for rendering.
			// Haven't found any mentioning of this behaviour in the documentation ...
			if(!glowEncodingMat.SetPass(0)) return;
			
			MeshFilter[] meshFilters = subscriber.GetComponentsInChildren<MeshFilter>();
			
			
			// TODO Implementation does not currently take depth into account properly.
			// If we are running in deffered the depth texture should just be here ...
			System.Array.ForEach( meshFilters, meshFilter => {
				Transform transform = meshFilter.gameObject.transform;
				Graphics.DrawMeshNow( meshFilter.mesh, transform.localToWorldMatrix );
			});
		}

		private void BrightFilter (float thresh, RenderTexture from, RenderTexture to) {
			brightPassFilterMaterial.SetVector ("_Threshhold", new Vector4 (thresh, thresh, thresh, thresh));
			Graphics.Blit (from, to, brightPassFilterMaterial, 0);			
		}

		/// <summary>
		/// A modified version of the Unity Bloom image effect
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public void RenderBlur(RenderTexture source, RenderTexture destination) {
			//Graphics.Blit(source, destination);
			BloomScreenBlendMode realBlendMode  = screenBlendMode;

			var rtFormat = RenderTextureFormat.Default;
			var rtW2 = source.width/2;
			var rtH2 = source.height/2;
			var rtW4 = source.width/4;
			var rtH4 = source.height/4;
			
			float widthOverHeight = (1.0f * source.width) / (1.0f * source.height);
			float oneOverBaseSize = 1.0f / 512.0f;

			// downsample
			RenderTexture quarterRezColor = RenderTexture.GetTemporary (rtW4, rtH4, 0, rtFormat);
			RenderTexture halfRezColorDown = RenderTexture.GetTemporary (rtW2, rtH2, 0, rtFormat);
			Graphics.Blit (source, halfRezColorDown, screenBlend, 2);
			RenderTexture rtDown4 = RenderTexture.GetTemporary (rtW4, rtH4, 0, rtFormat);
			Graphics.Blit (halfRezColorDown, rtDown4, screenBlend, 2);
			Graphics.Blit (rtDown4, quarterRezColor, screenBlend, 6);
			RenderTexture.ReleaseTemporary(rtDown4);

			RenderTexture.ReleaseTemporary (halfRezColorDown);

			#region TODO should be configurable
			int bloomBlurIterations = 2;
			float sepBlurSpread = 2.5f;
			float bloomIntensity = 0.5f;
			float bloomThreshhold = 0.5f;
			//Color bloomThreshholdColor = Color.white;
			#endregion

			// cut colors (threshholding)			
			RenderTexture secondQuarterRezColor = RenderTexture.GetTemporary (rtW4, rtH4, 0, rtFormat);		
			BrightFilter(bloomThreshhold /* TODO only valid in js... * bloomThreshholdColor*/, quarterRezColor, secondQuarterRezColor);		

			bloomBlurIterations = Mathf.Clamp(bloomBlurIterations, 1, 10);


			for ( int iter = 0; iter < bloomBlurIterations; iter++ ) {
				float spreadForPass = (1.0f + (iter * 0.25f)) * sepBlurSpread;
				
				// vertical blur
				RenderTexture blur4 = RenderTexture.GetTemporary (rtW4, rtH4, 0, rtFormat);
				blurAndFlaresMaterial.SetVector ("_Offsets", new Vector4 (0.0f, spreadForPass * oneOverBaseSize, 0.0f, 0.0f));
				Graphics.Blit (secondQuarterRezColor, blur4, blurAndFlaresMaterial, 4);
				RenderTexture.ReleaseTemporary(secondQuarterRezColor);
				secondQuarterRezColor = blur4;
				
				// horizontal blur
				blur4 = RenderTexture.GetTemporary (rtW4, rtH4, 0, rtFormat);
				blurAndFlaresMaterial.SetVector ("_Offsets", new Vector4 ((spreadForPass / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));	
				Graphics.Blit (secondQuarterRezColor, blur4, blurAndFlaresMaterial, 4);
				RenderTexture.ReleaseTemporary (secondQuarterRezColor);
				secondQuarterRezColor = blur4;
				
				//if (quality > BloomQuality.Cheap) {
					if (iter == 0)
					{
						Graphics.SetRenderTarget(quarterRezColor);
						GL.Clear(false, true, Color.black); // Clear to avoid RT restore
						Graphics.Blit (secondQuarterRezColor, quarterRezColor);
					}
					else
					{
						quarterRezColor.MarkRestoreExpected(); // using max blending, RT restore expected
						Graphics.Blit (secondQuarterRezColor, quarterRezColor, screenBlend, 10);
					}
				//}
			}

			
//			if(quality > BloomQuality.Cheap)
//			{
				Graphics.SetRenderTarget(secondQuarterRezColor);
				GL.Clear(false, true, Color.black); // Clear to avoid RT restore
				Graphics.Blit (quarterRezColor, secondQuarterRezColor, screenBlend, 6); 
			//}
			
			
			
			int blendPass = (int) realBlendMode;
			//if(Mathf.Abs(chromaticBloom) < Mathf.Epsilon) 
			//	blendPass += 4;
			
			screenBlend.SetFloat ("_Intensity", bloomIntensity);
			screenBlend.SetTexture ("_ColorBuffer", source);
			
//			if(quality > BloomQuality.Cheap) {
			RenderTexture halfRezColorUp = RenderTexture.GetTemporary (rtW2, rtH2, 0, rtFormat);
			Graphics.Blit (secondQuarterRezColor, halfRezColorUp);
			Graphics.Blit (halfRezColorUp, destination, screenBlend, blendPass);
			RenderTexture.ReleaseTemporary (halfRezColorUp);
//			}
//			else
//				Graphics.Blit (secondQuarterRezColor, destination, screenBlend, blendPass);
			
			RenderTexture.ReleaseTemporary (quarterRezColor);	
			RenderTexture.ReleaseTemporary (secondQuarterRezColor);	


		}
	}
	
}

