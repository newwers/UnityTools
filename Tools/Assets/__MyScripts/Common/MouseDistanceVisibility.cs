using System.Collections;
using UnityEngine;

/// <summary>
/// 气球环和释放环的距离显示控制脚本
/// </summary>
public class MouseDistanceVisibility : MonoBehaviour
{
    [Header("距离参数")]
    [Tooltip("触发显示的最大距离")]
    public float maxDistance = 0.1f; // 可编辑的距离参数
    public bool isDragShow = false; // 是否在拖拽时显示
    public bool bShow = true; // 是否显示物体，默认显示
    private Collider2D targetCollider; // 可选的碰撞器，用于检测鼠标是否在物体上
    private Renderer targetRenderer; // 需要控制显隐的渲染器
    private Camera mainCamera;

    public void SetIsShow(bool isShow)
    {
        bShow = isShow;
        if (targetRenderer != null)
        {
            targetRenderer.enabled = isShow;
        }
        if (targetCollider != null)
        {
            targetCollider.enabled = isShow;
        }
    }

    private void Awake()
    {
        // 自动获取主相机
        mainCamera = Camera.main;

        // 如果未指定渲染器，尝试自动获取
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                LogManager.LogError("未找到渲染器组件！", this);
            }
        }
        targetCollider = GetComponent<Collider2D>();
    }


    void Update()
    {
        if (bShow == false)
        {
            targetRenderer.enabled = false;
            targetCollider.enabled = false;
            return;
        }

        // 获取鼠标世界坐标（2D 正交相机适用）
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 确保 Z 轴为 0（2D 环境）

        // 计算物体与鼠标的距离
        float distance = Vector2.Distance(transform.position, mousePos);

        // 根据距离控制显隐
        bool isShow = distance <= maxDistance + targetCollider.bounds.size.x;

        if (isShow && isDragShow)//拖拽显示
        {
            isShow = GameManager.Instance.IsDragging && GameManager.Instance.DragObject.dragType == DragType.Balloon;
        }
        targetRenderer.enabled = isShow;
        targetCollider.enabled = isShow; // 如果有碰撞器，也控制其启用状态
    }


    public void SetDelayShow()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayShow(0.5f)); // 延迟0.5秒后显示
        }

    }

    IEnumerator DelayShow(float time)
    {
        yield return new WaitForSeconds(time);
        SetIsShow(true);
    }
}