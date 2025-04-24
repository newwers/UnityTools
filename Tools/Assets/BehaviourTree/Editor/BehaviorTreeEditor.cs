using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BehaviorTreeEditor : EditorWindow
{
    public enum NodeType { Selector, Sequence, Task }

    public class BehaviorTreeNode
    {
        public Rect rect;
        public string title;
        public string descript;
        public NodeType type;
        public List<BehaviorTreeNode> children = new List<BehaviorTreeNode>();
        public BehaviorTreeNode parent;
        public bool isRoot => parent == null;

        public BehaviorTreeNode(string title, string des, NodeType type, Vector2 position)
        {
            this.title = title;
            this.type = type;
            descript = des;
            rect = new Rect(position.x, position.y, 150, 70);
        }
    }

    private List<BehaviorTreeNode> nodes = new List<BehaviorTreeNode>();
    private BehaviorTreeNode selectedNode;
    private Vector2 dragOffset; // 用于节点拖拽的偏移量
    private Vector2 zoomOffset; // 用于缩放的偏移量
    private Vector2 drag;
    private float zoom = 1f;
    private readonly float minZoom = 0.5f;
    private readonly float maxZoom = 2f;

    [MenuItem("行为树/行为树编辑器")]
    public static void ShowWindow()
    {
        GetWindow<BehaviorTreeEditor>("行为树编辑器");
    }

    private void OnGUI()
    {
        ProcessMouseScroll(Event.current);

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnections();
        DrawNodes();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        DrawToolbar();

        if (GUI.changed) Repaint();
    }

    private void ProcessMouseScroll(Event e)
    {
        if (e.type == EventType.ScrollWheel)
        {
            float scrollDelta = e.delta.y;
            float zoomDelta = scrollDelta > 0 ? -0.1f : 0.1f;

            float newZoom = Mathf.Clamp(zoom + zoomDelta, minZoom, maxZoom);

            Vector2 mousePosition = e.mousePosition;
            Vector2 zoomCenter = (mousePosition - zoomOffset) / zoom;

            zoom = newZoom;

            zoomOffset = mousePosition - zoomCenter * zoom;

            e.Use();
            GUI.changed = true;
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / (gridSpacing * zoom));
        int heightDivs = Mathf.CeilToInt(position.height / (gridSpacing * zoom));

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        dragOffset += drag * 0.5f;
        Vector3 newOffset = new Vector3((dragOffset.x + zoomOffset.x) % (gridSpacing * zoom), (dragOffset.y + zoomOffset.y) % (gridSpacing * zoom), 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i * zoom, -gridSpacing * zoom, 0) + newOffset, new Vector3(gridSpacing * i * zoom, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing * zoom, gridSpacing * j * zoom, 0) + newOffset, new Vector3(position.width, gridSpacing * j * zoom, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();
        Handles.matrix = Matrix4x4.TRS(zoomOffset, Quaternion.identity, new Vector3(zoom, zoom, 1));// 设置缩放和偏移矩阵
        foreach (BehaviorTreeNode node in nodes)
        {
            foreach (BehaviorTreeNode child in node.children)
            {
                Vector3 start = new Vector3(node.rect.x + node.rect.width / 2, node.rect.y + node.rect.height, 0) - Vector3.down * 10;
                Vector3 end = new Vector3(child.rect.x + child.rect.width / 2, child.rect.y, 0);
                Handles.DrawBezier(start, end, start - Vector3.down * 50, end - Vector3.up * 50, Color.white, null, 3f / zoom);
            }
        }
        Handles.matrix = Matrix4x4.identity;// 恢复矩阵
        Handles.EndGUI();

    }

    private void DrawNodes()
    {
        Color normalColor = GUI.color;
        GUI.matrix = Matrix4x4.TRS(zoomOffset, Quaternion.identity, new Vector3(zoom, zoom, 1));
        for (int i = 0; i < nodes.Count; i++)
        {
            BehaviorTreeNode node = nodes[i];

            if (node == selectedNode)
            {
                //选中效果
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = normalColor;
            }

            // 绘制节点的背景
            GUI.Box(node.rect, node.title);

            // 绘制节点的标题
            //GUI.Label(new Rect(node.rect.x, node.rect.y, node.rect.width, 20), node.title);

            // 绘制节点的内容
            GUI.Label(new Rect(node.rect.x, node.rect.y + 25, node.rect.width, 50), node.descript);

            // 绘制连接点
            float widght = 32;
            float height = 16;
            Rect outRect = new Rect(node.rect.x + node.rect.width / 2 - widght / 2, node.rect.y + node.rect.height, widght, height);
            Rect inRect = new Rect(node.rect.x + node.rect.width / 2 - widght / 2, node.rect.y - height, widght, height);
            if (node.isRoot == false)
            {
                GUI.Button(inRect, "");
            }

            GUI.Button(outRect, "");
        }
        GUI.matrix = Matrix4x4.identity;
        GUI.color = normalColor;
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("新建", EditorStyles.toolbarButton))
        {
            CreatedTree();
        }
        if (GUILayout.Button("保存", EditorStyles.toolbarButton))
        {
            SaveTree();
        }
        if (GUILayout.Button("加载", EditorStyles.toolbarButton))
        {
            LoadTree();
        }
        if (GUILayout.Button("加载最后一次文件", EditorStyles.toolbarButton))
        {
            LoadLastTree();
        }
        GUILayout.EndHorizontal();
    }


    private void LoadLastTree()
    {
        // 加载最后一次修改的文件
        var path = PlayerPrefs.GetString("LastTreePath");
        if (string.IsNullOrWhiteSpace(path))
        {
            LoadTree();
        }
        else
        {
            LoadTree(path);
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            Vector2 scaledMousePosition = (e.mousePosition - zoomOffset) / zoom;
            bool isClickNode = nodes[i].rect.Contains(scaledMousePosition);

            if (isClickNode || selectedNode != null)
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0 && isClickNode)
                        {
                            SetSelectionNode(nodes[i]);
                            drag = Vector2.zero;
                            GUI.changed = true;
                        }
                        if (e.button == 1 && isClickNode)
                        {
                            SetSelectionNode(nodes[i]);
                            ProcessContextMenu(selectedNode);
                            e.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (e.button == 0)
                        {
                            if (isClickNode)
                            {
                                SetSelectionNode(nodes[i]);
                            }

                            DragNode(selectedNode, e.delta / zoom);
                            e.Use();
                        }
                        break;
                }
            }
        }

        //todo:如果所有节点都没有被点击，清除选中状态
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDrag:
                if (e.button == 2)
                {
                    OnDrag(e.delta);
                    e.Use();
                }
                break;

            case EventType.MouseDown:
                if (e.button == 1)
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("创建选择器节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, NodeType.Selector));
                    genericMenu.AddItem(new GUIContent("创建序列节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, NodeType.Sequence));
                    genericMenu.AddItem(new GUIContent("创建任务节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, NodeType.Task));
                    genericMenu.ShowAsContext();
                    e.Use();
                }
                break;
        }
    }

    private void ProcessContextMenu(BehaviorTreeNode node)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("创建新子节点"), false, () => OnClickCreateChildNode(node));
        genericMenu.AddItem(new GUIContent("删除节点"), false, () => OnClickDeleteChildNode(node));
        genericMenu.ShowAsContext();
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        foreach (BehaviorTreeNode node in nodes)
        {
            node.rect.position += delta / zoom;
        }

        GUI.changed = true;
    }

    private void DragNode(BehaviorTreeNode node, Vector2 delta)
    {
        if (node.parent != null)
        {
            var parentNode = node.parent;
            var posY = node.rect.position.y + delta.y;
            if (posY < parentNode.rect.position.y + parentNode.rect.height)
            {
                delta = new Vector2(delta.x, 0);
            }
        }

        node.rect.position += delta;
        DragChildrenNode(node, delta);

        GUI.changed = true;
    }

    void DragChildrenNode(BehaviorTreeNode node, Vector2 delta)
    {
        if (node.children == null || node.children.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < node.children.Count; i++)
        {
            node.children[i].rect.position += delta;
        }
    }

    private void OnClickCreateNode(Vector2 position, NodeType type)
    {
        position.x = Mathf.Clamp(position.x, 0, this.position.width / zoom);
        position.y = Mathf.Clamp(position.y, 0, this.position.height / zoom);
        nodes.Add(new BehaviorTreeNode(type + "节点", "节点描述", type, position));
        Repaint();
    }

    private void OnClickCreateChildNode(BehaviorTreeNode parent)
    {
        BehaviorTreeNode child = new BehaviorTreeNode("新子节点,这边节点类型需要选择而不是固定", "节点描述", NodeType.Task, parent.rect.position + new Vector2(0, parent.rect.height * 1.8f));
        child.parent = parent;
        parent.children.Add(child);
        nodes.Add(child);
        Repaint();
    }

    private void OnClickDeleteChildNode(BehaviorTreeNode node)
    {
        if (selectedNode == null)
        {
            return;
        }

        if (EditorUtility.DisplayDialog("确认删除", $"确定要删除节点 '{node.title}' 吗？此操作无法撤销！", "删除", "取消"))
        {
            DeleteNode(node);
        }
    }

    private void DeleteNode(BehaviorTreeNode nodeToDelete)
    {
        // 从父节点的子节点列表中移除
        if (nodeToDelete.parent != null)
        {
            nodeToDelete.parent.children.Remove(nodeToDelete);
        }
        // 从主节点列表中移除
        nodes.Remove(nodeToDelete);
        // 清除选中的节点
        ClearSelection();
        Repaint();
    }

    private void ClearSelection()
    {
        selectedNode = null;
    }

    private void SetSelectionNode(BehaviorTreeNode node)
    {
        selectedNode = node;
        //Debug.Log("选择了节点:" + node.title);
    }


    private void CreatedTree()
    {
        // 先保存?

        nodes.Clear();

    }

    private void SaveTree()
    {
        string path = EditorUtility.SaveFilePanel("保存行为树", "", "BehaviorTree.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = JsonUtility.ToJson(new SerializableTree(nodes));
            File.WriteAllText(path, json);
        }
    }

    private void LoadTree()
    {
        string path = EditorUtility.OpenFilePanel("加载行为树", "", "json");
        LoadTree(path);
        Repaint();
    }

    private void LoadTree(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);
            SerializableTree tree = JsonUtility.FromJson<SerializableTree>(json);
            nodes = tree.ToNodes();
            PlayerPrefs.SetString("LastTreePath", path); // 保存最后一次加载的路径
        }
    }

    [System.Serializable]
    private class SerializableTree
    {
        public List<SerializableNode> nodes = new List<SerializableNode>();

        public SerializableTree(List<BehaviorTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                this.nodes.Add(new SerializableNode(node));
            }
        }

        public List<BehaviorTreeNode> ToNodes()
        {
            Dictionary<int, BehaviorTreeNode> nodeMap = new Dictionary<int, BehaviorTreeNode>();
            foreach (var sNode in nodes)
            {
                nodeMap[sNode.id] = new BehaviorTreeNode(sNode.title, sNode.descript, sNode.type, sNode.position);
            }

            foreach (var sNode in nodes)
            {
                if (sNode.parentId != -1)
                {
                    nodeMap[sNode.id].parent = nodeMap[sNode.parentId];
                    nodeMap[sNode.parentId].children.Add(nodeMap[sNode.id]);
                }
            }

            return new List<BehaviorTreeNode>(nodeMap.Values);
        }
    }

    [System.Serializable]
    private class SerializableNode
    {
        public int id;
        public string title;
        public string descript;
        public NodeType type;
        public Vector2 position;
        public int parentId;

        public SerializableNode(BehaviorTreeNode node)
        {
            id = node.GetHashCode();
            title = node.title;
            descript = node.descript;
            type = node.type;
            position = node.rect.position;
            parentId = node.parent != null ? node.parent.GetHashCode() : -1;
        }
    }
}