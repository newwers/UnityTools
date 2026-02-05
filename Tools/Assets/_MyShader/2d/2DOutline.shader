/*
这个Shader通过在片元着色器中采样当前像素上下左右四个方向的alpha值，来判断当前像素是否处于轮廓位置。如果四个方向都有alpha（即都不是透明），则当前像素使用原始颜色；否则，使用轮廓颜色。这样，在精灵的透明边界处就会绘制出轮廓。
注意：这个Shader有一个潜在问题，就是当轮廓宽度较大时，四个方向的采样点可能不足以覆盖整个轮廓区域，导致轮廓不连续。另外，这个Shader只在一个Pass中绘制，所以轮廓和原始颜色是同时绘制的。而且，它只考虑了上下左右四个方向，没有考虑四个斜角方向，可能会导致斜角方向的轮廓缺失。如果需要更完整的轮廓，可以考虑使用八个方向（包括斜角）的采样，或者使用边缘检测算法
*/

Shader "Custom/2DOutline"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 1)) = 0.01
    }
    SubShader
    {
        // 标签，设置渲染队列为透明，这样可以在其他不透明物体之后渲染，避免被遮挡
        Tags
        {
            "Queue"="Transparent"
        }
        // 混合模式，设置Alpha混合，用于透明效果
        // 混合模式：透明混合（SrcAlpha * 源颜色 + OneMinusSrcAlpha * 目标颜色）
        Blend SrcAlpha OneMinusSrcAlpha

        // Base Layer
        Pass
        {
            CGPROGRAM
            // 指定顶点和片元着色器函数
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // 顶点着色器输入结构
            struct appdata
            {
                // 顶点位置
                float4 vertex : POSITION;
                // 纹理坐标
                float2 uv : TEXCOORD0;
            };

            // 顶点着色器输出结构，也是片元着色器输入结构
            struct v2f
            {
                // 纹理坐标
                float2 uv : TEXCOORD0;
                // 裁剪空间中的位置
                float4 pos : SV_POSITION;
            };

            // 声明在Properties块中定义的变量
            sampler2D _MainTex;
            // 用于存储主纹理的缩放和偏移，但此处未使用，因为注释掉了TRANSFORM_TEX
            float4 _MainTex_ST;
            // 主纹理的纹素大小，用于计算相邻像素的UV偏移
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                // 将顶点从对象空间转换到裁剪空间
                o.pos = UnityObjectToClipPos(v.vertex);
                // 通常使用TRANSFORM_TEX来处理纹理的缩放和偏移，但这里被注释掉了，直接使用原始UV
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样主纹理，得到当前像素的颜色
                fixed4 col = tex2D(_MainTex, i.uv);

                // 计算当前像素上下左右四个方向的UV坐标，偏移量由轮廓宽度和纹素大小决定
                // 纹素大小（TexelSize）是一个float4类型，其xy分量表示一个纹素在UV空间中的大小（即1/纹理尺寸）
                // 因此，通过将_OutlineWidth与纹素大小相乘，可以得到在UV空间中对应轮廓宽度的偏移量
                float2 uv_up = i.uv+_MainTex_TexelSize.xy * float2(0.0, _OutlineWidth);
                float2 uv_down = i.uv-_MainTex_TexelSize.xy * float2(0.0, _OutlineWidth);
                float2 uv_left = i.uv-_MainTex_TexelSize.xy * float2(_OutlineWidth, 0.0);
                float2 uv_right = i.uv+_MainTex_TexelSize.xy * float2(_OutlineWidth, 0.0);

                // 分别采样四个方向纹理的alpha值，并相乘得到w
                // 如果当前像素的四个方向都有alpha值（即都不为0），则w=1，否则w=0
                // 注意：这里使用的是相乘，所以只要有一个方向的alpha为0，w就会为0
                // float w = tex2D(_MainTex, uv_up).a * tex2D(_MainTex,uv_down).a
                // * tex2D(_MainTex, uv_left).a * tex2D(_MainTex, uv_right).a;

                // // 使用lerp（线性插值）函数，根据w的值在轮廓颜色和原始颜色之间插值
                // // 当w=1时，使用原始颜色；当w=0时，使用轮廓颜色
                // // 注意：这里只改变了rgb通道，alpha通道保持不变
                // col.rgb = lerp(_OutlineColor, col.rgb, w);


                //另一种描边实现方式：将四个方向的alpha值相加，得到总的alpha值
                float totalAlpha = tex2D(_MainTex, uv_up).a + tex2D(_MainTex, uv_down).a + tex2D(_MainTex, uv_left).a + tex2D(_MainTex, uv_right).a;
                totalAlpha = saturate(totalAlpha);
                float4 a = (totalAlpha - col.a) * _OutlineColor;
                fixed4 b = a + col;
                col.rgb = b.rgb;
                col.a = totalAlpha;
                

                // 返回最终颜色
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}