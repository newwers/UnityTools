using UnityEngine;

/// <summary>
/// 鼠标交互管理器，统一管理所有作物、果实、果实堆的鼠标事件
/// </summary>
public class MouseInteractionManager : BaseMonoSingleClass<MouseInteractionManager>
{
    [Header("设置")]
    [Tooltip("主相机")]
    public Camera mainCamera;
    
    [Tooltip("检测层")]
    public LayerMask interactionLayer;

    /// <summary>当前鼠标悬停的对象</summary>
    private Transform currentHoverObject;
    
    /// <summary>上一帧鼠标悬停的对象</summary>
    private Transform lastHoverObject;

    protected override void Awake()
    {
        base.Awake();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        // 获取鼠标在世界坐标中的位置
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // 射线检测
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, interactionLayer);
        
        // 更新当前悬停对象
        currentHoverObject = hit.collider != null ? hit.collider.transform : null;
        
        // 处理鼠标点击
        CheckMouseClick(hit);
        
        // 处理鼠标进入
        CheckMouseEnter(hit);
        
        // 处理鼠标移出
        CheckMouseExit();
        
        // 更新上一帧对象
        lastHoverObject = currentHoverObject;
    }

    /// <summary>
    /// 检查鼠标点击
    /// </summary>
    private void CheckMouseClick(RaycastHit2D hit)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider != null)
            {
                // 尝试获取可点击接口
                IMouseClickable clickable = hit.collider.GetComponent<IMouseClickable>();
                clickable?.OnMouseClickHandle();
            }
        }
    }

    /// <summary>
    /// 检查鼠标进入
    /// </summary>
    private void CheckMouseEnter(RaycastHit2D hit)
    {
        if (currentHoverObject != null && currentHoverObject != lastHoverObject)
        {
            IMouseEnterable enterable = currentHoverObject.GetComponent<IMouseEnterable>();
            enterable?.OnMouseEnterHandle();
        }
    }

    /// <summary>
    /// 检查鼠标移出
    /// </summary>
    private void CheckMouseExit()
    {
        if (lastHoverObject != null && currentHoverObject != lastHoverObject)
        {
            IMouseExitable exitable = lastHoverObject.GetComponent<IMouseExitable>();
            exitable?.OnMouseExitHandle();
        }
    }
}

/// <summary>
/// 鼠标点击接口
/// </summary>
public interface IMouseClickable
{
    void OnMouseClickHandle();
}

/// <summary>
/// 鼠标进入接口
/// </summary>
public interface IMouseEnterable
{
    void OnMouseEnterHandle();
}

/// <summary>
/// 鼠标移出接口
/// </summary>
public interface IMouseExitable
{
    void OnMouseExitHandle();
}
