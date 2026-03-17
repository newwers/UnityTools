using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "IdleState", menuName = "StateMachine/IdleState", order = 2)]
    public class IdleStateSO : StateSO
    {
        [Header("Transition Probabilities")]
        [Range(0, 100)]
        public int idleToWalkProbability = 50;

        private void OnEnable()
        {
            stateType = StateType.Idle;
        }
    }
}
