Shader "Custom/WaterFillShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 水色填充相关属性
        _WaterColor ("Water Color", Color) = (0.2, 0.6, 1, 0.7)  // 水蓝色，半透明
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
        Blend One OneMinusSrcAlpha

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
                float2 worldPos : TEXCOORD1;
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
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xy;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                
                return OUT;
            }

            // 简单的正弦波函数
            float WaveEffect(float2 pos, float time)
            {
                return sin(pos.x * 10 + time) * 0.5 + 0.5;
            }

            fixed4 SpriteFrag(v2f_custom IN) : SV_Target
            {
                // 获取原始颜色
                fixed4 originalColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                originalColor.rgb *= originalColor.a;
                
                // 计算Sprite的局部UV坐标
                float localY = (IN.texcoord.y - _SpriteMinUV.y) / (_SpriteMaxUV.y - _SpriteMinUV.y);
                
                // 创建波浪效果的水面边界
                float waveOffset = sin(IN.worldPos.x * 10 + _Time.y * _WaveSpeed) * _WaveIntensity;
                float waterLevel = _FillAmount + waveOffset;
                
                // 判断是否在水面以下
                float isInWater = step(localY, waterLevel);
                
                // 水色颜色（使用半透明）
                fixed4 waterColor = _WaterColor;
                
                // 计算深度效果：离水面越近，颜色越浅
                float depthFactor = 1.0 - (waterLevel - localY) / waterLevel;
                depthFactor = clamp(depthFactor, 0.2, 1.0);
                
                // 增强水面附近的水波高光
                float surfaceDistance = 1.0 - abs(localY - waterLevel) * 20.0;
                surfaceDistance = clamp(surfaceDistance, 0, 1);
                float highlight = smoothstep(0.7, 0.8, surfaceDistance) * 0.3;
                
                // 应用深度和波浪
                waterColor.rgb = waterColor.rgb * depthFactor;
                waterColor.rgb += fixed3(highlight, highlight, highlight);
                
                // Alpha混合：水色与原始颜色的混合
                // 使用相乘混合模式，保持水下的半透明效果
                fixed4 waterLayer = waterColor;
                waterLayer.rgb *= waterLayer.a;  // 预乘alpha
                
                // 在填充区域内应用水色效果
                // 使用屏幕混合模式，实现自然的水下效果
                fixed4 result = originalColor;
                
                if (isInWater > 0.5)
                {
                    // 混合计算：水色与原始颜色叠加
                    fixed3 waterOverlay = 2.0 * waterColor.rgb * originalColor.rgb;
                    fixed3 screenBlend = 1.0 - (1.0 - waterColor.rgb) * (1.0 - originalColor.rgb);
                    
                    // 使用加权混合
                    fixed3 finalColor = lerp(originalColor.rgb, screenBlend, waterColor.a * 0.7);
                    
                    // 添加水色的颜色叠加
                    finalColor = finalColor * (1.0 - waterColor.a * 0.5) + waterColor.rgb * waterColor.a * 0.5;
                    
                    result.rgb = finalColor;
                    
                    // 在水下稍微降低对比度，模拟水的散射效果
                    result.rgb = lerp(result.rgb, fixed3(0.5, 0.5, 0.5), waterColor.a * 0.2);
                }
                
                return result;
            }
            ENDCG
        }
    }
}