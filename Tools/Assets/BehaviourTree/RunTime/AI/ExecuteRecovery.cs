using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class ExecuteRecovery : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;

            if (!controller.CanUseRecoverySkill())
                return State.Failure;

            controller.PerformRecoverySkill();
            return State.Success;
        }
    }
}
