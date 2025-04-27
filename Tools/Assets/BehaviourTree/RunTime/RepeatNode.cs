namespace Z.BehaviourTree
{

    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart()
        {

        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            childNode.Update();
            return State.Running;
        }


    }
}
