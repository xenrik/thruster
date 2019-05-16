Shader "Custom/Intersection/Surface" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _FadeLength("Fade Length", Range(0, 2)) = 1
    }
    SubShader {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        sampler2D _CameraDepthTexture;
        fixed4 _Color;
        fixed4 _GlowColor;
        float _FadeLength;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)));
            float surfZ = -mul(UNITY_MATRIX_V, float4(IN.worldPos.xyz, 1)).z;
            float diff = sceneZ - surfZ;
            float intersect = 1 - saturate(diff / _FadeLength);

            fixed4 col = fixed4(lerp(tex2D(_MainTex, IN.uv_MainTex) * _Color, _GlowColor, pow(intersect, 4)));
            o.Albedo = col.rgb;
            o.Alpha = col.a;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}