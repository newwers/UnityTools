/*
平时走参考蜗牛,点击开始飞参考蝴蝶(模拟昆虫随机飞,可以控制飞行轨迹(上下有点高低起伏)),
飞的时候点击下落,不点击的时候,飞一段时间也降落,降落需要看看能否实现降落到蘑菇或者摆件上
*/
using StateMachineSystem;
using UnityEngine;

public class Ladybug : AnimalBase
{
    public float vibrateDuration = 0.2f;
    public float vibrateIntensity = 0.05f;
    private FlyEndStateSO m_flyEndStateSO;

    public bool IsGround { get; private set; }



    private void Start()
    {
        if (StateMachine)
        {
            var exitState = StateMachine.GetState(StateType.Interact_Exit);
            if (exitState != null)
            {
                var config = exitState.GetStateConfig();
                if (config)
                {
                    m_flyEndStateSO = config as FlyEndStateSO;
                }
            }
        }

        // 获取MainGround层的索引
        m_GroundLayer = LayerMask.NameToLayer("MainGround");

    }




    void Update()
    {
        // 每帧检查地面碰撞状态
        bool previousIsGround = IsGround;
        IsGround = CheckGroundCollision();

        // 当状态变化时输出日志
        //if (IsGround != previousIsGround)
        //{
        //    LogManager.Log($"Ladybug IsGround状态变化: {IsGround}");
        //}

        // 获取地面高度并修正位置
        if (WoodBox.HasInstance && WoodBox.Instance.MainGround != null)
        {
            float groundHeight = WoodBox.Instance.MainGround.GetGroundHeight();
            if (transform.position.y < groundHeight)
            {
                transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);
            }
        }
    }


    public override bool CheckGroundCollision()
    {
        //base.CheckGroundCollision();
        float collisionCheckRadius = 0.01f;
        if (m_flyEndStateSO)
        {
            collisionCheckRadius = m_flyEndStateSO.collisionCheckRadius;
        }
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + new Vector3(0, collisionCheckRadius / 2, 0), collisionCheckRadius);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == m_GroundLayer)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        var mushRoom = collision.GetComponent<MushRoom>();
        if (mushRoom != null)
        {
            mushRoom.OnAnimalTriggerClickMound();//触发点击土包
            mushRoom.OnAnimalTriggerClickMushroom();//触发点击蘑菇
        }


        if (StateMachine != null)
        {
            if (IsDragging)
            {
                if (StateMachine.currentStateType == StateType.Stay)
                {
                    // 获取实例
                    var state = StateMachine.GetState(StateType.Stay);
                    if (state != null)
                    {
                        var stayState = state as StayState;
                        if (stayState != null)
                        {
                            stayState.OnDraggingTriggerEnter2D(collision);
                        }
                    }
                }
            }
            else
            {
                if (StateMachine.currentStateType == StateType.Interact_Idle)
                {
                    // 获取LadybugFlyState实例
                    var state = StateMachine.GetState(StateType.Interact_Idle);
                    if (state != null)
                    {
                        LadybugFlyState ladybugFlyState = state as LadybugFlyState;
                        if (ladybugFlyState != null)
                        {
                            ladybugFlyState.OnTriggerEnter2D(collision);
                        }
                    }
                }
            }

        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (StateMachine != null)
        {
            if (IsDragging)
            {
                if (StateMachine.currentStateType == StateType.Stay)
                {
                    // 获取实例
                    var state = StateMachine.GetState(StateType.Stay);
                    if (state != null)
                    {
                        var stayState = state as StayState;
                        if (stayState != null)
                        {
                            stayState.OnDraggingTriggerExit2D(collision);
                        }
                    }
                }
            }
            else
            {
                if (StateMachine.currentStateType == StateType.Interact_Idle)
                {
                    // 获取LadybugFlyState实例
                    var state = StateMachine.GetState(StateType.Interact_Idle);
                    if (state != null)
                    {
                        LadybugFlyState ladybugFlyState = state as LadybugFlyState;
                        if (ladybugFlyState != null)
                        {
                            ladybugFlyState.OnTriggerExit2D(collision);
                        }
                    }
                }
            }

        }
    }

    public override void OnMouse_Down()
    {
        if (StateMachine != null)
        {
            switch (StateMachine.currentStateType)
            {
                case StateType.Interact_Enter:
                    // 如果已经在 Interact_Enter 状态，则跳过
                    break;
                case StateType.Interact_Idle:
                    // 飞行状态中点击进入下落状态
                    StateMachine.SetState(StateType.Interact_Exit);
                    break;
                case StateType.Interact_Exit:
                    // 如果已经在 Interact_Exit 状态，则忽略
                    break;
                case StateType.Stay:
                    // 停留状态下，50%概率进入飞行状态，50%概率直接掉落
                    int randomValue = Random.Range(0, 100);
                    if (randomValue < 50)
                    {
                        // 进入飞行状态
                        StateMachine.SetState(StateType.Interact_Enter);
                    }
                    else
                    {
                        // 直接掉落，进入下落状态
                        StateMachine.SetState(StateType.Interact_Exit);
                    }
                    break;
                default:
                    // 其他状态，切换到 Interact_Enter 状态
                    StateMachine.SetState(StateType.Interact_Enter);
                    break;
            }
        }
    }

    public override bool CanDrag()
    {
        return StateMachine != null && StateMachine.currentStateType == StateType.Stay;
    }

    public override void OnDragEnd()
    {
        if (StateMachine != null)
        {
            // 如果不在物体内部，切换到Interact_Idle状态
            if (!IsInObject)
            {
                StateMachine.SetState(StateType.Interact_Idle);
            }
            // 如果在物体内部，保持原有状态
        }
    }


    private void OnDrawGizmosSelected()
    {
        float collisionCheckRadius = 0.01f;
        if (m_flyEndStateSO)
        {
            collisionCheckRadius = m_flyEndStateSO.collisionCheckRadius;
        }

        //绘制出射线，方便调试
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, collisionCheckRadius / 2, 0), collisionCheckRadius);
    }
}
