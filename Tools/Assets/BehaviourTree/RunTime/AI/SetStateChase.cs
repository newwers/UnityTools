using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class SetStateChase : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            controller.ChangeState(CharacterState.Chase);
            return State.Success;
        }
    }
}
