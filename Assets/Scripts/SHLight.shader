Shader "Custom/SHLight"
 {
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Albedo ("lbedo", Range(0,1)) = 0.5
	}
	SubShader 
	{
		pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _Albedo;

			float4 _MainTex_ST;

			sampler2D _MainTex;


			struct v2f
			{
				float4 pos:POSITION;
				float4 col:COLOR;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos= UnityObjectToClipPos(v.vertex);
				o.col = v.color*_Albedo;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f v):COLOR
			{
				float4 tex = tex2D(_MainTex,v.uv);
				return v.col*tex;
			}


			ENDCG
		}
	}
	FallBack "Diffuse"
}
