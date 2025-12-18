using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHealthBar : MonoBehaviour
{
    [Header("血条组件引用")]
    public Slider healthSlider;
    public Slider shieldSlider;
    public Slider energySlider;
    public Image healthFillImage;
    public Image shieldFillImage;
    public Image energyFillImage;

    [Header("文本显示")]
    public TextMeshProUGUI healthText;
    public bool showHealthText = true;
    public bool showPercentage = true;

    [Header("颜色设置")]
    public Color healthColor = new Color(0.8f, 0.2f, 0.2f);
    public Color shieldColor = new Color(0.2f, 0.6f, 1f);
    public Color energyColor = new Color(0.2f, 0.8f, 0.4f);
    public Color lowHealthColor = new Color(1f, 0.3f, 0f);
    public float lowHealthThreshold = 0.3f;

    [Header("世界空间设置")]
    public bool isWorldSpace = true;
    public Vector3 worldOffset = new Vector3(0, 1.5f, 0);
    public Transform followTarget;

    [Header("动画设置")]
    public bool smoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("显示控制")]
    public bool hideWhenFull = false;
    public bool hideWhenDead = true;
    public float autoHideDelay = 3f;

    private CharacterAttributes attributes;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private float targetHealthFill;
    private float targetShieldFill;
    private float targetEnergyFill;
    private float currentHealthFill;
    private float currentShieldFill;
    private float currentEnergyFill;

    private float lastDamageTime;
    private bool isInitialized = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (isWorldSpace)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
            }
        }

        if (healthFillImage != null)
        {
            healthFillImage.color = healthColor;
        }

        if (shieldFillImage != null)
        {
            shieldFillImage.color = shieldColor;
        }

        if (energyFillImage != null)
        {
            energyFillImage.color = energyColor;
        }
    }

    public void Initialize(CharacterAttributes characterAttributes, Transform target)
    {
        attributes = characterAttributes;
        followTarget = target;

        if (attributes == null)
        {
            LogManager.LogError("[CharacterHealthBar] CharacterAttributes未设置");
            return;
        }

        attributes.OnHealthChanged += UpdateHealthBar;
        attributes.OnShieldChanged += UpdateShieldBar;
        attributes.OnEnergyChanged += UpdateEnergyBar;
        attributes.OnDeath += OnCharacterDeath;

        UpdateHealthBar();
        UpdateShieldBar();
        UpdateEnergyBar();

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (attributes != null)
        {
            attributes.OnHealthChanged -= UpdateHealthBar;
            attributes.OnShieldChanged -= UpdateShieldBar;
            attributes.OnEnergyChanged -= UpdateEnergyBar;
            attributes.OnDeath -= OnCharacterDeath;
        }
    }

    private void Update()
    {
        if (!isInitialized || attributes == null)
            return;

        UpdatePosition();
        UpdateSmoothFill();
        UpdateAutoHide();
    }

    private void UpdatePosition()
    {
        if (isWorldSpace && followTarget != null)
        {
            transform.position = followTarget.position + worldOffset;

            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }
    }

    private void UpdateSmoothFill()
    {
        if (!smoothTransition)
            return;

        if (healthSlider != null)
        {
            currentHealthFill = Mathf.Lerp(currentHealthFill, targetHealthFill, Time.deltaTime * transitionSpeed);
            healthSlider.value = currentHealthFill;
        }

        if (shieldSlider != null && attributes.maxShield > 0)
        {
            currentShieldFill = Mathf.Lerp(currentShieldFill, targetShieldFill, Time.deltaTime * transitionSpeed);
            shieldSlider.value = currentShieldFill;
        }

        if (energySlider != null && attributes.maxEnergy > 0)
        {
            currentEnergyFill = Mathf.Lerp(currentEnergyFill, targetEnergyFill, Time.deltaTime * transitionSpeed);
            energySlider.value = currentEnergyFill;
        }
    }

    private void UpdateAutoHide()
    {
        if (hideWhenFull && attributes.currentHealth >= attributes.maxHealth && attributes.currentShield <= 0)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        if (autoHideDelay > 0 && Time.time - lastDamageTime > autoHideDelay)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * 2f);
        }
        else
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * 5f);
        }
    }

    private void UpdateHealthBar()
    {
        if (attributes == null || healthSlider == null)
            return;

        lastDamageTime = Time.time;

        float healthPercent = attributes.currentHealth / attributes.maxHealth;
        targetHealthFill = healthPercent;

        if (!smoothTransition)
        {
            healthSlider.value = healthPercent;
            currentHealthFill = healthPercent;
        }

        if (healthFillImage != null)
        {
            healthFillImage.color = healthPercent <= lowHealthThreshold ? lowHealthColor : healthColor;
        }

        UpdateHealthText();
    }

    private void UpdateShieldBar()
    {
        if (attributes == null)
            return;

        lastDamageTime = Time.time;

        if (shieldSlider != null)
        {
            if (attributes.maxShield > 0)
            {
                float shieldPercent = attributes.currentShield / attributes.maxShield;
                targetShieldFill = shieldPercent;

                if (!smoothTransition)
                {
                    shieldSlider.value = shieldPercent;
                    currentShieldFill = shieldPercent;
                }

                shieldSlider.gameObject.SetActive(true);
            }
            else
            {
                shieldSlider.gameObject.SetActive(false);
            }
        }

        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (!showHealthText || healthText == null || attributes == null)
            return;

        if (showPercentage)
        {
            float healthPercent = (attributes.currentHealth / attributes.maxHealth) * 100f;
            healthText.text = $"{Mathf.RoundToInt(healthPercent)}%";
        }
        else
        {
            healthText.text = $"{Mathf.RoundToInt(attributes.currentHealth)}/{Mathf.RoundToInt(attributes.maxHealth)}";
        }
    }

    private void OnCharacterDeath(CharacterBase source)
    {
        if (hideWhenDead)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
    }

    public void ForceUpdate()
    {
        UpdateHealthBar();
        UpdateShieldBar();
        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        if (attributes == null)
            return;

        lastDamageTime = Time.time;

        if (energySlider != null)
        {
            if (attributes.maxEnergy > 0)
            {
                float energyPercent = attributes.currentEnergy / attributes.maxEnergy;
                targetEnergyFill = energyPercent;

                if (!smoothTransition)
                {
                    energySlider.value = energyPercent;
                    currentEnergyFill = energyPercent;
                }

                energySlider.gameObject.SetActive(true);
            }
            else
            {
                energySlider.gameObject.SetActive(false);
            }
        }
    }
}
