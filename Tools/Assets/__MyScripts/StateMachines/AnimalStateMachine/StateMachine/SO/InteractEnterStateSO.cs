﻿﻿using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "InteractEnterState", menuName = "StateMachine/InteractEnterState", order = 5)]
    public class InteractEnterStateSO : StateSO
    {
        [Header("Animation Settings")]
        public string animationParameterName = "Interact_Enter";

        private void OnEnable()
        {
            stateType = StateType.Interact_Enter;
        }
    }
}