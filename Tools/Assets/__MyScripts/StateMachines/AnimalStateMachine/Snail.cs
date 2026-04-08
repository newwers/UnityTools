using StateMachineSystem;
using UnityEngine;

public class Snail : AnimalBase
{
    public float vibrateDuration = 0.2f;
    public float vibrateIntensity = 0.05f;
    public float fallSpeed = 2f;
    private Coroutine m_VibrateAnimalCoroutine;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Coin coin = collision.GetComponent<Coin>();
        if (coin != null)
        {
            coin.OnPickCoins();
        }

        var mushRoom = collision.GetComponent<MushRoom>();
        if (mushRoom != null)
        {
            mushRoom.OnAnimalTriggerClickMound();//触发点击土包
        }
    }

    public override void OnMouse_Down()
    {
        base.OnMouse_Down();
        if (StateMachine != null)
        {
            switch (StateMachine.currentStateType)
            {
                case StateType.Interact_Enter:
                    // 如果已经在 Interact_Enter 状态，则跳过
                    break;
                case StateType.Interact_Idle:
                    // 如果已经在 Interact_Idle 状态，则震动动物
                    VibrateAnimal();
                    break;
                case StateType.Interact_Exit:
                    // 如果已经在 Interact_Exit 状态，则忽略
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
        return StateMachine != null && StateMachine.currentStateType == StateType.Interact_Idle;
    }

    public override void OnDragEnd()
    {
        if (StateMachine != null)
        {
            // 立即开始下坠协程
            StartCoroutine(FallToGroundCoroutine());
        }
    }

    private System.Collections.IEnumerator FallToGroundCoroutine()
    {
        // 检测地面位置
        int groundLayer = LayerMask.NameToLayer("MainGround");
        if (groundLayer == -1)
        {
            // 如果找不到MainGround层，直接进入状态
            StateMachine.SetState(StateType.Interact_Exit);
            yield break;
        }

        //bool isGrounded = false;
        //isGrounded = CheckGroundCollision();



        LayerMask groundLayerMask = 1 << groundLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayerMask);
        if (hit.collider != null)
        {
            // 平滑下坠到地面
            float targetY = hit.point.y /*+ 0.01f*/; // 稍微高于地面
            while (transform.position.y > targetY)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - fallSpeed * Time.deltaTime, transform.position.z);
                LogManager.Log("空中");
                yield return null;
            }
            // 确保精确位置
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }
        LogManager.Log("地面");
        // 进入Interact_Exit状态
        //StateMachine.SetState(StateType.Interact_Exit);//由Interact_Idle状态的GetNextState决定进入Interact_Exit状态

    }

    public void VibrateAnimal()
    {
        if (m_VibrateAnimalCoroutine == null)
        {
            m_VibrateAnimalCoroutine = StartCoroutine(VibrateCoroutine(vibrateDuration, vibrateIntensity));
        }
    }

    private System.Collections.IEnumerator VibrateCoroutine(float duration, float intensity)
    {
        Vector3 originalPosition = StateMachine.spriteRenderer.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 计算震动偏移量
            float offsetY = Random.Range(0, intensity);
            Vector3 newPosition = originalPosition + new Vector3(0, offsetY, 0f);

            // 应用新位置
            StateMachine.spriteRenderer.transform.position = newPosition;

            // 等待一小段时间
            yield return new WaitForSeconds(0.05f);

            elapsedTime += 0.05f;
        }

        // 恢复原始位置
        StateMachine.spriteRenderer.transform.position = originalPosition;

        m_VibrateAnimalCoroutine = null;
    }
}
