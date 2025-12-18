using System;
using System.Collections.Generic;
using UnityEngine;

// 添加触发类型枚举
public enum AttackTriggerType
{
    Tap,        // 点击触发
    LongPress,       // 长按触发,按住直到 holdTimeThreshold 达到后发起攻击（在 Update 中检测 AttackHoldTime）。
    Hold      // 按住时持续触发技能，松开后结束
}

// 攻击框形状枚举
public enum HitboxType
{
    /// <summary>
    /// 矩形（默认）
    /// </summary>
    Rectangle,  // 
    /// <summary>
    /// 圆形
    /// </summary>
    Circle,     // 
    /// <summary>
    /// 胶囊形
    /// </summary>
    Capsule,    // 
    /// <summary>
    /// 扇形
    /// </summary>
    Sector,     // 
    /// <summary>
    /// 线段
    /// </summary>
    Line        // 
}



/// <summary>
/// 攻击帧数据
/// </summary>
[System.Serializable]
public class AttackFrameData
{
    [Header("帧设置")]
    public int frameIndex; // 帧索引（从0开始）
    public bool isAttackFrame = true; // 是否为攻击帧

    [Header("受击层设置")]
    public LayerMask hitLayers;


    [Header("命中行为")]
    [Tooltip("启用后此帧会在同一次攻击中独立结算伤害，不受已有命中记录限制")]
    public bool allowIndependentHit = false;

    [Tooltip("此攻击帧最多命中的目标数量，0表示无限制")]
    public int maxHitTargets = 0;

    [Header("攻击框设置")]
    public HitboxType hitboxType = HitboxType.Rectangle;
    public Vector2 hitboxOffset = new Vector2(0.5f, 0f);
    public Vector2 hitboxSize = new Vector2(1f, 0.5f);

    [Header("特殊形状参数")]
    public float hitboxRadius = 0.5f;
    public float hitboxAngle = 90f;
    public Vector2 hitboxEndPoint = new Vector2(1f, 0f);

    [Header("伤害设置")]
    [Tooltip("附加伤害值，最终伤害 = 技能基础伤害(baseDamage) + 此附加伤害")]
    public int damage = 0;

    [Header("击退力")]
    [Tooltip("附加击退力，最终击退力 = 技能基础击退力 + 此附加击退力")]
    public Vector2 knockbackForce = new Vector2(0, 0);

    [Header("效果列表")]
    [Tooltip("此攻击帧施加的效果列表，可以包含眩晕、减速等各种Buff/Debuff")]
    public List<EffectData> effects = new List<EffectData>();

    [Header("特效")]
    public GameObject hitEffect;
    public AudioClip hitSound;
}



/// <summary>
/// 攻击行为数据（继承自基础行为数据）
/// </summary>
[CreateAssetMenu(fileName = "New Attack Action Data", menuName = "Character System/Character Action/Attack Action Data")]
public class AttackActionData : ActionData
{
    [Header("技能数据")]
    public SkillData skillData;

    [Header("攻击触发方式")]
    public AttackTriggerType triggerType = AttackTriggerType.Tap; // 触发方式
    [Range(0, 2f)] public float longPressTimeThreshold = 0.3f; // 长按时间阈值

    [Header("hold攻击设置")]
    public float holdTickInterval = 0.1f; // 持续触发的间隔时间
    public int maxHoldTicks = 10; // 最大触发次数限制

    [Header("攻击时间设置（秒）")]
    [Range(0, 2f)] public float windUpTime = 0.1f;     // 前摇时间
    [Range(0, 2f)] public float activeTime = 0.2f;     // 攻击中时间
    [Range(0, 2f)] public float recoveryTime = 0.3f;   // 后摇时间
    [Range(0, 1f)] public float comboWindow = 0.1f;    // 连招输入窗口

    [Header("攻击帧数设置（动画实际各个阶段帧数）")]
    public int ActualWindUpFrames = 3;
    public int ActualActiveFrames = 6;
    public int ActualRecoveryFrames = 9;

    [Header("配置各个阶段对应自动计算的帧数")]
    [FieldReadOnly] public int windUpFrames = 3;
    [FieldReadOnly] public int activeFrames = 6;
    [FieldReadOnly] public int recoveryFrames = 9;

    [Header("攻击位移设置")]
    public bool enableMovement = false;
    [Header("是否累加力")]
    public bool IsAccumulateForce = false;
    public Vector2 movementSpeed = new Vector2(10, 0);
    public AnimationCurve movementCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("连招设置")]
    public bool canCombo = true;
    public bool resetComboOnMiss = false; // 打空是否重置连招

    [Header("多帧攻击框配置")]
    public List<AttackFrameData> frameData = new List<AttackFrameData>();

    [Header("帧检测设置")]
    // 自动计算检测间隔：基于帧率
    public float DetectionInterval => 1f / frameRate;

    [Header("攻击特殊效果")]
    public GameObject hitEffect;
    public AudioClip attackSound;

    // 计算属性
    public float TotalDuration => windUpTime + activeTime + recoveryTime;
    public float ComboStartTime => windUpTime + activeTime + recoveryTime - comboWindow;

    // 获取指定帧的攻击框数据
    public AttackFrameData GetFrameData(int frameIndex)
    {
        return frameData.Find(f => f.frameIndex == frameIndex);
    }

    // 获取所有攻击帧
    public List<AttackFrameData> GetAttackFrames()
    {
        return frameData.FindAll(f => f.isAttackFrame);
    }

    // 获取攻击中阶段的帧范围
    public (int startFrame, int endFrame) GetActivePhaseFrames()
    {
        int startFrame = ActualWindUpFrames;
        int endFrame = startFrame + ActualActiveFrames;
        return (startFrame, endFrame);
    }

    // 自动生成帧数据（基于动画时长和帧率）
    public void GenerateFrameDataFromAnimation()
    {
        if (animationClip == null) return;

        frameData.Clear();
        int totalFrames = Mathf.RoundToInt(animationClip.length * animationClip.frameRate);

        // 设置正确的帧率
        if (frameRate != animationClip.frameRate)
        {
            frameRate = animationClip.frameRate;
        }

        for (int i = 0; i < totalFrames; i++)
        {
            bool isAttackFrame = (i >= ActualWindUpFrames && i < ActualWindUpFrames + ActualActiveFrames);
            if (isAttackFrame)
            {
                frameData.Add(new AttackFrameData
                {
                    frameIndex = i,
                    isAttackFrame = isAttackFrame,
                    hitEffect = hitEffect
                });
            }
        }

        LogManager.Log($"[AttackData] 为动画 {animationClip.name} 生成 {totalFrames} 帧数据，攻击帧: {ActualActiveFrames}");
    }

    // 自动同步全局设置到所有帧
    public void SyncGlobalSettingsToFrames()
    {
        foreach (var frame in frameData)
        {
            if (frame.isAttackFrame)
            {
                frame.hitEffect = hitEffect;
            }
        }
    }

    // 自动计算帧数
    public new void OnValidate()
    {
        base.OnValidate(); // 调用基类的验证

        if (animationClip)
        {
            frameRate = animationClip.frameRate;
        }

        windUpFrames = Mathf.RoundToInt(windUpTime * frameRate);
        activeFrames = Mathf.RoundToInt(activeTime * frameRate);
        recoveryFrames = Mathf.RoundToInt(recoveryTime * frameRate);

        // 验证时长配置是否合理
        if (windUpTime <= 0) Debug.LogWarning($"{acitonName}: 前摇时长必须大于0");
        if (activeTime <= 0) Debug.LogWarning($"{acitonName}: 攻击中时长必须大于0");
        if (recoveryTime <= 0) Debug.LogWarning($"{acitonName}: 后摇时长必须大于0");
    }

    public float GetCooldown()
    {
        if (skillData == null)
            return 0f;

        return skillData.Cooldown;
    }

    public float GetEnergyCost()
    {
        if (skillData == null)
            return 0f;

        return skillData.EnergyCost;
    }

    public bool HasSkillData()
    {
        return skillData != null;
    }
}