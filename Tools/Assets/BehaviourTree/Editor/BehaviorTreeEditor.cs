using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Z.BehaviourTree;

public class BehaviorTreeEditor : EditorWindow
{
    public enum NodeType { Root, Action, Composite, Decorator }
    public enum PortType { In, Out }

    public class BehaviorTreeNode
    {
        public Node node;
        public Rect rect
        {
            get => node.rect;
        }

        public string title => node.title;
        public string descript => node.descript;

        public List<BehaviorTreeNode> children = new List<BehaviorTreeNode>();
        public BehaviorTreeNode parent;
        public bool hasParent => parent == null;

        public BehaviorTreeNode(string title, string des, Node node, Vector2 position)
        {
            node.title = title;
            node.descript = des;
            node.rect = new Rect(position.x, position.y, 150, 70);
            this.node = node;
        }

        public BehaviorTreeNode(Node node)
        {
            this.node = node;
        }

    }

    private struct ConnectingPort
    {
        public BehaviorTreeNode node;
        public PortType portType;
    }
    BehaviorTree m_BehaviourTreeData;
    private List<BehaviorTreeNode> nodes = new List<BehaviorTreeNode>();
    private BehaviorTreeNode selectedNode;
    private ConnectingPort connectingPort; // 记录当前正在连接的端口信息
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

    private void OnEnable()
    {
        m_BehaviourTreeData = null;
        Debug.Log("BehaviorTreeEditor OnEnable!");
        Selection.selectionChanged += OnSelectionChange;
        connectingPort = new ConnectingPort();//清空连接端口选择
    }

    private void OnSelectionChange()
    {
        if (m_BehaviourTreeData != null)
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        path = Path.Combine(Application.dataPath.Replace("Assets", ""), path);
        Debug.Log("Select Path:" + path);
        LoadTree(path);
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
        DrawTips();

        if (GUI.changed) Repaint();
    }

    void DrawTips()
    {
        //绘制一个label提示m_BehaviourTreeData是否为空
        if (m_BehaviourTreeData == null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "请先创建或加载行为树文件");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 200, 20), "当前行为树文件: " + m_BehaviourTreeData.name);
        }
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

    private void DrawNodes()
    {
        Color normalColor = GUI.color;
        GUI.matrix = Matrix4x4.TRS(zoomOffset, Quaternion.identity, new Vector3(zoom, zoom, 1));
        for (int i = 0; i < nodes.Count; i++)
        {
            BehaviorTreeNode node = nodes[i];

            if (node == selectedNode)
            {
                // 选中效果
                GUI.color = Color.green;
            }
            else if (m_BehaviourTreeData != null && m_BehaviourTreeData.rootNode == node.node)
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.white;
            }

            // 绘制节点的背景
            GUI.Box(node.rect, node.title);

            // 绘制节点的内容
            GUI.Label(new Rect(node.rect.x, node.rect.y + 25, node.rect.width, 50), node.descript);

            // 绘制连接点
            float width = 32;
            float height = 16;
            Rect outRect = new Rect(node.rect.x + node.rect.width / 2 - width / 2, node.rect.y + node.rect.height, width, height);
            Rect inRect = new Rect(node.rect.x + node.rect.width / 2 - width / 2, node.rect.y - height, width, height);

            // 保存原按钮颜色
            Color originalButtonColor = GUI.backgroundColor;


            // 处理 out 端口
            if (connectingPort.node == node && connectingPort.portType == PortType.Out)
            {
                GUI.backgroundColor = Color.yellow;
            }

            if ((connectingPort.node != null && connectingPort.portType == PortType.Out && connectingPort.node != node && node.children.Count == 0)
                || connectingPort.node != null && connectingPort.portType == PortType.In && connectingPort.node == node
                || node.node is ActionNode)
            {
                //不绘制out端口,connectingPort有值并且是out端口并且连接的节点不是自己并且自己没有子节点
                //out端口绘制规则是,
                //当connectingPort为空时绘制,
                //connectingPort的PortType为in并且是其他节点的in端口,
                //connectingPort的PortType为out时并且out节点是自己
                //不绘制规则
                //connectingPort的PortType为out并且是其他节点并且其他节点有子节点的out端口
                //没有子节点的Action节点
            }
            else if (GUI.Button(outRect, ""))
            {
                if (connectingPort.node == null)
                {
                    connectingPort = new ConnectingPort { node = node, portType = PortType.Out };
                }
                else if (connectingPort.node == node && connectingPort.portType == PortType.Out)
                {
                    connectingPort = new ConnectingPort();//清空连接端口选择
                }
                else
                {
                    ConnectNodes(connectingPort.node, node, connectingPort.portType, PortType.Out);
                    connectingPort = new ConnectingPort();
                }
            }
            GUI.backgroundColor = originalButtonColor;

            // 处理 in 端口

            if (connectingPort.node == node && connectingPort.portType == PortType.In)
            {
                GUI.backgroundColor = Color.yellow;
            }

            if ((connectingPort.node != null && connectingPort.portType == PortType.In && connectingPort.node != node && node.hasParent)
                || connectingPort.node != null && connectingPort.portType == PortType.Out && connectingPort.node == node
                || node.node is RootNode)
            {
                //不绘制int端口,connectingPort有值并且是out端口并且连接的节点不是自己并且自己没有子节点
            }
            else if (GUI.Button(inRect, ""))
            {
                if (connectingPort.node == null)
                {
                    connectingPort = new ConnectingPort { node = node, portType = PortType.In };
                }
                else if (connectingPort.node == node && connectingPort.portType == PortType.In)
                {
                    connectingPort = new ConnectingPort();//清空连接端口选择
                }
                else
                {
                    ConnectNodes(connectingPort.node, node, connectingPort.portType, PortType.In);
                    connectingPort = new ConnectingPort();
                }
            }
            GUI.backgroundColor = originalButtonColor;

        }
        GUI.matrix = Matrix4x4.identity;
        GUI.color = normalColor;
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();
        Handles.matrix = Matrix4x4.TRS(zoomOffset, Quaternion.identity, new Vector3(zoom, zoom, 1));// 设置缩放和偏移矩阵
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            for (int j = 0; j < node.children.Count; j++)
            {
                var child = node.children[j];
                Vector3 start = new Vector3(node.rect.x + node.rect.width / 2, node.rect.y + node.rect.height, 0) - Vector3.down * 10;
                Vector3 end = new Vector3(child.rect.x + child.rect.width / 2, child.rect.y, 0);
                Handles.DrawBezier(start, end, start - Vector3.down * 50, end - Vector3.up * 50, Color.white, null, 3f / zoom);

                // 计算连线中间位置
                Vector3 midPoint = (start + end) / 2;
                float buttonSize = 20f;
                // 考虑缩放和偏移
                Rect buttonRect = new Rect(
                    (midPoint.x - buttonSize / 2) * zoom + zoomOffset.x,
                    (midPoint.y - buttonSize / 2) * zoom + zoomOffset.y,
                    buttonSize * zoom,
                    buttonSize * zoom);

                // 绘制断开连线按钮
                if (GUI.Button(buttonRect, "X"))
                {
                    DisconnectNodes(node, child);
                }
            }
        }
        Handles.matrix = Matrix4x4.identity;// 恢复矩阵
        Handles.EndGUI();
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

    private void DisconnectNodes(BehaviorTreeNode parent, BehaviorTreeNode child)
    {
        //view层
        parent.children.Remove(child);
        child.parent = null;

        //data层
        m_BehaviourTreeData.RemoveChild(parent.node, child.node);
        Repaint();
    }

    private void ConnectNodes(BehaviorTreeNode firstNode, BehaviorTreeNode secondNode, PortType firstPortType, PortType secondPortType)
    {
        BehaviorTreeNode parent, child;
        if ((firstPortType == PortType.Out && secondPortType == PortType.In) ||
            (firstPortType == PortType.In && secondPortType == PortType.Out))
        {
            if (firstPortType == PortType.Out)
            {
                parent = firstNode;
                child = secondNode;
            }
            else
            {
                parent = secondNode;
                child = firstNode;
            }

            if (child.parent != null)
            {
                child.parent.children.Remove(child);
            }
            if (parent.node is DecoratorNode dec && dec.childNode != null)
            {
                //view层
                var c = parent.children[0];
                DisconnectNodes(parent, c);
            }
            if (parent.node is RootNode root && root.childNode != null)
            {
                //view层
                var c = parent.children[0];
                DisconnectNodes(parent, c);
            }

            //data层
            m_BehaviourTreeData.AddChild(parent.node, child.node);

            //view层
            child.parent = parent;
            parent.children.Add(child);


            Repaint();
        }
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
                            ProcessContextMenu(selectedNode, e);
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
                    if (m_BehaviourTreeData != null)//只有创建了行为树文件才能创建节点
                    {
                        GenericMenu genericMenu = new GenericMenu();
                        AddNodeGenericMenu(genericMenu, e, null);
                        genericMenu.ShowAsContext();
                    }
                    else
                    {
                        GenericMenu genericMenu = new GenericMenu();
                        genericMenu.AddItem(new GUIContent("创建行为树"), false, () => CreatedTree());
                        genericMenu.AddItem(new GUIContent("加载行为树"), false, () => LoadTree());
                        genericMenu.AddItem(new GUIContent("加载最后一次行为树"), false, () => LoadLastTree());
                        genericMenu.ShowAsContext();
                    }

                    e.Use();
                }
                break;
            case EventType.KeyUp:
                if (e.control && e.keyCode == KeyCode.S)
                {
                    e.Use();
                    SaveTree();
                }
                break;
        }
    }

    private void ProcessContextMenu(BehaviorTreeNode node, Event e)
    {
        GenericMenu genericMenu = new GenericMenu();
        //genericMenu.AddItem(new GUIContent("设置为根节点"), false, () => OnClickSetRootNode(node));
        if (node.node is RootNode root)//根节点不能删除
        {

        }
        else
        {
            genericMenu.AddItem(new GUIContent("删除节点"), false, () => OnClickDeleteChildNode(node));
        }

        //genericMenu.AddItem(new GUIContent("创建新子节点"), false, () => OnClickCreateChildNode(node));
        AddNodeGenericMenu(genericMenu, e, node);
        genericMenu.ShowAsContext();
    }

    void AddNodeGenericMenu(GenericMenu genericMenu, Event e, BehaviorTreeNode parentNode)
    {
        //genericMenu.AddItem(new GUIContent("根节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, typeof(RootNode), null));

        var actionTypes = TypeCache.GetTypesDerivedFrom<ActionNode>();
        foreach (var actionType in actionTypes)
        {
            genericMenu.AddItem(new GUIContent($"ActionNode/创建{actionType.Name}节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, actionType, parentNode));
        }

        var compositeTypes = TypeCache.GetTypesDerivedFrom<CompositeNode>();
        foreach (var compositeType in compositeTypes)
        {
            genericMenu.AddItem(new GUIContent($"CompositeNode/创建{compositeType.Name}节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, compositeType, parentNode));
        }

        var decoratorTypes = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
        foreach (var decoratorType in decoratorTypes)
        {
            genericMenu.AddItem(new GUIContent($"DecoratorNode/创建{decoratorType.Name}节点"), false, () => OnClickCreateNode((e.mousePosition - zoomOffset) / zoom, decoratorType, parentNode));
        }
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        foreach (BehaviorTreeNode node in nodes)
        {
            node.node.rect.position += delta / zoom;
        }

        GUI.changed = true;
    }

    private void DragNode(BehaviorTreeNode node, Vector2 delta)
    {
        if (node.parent != null)
        {
            var parentNode = node.parent;
            var posY = node.rect.position.y + delta.y;
            if (posY < parentNode.rect.position.y + parentNode.rect.height)//子节点再父节点上面的情况
            {
                delta = new Vector2(delta.x, 0);
                node.node.rect.position = new Vector2(node.rect.position.x, parentNode.rect.position.y + parentNode.rect.height * 1.8f);//重新设置节点位置
            }
        }

        node.node.rect.position += delta;
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
            node.children[i].node.rect.position += delta;
        }
    }


    private void OnClickCreateNode(Vector2 position, Type type, BehaviorTreeNode parentNode)
    {
        position.x = Mathf.Clamp(position.x, 0, this.position.width / zoom);
        position.y = Mathf.Clamp(position.y, 0, this.position.height / zoom);
        var node = m_BehaviourTreeData.CreateNode(type);

        var viewNode = new BehaviorTreeNode(type.Name + "节点", "节点描述", node, position);
        nodes.Add(viewNode);
        if (parentNode != null)
        {
            //view层
            //有些单个子节点的,新增子节点时,需要断开原有的子节点
            if (parentNode.node is DecoratorNode dec && dec.childNode != null)
            {
                //断开原有的子节点
                var child = parentNode.children[0];
                DisconnectNodes(parentNode, child);
            }
            parentNode.children.Add(viewNode);
            viewNode.parent = parentNode;

            //data层
            m_BehaviourTreeData.AddChild(parentNode.node, node);
        }
        Repaint();
    }


    private void OnClickSetRootNode(BehaviorTreeNode node)
    {
        m_BehaviourTreeData.rootNode = node.node;
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

        // 从 m_BehaviourTreeData 中移除节点
        m_BehaviourTreeData.DeleteNode(nodeToDelete.node);


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
        Selection.objects = null;
        Selection.activeObject = node.node;
    }

    private void CreatedTree()
    {
        // 先保存?

        nodes.Clear();

        string path = EditorUtility.SaveFilePanel("保存行为树", "Assets/Scripts/BehaviourTree/data/", "BehaviorTree.asset", "asset");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        CreatedTree(path);
    }

    private void CreatedTree(string path)
    {
        // 先保存?
        nodes.Clear();


        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        // 创建 ScriptableObject 实例
        BehaviorTree bt = ScriptableObject.CreateInstance<BehaviorTree>();
        m_BehaviourTreeData = bt;
        // 保存为资源文件
        // 将绝对路径转换为相对于 Assets 文件夹的路径
        path = FileUtil.GetProjectRelativePath(path);

        AssetDatabase.CreateAsset(bt, path);
        AssetDatabase.SaveAssets();
        // 刷新资产数据库，使新创建的资产在编辑器中可见
        AssetDatabase.Refresh();
        Selection.activeObject = bt;


        CreatRootNode();
    }

    void CreatRootNode()
    {
        if (m_BehaviourTreeData == null)
        {
            return;
        }
        Type type = typeof(RootNode);
        var node = m_BehaviourTreeData.CreateNode(type);

        //默认位置中间上面
        var position = new Vector2(this.position.width / 2, this.position.height / 4);
        var viewNode = new BehaviorTreeNode(type.Name + "节点", "节点描述", node, position);
        nodes.Add(viewNode);
    }

    private void SaveTree()
    {
        // 标记对象为已修改
        EditorUtility.SetDirty(m_BehaviourTreeData);

        // 保存资产数据库的更改
        AssetDatabase.SaveAssets();

        // 刷新资产数据库，使更改在编辑器中可见
        AssetDatabase.Refresh();

        ShowNotification(new GUIContent("保存成功!"));
    }

    private void LoadTree()
    {
        string path = EditorUtility.OpenFilePanel("加载行为树", "Assets/Scripts/BehaviourTree/data/", "asset");
        LoadTree(path);
        Repaint();
    }

    private void LoadTree(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            nodes.Clear();
            ClearSelection();



            string RelativePath = FileUtil.GetProjectRelativePath(path);
            m_BehaviourTreeData = AssetDatabase.LoadAssetAtPath<BehaviorTree>(RelativePath);
            if (m_BehaviourTreeData == null)
            {
                //ShowNotification(new GUIContent($"加载不了路径{path}的行为树文件"));
                return;
            }

            PlayerPrefs.SetString("LastTreePath", path); // 保存最后一次加载的路径

            // 创建节点映射，方便查找节点
            Dictionary<Node, BehaviorTreeNode> nodeMap = new Dictionary<Node, BehaviorTreeNode>();

            // 首先将所有节点添加到 nodes 列表和节点映射中
            for (int i = 0; i < m_BehaviourTreeData.nodes.Count; i++)
            {
                var node = m_BehaviourTreeData.nodes[i];
                var viewNode = new BehaviorTreeNode(node);
                nodes.Add(viewNode);
                nodeMap[node] = viewNode;
            }

            // 然后恢复节点之间的父子关系
            foreach (var node in m_BehaviourTreeData.nodes)
            {
                if (node is CompositeNode compositeNode)
                {
                    var compositeViewNode = nodeMap[compositeNode];
                    foreach (var child in compositeNode.GetChildrenNode())
                    {
                        if (nodeMap.TryGetValue(child, out var childViewNode))
                        {
                            compositeViewNode.children.Add(childViewNode);
                            childViewNode.parent = compositeViewNode;
                        }
                    }
                }
                else if (node is DecoratorNode decoratorNode)
                {
                    if (decoratorNode.childNode != null && nodeMap.TryGetValue(decoratorNode.childNode, out var childViewNode))
                    {
                        var decoratorViewNode = nodeMap[decoratorNode];
                        decoratorViewNode.children.Add(childViewNode);
                        childViewNode.parent = decoratorViewNode;
                    }
                }
                else if (node is RootNode rootNode)
                {
                    if (rootNode.childNode != null && nodeMap.TryGetValue(rootNode.childNode, out var childViewNode))
                    {
                        var rootViewNode = nodeMap[rootNode];
                        rootViewNode.children.Add(childViewNode);
                        childViewNode.parent = rootViewNode;
                    }
                }
            }
            ShowNotification(new GUIContent($"加载{m_BehaviourTreeData.name}成功!"));
            Repaint();
        }
    }


}