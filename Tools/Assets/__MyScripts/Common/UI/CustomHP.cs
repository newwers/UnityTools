/********************************************************************
��������:	12:28:2020 13:16
��    ��: 	CustomHP
��    ��:	zdq
��    ��:	�Զ���Ѫ��
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace zdq.UI
{
    public class CustomHP : Image
    {
        struct DamageData
        {
            public float damage;

            /// <summary>
            /// ����˺��󣬵�ǰѪ��
            /// </summary>
            public float curHP;

            /// <summary>
            /// Ѫ����ʼֵ 0��1
            /// </summary>
            public float startValue;
            /// <summary>
            /// Ѫ������ֵ 0��1
            /// </summary>
            public float endValue;

            /// <summary>
            /// ���ƵĶ���ƫ��
            /// </summary>
            public float vertexOffsetY;

            public Color color;

            /// <summary>
            /// ���ƾ��εĳ���
            /// </summary>
            public float rectLength
            {
                get
                {
                    return Mathf.Clamp(endValue - startValue,0,1);
                }
            }

            /// <summary>
            /// ��ֵ�Ľ��� 0��1
            /// </summary>
            public float lerpPer;

            public float time;

        }

        public Action<int> OnHPChange;

        [Range(0f, 1f)]
        public float slider = 1f;


        public float m_offsetX;//x��ƫ��
        public float m_offsetY;//y��ƫ��

        public List<Color> m_ColorList;

        public float Max;
        public float Cur;
        public float CurLerpValue;
        public float SingleValue;
        public int ColorIndex;
        /// <summary>
        /// �ܵ�Ѫ������
        /// </summary>
        public int MaxHPCount;
        /// <summary>
        /// ��ǰѪ������
        /// </summary>
        public int CurHPCount;

        public bool bInit = false;

        /// <summary>
        /// �˺�Ѫ������ʱ��
        /// </summary>
        public float FlashingTime = 0.5f;

        

        /// <summary>
        /// ÿ�ι����˺�����
        /// </summary>
        DamageData damageData = new DamageData();

        List<DamageData> m_vRecords = new List<DamageData>();

        bool m_bVertexChange = false;

        protected override void Start()
        {
            base.Start();
        }
        //------------------------------------------------------
        void Update()
        {
            VertexLerp();

            ColorLerp();

            if (m_bVertexChange)
            {
                m_bVertexChange = false;
                SetVerticesDirty();
            }

            if (!bInit || CurLerpValue <= Cur)
            {
                return;
            }

            CurLerpValue = Mathf.Lerp(CurLerpValue, Cur, 0.5f);
            RefreshSlider(CurLerpValue);
        }
        
        void VertexLerp()
        {
            if (m_vRecords.Count == 0)
            {
                return;
            }

            DamageData data;
            for (int i = 0; i < m_vRecords.Count; i++)
            {
                data = m_vRecords[i];

                //data.endValue = Mathf.Lerp(data.startValue, data.endValue, 0.5f);

                data.lerpPer = 1 - (Time.realtimeSinceStartup - data.time) / FlashingTime;

                m_vRecords[i] = data;
            }

            m_bVertexChange = true;
        }

        void ColorLerp()
        {
            if (m_vRecords.Count == 0)
            {
                return;
            }

            //����Ѿ�͸��������
            DamageData data;
            for (int i = m_vRecords.Count -1; i >= 0; i--)
            {
                data = m_vRecords[i];
                if (data.color.a <= 0)
                {
                    m_vRecords.RemoveAt(i);
                }
            }

            //������ɫ
            for (int i = 0; i < m_vRecords.Count; i++)
            {
                data = m_vRecords[i];
                data.color.a = data.lerpPer;
                m_vRecords[i] = data;
            }

            m_bVertexChange = true;

        }

        //------------------------------------------------------
        public void Test()
        {
            float value = UnityEngine.Random.Range(0, 100);
            Debug.Log("Random value:" + value);
            //SetLerpValue(Cur - value);
            SetValue(Cur - value);
        }
        //------------------------------------------------------
        public void Init(float cur, float max, int maxHPCount)
        {
            Cur = cur;
            CurLerpValue = Cur;
            Max = max;

            MaxHPCount = maxHPCount;
            CurHPCount = maxHPCount;
            SingleValue = 1;
            if (MaxHPCount > 0)
            {
                SingleValue = max / MaxHPCount;
            }

            ColorIndex = 0;
            SetValue(Cur);
            m_vRecords.Clear();
            bInit = true;
            //��ɫ�����������Լ�����
        }
        //------------------------------------------------------
        /// <summary>
        /// ��ֵ����������ֵ
        /// </summary>
        /// <param name="cur"></param>
        public void SetLerpValue(float cur)
        {

            if (CurLerpValue <= Cur)//��������ڹ�����
            {
                CurLerpValue = Cur;//��¼��һ�ε���ֵ
            }

            //float before = Cur;

            Cur = Mathf.Clamp(cur, 0, Max);//���µ�ǰ��ֵ

            //RefreshDamageData(Cur, before);

            CurHPCount = Mathf.FloorToInt(Cur / SingleValue);
        }
        //------------------------------------------------------
        /// <summary>
        /// ֱ��������ֵ
        /// </summary>
        /// <param name="cur"></param>
        public void SetValue(float cur)
        {
            float before = Cur;

            Cur = Mathf.Clamp(cur, 0, Max);

            CurHPCount = Mathf.FloorToInt(Cur / SingleValue);

            RefreshSlider(Cur);

            RefreshDamageData(Cur, before);

            OnHPChange?.Invoke(CurHPCount);
        }
        //------------------------------------------------------
        void RefreshDamageData(float cur, float before)
        {
            damageData.damage = Mathf.Max(before - cur, 0);//���˵���Ѫ���

            //��¼δ��Ѫǰ�� ��ʼ����
            

            if (damageData.damage > 0)
            {
                damageData.time = Time.realtimeSinceStartup;

                //���㵱ǰ��Ѫ������ 0��1
                float remainder = cur % SingleValue;//��������
                damageData.startValue = remainder / SingleValue;//������������slider

                //�����˺�ǰ��Ѫ������ 0��1
                remainder = before % SingleValue;
                remainder = remainder == 0 ? SingleValue : remainder;
                damageData.endValue = remainder / SingleValue;

                damageData.curHP = before;

                damageData.vertexOffsetY = m_offsetY *( m_vRecords.Count + 1);

                damageData.color = Color.white;

                m_vRecords.Add(damageData);
            }
        }
        //------------------------------------------------------
        void RefreshSlider(float value)
        {
            //���ݵ�ǰֵ,�����sliderֵ,��ɫindex
            float remainder = value % SingleValue;//��������

            slider = remainder / SingleValue;//������������slider

            if (value == Max)//��Ѫ���
            {
                slider = 1;
            }


            float reductValue = Max - value;
            ColorIndex = Mathf.FloorToInt(reductValue / SingleValue);//��ɫindex = ��ȥ��ֵ / ������ɫ��ֵ



            SetVerticesDirty();
        }
        //------------------------------------------------------
        Color GetColor(int index)
        {
            if (m_ColorList.Count == 0)
            {
                Debug.LogError("Ѫ����ɫû������!");
                return Color.white;
            }

            bool isLastColor = false;
            isLastColor = index >= MaxHPCount;
            //Debug.Log("ColorIndex:" + index);

            index = index % m_ColorList.Count;

            Color color = m_ColorList[index];

            if (isLastColor)//���һ��
            {
                color.a = 0;
            }

            return color;
        }
        #region Draw
        //------------------------------------------------------
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            //base.OnPopulateMesh(toFill);
            toFill.Clear();
            Rect rect = GetPixelAdjustedRect();
            DrawHPRect(toFill);

            DamageData data;
            for (int i = 0; i < m_vRecords.Count; i++)
            {
                data = m_vRecords[i];
                DrawDamageRect(toFill,data);
            }

        }

        //------------------------------------------------------
        private void DrawHPRect(VertexHelper vh)
        {
            /*
             ˼·:������������,һ����Ϊ��ǰѪ��,һ����Ϊ��һ��Ѫ��,
            Ȼ�����һ��slider,���п�����ʾ����
            ����м�Ҫ����,��ô��Ҫ�ټ�һ��������Ϊ����ʹ��,ͬʱ����Ч������Ҫʵ��

            1---2-5---6
            |   | |   |
            0---3-4---7
             */
            Rect rect = GetPixelAdjustedRect();//��ȡ��Ѫ��UI�ĳ���

            //��ȡ�������½�����
            float x = /*m_offsetX +*/ rect.xMin;
            float y =/* m_offsetY +*/ rect.yMin;


            Color color1 = GetColor(ColorIndex);
            Color color2 = GetColor(ColorIndex + 1);


            //������һ������
            vh.AddVert(new Vector3(x, y, 0), color1, Vector2.zero);
            vh.AddVert(new Vector3(x, y + rect.height, 0), color1, new Vector2(0, 1));
            vh.AddVert(new Vector3(x + slider * rect.width, y + rect.height, 0), color1, new Vector2(slider, 1));
            vh.AddVert(new Vector3(x + slider * rect.width, y, 0), color1, new Vector2(slider, 0));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);

            //�����ڶ�������

            vh.AddVert(new Vector3(x + slider * rect.width, y, 0), color2, new Vector2(slider, 0));
            vh.AddVert(new Vector3(x + slider * rect.width, y + rect.height, 0), color2, new Vector2(slider, 1));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax, 0), color2, new Vector2(1, 1));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin, 0), color2, new Vector2(1, 0));

            vh.AddTriangle(4, 5, 6);
            vh.AddTriangle(4, 6, 7);
        }
        //------------------------------------------------------
        /// <summary>
        /// ���Ƶ�ǰ���˺�Ѫ��
        /// </summary>
        void DrawDamageRect(VertexHelper vh, DamageData data)
        {
            if (data.damage <= 0)//���˺�����
            {
                return;
            }
            //���� DamageData ���ƶ�Ӧ�ľ���
            //����㿪ʼ����,�����˺���������γ���,
            //���� = �˺� / ����Ѫ����ֵ  ���Ҫ���Ƕ������,���һ�Ҫ���ǵ�ǰ��ʣ���������

            Rect rect = GetPixelAdjustedRect();//��ȡ��Ѫ��UI�ĳ���

            Color color = data.color;


            float remainder = data.curHP % SingleValue;//���㵱ǰѪ������
            remainder = remainder == 0 ? SingleValue : remainder;
            if (data.damage <= remainder)//��ǰѪ���ڵ����
            {
                float length = (data.damage / SingleValue) * data.lerpPer;//�����˺���Ѫ���еĳ��ȱ���

                //���㿪ʼ�ľ������½����� = ��ʼ���½����� + (��ʼ���� - �˺�����) * ���ο��
                float x = rect.xMin + (data.startValue) * rect.width;
                float y = rect.yMin + data.vertexOffsetY;

                int vertCount = vh.currentVertCount;

                //��������
                vh.AddVert(new Vector3(x, y, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + length * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + length * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));


                vh.AddTriangle(vertCount, vertCount + 1, vertCount + 2);
                vh.AddTriangle(vertCount, vertCount + 2, vertCount + 3);
            }
            else//������ǰѪ�����,
            {
                bool isOverNext = (data.damage - remainder) > SingleValue;
                //�ȹ���һ����ǰѪ���ľ���,�����Ƿ񳬹�����Ѫ��,ʹ�ò�ͬ�Ķ���λ�û��ƾ���
                float length = (isOverNext? data.startValue : data.endValue) * data.lerpPer;
                //���㿪ʼ�ľ������½����� = ��ʼ���½����� 
                float x = rect.xMin;
                float y = rect.yMin + data.vertexOffsetY;

                int vertCount = vh.currentVertCount;

                vh.AddVert(new Vector3(x, y, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + length * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + length * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));


                vh.AddTriangle(vertCount, vertCount + 1, vertCount + 2);
                vh.AddTriangle(vertCount, vertCount + 2, vertCount + 3);

                //Ȼ���ٸ���ʣ��Ѫ��������һ��
                //�����һ������һ����,���ٹ���һ���ӿ�ͷ,����ǰλ�õľ��μ���,Ҳ����˵һ���˺���๹����������
                if (isOverNext)
                {
                    vertCount = vh.currentVertCount;

                    vh.AddVert(new Vector3(x, y, 0), color, new Vector2(0.5f, 0.5f));
                    vh.AddVert(new Vector3(x, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                    vh.AddVert(new Vector3(x + rect.width * data.lerpPer, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                    vh.AddVert(new Vector3(x + rect.width * data.lerpPer, y, 0), color, new Vector2(0.5f, 0.5f));


                    vh.AddTriangle(vertCount, vertCount + 1, vertCount + 2);
                    vh.AddTriangle(vertCount, vertCount + 2, vertCount + 3);
                }


                //������Ѫ��ͷ��ʼ����ǰ���ȵľ���
                vertCount = vh.currentVertCount;

                vh.AddVert(new Vector3(x + data.startValue * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + data.startValue * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + Mathf.Lerp(data.startValue, 1, data.lerpPer)* rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + Mathf.Lerp(data.startValue, 1, data.lerpPer) * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));


                vh.AddTriangle(vertCount, vertCount + 1, vertCount + 2);
                vh.AddTriangle(vertCount, vertCount + 2, vertCount + 3);

            }

        }
        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomHP), true)]
    [CanEditMultipleObjects]
    public class CustomHPEditor : UnityEditor.UI.ImageEditor
    {
        private SerializedProperty m_Slider;
        private SerializedProperty m_offsetX;
        private SerializedProperty m_offsetY;
        private SerializedProperty m_ColorList;
        private SerializedProperty m_Max;
        private SerializedProperty m_Cur;
        private SerializedProperty m_SingleValue;
        private SerializedProperty m_MaxHPCount;
        private SerializedProperty m_CurHPCount;
        private SerializedProperty m_FlashingTime;
        private SerializedProperty m_ColorIndex;
        CustomHP m_hp;

        //------------------------------------------------------
        protected override void OnEnable()
        {
            base.OnEnable();
            m_hp = target as CustomHP;
            m_Slider = serializedObject.FindProperty("slider");
            m_offsetX = serializedObject.FindProperty("m_offsetX");
            m_offsetY = serializedObject.FindProperty("m_offsetY");
            m_ColorIndex = serializedObject.FindProperty("ColorIndex");
            m_ColorList = serializedObject.FindProperty("m_ColorList");
            m_Max = serializedObject.FindProperty("Max");
            m_Cur = serializedObject.FindProperty("Cur");
            m_SingleValue = serializedObject.FindProperty("SingleValue");
            m_MaxHPCount = serializedObject.FindProperty("MaxHPCount");
            m_CurHPCount = serializedObject.FindProperty("CurHPCount");
            m_FlashingTime = serializedObject.FindProperty("FlashingTime");

        }
        //------------------------------------------------------
        protected override void OnDisable()
        {
            base.OnDisable();
            //CustomHP image = target as CustomHP;
        }
        //------------------------------------------------------
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Slider);
            EditorGUILayout.PropertyField(m_offsetX);
            EditorGUILayout.PropertyField(m_offsetY);
            EditorGUILayout.PropertyField(m_ColorIndex);
            EditorGUILayout.PropertyField(m_ColorList);
            EditorGUILayout.PropertyField(m_Max);
            EditorGUILayout.PropertyField(m_Cur);
            EditorGUILayout.PropertyField(m_SingleValue);
            EditorGUILayout.PropertyField(m_MaxHPCount);
            EditorGUILayout.PropertyField(m_CurHPCount);
            EditorGUILayout.PropertyField(m_FlashingTime);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();

            if (GUILayout.Button("���Գ�ʼ������"))
            {
                if (m_hp != null)
                {
                    m_hp.Init(5000, 5000, 20);
                }
            }
            if (GUILayout.Button("����"))
            {
                if (m_hp != null)
                {
                    m_hp.Test();
                }
            }
        }
    }
#endif
}