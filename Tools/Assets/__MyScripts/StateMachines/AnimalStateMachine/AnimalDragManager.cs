using UnityEngine;
using System.Collections.Generic;

public class AnimalDragManager : MonoSingleton<AnimalDragManager>
{
    private AnimalBase currentClickedAnimal;
    private AnimalBase currentDraggingAnimal;

    [SerializeField] private GameObject clickAnimationPrefab;
    private List<GameObject> clickAnimationPool = new List<GameObject>();
    private int currentPoolIndex = 0;
    private const int MAX_ANIMATION_INSTANCES = 3;

    protected override void Awake()
    {
        base.Awake();
        InitializeClickAnimationPool();
    }

    private void InitializeClickAnimationPool()
    {
        // 预制体将在Unity编辑器中手动赋值
        // 如果未赋值，尝试从Assets目录加载
        if (clickAnimationPrefab == null)
        {
            // 注意：在运行时从Assets目录直接加载预制体需要特殊处理
            // 这里我们使用一种更可靠的方法，通过查找场景中可能存在的Click预制体
            // 或者在Unity编辑器中将预制体拖放到Inspector中
            Debug.LogWarning("Click animation prefab not assigned. Please assign it in the Inspector.");
            return;
        }

        for (int i = 0; i < MAX_ANIMATION_INSTANCES; i++)
        {
            GameObject animationInstance = Instantiate(clickAnimationPrefab);
            animationInstance.SetActive(false);
            animationInstance.transform.SetParent(transform);
            clickAnimationPool.Add(animationInstance);
        }
    }

    private void PlayClickAnimation(Vector3 position)
    {
        if (clickAnimationPrefab == null || clickAnimationPool.Count == 0)
        {
            return;
        }

        // 循环使用对象池中的实例
        GameObject animationInstance = clickAnimationPool[currentPoolIndex];
        currentPoolIndex = (currentPoolIndex + 1) % MAX_ANIMATION_INSTANCES;

        // 设置位置并激活
        animationInstance.transform.position = position;
        animationInstance.SetActive(true);

        // 播放动画
        Animator animator = animationInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, 0, 0f);
        }

        // 动画结束后禁用
        StartCoroutine(DisableAnimationAfterDelay(animationInstance, 1f));
    }

    private System.Collections.IEnumerator DisableAnimationAfterDelay(GameObject animationInstance, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animationInstance != null)
        {
            animationInstance.SetActive(false);
        }
    }

    private void Update()
    {
        // 鼠标左键点击检测
        if (Input.GetMouseButtonDown(0))
        {
            DetectAnimalClick();
        }
        // 鼠标左键松开检测
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentClickedAnimal != null)
            {
                currentClickedAnimal.OnMouse_Up();
                currentClickedAnimal = null;
            }
        }

        // 鼠标中键拖拽检测
        if (Input.GetMouseButtonDown(2))
        {
            DetectAnimalDragStart();
        }
        else if (Input.GetMouseButtonUp(2))
        {
            if (currentDraggingAnimal != null)
            {
                currentDraggingAnimal.isDragging = false;
                currentDraggingAnimal.OnDragEnd();
                currentDraggingAnimal = null;
            }
        }

        // 拖拽过程
        if (currentDraggingAnimal != null && currentDraggingAnimal.isDragging)
        {
            currentDraggingAnimal.OnMouse_Drag();
        }
    }

    private void DetectAnimalClick()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // 发射射线检测碰撞器
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            AnimalBase animal = hit.collider.GetComponent<AnimalBase>();
            if (animal != null)
            {
                currentClickedAnimal = animal;
                animal.OnMouse_Down();
                // 播放点击动画
                PlayClickAnimation(mousePosition);
            }
        }
    }

    private void DetectAnimalDragStart()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // 发射射线检测碰撞器
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            AnimalBase animal = hit.collider.GetComponent<AnimalBase>();
            if (animal != null && animal.CanDrag())
            {
                currentDraggingAnimal = animal;
                animal.dragOffset = animal.transform.position - mousePosition;
                animal.isDragging = true;
                animal.OnDragStart();
            }
        }
    }
}
