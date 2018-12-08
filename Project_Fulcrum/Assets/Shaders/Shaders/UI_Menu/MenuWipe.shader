Shader "ProjectFulcrum/MenuWipe" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_SliceGuide("SliceGuide", 2D) = "white" {}
		_SliceDirection("_SliceDirection", Range(-1.0, 1.0)) = 1.0
		_Brightness("_Brightness", Range(0, 1)) = 1
		_SliceAmount("_SliceAmount", Range(0.0, 1.0)) = 0.5
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
			Name "MenuWipe"
			Blend SrcAlpha OneMinusSrcAlpha 


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _DisplaceTex;
			float _Magnitude;
			float4 _Color;
			sampler2D _SliceGuide;
			float _SliceAmount;
			float _SliceDirection;
			float _Brightness;
			float2 uv_SliceGuide;

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
				float4 colour = tex2D(_MainTex, i.uv);
				colour.rgb = colour.rgb*_Brightness;

				float2 sliceUV = i.uv;
				sliceUV.x = sliceUV.x*_SliceDirection;
				float offset = 0.5 - 0.5*_SliceDirection;
				sliceUV.x = offset+sliceUV.x;
				float3 sliceColour = tex2D(_SliceGuide, sliceUV.xy).rgb;
				float cutoff = 0.666f*sliceColour.r + 0.333f*sliceColour.g;
				clip(cutoff - _SliceAmount);


				//colour *= i.color;
				//clip(colour.a-1);
				//return colour4;
				return colour;
			}
			ENDCG
		}
	}
}
