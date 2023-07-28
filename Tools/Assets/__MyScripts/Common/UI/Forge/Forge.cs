using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Forge
{
    public class Forge : Image,IPointerClickHandler
    {
        public AudioSource audioSource;
        public Vector2 offset;//x �ǿ� y�Ǹ�
        public float ratio = 10;
        public Vector2 spriteSize;
        [Header("����ϸ��")]
        public Vector2Int SubdivisionSurface=new Vector2Int(1,1);

        public float forgeRadius = 10f; // ����뾶
        public float forgeOffset = 1f; // ����ƫ����
        public float forgeOffsetMax = 10f; // �������ƫ����

        VertexHelper m_vh;
        private Dictionary<int, Vector2> m_vOffsetVertList = new Dictionary<int, Vector2>(); 

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log($"VertCount:{m_vh.currentVertCount}");
            //Debug.Log($"m_vVertList Count:{m_vVertList.Count}");


            if (m_vh == null) return;
            AudioSource.PlayClipAtPoint(audioSource.clip, Vector3.zero);

            // ������λ����UIԪ�ؾֲ�����ϵ�е�λ��
            Vector2 localClickPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localClickPos);
            //print($"localClickPos=>{localClickPos},eventData.position:{eventData.position}");
            // ��ȡ���񶥵���
            int vertexCount = (SubdivisionSurface.x + 1) * (SubdivisionSurface.y + 1);

            bool isUpdate = false;
            // �������㣬���������λ��һ���뾶�ڵĶ���ƫ��
            for (int i = 0; i < vertexCount; i++)
            {
                if (m_vh.currentVertCount <=i)
                {
                    continue;
                }
                UIVertex vertex = new UIVertex();
                m_vh.PopulateUIVertex(ref vertex, i);

                // ���㶥����UIԪ�ؾֲ�����ϵ�е�λ��
                //Vector2 localVertexPos = rectTransform.InverseTransformPoint(vertex.position);
                Vector2 localVertexPos = vertex.position ;


                // ���㶥������λ�õľ���
                float distance = Vector2.Distance(localClickPos, localVertexPos);



                // ��������ڶ���뾶��Χ�ڣ������ƫ�Ƽ���
                if (distance <= forgeRadius)
                {
                    // ���㶥���ƫ�Ʒ���
                    Vector2 offsetDirection = (localVertexPos - localClickPos).normalized;

                    // ���㶥�����λ��
                    Vector2 newVertexPos = localVertexPos + offsetDirection * forgeOffset;

                    // ����λ��ת��Ϊ��������ϵ
                    //Vector3 worldVertexPos = rectTransform.TransformPoint(newVertexPos);

                    //print($"<color=#00aa00>index:{i}, vertex.position:{vertex.position} => {newVertexPos},distance:{distance},isOffset</color>");

                    Vector2 newOffset = newVertexPos - new Vector2(vertex.position.x, vertex.position.y);

                    if (m_vOffsetVertList.TryGetValue(i,out var offset))
                    {
                        if (offset.magnitude >= forgeOffsetMax && offset.magnitude < newOffset.magnitude)//�ɵ�ƫ�Ƴ�����󳤶�,�����µĳ��Ȼ������ɵ�
                        {
                            continue;
                        }
                    }

                    m_vOffsetVertList[i] = offset + newOffset;//������ƫ�� = ԭ��ƫ�� + ��ƫ��
                    

                    isUpdate = true;
                }

                //if (i == 1)
                //{
                //    print($"index:{i}, vertex.position:{vertex.position},beforeOffset:{beforeOffset}");
                //}
            }

            if (isUpdate)
            {
                // ������������
                //SetAllDirty();
                SetVerticesDirty();
            }
            

        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //print($"OnPopulateMesh begin: vh.vertCount:{vh.currentVertCount}");
            var r = GetPixelAdjustedRect();

            //ֻ����size��С��ͼƬ v = (���½�x,���½�y,���Ͻ�x,���Ͻ�y)
            var v = new Vector4((r.center.x - spriteSize.x/2) + offset.x / 2, (r.center.y - spriteSize.y / 2) + offset.y / 2, (r.center.x + spriteSize.x / 2) - offset.x / 2, (r.center.y + spriteSize.y / 2) - offset.y / 2);//ȫ��+�м���ʾͼƬ
                                                                                                                                                                                                                              //var v = new Vector4(r.x + offset.x / 2, r.y + offset.y / 2, r.x + r.width - offset.x / 2, r.y + r.height - offset.y / 2);//��ʼ�汾 + ƫ������

            // ����ϸ�ֺ�Ķ�������
            int vertexCount = (int)((SubdivisionSurface.x + 1) * (SubdivisionSurface.y + 1));
            // ����ϸ�ֺ����������Ƭ����
            int triangleCount = (int)(SubdivisionSurface.x * SubdivisionSurface.y * 2);

            // ���ԭ�еĶ����������
            vh.Clear();

            int index = 0;
            // ���ϸ�ֺ�Ķ���
            for (int y = 0; y <= SubdivisionSurface.y; y++)
            {
                for (int x = 0; x <= SubdivisionSurface.x; x++)
                {
                    float xPos = Mathf.Lerp(v.x, v.z, (float)x / SubdivisionSurface.x);
                    float yPos = Mathf.Lerp(v.y, v.w, (float)y / SubdivisionSurface.y);
                    

                    if (m_vOffsetVertList.TryGetValue(index,out var offset))
                    {
                        xPos += offset.x;
                        yPos += offset.y;
                    }
                    vh.AddVert(new Vector3(xPos, yPos), color, new Vector2((float)x / SubdivisionSurface.x, (float)y / SubdivisionSurface.y));

                    //if (index ==1)
                    //{
                    //    print($"index:{index},pos:{new Vector3(xPos, yPos)},offset:{offset}");
                    //}

                    index++;
                }
            }

            // ���ϸ�ֺ����������Ƭ
            for (int y = 0; y < SubdivisionSurface.y; y++)
            {
                for (int x = 0; x < SubdivisionSurface.x; x++)
                {
                    int vertexIndex = y * (int)(SubdivisionSurface.x + 1) + x;
                    vh.AddTriangle(vertexIndex, vertexIndex + (int)(SubdivisionSurface.x + 1), vertexIndex + 1);
                    vh.AddTriangle(vertexIndex + 1,vertexIndex + (int)(SubdivisionSurface.x + 1), vertexIndex + (int)(SubdivisionSurface.x + 2));
                }
            }


            m_vh = vh;
            print($"OnPopulateMesh end: vh.vertCount:{vh.currentVertCount}");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Forge))]
    public class ForgeEditor : UnityEditor.UI.ImageEditor
    {

        SerializedProperty m_Offset;
        private SerializedProperty m_AudioSource;
        private SerializedProperty m_Ratio;
        private SerializedProperty m_SpriteSize;
        private SerializedProperty m_SubdivisionSurface;
        private SerializedProperty m_ForgeRadius;
        private SerializedProperty m_ForgeOffset;
        private SerializedProperty m_ForgeOffsetMax;
        Forge m_Forge;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Forge = target as Forge;
            m_Offset = serializedObject.FindProperty("offset");
            m_AudioSource = serializedObject.FindProperty("audioSource");
            m_Ratio = serializedObject.FindProperty("ratio");
            m_SpriteSize = serializedObject.FindProperty("spriteSize");
            m_SubdivisionSurface = serializedObject.FindProperty("SubdivisionSurface");
            m_ForgeRadius = serializedObject.FindProperty("forgeRadius");
            m_ForgeOffset = serializedObject.FindProperty("forgeOffset");
            m_ForgeOffsetMax = serializedObject.FindProperty("forgeOffsetMax");
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
            EditorGUILayout.PropertyField(m_Offset);
            EditorGUILayout.PropertyField(m_AudioSource);
            EditorGUILayout.PropertyField(m_Ratio);
            EditorGUILayout.PropertyField(m_SpriteSize);
            EditorGUILayout.PropertyField(m_SubdivisionSurface);
            EditorGUILayout.PropertyField(m_ForgeRadius);
            EditorGUILayout.PropertyField(m_ForgeOffset);
            EditorGUILayout.PropertyField(m_ForgeOffsetMax);


            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
#endif
}
