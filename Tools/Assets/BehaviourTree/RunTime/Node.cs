using UnityEngine;

namespace Z.BehaviourTree
{
    /// <summary>
    /// 行为树节点基类
    /// </summary>
    public abstract class Node : ScriptableObject
    {
        public enum State
        {
            Running,
            Failure,
            Success,
        }
        public State state = State.Running;
        /// <summary>
        /// 是否执行OnStart函数
        /// </summary>
        public bool started = false;

        public State Update()
        {
            if (started == false)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();
            if (state == State.Failure || state == State.Success)
            {
                OnStop();
                started = false;
            }

            return state;
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();
    }

}