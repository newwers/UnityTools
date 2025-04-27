using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
namespace Z.BehaviourTree
{
    /// <summary>
    /// 创建行为树文件脚本
    /// </summary>
    [CreateAssetMenu(fileName = "行为树文件", menuName = "行为树/创建行为树文件")]
    public class BehaviorTree : ScriptableObject
    {
        public Node rootNode;//行为树根节点
        public Node.State state = Node.State.Running;
        public List<Node> nodes = new List<Node>();//存放这个行为树中所有的节点(里面可能存在一些多余没用到的节点)

        public Node.State Update()
        {
            if (rootNode == null)
            {
                Debug.LogError("行为树没有根节点");
                return Node.State.Failure;
            }
            if (state == Node.State.Running)
            {
                state = rootNode.Update();
            }

            return state;
        }
#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            if (nodes.Count == 0)//创建第一个节点默认根节点
            {
                rootNode = node;
            }
            nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);//将创建的scriptable资源添加到行为树的scriptable资源底下面
            AssetDatabase.SaveAssets();

            return node;

        }

        public void DeleteNode(Node node)
        {
            nodes.Remove(node);
            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            if (parent is CompositeNode compositeNode)
            {
                compositeNode.AddChildrenNode(child);
            }
            if (parent is DecoratorNode decoratorNode)
            {
                decoratorNode.SetChild(child);
            }
            if (parent is RootNode rootNode)
            {
                rootNode.childNode = child;
            }

        }

        public void RemoveChild(Node parent, Node child)
        {
            if (parent is CompositeNode compositeNode)
            {
                compositeNode.RemoveChildrenNode(child);
            }
            if (parent is DecoratorNode decoratorNode)
            {
                decoratorNode.SetChild(null);
            }
            if (parent is RootNode rootNode)
            {
                rootNode.childNode = null;
            }
        }

        public List<Node> GetChildren(Node parent)
        {
            if (parent is CompositeNode compositeNode)
            {
                return compositeNode.GetChildrenNode();
            }
            if (parent is DecoratorNode decoratorNode)
            {
                return new List<Node>() { decoratorNode.childNode };
            }
            if (parent is RootNode rootNode)
            {
                return new List<Node>() { rootNode.childNode };
            }
            return null;
        }

        public BehaviorTree Clone()
        {
            BehaviorTree behaviorTree = Instantiate(this);
            behaviorTree.rootNode = behaviorTree.rootNode.Clone();
            return behaviorTree;
        }
#endif
    }
}