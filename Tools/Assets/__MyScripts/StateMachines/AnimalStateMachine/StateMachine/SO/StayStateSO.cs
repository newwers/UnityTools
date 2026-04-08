using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "StayState", menuName = "StateMachine/StayState", order = 3)]
    public class StayStateSO : StateSO
    {
        [Header("Stay Settings")]
        [Range(1f, 10f)]
        public float minStayTime = 3f;
        [Range(1f, 120f)]
        public float maxStayTime = 8f;

        [Header("Animation")]
        public string animationParameterName = "Stay";

        private void OnEnable()
        {
            stateType = StateType.Stay;
        }
    }
}