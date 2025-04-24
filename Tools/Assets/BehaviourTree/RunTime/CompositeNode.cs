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
        protected List<Node> childNodes = new List<Node>();
    }
}
