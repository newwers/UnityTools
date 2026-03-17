using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "WalkState", menuName = "StateMachine/WalkState", order = 4)]
    public class WalkStateSO : StateSO
    {
        [Header("Transition Probabilities")]
        [Range(0, 100)]
        public int walkToIdleProbability = 50;

        [Header("Movement Settings")]
        [Range(0.1f, 5f)]
        public float moveSpeed = 1f;

        [Range(1f, 10f)]
        public float moveRangeX = 5f;

        [Range(0.01f, 1f)]
        public float targetReachDistance = 0.1f;

        private void OnEnable()
        {
            stateType = StateType.Walk;
        }
    }
}
