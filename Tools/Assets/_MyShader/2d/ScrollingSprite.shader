Shader "Custom/ScrollingSpriteInAtlas"
{
    Properties
    {
        _MainTex ("Sprite Atlas Texture", 2D) = "white" {}
        _SpriteRect ("Sprite UV Rectangle", Vector) = (0,0,1,1)
        _ScrollSpeed ("Scroll Speed (XY)", Vector) = (0.5, 0.0, 0, 0)
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _SpriteRect;  // (x:uMin, y:vMin, z:uWidth, w:vHeight)
            float2 _ScrollSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 计算当前精灵在图集中的局部UV坐标 (0-1范围内)
                float2 localUV = (i.uv - _SpriteRect.xy) / _SpriteRect.zw;
                
                // 2. 应用滚动偏移
                float2 scrollUV = float2(
                    _ScrollSpeed.x * _Time.y, 
                    _ScrollSpeed.y * _Time.y
                );
                
                // 3. 保持循环滚动
                float2 scrolledUV = frac(localUV - scrollUV);
                
                // 4. 将局部UV映射回图集坐标
                float2 atlasUV = scrolledUV * _SpriteRect.zw + _SpriteRect.xy;
                
                // 5. 采样纹理并应用顶点颜色
                fixed4 color = tex2D(_MainTex, atlasUV) * i.color;
                return color;
            }
            ENDCG
        }
    }
}