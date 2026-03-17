using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    public class StateManager 
    {
        AStateBase m_aState;
        EStateEnum m_CurStateEnum = EStateEnum.None;

        Dictionary<EStateEnum ,AStateBase> m_vStateCache;
        IStateMachineOwner m_Ower;

        public void Init(IStateMachineOwner owner)
        {
            m_vStateCache = new Dictionary<EStateEnum, AStateBase>();
            m_Ower = owner;
        }


        public void Update()
        {
            if (m_aState != null)
            {
                m_aState.OnStateUpdate();
            }
        }

        public void OnStatePerSecondUpdate()
        {
            if (m_aState!= null)
            {
                m_aState.OnStatePerSecondUpdate();
            }
        }

        void ChangeState(AStateBase curState,AStateBase nextState)
        {
            if (curState != null)
            {
                curState.OnStateExit(nextState);
            }

            if (nextState != null)
            {
                nextState.OnStateEnter(curState);
            }

            m_aState = nextState;
        }

        public void ChangeState(EStateEnum stateEnum)
        {
            if (m_CurStateEnum == stateEnum)//过滤相同状态?
            {
                return;
            }
            switch (stateEnum)
            {
                case EStateEnum.None:
                    m_CurStateEnum = EStateEnum.None;
                    ChangeState(m_aState, GetStete(EStateEnum.None));
                    break;
                case EStateEnum.Use:
                    m_CurStateEnum = EStateEnum.Use;
                    ChangeState(m_aState, GetStete(EStateEnum.Use));//这边每次切换都需要new吗?
                    break;
                case EStateEnum.NeedRest:
                    m_CurStateEnum = EStateEnum.NeedRest;
                    ChangeState(m_aState, GetStete(EStateEnum.NeedRest));
                    break;
                case EStateEnum.Rest:
                    m_CurStateEnum = EStateEnum.Rest;
                    ChangeState(m_aState, GetStete(EStateEnum.Rest));
                    break;
                default:
                    break;
            }
        }

        public void Exit()
        {
            if (m_aState != null)
            {
                m_aState.OnStateExit(null);
            }
            m_aState = null;
        }

        /// <summary>
        /// 获取状态对象
        /// 每次新增状态这边要加对应获取方式
        /// </summary>
        /// <param name="stateEnum"></param>
        /// <returns></returns>
        AStateBase GetStete(EStateEnum stateEnum)
        {
            AStateBase state = null;

            if (m_vStateCache.TryGetValue(stateEnum, out state))
            {
                return state;
            }


            switch (stateEnum)
            {
                case EStateEnum.None:

                    return null;
                case EStateEnum.Use:

                    state = new UseState();
                    state.Init(m_Ower,this);

                    m_vStateCache.Add(stateEnum,state);
                    return state;
                case EStateEnum.NeedRest:

                    state = new NeedRestState();
                    state.Init(m_Ower, this);

                    m_vStateCache.Add(stateEnum, state);
                    return state;
                case EStateEnum.Rest:

                    state = new RestState();
                    state.Init(m_Ower, this);

                    m_vStateCache.Add(stateEnum, state);
                    return state;
                default:
                    return null;
            }
        }
    }
}
