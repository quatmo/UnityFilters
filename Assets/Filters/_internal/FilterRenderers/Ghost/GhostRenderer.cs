using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace Filters {


	public class GhostRenderer : BaseFilterRenderer {
		private Material ghostMat;

		private int PASS_PRIME_Z_BUFFER_BACKFACES = 0;
		//private int PASS_PRIME_Z_BUFFER_FRONTACES = 1;
		private int PASS_RENDER_BACKFACES = 2;
		//private int PASS_RENDER_FRONTFACES = 3;
		private int PASS_RENDER_DEBUG_SOLID = 4;

		public GhostRenderer(GhostSettings settings) : base(settings) {
		}

		protected override void CheckResources() {
			Shader animatedRimshader = Shader.Find("Custom/Vistra/AnimatedRimshader");
			ghostMat = new Material( animatedRimshader );
		}


		private void RenderBackside(GhostFilter subscriber, List<MeshFilter> meshFilters, Camera filterCamera) {
			bool debug = false;

			if (debug) {
				if (!ghostMat.SetPass(PASS_RENDER_DEBUG_SOLID)) {
					return;
				}
				
				meshFilters.ForEach( meshFilter => {
					Transform transform = meshFilter.gameObject.transform;
					Graphics.DrawMeshNow( meshFilter.mesh, transform.localToWorldMatrix );
				});
				return;
			}

			// Get references to screen buffers
//			RenderBuffer colorBuffer_screen = Graphics.activeColorBuffer;
//			RenderBuffer depthBuffer_screen = Graphics.activeDepthBuffer;
			// Since the object could be a hierachy of meshes and we only want the back of the combined (!) object,
			// we need to render all meshes and manually keep track of what the real backside if the combined meshes are.
			// So we grab a rendertexture for this purpose, and activate off screen rendering.
//			RenderTexture depthBuffer_ghost = RenderTexture.GetTemporary( (int) filterCamera.pixelWidth, (int) filterCamera.pixelHeight );
//			Graphics.SetRenderTarget(depthBuffer_ghost);
//			GL.Clear(true, true, new Color(0x0,0x0,0x0,0x0));

			// Set material properties
			ghostMat.SetTexture("_MainTex", subscriber.smokeTexture);
//			ghostMat.SetTexture("_DepthBuffer", depthBuffer_ghost);
			ghostMat.SetColor("_RimColor", subscriber.rimColor);

			// Pass is activated after(!) setting the material properties. Otherwise, they don't seem to update properly for rendering.
			// Haven't found any mentioning of this behaviour in the documentation ...
			if (!ghostMat.SetPass(PASS_PRIME_Z_BUFFER_BACKFACES)) {
				return;
			}


			meshFilters.ForEach( meshFilter => {
				Transform transform = meshFilter.gameObject.transform;
				Graphics.DrawMeshNow( meshFilter.mesh, transform.localToWorldMatrix );
			});

			//Graphics.SetRenderTarget(colorBuffer_screen, depthBuffer_screen);

			if (!ghostMat.SetPass(PASS_RENDER_BACKFACES)) {
				return;
			}
			
			meshFilters.ForEach( meshFilter => {
				Transform transform = meshFilter.gameObject.transform;
				Graphics.DrawMeshNow( meshFilter.mesh, transform.localToWorldMatrix );
			});

			//RenderTexture.ReleaseTemporary( depthBuffer_ghost );		
		}

		private void RenderFrontside(GhostFilter subscriber, List<MeshFilter> meshFilters, Camera filterCamera) { }

		public void RenderGhosts(List<GhostFilter> subscribers, Camera filterCamera) {
			subscribers.ForEach( subcscriber => {
				List<MeshFilter> meshFilters = subcscriber.GetComponentsInChildren<MeshFilter>().ToList();

				RenderBackside(subcscriber, meshFilters, filterCamera);
				RenderFrontside(subcscriber, meshFilters, filterCamera);			
			});

		}

	}
}

