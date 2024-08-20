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
        public int sides = 4; // Ĭ�ϱ���Ϊ4����ʾ�ı���
        public Color highLightColor = Color.gray;//������ɫ
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


            var rect = GetPixelAdjustedRect();//��ȡ��ǰui�ľ���

            // �������ε�����
            center = new Vector2(0f, 0f); // ������Ҫ��������λ��
            radius = rect.width/2; // ������Ҫ�����뾶
            diameter = radius * 2;//ֱ��

            // ����ÿ����֮��Ļ��Ȳ�
            float angleStep = 360f / sides;

            // �洢����

            // ��Ӷ���

            //����Բ��
            m_vVertices.Add(new UIVertex
            {
                position = new Vector3(0, 0),
                color = color,
                uv0 = new Vector2(0.5f, 0.5f) // �����������UV
            });

            for (int i = 1; i <= sides; ++i)//����ÿ������λ�ú�uvλ��,��ɫ
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius + center.x;
                float y = Mathf.Sin(angle) * radius + center.y;
                m_vVertices.Add(new UIVertex
                {
                    position = new Vector3(x, y),
                    color = color,
                    uv0 = new Vector2(Mathf.Clamp01((x + radius) / diameter ), Mathf.Clamp01((y + radius) / diameter)) // �����������UV,uv��������½���Ϊ0��,�������x,y����0.5
                });

                //print($"index:{i},x:{x},y:{y},uv:{new Vector2(Mathf.Clamp01((x + radius) / diameter), Mathf.Clamp01((y + radius) / diameter))}");
            }

            // ���������
            for (int i = 1; i < sides; ++i)
            {
                m_vIndices.Add(0);//��ʱ����Ӷ���˳��
                m_vIndices.Add(i+1);
                m_vIndices.Add(i);
            }
            // ��ն����
            m_vIndices.Add(0);
            m_vIndices.Add(1);
            m_vIndices.Add(sides);

            // ��������ӵ�VertexHelper
            vh.AddUIVertexStream(m_vVertices, m_vIndices);
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return base.Raycast(sp, eventCamera);
        }

        //private void OnMouseDown()
        //{
        //    print("OnMouseDown");//�޷�����,�� OnPointerDown ʵ��
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

            // �ҵ�����x
            SerializedProperty sides = serializedObject.FindProperty("sides");
            SerializedProperty normalColor = serializedObject.FindProperty("normalColor");
            SerializedProperty highLightColor = serializedObject.FindProperty("highLightColor");

            // ����Inspector���
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
