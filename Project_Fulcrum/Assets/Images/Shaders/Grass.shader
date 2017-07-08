
Shader "Custom/Grass" {
	
	Properties {
		_Color("_Color", Color) = (1,1,1,1)
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
		
				//if(disp > 0)
				//{
				//disp -= 0.05f;
				//}
				//if(disp < 0)
				//{
				//disp += 0.05f;
				//}
				//disp = (((disp*2)-1)*_Radius);
				//disp = (disp*_WindForce);
				//float2 magnitude2 = float2(0,((0.5f*sin(_Time.y))+0.5f));
				//float2 waveOffset = float2(0, 0.2*(sin(2*(i.uv.x+_Time.y))));
				//float4 colour = tex2D(_MainTex, i.uv);

//				if(i.uv.y > 0.5f)
//				{
//						i.uv.y *= 2;
//				}

				//float angle = atan(i.uv.y/i.uv.x);
				i.uv.x += disp*abs(disp)/2;
				i.uv.y += (abs(ydisp)/2);


				//newuv.x += disp*abs(disp);
				//newuv.x += i.uv.y*_WindForce;
				//newuv.y += abs(disp);

				//float alpha1 = i.uv.x-i.uv.y;
				//float alpha = 1;

				//float repeatRad = radius % 0.2f;
			
				float4 colour = tex2D(_MainTex, i.uv);

				//float4 colour2 = float4(colour.r*i.uv.x,colour.g*i.uv.y,0,colour.a);
				//float4 colour3 = float4(1-colour.r, 1-colour.g, 1-colour.b, colour.a);
				//float4 colour4 = float4(colour.r*_Color.r,colour.g*_Color.g,colour.b*_Color.b,alpha*_Color.a);
				colour *= _Color;
				clip(colour.a-1);
				//return colour4;
				return colour;
			}
			ENDCG
		}
	}
}
