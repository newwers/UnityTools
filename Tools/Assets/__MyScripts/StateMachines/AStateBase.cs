using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StateMachines
{
    /// <summary>
    /// 持有状态机的对象实现该接口
    /// </summary>
    public interface IStateMachineOwner
    {

    }

    /// <summary>
    /// 定义状态机接口的抽象类
    /// </summary>
    public abstract class AStateBase 
    {
        /// <summary>
        /// 状态初始化
        /// </summary>
        /// <param name="owner">用这个参数获取所需要的对象</param>
        /// <param name="stateManager"></param>
        public abstract void Init(IStateMachineOwner owner,StateManager stateManager);

        public abstract void OnStateEnter(AStateBase beforState);
        public abstract void OnStateUpdate();

        public abstract void OnStatePerSecondUpdate();
        public abstract void OnStateExit(AStateBase nextState);

    }
}
