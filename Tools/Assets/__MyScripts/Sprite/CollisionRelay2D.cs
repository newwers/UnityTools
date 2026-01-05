using System;
using UnityEngine;

public class CollisionRelay2D : MonoBehaviour
{
    // 委托定义
    public event Action<Collision2D> OnCollisionEnter2DEvent;
    public event Action<Collision2D> OnCollisionStay2DEvent;
    public event Action<Collision2D> OnCollisionExit2DEvent;

    public event Action<Collider2D> OnTriggerEnter2DEvent;
    public event Action<Collider2D> OnTriggerStay2DEvent;
    public event Action<Collider2D> OnTriggerExit2DEvent;

    public event Action OnMouseDownEvent;
    public event Action OnMouseDragEvent;
    public event Action OnMouseUpEvent;

    [Tooltip("是否在碰撞时将自身信息也传递")]
    [SerializeField] private bool includeSelf = true;

    [Tooltip("传递碰撞事件的游戏对象（默认为父物体）")]
    [SerializeField] private GameObject targetObject;

    private void Start()
    {
        // 如果未指定目标，默认使用父物体
        if (targetObject == null && transform.parent != null)
        {
            targetObject = transform.parent.gameObject;
        }
    }

    // 碰撞事件
    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionEnter2DEvent?.Invoke(collision);

        if (targetObject != null)
        {
            targetObject.SendMessage("OnChildCollisionEnter2D",
                new ChildCollisionData(collision, gameObject, includeSelf),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionStay2DEvent?.Invoke(collision);

        if (targetObject != null)
        {
            targetObject.SendMessage("OnChildCollisionStay2D",
                new ChildCollisionData(collision, gameObject, includeSelf),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        OnCollisionExit2DEvent?.Invoke(collision);

        if (targetObject != null)
        {
            targetObject.SendMessage("OnChildCollisionExit2D",
                new ChildCollisionData(collision, gameObject, includeSelf),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    // 触发器事件
    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnter2DEvent?.Invoke(other);

        if (targetObject != null)
        {
            targetObject.SendMessage("OnChildTriggerEnter2D",
                new ChildTriggerData(other, gameObject, includeSelf),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnTriggerStay2DEvent?.Invoke(other);

        if (targetObject != null)
        {
            targetObject.SendMessage("OnChildTriggerStay2D",
                new ChildTriggerData(other, gameObject, includeSelf),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerExit2DEvent?.Invoke(other);

        if (targetObject != null)
        {
            targetObject.SendMessage("OnChildTriggerExit2D",
                new ChildTriggerData(other, gameObject, includeSelf),
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnMouseDown()
    {
        OnMouseDownEvent?.Invoke();
        if (targetObject != null)
        {
            targetObject.SendMessage("OnMouseDown",
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnMouseDrag()
    {
        OnMouseDragEvent?.Invoke();

        if (targetObject != null)
        {
            targetObject.SendMessage("OnMouseDrag",
                SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnMouseUp()
    {
        OnMouseUpEvent?.Invoke();
        if (targetObject != null)
        {
            targetObject.SendMessage("OnMouseUp",
                SendMessageOptions.DontRequireReceiver);
        }
    }

    // 设置目标对象
    public void SetTarget(GameObject target)
    {
        targetObject = target;
    }

    // 数据结构
    [System.Serializable]
    public class ChildCollisionData
    {
        public Collision2D collision;
        public GameObject childObject;
        public GameObject thisGameObject;
        public Collider2D collider;
        public Rigidbody2D rigidbody;

        public ChildCollisionData(Collision2D collision, GameObject child, bool includeSelf)
        {
            this.collision = collision;
            this.childObject = child;

            if (includeSelf)
            {
                thisGameObject = child;
                collider = child.GetComponent<Collider2D>();
                rigidbody = child.GetComponent<Rigidbody2D>();
            }
        }
    }

    [System.Serializable]
    public class ChildTriggerData
    {
        public Collider2D collider;
        public GameObject childObject;
        public GameObject thisGameObject;
        public Collider2D triggerCollider;

        public ChildTriggerData(Collider2D collider, GameObject child, bool includeSelf)
        {
            this.collider = collider;
            this.childObject = child;

            if (includeSelf)
            {
                thisGameObject = child;
                triggerCollider = child.GetComponent<Collider2D>();
            }
        }
    }
}