// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/CullFront"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Diffuse ("Diffuse", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {
			"RenderType"="Opaque"
			"LightMode"="ForwardBase"
		}
        LOD 100
		Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;//模型的顶点坐标空间
                float2 uv : TEXCOORD0;//第一张贴图
				float3 normal : NORMAL;//法线
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;//标记 作为投影空间坐标
				fixed3 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Diffuse;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				//开始半兰伯特光照计算

				//获取环境光 是Unity自带的宏定义常量
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;//环境光
				//将模型的顶点坐标转换撑世界坐标
				fixed3 worldNormal = normalize(mul(v.normal,(float3x3)unity_WorldToObject));
				//获取世界空间的光照方向
				fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
				//计算漫反射 = 第一个光照颜色 * 自定义漫反射颜色 * 世界空间下的光照和世界空间下的模型顶点的点击
				//半兰伯特和兰伯特的区别在于这边dot 时,多了一步 * 0.5 +0.5的操作,让原本0-1的范围,改为0.5-1的区间,提高的暗处的亮度
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal,worldLight)*0.5 + 0.5);
				//最终颜色 = 漫反射颜色 + 环境光颜色
				o.color = diffuse + ambient;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
