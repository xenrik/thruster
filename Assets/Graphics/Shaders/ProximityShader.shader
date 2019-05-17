// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ProximityShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Position ("Position", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Render"="Transparent" "IgnoreProjector" = "True"}
		LOD 200

		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			//#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex: POSITION;
			};

			struct v2f {
				float4 vertex: SV_POSITION;
				float3 worldPos: TEXCOORD0;
			};

			uniform float3 _Position;
			uniform float4 _Color;

			v2f vert(appdata v) {
				v2f o;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float4 col = _Color;
				col.r = 1;
				col.g = 0;
				col.b = 0;
				col.a = 0; //1 - distance(i.worldPos, _Position);

				return col;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
