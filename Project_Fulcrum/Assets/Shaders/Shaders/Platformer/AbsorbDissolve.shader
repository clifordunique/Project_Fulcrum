
Shader "ProjectFulcrum/AbsorbDissolve" {
	
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_DisplaceTex ("Displacement Texture", 2D) = "white" {}
		_Magnitude("Magnitude", Range(0,10)) = 1
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
			Name "VORTEX"
			Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _DisplaceTex;
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
				float2 mid = float2(0.5,0.5);
				// Noisy pull-to-middle transformation
				float disp = 1+(tex2D(_DisplaceTex, i.uv).r*_Magnitude);
				float2 start = i.uv;


				//i.uv = ((1-disp)*start)-((disp)*mid);
				i.uv = mid+((start-mid)*disp);

				// Spiral transformation
				float distFromMiddle = length(mid-i.uv);

				float2 shift = i.uv - mid;

				float angle = (1-distFromMiddle)*360*(_Magnitude/500);

				float s = sin(angle);
				float c = cos(angle);

				float newX = shift.x*c - shift.y*s;
				float newY = shift.x*s + shift.y*c;

				float2 shiftedBack = float2(newX+0.5, newY+0.5);
				i.uv = shiftedBack;
//				i.uv.x += disp*abs(disp)/2;
//				i.uv.y += (abs(ydisp)/2);

				float4 colour = tex2D(_MainTex, i.uv);

				//colour *= i.color;
				//clip(colour.a-1);
				//return colour4;
				return colour;
			}
			ENDCG
		}
	}
}
