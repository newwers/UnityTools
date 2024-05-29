using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    /// <summary>
    /// ��Ҫ��Ϣ״̬��ʵҲ��һ��ʹ��״̬,���Լ̳�UseState,ֻ����ʾ����������
    /// </summary>
    public class NeedRestState : UseState
    {
        public override void Init(IStateMachineOwner owner, StateManager stateManager)
        {
            //m_owner = owner;
            //m_stateManager = stateManager;
            //m_GameManager = owner as GameManager;
            base.Init(owner, stateManager);
            //���ܵ��� base.Init �����ִ��һ��OnUse����
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
            if (m_GameManager.m_CheckUseTime > 2* m_GameManager.RestTime)//����������Ϣʱ��ʱ,��������
            {
                m_GameManager.OnNeedRest(m_GameManager.m_CheckUseTime);
            }
            
            if (m_GameManager.m_CheckUseTime >= m_GameManager.CheckUseTime && !m_GameManager.m_bPerformanceMode)//��ʱ��û�õ��� ���Ҳ�������ģʽʱ�Ž�����Ϣ״̬
            {
                m_GameManager.OnRest();
                m_GameManager.m_UseTime = 0;
                m_GameManager.m_CheckUseTime = 0;
            }
        }
    }
}
