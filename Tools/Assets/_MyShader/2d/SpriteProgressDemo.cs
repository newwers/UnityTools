using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteProgressDemo : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material materialInstance;

    [Header("进度条设置")]
    [Range(0f, 1f)]
    public float progress = 0.5f;

    public bool isEnable;



    void Start()
    {
        // 获取精灵渲染器组件
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 创建材质实例，避免修改共享材质
        //materialInstance = new Material(spriteRenderer.material);
        //spriteRenderer.material = materialInstance;
        materialInstance = spriteRenderer.material;

        // 初始化参数
        UpdateProgress();

    }

    void Update()
    {
        if (isEnable == false)
        {
            return;
        }
        // 实时更新进度（如果需要动画效果）
        // 取消下面一行的注释可以看到自动动画效果
        progress = Mathf.PingPong(Time.time * 0.2f, 1f);
        UpdateProgress();
    }

    /// <summary>
    /// 更新进度值
    /// </summary>
    public void UpdateProgress()
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat("_MaskProgress", progress);
        }
    }




    // 清理材质实例
    void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
