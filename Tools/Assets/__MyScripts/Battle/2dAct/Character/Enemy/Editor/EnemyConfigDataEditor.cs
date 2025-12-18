using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyConfigData))]
public class EnemyConfigDataEditor : Editor
{
    private SerializedProperty enemyNameProp;
    private SerializedProperty difficultyProp;
    private SerializedProperty attributesProp;
    private SerializedProperty patrolAreaProp;
    private SerializedProperty idleTimeMinProp;
    private SerializedProperty idleTimeMaxProp;
    private SerializedProperty patrolSpeedProp;
    private SerializedProperty detectRangeTypeProp;
    private SerializedProperty detectRangeProp;
    private SerializedProperty detectWidthProp;
    private SerializedProperty detectHeightProp;
    private SerializedProperty targetLayersProp;
    private SerializedProperty chaseSpeedProp;
    private SerializedProperty loseTargetDistanceProp;
    private SerializedProperty attackActionsProp;
    private SerializedProperty attackRangeProp;
    private SerializedProperty attackCooldownProp;
    private SerializedProperty eliteAttackActionsProp;
    private SerializedProperty bossPhasesProp;
    private SerializedProperty bossAttackActionsProp;

    private void OnEnable()
    {
        enemyNameProp = serializedObject.FindProperty("enemyName");
        difficultyProp = serializedObject.FindProperty("difficulty");
        attributesProp = serializedObject.FindProperty("attributes");
        patrolAreaProp = serializedObject.FindProperty("patrolArea");
        idleTimeMinProp = serializedObject.FindProperty("idleTimeMin");
        idleTimeMaxProp = serializedObject.FindProperty("idleTimeMax");
        patrolSpeedProp = serializedObject.FindProperty("patrolSpeed");
        detectRangeTypeProp = serializedObject.FindProperty("detectRangeType");
        detectRangeProp = serializedObject.FindProperty("detectRange");
        detectWidthProp = serializedObject.FindProperty("detectWidth");
        detectHeightProp = serializedObject.FindProperty("detectHeight");
        targetLayersProp = serializedObject.FindProperty("targetLayers");
        chaseSpeedProp = serializedObject.FindProperty("chaseSpeed");
        loseTargetDistanceProp = serializedObject.FindProperty("loseTargetDistance");
        attackActionsProp = serializedObject.FindProperty("attackActions");
        attackRangeProp = serializedObject.FindProperty("attackRange");
        attackCooldownProp = serializedObject.FindProperty("attackCooldown");
        eliteAttackActionsProp = serializedObject.FindProperty("eliteAttackActions");
        bossAttackActionsProp = serializedObject.FindProperty("bossAttackActions");
        bossPhasesProp = serializedObject.FindProperty("bossPhases");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(enemyNameProp);
        EditorGUILayout.PropertyField(difficultyProp);

        EnemyDifficulty difficulty = (EnemyDifficulty)difficultyProp.enumValueIndex;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("角色属性", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(attributesProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("巡逻设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(patrolAreaProp);
        EditorGUILayout.PropertyField(idleTimeMinProp);
        EditorGUILayout.PropertyField(idleTimeMaxProp);
        EditorGUILayout.PropertyField(patrolSpeedProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("检测设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(detectRangeTypeProp);
        
        DetectRangeType rangeType = (DetectRangeType)detectRangeTypeProp.enumValueIndex;
        if (rangeType == DetectRangeType.Circle)
        {
            EditorGUILayout.PropertyField(detectRangeProp);
        }
        else
        {
            EditorGUILayout.PropertyField(detectWidthProp);
            EditorGUILayout.PropertyField(detectHeightProp);
        }
        
        EditorGUILayout.PropertyField(targetLayersProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("追击设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(chaseSpeedProp);
        EditorGUILayout.PropertyField(loseTargetDistanceProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("攻击设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(attackActionsProp, new GUIContent("基础攻击列表"));
        EditorGUILayout.PropertyField(attackRangeProp);
        EditorGUILayout.PropertyField(attackCooldownProp);

        if (difficulty == EnemyDifficulty.Elite || difficulty == EnemyDifficulty.Boss)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("精英特殊设置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("精英敌人可以使用基础攻击 + 精英攻击", MessageType.Info);
            EditorGUILayout.PropertyField(eliteAttackActionsProp, new GUIContent("精英额外攻击"));
        }

        if (difficulty == EnemyDifficulty.Boss)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Boss设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(bossAttackActionsProp);
            EditorGUILayout.HelpBox("Boss可以根据血量百分比切换到不同的阶段配置", MessageType.Info);
            EditorGUILayout.PropertyField(bossPhasesProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
