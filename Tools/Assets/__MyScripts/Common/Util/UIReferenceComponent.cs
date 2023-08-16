/********************************************************************
生成日期:	2020-06-28
类    名: 	UIReferenceComponent
作    者:	zdq
描    述:	UI控件引用获取组件,主要用在Scroll View的Cell上,避免每次获取组件都通过Find的方式进行
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Z.UI
{
    public class UIReferenceComponent : MonoBehaviour
    {
        [System.Serializable]
        public class UIReferenceData
        {
            public string name;
            public Component component;
        }

        public List<UIReferenceData> Datas;
        Dictionary<string,UIReferenceData> m_vuiReferences = new Dictionary<string, UIReferenceData>();

        private void Awake()
        {
            for (int i = 0; i < Datas.Count; i++)
            {
                var item = Datas[i];
                if (item.component == null)
                {
                    continue;
                }
                string key = item.name;
                if (string.IsNullOrWhiteSpace(item.name))
                {
                    item.name = item.component.name;
                    key = item.name;
                }

                m_vuiReferences[key] = item;
            }
        }

        public T GetUI<T>(string name) where T : Component
        {
            if (m_vuiReferences.Count == 0 && Datas.Count > 0)//未初始化判断,初始隐藏状态不执行awake函数
            {
                Awake();
            }
            if (m_vuiReferences.TryGetValue(name,out var value))
            {
                return value.component as T;
            }

            return null;
        }


        

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UIReferenceComponent))]
    public class UIReferenceComponentEditor : Editor
    {
        UIReferenceComponent m_ui;
        private SerializedProperty m_uiReferenceList;

        private void OnEnable()
        {
            m_ui = target as UIReferenceComponent;

            m_uiReferenceList = serializedObject.FindProperty("uiReferenceList");
        }

        public override void OnInspectorGUI()
        {
            if (m_ui == null || m_ui.Datas == null)
            {
                return;
            }

            
            

            serializedObject.Update();

            for (int i = 0; i < m_ui.Datas.Count; i++)
            {
                var item = m_ui.Datas[i];
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("复制代码"))
                {
                    if (item.component != null)
                    {
                        var typeName = item.component.GetType().FullName;
                        GUIUtility.systemCopyBuffer = typeName + " " + item.name.ToLower() + " = ui.GetUI<" + typeName + ">(\"" + item.name + "\");";
                    }
                }
                item.name = EditorGUILayout.TextField("名称:", item.name);
                item.component = EditorGUILayout.ObjectField(item.component, typeof(Component), true) as Component;
                if (GUILayout.Button("X"))
                {
                    m_ui.Datas.Remove(item);


                    EditorUtility.SetDirty(target);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            


            //EditorGUILayout.PropertyField(m_uiReferenceList);

            if (GUILayout.Button("添加"))
            {
                m_ui.Datas.Add(new UIReferenceComponent.UIReferenceData());

                UnityEditor.EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("设置所有元素名字和物体名字一致"))
            {
                for (int i = 0; i < m_ui.Datas.Count; i++)
                {
                    var item = m_ui.Datas[i];
                    if (item.component == null)
                    {
                        continue;
                    }
                    string key = item.name;
                    if (string.IsNullOrWhiteSpace(item.name))
                    {
                        item.name = item.component.name;
                        key = item.name;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            //base.OnInspectorGUI();
        }

    }
#endif
}
