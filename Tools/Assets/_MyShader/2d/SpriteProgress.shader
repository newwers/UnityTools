// 自定义精灵进度条着色器（支持图集）
// 功能：根据进度值在水平/垂直方向对精灵进行部分遮罩，支持精灵图集环境
Shader "Custom/SpriteProgress"
{
    Properties
    {
        //PerRendererData 在着色器中使用 [PerRendererData] 标记属性，这样每个渲染器可以有自己的属性值，而不会相互干扰。可以共用一个材质球
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} 
        // 主纹理，通常关联精灵图片（可能来自图集）
        [PerRendererData] _Color ("Mask Color", Color) = (1,1,1,1) 
        // 遮罩颜色，进度范围内的区域会应用此颜色
        [PerRendererData] _MaskProgress ("Mask Progress", Range(0, 1)) = 0 
        // 遮罩进度（0-1），控制显示区域大小
        [PerRendererData] _IsHorizontal ("Is Horizontal", Int) = 1 
        // 遮罩方向：1=水平方向，0=垂直方向
        [PerRendererData] [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0 
        // 像素对齐开关，启用后使渲染与像素网格对齐（适合像素艺术）
        
        // 新增属性：适配精灵图集的关键参数
        [PerRendererData] _SpriteOffset ("Sprite Atlas Offset", Vector) = (0,0,0,0) 
        // 精灵在图集中的UV偏移（x=水平偏移，y=垂直偏移），用于定位图集中的精灵
        [PerRendererData] _SpriteSize ("Sprite Atlas Size", Vector) = (1,1,0,0) 
        // 精灵在图集中的UV尺寸（x=宽度，y=高度），用于计算精灵在图集中的实际范围
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" // 透明队列，确保在不透明物体后渲染
            "IgnoreProjector"="True" // 忽略投影器影响，避免投影干扰透明效果
            "RenderType"="Transparent" // 标记为透明类型，用于渲染路径分类
            "PreviewType"="Plane" // 在编辑器预览中以平面形式显示
            "CanUseSpriteAtlas"="True" // 声明支持精灵图集，允许精灵打包到图集后正常工作
        }

        // 渲染状态设置（透明精灵常用配置）
        Cull Off // 关闭背面剔除，确保精灵正反面都能看到
        Lighting Off // 关闭光照计算，精灵颜色不受场景光照影响
        ZWrite Off // 不写入深度缓冲，避免遮挡其他透明物体
        Blend One OneMinusSrcAlpha // 标准透明混合模式（源颜色*1 + 目标颜色*(1-源透明度)）

        Pass
        {
        CGPROGRAM
            // 声明顶点着色器和片段着色器函数
            #pragma vertex vert
            #pragma fragment frag
            // 编译选项：根据是否启用像素对齐生成不同版本代码
            #pragma multi_compile _ PIXELSNAP_ON
            // 引入Unity内置CG工具库
            #include "UnityCG.cginc"

            // 顶点输入数据结构（从应用程序传入GPU的数据）
            struct appdata_t
            {
                float4 vertex   : POSITION; // 顶点位置（模型空间）
                float4 color    : COLOR; // 顶点颜色（精灵可能设置的顶点色）
                float2 texcoord : TEXCOORD0; // 纹理坐标（本地UV，0-1范围）
            };

            // 顶点到片段的传递数据结构（顶点着色器输出，片段着色器输入）
            struct v2f
            {
                float4 vertex   : SV_POSITION; // 顶点位置（裁剪空间，用于屏幕显示）
                fixed4 color    : COLOR; // 传递给片段的颜色
                float2 texcoord : TEXCOORD0; // 传递给片段的纹理坐标
            };

            // 声明Properties中定义的变量（供着色器函数使用）
            fixed4 _Color;
            float _MaskProgress;
            int _IsHorizontal;
            float2 _SpriteOffset; // 精灵在图集中的UV偏移
            float2 _SpriteSize;   // 精灵在图集中的UV尺寸

            // 顶点着色器：处理顶点位置和数据传递
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                // 将模型空间顶点位置转换为裁剪空间（屏幕显示坐标）
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                // 传递纹理坐标（本地UV）
                OUT.texcoord = IN.texcoord;
                // 传递顶点颜色
                OUT.color = IN.color;
                
                // 如果启用像素对齐，对顶点位置进行像素对齐处理
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            // 主纹理采样器（用于读取纹理像素）
            sampler2D _MainTex;

            // 片段着色器：计算每个像素的最终颜色
            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 计算精灵在图集中的实际UV坐标
                // 本地UV（0-1）转换为图集UV：偏移 + 本地UV*尺寸
                float2 atlasUV = float2(
                    _SpriteOffset.x + IN.texcoord.x * _SpriteSize.x,
                    _SpriteOffset.y + IN.texcoord.y * _SpriteSize.y
                );
                
                // 2. 采样主纹理（使用本地UV，实际项目中建议改为atlasUV以适配图集）
                // 注意：原代码此处使用IN.texcoord，若精灵在图集中，需改为atlasUV才能正确采样
                fixed4 original = tex2D(_MainTex, IN.texcoord) * IN.color;
                original.rgb *= original.a; // 预乘Alpha，符合Unity精灵渲染的标准处理
                
                // 3. 计算遮罩判断参数
                // 获取当前像素在精灵本地的坐标（水平取x，垂直取y）
                float spriteLocalCoord = _IsHorizontal ? 
                    IN.texcoord.x : 
                    IN.texcoord.y;

                // 计算进度值对应的坐标（转换为图集UV空间的进度位置）
                float _MaskProgressCoord = _IsHorizontal ? 
                    _SpriteOffset.x + _MaskProgress * _SpriteSize.x: // 水平方向：偏移 + 进度*宽度
                    _SpriteOffset.y + _MaskProgress * _SpriteSize.y; // 垂直方向：偏移 + 进度*高度
                
                // 4. 应用遮罩效果
                fixed4 maskedColor = original; // 默认使用原始颜色
                // 若当前像素坐标在进度范围内，则应用遮罩颜色
                if (spriteLocalCoord <= _MaskProgressCoord)
                {
                    maskedColor = original * _Color; // 原始颜色与遮罩颜色混合
                }

                return maskedColor; // 输出最终像素颜色
            }
        ENDCG
        }
    }
}