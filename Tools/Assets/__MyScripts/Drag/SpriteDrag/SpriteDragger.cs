using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum DragType
{
    None,
    Balloon, // 气球
    Item, // 物品
}

public class SpriteDragger : BaseDrag
{

    [FormerlySerializedAs("springJoint2D")]
    public SpringJoint2D connectSpringJoint2D;
    public BalloonController balloonController;
    [Header("是否设为当前拖拽对象")]
    public bool isSetDragSelect = true;


    private List<DisconnectRing> childrenDisconnectRings; // 子物体的DisconnectRing列表



    private int m_OrderInLayer = 0; // 层级

    protected override void Awake()
    {
        base.Awake();

        childrenDisconnectRings = new List<DisconnectRing>(GetComponentsInChildren<DisconnectRing>());
        childrenDisconnectRings = GetComponentsInChildren<DisconnectRing>().ToList();


        if (rig2D == null)
        {
            rig2D = GetComponent<Rigidbody2D>();
            if (rig2D == null)
            {
                rig2D = gameObject.AddComponent<Rigidbody2D>();
            }
        }
    }


    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        foreach (DisconnectRing ring in childrenDisconnectRings)
        {
            if (!ring.ring.enabled) continue;
            Collider2D col = ring.GetCollider();
            if (col != null && col.OverlapPoint(mouseWorldPos))
            {
                // 检测到点击子物体的DisconnectRing，直接退出不执行拖拽
                return;
            }
        }

        // 以下是原有的拖拽逻辑
        //print("OnMouseDown:spriteDrag:" + name);
        if (balloonController && balloonController.isAttached)
        {
            return;
        }


        // 配置弹簧参数
        if (connectSpringJoint2D)
        {
            connectSpringJoint2D.connectedAnchor = mouseWorldPos;
            connectSpringJoint2D.enabled = true;
        }

        //提高物理迭代次数
        Physics2D.velocityIterations = 16; // 默认8
        Physics2D.positionIterations = 16; // 默认3



        if (balloonController)
        {
            m_OrderInLayer = balloonController.balloonSprite.sortingOrder;
            balloonController.OnBeginDrag();
        }



        if (GameManager.Instance.DragObject == null)//当前拖拽物体空的时候才设置鼠标光标
        {
            GameManager.Instance.SetMouseCursor(EMouseCursorType.Drag);
        }

        if (isSetDragSelect)
        {
            GameManager.Instance.DragObject = this; // 设置当前拖拽对象
        }
    }

    protected override void OnMouseDrag()
    {
        base.OnMouseDrag();

        if (isDragging)
        {
            if (connectSpringJoint2D)
            {
                connectSpringJoint2D.connectedAnchor = mouseWorldPos;
            }
        }
    }

    protected override void OnMouseUp()
    {
        base.OnMouseUp();

        if (connectSpringJoint2D)
        {
            connectSpringJoint2D.enabled = false;
        }
        if (balloonController)
        {
            balloonController.SetOrderInLayer(m_OrderInLayer); // 设置层级，确保在最上层
            balloonController.OnEndDrag();
        }
        //提高物理迭代次数
        Physics2D.velocityIterations = 8; // 默认8
        Physics2D.positionIterations = 3; // 默认3

        if (isSetDragSelect)
        {
            GameManager.Instance.DragObject = null; // 设置当前拖拽对象
            GameManager.Instance.OnDropObject(this);
        }





        GameManager.Instance.SetMouseCursor(EMouseCursorType.Hand);
    }

    private void OnMouseEnter()
    {
        if (isDragging == false)
        {
            GameManager.Instance.SetMouseCursor(EMouseCursorType.Hand);
        }
    }

    private void OnMouseExit()
    {
        if (isDragging == false)
        {
            GameManager.Instance.SetMouseCursor(EMouseCursorType.None);
        }
    }
}