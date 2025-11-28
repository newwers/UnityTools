using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Skill", menuName = "Character System/Skill/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("基础设置")]
    public string SkillName;
    [Tooltip("效果图标，用于UI显示")]
    public Sprite icon;

    public float EnergyCost = 10f;
    public float Cooldown = 1f;

    [Header("伤害设置")]
    public float baseDamage = 10f;
    [Header("击退")]
    public Vector2 knockbackForce = Vector2.zero;

    [Header("破防力相关")]
    public float breakPower = 0f;
    public float staggerDuration = 0f;

    [Header("暴击设置")]
    public bool isGuaranteedCrit = false;
    public int guaranteedCritOnHitCount = 0;

    [Header("生命偷取")]
    [Range(0, 100)] public float lifeStealPercent = 0f;
    [Tooltip("每次攻击A%概率回复造成伤害B%的生命值")]
    [Range(0, 100)] public float lifeStealChance = 0f;
    [Range(0, 100)] public float lifeStealAmount = 0f;

    [Header("施加效果,技能释放前和释放后")]
    [Tooltip("技能释放前触发的效果列表（例如：自身增益、消耗buff等）")]
    public List<EffectData> effectsOnCast = new List<EffectData>();

    [Tooltip("技能释放后触发的效果列表（例如：清除debuff、额外奖励等）")]
    public List<EffectData> effectsOnComplete = new List<EffectData>();

    [Header("视觉效果")]
    public GameObject vfxPrefab;
    public AudioClip sfxClip;

}
