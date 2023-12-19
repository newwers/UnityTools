Shader "Custom/BlurUIShader" {
    Properties {
        [HideInInspector][PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}//Image组件上的Source Image
        [HideInInspector]_Color ("Tint", Color) = (1,1,1,1)//Image组件上的Color

        _BlurSize ("Blur Size", Range(0.0, 0.004)) = 0//偏移大小
        _BlurRadius ("Blur Radius", Range(0.0, 10.0)) = 4.0 // 定义模糊半径参数
    }
    SubShader {
        Tags
        {
            "Queue"="Transparent"// 设置渲染队列为透明类型
        }
        ZWrite Off // 关闭深度写入
        Blend SrcAlpha OneMinusSrcAlpha // 开启透明混合

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"// 包含UnityCG.cginc文件，提供了一些常用的CG函数和宏


            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR;//Image组件上的Color
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            float _BlurSize;
            float _BlurRadius; // 使用定义的模糊半径参数
            fixed4 _Color;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);// 将顶点位置转换为裁剪空间
                o.uv = v.uv;
                o.color = v.color * _Color;//Image组件上的Color和实际图片颜色混合,目的是为了能让Image上的Color可以修改颜色
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                half4 col = tex2D(_MainTex, i.uv);// 获取纹理颜色
                half4 blurCol = half4(0, 0, 0, 0); // 初始化模糊颜色为0
                float blurSize = _BlurSize;

                for (float x = -_BlurRadius; x <= _BlurRadius; x += 1) {// 在x方向上进行模糊计算
                    for (float y = -_BlurRadius; y <= _BlurRadius; y += 1) {// 在y方向上进行模糊计算
                        float2 offset = float2(x * blurSize, y * blurSize);// 计算偏移量
                        blurCol += tex2D(_MainTex, i.uv + offset);// 累加模糊颜色
                    }
                }

                blurCol /= (_BlurRadius * 2 +1)*(_BlurRadius * 2 +1);// 对模糊颜色进行平均处理

                return blurCol * i.color; // 返回模糊后的颜色
            }
            ENDCG
        }
    }
}
