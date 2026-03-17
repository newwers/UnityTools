using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    public class RestState : StateAgent
    {

        public override void Init(IStateMachineOwner owner, StateManager stateManager)
        {
            base.Init(owner, stateManager);
        }

        public override void OnStateEnter(AStateBase beforState)
        {
            base.OnStateEnter(beforState);
        }

        public override void OnStateExit(AStateBase nextState)
        {
            base.OnStateExit(nextState);
        }

    }
}
