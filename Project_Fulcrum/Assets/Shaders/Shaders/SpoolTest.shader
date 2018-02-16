// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpoolTest" {
	
	Properties {
		_Color("_Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_DisplaceTex ("Displacement Texture", 2D) = "white" {}
		_Radius("_Radius", Range(0,1)) = 1
		_RingWidth("_RingWidth", Range(0,0.1)) = 1
		_Angle("_Angle", Range(0,6.28)) = 1
//		_Glossiness ("Smoothness", Range(0,1)) = 0.5
//		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{

		Tags
		{
			"Queue" = "Transparent"
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
			float _Radius;
			float _RingWidth;
			float _Angle;
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
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = v.uv;

				o.uv = v.uv.xy - fixed2(0.5,0.5);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				//_Radius = 0.5*((0.15f*sin(_Time.z*2))+0.15f);
				//float2 disp = tex2D(_DisplaceTex, i.uv).xy;
				//disp = (((disp*2)-1)*_Radius);
				//float2 magnitude2 = float2(0,((0.5f*sin(_Time.y))+0.5f));
				//float2 waveOffset = float2(0, 0.2*(sin(2*(i.uv.x+_Time.y))));
				//float4 colour = tex2D(_MainTex, i.uv);

				float angle = atan(i.uv.y/i.uv.x);

				//float alpha1 = i.uv.x-i.uv.y;
				float alpha = 1;

				float radius = sqrt(pow(i.uv.x, 2)+pow(i.uv.y, 2));

				//float repeatRad = radius % 0.2f;

				if((radius > _Radius) && (radius < _Radius+_RingWidth))
				{
					alpha = 1;
				}
				else
				{
					alpha = 0;
				}

				if(i.uv.x < 0)
				{
					angle += 3.14f;
				}
				else
				{
					if(i.uv.y < 0)
					{
						angle += 6.28f;
					}
				}

				if(angle >= _Angle && alpha == 1)
				{
					alpha = 0;
				}
			
				float4 colour = tex2D(_MainTex, i.uv);
				//float4 colour2 = float4(colour.r*i.uv.x,colour.g*i.uv.y,0,colour.a);
				//float4 colour3 = float4(1-colour.r, 1-colour.g, 1-colour.b, colour.a);
				float4 colour4 = float4(colour.r*_Color.r,colour.g*_Color.g,colour.b*_Color.b,alpha*_Color.a);
				//colour *= _Color;
				return colour4;
			}
			ENDCG
		}
	}
}
