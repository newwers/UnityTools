using UnityEngine;

namespace StateMachineSystem
{
    public abstract class StateSO : ScriptableObject
    {
        public StateType stateType;

        [Header("State Settings")]
        public float transitionCheckInterval = 5f;
    }
}
