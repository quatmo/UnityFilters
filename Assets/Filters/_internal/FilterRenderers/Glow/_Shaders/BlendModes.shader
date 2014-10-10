Shader "Hidden/Filters/BlendModes" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ColorBuffer ("Color", 2D) = "" {}
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	sampler2D _ColorBuffer;
	half4 EMPTY = half4(0,0,0,0);
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  v.texcoord.xy;
		
		return o;
	}
	
	half4 fragScreen (v2f i) : COLOR {
		half4 screencolor = tex2D(_MainTex, i.uv.xy);
		half4 addedbloom = tex2D(_ColorBuffer, i.uv.xy);// * 5;
		half intensity = addedbloom.a;
		//half4 screencolor = tex2D(_ColorBuffer, i.uv[1]);
		//return 1-(1-addedbloom)*(1-screencolor);
		return addedbloom * intensity + screencolor;
	}
	
	half4 fragScreenCutout (v2f i) : COLOR {
		half4 solids = tex2D(_MainTex, i.uv.xy);
		half4 outline = tex2D(_ColorBuffer, i.uv.xy);;
		
		// If the solid background has the same color as the outline
		if(length(solids.rgb-outline.rgb) == 0) { 
			// We should cut out this area of the outline.
			return EMPTY;
		} else {
			// If not, we return the outline color
			return outline;
		}
	}
	
	half4 fragScreenOverlay(v2f i) : COLOR {
		half4 screen = tex2D(_MainTex, i.uv.xy);
		half4 outline = tex2D(_ColorBuffer, i.uv.xy);;
		
		// If there is an outline at this position
		if(length(outline) != 0) { 
			// we return it
			return outline;
		} else {
			// Otherwise just show the scene contents.
			return screen;
		}
	}
	
	ENDCG 
	
	SubShader {
	 	ZTest Always Cull Back ZWrite Off
  		Fog { Mode off }  
  		ColorMask RGBA
		
		// 0: Alpha blended
		Pass {
		 	CGPROGRAM
	      	#pragma fragmentoption ARB_precision_hint_fastest
	      	#pragma vertex vert
	      	#pragma fragment fragScreen
	      	ENDCG
		}
		
		// 1: Cutout
		Pass {
		 	CGPROGRAM
	      	#pragma fragmentoption ARB_precision_hint_fastest
	      	#pragma vertex vert
	      	#pragma fragment fragScreenCutout
	      	ENDCG
		}
		
		// 2: Overlay
		Pass {
		 	CGPROGRAM
	      	#pragma fragmentoption ARB_precision_hint_fastest
	      	#pragma vertex vert
	      	#pragma fragment fragScreenOverlay
	      	ENDCG
		}
	}
	
}
