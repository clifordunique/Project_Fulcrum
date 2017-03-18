Shader "Custom/Test" {
	
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_DisplaceTex ("Displacement Texture", 2D) = "white" {}
		_Magnitude("Magnitude", Range(0,0.1)) = 1
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
			sampler2D _DisplaceTex;
			float _Magnitude;
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
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				_Magnitude = ((0.02f*sin(_Time.y))+0.02f);
				float2 disp = tex2D(_DisplaceTex, i.uv).xy;
				disp = (((disp*2)-1)*_Magnitude);
				//float2 magnitude2 = float2(0,((0.5f*sin(_Time.y))+0.5f));
				float xVal = i.uv.x;
				float2 waveOffset = float2(0, 0.2*(sin(2*(i.uv.x+_Time.y))));
				//float4 colour = tex2D(_MainTex, i.uv);
				//float4 colour = float4(colour.r*i.uv.x,colour.g*i.uv.y,0,colour.a);
				//float4 colour = float4(1-colour.r, 1-colour.g, 1-colour.b, colour.a);
				float4 colour = tex2D(_MainTex, i.uv);  //i.uv + disp + waveOffset
				//colour *= _Color;
				return colour;
			}
			ENDCG
		}
	}
}
