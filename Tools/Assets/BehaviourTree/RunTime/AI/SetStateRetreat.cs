using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class SetStateRetreat : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            controller.ChangeState(CharacterState.Retreat);
            return State.Success;
        }
    }
}
