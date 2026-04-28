using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class CheckHasTarget : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            return controller.CurrentTarget != null ? State.Success : State.Failure;
        }
    }
}
