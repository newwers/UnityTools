using UnityEngine;
using TMPro;
using System.Collections;

public class DamageTextController : MonoBehaviour
{
    [Header("飘字设置")]
    public float moveSpeed = 50f;
    public float fadeDuration = 1f;
    public float lifetime = 1.5f;
    public Vector2 randomOffset = new Vector2(30f, 10f);
    
    [Header("颜色设置")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.yellow;
    public Color missColor = Color.gray;
    public Color blockColor = Color.cyan;
    public Color healColor = Color.green;
    
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 moveDirection;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void Initialize(float damage, DamageTextType textType, Vector3 worldPosition)
    {
        string damageText = "";
        Color textColor = normalDamageColor;
        float fontSize = 24f;
        
        switch (textType)
        {
            case DamageTextType.Normal:
                damageText = Mathf.RoundToInt(damage).ToString();
                textColor = normalDamageColor;
                fontSize = 24f;
                break;
            case DamageTextType.Critical:
                damageText = Mathf.RoundToInt(damage).ToString() + "!";
                textColor = criticalDamageColor;
                fontSize = 32f;
                break;
            case DamageTextType.Miss:
                damageText = "闪避";
                textColor = missColor;
                fontSize = 20f;
                break;
            case DamageTextType.Block:
                damageText = "格挡";
                textColor = blockColor;
                fontSize = 20f;
                break;
            case DamageTextType.Heal:
                damageText = "+" + Mathf.RoundToInt(damage).ToString();
                textColor = healColor;
                fontSize = 24f;
                break;
        }
        
        textMesh.text = damageText;
        textMesh.color = textColor;
        textMesh.fontSize = fontSize;
        
        SetPosition(worldPosition);
        
        float randomX = Random.Range(-randomOffset.x, randomOffset.x);
        float randomY = Random.Range(0, randomOffset.y);
        moveDirection = new Vector3(randomX, moveSpeed + randomY, 0f);
        
        StartCoroutine(AnimateText());
    }
    
    private void SetPosition(Vector3 worldPosition)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                cam,
                out Vector2 localPoint
            );
            rectTransform.localPosition = localPoint;
        }
    }
    
    private IEnumerator AnimateText()
    {
        float elapsed = 0f;
        Vector3 startPosition = rectTransform.localPosition;
        
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            
            rectTransform.localPosition += moveDirection * Time.deltaTime;
            
            if (elapsed >= lifetime - fadeDuration)
            {
                float fadeProgress = (lifetime - elapsed) / fadeDuration;
                canvasGroup.alpha = fadeProgress;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}

public enum DamageTextType
{
    Normal,
    Critical,
    Miss,
    Block,
    Heal
}
