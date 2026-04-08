using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "FlyEndState", menuName = "StateMachine/FlyEndState", order = 8)]
    public class FlyEndStateSO : InteractExitStateSO
    {
        [Header("Fall Settings")]
        public float fallSpeed = 5.0f;
        public float collisionCheckRadius = 0.3f;
        public float collisionCheckDelayTime = 0.2f;

        private void OnEnable()
        {
            stateType = StateType.Interact_Exit;
        }
    }
}