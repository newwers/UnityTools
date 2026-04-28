using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class CheckInAttackRange : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null || controller.CurrentTarget == null) return State.Failure;
            
            float distance = UnityEngine.Vector3.Distance(
                controller.transform.position, 
                controller.CurrentTarget.position);
            
            return distance <= controller.GetCurrentAttackRange() ? State.Success : State.Failure;
        }
    }
}