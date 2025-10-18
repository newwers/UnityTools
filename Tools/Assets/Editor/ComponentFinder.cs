using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ComponentFinder : EditorWindow
{
    private string searchType = "";
    private Vector2 scrollPosition;
    private List<GameObject> foundObjects = new List<GameObject>();
    private bool includeInactive = false;
    private bool searchInChildren = true;

    [MenuItem("Tools/Component Finder")]
    public static void ShowWindow()
    {
        GetWindow<ComponentFinder>("Component Finder");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // 搜索设置区域
        EditorGUILayout.LabelField("组件类型搜索", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("输入完整的组件类型名称（如：Rigidbody, MeshRenderer, YourCustomScript）", MessageType.Info);

        GUILayout.Space(5);

        // 搜索类型输入
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("组件类型:", GUILayout.Width(80));
        searchType = EditorGUILayout.TextField(searchType);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 选项设置
        EditorGUILayout.BeginVertical("box");
        includeInactive = EditorGUILayout.Toggle("包含未激活对象", includeInactive);
        searchInChildren = EditorGUILayout.Toggle("在子对象中搜索", searchInChildren);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // 搜索按钮
        if (GUILayout.Button("搜索场景中的组件", GUILayout.Height(30)))
        {
            SearchComponents();
        }

        GUILayout.Space(10);

        // 结果显示
        EditorGUILayout.LabelField($"找到 {foundObjects.Count} 个对象", EditorStyles.boldLabel);

        if (foundObjects.Count > 0)
        {
            // 全选按钮
            if (GUILayout.Button("选择所有找到的对象"))
            {
                Selection.objects = foundObjects.ToArray();
            }

            GUILayout.Space(5);

            // 对象列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, "box", GUILayout.Height(200));

            foreach (var obj in foundObjects)
            {
                if (obj == null) continue;

                EditorGUILayout.BeginHorizontal();

                // 显示对象名称和选择按钮
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);

                if (GUILayout.Button("选择", GUILayout.Width(50)))
                {
                    Selection.activeGameObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // 快捷搜索按钮
        GUILayout.Space(15);
        EditorGUILayout.LabelField("快捷搜索", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rigidbody"))
        {
            searchType = "Rigidbody";
            SearchComponents();
        }
        if (GUILayout.Button("MeshRenderer"))
        {
            searchType = "MeshRenderer";
            SearchComponents();
        }
        if (GUILayout.Button("Collider"))
        {
            SearchByBaseType("Collider");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Camera"))
        {
            searchType = "Camera";
            SearchComponents();
        }
        if (GUILayout.Button("Light"))
        {
            searchType = "Light";
            SearchComponents();
        }
        if (GUILayout.Button("Canvas"))
        {
            searchType = "Canvas";
            SearchComponents();
        }
        if (GUILayout.Button("TextMeshProUGUI"))
        {
            searchType = "TextMeshProUGUI";
            SearchComponents();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SearchComponents()
    {
        foundObjects.Clear();

        if (string.IsNullOrEmpty(searchType))
        {
            Debug.LogWarning("请输入要搜索的组件类型名称");
            return;
        }

        // 获取场景中的所有游戏对象
        var allObjects = FindObjectsByType<GameObject>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);


        foreach (var obj in allObjects)
        {
            if (obj == null) continue;

            // 检查组件是否存在
            Component component = null;

            if (searchInChildren)
            {
                component = obj.GetComponent(searchType);
            }
            else
            {
                component = obj.GetComponent(System.Type.GetType(searchType));
            }

            if (component != null)
            {
                foundObjects.Add(obj);
            }
        }

        // 按名称排序
        foundObjects = foundObjects.OrderBy(obj => obj.name).ToList();

        Debug.Log($"找到 {foundObjects.Count} 个包含 {searchType} 组件的对象");

        if (foundObjects.Count > 0)
        {
            // 自动选择第一个找到的对象
            Selection.activeGameObject = foundObjects[0];
            EditorGUIUtility.PingObject(foundObjects[0]);
        }
    }

    private void SearchByBaseType(string baseTypeName)
    {
        foundObjects.Clear();
        searchType = baseTypeName;

        var allObjects = FindObjectsByType<GameObject>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var baseType = System.Type.GetType($"UnityEngine.{baseTypeName}, UnityEngine");

        if (baseType == null)
        {
            Debug.LogError($"未找到类型: {baseTypeName}");
            return;
        }

        foreach (var obj in allObjects)
        {
            if (obj == null) continue;

            Component[] components;
            if (searchInChildren)
            {
                components = obj.GetComponentsInChildren(baseType, includeInactive);
            }
            else
            {
                components = obj.GetComponents(baseType);
            }

            if (components != null && components.Length > 0)
            {
                foundObjects.Add(obj);
            }
        }

        foundObjects = foundObjects.Distinct().OrderBy(obj => obj.name).ToList();
        Debug.Log($"找到 {foundObjects.Count} 个包含 {baseTypeName} 类型组件的对象");

        if (foundObjects.Count > 0)
        {
            Selection.activeGameObject = foundObjects[0];
            EditorGUIUtility.PingObject(foundObjects[0]);
        }
    }

    private void OnInspectorUpdate()
    {
        // 定期重绘窗口，确保对象引用仍然有效
        Repaint();
    }
}