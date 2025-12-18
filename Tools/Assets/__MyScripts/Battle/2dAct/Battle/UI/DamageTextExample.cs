using UnityEngine;

public class DamageTextExample : MonoBehaviour
{
    [Header("测试设置")]
    public KeyCode damageKey = KeyCode.D;
    public KeyCode criticalKey = KeyCode.C;
    public KeyCode healKey = KeyCode.H;
    public KeyCode missKey = KeyCode.M;
    public float testDamage = 100f;
    
    void Update()
    {
        if (Input.GetKeyDown(damageKey))
        {
            ShowTestDamage(DamageTextType.Normal);
        }
        
        if (Input.GetKeyDown(criticalKey))
        {
            ShowTestDamage(DamageTextType.Critical);
        }
        
        if (Input.GetKeyDown(healKey))
        {
            ShowTestDamage(DamageTextType.Heal);
        }
        
        if (Input.GetKeyDown(missKey))
        {
            ShowTestDamage(DamageTextType.Miss);
        }
    }
    
    void ShowTestDamage(DamageTextType type)
    {
        if (DamageTextManager.Instance != null)
        {
            Vector3 position = transform.position + Vector3.up * 1.5f;
            DamageTextManager.Instance.ShowDamageText(testDamage, type, position);
        }
        else
        {
            Debug.LogWarning("DamageTextManager not found in scene!");
        }
    }
}
