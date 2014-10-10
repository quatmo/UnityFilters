Shader "Hidden/Filters/OutlineEncoder" {
	Properties {
		_ColorBuffer("_ColorBuffer", 2D) = "" {}
		_MainColor("Color", Color) = (1,1,1,1) 	
		_Width("Width", float) = 0.1
	}
	
	CGINCLUDE
	

	#include "UnityCG.cginc"

	fixed4 _MainColor;	
	float _Width;
	sampler2D _ColorBuffer;
	
	uniform half4 _TexelSize;
	
	static const fixed kernelLength = 8;
	static const fixed2[8] kernel  = {
		fixed2(0,1),
		fixed2(0.707,0.707), // 0.707 -> the points on the kernel should lie approximately on the unity sphere.
		fixed2(1,0),
		fixed2(0.707,-0.707),
		fixed2(0,-1),
		fixed2(-0.707,-0.707),
		fixed2(-1,0),
		fixed2(-0.707,0.707)
	};
	static const fixed4 EMPTY = fixed4(0,0,0,0);
			
	// In / out structs		
			
	struct vertexInput {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct vertexInput_img {
		float4 vertex : POSITION;
		float4 uv : TEXCOORD0;
	};

	struct fragmentInput{
		float4 position : SV_POSITION;
	};
	
	struct fragmentInput_img {
		float4 position : SV_POSITION;
		float4 uv : TEXCOORD0;
	};
	
	// Vertex operations
	
	fragmentInput vert(vertexInput i) {
		fragmentInput o;
		float width = 10;
		o.position = mul (UNITY_MATRIX_MVP, i.vertex);
		return o;
	}
	
	fragmentInput_img vert_img2(vertexInput_img i) {
		fragmentInput_img o;
		o.position = mul (UNITY_MATRIX_MVP, i.vertex);
		o.uv = i.uv;
		return o;
	}
	
	fragmentInput vert_extrudeNormals(vertexInput i) {
		fragmentInput o;
		float width = 10;
		o.position = mul (UNITY_MATRIX_MVP, i.vertex + float4(i.normal * _Width, 0) );
		return o;
	}
	
	// Fragment operations
	
	fixed4 frag(fragmentInput i) : COLOR {
		return _MainColor;
	}
	
	fixed4 frag_constant(fragmentInput i) : COLOR {
		return fixed4(1,1,1,0.5);
	}
	
	fixed4 frag_screenspaceAnalysis(fragmentInput_img i) : COLOR {
		fixed4 outlineEncoding = tex2D(_ColorBuffer, i.uv.xy);
		fixed width = 4;
		fixed maxAlpha = outlineEncoding.a;
		fixed4 output = EMPTY;
		
		for(fixed index = 0; index < kernelLength; index++) {
			fixed4 outlineEncoding = tex2D(_ColorBuffer, i.uv.xy + kernel[index] * _TexelSize.xy * width);
			// Depth is encoded in the alpha channel
			if(length(outlineEncoding) != 0 && outlineEncoding.a > maxAlpha) {
				maxAlpha = outlineEncoding.a;
				output = outlineEncoding;
			}	
		}
		return output;
	}
	
	ENDCG
	
	
	SubShader {
		
		// 0: Prime color buffer
		Pass {
			Zwrite off
			ZTest always
			Cull back
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
		
		// 1: Extrude normals
		Pass {
			Zwrite off
			ZTest always
			Cull off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_extrudeNormals
			#pragma fragment frag
			ENDCG
		}
		
		// 2: Screen pass outlines
		Pass {
			Zwrite off
			ZTest always
			Cull off
			Fog { Mode off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert_img2
			#pragma fragment frag_screenspaceAnalysis
			ENDCG
		}
	}
}

