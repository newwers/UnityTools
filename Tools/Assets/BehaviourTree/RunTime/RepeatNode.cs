namespace Z.BehaviourTree
{

    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnStop()
        {
            throw new System.NotImplementedException();
        }

        protected override State OnUpdate()
        {
            return childNode.Update();
        }
    }
}
