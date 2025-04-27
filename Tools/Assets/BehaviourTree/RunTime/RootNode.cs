namespace Z.BehaviourTree
{


    public class RootNode : Node
    {
        public Node childNode;
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return childNode.Update();
        }

        public override Node Clone()
        {
            RootNode node = Instantiate(this);
            node.childNode = childNode.Clone();
            return node;
        }
    }
}