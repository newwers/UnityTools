using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class CheckStunned : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            return controller.IsStunned ? State.Success : State.Failure;
        }
    }
}