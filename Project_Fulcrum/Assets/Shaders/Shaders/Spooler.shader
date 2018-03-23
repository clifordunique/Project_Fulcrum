
Shader "ProjectFulcrum/Spooler" {
	
	Properties {
		_Color("_Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex ("Spiral Noise Texture", 2D) = "white" {}
		_Radius("_Radius", Range(0,1)) = 1
		_Vortex("_Vortex", Range(0,10)) = 1
		_RingWidth("_RingWidth", Range(0,0.1)) = 1
		_Angle("_Angle", Range(0,6.28)) = 1
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
			sampler2D _NoiseTex;
			float _Radius;
			float _RingWidth;
			float _Vortex;
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

				o.uv = v.uv.xy;// - fixed2(0.5,0.5);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 mid = float2(0.5,0.5);

//				// Noisy pull-to-middle transformation
				float disp = 1+(tex2D(_NoiseTex, i.uv).r*_Vortex);
				float2 start = i.uv;
				i.uv = mid+((start-mid)*disp);
				float2 vecFromOrigin = i.uv-mid;

				// Spiral transformation
//				float distFromMiddle = length(mid-i.uv);
//
//				float2 shift = i.uv - mid;
//
//				float spinAngle = (1-distFromMiddle)*360*(_Vortex/500);
//
//				float s = sin(spinAngle);
//				float c = cos(spinAngle);
//
//				float newX = shift.x*c - shift.y*s;
//				float newY = shift.x*s + shift.y*c;
//
//				float2 shiftedBack = float2(newX+0.5, newY+0.5);
//				i.uv = shiftedBack;
				
				float angle = atan(vecFromOrigin.y/vecFromOrigin.x);
			
				float alpha = 1;

				float radius = sqrt(pow(vecFromOrigin.x, 2)+pow(vecFromOrigin.y, 2));


				if((radius > _Radius) && (radius < _Radius+_RingWidth))
				{
					alpha = 1;
				}
				else
				{
					alpha = 0;
				}

				if(vecFromOrigin.x < 0)
				{
					angle += 3.14f;
				}
				else
				{
					if(vecFromOrigin.y < 0)
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
//		Pass
//		{
//			Name "VORTEX"
//			Blend SrcAlpha OneMinusSrcAlpha 
//
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//
//			sampler2D _MainTex;
//			float4 _MainTex_TexelSize;
//			sampler2D _NoiseTex;
//			float _Vortex;
//			float4 _Color;
//
//			#include "UnityCG.cginc"
//			struct appdata
//			{
//				float4 vertex : POSITION;
//				float2 uv : TEXCOORD0;
//				fixed4 color : COLOR;
//			};
//
//			struct v2f
//			{
//				float4 vertex : SV_POSITION;
//				float2 uv : TEXCOORD0;
//				fixed4 color : COLOR;
//			};
//
//			v2f vert(appdata v)
//			{
//				v2f o;
//				o.vertex = UnityObjectToClipPos(v.vertex);
//				//o.uv = v.uv;
//				o.color = v.color;
//				o.uv = v.uv;//.xy - fixed2(0.5,0.5);
//
//				return o;
//			}
//
//			float4 frag(v2f i) : SV_Target
//			{
//				float2 mid = float2(0.5,0.5);
//				// Noisy pull-to-middle transformation
//				float disp = 1+(tex2D(_NoiseTex, i.uv).r*_Vortex);
//				float2 start = i.uv;
//
//
//				//i.uv = ((1-disp)*start)-((disp)*mid);
//				i.uv = mid+((start-mid)*disp);
//
//				// Spiral transformation
//				float distFromMiddle = length(mid-i.uv);
//
//				float2 shift = i.uv - mid;
//
//				float spinAngle = (1-distFromMiddle)*360*(_Vortex/500);
//
//				float s = sin(spinAngle);
//				float c = cos(spinAngle);
//
//				float newX = shift.x*c - shift.y*s;
//				float newY = shift.x*s + shift.y*c;
//
//				float2 shiftedBack = float2(newX+0.5, newY+0.5);
//				i.uv = shiftedBack;
////				i.uv.x += disp*abs(disp)/2;
////				i.uv.y += (abs(ydisp)/2);
//
//				float4 colour = tex2D(_MainTex, i.uv);
//
//				//colour *= i.color;
//				//clip(colour.a-1);
//				//return colour4;
//				return colour;
//			}
//			ENDCG
//		}
	}
}
