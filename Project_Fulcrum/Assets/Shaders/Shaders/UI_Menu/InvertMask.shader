Shader "ProjectFulcrum/InvertMask" {

	Properties
	{
		_MainTex("Sprite Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		ZWrite Off

		GrabPass
		{
			"_BG"
		}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				// vertex input: position, UV
				struct appdata 
				{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 grabPos : TEXCOORD0;
					float4 pos : SV_POSITION;
					float2 alphaUV : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _BG;


				v2f vert(appdata v) {
					v2f o;
					// use UnityObjectToClipPos from UnityCG.cginc to calculate 
					// the clip-space of the vertex
					o.pos = UnityObjectToClipPos(v.vertex);
					// use ComputeGrabScreenPos function from UnityCG.cginc
					// to get the correct texture coordinate
					o.grabPos = ComputeGrabScreenPos(o.pos);

					o.alphaUV = TRANSFORM_TEX(v.texcoord, _MainTex);

					return o;
				}

				half4 frag(v2f i) : SV_Target
				{
					half4 bgcolor = tex2Dproj(_BG, i.grabPos);

					bgcolor = 1 - bgcolor;
					

					bgcolor.a = tex2D(_MainTex, i.alphaUV).a;
					clip(bgcolor.a - 1);
					return bgcolor;
				}
				ENDCG
		}

	}
}

//		Pass
//		{
//			Name "InvertMask"
//			Blend OneMinusDstColor OneMinusSrcAlpha
//			BlendOp Add
//			//Cull Off
//			Lighting Off
//			ZWrite On
//
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//
//			sampler2D _BG;
//			sampler2D _MainTex;
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
//			struct grabv2f
//			{
//				float4 pos : SV_POSITION;
//				float2 grabPos : TEXCOORD0;
//			};
//
//			v2f vert(appdata v)
//			{
//				v2f o;
//				o.vertex = UnityObjectToClipPos(v.vertex);
//				o.color = v.color;
//				o.uv = v.uv;
//
//				return o;
//			}
//
//			grabv2f vert(appdata_base v) {
//				grabv2f o;
//				// use UnityObjectToClipPos from UnityCG.cginc to calculate 
//				// the clip-space of the vertex
//				o.pos = UnityObjectToClipPos(v.vertex);
//				// use ComputeGrabScreenPos function from UnityCG.cginc
//				// to get the correct texture coordinate
//				o.grabPos = ComputeGrabScreenPos(o.pos);
//				return o;
//			}
//
//
//			float4 frag(v2f i) : SV_Target
//			{
//				float4 maskColour = tex2D(_MainTex, i.uv);
//				clip(maskColour.a-1);
//				float4 colour = tex2Dproj(_BG, i.uv);
//				colour = 1 - colour;
//				return colour;
//			}
//			ENDCG
//		}
//	}
//}
