Shader "Custom/BlurUIShader" {
    Properties {
        [HideInInspector][PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}//Image����ϵ�Source Image
        [HideInInspector]_Color ("Tint", Color) = (1,1,1,1)//Image����ϵ�Color

        _BlurSize ("Blur Size", Range(0.0, 0.004)) = 0//ƫ�ƴ�С
        _BlurRadius ("Blur Radius", Range(0.0, 10.0)) = 4.0 // ����ģ���뾶����
    }
    SubShader {
        Tags
        {
            "Queue"="Transparent"// ������Ⱦ����Ϊ͸������
        }
        ZWrite Off // �ر����д��
        Blend SrcAlpha OneMinusSrcAlpha // ����͸�����

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"// ����UnityCG.cginc�ļ����ṩ��һЩ���õ�CG�����ͺ�


            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR;//Image����ϵ�Color
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            float _BlurSize;
            float _BlurRadius; // ʹ�ö����ģ���뾶����
            fixed4 _Color;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);// ������λ��ת��Ϊ�ü��ռ�
                o.uv = v.uv;
                o.color = v.color * _Color;//Image����ϵ�Color��ʵ��ͼƬ��ɫ���,Ŀ����Ϊ������Image�ϵ�Color�����޸���ɫ
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                half4 col = tex2D(_MainTex, i.uv);// ��ȡ������ɫ
                half4 blurCol = half4(0, 0, 0, 0); // ��ʼ��ģ����ɫΪ0
                float blurSize = _BlurSize;

                for (float x = -_BlurRadius; x <= _BlurRadius; x += 1) {// ��x�����Ͻ���ģ������
                    for (float y = -_BlurRadius; y <= _BlurRadius; y += 1) {// ��y�����Ͻ���ģ������
                        float2 offset = float2(x * blurSize, y * blurSize);// ����ƫ����
                        blurCol += tex2D(_MainTex, i.uv + offset);// �ۼ�ģ����ɫ
                    }
                }

                blurCol /= (_BlurRadius * 2 +1)*(_BlurRadius * 2 +1);// ��ģ����ɫ����ƽ������

                return blurCol * i.color; // ����ģ�������ɫ
            }
            ENDCG
        }
    }
}
