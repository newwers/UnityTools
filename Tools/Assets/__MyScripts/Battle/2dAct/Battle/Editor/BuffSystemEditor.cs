#if UNITY_EDITOR


using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuffSystem))]
public class BuffSystemEditor : Editor
{
    private BuffSystem buffSystem;
    private Vector2 scrollPosition;
    private bool showBuffDetails = true;
    private bool showPeriodicEffects = true;
    private bool showImmunities = true;

    public override void OnInspectorGUI()
    {
        buffSystem = target as BuffSystem;

        // 显示默认Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Buff系统调试信息", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("请在运行模式下查看Buff信息", MessageType.Info);
            return;
        }

        // 获取激活的Buff列表
        var activeBuffs = buffSystem.GetActiveBuffs();

        // 显示基本信息
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField($"激活Buff数量: {activeBuffs.Count}", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Buff详情折叠区域
        showBuffDetails = EditorGUILayout.Foldout(showBuffDetails, "激活Buff详情", true);
        if (showBuffDetails)
        {
            DisplayBuffList(activeBuffs);
        }

        EditorGUILayout.Space();

        // 周期性效果信息
        showPeriodicEffects = EditorGUILayout.Foldout(showPeriodicEffects, "周期性效果状态", true);
        if (showPeriodicEffects)
        {
            DisplayPeriodicEffects(activeBuffs);
        }

        EditorGUILayout.Space();

        // 免疫状态信息
        showImmunities = EditorGUILayout.Foldout(showImmunities, "免疫状态", true);
        if (showImmunities)
        {
            DisplayImmunities();
        }

        EditorGUILayout.Space();

        // 操作按钮
        DisplayActionButtons();
    }

    /// <summary>
    /// 显示Buff列表
    /// </summary>
    private void DisplayBuffList(List<ActiveBuff> activeBuffs)
    {
        if (activeBuffs.Count == 0)
        {
            EditorGUILayout.HelpBox("没有激活的Buff", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            var buff = activeBuffs[i];
            DisplayBuffItem(buff, i);
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 显示单个Buff项
    /// </summary>
    private void DisplayBuffItem(ActiveBuff buff, int index)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // Buff名称和层数
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{index + 1}. {buff.data.effectName}", EditorStyles.boldLabel);
        if (buff.currentStacks > 1)
        {
            EditorGUILayout.LabelField($"x{buff.currentStacks}", GUILayout.Width(40));
        }
        EditorGUILayout.EndHorizontal();

        // 分类和持续时间
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"分类: {buff.data.category}", GUILayout.Width(150));

        string durationText = buff.data.isPermanent ? "永久" :
            buff.remainingDuration > 0 ? $"{buff.remainingDuration:F1}秒" : "立即生效";
        EditorGUILayout.LabelField($"持续时间: {durationText}");
        EditorGUILayout.EndHorizontal();

        // 来源信息
        if (buff.source != null)
        {
            EditorGUILayout.LabelField($"来源: {buff.source.name}");
        }

        // 参数信息
        if (buff.data.parameters != null && buff.data.parameters.Count > 0)
        {
            EditorGUILayout.LabelField("参数:", EditorStyles.miniBoldLabel);
            foreach (var param in buff.data.parameters)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  {param.name}: {param.value}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    /// <summary>
    /// 显示周期性效果状态
    /// </summary>
    private void DisplayPeriodicEffects(List<ActiveBuff> activeBuffs)
    {
        var periodicBuffs = activeBuffs.FindAll(b =>
            b.data.category == EffectCategory.Burn ||
            b.data.category == EffectCategory.Poison ||
            b.data.category == EffectCategory.Bleed ||
            b.data.category == EffectCategory.ConditionalHeal ||
            b.data.category == EffectCategory.EnergyDrain ||
            b.data.category == EffectCategory.EnergyRegeneration);

        if (periodicBuffs.Count == 0)
        {
            EditorGUILayout.HelpBox("没有周期性效果", MessageType.Info);
            return;
        }

        foreach (var buff in periodicBuffs)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(buff.data.effectName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"累积时间: {buff.accumulatedTime:F2}秒", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            // 显示周期性效果的具体参数
            switch (buff.data.category)
            {
                case EffectCategory.Burn:
                case EffectCategory.Poison:
                case EffectCategory.Bleed:
                    float dps = buff.data.GetParameterValue("damagePerSecond") * buff.currentStacks;
                    EditorGUILayout.LabelField($"每秒伤害: {dps}");
                    break;

                case EffectCategory.ConditionalHeal:
                    float hps = buff.data.GetParameterValue("healPerSecond");
                    EditorGUILayout.LabelField($"每秒治疗: {hps}");
                    break;

                case EffectCategory.EnergyDrain:
                    float drain = buff.data.GetParameterValue("drainPerSecond");
                    EditorGUILayout.LabelField($"每秒能量消耗: {drain}");
                    break;

                case EffectCategory.EnergyRegeneration:
                    float regen = buff.data.GetParameterValue("energyPerSecond");
                    EditorGUILayout.LabelField($"每秒能量回复: {regen}");
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    /// <summary>
    /// 显示免疫状态
    /// </summary>
    private void DisplayImmunities()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        bool stunImmune = buffSystem.HasBuff(EffectCategory.StunImmunity);
        bool invincibleImmune = buffSystem.HasBuff(EffectCategory.InvincibleImmunity);
        bool superArmorImmune = buffSystem.HasBuff(EffectCategory.SuperArmorImmunity);

        EditorGUILayout.LabelField("眩晕免疫: " + (stunImmune ? "是" : "否"));
        EditorGUILayout.LabelField("无敌免疫: " + (invincibleImmune ? "是" : "否"));
        EditorGUILayout.LabelField("霸体免疫: " + (superArmorImmune ? "是" : "否"));

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 显示操作按钮
    /// </summary>
    private void DisplayActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("清除所有Buff"))
        {
            buffSystem.ClearAllBuffs();
            EditorUtility.SetDirty(buffSystem);
        }

        if (GUILayout.Button("刷新显示"))
        {
            Repaint();
        }

        EditorGUILayout.EndHorizontal();
    }
}

/// <summary>
/// 为ActiveBuff类添加自定义PropertyDrawer，使其在Inspector中显示更友好
/// </summary>
[CustomPropertyDrawer(typeof(ActiveBuff))]
public class ActiveBuffDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 绘制背景
        position.height = EditorGUIUtility.singleLineHeight;
        Rect bgRect = position;
        bgRect.height = GetPropertyHeight(property, label) - 2;
        EditorGUI.DrawRect(bgRect, new Color(0.2f, 0.2f, 0.2f, 0.1f));

        // 绘制折叠标签
        var dataProp = property.FindPropertyRelative("data");
        var durationProp = property.FindPropertyRelative("remainingDuration");
        var stacksProp = property.FindPropertyRelative("currentStacks");

        string displayName = "未知Buff";
        if (dataProp.objectReferenceValue != null)
        {
            var effectData = dataProp.objectReferenceValue as EffectData;
            displayName = effectData.effectName;
        }

        string durationText = durationProp.floatValue > 0 ? $"{durationProp.floatValue:F1}秒" : "永久";
        string stackText = stacksProp.intValue > 1 ? $" x{stacksProp.intValue}" : "";

        label.text = $"{displayName} ({durationText}){stackText}";

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            position.y += EditorGUIUtility.singleLineHeight + 2;

            // 缩进显示详细属性
            EditorGUI.indentLevel++;

            // 显示data
            Rect dataRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(dataRect, dataProp);
            position.y += EditorGUIUtility.singleLineHeight + 2;

            // 显示duration
            Rect durationRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(durationRect, durationProp);
            position.y += EditorGUIUtility.singleLineHeight + 2;

            // 显示stacks
            Rect stacksRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(stacksRect, stacksProp);
            position.y += EditorGUIUtility.singleLineHeight + 2;

            // 显示source（如果存在）
            var sourceProp = property.FindPropertyRelative("source");
            if (sourceProp.objectReferenceValue != null)
            {
                Rect sourceRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(sourceRect, sourceProp);
                position.y += EditorGUIUtility.singleLineHeight + 2;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 2;

        if (property.isExpanded)
        {
            height += (EditorGUIUtility.singleLineHeight + 2) * 4; // data, duration, stacks, source

            var sourceProp = property.FindPropertyRelative("source");
            if (sourceProp.objectReferenceValue == null)
            {
                height -= (EditorGUIUtility.singleLineHeight + 2);
            }
        }

        return height;
    }
}
#endif