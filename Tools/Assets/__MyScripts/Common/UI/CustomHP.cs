/********************************************************************
生成日期:	12:28:2020 13:16
类    名: 	CustomHP
作    者:	zdq
描    述:	自定义血条
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TopGame.UI
{
    [UIWidgetExport]
    public class CustomHP : Image
    {
        struct DamageData
        {
            public float damage;
            /// <summary>
            /// 当前血条百分比(0到1),例如从50%掉到20%,那么这就是0.5f
            /// </summary>
            public float startValue;
            /// <summary>
            /// 受伤前,当前血量
            /// </summary>
            public float curHP;

            public float time;
            /// <summary>
            /// 剩余百分比(0到1),例如剩余20%血量,就是0.2f
            /// </summary>
            public float endValue;

            /// <summary>
            /// 绘制的顶点偏移
            /// </summary>
            public float vertexOffsetY;

            public Color color;

            /// <summary>
            /// 绘制矩形的长度
            /// </summary>
            public float rectLength
            {
                get
                {
                    return Mathf.Clamp(endValue - startValue, 0, 1);
                }
            }

            /// <summary>
            /// 插值的进度 0到1
            /// </summary>
            public float lerpPer;

            public bool IsShow
            {
                get
                {
                    return (Time.time - time) < 0.5f;
                }
            }
        }


        [Range(0f, 1f)]
        public float slider = 1f;

        public bool bReverse = false;

        public float m_offsetX;//x轴偏移
        public float m_offsetY;//y轴偏移

        public List<Color> m_ColorList;

        public float Max;
        public float Cur;
        public float CurLerpValue;
        public float SingleValue;
        public int ColorIndex;
        /// <summary>
        /// 总的血条数量
        /// </summary>
        public int MaxHPCount;
        /// <summary>
        /// 当前血条数量
        /// </summary>
        public int CurHPCount;

        public bool bInit = false;

        public float FlashingTime = 0.5f;

        /// <summary>
        /// 每次攻击伤害数据
        /// </summary>
        DamageData damageData = new DamageData();

        List<DamageData> m_vRecords = new List<DamageData>();
        private bool m_bVertexChange;

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
        //------------------------------------------------------
        public void Test()
        {
            float value = Random.Range(0, 100);
            Debug.Log("Random value:" + value);
            //SetLerpValue(Cur- value);
            SetValue(Cur - value);
        }
        //------------------------------------------------------
        public void Init(float cur,float max,int maxHPCount)
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
            //颜色在组件面板中自己调整
        }
        //------------------------------------------------------
        /// <summary>
        /// 插值表现设置数值
        /// </summary>
        /// <param name="cur"></param>
        public void SetLerpValue(float cur)
        {
            
            if (CurLerpValue <= Cur)//如果不处于过渡中
            {
                CurLerpValue = Cur;//记录上一次得数值
            }

            //float before = Cur;

            Cur = Mathf.Clamp(cur, 0, Max);//更新当前数值

            //RefreshDamageData(Cur, before);

            CurHPCount = Mathf.FloorToInt(Cur / SingleValue);
        }
        //------------------------------------------------------
        /// <summary>
        /// 直接设置数值
        /// </summary>
        /// <param name="cur"></param>
        public void SetValue(float cur)
        {
            float before = Cur;
            
            Cur = Mathf.Clamp(cur,0,Max);

            CurHPCount = Mathf.FloorToInt(Cur / SingleValue);

            RefreshDamageData(Cur, before);

            RefreshSlider(Cur);
        }
        //------------------------------------------------------
        void RefreshDamageData(float cur,float before)
        {
            damageData.damage = Mathf.Max(before - cur, 0);//过滤掉回血情况
            float startValue = before % SingleValue;
            if (startValue == 0)
            {
                startValue = SingleValue;
            }
            damageData.startValue = startValue / SingleValue;//记录未掉血前的 开始坐标
            damageData.curHP = before;

            if (damageData.damage > 0)
            {
                damageData.color = color;
                damageData.time = Time.time;
                float remainder = Cur % SingleValue;//计算余数
                //if (remainder == 0)//这边如果一次性扣除一条血,则需要表现
                //{
                //    remainder = SingleValue;
                //}
                damageData.endValue = remainder / SingleValue;//根据余数计算slider
            }
        }
        //------------------------------------------------------
        void RefreshSlider(float value)
        {
            //根据当前值,计算出slider值,颜色index
            float remainder = value % SingleValue;//计算余数

            slider = remainder / SingleValue;//根据余数计算slider

            if (value == Max)//满血情况
            {
                slider = 1;
            }

            if (bReverse)//取反
            {
                slider = 1 - slider;
            }

            float reductValue = Max - value;
            ColorIndex = Mathf.FloorToInt(reductValue / SingleValue);//颜色index = 减去的值 / 单个颜色的值

            

            SetVerticesDirty();
        }
        //------------------------------------------------------
        Color GetColor(int index)
        {
            if (m_ColorList == null || m_ColorList.Count == 0)
            {
                Debug.LogError("血条颜色没有设置!");
                return Color.white;
            }

            bool isLastColor = false;
            isLastColor = index >= MaxHPCount;
            //Debug.Log("ColorIndex:" + index);

            index = index % m_ColorList.Count;

            Color color = Color.red;
            if (index >= 0 && index < m_ColorList.Count)
            {
                color = m_ColorList[index];
            }

            if (isLastColor)//最后一条
            {
                color.a = 0;
            }

            return color;
        }
        //------------------------------------------------------

        void VertexLerp()
        {
            DamageData data = damageData;
            if (data.damage > 0 && data.IsShow)
            {
                data.lerpPer = 1 - Mathf.Clamp01((Time.time - data.time) / FlashingTime);
                damageData = data;
                m_bVertexChange = true;
            }
        }

        void ColorLerp()
        {
            //更新颜色
            if (damageData.IsShow)
            {
                damageData.color.a = damageData.lerpPer;

                m_bVertexChange = true;
            }

        }

        #region Draw
        //------------------------------------------------------
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            //base.OnPopulateMesh(toFill);
            toFill.Clear();
            Rect rect = GetPixelAdjustedRect();
            DrawHPRect(toFill);
            DrawDamageRect(toFill,damageData);
        }

        //------------------------------------------------------
        private void DrawHPRect(VertexHelper vh)
        {
            /*
             思路:绘制两个矩形,一个作为当前血条,一个作为下一个血条,
            然后根据一个slider,进行控制显示比例
            如果中间要过渡,那么需要再加一个矩形作为过渡使用,同时过渡效果还需要实现

            1---2-5---6
            |   | |   |
            0---3-4---7
             */
            Rect rect = GetPixelAdjustedRect();//获取到血条UI的长宽

            //获取矩形左下角坐标
            float x = m_offsetX + rect.xMin;
            float y = m_offsetY + rect.yMin;


            Color color1 = GetColor(ColorIndex);
            Color color2 = GetColor(ColorIndex + 1);

            if (bReverse)
            {
                color1 = GetColor(ColorIndex +1);
                color2 = GetColor(ColorIndex);
            }


            //构建第一个矩形
            vh.AddVert(new Vector3(x, y, 0), color1, Vector2.zero);
            vh.AddVert(new Vector3(x, y + rect.height, 0), color1, new Vector2(0,1));
            vh.AddVert(new Vector3(x  + slider * rect.width, y + rect.height, 0), color1, new Vector2(slider, 1));
            vh.AddVert(new Vector3(x  + slider * rect.width, y, 0), color1, new Vector2(slider, 0));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);

            //构建第二个矩形

            vh.AddVert(new Vector3(x + slider * rect.width, y, 0), color2, new Vector2(slider, 0));
            vh.AddVert(new Vector3(x + slider * rect.width, y + rect.height, 0), color2, new Vector2(slider, 1));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax, 0), color2, new Vector2(1, 1));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin, 0), color2, new Vector2(1, 0));

            vh.AddTriangle(4, 5, 6);
            vh.AddTriangle(4, 6, 7);
        }
        //------------------------------------------------------
        /// <summary>
        /// 绘制当前的伤害血条
        /// </summary>
        void DrawDamageRect(VertexHelper vh, DamageData data)
        {
            if (data.damage <= 0)//无伤害过滤
            {
                return;
            }
            //根据 DamageData 绘制对应的矩形
            //从起点开始绘制,根据伤害计算出矩形长度,
            //长度 = 伤害 / 单个血条数值  这边要考虑多条情况,并且还要考虑当前条剩余数的情况

            Rect rect = GetPixelAdjustedRect();//获取到血条UI的长宽

            Color color = data.color;


            float remainder = data.curHP % SingleValue;//计算当前血条余数
            remainder = remainder == 0 ? SingleValue : remainder;
            if (data.damage <= remainder)//当前血条内的情况
            {
                float length = (data.damage / SingleValue) * data.lerpPer;//本次伤害在血条中的长度比例

                //计算开始的矩形左下角坐标 = 起始左下角坐标 + (开始坐标 - 伤害长度) * 矩形宽度
                float x = rect.xMin + (data.endValue) * rect.width;
                float y = rect.yMin + data.vertexOffsetY;

                int vertCount = vh.currentVertCount;

                //构建矩形
                vh.AddVert(new Vector3(x, y, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + length * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + length * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));


                vh.AddTriangle(vertCount, vertCount + 1, vertCount + 2);
                vh.AddTriangle(vertCount, vertCount + 2, vertCount + 3);
            }
            else//超出当前血条情况,
            {
                bool isOverNext = (data.damage - remainder) > SingleValue;
                //先构建一条当前血条的矩形,根据是否超过两条血条,使用不同的顶点位置绘制矩形
                float length = (isOverNext ? data.startValue : data.endValue) * data.lerpPer;
                //计算开始的矩形左下角坐标 = 起始左下角坐标 
                float x = rect.xMin;
                float y = rect.yMin + data.vertexOffsetY;

                int vertCount = vh.currentVertCount;

                //vh.AddVert(new Vector3(x, y, 0), color, new Vector2(0.5f, 0.5f));
                //vh.AddVert(new Vector3(x, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                //vh.AddVert(new Vector3(x + length * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                //vh.AddVert(new Vector3(x + length * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));


                //vh.AddTriangle(vertCount, vertCount + 1, vertCount + 2);
                //vh.AddTriangle(vertCount, vertCount + 2, vertCount + 3);

                //然后再根据剩余血量构建下一条
                //如果下一条超过一整条,则再构建一条从开头,到当前位置的矩形即可,也就是说一次伤害最多构建三个矩形
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


                //构建从血条头开始到当前进度的矩形
                vertCount = vh.currentVertCount;

                vh.AddVert(new Vector3(x + data.endValue * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + data.endValue * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + Mathf.Lerp(data.endValue, 1, data.lerpPer) * rect.width, y + rect.height, 0), color, new Vector2(0.5f, 0.5f));
                vh.AddVert(new Vector3(x + Mathf.Lerp(data.endValue, 1, data.lerpPer) * rect.width, y, 0), color, new Vector2(0.5f, 0.5f));


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
        private SerializedProperty m_bReverse;
        private SerializedProperty m_MaxHPCount;
        private SerializedProperty m_CurHPCount;
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
            m_bReverse = serializedObject.FindProperty("bReverse");
            m_MaxHPCount = serializedObject.FindProperty("MaxHPCount");
            m_CurHPCount = serializedObject.FindProperty("CurHPCount");

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
            EditorGUILayout.PropertyField(m_bReverse);
            EditorGUILayout.PropertyField(m_offsetX);
            EditorGUILayout.PropertyField(m_offsetY);
            EditorGUILayout.PropertyField(m_ColorIndex);
            EditorGUILayout.PropertyField(m_ColorList);
            EditorGUILayout.PropertyField(m_Max);
            EditorGUILayout.PropertyField(m_Cur);
            EditorGUILayout.PropertyField(m_SingleValue);
            EditorGUILayout.PropertyField(m_MaxHPCount);
            EditorGUILayout.PropertyField(m_CurHPCount);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();

            if (GUILayout.Button("测试初始化参数"))
            {
                if (m_hp != null)
                {
                    m_hp.Init(500, 500, 5);
                }
            }
            if (GUILayout.Button("测试"))
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