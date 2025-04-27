namespace Z.BehaviourTree
{


    public class SequenceNode : CompositeNode
    {
        int currentIndex = 0;
        protected override void OnStart()
        {
            currentIndex = 0;
        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            var children = childNodes[currentIndex];
            switch (children.Update())
            {
                case State.Running:
                    return State.Running;
                case State.Failure:
                    return State.Failure;
                case State.Success:
                    currentIndex++;
                    break;
                default:
                    break;
            }




            return currentIndex == childNodes.Count ? State.Success : State.Running;//顺序节点状态判断
        }
    }
}
