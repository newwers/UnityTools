using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    /// <summary>
    /// 需要休息状态其实也是一种使用状态,所以继承UseState,只是显示上有所区别
    /// </summary>
    public class NeedRestState : UseState
    {
        public override void Init(IStateMachineOwner owner, StateManager stateManager)
        {
            //m_owner = owner;
            //m_stateManager = stateManager;
            //m_GameManager = owner as GameManager;
            base.Init(owner, stateManager);
            //不能调用 base.Init 否则会执行一次OnUse函数
        }

        public override void OnStateEnter(AStateBase beforState)
        {
            //base.OnStateEnter(beforState);
            var useState = beforState as UseState;
            if (useState == null)
            {
                return;
            }

            //m_GameManager.ShowText.color = Color.red;
        }

        public override void OnStateExit(AStateBase nextState)
        {
            base.OnStateExit(nextState);
        }


        public override void OnStatePerSecondUpdate()
        {
            if (!m_GameManager)
            {
                return;
            }

            m_GameManager.ShowText.text = Tool.FormatSeconds(m_GameManager.m_UseTime);
            m_GameManager.m_UseTime++;

            m_GameManager.m_CheckUseTime++;
            if (m_GameManager.m_CheckUseTime > 2* m_GameManager.RestTime)//大于两倍休息时间时,严重提醒
            {
                m_GameManager.OnNeedRest(m_GameManager.m_CheckUseTime);
            }
            
            if (m_GameManager.m_CheckUseTime >= m_GameManager.CheckUseTime && !m_GameManager.m_bPerformanceMode)//长时间没用电脑 并且不是性能模式时才进入休息状态
            {
                m_GameManager.OnRest();
                m_GameManager.m_UseTime = 0;
                m_GameManager.m_CheckUseTime = 0;
            }
        }
    }
}
