using UnityEngine;
using Z.Core.Audio;

public class BaseDrag : MonoBehaviour
{
    public Transform target; // 目标位置.
    public Rigidbody2D rig2D;

    [Header("拖拽时关闭重力")]
    public bool isDragCloseGravity = false; // 是否在拖拽时关闭重力

    [Header("拖拽锁定Z轴")]
    public bool isDragLockZ = false;

    protected bool isDragging = false;
    protected Collider2D spriteCollider;
    protected Camera mainCamera;
    protected float initialZ;
    protected Vector3 mouseWorldPos;
    protected float m_GravityScale = 1;

    private Vector3 offset;
    private RigidbodyConstraints2D m_Constraints;

    protected virtual void Awake()
    {
        // 获取主摄像机引用
        mainCamera = Camera.main;

        // 如果没有Collider2D组件，添加一个BoxCollider2D
        spriteCollider = GetComponent<Collider2D>();


        if (spriteCollider == null)
        {
            spriteCollider = gameObject.AddComponent<BoxCollider2D>();
        }


        if (target == null)
        {
            target = transform; // 如果没有设置目标位置，默认为自身位置
        }


        // 记录初始Z轴位置
        initialZ = target.position.z;



        m_Constraints = rig2D.constraints;
    }

    protected virtual void OnMouseDown()
    {
        // 新增：检查是否点击到子物体的 DisconnectRing
        // 将鼠标位置转换为世界坐标
        mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = initialZ;

        // 检查鼠标是否点击在Sprite上
        if (spriteCollider.OverlapPoint(mouseWorldPos))
        {
            isDragging = true;
            // 计算鼠标位置与精灵位置的偏移量
            offset = target.position - mouseWorldPos;
        }

        if (isDragCloseGravity && rig2D)
        {
            //print("sprite drag OnMouseDown isDragCloseGravity:" + m_GravityScale);
            m_GravityScale = rig2D.gravityScale;
            rig2D.gravityScale = 0; // 在拖拽时关闭重力
        }

        if (isDragLockZ && rig2D)
        {
            rig2D.constraints = RigidbodyConstraints2D.FreezeRotation; // 锁定Z轴位置
        }


        AudioManager.Instance.PlayAudioEffect(AudioManager.Instance.PickItemAduio);
    }

    protected virtual void OnMouseDrag()
    {
        if (isDragging)
        {
            // 将鼠标位置转换为世界坐标
            mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // 设置精灵的位置（保持初始Z轴位置）
            target.position = new Vector3(mouseWorldPos.x + offset.x, mouseWorldPos.y + offset.y, initialZ);
        }
    }

    protected virtual void OnMouseUp()
    {
        isDragging = false;

        AudioManager.Instance.PlayAudioEffect(AudioManager.Instance.DropItemAduio);


        if (isDragCloseGravity && rig2D)
        {
            //print("sprite drag OnMouseUp isDragCloseGravity:" + m_GravityScale);
            rig2D.gravityScale = m_GravityScale; // 在拖拽时关闭重力
        }

        if (isDragLockZ && rig2D)
        {
            rig2D.constraints = m_Constraints; // 恢复Z轴设置
        }
    }
}
