using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class SetStateIdle : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            controller.ChangeState(CharacterState.Idle);
            return State.Success;
        }
    }
}
