#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterLogic))]
public class CharacterLogicEditor : Editor
{
    private CharacterLogic characterLogic;
    private bool showAttackDebug = true;

    private void OnEnable()
    {
        characterLogic = (CharacterLogic)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            DrawAttackDebugInfo();
        }
    }

    private void DrawAttackDebugInfo()
    {
        showAttackDebug = EditorGUILayout.Foldout(showAttackDebug, "攻击系统调试信息", true);

        if (showAttackDebug)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField($"当前攻击状态: {characterLogic.CurrentState}");

            if (characterLogic.IsAttacking())
            {
                EditorGUILayout.LabelField($"攻击阶段: {characterLogic.currentAttackPhase}");
                EditorGUILayout.LabelField($"攻击计时: {characterLogic.currentAttackTimer:F2}s");
                EditorGUILayout.LabelField($"连招段数: {characterLogic.currentComboIndex}");

                if (characterLogic.currentAttackActionData != null)
                {
                    EditorGUILayout.LabelField($"攻击数据: {characterLogic.currentAttackActionData.acitonName}");
                    EditorGUILayout.LabelField($"前摇: {characterLogic.currentAttackActionData.windUpTime:F2}s");
                    EditorGUILayout.LabelField($"攻击中: {characterLogic.currentAttackActionData.activeTime:F2}s");
                    EditorGUILayout.LabelField($"后摇: {characterLogic.currentAttackActionData.recoveryTime:F2}s");
                }
            }

            EditorGUILayout.LabelField($"攻击输入缓冲: {characterLogic.hasBufferedAttack}");

            EditorGUI.indentLevel--;
        }
    }
}
#endif