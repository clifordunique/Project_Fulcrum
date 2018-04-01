Shader "ProjectFulcrum/MinimapHologram"
{
	

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BaseLayer ("BaseLayer", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always


		Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

		Pass
		{
			Blend SrcAlpha Zero
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

				col.r = 0.5;
				col.g = 0;
				col.b = 0;

				if(col.a != 0)
				{
					fixed4 pixelAbove = tex2D(_MainTex, i.uv+fixed2(0, _MainTex_TexelSize.y));
//					if(i.uv.y+_MainTex_TexelSize.y > 1)
//					{
//						pixelAbove = fixed4(0,0,0,0);
//					}

					fixed4 pixelBelow = tex2D(_MainTex, i.uv-fixed2(0, _MainTex_TexelSize.y));
//					if(i.uv.y-_MainTex_TexelSize.y < 0)
//					{
//						pixelBelow = fixed4(0,0,0,0);
//					}

					fixed4 pixelRight = tex2D(_MainTex, i.uv+fixed2(_MainTex_TexelSize.x, 0));
//					if(i.uv.x+_MainTex_TexelSize.x > 1)
//					{
//						pixelRight = fixed4(0,0,0,0);
//					}

					fixed4 pixelLeft = tex2D(_MainTex, i.uv-fixed2(_MainTex_TexelSize.x, 0));
//					if(i.uv.x-_MainTex_TexelSize.x < 0)
//					{
//						pixelLeft = fixed4(0,0,0,0);
//					}
					col = fixed4(0.5,0,0,1);
					if(pixelAbove.a * pixelBelow.a * pixelRight.a * pixelLeft.a != 0)
					{
						col = fixed4(0.15,0,0,1);
						//col.a = (0.75+0.05*sin(250*i.uv.y))*(1-(0.2*(sin(fmod(i.uv.y+_Time.y/2,1.57)))));
					}
				}

				//col.gb = 0;
				//col = fixed4(1.0,0,0,);
				return col;
			}
			ENDCG
		}

		Pass
		{
			Blend Off
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
			sampler2D _BaseLayer;
			float4 _MainTex_TexelSize;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 oldcol = tex2D(_BaseLayer, i.uv);
				//float4 f = _MainTex_TexelSize;

				if(col.a != 0)
				{
					col = fixed4(0.5,0,0,1);
					//col.a = (0.95+0.05*sin(250*i.uv.y))*(1-(0.2*(sin(fmod(i.uv.y+_Time.y/2,1.57)))));
				}
				else
				{
					col = oldcol;
				}

				//col.gb = 0;
				//col = fixed4(1.0,0,0,);
				return col;
			}
			ENDCG
		}
	}
}
