Shader "Custom/SHLight1"
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

			float4x4 rotRLight;
			float4x4 rotGLight;
			float4x4 rotBLight;

			int width;
			int height;

			struct SHData
			{
				float4x4 mat0;
			};

			StructuredBuffer<SHData> shDataBuffer;

			sampler2D _MainTex;


			struct v2f
			{
				float4 pos:POSITION;
				float2 uv : TEXCOORD0;
				float4x4 sh : TEXCOORD1;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos= UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.sh = shDataBuffer[int(o.uv.x)].mat0;
				return o;
			}

			fixed4 frag(v2f v):COLOR
			{

				float r = 0.0f;
				float g = 0.0f;
				float b = 0.0f;

				for(int i=0;i<16;i++)
				{
					int j = i/4;
					int k = i%4;
					r += rotRLight[j][k]*v.sh[j][k];
					g += rotGLight[j][k]*v.sh[j][k];
					b += rotBLight[j][k]*v.sh[j][k];
				}

				float4 col = float4(r,g,b,1);

				return col*_Albedo;
			}


			ENDCG
		}
	}
	FallBack "Diffuse"
}