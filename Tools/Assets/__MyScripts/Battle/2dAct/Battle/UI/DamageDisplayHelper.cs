using UnityEngine;

public static class DamageDisplayHelper
{
    public static void ShowDamageOnCharacter(DamageResult result, Transform target)
    {
        if (DamageTextManager.Instance == null)
        {
            LogManager.LogWarning("[DamageDisplayHelper] DamageTextManager未初始化");
            return;
        }
        
        if (target == null)
        {
            LogManager.LogWarning("[DamageDisplayHelper] 目标Transform为空");
            return;
        }
        
        Vector3 displayPosition = target.position + Vector3.up * 1.5f;
        DamageTextManager.Instance.ShowDamageResult(result, displayPosition);
    }
    
    public static void ShowHealOnCharacter(float healAmount, Transform target)
    {
        if (DamageTextManager.Instance == null)
        {
            LogManager.LogWarning("[DamageDisplayHelper] DamageTextManager未初始化");
            return;
        }
        
        if (target == null)
        {
            LogManager.LogWarning("[DamageDisplayHelper] 目标Transform为空");
            return;
        }
        
        Vector3 displayPosition = target.position + Vector3.up * 1.5f;
        DamageTextManager.Instance.ShowHealText(healAmount, displayPosition);
    }
    
    public static void ShowCustomText(string text, DamageTextType textType, Transform target)
    {
        if (DamageTextManager.Instance == null)
        {
            LogManager.LogWarning("[DamageDisplayHelper] DamageTextManager未初始化");
            return;
        }
        
        if (target == null)
        {
            LogManager.LogWarning("[DamageDisplayHelper] 目标Transform为空");
            return;
        }
        
        Vector3 displayPosition = target.position + Vector3.up * 1.5f;
        DamageTextManager.Instance.ShowDamageText(0, textType, displayPosition);
    }
}
