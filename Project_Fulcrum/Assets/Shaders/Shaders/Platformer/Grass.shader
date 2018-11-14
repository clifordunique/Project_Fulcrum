﻿
Shader "ProjectFulcrum/Grass" {
	
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_DisplaceTex ("Displacement Texture", 2D) = "white" {}
		_WindForce("_WindForce", Range(-2,2)) = 0
	}
	SubShader 
	{

		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
		}
		ZWrite Off
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			sampler2D _DisplaceTex;
			float _WindForce;
			float4 _Color;

			#include "UnityCG.cginc"
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
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv = v.uv;

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float disp = i.uv.y*_WindForce;
				if (abs(_WindForce) < 0.5f)
				{
					_WindForce = 0.5f;
				}
				float ydisp = i.uv.y*_WindForce;
		
			
				i.uv.x += disp*abs(disp)/2;
				i.uv.y += (abs(ydisp)/2);

				float4 colour = tex2D(_MainTex, i.uv);

				colour *= i.color;
				clip(colour.a-1);
				return colour;
			}
			ENDCG
		}
	}
}