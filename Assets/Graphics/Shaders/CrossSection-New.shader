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

			Stencil {
				Ref 1
				CompFront Always
				PassFront Replace
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
				fixed4 col;
				col.r = 1;
				col.g = 0;
				col.b = 1;
				col.a = 1;

				return col;
			}

			ENDCG
		}
	}

	//FallBack "Diffuse"
}
