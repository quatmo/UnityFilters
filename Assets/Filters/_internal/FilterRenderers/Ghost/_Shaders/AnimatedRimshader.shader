Shader "Custom/Vistra/AnimatedRimshader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_RimColor ("Rim color", color) = (1,1,1,1)
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	sampler2D _DepthBuffer;
	fixed4 _RimColor;
	
	static const float MAX_DEPTH = 10000;
	static const float EPSILON = 0.0000000001;
	
	// Input structs
	
	struct vertexInput {
		float4 vertex : POSITION;
	};
	
	struct vertexInput_normals {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
	
	struct fragmentInput_screencoordinates {
		float4 position : SV_POSITION;
		float4 position_screen;
	};
	
	struct fragmentInput_rim {
		float4 position : SV_POSITION;
		float4 position_screen;
		float2 texcoord;
		float2 texcoord1;
		float3 normal;
		float3 viewDir;
	};
	
	// Vertex functions
	
	fragmentInput_screencoordinates vert_screenspace(vertexInput i) {
		fragmentInput_screencoordinates o;
		o.position = mul (UNITY_MATRIX_MVP, i.vertex);
		o.position_screen = ComputeScreenPos(o.position);
		return o;
	}
	
	fragmentInput_rim vert_rim(vertexInput_normals i)  {
		fragmentInput_rim o;
		o.position = mul (UNITY_MATRIX_MVP, i.vertex);
		o.normal = i.normal;
		// Not sure if normalization is needed here ...
		// Normal is in object space, we need the view direction in the same space.
		o.viewDir = ObjSpaceViewDir( i.vertex);
		o.texcoord = float2( i.vertex.x + i.vertex.z, i.vertex.y  + i.vertex.z) * 0.001  + float2(0.06, 0.07) * _SinTime;
		o.texcoord1 = float2( i.vertex.x - i.vertex.z, i.vertex.y  + i.vertex.x) * 0.01  + float2(0.03, -0.07) * _CosTime;
		o.position_screen = ComputeScreenPos(o.position);
		return o;
	}
	
	// Fragments functions
	
	fixed4 frag_depthWrite(fragmentInput_screencoordinates i) : COLOR {
		
		float2 position_viewport = i.position_screen.xy/i.position_screen.w;
		float depth_current = DecodeFloatRGBA( tex2D(_DepthBuffer, position_viewport) );
		float depth_this = -1 * (i.position_screen.z / i.position_screen.w ) / MAX_DEPTH;
		
		if(depth_this < 0) {
			return fixed4(1,0,0,1);
		}
		
		if(depth_this >= depth_current) {
			return EncodeFloatRGBA( depth_this );
		} else {
			return EncodeFloatRGBA( depth_current );
		}
		
//		if(depth_this > 0.98) {
//			return fixed4(1,0,0,1);
//		}
		
		//return EncodeFloatRGBA( depth_this );
		
		//return fixed4(0,0,1,1);
	}
	
	fixed4 frag_rim(fragmentInput_rim i) : COLOR {
		
//		float2 position_viewport = i.position_screen.xy/i.position_screen.w;
//		float depth_current = DecodeFloatRGBA( tex2D(_DepthBuffer, position_viewport) );
//		float depth_this = -1 * (i.position_screen.z / i.position_screen.w ) / MAX_DEPTH;
//		//eturn DecodeFloatRGBA(depth_this);
//		
//		//return tex2D(_DepthBuffer, position_viewport);
//		
//		if(depth_this - depth_current > 0.000000001) {
//			//return fixed4(0,1,0,1);
//			discard;
//		} 
//		return fixed4(1,0,0,1);
//		return tex2D(_DepthBuffer, position_viewport);
		
		
		fixed4 tex = tex2D(_MainTex, i.texcoord);
		tex *= tex2D(_MainTex, i.texcoord1);
	
		fixed _SpecPower = 1.2;
		fixed _RimPower = 1.82;
		float _MinAlpha = 0.2;
	
		
		half rim = 1.0 - saturate(dot (normalize(i.viewDir), normalize(1*i.normal) ));
		
		rim = pow(rim, _RimPower);
		
	//half spec = pow(1.0-rim,_SpecPower);

		fixed3 _SpecColor2 = fixed3(0,1,0);

//			//o.Alpha = max( min(_MinAlpha, pow(rim, _RimPower)), min(_MinAlpha,spec));
	
		return fixed4( _RimColor.rgb, rim*7 * tex.r  );
		
	}
	
	ENDCG
	
	
	
	SubShader {
		
//		// 0: Prime z buffer backfaces
//		Fog { Mode off } 
//		Cull front
//		Zwrite off
//		Ztest always	
//		Pass {
//			CGPROGRAM			
//			#include "UnityCG.cginc"
//			#pragma target 3.0
//			#pragma vertex vert_screenspace
//			#pragma fragment frag_depthWrite
//			
//			
//			ENDCG
//			//color(0,1,0,1)
//			//ColorMask 0
//		}
		
		// 0: Prime z buffer backfaces
//		Blend One One 
//		Fog { Mode off } 
//		Cull front
//		//ZTEst GEqual
//		ZTEst LEqual
//		Zwrite on
		Blend One One
		Pass {
			ColorMask 0
		}
		
		// 1: Prime z buffer frontfaces
//		Blend One One 
//		Fog { Mode off } 
//		Cull back
//		Zwrite off
		Pass {
			ColorMask 0
		}
		
		// 2: Render backfaces
		Pass{
		
			Blend SrcAlpha OneMinusSrcAlpha
			Fog { Mode off }
			ZWrite off
			Cull front
			//Ztest Equal
			Ztest Always
			//ColorMask 0
			
			CGPROGRAM			
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert_rim
			#pragma fragment frag_rim
			
			
			
			ENDCG
		}
		
		
		// 3: Render frontfaces
		Pass{
		
			Blend SrcAlpha OneMinusSrcAlpha
			Fog { Mode off }
			ZWrite off
			Cull back
			Ztest Equal
			
			CGPROGRAM			
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert_rim
			#pragma fragment frag_rim
			
			
			
			ENDCG
		}
			
		// 4: Debug render as solids
		//Ztest LEqual
		//Ztest always
		Blend One zero
		Fog { Mode off } 
		Cull back
		Pass {
			Color(1,0,0,1)
			ColorMask RGBA
		}
		
		
	} 
}
