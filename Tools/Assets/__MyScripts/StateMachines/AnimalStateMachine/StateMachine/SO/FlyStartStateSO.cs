using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "FlyStartState", menuName = "StateMachine/FlyStartState", order = 6)]
    public class FlyStartStateSO : InteractEnterStateSO
    {
        [Header("Fly Start Settings")]
        public float maxHeight = 1.0f;
        public float animationDuration = 1.0f;
        public float forwardDistance = 2.0f;
        
        private void OnEnable()
        {
            stateType = StateType.Interact_Enter;
        }
    }
}