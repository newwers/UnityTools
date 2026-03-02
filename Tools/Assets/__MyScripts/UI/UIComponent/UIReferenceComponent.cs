/********************************************************************
生成日期:	2020-06-28
类    名: 	UIReferenceComponent
作    者:	zdq
描    述:	UI控件引用获取组件,避免每次获取组件都通过Find的方式进行
*********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using static Z.UI.UIReferenceComponent;
using System.Text;

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

        public List<UIReferenceData> Datas = new List<UIReferenceData>();
        Dictionary<string, UIReferenceData> m_vUiReferences = new Dictionary<string, UIReferenceData>();

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

                m_vUiReferences[key] = item;
            }
        }

        public T GetUI<T>(string name) where T : Component
        {
            if (m_vUiReferences.Count == 0 && Datas.Count > 0)//未初始化判断,初始隐藏状态不执行awake函数
            {
                Awake();
            }
            if (m_vUiReferences.TryGetValue(name, out var value))
            {
                return value.component as T;
            }

            return null;
        }

        public GameObject GetUIGameObject(string name)
        {
            if (m_vUiReferences.Count == 0 && Datas.Count > 0)//未初始化判断,初始隐藏状态不执行awake函数
            {
                Awake();
            }
            if (m_vUiReferences.TryGetValue(name, out var value))
            {
                return value.component.gameObject;
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
        List<UIReferenceData> m_vPreviousDatas;
        private int m_draggedSlotIndex = -1;
        // 新增：前缀输入框的字段
        private string m_prefix = "";

        private void OnEnable()
        {
            m_ui = target as UIReferenceComponent;

            m_uiReferenceList = serializedObject.FindProperty("uiReferenceList");
            m_vPreviousDatas = new List<UIReferenceData>();
            if (m_ui.Datas != null)
            {
                for (int i = 0; i < m_ui.Datas.Count; i++)
                {
                    m_vPreviousDatas.Add(new UIReferenceData() { component = m_ui.Datas[i].component });
                }
            }
        }

        // 新增：更新所有Datas的name前缀
        private void UpdateAllNamesWithPrefix()
        {
            if (string.IsNullOrEmpty(m_prefix) || m_ui.Datas == null) return;

            for (int i = 0; i < m_ui.Datas.Count; i++)
            {
                var item = m_ui.Datas[i];
                if (item.component == null) continue;

                // 1. 确定基础名称（无前缀）
                string baseName;
                if (string.IsNullOrWhiteSpace(item.name))
                {
                    // 无名称时用 组件名_组件类型 作为基础名
                    baseName = $"{item.component.name.Replace(' ', '_')}_{item.component.GetType().Name}";
                }
                else
                {
                    // 有名称时，移除已有的相同前缀（避免重复添加）
                    baseName = item.name.StartsWith($"{m_prefix}_")
                        ? item.name.Substring($"{m_prefix}_".Length)
                        : item.name;
                }

                // 2. 拼接新前缀
                item.name = $"{m_prefix}_{baseName}";
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_ui == null || m_ui.Datas == null)
            {
                base.OnInspectorGUI();
                return;
            }

            serializedObject.Update();

            // ========== 新增：前缀设置区域 ==========
            EditorGUILayout.BeginHorizontal();
            m_prefix = EditorGUILayout.TextField("名称前缀:", m_prefix);
            if (GUILayout.Button("更新所有前缀", GUILayout.Width(120)))
            {
                UpdateAllNamesWithPrefix();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator(); // 分隔线
            // =======================================

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

                if (item.component != m_vPreviousDatas[i].component)
                {
                    m_draggedSlotIndex = i;
                    m_vPreviousDatas[i].component = item.component;
                    Debug.Log("m_draggedSlotIndex:" + m_draggedSlotIndex);
                }

                if (GUILayout.Button("X"))
                {
                    m_ui.Datas.Remove(item);
                    for (int j = 0; j < m_vPreviousDatas.Count; j++)
                    {
                        if (m_vPreviousDatas[j].component == item.component)
                        {
                            m_vPreviousDatas.RemoveAt(j);
                            break;
                        }
                    }

                    EditorUtility.SetDirty(target);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("添加"))
            {
                m_ui.Datas.Add(new UIReferenceComponent.UIReferenceData());
                m_vPreviousDatas.Add(new UIReferenceComponent.UIReferenceData());
                UnityEditor.EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("复制所有"))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in m_ui.Datas)
                {
                    if (item.component != null)
                    {
                        var typeName = item.component.GetType().FullName;
                        sb.AppendLine(typeName + " " + item.name.ToLower() + " = ui.GetUI<" + typeName + ">(\"" + item.name + "\");");
                    }
                }
                GUIUtility.systemCopyBuffer = sb.ToString();
            }

            if (UnityEngine.Event.current.type == EventType.DragExited)
            {
                if (DragAndDrop.objectReferences.Length > 0 && m_draggedSlotIndex >= 0 && m_draggedSlotIndex < m_ui.Datas.Count)
                {
                    var item = m_ui.Datas[m_draggedSlotIndex];
                    var obj = DragAndDrop.objectReferences[0];
                    var go = obj as GameObject;
                    Component[] comps;
                    if (go != null)
                    {
                        comps = go.GetComponents<Component>();
                    }
                    else
                    {
                        comps = new Component[] { obj as Component };
                    }

                    GenericMenu menu = new GenericMenu();
                    foreach (var comp in comps)
                    {
                        menu.AddItem(new GUIContent(comp.GetType().Name), false, () =>
                        {
                            item.component = comp;
                            // ========== 修改：自动生成name时添加前缀 ==========
                            string baseName = $"{comp.name.Replace(' ', '_')}_{comp.GetType().Name}";
                            item.name = string.IsNullOrEmpty(m_prefix)
                                ? baseName
                                : $"{m_prefix}_{baseName}";
                            // ==============================================
                        });
                    }
                    menu.ShowAsContext();
                }
            }

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
#endif
}