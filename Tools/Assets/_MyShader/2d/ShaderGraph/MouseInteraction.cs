using System.Collections;
using UnityEngine;

public class MouseInteraction : MonoBehaviour
{
    [Header("鼠标晃动设置")]
    [SerializeField] private float maxShakeIntensity = 0.3f;     // 最大晃动强度
    [SerializeField] private float shakeFrequency = 8f;       // 晃动频率
    [SerializeField] private float shakeDuration = 0.5f;        // 晃动持续时间
    [SerializeField] private float fadeOutDuration = 0.5f;      // 淡出时间

    private Material treeMaterial;
    private float currentShakeIntensity = 0f;
    private Coroutine shakeCoroutine;

    private static readonly int MouseIntensityID = Shader.PropertyToID("_MouseIntensity");
    private static readonly int MouseFrequencyID = Shader.PropertyToID("_MouseFrequency");

    void Start()
    {
        // 获取材质
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            treeMaterial = renderer.material;
        }

        // 初始化Shader参数
        if (treeMaterial != null)
        {
            treeMaterial.SetFloat(MouseIntensityID, 0f);
            treeMaterial.SetFloat(MouseFrequencyID, shakeFrequency);
        }
    }

    void OnMouseEnter()
    {
        // 鼠标进入，开始晃动
        StartShake();
    }

    void StartShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsedTime = 0f;
        currentShakeIntensity = maxShakeIntensity;

        // 强烈晃动阶段
        while (elapsedTime < shakeDuration)
        {
            if (treeMaterial != null)
            {
                treeMaterial.SetFloat(MouseIntensityID, currentShakeIntensity);
            }

            elapsedTime += Time.deltaTime;

            // 在晃动阶段轻微衰减
            float progress = elapsedTime / shakeDuration;
            currentShakeIntensity = Mathf.Lerp(maxShakeIntensity, maxShakeIntensity * 0.3f, progress);

            yield return null;
        }

        // 淡出阶段
        float fadeTime = 0f;
        float startIntensity = currentShakeIntensity;

        while (fadeTime < fadeOutDuration)
        {
            if (treeMaterial != null)
            {
                float t = fadeTime / fadeOutDuration;
                currentShakeIntensity = Mathf.Lerp(startIntensity, 0f, t);
                treeMaterial.SetFloat(MouseIntensityID, currentShakeIntensity);
            }

            fadeTime += Time.deltaTime;
            yield return null;
        }

        // 完全停止
        if (treeMaterial != null)
        {
            treeMaterial.SetFloat(MouseIntensityID, 0f);
        }

        currentShakeIntensity = 0f;
        shakeCoroutine = null;
    }

    void OnDestroy()
    {
        if (treeMaterial != null)
        {
            treeMaterial.SetFloat(MouseIntensityID, 0f);
        }
    }
}