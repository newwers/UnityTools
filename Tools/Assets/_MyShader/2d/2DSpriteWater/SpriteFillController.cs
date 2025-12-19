using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class WaterFillController : MonoBehaviour
{
    [Header("填充控制")]
    [Range(0, 1)]
    [SerializeField] private float fillAmount = 0;

    [Header("水色设置")]
    [SerializeField] private Color waterColor = new Color(0.2f, 0.6f, 1f, 0.7f);

    [Header("波浪效果")]
    [Range(0, 0.1f)]
    [SerializeField] private float waveIntensity = 0.02f;
    [Range(0, 5f)]
    [SerializeField] private float waveSpeed = 1f;

    [Header("高级设置")]
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private float updateInterval = 0.1f;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Vector4 spriteUVBounds = Vector4.zero;
    private float lastUpdateTime = 0;

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            fillAmount = Mathf.Clamp01(value);
            UpdateMaterialProperties();
        }
    }

    public Color WaterColor
    {
        get => waterColor;
        set
        {
            waterColor = value;
            UpdateMaterialProperties();
        }
    }

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        UpdateSpriteUVBounds();

        if (spriteRenderer.sharedMaterial == null)
        {
            Debug.LogWarning("请为Sprite Renderer分配WaterFill材质");
        }

        UpdateMaterialProperties();
    }

    private void Update()
    {
        if (!autoUpdate) return;

        // 限制更新频率
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMaterialProperties();
            lastUpdateTime = Time.time;
        }
    }

    private void LateUpdate()
    {
        // 每帧检查Sprite是否发生变化
        if (spriteRenderer.sprite != null)
        {
            Vector4 currentBounds = CalculateUVBounds();
            if (currentBounds != spriteUVBounds)
            {
                UpdateSpriteUVBounds();
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying || !Application.isEditor)
        {
            UpdateMaterialProperties();
        }
    }

    [ContextMenu("立即更新材质属性")]
    public void UpdateMaterialProperties()
    {
        if (spriteRenderer == null || propertyBlock == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        spriteRenderer.GetPropertyBlock(propertyBlock);

        // 设置填充参数
        propertyBlock.SetFloat("_FillAmount", fillAmount);
        propertyBlock.SetColor("_WaterColor", waterColor);
        propertyBlock.SetFloat("_WaveIntensity", waveIntensity);
        propertyBlock.SetFloat("_WaveSpeed", waveSpeed);

        // 设置Sprite在图集中的UV边界
        propertyBlock.SetVector("_SpriteMinUV",
            new Vector4(spriteUVBounds.x, spriteUVBounds.y, 0, 0));
        propertyBlock.SetVector("_SpriteMaxUV",
            new Vector4(spriteUVBounds.z, spriteUVBounds.w, 0, 0));

        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    [ContextMenu("更新Sprite UV边界")]
    private void UpdateSpriteUVBounds()
    {
        spriteUVBounds = CalculateUVBounds();
    }

    private Vector4 CalculateUVBounds()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("Sprite Renderer或Sprite为空");
            return new Vector4(0, 0, 1, 1);
        }

        Sprite sprite = spriteRenderer.sprite;
        Vector4 bounds = sprite.border;

        // 如果sprite.uvBounds不可用，则手动计算
        if (bounds == Vector4.zero && sprite.uv.Length >= 4)
        {
            Vector2[] uv = sprite.uv;
            Vector2 min = uv[0];
            Vector2 max = uv[0];

            for (int i = 1; i < uv.Length; i++)
            {
                min = Vector2.Min(min, uv[i]);
                max = Vector2.Max(max, uv[i]);
            }

            bounds = new Vector4(min.x, min.y, max.x, max.y);
        }

        return bounds;
    }

    #region 动画辅助方法

    public void SetFillAnimated(float targetFill, float duration)
    {
        StartCoroutine(AnimateFill(targetFill, duration));
    }

    private System.Collections.IEnumerator AnimateFill(float targetFill, float duration)
    {
        float startFill = fillAmount;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = 1 - Mathf.Pow(1 - t, 3); // 缓动效果
            fillAmount = Mathf.Lerp(startFill, targetFill, t);
            UpdateMaterialProperties();
            yield return null;
        }

        fillAmount = targetFill;
        UpdateMaterialProperties();
    }

    public void SetWaterColorAnimated(Color targetColor, float duration)
    {
        StartCoroutine(AnimateWaterColor(targetColor, duration));
    }

    private System.Collections.IEnumerator AnimateWaterColor(Color targetColor, float duration)
    {
        Color startColor = waterColor;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            waterColor = Color.Lerp(startColor, targetColor, t);
            UpdateMaterialProperties();
            yield return null;
        }

        waterColor = targetColor;
        UpdateMaterialProperties();
    }

    #endregion

    #region 预设水色

    [ContextMenu("设置为纯净水色")]
    public void SetPureWaterColor()
    {
        WaterColor = new Color(0.2f, 0.6f, 1f, 0.6f);
    }

    [ContextMenu("设置为浑浊水色")]
    public void SetMuddyWaterColor()
    {
        WaterColor = new Color(0.3f, 0.4f, 0.3f, 0.5f);
    }

    [ContextMenu("设置为海水色")]
    public void SetSeaWaterColor()
    {
        WaterColor = new Color(0f, 0.3f, 0.6f, 0.7f);
    }

    [ContextMenu("设置为绿水色")]
    public void SetGreenWaterColor()
    {
        WaterColor = new Color(0.1f, 0.8f, 0.4f, 0.5f);
    }

    #endregion

    #region 调试方法

    [ContextMenu("测试填充动画")]
    public void TestFillAnimation()
    {
        SetFillAnimated(0.5f, 2f);
    }

    [ContextMenu("重置填充")]
    public void ResetFill()
    {
        FillAmount = 0;
    }

    [ContextMenu("满填充")]
    public void FullFill()
    {
        FillAmount = 1;
    }

    #endregion
}