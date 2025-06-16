using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("长按设置")]
    [SerializeField] private float pressDuration = 1.0f; // 长按触发时间（秒）

    [Header("视觉反馈")]
    [SerializeField] private Image progressFill; // 进度条填充图像
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color pressedColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;

    [Header("事件")]
    public UnityEvent onLongPress; // 长按完成时触发的事件

    private bool isPressing = false;
    private float pressStartTime;
    private Coroutine pressCoroutine;

    void Start()
    {
        // 初始化进度显示
        if (progressFill != null)
        {
            progressFill.fillAmount = 0;
            progressFill.color = normalColor;
        }
    }

    void Update()
    {
        UpdateProgressVisual();
    }

    // 鼠标/触摸按下
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPressing)
        {
            isPressing = true;
            pressStartTime = Time.time;
            pressCoroutine = StartCoroutine(PressRoutine());

            if (progressFill != null)
            {
                progressFill.color = pressedColor;
            }
        }
    }

    // 鼠标/触摸释放
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressing)
        {
            ResetPress();
        }
    }

    // 指针离开按钮区域
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPressing)
        {
            ResetPress();
        }
    }

    // 长按协程
    private IEnumerator PressRoutine()
    {
        while (isPressing)
        {
            float elapsed = Time.time - pressStartTime;
            if (elapsed >= pressDuration)
            {
                // 长按完成，触发事件
                onLongPress?.Invoke();

                // 触发完成视觉效果
                if (progressFill != null)
                {
                    progressFill.color = completedColor;
                    yield return new WaitForSeconds(0.2f);
                }

                ResetPress();
                break;
            }
            yield return null;
        }
    }

    // 更新进度显示
    private void UpdateProgressVisual()
    {
        if (isPressing && progressFill != null)
        {
            float progress = Mathf.Clamp01((Time.time - pressStartTime) / pressDuration);
            progressFill.fillAmount = progress;
        }
    }

    // 重置按钮状态
    private void ResetPress()
    {
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }

        isPressing = false;

        if (progressFill != null)
        {
            // 短暂显示完成效果后的重置
            if (progressFill.color == completedColor)
            {
                StartCoroutine(ResetVisualAfterDelay(0.2f));
            }
            else
            {
                ResetVisual();
            }
        }
    }

    // 延迟重置视觉效果
    private IEnumerator ResetVisualAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetVisual();
    }

    // 重置视觉效果
    private void ResetVisual()
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = 0;
            progressFill.color = normalColor;
        }
    }
}