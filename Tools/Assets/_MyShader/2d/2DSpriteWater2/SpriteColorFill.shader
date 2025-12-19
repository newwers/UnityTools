Shader "Custom/WaterFillShader_AlphaMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 水色填充相关属性
        _WaterColor ("Water Color", Color) = (0.2, 0.6, 1, 0.7)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _WaveIntensity ("Wave Intensity", Range(0, 0.1)) = 0.02
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        // Sprite在图集中的信息
        _SpriteMinUV ("Sprite Min UV", Vector) = (0,0,0,0)
        _SpriteMaxUV ("Sprite Max UV", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
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
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _WaterColor;
            float _FillAmount;
            float _WaveIntensity;
            float _WaveSpeed;
            float2 _SpriteMinUV;
            float2 _SpriteMaxUV;

            struct v2f_custom
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f_custom vert(appdata_t IN)
            {
                v2f_custom OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                
                return OUT;
            }

            // 简单的波浪效果
            float WaveEffect(float x, float time)
            {
                return sin(x * 5.0 + time) * 0.1;
            }

            fixed4 SpriteFrag(v2f_custom IN) : SV_Target
            {
                // 获取原始颜色
                fixed4 originalColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 如果Sprite完全透明，直接返回透明
                if (originalColor.a < 0.001)
                    return fixed4(0, 0, 0, 0);
                
                // 计算Sprite的局部UV坐标
                float localY = (IN.texcoord.y - _SpriteMinUV.y) / (_SpriteMaxUV.y - _SpriteMinUV.y);
                
                // 创建波浪效果
                float waveTime = _Time.y * _WaveSpeed;
                float waveOffset = sin(IN.texcoord.x * 20.0 + waveTime) * _WaveIntensity;
                float waterLevel = _FillAmount + waveOffset;
                
                // 判断是否在水面以下
                float isInWater = step(localY, waterLevel);
                
                // 如果不在水面以下，直接返回原始颜色
                if (isInWater < 0.5)
                    return originalColor;
                
                // 计算深度效果：离水面越近，颜色越浅
                float depthFactor = 1.0 - (waterLevel - localY) / waterLevel;
                depthFactor = clamp(depthFactor, 0.3, 1.0);
                
                // 计算波浪高光
                float surfaceDistance = 1.0 - abs(localY - waterLevel) * 10.0;
                surfaceDistance = clamp(surfaceDistance, 0, 1);
                float highlight = smoothstep(0.5, 0.8, surfaceDistance) * 0.4;
                
                // 计算最终的水色
                fixed4 waterColor = _WaterColor;
                waterColor.rgb *= depthFactor;
                waterColor.rgb += fixed3(highlight, highlight, highlight);
                waterColor.a *= originalColor.a;  // 继承Sprite的alpha通道
                
                // 混合原始颜色和水色
                // 使用alpha混合
                fixed4 result = originalColor;
                float waterAlphaFactor = waterColor.a * 0.7;  // 控制水色的强度
                
                // 混合水色和原始颜色
                result.rgb = lerp(originalColor.rgb, waterColor.rgb, waterAlphaFactor);
                
                // 保持原始透明度不变
                result.a = originalColor.a;
                
                return result;
            }
            ENDCG
        }
    }
}