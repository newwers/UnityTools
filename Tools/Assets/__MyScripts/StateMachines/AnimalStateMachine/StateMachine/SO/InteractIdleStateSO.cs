﻿﻿using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "InteractIdleState", menuName = "StateMachine/InteractIdleState", order = 6)]
    public class InteractIdleStateSO : StateSO
    {
        [Header("Idle Time Settings")]
        [Range(1f, 60f)]
        public float minIdleTime = 30f;

        [Range(1f, 120f)]
        public float maxIdleTime = 60f;

        [Header("Animation Settings")]
        public string animationParameterName = "Interact_Idle";

        private void OnEnable()
        {
            stateType = StateType.Interact_Idle;
        }
    }
}