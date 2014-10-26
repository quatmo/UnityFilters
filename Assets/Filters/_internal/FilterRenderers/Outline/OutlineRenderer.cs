using Filters;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum OutlineMethod : int {
	ExtrudeNormals, ScreenSpaceAnalysis
}

public enum OutlineFilterOutput : int {
	Release, DebugEncoding, DebugEncodingIntermediate1, DebugOutlines
}


public class OutlineRenderer : BaseFilterRenderer
{
	private OutlineSettings settings;

	private Material outlineEncodingMat;
	private Material blendModesMat;

//	public OutlineFilterOutput output;
//	public OutlineMethod method = OutlineMethod.ExtrudeNormals;

	private const int PRIME_COLOR_PASS = 0;
	private const int EXTRUDE_NORMALS_PASS = 1;
	private const int SCREEN_PASS_OUTLINES = 2;

	private const float MAX_DEPTH = 255f;

	public OutlineRenderer (OutlineSettings settings) : base(settings)
	{
		this.settings = settings;
	}

	protected override void CheckResources() {
		Shader outlineEncodingShader = Shader.Find("Hidden/Filters/OutlineEncoder"); 
		outlineEncodingMat = new Material(outlineEncodingShader);

		Shader blendShader = Shader.Find("Hidden/Filters/BlendModes");
		blendModesMat = new Material(blendShader);
	}


	private void PrimeColorBuffer(OutlineFilter subscriber) {
		// Encode color and depth information for the color prime shader
		Color color = subscriber.color;
		color.a = subscriber.depth/MAX_DEPTH;

		outlineEncodingMat.SetColor("_MainColor", color);
		outlineEncodingMat.SetFloat("_Width", subscriber.width);
		
		// Pass is activated after(!) setting the material properties. Otherwise, they don't seem to update properly for rendering.
		// Haven't found any mentioning of this behaviour in the documentation ...
		if(!outlineEncodingMat.SetPass(PRIME_COLOR_PASS)) return;
		
		MeshFilter[] meshFilters = subscriber.GetComponentsInChildren<MeshFilter>();

		System.Array.ForEach( meshFilters, meshFilter => {
			Transform transform = meshFilter.gameObject.transform;
			Graphics.DrawMeshNow( meshFilter.mesh, transform.localToWorldMatrix );
		});
	}

	
	private void ExtrudeNormals(OutlineFilter subscriber) {
		// The last 4 bits are currently free
		outlineEncodingMat.SetColor("_MainColor", subscriber.color);
		outlineEncodingMat.SetFloat("_Width", subscriber.width);
		
		// Pass is activated after(!) setting the material properties. Otherwise, they don't seem to update properly for rendering.
		// Haven't found any mentioning of this behaviour in the documentation ...
		if(!outlineEncodingMat.SetPass(EXTRUDE_NORMALS_PASS)) return;
		
		MeshFilter[] meshFilters = subscriber.GetComponentsInChildren<MeshFilter>();
		
		
		System.Array.ForEach( meshFilters, meshFilter => {
			Transform transform = meshFilter.gameObject.transform;
			Graphics.DrawMeshNow( meshFilter.mesh, transform.localToWorldMatrix );
		});
	}


	public RenderTexture RenderOutlineFilter(List<OutlineFilter> subscribers, Camera filterCamera, RenderTexture source, RenderTexture destination) {
		if(subscribers.Count == 0) return source;

		subscribers = subscribers.OrderBy( s => s.depth ).ToList();

		// Todo, do outlines a 1/4 screen res. Should give some nice blurring and make it run faster

		// Set handy values
		int screenW = (int) filterCamera.pixelWidth;
		int screenH = (int) filterCamera.pixelHeight;
		int screenW_4 = (int) screenW;
		int screenH_4 = (int) screenH;
		Vector4 texelSize = new Vector4(1f/screenW, 1f/screenH,1f,1f);

		// Grab render textures and acquire graphics state
		RenderTexture filterEncodingRT = RenderTexture.GetTemporary( screenW_4, screenH_4 ); 
		RenderTexture outlinesRT = RenderTexture.GetTemporary( screenW, screenH ); 
		Graphics.SetRenderTarget(filterEncodingRT);
		GL.Clear(true, true, new Color(0x0,0x0,0x0,0x0));

		// Render outlines
		switch(settings.method) {
			case OutlineMethod.ExtrudeNormals : 
				// Render outlined objects as solids to color buffer. We only want outlines around (not inside) the objects
				subscribers.ForEach( subscriber => PrimeColorBuffer(subscriber) );
				// Extrude normals
				Graphics.SetRenderTarget(outlinesRT);
				GL.Clear(true, true, new Color(0x0,0x0,0x0,0x0));
				subscribers.ForEach( subscriber => ExtrudeNormals(subscriber) );
				if(settings.output != OutlineFilterOutput.DebugEncodingIntermediate1) {
					// And finally cut out normally rendered objects from the extruded objects, thus creating the outlines.
					blendModesMat.SetTexture("_ColorBuffer", outlinesRT );
					Graphics.Blit(filterEncodingRT, outlinesRT, blendModesMat, 1); 
					// Maybe use the stencil buffer for this ?
				}
				break;
			case OutlineMethod.ScreenSpaceAnalysis :
				// Render outlined objects as solid to color buffer
				subscribers.ForEach( subscriber => PrimeColorBuffer(subscriber) );
				// Run kernel in screen space pass to compute normals
				outlineEncodingMat.SetTexture("_ColorBuffer", filterEncodingRT );
				outlineEncodingMat.SetVector("_TexelSize", texelSize);

				Graphics.Blit(filterEncodingRT, outlinesRT, outlineEncodingMat, SCREEN_PASS_OUTLINES); 
				break;
		}

		switch(settings.output) {
			case OutlineFilterOutput.DebugEncoding :
				Graphics.Blit(filterEncodingRT, destination);
				break;
			case OutlineFilterOutput.DebugOutlines :
				Graphics.Blit(outlinesRT, destination);
				break;
			case OutlineFilterOutput.DebugEncodingIntermediate1 :
				Graphics.Blit(outlinesRT, destination);
				break;
			case OutlineFilterOutput.Release :
				blendModesMat.SetTexture("_ColorBuffer", outlinesRT);
				Graphics.Blit(source, destination, blendModesMat, 2);
				break;
		}
		
		// Release render textures and free graphics state
		Graphics.SetRenderTarget(null);
		RenderTexture.ReleaseTemporary(filterEncodingRT);
		RenderTexture.ReleaseTemporary(outlinesRT);

		return destination;
	}
}

