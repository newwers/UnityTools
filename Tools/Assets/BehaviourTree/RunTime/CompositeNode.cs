using System.Collections.Generic;

namespace Z.BehaviourTree
{
    /// <summary>
    /// 复合节点,子节点有多个时使用复合节点来管理子节点的执行顺序和逻辑。
    /// </summary>
    /// <remarks>
    /// 复合节点是行为树的核心组成部分，负责管理子节点的执行顺序和逻辑。
    /// </remarks>
    public abstract class CompositeNode : Node
    {
        public List<Node> childNodes = new List<Node>();

        public void AddChildrenNode(Node node)
        {
            childNodes.Add(node);
        }

        public void RemoveChildrenNode(Node node)
        {
            childNodes.Remove(node);
        }

        public void ClearChildrenNode()
        {
            childNodes.Clear();
        }

        public List<Node> GetChildrenNode()
        {
            return childNodes;
        }

        public override Node Clone()
        {
            CompositeNode node = Instantiate(this);
            node.childNodes = childNodes.ConvertAll(x => x.Clone());//克隆数组,而不是引用
            return node;
        }
    }
}
