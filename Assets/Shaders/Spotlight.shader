Shader "IQM/Spotlight"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CharPos("Char Pos", vector) = (0,0,0,0)
		_Radius("Spotlight Radius", Range(0,20)) = 3
		_RingSize("Ring Size", Range(0,5)) = 1
		_ColourTint("Outside Tint", Color) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
				float3 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _CharPos;
			float _Radius;
			float _RingSize;
			float4 _ColourTint;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _ColourTint;
				float dist = distance(i.worldPos, _CharPos.xyz);
				if (dist < _Radius)
				{
					col = tex2D(_MainTex, i.uv);
				} else if (dist > _Radius && dist < _Radius + _RingSize)
				{
					float blendStr = dist - _Radius;
					col = lerp(tex2D(_MainTex, i.uv), _ColourTint, blendStr / _RingSize);
				}

				return col;
			}
			ENDCG
		}
	}
}
