﻿
Shader "ProjectFulcrum/StandardUIText"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
				"IgnoreProjector" = "True"
			}

			Blend SrcAlpha OneMinusSrcAlpha, OneMinusDstAlpha One
			Cull Off
			Lighting Off
			ZWrite Off

			Pass
			{
				Name "StandardUIText"

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				sampler2D _MainTex;
				float4 _Color;
				float4 _ClipRect;

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR;
					float4 worldPosition : TEXCOORD1; //we need to pass world pos to the fragment shader for clipping
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.uv = v.uv;

					o.worldPosition = v.vertex;
					o.vertex = UnityObjectToClipPos(o.worldPosition);

					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					float alpha = tex2D(_MainTex, i.uv).a;
					float4 colour = i.color;
					colour.a *= alpha;
					//colour.rgb = _Color.rgb;
					colour.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
					return colour;
				}
				ENDCG
			}
		}
}
