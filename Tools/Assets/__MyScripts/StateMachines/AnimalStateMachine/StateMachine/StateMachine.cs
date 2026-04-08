using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineSystem
{
    public class StateMachine : MonoBehaviour
    {
        [Header("State Configs")]
        public BlackboardSO blackboard;
        public IdleStateSO lieStateConfig;
        public WalkStateSO walkStateConfig;
        public StayStateSO stayStateConfig;
        public InteractEnterStateSO interactEnterStateConfig;
        public InteractIdleStateSO interactIdleStateConfig;
        public InteractExitStateSO interactExitStateConfig;

        [Header("Animator")]
        public Animator animator;

        [Header("Sprite Renderer")]
        public SpriteRenderer spriteRenderer;

        [Header("Current State")]
        public StateType currentStateType = StateType.Idle;

        [Header("Previous State")]
        public StateType previousStateType = StateType.Idle;

        AnimalBase current;
        public AnimalBase Current
        {
            get { return current; }
            set { current = value; }
        }

        public StateType PreviousStateType
        {
            get { return previousStateType; }
        }

        public StateBase PreviousState
        {
            get { return previousState; }
        }



        private Coroutine transitionTimer;

        private Dictionary<StateType, StateBase> states = new Dictionary<StateType, StateBase>();
        private StateBase currentState;
        private StateBase previousState;

        private void Awake()
        {
            current = GetComponent<AnimalBase>();


            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            InitializeStates();
        }

        private void Start()
        {
            SetState(currentStateType);
            StartTransitionTimer();
        }

        private void InitializeStates()
        {
            IdleState lieState = new IdleState();
            lieState.Initialize(this, lieStateConfig);
            states[StateType.Idle] = lieState;


            WalkState walkState = new WalkState();
            walkState.Initialize(this, walkStateConfig);
            states[StateType.Walk] = walkState;

            StayState stayState = new StayState();
            stayState.Initialize(this, stayStateConfig);
            states[StateType.Stay] = stayState;

            // 初始化蝴蝶状态
            if (current is Butterfly)
            {
                ButterflyWalkState butterflyWalkState = new ButterflyWalkState();
                butterflyWalkState.Initialize(this, walkStateConfig);
                states[StateType.Walk] = butterflyWalkState;

                ButterflyStayState butterflyStayState = new ButterflyStayState();
                butterflyStayState.Initialize(this, stayStateConfig);
                states[StateType.Stay] = butterflyStayState;
            }

            // 根据配置文件类型动态创建状态类
            StateBase interactEnterState;
            if (interactEnterStateConfig is FlyStartStateSO)
            {
                interactEnterState = new FlyStartState();
            }
            else
            {
                interactEnterState = new InteractEnterState();
            }
            interactEnterState.Initialize(this, interactEnterStateConfig);
            states[StateType.Interact_Enter] = interactEnterState;

            StateBase interactIdleState;
            if (interactIdleStateConfig is LadybugFlyStateSO)
            {
                interactIdleState = new LadybugFlyState();
            }
            else
            {
                interactIdleState = new InteractIdleState();
            }
            interactIdleState.Initialize(this, interactIdleStateConfig);
            states[StateType.Interact_Idle] = interactIdleState;

            StateBase interactExitState;
            if (interactExitStateConfig is FlyEndStateSO)
            {
                interactExitState = new FlyEndState();
            }
            else
            {
                interactExitState = new InteractExitState();
            }
            interactExitState.Initialize(this, interactExitStateConfig);
            states[StateType.Interact_Exit] = interactExitState;

        }

        private void OnEnable()
        {
            if (transitionTimer == null)
            {
                StartTransitionTimer();
            }
        }

        private void OnDisable()
        {
            StopTransitionTimer();
        }

        private void StartTransitionTimer()
        {
            float interval = GetCurrentStateCheckInterval();
            transitionTimer = StartCoroutine(TransitionCheckCoroutine(interval));
        }

        private float GetCurrentStateCheckInterval()
        {
            switch (currentStateType)
            {
                case StateType.Idle:
                    return lieStateConfig.transitionCheckInterval;

                case StateType.Walk:
                    return walkStateConfig.transitionCheckInterval;

                case StateType.Stay:
                    return stayStateConfig.transitionCheckInterval;

                case StateType.Interact_Enter:
                    return interactEnterStateConfig.transitionCheckInterval; // 动画状态检查间隔较短

                case StateType.Interact_Idle:
                    return interactIdleStateConfig.transitionCheckInterval; //  idle 状态检查间隔适中

                case StateType.Interact_Exit:
                    return interactExitStateConfig.transitionCheckInterval; // 动画状态检查间隔较短

                default:
                    return 5f;
            }
        }

        private void StopTransitionTimer()
        {
            if (transitionTimer != null)
            {
                StopCoroutine(transitionTimer);
                transitionTimer = null;
            }
        }

        private IEnumerator TransitionCheckCoroutine(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                CheckStateTransition();
            }
        }

        private void CheckStateTransition()
        {
            if (currentState != null)
            {
                StateType newStateType = currentState.GetNextState();
                if (newStateType != currentStateType)
                {
                    SetState(newStateType);
                }
                else
                {
                    // 状态未改变，更新定时器间隔
                    StopTransitionTimer();
                    StartTransitionTimer();
                }
            }
        }

        public StateBase GetState(StateType stateType)
        {
            if (states.TryGetValue(stateType, out StateBase state))
            {
                return state;
            }
            return null;
        }

        public void SetState(StateType stateType)
        {
            if (states.TryGetValue(stateType, out StateBase newState))
            {
                if (currentState != null)
                {
                    currentState.Exit();
                }

                // 保存当前状态为上一个状态
                previousState = currentState;
                previousStateType = currentStateType;

                currentState = newState;
                currentStateType = stateType;
                currentState.Enter();

                //Debug.Log($"State changed to: {stateType}");

                // 状态改变后更新定时器间隔
                StopTransitionTimer();
                StartTransitionTimer();
            }
            else
            {
                Debug.LogError($"State not found: {stateType}");
            }
        }

        private void Update()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (currentState != null)
            {
                currentState.OnDrawGizmosSelected();
            }
        }
#endif
        // 动画控制方法
        public void SetAnimatorBool(string parameterName, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(parameterName, value);
            }
        }

        public void ResetAllAnimatorBools()
        {
            if (animator != null)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Walk", false);
                animator.SetBool("Interact_Enter", false);
                animator.SetBool("Interact_Idle", false);
                animator.SetBool("Interact_Exit", false);
                animator.SetBool("Stay", false);

                animator.SetFloat("AnimatorSpeed", 1);
                //LogManager.Log("重置所有动画参数:" + name);
            }
        }

        // 获取SpriteRenderer组件
        public SpriteRenderer GetSpriteRenderer()
        {
            return spriteRenderer;
        }

        // 设置当前碰撞物体
        public void SetCurrentCollidedObject(GameObject obj)
        {
            if (blackboard != null)
            {
                blackboard.SetCurrentCollidedObject(obj);
            }
        }

        // 获取当前碰撞物体
        public GameObject GetCurrentCollidedObject()
        {
            if (blackboard != null)
            {
                return blackboard.GetCurrentCollidedObject();
            }
            return null;
        }
    }
}
