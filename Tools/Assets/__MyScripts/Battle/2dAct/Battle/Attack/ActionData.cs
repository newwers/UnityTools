using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 行为数据
/// </summary>
[CreateAssetMenu(fileName = "New Action Data", menuName = "Character System/Aciton Data")]
public class ActionData : ScriptableObject
{
    [System.Serializable]
    public class AnimationParameter
    {
        public string parameterName;
        public AnimationParameterType type;

        // 根据参数类型设置不同的值
        public int animationIntValue = 1;
        public float animationFloatValue = 1.0f;
        public bool animationBoolValue = true;
    }

    // 动画参数类型枚举
    public enum AnimationParameterType
    {
        Trigger,
        Bool,
        Int,
        Float
    }


    [Header("基础设置")]
    public string acitonName;
    public int priority = 0; // 动作优先级

    #region 动画相关



    [Header("动画参数配置")]
    public GameObject character; //绑定角色
    public AnimationClip animationClip;
    public float frameRate = 30f;
    public float animationClipLength
    {
        get
        {
            return animationClip ? animationClip.length : 0f;
        }
    }
    /// <summary>
    /// 状态对应动画参数
    /// 目前只有切换状态的时候触发一次,对于持续性例如移动动作,在update中负责动画调用
    /// 例如移动,空中速度,移动动画速度,地面检测
    /// </summary>
    public List<AnimationParameter> animationParameters = new List<AnimationParameter>();


    #endregion

    public bool canCancel = true; // 是否可被取消


    // 自动计算帧数
    public void OnValidate()
    {
        if (animationClip)
        {
            frameRate = animationClip.frameRate;
        }
    }
}

