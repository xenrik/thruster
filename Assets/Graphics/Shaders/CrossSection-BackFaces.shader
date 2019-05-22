Shader "Custom/CrossSection/BackFaces" {
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
		Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
		//LOD 200
		Stencil
		{
			Ref 3
			ReadMask 2
			WriteMask 1

			CompFront NotEqual
			PassFront Replace
		}

		ZTest Off
		Cull Front
		CGPROGRAM
#pragma surface surf NoLighting  noambient

		struct Input {
			half2 uv_MainTex;
			float3 worldPos;

		};

		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _CrossColor;
		fixed3 _PlaneNormal;
		fixed3 _PlanePosition;
		bool checkVisability(fixed3 worldPos)
		{
			float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
			return dotProd1 >0 ;
		}
		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			if (checkVisability(IN.worldPos))discard;
			o.Albedo = _CrossColor;
			o.Alpha = _CrossColor.a;
		}
			ENDCG
		
	}
	//FallBack "Diffuse"
}
