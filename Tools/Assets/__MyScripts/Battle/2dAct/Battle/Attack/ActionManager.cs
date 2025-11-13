/*
 角色行为管理器
 */
using UnityEngine;

[CreateAssetMenu(fileName = "Character Action Manager", menuName = "Character System/Character Action Manager")]
public class ActionManager : ScriptableObject
{
    [Header("角色标识")]
    public Character character; //攻击所属角色

    [Header("攻击配置")]
    public ActionData[] normalAttacks;      // 普通攻击序列
    public ActionData dashAttack;           // 冲刺攻击
    public ActionData jumpAttack;           // 跳跃攻击
    public ActionData parryAttack;           // 弹反攻击
    public ActionData specialAttack;       // 特殊攻击
    public ActionData heavyAttack;         // 重攻击

    [Header("全局设置")]
    [Tooltip("连招重置时长(未接入)")]
    public float globalComboResetTime = 1.5f;

    // 通过名称获取攻击数据
    public ActionData GetAttackByName(string name)
    {
        foreach (var attack in normalAttacks)
        {
            if (attack.acitonName == name) return attack;
        }
        return null;
    }

    // 获取连招序列
    public ActionData[] GetComboSequence()
    {
        return normalAttacks;
    }
}