Shader "ProjectFulcrum/MenuWipe" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_SliceGuide("Slice Guide (RGB)", 2D) = "white" {}
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


				clip(tex2D(_SliceGuide, i.uv).rgb - _SliceAmount);


				//colour *= i.color;
				//clip(colour.a-1);
				//return colour4;
				return colour;
			}
			ENDCG
		}
	}
}







	//	Properties{
	//	  _MainTex("Texture (RGB)", 2D) = "white" {}
	//	  _SliceGuide("Slice Guide (RGB)", 2D) = "white" {}
	//	  _SliceAmount("Slice Amount", Range(0.0, 1.0)) = 0.5
	//	}
	//		SubShader{
	//			Tags
	//			{
	//				"Queue" = "Transparent"
	//				"RenderType" = "Transparent"
	//				"IgnoreProjector" = "True"
	//			}
	//			ZWrite Off
	//			CGPROGRAM
	//			#pragma vertex vert
	//			#pragma fragment frag
	//			struct Input {
	//				float2 uv_MainTex;
	//				float2 uv_SliceGuide;
	//				float _SliceAmount;
	//			};
	//			sampler2D _MainTex;
	//			sampler2D _SliceGuide;
	//			float _SliceAmount;
	//			void surf(Input IN, inout SurfaceOutput o) {
	//				clip(tex2D(_SliceGuide, IN.uv_SliceGuide).rgb - _SliceAmount);
	//				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
	//			}
	//			ENDCG
	//	  }
	//		  Fallback "Diffuse"
	//}