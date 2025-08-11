using System;
using UnityEngine;

/// <summary>
/// 动画播放完毕后自动隐藏游戏对象
/// </summary>
public class AutoHideAfterAnimation : MonoBehaviour
{
    [Header("配置选项")]
    [Tooltip("动画播放完毕后是否禁用对象")]
    public bool disableObject = true;

    [Tooltip("动画播放完毕后是否销毁对象")]
    public bool destroyObject = false;

    [Tooltip("等待动画播放的时间（秒），设为0则自动从Animator获取动画长度")]
    public float waitTime = 0f;

    [Tooltip("是否在启用时自动开始计时")]
    public bool autoStart = true;

    public event Action<GameObject> OnAnimatorEndEvent;

    private Animator animator;
    private float animationLength = 0f;
    private bool isPlaying = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (autoStart)
        {
            StartAnimationTimer();
        }
    }

    /// <summary>
    /// 开始动画计时器
    /// </summary>
    public void StartAnimationTimer()
    {
        // 如果未指定等待时间，则尝试从Animator获取动画长度
        if (waitTime <= 0 && animator != null)
        {
            animationLength = GetCurrentAnimationLength();
        }
        else
        {
            animationLength = waitTime;
        }

        isPlaying = true;
        CancelInvoke(nameof(HideObject));
        Invoke(nameof(HideObject), animationLength);
    }

    /// <summary>
    /// 获取当前Animator播放的动画长度
    /// </summary>
    private float GetCurrentAnimationLength()
    {
        if (animator == null) return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    /// <summary>
    /// 隐藏对象的方法
    /// </summary>
    private void HideObject()
    {
        if (disableObject)
        {
            gameObject.SetActive(false);
        }

        if (destroyObject)
        {
            Destroy(gameObject);
        }

        isPlaying = false;
        OnAnimatorEndEvent?.Invoke(gameObject);
    }

    /// <summary>
    /// 停止计时器
    /// </summary>
    public void StopTimer()
    {
        isPlaying = false;
        CancelInvoke(nameof(HideObject));
    }

    /// <summary>
    /// 检查动画是否正在播放
    /// </summary>
    public bool IsAnimationPlaying()
    {
        return isPlaying;
    }
}