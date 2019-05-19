// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ProximityShader" {
	Properties {
		_WarningColor ("Warning Color", Color) = (1,1,0,1)
		_DangerColor ("Danger Color", Color) = (1,0,0,1)
		_MaxAlpha("Max Alpha", Range(0,1)) = 0.5

		_Position ("Position", Vector) = (0, 0, 0, 0)
		_MinDistance("Min Distance", Float) = 1
		_MaxDistance("Max Distance", Float) = 5

		_DangerPercentage("Danger Percentage", Range(0,1)) = 0.5
		_DangerFlashSpeed("Danger Flash Speed", Float) = 0.1
	}

	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+1"}
		LOD 200

		ZWrite Off
		Cull Off
		Lighting Off
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
			uniform float4 _WarningColor;
			uniform float4 _DangerColor;
			uniform float  _MaxAlpha;

			uniform float  _MinDistance;
			uniform float  _MaxDistance;

			uniform float  _DangerPercentage;
			uniform float  _DangerFlashSpeed;

			v2f vert(appdata v) {
				v2f o;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				static const float PI = 3.14159;

				float d = distance(i.worldPos, _Position);
				d -= _MinDistance;
				d = clamp(1 - d / (_MaxDistance - _MinDistance), 0, 1);

				float4 col = _WarningColor;
				if (d > 1 - _DangerPercentage) {
					float st = (sin(_Time[1] / (_DangerFlashSpeed / PI)) + 1) / 2;
					col.r = (_DangerColor.r * st * d) + (_WarningColor.r * (1 - (st * d)));
					col.b = (_DangerColor.b * st * d) + (_WarningColor.b * (1 - (st * d)));
					col.g = (_DangerColor.g * st * d) + (_WarningColor.g * (1 - (st * d)));
				}

				col.a = d * _MaxAlpha;
				return col;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
