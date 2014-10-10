
Shader "Hidden/Filters/GlowEncoder" {
	Properties {
		_MainColor("Color", Color) = (1,1,1,1) 	
	}
	SubShader {
		
		
		Pass {
			CGPROGRAM
			fixed4 _MainColor;	
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct vertexInput {
				float4 vertex : POSITION;
			};

			struct fragmentInput{
				float4 position : SV_POSITION;
			};
			
			fragmentInput vert(vertexInput i) {
				fragmentInput o;
				o.position = mul (UNITY_MATRIX_MVP, i.vertex);
				return o;
			}
			
			fixed4 frag(fragmentInput i) : COLOR {
				return _MainColor;
			}
			ENDCG
		}
	}
}

