
Shader "Custom/Grass" {
	
	Properties {
		//_Color("_Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_DisplaceTex ("Displacement Texture", 2D) = "white" {}
		_WindForce("_WindForce", Range(-2,2)) = 0
		//_RingWidth("_RingWidth", Range(0,0.1)) = 1
		//_Angle("_Angle", Range(0,6.28)) = 1
//		_Glossiness ("Smoothness", Range(0,1)) = 0.5
//		_Metallic ("Metallic", Range(0,1)) = 0.0
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
			//Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			sampler2D _DisplaceTex;
			float _WindForce;
			//float _RingWidth;
			//float _Angle;
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
				//o.uv = v.uv;
				o.color = v.color;
				o.uv = v.uv;//.xy - fixed2(0.5,0.5);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				//_Radius = 0.5*((0.15f*sin(_Time.z*2))+0.15f);
				//float disp = tex2D(_DisplaceTex, i.uv).r;
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
				//return colour4;
				return colour;
			}
			ENDCG
		}
	}
}
