namespace StateMachineSystem
{
    public abstract class StateBase
    {
        protected StateMachine stateMachine;
        protected StateSO stateConfig;

        public abstract StateType StateType { get; }

        public virtual void Initialize(StateMachine machine, StateSO config)
        {
            stateMachine = machine;
            stateConfig = config;
        }

        public virtual void Enter()
        {
            //Debug.Log($"Entering state: {StateType}");
        }

        public virtual void Exit()
        {
            //Debug.Log($"Exiting state: {StateType}");
        }

        public abstract StateType GetNextState();

        public virtual void Update()
        {
            // 状态更新逻辑
        }
#if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected()
        {

        }
#endif
        public StateSO GetStateConfig()
        {
            return stateConfig;
        }
    }
}
