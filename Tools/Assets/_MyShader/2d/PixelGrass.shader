Shader "Custom/PixelGrass"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}
        _Color("Base Color", Color) = (1,1,1,1)
        _WindFrequency("控制风速（数值越大摆动越快）", Range(0,5)) = 2.0
        _WindAmplitude("控制摆动幅度", Range(0,0.5)) = 0.1
        _WaveScale("控制波纹密度", Range(0,20)) = 8.0
        _Stiffness("控制草的硬度（0=柔软，1=僵硬）", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "DisableBatching" = "True" // 禁用合批以保证顶点动画效果
        }
        
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR; // 使用顶点颜色控制摇摆
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            half _WindFrequency;
            half _WindAmplitude;
            half _WaveScale;
            fixed _Stiffness;

            // 噪声函数用于自然波动
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);//frac 函数，全称为 ​​fractional part function​​（取小数部分函数）
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // 仅影响草的上半部分
                float verticalInfluence = clamp(v.vertex.y * 2.0, 0, 1); // 底部为0，顶部为1
                verticalInfluence *= verticalInfluence; // 二次曲线增强效果
                
                // 基于时间和顶点位置的风向偏移
                float phase = _Time.y * _WindFrequency;//_Time.y 一倍数时间
                float2 windDir = float2(sin(phase), cos(phase * 0.8)) * 0.3;
                
                // 添加基于顶点的随机变化
                // float vertexNoise = noise(float2(v.vertex.x * 0.3, phase));
                // float2 offset = windDir * _WindAmplitude * verticalInfluence * (0.8 + 0.4 * vertexNoise);
                float2 offset = windDir * _WindAmplitude * verticalInfluence;
                
                // 添加波纹效果
                 float wave = sin(_Time.y * _WindFrequency * 1.5 + v.vertex.x * _WaveScale) * 0.08;
                 offset.x += wave * verticalInfluence * (1.0 - _Stiffness);
                
                // 使用顶点颜色控制摇摆幅度
                 offset *= v.color.g * 1.2;
                
                v.vertex.x += offset;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}