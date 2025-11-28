/*
 action.started += OnStarted;//按下
 action.performed += OnPerformedCallback;//满足上面设定的条件后执行,默认没设置条件时,按下就执行,如果设置为Hold,则按下不会执行,需要满足Hold时间后才执行
 action.canceled += OnCanceledCallback;//抬起

Tap - 按下并快速抬起才会触发performed,触发后不触发canceled,
Hold - 按下并持续一段时间后才会触发performed
press -??
 
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TestInput : MonoBehaviour
{
    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference inputAction;

    [Header("Timing Settings")]
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float doubleClickThreshold = 0.3f;

    [Header("Events")]
    public UnityEvent OnPressed;
    public UnityEvent OnPerformed;
    public UnityEvent OnCanceled;
    public UnityEvent OnClick;
    public UnityEvent OnDoubleClick;
    public UnityEvent OnHoldStart;
    public UnityEvent OnHoldPerformed;
    public UnityEvent OnHoldEnd;

    // 状态变量
    private float pressTime;
    private bool isPressing;
    private bool isHolding;
    private float lastClickTime;
    private int clickCount;

    // 属性用于外部访问状态
    public bool IsPressing => isPressing;
    public bool IsHolding => isHolding;
    public float CurrentPressDuration => isPressing ? Time.time - pressTime : 0f;

    private void OnEnable()
    {
        if (inputAction != null)
        {
            inputAction.action.Enable();
            inputAction.action.started += OnStarted;//按下
            inputAction.action.performed += OnPerformedCallback;//满足上面设定的条件后执行,默认没设置条件时,按下就执行
            inputAction.action.canceled += OnCanceledCallback;//抬起
        }
    }

    private void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.action.started -= OnStarted;
            inputAction.action.performed -= OnPerformedCallback;
            inputAction.action.canceled -= OnCanceledCallback;
            inputAction.action.Disable();
        }

        // 重置状态
        ResetState();
    }

    //private void Update()
    //{
    //    HandleHoldDetection();
    //    HandleDoubleClickReset();
    //}

    private void OnStarted(InputAction.CallbackContext context)
    {
        LogManager.Log_Green($"按键按下: {context.startTime},ispress:{inputAction.action.IsPressed()}");

        isPressing = true;
        pressTime = Time.time;
        isHolding = false;

        OnPressed?.Invoke();
    }

    private void OnPerformedCallback(InputAction.CallbackContext context)
    {
        LogManager.Log_Green($"按键执行: {context.time},ispress:{inputAction.action.IsPressed()}");
        OnPerformed?.Invoke();
    }

    private void OnCanceledCallback(InputAction.CallbackContext context)
    {
        LogManager.Log_Green($"按键取消: {context.time},ispress:{inputAction.action.IsPressed()}");

        float pressDuration = Time.time - pressTime;
        LogManager.Log_Green($"按键持续时间: {pressDuration:F2}秒");

        // 判断点击类型
        //if (pressDuration < holdDuration)
        //{
        //    HandleClick();
        //}
        //else
        //{
        //    // 长按结束
        //    if (isHolding)
        //    {
        //        OnHoldEnd?.Invoke();
        //        LogManager.Log_Green("长按结束");
        //    }
        //}

        OnCanceled?.Invoke();
        ResetPressState();
    }

    private void HandleHoldDetection()
    {
        if (isPressing && !isHolding)
        {
            float currentDuration = Time.time - pressTime;

            if (currentDuration >= holdDuration)
            {
                isHolding = true;
                OnHoldStart?.Invoke();
                OnHoldPerformed?.Invoke();
                LogManager.Log_Green($"长按触发，持续时间: {currentDuration:F2}秒");
            }
        }

        // 持续长按回调
        if (isHolding)
        {
            // 这里可以添加持续长按的逻辑
            // 例如：OnHolding?.Invoke(CurrentPressDuration);
        }
    }

    private void HandleClick()
    {
        clickCount++;

        // 双击检测
        if (Time.time - lastClickTime <= doubleClickThreshold)
        {
            if (clickCount >= 2)
            {
                OnDoubleClick?.Invoke();
                LogManager.Log_Green("双击触发");
                clickCount = 0;
            }
        }
        else
        {
            // 单点击
            OnClick?.Invoke();
            LogManager.Log_Green("单击触发");
            clickCount = 1;
        }

        lastClickTime = Time.time;
    }

    private void HandleDoubleClickReset()
    {
        // 重置双击计数（如果超过阈值时间）
        if (clickCount > 0 && Time.time - lastClickTime > doubleClickThreshold)
        {
            clickCount = 0;
        }
    }

    private void ResetPressState()
    {
        isPressing = false;
        isHolding = false;
        pressTime = 0f;
    }

    private void ResetState()
    {
        ResetPressState();
        clickCount = 0;
        lastClickTime = 0f;
    }

    // 公共方法用于手动触发事件（用于测试）
    [ContextMenu("模拟按下")]
    public void SimulatePress()
    {
        OnStarted(new InputAction.CallbackContext());
    }

    [ContextMenu("模拟执行")]
    public void SimulatePerform()
    {
        OnPerformedCallback(new InputAction.CallbackContext());
    }

    [ContextMenu("模拟取消")]
    public void SimulateCancel()
    {
        OnCanceledCallback(new InputAction.CallbackContext());
    }

    // 调试信息
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("输入测试状态:");
        GUILayout.Label($"按下状态: {isPressing}");
        GUILayout.Label($"长按状态: {isHolding}");
        GUILayout.Label($"按下时长: {CurrentPressDuration:F2}秒");
        GUILayout.Label($"点击计数: {clickCount}");

        if (isPressing)
        {
            GUILayout.Label($"长按进度: {Mathf.Clamp01(CurrentPressDuration / holdDuration):P0}");
        }

        GUILayout.EndArea();
    }
}