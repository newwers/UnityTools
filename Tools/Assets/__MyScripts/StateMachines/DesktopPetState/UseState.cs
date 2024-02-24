using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    public class UseState : StateAgent
    {
        public override void Init(IStateMachineOwner owner, StateManager stateManager)
        {
            base.Init(owner, stateManager);
        }

        public override void OnStateEnter(AStateBase beforState)
        {
            base.OnStateEnter(beforState);
            if (m_GameManager == null)
            {
                return;
            }

            m_GameManager.m_UseTime = 0;
            m_GameManager.m_CheckUseTime = 0;
            m_GameManager.OnUse();
        }

        public override void OnStateExit(AStateBase nextState)
        {
            base.OnStateExit(nextState);
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }

        public override void OnStatePerSecondUpdate()
        {
            base.OnStatePerSecondUpdate();

            if (!m_GameManager)
            {
                return;
            }

            m_GameManager.ShowText.text = Tool.FormatSeconds(m_GameManager.m_UseTime);
            m_GameManager.m_UseTime++;

            if (m_GameManager.m_UseTime >= m_GameManager.RestTime)//休息时间到
            {
                m_GameManager.OnNeedRest();

            }

            m_GameManager.m_CheckUseTime++;
            if (m_GameManager.m_CheckUseTime >= m_GameManager.CheckUseTime && !m_GameManager.m_bPerformanceMode)//长时间没用电脑 并且不是性能模式时才进入休息状态
            {
                m_GameManager.OnRest();
                m_GameManager.m_UseTime = 0;
                m_GameManager.m_CheckUseTime = 0;
            }
        }
    }
}
