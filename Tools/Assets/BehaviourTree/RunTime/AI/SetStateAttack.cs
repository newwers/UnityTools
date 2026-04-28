using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class SetStateAttack : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            controller.ChangeState(CharacterState.Attacking);
            return State.Success;
        }
    }
}
