Shader "Unlit/CustomCull"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_UVOffsetX ("UVOffsetX", Range(0,1)) = 1
		_UVOffsetY ("UVOffsetY", Range(0,1)) = 1
		_MainTexOffsetX ("MainTexOffsetX", float) = 1
		_MainTexOffsetY ("MainTexOffsetY", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _UVOffsetX;
			float _UVOffsetY;
			float _MainTexOffsetX;
			float _MainTexOffsetY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				float2 offset = float2(_UVOffsetX,_UVOffsetY);
				_MainTex_ST.xy += float2(_MainTexOffsetX,_MainTexOffsetY);
                o.uv = TRANSFORM_TEX(v.uv + offset, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
