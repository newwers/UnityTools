using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor.UI;
using UnityEditor;
#endif


namespace Z.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RingMenu : Image, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Range(3,128)]
        public int sides = 4; // 默认边数为4，表示四边形
        public Color highLightColor = Color.gray;//高亮颜色
        public Color normalColor = Color.white;

        private Vector2 center;
        private float radius;
        private float diameter;
        List<UIVertex> m_vVertices = new List<UIVertex>();
        List<int> m_vIndices = new List<int>();

        protected override void Awake()
        {
            radius = rectTransform.rect.width / 2;
            diameter = radius* 2;
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            m_vVertices.Clear();
            m_vIndices.Clear();


            var rect = GetPixelAdjustedRect();//获取当前ui的矩形

            // 计算多边形的中心
            center = new Vector2(0f, 0f); // 根据需要调整中心位置
            radius = rect.width/2; // 根据需要调整半径
            diameter = radius * 2;//直径

            // 计算每条边之间的弧度差
            float angleStep = 360f / sides;

            // 存储顶点

            // 添加顶点

            //中心圆点
            m_vVertices.Add(new UIVertex
            {
                position = new Vector3(0, 0),
                color = color,
                uv0 = new Vector2(0.5f, 0.5f) // 根据纹理调整UV
            });

            for (int i = 1; i <= sides; ++i)//计算每个顶点位置和uv位置,颜色
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius + center.x;
                float y = Mathf.Sin(angle) * radius + center.y;
                m_vVertices.Add(new UIVertex
                {
                    position = new Vector3(x, y),
                    color = color,
                    uv0 = new Vector2(Mathf.Clamp01((x + radius) / diameter ), Mathf.Clamp01((y + radius) / diameter)) // 根据纹理调整UV,uv坐标从左下角作为0点,所以这边x,y各加0.5
                });

                //print($"index:{i},x:{x},y:{y},uv:{new Vector2(Mathf.Clamp01((x + radius) / diameter), Mathf.Clamp01((y + radius) / diameter))}");
            }

            // 添加三角形
            for (int i = 1; i < sides; ++i)
            {
                m_vIndices.Add(0);//逆时针添加顶点顺序
                m_vIndices.Add(i+1);
                m_vIndices.Add(i);
            }
            // 封闭多边形
            m_vIndices.Add(0);
            m_vIndices.Add(1);
            m_vIndices.Add(sides);

            // 将顶点添加到VertexHelper
            vh.AddUIVertexStream(m_vVertices, m_vIndices);
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return base.Raycast(sp, eventCamera);
        }

        //private void OnMouseDown()
        //{
        //    print("OnMouseDown");//无法触发,用 OnPointerDown 实现
        //}

        public void OnPointerEnter(PointerEventData eventData)
        {
            print("OnPointerEnter");
            color = highLightColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            print("OnPointerDown");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            print("OnPointerExit");
            color = normalColor;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(RingMenu))]
    public class RingMenuEditor : ImageEditor
    {
        RingMenu m_RingMenu;



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_RingMenu = (RingMenu)target;

            // 找到变量x
            SerializedProperty sides = serializedObject.FindProperty("sides");
            SerializedProperty normalColor = serializedObject.FindProperty("normalColor");
            SerializedProperty highLightColor = serializedObject.FindProperty("highLightColor");

            // 更新Inspector面板
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), sides);
            EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), normalColor);
            EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), highLightColor);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

#endif
}
