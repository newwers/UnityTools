using UnityEngine;

public class IdleStateChangeStateByTime : IdleStateChangeStateByAnimation
{
    [Header("休闲状态持续时间")]
    public Vector2 idleDuration = new Vector2(6, 20);

    private float idleTimer = 0f;

    public override void Initialize(Character character)
    {
        base.Initialize(character);
    }

    public override void Enter()
    {
        base.Enter();

        idleTimer = Time.time + Random.Range(idleDuration.x, idleDuration.y);

        LogManager.Log($"进入休闲状态,时长:" + (idleTimer - Time.time));
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }
    /// <summary>
    /// 切换状态条件
    /// </summary>
    /// <returns></returns>
    public override bool IsConditionMet()
    {
        //状态切换条件,根据每隔待机动画不同,有的是一段时间切换,有的是播放几次动画完后切换
        return Time.time >= idleTimer;
    }
}
