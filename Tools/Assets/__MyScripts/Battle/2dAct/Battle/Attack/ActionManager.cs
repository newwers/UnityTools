/*
 角色行为管理器
 */
using UnityEngine;

[CreateAssetMenu(fileName = "Character Action Manager", menuName = "Character System/Character Action Manager")]
public class ActionManager : ScriptableObject
{
    [Header("角色标识")]
    public GameObject character; //绑定角色

    [Header("行为配置")]
    public IdleActionData idleAction;          // 站立动作
    public MoveActionData moveAction;          // 行走动作
    public DashActionData dashAction;          // 冲刺动作
    public JumpActionData jumpAction;          // 跳跃动作
    public ActionData fallAction;          // 下落动作
    //public ActionData landAction;          // 着陆动作
    public ActionData hurtAction;         // 受伤动作
    public ActionData deathAction;        // 死亡动作
    public BlockActionData blockAction;        // 格挡动作

    [Header("攻击配置")]
    public AttackActionData[] normalAttacks;      // 普通攻击序列
    public AttackActionData dashAttack;           // 冲刺攻击
    public AttackActionData jumpAttack;           // 跳跃攻击
    public AttackActionData parryAttack;           // 弹反攻击
    public AttackActionData specialAttack;       // 特殊攻击
    public AttackActionData heavyAttack;         // 重攻击


    // 获取连招序列
    public AttackActionData[] GetComboSequence()
    {
        return normalAttacks;
    }
}