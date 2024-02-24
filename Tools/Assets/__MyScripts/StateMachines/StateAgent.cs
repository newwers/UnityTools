using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    /// <summary>
    /// 状态中通用部分提取到这个,状态基础自该类
    /// </summary>
    public class StateAgent : AStateBase
    {
        protected IStateMachineOwner m_owner;
        protected StateManager m_stateManager;
        protected GameManager m_GameManager;

        public override void Init(IStateMachineOwner owner, StateManager stateManager)
        {
            m_owner = owner;
            m_stateManager = stateManager;
            m_GameManager = owner as GameManager;
        }

        public override void OnStateEnter(AStateBase beforState)
        {

        }

        public override void OnStateExit(AStateBase nextState)
        {

        }

        public override void OnStateUpdate()
        {

        }

        public override void OnStatePerSecondUpdate()
        {

        }
    }
}
