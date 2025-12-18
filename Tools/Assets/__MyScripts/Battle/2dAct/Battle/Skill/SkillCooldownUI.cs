using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("引用")]
    public CharacterAttackController attackController;
    public AttackActionData attackActionToTrack;

    [Header("UI元素")]
    public Image cooldownFillImage;
    public Text cooldownText;

    private void Update()
    {
        if (attackController == null || attackActionToTrack == null)
            return;

        float cooldown = attackActionToTrack.GetCooldown();
        if (cooldown <= 0)
        {
            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = 0f;
            if (cooldownText != null)
                cooldownText.text = "";
            return;
        }

        float remaining = attackController.GetSkillRemainingCooldown(attackActionToTrack);
        float fillAmount = remaining / cooldown;

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = fillAmount;
        }

        if (cooldownText != null)
        {
            if (remaining > 0)
            {
                cooldownText.text = remaining.ToString("F1");
            }
            else
            {
                cooldownText.text = "";
            }
        }
    }
}
