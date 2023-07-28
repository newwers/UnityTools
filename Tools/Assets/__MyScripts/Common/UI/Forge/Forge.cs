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
        public Vector2 offset;//x 是宽 y是高
        public float ratio = 10;
        public Vector2 spriteSize;
        [Header("曲面细分")]
        public Vector2Int SubdivisionSurface=new Vector2Int(1,1);

        public float forgeRadius = 10f; // 锻造半径
        public float forgeOffset = 1f; // 锻造偏移量
        public float forgeOffsetMax = 10f; // 锻造最大偏移量

        VertexHelper m_vh;
        private Dictionary<int, Vector2> m_vOffsetVertList = new Dictionary<int, Vector2>(); 

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log($"VertCount:{m_vh.currentVertCount}");
            //Debug.Log($"m_vVertList Count:{m_vVertList.Count}");


            if (m_vh == null) return;
            AudioSource.PlayClipAtPoint(audioSource.clip, Vector3.zero);

            // 计算点击位置在UI元素局部坐标系中的位置
            Vector2 localClickPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localClickPos);
            //print($"localClickPos=>{localClickPos},eventData.position:{eventData.position}");
            // 获取网格顶点数
            int vertexCount = (SubdivisionSurface.x + 1) * (SubdivisionSurface.y + 1);

            bool isUpdate = false;
            // 遍历顶点，计算距离点击位置一定半径内的顶点偏移
            for (int i = 0; i < vertexCount; i++)
            {
                if (m_vh.currentVertCount <=i)
                {
                    continue;
                }
                UIVertex vertex = new UIVertex();
                m_vh.PopulateUIVertex(ref vertex, i);

                // 计算顶点在UI元素局部坐标系中的位置
                //Vector2 localVertexPos = rectTransform.InverseTransformPoint(vertex.position);
                Vector2 localVertexPos = vertex.position ;


                // 计算顶点与点击位置的距离
                float distance = Vector2.Distance(localClickPos, localVertexPos);



                // 如果顶点在锻造半径范围内，则进行偏移计算
                if (distance <= forgeRadius)
                {
                    // 计算顶点的偏移方向
                    Vector2 offsetDirection = (localVertexPos - localClickPos).normalized;

                    // 计算顶点的新位置
                    Vector2 newVertexPos = localVertexPos + offsetDirection * forgeOffset;

                    // 将新位置转换为世界坐标系
                    //Vector3 worldVertexPos = rectTransform.TransformPoint(newVertexPos);

                    //print($"<color=#00aa00>index:{i}, vertex.position:{vertex.position} => {newVertexPos},distance:{distance},isOffset</color>");

                    Vector2 newOffset = newVertexPos - new Vector2(vertex.position.x, vertex.position.y);

                    if (m_vOffsetVertList.TryGetValue(i,out var offset))
                    {
                        if (offset.magnitude >= forgeOffsetMax && offset.magnitude < newOffset.magnitude)//旧的偏移超过最大长度,并且新的长度还超过旧的
                        {
                            continue;
                        }
                    }

                    m_vOffsetVertList[i] = offset + newOffset;//最终新偏移 = 原本偏移 + 新偏移
                    

                    isUpdate = true;
                }

                //if (i == 1)
                //{
                //    print($"index:{i}, vertex.position:{vertex.position},beforeOffset:{beforeOffset}");
                //}
            }

            if (isUpdate)
            {
                // 重新生成网格
                //SetAllDirty();
                SetVerticesDirty();
            }
            

        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //print($"OnPopulateMesh begin: vh.vertCount:{vh.currentVertCount}");
            var r = GetPixelAdjustedRect();

            //只绘制size大小的图片 v = (左下角x,左下角y,右上角x,右上角y)
            var v = new Vector4((r.center.x - spriteSize.x/2) + offset.x / 2, (r.center.y - spriteSize.y / 2) + offset.y / 2, (r.center.x + spriteSize.x / 2) - offset.x / 2, (r.center.y + spriteSize.y / 2) - offset.y / 2);//全屏+中间显示图片
                                                                                                                                                                                                                              //var v = new Vector4(r.x + offset.x / 2, r.y + offset.y / 2, r.x + r.width - offset.x / 2, r.y + r.height - offset.y / 2);//初始版本 + 偏移缩放

            // 计算细分后的顶点数量
            int vertexCount = (int)((SubdivisionSurface.x + 1) * (SubdivisionSurface.y + 1));
            // 计算细分后的三角形面片数量
            int triangleCount = (int)(SubdivisionSurface.x * SubdivisionSurface.y * 2);

            // 清空原有的顶点和三角形
            vh.Clear();

            int index = 0;
            // 添加细分后的顶点
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

            // 添加细分后的三角形面片
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

        //完全重写Inspector面板
        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            return base.CreateInspectorGUI();
        }

        //部分重写面板使用
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
