using System;
using UnityEngine;

/// <summary>
/// 拖拽3D物体脚本
/// 原理:直接拖拽物体,而不创建新的
/// </summary>
public class DragObject : MonoBehaviour
{

    public event Action<bool> OnDropEvent;
    public event Action OnBeginDragEvent;
    public int Index;
    public int TargetIndex;
    public bool IsRight
    {
        get
        {
            return Index == TargetIndex;
        }
    }
    //public event Action OnDragEvent;

    private Vector3 screenPoint;
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 originalPosition; // 记录物体的原始位置
    private Collider objectCollider; // 存储物体的碰撞体组件

    void Start()
    {
        objectCollider = GetComponent<Collider>(); // 获取物体的碰撞体组件
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    originalPosition = transform.position; // 在开始时记录物体的原始位置

                    screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
                    offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                    isDragging = true;
                    // 触发开始拖拽事件
                    OnBeginDragEvent?.Invoke();
                    // 禁用射线检测
                    DisableRaycastDetection();
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            if (curPosition.z < 3.4f)//防止看不到
            {
                curPosition.z = 3.4f;
            }
            transform.position = curPosition;
            // 触发拖拽过程事件
            //OnDragEvent?.Invoke();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            CheckDropPosition();

            // 恢复射线检测
            EnableRaycastDetection();
        }
    }


    private void CheckDropPosition()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool canDrop = false;
        if (Physics.Raycast(ray, out hit))
        {
            DropObject dropObject = hit.collider.GetComponent<DropObject>();
            if (dropObject != null)
            {
                canDrop = true;
                Index = dropObject.TargetIndex;
                transform.position = dropObject.transform.position;
            }
            else
            {
                var target = hit.collider.GetComponent<DragObject>();
                if (target)
                {
                    Vector3 temPos = target.transform.position;
                    target.transform.position = originalPosition;
                    transform.position = temPos;
                    canDrop = true;

                    var indexTarget = target.Index;
                    target.Index = Index;
                    Index = indexTarget;
                }
                else
                {
                    // 不能释放，回到原本位置
                    transform.position = originalPosition;
                }
            }
        }
        else
        {
            // 未击中任何物体，回到原本位置
            transform.position = originalPosition;
        }
        // 触发放下事件，传递是否成功放下的布尔值
        OnDropEvent?.Invoke(canDrop);
    }

    private void DisableRaycastDetection()
    {
        if (objectCollider != null)
        {
            objectCollider.enabled = false; // 禁用碰撞体，从而禁用射线检测
        }
    }

    private void EnableRaycastDetection()
    {
        if (objectCollider != null)
        {
            objectCollider.enabled = true; // 启用碰撞体，恢复射线检测
        }
    }
}
