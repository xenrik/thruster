Shader "Custom/CrossSection/New" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_CrossColor("Cross Section Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_PlaneNormal("PlaneNormal",Vector) = (0,1,0,0)
		_PlanePosition("PlanePosition",Vector) = (0,0,0,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			Cull Front
			ZTest Off
			ZWrite Off
			ColorMask 0

			Stencil {
				Ref 1
				Comp Always
				Pass Replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}	

			fixed4 frag(v2f i) : SV_Target {
				return 0;
			}

			ENDCG
		}

		Pass {
			Cull Back
			ZTest Off
			ZWrite Off
			ColorMask 0

			Stencil {
				Ref 1
				Comp Always
				Pass Zero
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}	

			fixed4 frag(v2f i) : SV_Target {
				return 0;
			}

			ENDCG
		}

		Pass {
			Cull Back
			ZTest On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 world : TEXCOORD1;
                float2 uv : TEXCOORD0;
			};

            sampler2D _MainTex; 
            float4 _MainTex_ST;
			float4 _PlanePosition;
			float4 _PlaneNormal;
			
			bool checkVisability(fixed3 worldPos) {
				float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
				return dotProd1 > 0;
			}

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.world = mul(unity_ObjectToWorld, v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}	

			fixed4 frag(v2f i) : SV_Target {			
				if (checkVisability(i.world)) {
					discard;
				}

                fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}

			ENDCG
		}

		Pass {
			Cull Off
			ZTest Off

			Stencil {
				Ref 1
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
			};

            sampler2D _MainTex; 
            float4 _MainTex_ST;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}	

			fixed4 frag(v2f i) : SV_Target {
                fixed4 c;
				c.r = 1;
				c.g = 0;
				c.b = 1;
				c.a = 1;
				return c;
			}

			ENDCG
		}
	}

	//FallBack "Diffuse"
}
