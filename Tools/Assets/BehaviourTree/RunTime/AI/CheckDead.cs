using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class CheckDead : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            return controller.IsDead ? State.Success : State.Failure;
        }
    }
}
