// 自定义精灵进度条着色器（支持图集、方向切换和反转）
// 功能：根据进度值在指定方向裁剪精灵显示区域，并可反转裁剪范围，支持精灵图集
Shader "Custom/SpriteProgressBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // 主纹理（支持图集）
        [PerRendererData] _Color ("Tint", Color) = (1,1,1,1) // 色调颜色（与原始颜色相乘）
        [PerRendererData] [MaterialToggle] _Horizontal ("Horizontal Direction", Float) = 1 // 方向开关：1=水平，0=垂直
        [PerRendererData] [MaterialToggle] _Invert ("Invert Mask", Float) = 0 // 反转遮罩：1=反转范围，0=正常范围
        [PerRendererData] _Progress ("Progress", Range(0, 1)) = 0.5 // 进度值（0-1，控制显示范围）
        
        // 新增：图集适配参数（由渲染器自动传入，用于处理精灵在图集中的位置）
        [PerRendererData] _SpriteOffset ("Sprite Atlas Offset", Vector) = (0,0,0,0) // 精灵在图集中的UV偏移
        [PerRendererData] _SpriteSize ("Sprite Atlas Size", Vector) = (1,1,0,0) // 精灵在图集中的UV尺寸
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 atlasUV : TEXCOORD0;      // 用于纹理采样（图集UV）
                float2 localUV : TEXCOORD1;     // 用于进度裁剪（本地UV，0-1范围）
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Progress;
            float _Horizontal;
            float _Invert;
            float2 _SpriteOffset;
            float2 _SpriteSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localUV = v.uv; // 保留原始UV（0-1范围，相对于精灵本身）
                
                // 计算图集UV：将本地UV转换为图集中的实际UV坐标
                o.atlasUV = TRANSFORM_TEX(v.uv, _MainTex);
                // o.atlasUV = float2(
                //     _SpriteOffset.x + o.atlasUV.x * _SpriteSize.x,
                //     _SpriteOffset.y + o.atlasUV.y * _SpriteSize.y
                // );
                
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 使用图集UV采样纹理
                fixed4 col = tex2D(_MainTex, i.atlasUV) * i.color;
                
                // 计算当前像素在进度条上的位置（水平或垂直）
                float position = lerp(i.localUV.y, i.localUV.x, _Horizontal);
                
                // 反转位置（如果启用反转选项）
                position = lerp(position, 1 - position, _Invert);
                


                float spriteLocalCoord = _Horizontal ? 
                    i.atlasUV.x : 
                    i.atlasUV.y;

                // 计算进度值对应的坐标（转换为图集UV空间的进度位置）
                float _MaskProgressCoord = _Horizontal ? 
                    _SpriteOffset.x + _Progress * _SpriteSize.x: // 水平方向：偏移 + 进度*宽度
                    _SpriteOffset.y + _Progress * _SpriteSize.y; // 垂直方向：偏移 + 进度*高度

                float value = _Invert? spriteLocalCoord - _MaskProgressCoord: // 反转时：当前坐标 - 进度坐标
                                _MaskProgressCoord - spriteLocalCoord; // 正常时：进度坐标 - 当前坐标

                
                // 使用clip函数裁剪像素：clip(x) 当x<0时丢弃像素
                clip(value  + 0.001); // 添加小偏移量避免精度问题
                
                return col;
            }
            ENDCG
        }
    }
}