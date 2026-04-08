using StateMachineSystem;
using UnityEngine;

public class AnimalBase : MonoBehaviour, IReusableObject
{
    [Header("基本信息")]
    public string instanceId;

    public AnimalDataSO animalData;
    public AnimalType animalType;

    public StateMachine StateMachine;

    public bool IsDragging
    {
        get
        {
            return isDragging;
        }
    }

    public bool IsInGround
    {
        get
        {
            return CheckGroundCollision();
        }
    }

    public bool isDragging = false;
    public Vector3 dragOffset;

    private bool isInObject = false;
    public bool IsInObject
    {
        get { return isInObject; }
        set { isInObject = value; }
    }

    private Collider2D col;
    protected int m_GroundLayer;

    private void Awake()
    {
        col = GetComponent<Collider2D>();


        // 获取MainGround层的索引
        m_GroundLayer = LayerMask.NameToLayer("MainGround");
    }

    public void ApplyColor(Color gardenMaskColor)
    {
        if (StateMachine == null || StateMachine.spriteRenderer == null)
        {
            return;
        }

        StateMachine.spriteRenderer.color = gardenMaskColor;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {

    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {

    }

    public void OnSpawn()
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            instanceId = System.Guid.NewGuid().ToString();
        }
        if (animalData != null)
        {
            animalType = animalData.animalType;
        }
        if (Garden.HasInstance)
        {
            ApplyColor(Garden.Instance.gardenMaskColor);
        }


        gameObject.SetActive(true);
    }

    public void OnRecycle()
    {
        gameObject.SetActive(false);
    }

    public void FadeIn(float duration = 1f)
    {
        StartCoroutine(FadeInCoroutine(duration));
    }

    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeInCoroutine(float duration)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;

        // 先设置透明度为0
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = 0f;
            renderer.color = color;
        }

        while (elapsedTime < duration)
        {
            float alpha = elapsedTime / duration;
            foreach (var renderer in renderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = 1f - (elapsedTime / duration);
            foreach (var renderer in renderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = 0f;
            renderer.color = color;
        }
    }

    public virtual void OnMouse_Down()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mousePosition;
    }

    public virtual void OnMouse_Up()
    {
        if (isDragging)
        {
            isDragging = false;
            OnDragEnd();
        }
    }

    public virtual void OnMouse_Drag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            var pos = mousePosition + dragOffset;
            transform.position = pos;
            //LogManager.Log($"drag mouse pos:" + pos);
        }
    }

    public virtual bool CanDrag()
    {
        return false;
    }

    public virtual void OnDragStart()
    {

    }

    public virtual void OnDragEnd()
    {

    }


    public virtual bool CheckGroundCollision()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + new Vector3(0, animalData.collisionCheckRadius / 2, 0), animalData.collisionCheckRadius);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == m_GroundLayer)
            {
                return true;
            }
        }

        return false;
    }
}
