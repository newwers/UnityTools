#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// 玩家属性组件，负责管理血量等数值
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BuffSystem))]
public class PlayerAttributes : MonoBehaviour
{
    public CharacterHealthBar healthBar;
    public CharacterAttributes characterAtttibute;

    /// <summary>
    /// 是否死亡（仅根据生命值判断）
    /// </summary>
    public bool IsDead => characterAtttibute.currentHealth <= 0;

    private void Update()
    {
        characterAtttibute.UpdatePerFrame(Time.deltaTime);
    }

    /// <summary>
    /// 初始化属性（用于开局或重生）
    /// </summary>
    public void Initialize()
    {
        characterAtttibute.Initialize();

        characterAtttibute.currentHealth = Mathf.Clamp(characterAtttibute.currentHealth, 0, characterAtttibute.maxHealth);
        if (characterAtttibute.currentHealth == 0)
        {
            characterAtttibute.currentHealth = characterAtttibute.maxHealth;
        }

        characterAtttibute.currentEnergy = Mathf.Clamp(characterAtttibute.currentEnergy, 0, characterAtttibute.maxEnergy);
        if (characterAtttibute.currentEnergy == 0)
        {
            characterAtttibute.currentEnergy = characterAtttibute.maxEnergy;
        }

        healthBar?.Initialize(characterAtttibute, transform);
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(PlayerAttributes))]
public class PlayerAttributesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerAttributes playerAttributes = (PlayerAttributes)target;

        if (playerAttributes.characterAtttibute == null)
        {
            EditorGUILayout.HelpBox("CharacterAttributes reference is missing!", MessageType.Error);
            DrawDefaultInspector();
            return;
        }

        // 绘制默认的序列化字段
        SerializedProperty healthBar = serializedObject.FindProperty("healthBar");
        EditorGUILayout.PropertyField(healthBar);

        SerializedProperty characterAttrProp = serializedObject.FindProperty("characterAtttibute");
        EditorGUILayout.PropertyField(characterAttrProp);


        if (!playerAttributes.characterAtttibute.Equals(null))
        {
            DrawFinalValues(playerAttributes.characterAtttibute);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFinalValues(CharacterAttributes attributes)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("最终属性数值", EditorStyles.boldLabel);

        // 生命相关
        EditorGUILayout.LabelField("生命相关", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"当前生命值: {attributes.currentHealth:F1} / {attributes.FinalMaxHealth:F1}");
        EditorGUILayout.LabelField($"生命恢复: {attributes.healthRegenRate:F1}/秒");

        // 能量相关
        EditorGUILayout.LabelField("能量相关", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"当前能量: {attributes.currentEnergy:F1} / {attributes.FinalMaxEnergy:F1}");
        EditorGUILayout.LabelField($"能量恢复: {attributes.energyRegenRate:F1}/秒");

        // 基础属性
        EditorGUILayout.LabelField("基础属性", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"力量: {attributes.FinalStrength:F1} (基础: {attributes.BaseStrength:F1})");
        EditorGUILayout.LabelField($"敏捷: {attributes.FinalAgility:F1} (基础: {attributes.BaseAgility:F1})");
        EditorGUILayout.LabelField($"移动速度: {attributes.FinalMoveSpeed:F1} (基础: {attributes.BaseMoveSpeed:F1})");
        EditorGUILayout.LabelField($"攻击速度倍率: {attributes.GetAttackSpeedMultiplier():F2}x");
        EditorGUILayout.LabelField($"施法速度: {attributes.GetCastSpeed():F2}");

        // 暴击相关
        EditorGUILayout.LabelField("暴击相关", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"暴击率: {attributes.FinalCritRate:F1}% (基础: {attributes.BaseCritRate:F1}%)");
        EditorGUILayout.LabelField($"暴击倍率: {attributes.FinalCritMultiplier:F1}x (基础: {attributes.BaseCritMultiplier:F1}x)");

        // 防御属性
        EditorGUILayout.LabelField("防御属性", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"防御力: {attributes.FinalDefense} (基础: {attributes.baseDefense})");
        EditorGUILayout.LabelField($"格挡值: {attributes.currentBlockValue:F1} / {attributes.maxBlockValue:F1}");
        EditorGUILayout.LabelField($"护盾: {attributes.currentShield:F1} / {attributes.maxShield:F1}");

        // 状态信息
        EditorGUILayout.LabelField("状态信息", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"闪避点数: {attributes.currentDodgeCount} / {attributes.maxDodgeCount}");
        EditorGUILayout.LabelField($"震击值: {attributes.currentStagger:F1} / {attributes.maxStagger:F1}");
        EditorGUILayout.Toggle("格挡中", attributes.isBlocking);
        EditorGUILayout.Toggle("闪避中", attributes.isDodging);
        EditorGUILayout.Toggle("震击状态", attributes.isStaggered);
        EditorGUILayout.Toggle("霸体", attributes.hasSuperArmor);
        EditorGUILayout.IntField("无敌计数", attributes.isInvincible);

        // 活跃修改器数量
        EditorGUILayout.LabelField($"活跃属性修改器: 通过代码访问", EditorStyles.miniLabel);
    }
}
#endif


