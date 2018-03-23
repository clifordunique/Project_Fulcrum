
Shader "ProjectFulcrum/ChromaticFlux" {
	
	Properties 
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_DisplaceTex ("Displacement Texture", 2D) = "white" {}
		_Magnitude("Magnitude", Range(0,100)) = 1
		//_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader 
	{

		Tags
		{
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		ZWrite Off
		Pass
		{
			Name "ChromaticFlux"
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _DisplaceTex;
			float4 _DisplaceTex_TexelSize;
			float _Magnitude;
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
				float timeFactor = _Time.y;
				float verticalWave = 0.5+0.5*sin((i.uv.y+timeFactor)/2);
				float verticalWave2 = 1+0.5*cos((i.uv.y-timeFactor)*15);
				fixed2 disOffset = i.uv;
				disOffset /= 4;
				disOffset += 0.5+(verticalWave-timeFactor)/2;//*verticalWave2;
				float displacement = tex2D(_DisplaceTex, disOffset).r*_Magnitude*verticalWave2;//*(verticalWave2+_Magnitude);//+verticalWave;

		

//				float midWave = 0.5+0.5*sin((i.uv.y+timeFactor));
////				fixed2 midOffset = i.uv;
////				midOffset.y /= 2;
////				midOffset.y += sin((i.uv.y+timeFactor));
////				float midDisp = tex2D(_DisplaceTex, disOffset).r*_Magnitude*(verticalWave-0.5);
////				midDisp.x -= 0.5;	
//				i.uv.y += midWave;


				float4 pixelLeft = tex2D(_MainTex, i.uv-fixed2(_MainTex_TexelSize.x*displacement, 0));
				pixelLeft *= i.color;
				//pixelLeft.r = 0;
				pixelLeft.g = 0.5f;
				pixelLeft.b = 0.5f;
				//pixelLeft.a /= 3;

				if(pixelLeft.a <= 0)
				{
					pixelLeft.r = 0.3;
					pixelLeft.g = 0.3;
					pixelLeft.b = 0.3;
				}

				float4 pixelRight = tex2D(_MainTex, i.uv+fixed2(_MainTex_TexelSize.x*displacement, 0));
				pixelRight *= i.color;
				pixelRight.r = 0.5f;
				pixelRight.g = 0.5f;
				//pixelRight.b = 0;
				//pixelRight.a /= 3;

				if(pixelRight.a <= 0)
				{
					pixelRight.r = 0.3;
					pixelRight.g = 0.3;
					pixelRight.b = 0.3;
				}

				float4 middle = tex2D(_MainTex, i.uv); //+fixed2(_MainTex_TexelSize.x*midDisp, 0)
				middle *= i.color;
				middle.r = 0.5f;
				//middle.g = 0;
				middle.b = 0.5f;
				//middle.a /= 3;

				if(middle.a <= 0)
				{
					middle.r = 0.3;
					middle.g = 0.3;
					middle.b = 0.3;
				}

				//return pixelRight + pixelLeft + middle;

//				float2 mid = float2(0.5,0.5);
//				// Noisy pull-to-middle transformation
//				float disp = 1+(tex2D(_DisplaceTex, i.uv).r*_Magnitude);
//				float2 start = i.uv;
//
//				i.uv = mid+((start-mid)*disp);
//
//				// Spiral transformation
//				float distFromMiddle = length(mid-i.uv);
//
//				float2 shift = i.uv - mid;
//
//				float angle = (1-distFromMiddle)*360*(_Magnitude/500);
//
//				float s = sin(angle);
//				float c = cos(angle);
//
//				float newX = shift.x*c - shift.y*s;
//				float newY = shift.x*s + shift.y*c;
//
//				float2 shiftedBack = float2(newX+0.5, newY+0.5);
//				i.uv = shiftedBack;
//
				
			

				float4 finalColour = float4(pixelLeft.r, middle.g, pixelRight.b, pixelLeft.a+middle.a+pixelRight.a);

				return finalColour;
			}
			ENDCG
		}
	}
}
