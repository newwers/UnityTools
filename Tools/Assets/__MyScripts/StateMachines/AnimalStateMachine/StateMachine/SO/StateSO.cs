using UnityEngine;

namespace StateMachineSystem
{
    public abstract class StateSO : ScriptableObject
    {
        public StateType stateType;

        [Header("State Settings")]
        [Range(0.1f, 5f)]
        public float transitionCheckInterval = 5f;
    }
}
