using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HP
{


    [RequireComponent(typeof(Image))]
    public class LiquidProgressBar : Image
    {
        [Range(-1,1)]
        public new float fillAmount = 1f; // Ѫ��������
        public float waveSpeed = 1f; // Һ�岨���ٶ�
        public float waveHeight = 0.1f; // Һ�岨���߶�
        public float waveTime = 0f; // ���˵�ʱ������

        [Header("����ϸ��")]
        public Vector2Int SubdivisionSurface = new Vector2Int(1, 1);

        public float waveOffset = 0f; // Һ�岨��ƫ����
        public Color color2 ; // �м�Һ����ɫ


        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();

            //ֻ����size��С��ͼƬ v = (���½�x,���½�y,���Ͻ�x,���Ͻ�y)
            var v = new Vector4(r.xMin, r.yMin, r.xMax, r.yMax * fillAmount);

            // ���ԭ�еĶ����������
            vh.Clear();

            // ���ϸ�ֺ�Ķ���
            for (int y = 0; y <= SubdivisionSurface.y; y++)
            {
                for (int x = 0; x <= SubdivisionSurface.x; x++)
                {
                    float xPos = Mathf.Lerp(v.x, v.z, (float)x / SubdivisionSurface.x);
                    float yPos = Mathf.Lerp(v.y, v.w, (float)y / SubdivisionSurface.y);
                    var finalColor = color;
                    if (y == SubdivisionSurface.y)//�������
                    {
                        yPos += Mathf.Sin(waveTime + ((float)x/ SubdivisionSurface.x)*Mathf.PI) * waveHeight * (1-(Mathf.Abs(fillAmount/2)+0.5f));
                        finalColor = color2;
                    }
                    else if (y == SubdivisionSurface.y-1)//�м��y����
                    {
                        yPos = v.w;//���ڶ��Ŷ�������һ�Ŷ���λ��һ��
                        yPos += Mathf.Sin(waveTime + ((float)x / SubdivisionSurface.x) * Mathf.PI + waveOffset) * waveHeight * (1 - (Mathf.Abs(fillAmount / 2) + 0.5f));
                    }

                    vh.AddVert(new Vector3(xPos, yPos), finalColor, new Vector2((float)x / SubdivisionSurface.x, (float)y / SubdivisionSurface.y));

                }
            }

            // ���ϸ�ֺ����������Ƭ
            for (int y = 0; y < SubdivisionSurface.y; y++)
            {
                for (int x = 0; x < SubdivisionSurface.x; x++)
                {
                    int vertexIndex = y * (int)(SubdivisionSurface.x + 1) + x;
                    vh.AddTriangle(vertexIndex, vertexIndex + (int)(SubdivisionSurface.x + 1), vertexIndex + 1);
                    vh.AddTriangle(vertexIndex + 1, vertexIndex + (int)(SubdivisionSurface.x + 1), vertexIndex + (int)(SubdivisionSurface.x + 2));
                }
            }

        }



        private void Update()
        {
            waveTime += Time.deltaTime * waveSpeed;
            SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LiquidProgressBar))]
    public class LiquidProgressBarEditor : UnityEditor.UI.ImageEditor
    {

        private SerializedProperty m_SubdivisionSurface;
        private SerializedProperty m_fillAmount;
        private SerializedProperty m_waveSpeed;
        private SerializedProperty m_waveHeight;
        private SerializedProperty m_waveTime;
        private SerializedProperty m_waveOffset;
        private SerializedProperty m_color2;
        LiquidProgressBar m_Forge;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Forge = target as LiquidProgressBar;
            m_SubdivisionSurface = serializedObject.FindProperty("SubdivisionSurface");
            m_fillAmount = serializedObject.FindProperty("fillAmount");
            m_waveSpeed = serializedObject.FindProperty("waveSpeed");
            m_waveHeight = serializedObject.FindProperty("waveHeight");
            m_waveTime = serializedObject.FindProperty("waveTime");
            m_waveOffset = serializedObject.FindProperty("waveOffset");
            m_color2 = serializedObject.FindProperty("color2");
        }

        //��ȫ��дInspector���
        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            return base.CreateInspectorGUI();
        }

        //������д���ʹ��
        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_SubdivisionSurface);
            EditorGUILayout.PropertyField(m_fillAmount);
            EditorGUILayout.PropertyField(m_waveSpeed);
            EditorGUILayout.PropertyField(m_waveHeight);
            EditorGUILayout.PropertyField(m_waveTime);
            EditorGUILayout.PropertyField(m_waveOffset);
            EditorGUILayout.PropertyField(m_color2);


            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
#endif

}