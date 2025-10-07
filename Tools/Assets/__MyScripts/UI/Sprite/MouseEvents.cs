using UnityEngine;
using UnityEngine.Events;

public class MouseEvents : MonoBehaviour
{
    public UnityEvent OnMouseClickHander;
    public UnityEvent OnMouseDownHander;
    public UnityEvent OnMouseUpHander;
    public UnityEvent OnMouseDragHander;

    private Camera mainCamera;
    private Collider2D m_Collider2D;
    private Vector3 m_MouseClickPos;
    private bool m_IsMouseDown = false;

    private void Awake()
    {
        m_Collider2D = GetComponent<Collider2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // 确保Z轴为0，因为2D游戏通常在XY平面上工作
            if (m_Collider2D != null && m_Collider2D.OverlapPoint(mouseWorldPos))
            {
                m_IsMouseDown = true;
                m_MouseClickPos = Input.mousePosition;
                OnMouseDownEvent();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {


            //之前的逻辑：只有在鼠标抬起位置还在物体上才触发
            //Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            //mouseWorldPos.z = 0; // 确保Z轴为0，因为2D游戏通常在XY平面上工作
            //if (m_Collider2D != null && m_Collider2D.OverlapPoint(mouseWorldPos))
            //{
            //    OnMouseUpEvent();
            //    m_IsMouseDown = false;
            //}

            //只要鼠抬起就触发，不管位置
            OnMouseUpEvent();
            m_IsMouseDown = false;

            CheckClick();
        }

        if (m_IsMouseDown && m_MouseClickPos != Input.mousePosition)
        {
            OnMouseDragEvent();
        }
    }

    void CheckClick()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, -1);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            OnMouseClickHander?.Invoke();
            //LogManager.Log("MouseEvents Update OnMouseUpAsButton");
        }
    }

    //private void OnMouseUpAsButton()
    //{
    //    OnMouseClickHander?.Invoke();
    //}

    private void OnMouseDownEvent()
    {
        OnMouseDownHander?.Invoke();
    }

    private void OnMouseUpEvent()
    {
        OnMouseUpHander?.Invoke();
    }

    private void OnMouseDragEvent()
    {
        OnMouseDragHander?.Invoke();
    }
}
