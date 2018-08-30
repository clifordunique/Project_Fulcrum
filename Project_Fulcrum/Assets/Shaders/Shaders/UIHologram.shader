Shader "ProjectFulcrum/UIHologram"
{
	

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BaseLayer ("BaseLayer", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)

		// Required for UIMask
		_StencilComp ("Stencil Comparison", Float) = 8.000000
		_Stencil ("Stencil ID", Float) = 0.000000
		_StencilOp ("Stencil Operation", Float) = 0.000000
		_StencilWriteMask ("Stencil Write Mask", Float) = 255.000000
		_StencilReadMask ("Stencil Read Mask", Float) = 255.000000
		_ColorMask ("Color Mask", Float) = 15.000000
		[Toggle(UNITY_UI_ALPHACLIP)]  _UseUIAlphaClip ("Use Alpha Clip", Float) = 0.000000
	}
	SubShader
	{
		// No culling or depth
		Cull Off
		ZWrite Off
//		ZTest [unity_GUIZTestMode]
		ZTest Off


		Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        // Required for UI.mask
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		ColorMask [_ColorMask]

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//float4 f = _MainTex_TexelSize;

				if(col.a != 0)
				{
					col.a = (0.9+0.1*sin(_MainTex_TexelSize.z*i.uv.y))*(1-(0.2*(sin(fmod(i.uv.y+_Time.y/4,1.57)))));
				}

				return col;
			}
			ENDCG
		}
	}
}
