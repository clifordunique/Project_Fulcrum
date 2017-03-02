Shader "Custom/Test" {
	
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white"
//		_Glossiness ("Smoothness", Range(0,1)) = 0.5
//		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{

		Tags
		{
			"Queue" = "Transparent"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			//Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _Color;

			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv*2;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 colour = tex2D(_MainTex, i.uv);
				float4 colour2 = float4(colour.r*i.uv.x,colour.g*i.uv.y,colour.b,colour.a);
				//colour *= _Color;
				return colour2;
			}
			ENDCG
		}
	}
}
