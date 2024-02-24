using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    /// <summary>
    /// ����״̬���Ķ���ʵ�ָýӿ�
    /// </summary>
    public interface IStateMachineOwner
    {

    }

    /// <summary>
    /// ����״̬���ӿڵĳ�����
    /// </summary>
    public abstract class AStateBase 
    {
        /// <summary>
        /// ״̬��ʼ��
        /// </summary>
        /// <param name="owner">�����������ȡ����Ҫ�Ķ���</param>
        /// <param name="stateManager"></param>
        public abstract void Init(IStateMachineOwner owner,StateManager stateManager);

        public abstract void OnStateEnter(AStateBase beforState);
        public abstract void OnStateUpdate();

        public abstract void OnStatePerSecondUpdate();
        public abstract void OnStateExit(AStateBase nextState);

    }
}
