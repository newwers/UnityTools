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
        [FieldReadOnly]
        public State state = State.Running;
        [FieldReadOnly]
        /// <summary>
        /// 是否执行OnStart函数
        /// </summary>
        public bool started = false;
        [FieldReadOnly]
        public string guid;
#if UNITY_EDITOR
        public string title;
        public string descript;
        public Vector2 position
        {
            get
            {
                return rect.position;
            }
        }
        public Rect rect;
#endif

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

        /// <summary>
        /// 在同一个行为树被共用的情况下,避免互相引用
        /// </summary>
        /// <returns></returns>
        public virtual Node Clone()
        {
            return Instantiate(this);
        }
    }

}