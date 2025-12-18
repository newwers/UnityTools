using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(SkillCooldownManager))]
public class SkillCooldownManagerEditor : Editor
{
    private const float REFRESH_INTERVAL = 0.1f;
    private double lastRefreshTime;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SkillCooldownManager manager = (SkillCooldownManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("技能冷却状态", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("运行时显示当前正在冷却的技能及其剩余时间", MessageType.Info);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("请在运行时查看冷却状态", MessageType.Warning);
            return;
        }

        DrawCooldownTimers(manager);

        if (EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
            Repaint();
        }
    }

    private void DrawCooldownTimers(SkillCooldownManager manager)
    {
        var cooldownTimers = GetCooldownTimers(manager);

        if (cooldownTimers == null || cooldownTimers.Count == 0)
        {
            EditorGUILayout.LabelField("当前没有技能在冷却中", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        foreach (var kvp in cooldownTimers)
        {
            AttackActionData attackData = kvp.Key;
            float endTime = kvp.Value;

            if (attackData == null)
                continue;

            float remainingTime = endTime - Time.time;
            if (remainingTime <= 0)
                continue;

            float totalCooldown = manager.GetCooldown(attackData);
            float progress = 1f - (remainingTime / totalCooldown);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(attackData.acitonName, GUILayout.Width(150));

            Rect progressRect = GUILayoutUtility.GetRect(100, 18, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(progressRect, progress, $"{remainingTime:F2}秒");

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"冷却中的技能数量: {cooldownTimers.Count}", EditorStyles.miniLabel);
    }

    private Dictionary<AttackActionData, float> GetCooldownTimers(SkillCooldownManager manager)
    {
        FieldInfo field = typeof(SkillCooldownManager).GetField("cooldownTimers", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(manager) as Dictionary<AttackActionData, float>;
    }
}
