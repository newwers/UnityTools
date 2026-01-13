using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class SkillCooldownManager : MonoBehaviour
{
    private Dictionary<AttackActionData, float> cooldownTimers = new Dictionary<AttackActionData, float>();

    private void Update()
    {
        List<AttackActionData> keysToRemove = new List<AttackActionData>();

        foreach (var kvp in cooldownTimers)
        {
            if (kvp.Value <= Time.time)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            cooldownTimers.Remove(key);
        }
    }

    public bool IsOnCooldown(AttackActionData attackData)
    {
        if (attackData == null) return false;

        return cooldownTimers.ContainsKey(attackData) && cooldownTimers[attackData] > Time.time;
    }

    public void StartCooldown(AttackActionData attackData)
    {
        if (attackData == null) return;

        float cooldownDuration = GetCooldown(attackData);
        if (cooldownDuration > 0)
        {
            cooldownTimers[attackData] = Time.time + cooldownDuration;
            LogManager.Log($"[SkillCooldownManager] 技能 {attackData.acitonName} 开始冷却，冷却时间: {cooldownDuration}秒");
        }
    }

    public float GetRemainingCooldown(AttackActionData attackData)
    {
        if (attackData == null || !cooldownTimers.ContainsKey(attackData))
            return 0f;

        float remaining = cooldownTimers[attackData] - Time.time;
        return Mathf.Max(0f, remaining);
    }

    public float GetCooldown(AttackActionData attackData)
    {
        return attackData.GetCooldown();
    }
}
