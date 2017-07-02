Shader "OrderTracker/Additive"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
	
		//---- add ----
		[HideInInspector]
		_Alpha ("Alpha Factor", Range(0.0,1.0)) = 1.0
		[HideInInspector]
		_ClipRect_0 ("Clip World Rect 0", Float) = (-100000,100000,-100000,100000)
		[HideInInspector]
		_ClipRect_1 ("Clip World Rect 1", Float) = (-100000,100000,-100000,100000)
		[HideInInspector]
		_ClipRect_2 ("Clip World Rect 2", Float) = (-100000,100000,-100000,100000)
		[HideInInspector]
		_ClipRect_3 ("Clip World Rect 3", Float) = (-100000,100000,-100000,100000)
		//---- add ----
	}
	
	Category
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		
		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				
				#include "UnityCG.cginc"
				#include "RenderInCanvas.cginc"
	
				sampler2D _MainTex;
				fixed4 _TintColor;
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
					return ApplyAlpha(ClipWorldRect(col, i.wpos));
				}
				ENDCG 
			}
		}	
	}
}
