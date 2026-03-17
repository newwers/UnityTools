using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachineSystem
{
    public class StateMachine : MonoBehaviour
    {
        [Header("State Configs")]
        public IdleStateSO lieStateConfig;
        public WalkStateSO walkStateConfig;

        [Header("Animator")]
        public Animator animator;

        [Header("Sprite Renderer")]
        public SpriteRenderer spriteRenderer;

        [Header("Current State")]
        public StateType currentStateType = StateType.Idle;

        AnimalBase current;
        public AnimalBase Current
        {
            get { return current; }
            set { current = value; }
        }

        private Coroutine transitionTimer;

        private Dictionary<StateType, StateBase> states = new Dictionary<StateType, StateBase>();
        private StateBase currentState;

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

        public void SetState(StateType stateType)
        {
            if (states.TryGetValue(stateType, out StateBase newState))
            {
                if (currentState != null)
                {
                    currentState.Exit();
                }

                currentState = newState;
                currentStateType = stateType;
                currentState.Enter();

                Debug.Log($"State changed to: {stateType}");

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
                animator.SetBool("Lie", false);
                animator.SetBool("LieToStand", false);
                animator.SetBool("Stand", false);
                animator.SetBool("StandToLie", false);
                animator.SetBool("Walk", false);
                animator.SetBool("PickUp", false);
            }
        }

        // 获取SpriteRenderer组件
        public SpriteRenderer GetSpriteRenderer()
        {
            return spriteRenderer;
        }
    }
}
