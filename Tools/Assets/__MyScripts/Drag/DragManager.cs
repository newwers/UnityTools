using System.Linq;
using UnityEngine;

using UnityEngine.Events;

public interface IDraggable
{
    void OnBeginDrag();
    void OnDrag();
    void OnEndDrag(bool isDrag);

    void OnClick();
}

/// <summary>
/// 单例拖拽管理器：同一时间只有一个拖拽目标
/// 管理实现了IDraggable的目标，并在拖拽开始/进行/结束时通知目标
/// </summary>
public class DragManager : BaseMonoSingleClass<DragManager>
{

    [System.Serializable]
    public class DragEvent : UnityEvent<GameObject> { }

    [Header("检测设置")]
    [SerializeField] private bool detect2D = true; // 默认检测2D碰撞
    [SerializeField] private LayerMask interactableLayer = ~0; // 可交互层
    [SerializeField] private string interactableTag = "Interactable"; // 可交互标签（可选）

    [Header("事件")]
    public DragEvent onDragStarted;    // 开始拖拽时触发
    public DragEvent onDragging;       // 拖拽过程中触发
    public DragEvent onDragEnded;      // 拖拽结束时触发

    [Header("调试")]
    [SerializeField] private bool debugMode = false;

    // 私有变量
    private Camera mainCamera;
    private GameObject currentDraggedObject;
    private Vector3 offset;
    private float zDistance;
    private bool isDragging = false;
    private Vector3 m_MouseClickPos;


    private IDraggable current;

    public IDraggable Current => current;

    public bool IsDragging => current != null;

    protected override void Awake()
    {
        base.Awake();
        // 获取主相机
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // 鼠标按下
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        // 鼠标按住
        if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag();
        }

        // 鼠标释放
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag(isDragging);
        }
    }

    private void StartDrag()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        GameObject hitObject = null;

        if (detect2D)
        {
            // 2D射线检测
            RaycastHit2D[] hits = RayCastAll2D();

            foreach (var item in hits)
            {
                LogManager.Log($"检测到2D物体: {item.collider.gameObject.name}, 位置: {item.point}");
            }

            hits = hits.OrderByDescending(hit =>
            {
                var go = hit.collider.gameObject;
                if (go.GetComponent<FloatingObject>() != null) return 3;    // FloatingObject 优先级最高
                if (go.GetComponent<PerchableObject>() != null) return 2;  // PerchableObject 次之
                return 1;                                                   // 其他物体最低
            })
    //.ThenBy(hit => hit.distance)  // 距离作为次要排序条件
    .ToArray();


            if (hits != null && hits.Length > 0)
            {
                var hit = hits[0];
                // 检查标签（如果设置了标签）
                if (string.IsNullOrEmpty(interactableTag) || hit.collider.CompareTag(interactableTag))
                {
                    hitObject = hit.collider.gameObject;
                    m_MouseClickPos = Input.mousePosition;

                    if (debugMode)
                    {
                        Debug.Log($"检测到2D物体: {hitObject.name}, 位置: {hit.point}");
                    }
                }
            }
        }
        else
        {
            // 3D射线检测
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
            {
                // 检查标签（如果设置了标签）
                if (string.IsNullOrEmpty(interactableTag) || hit.collider.CompareTag(interactableTag))
                {
                    hitObject = hit.collider.gameObject;
                    m_MouseClickPos = Input.mousePosition;

                    if (debugMode)
                    {
                        Debug.Log($"检测到3D物体: {hitObject.name}, 位置: {hit.point}");
                    }
                }
            }
        }

        if (hitObject == null)
        {
            return;
        }

        // 如果检测到物体，开始拖拽

        var draggable = hitObject.GetComponent<IDraggable>();

        if (draggable == null)
        {
            return;
        }

        currentDraggedObject = hitObject;
        SetDraggedObject(draggable);
        isDragging = true;

        // 计算偏移量
        if (detect2D)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            offset = currentDraggedObject.transform.position - mouseWorldPos;
            offset.z = 0;
        }
        else
        {
            zDistance = mainCamera.WorldToScreenPoint(currentDraggedObject.transform.position).z;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistance));
            offset = currentDraggedObject.transform.position - mouseWorldPos;
        }

        // 触发开始拖拽事件
        onDragStarted?.Invoke(currentDraggedObject);
        draggable.OnBeginDrag();

        if (debugMode)
        {
            Debug.Log($"开始拖拽: {currentDraggedObject.name}");
        }
    }

    public RaycastHit2D[] RayCastAll2D()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, interactableLayer);
    }

    private void ContinueDrag()
    {
        if (currentDraggedObject == null) return;

        Vector3 mousePosition = Input.mousePosition;

        if (detect2D)
        {
            // 2D拖拽
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition += offset;
            worldPosition.z = currentDraggedObject.transform.position.z; // 保持Z轴不变
            currentDraggedObject.transform.position = worldPosition;
        }
        else
        {
            // 3D拖拽
            mousePosition.z = zDistance;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition += offset;
            currentDraggedObject.transform.position = worldPosition;
        }

        // 触发拖拽中事件
        onDragging?.Invoke(currentDraggedObject);
        Current?.OnDrag();
    }

    private void EndDrag(bool isDrag)
    {
        if (currentDraggedObject == null) return;

        // 触发结束拖拽事件
        onDragEnded?.Invoke(currentDraggedObject);
        Current?.OnEndDrag(isDrag);

        if (m_MouseClickPos == Input.mousePosition)
        {
            Current?.OnClick();
        }

        if (debugMode)
        {
            Debug.Log($"结束拖拽: {currentDraggedObject.name}");
        }

        // 重置状态
        currentDraggedObject = null;
        isDragging = false;
        ForceClear(Current);
    }

    // 公开方法
    public void SetDetectionMode(bool is2D)
    {
        detect2D = is2D;
        if (debugMode)
        {
            Debug.Log($"切换检测模式为: {(is2D ? "2D" : "3D")}");
        }
    }

    public void SetInteractableLayer(LayerMask layer)
    {
        interactableLayer = layer;
    }

    public void SetInteractableTag(string tag)
    {
        interactableTag = tag;
    }


    /// <summary>
    /// 请求开始拖拽，只有当没有其他拖拽目标时才能成功
    /// </summary>
    //public bool TryBeginDrag(IDraggable draggable)
    //{
    //    //todo:待修改逻辑
    //    if (draggable == null) return false;
    //    if (current != null && current != draggable) return false;

    //    current = draggable;
    //    current.OnBeginDrag();
    //    return true;
    //}

    /// <summary>
    /// 调用进行中（每帧）
    /// </summary>
    //public void DoDrag(IDraggable draggable)
    //{
    //    if (current == draggable)
    //    {
    //        current.OnDrag();
    //    }
    //}

    /// <summary>
    /// 结束拖拽
    /// </summary>
    //public void EndDrag(IDraggable draggable, bool isDrag)
    //{
    //    if (current == null) return;

    //    if (current == draggable)
    //    {
    //        current.OnEndDrag(isDrag);
    //        current = null;
    //    }
    //}

    /// <summary>
    /// 强制清除当前拖拽目标（例如对象销毁时）
    /// </summary>
    public void ForceClear(IDraggable draggable)
    {
        if (current == draggable) current = null;
    }

    void SetDraggedObject(IDraggable draggable)
    {
        current = draggable;
    }
}
