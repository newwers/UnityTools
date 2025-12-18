using System.Collections.Generic;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    private static DamageTextManager instance;
    public static DamageTextManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerObject = new GameObject("DamageTextManager");
                instance = managerObject.AddComponent<DamageTextManager>();
            }
            return instance;
        }
    }

    [Header("预制体设置")]
    public DamageTextController damageTextPrefab;

    [Header("对象池设置")]
    public int poolSize = 20;

    private Canvas canvas;
    private Queue<DamageTextController> textPool = new Queue<DamageTextController>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeCanvas();
        InitializePool();
    }

    private void InitializeCanvas()
    {
        canvas = GetComponentInChildren<Canvas>();
    }

    private void InitializePool()
    {
        if (damageTextPrefab == null)
        {
            LogManager.LogWarning("[DamageTextManager] 伤害文本预制体未设置");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            var textObject = Instantiate(damageTextPrefab, canvas.transform);
            textObject.gameObject.SetActive(false);
            textPool.Enqueue(textObject);
        }
    }

    public void ShowDamageText(float damage, DamageTextType textType, Vector3 worldPosition)
    {
        DamageTextController textObject;

        if (textPool.Count > 0)
        {
            textObject = textPool.Dequeue();
            textObject.gameObject.SetActive(true);
        }
        else
        {
            if (damageTextPrefab == null)
            {
                LogManager.LogWarning("[DamageTextManager] 无法创建伤害文本,预制体未设置");
                return;
            }
            textObject = Instantiate(damageTextPrefab, canvas.transform);
        }

        if (textObject != null)
        {
            textObject.Initialize(damage, textType, worldPosition);
        }
    }

    public void ShowDamageResult(DamageResult result, Vector3 worldPosition)
    {
        if (result.isMiss)
        {
            ShowDamageText(0, DamageTextType.Miss, worldPosition);
        }
        else if (result.isBlocked)
        {
            ShowDamageText(0, DamageTextType.Block, worldPosition);
        }
        else if (result.isCritical)
        {
            ShowDamageText(result.finalDamage, DamageTextType.Critical, worldPosition);
        }
        else
        {
            ShowDamageText(result.finalDamage, DamageTextType.Normal, worldPosition);
        }
    }

    public void ShowHealText(float healAmount, Vector3 worldPosition)
    {
        ShowDamageText(healAmount, DamageTextType.Heal, worldPosition);
    }
}
