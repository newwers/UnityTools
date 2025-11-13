using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
#endif


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


    //[Header("独立攻击帧(同一攻击多次计算)")]
    //public bool IndependentAttack;
    //// 独立攻击帧检测频率
    //public float independentAttackInterval = 0.1f;

    [Header("攻击框设置")]
    public HitboxType hitboxType = HitboxType.Rectangle;
    public Vector2 hitboxOffset = new Vector2(0.5f, 0f);
    public Vector2 hitboxSize = new Vector2(1f, 0.5f);

    [Header("特殊形状参数")]
    public float hitboxRadius = 0.5f;
    public float hitboxAngle = 90f;
    public Vector2 hitboxEndPoint = new Vector2(1f, 0f);

    [Header("伤害设置")]
    public int damage = 10;
    public float knockbackForce = 5f;

    [Header("眩晕设置")]
    public bool causeStun = false; // 是否造成眩晕
    public float stunDuration = 1.0f; // 眩晕持续时间

    [Header("特效")]
    public GameObject hitEffect;
    public AudioClip hitSound;
}



/// <summary>
/// 行为数据
/// </summary>
[CreateAssetMenu(fileName = "New Action Data", menuName = "Character System/aciton Data")]
public class ActionData : ScriptableObject
{
    // 动画参数类型枚举
    public enum AnimationParameterType
    {
        Trigger,
        Bool,
        Int,
        Float
    }

    // 计算属性
    public float TotalDuration => windUpTime + activeTime + recoveryTime;
    public float ComboStartTime => windUpTime + activeTime + recoveryTime - comboWindow;


    [Header("基础设置")]
    public string acitonName;
    public int priority = 0; // 动作优先级

    #region 动画相关



    [Header("动画参数配置")]
    public AnimationClip animationClip;
    public float frameRate = 30f;
    public AnimationParameterType animationParameterType = AnimationParameterType.Trigger;
    public string animationParameterName = "Attack";

    // 根据参数类型设置不同的值
    public int animationIntValue = 1;
    public float animationFloatValue = 1.0f;
    public bool animationBoolValue = true;



    [Header("时间设置（秒）")]
    [Range(0, 2f)] public float windUpTime = 0.1f;     // 前摇时间
    [Range(0, 2f)] public float activeTime = 0.2f;     // 攻击中时间
    [Range(0, 2f)] public float recoveryTime = 0.3f;   // 后摇时间
    [Range(0, 1f)] public float comboWindow = 0.1f;    // 连招输入窗口


    [Header("帧数设置（动画实际各个阶段帧数）")]
    public int ActualWindUpFrames = 3;
    public int ActualActiveFrames = 6;
    public int ActualRecoveryFrames = 9;
    [Header("配置各个阶段对应自动计算的帧数")]
    [FieldReadOnly] public int windUpFrames = 3;
    [FieldReadOnly] public int activeFrames = 6;
    [FieldReadOnly] public int recoveryFrames = 9;

    #endregion
    #region 攻击相关



    // 特殊形状参数
    [Header("特殊形状参数")]
    public float hitboxRadius = 0.5f;          // 圆形半径/胶囊半径/扇形半径
    public float hitboxAngle = 90f;             // 扇形角度
    public Vector2 hitboxEndPoint = new Vector2(1f, 0f); // 线段终点

    [Header("攻击位移设置")]
    public bool enableMovement = false;
    public float movementSpeed = 3f;
    public AnimationCurve movementCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("连招设置")]
    public bool canCombo = true;
    public ActionData[] nextAttacks; // 可连接的下一段攻击
    public bool resetComboOnMiss = false; // 打空是否重置连招


    public bool canCancel = true; // 是否可被取消

    [Header("多帧攻击框配置")]
    public List<AttackFrameData> frameData = new List<AttackFrameData>();


    [Header("帧检测设置")]

    // 自动计算检测间隔：基于帧率
    public float DetectionInterval => 1f / frameRate;
    #endregion

    [Header("特殊效果")]
    public GameObject hitEffect;
    public AudioClip attackSound;


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
                    hitboxRadius = hitboxRadius,
                    hitboxAngle = hitboxAngle,
                    hitboxEndPoint = hitboxEndPoint,
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
                frame.hitboxRadius = hitboxRadius;
                frame.hitboxAngle = hitboxAngle;
                frame.hitboxEndPoint = hitboxEndPoint;
                frame.hitEffect = hitEffect;
            }
        }
    }


    // 自动计算帧数
    public void OnValidate()
    {
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
}

