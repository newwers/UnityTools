using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageMouseEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("鼠标事件")]
    public UnityEvent OnMouseEnter; // 鼠标进入事件
    public UnityEvent OnMouseExit;  // 鼠标离开事件

    [Header("视觉反馈")]
    public bool changeColorOnHover = false;
    public Color hoverColor = Color.grey;

    private Image targetImage;
    private Color originalColor;

    private void Awake()
    {
        targetImage = GetComponent<Image>();

        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }

    // 实现IPointerEnterHandler接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 触发鼠标进入事件
        OnMouseEnter?.Invoke();

        // 视觉反馈
        //if (targetImage != null && changeColorOnHover)
        //{
        //    targetImage.color = hoverColor;
        //}
    }

    // 实现IPointerExitHandler接口
    public void OnPointerExit(PointerEventData eventData)
    {
        // 触发鼠标离开事件
        OnMouseExit?.Invoke();

        // 恢复原始颜色
        if (targetImage != null && changeColorOnHover)
        {
            targetImage.color = originalColor;
        }
    }

    // 供外部调用的方法
    public void SetHoverColor(Color color)
    {
        hoverColor = color;
    }

    public void SetOriginalColor(Color color)
    {
        originalColor = color;

        if (targetImage != null)
        {
            targetImage.color = originalColor;
        }
    }
}