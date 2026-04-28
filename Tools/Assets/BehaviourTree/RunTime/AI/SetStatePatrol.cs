using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class SetStatePatrol : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;
            controller.ChangeState(CharacterState.Patrol);
            return State.Success;
        }
    }
}
