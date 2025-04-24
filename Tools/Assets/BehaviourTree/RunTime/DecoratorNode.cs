namespace Z.BehaviourTree
{

    /// <summary>
    /// 装饰节点,子节点单个时使用装饰节点来修饰子节点的行为。
    /// </summary>
    public abstract class DecoratorNode : Node
    {
        public Node childNode;
    }
}