﻿﻿using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "InteractExitState", menuName = "StateMachine/InteractExitState", order = 7)]
    public class InteractExitStateSO : StateSO
    {
        [Header("Transition Probabilities")]
        [Range(0, 100)]
        public int exitToIdleProbability = 50;

        [Header("Animation Settings")]
        public string animationParameterName = "Interact_Exit";

        private void OnEnable()
        {
            stateType = StateType.Interact_Exit;
        }
    }
}