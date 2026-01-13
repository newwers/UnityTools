#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileData))]
public class ProjectileDataEditor : Editor
{
    SerializedProperty prefabProp;
    SerializedProperty initialSpeedProp;
    SerializedProperty bodyTypeProp;
    SerializedProperty gravityScaleProp;
    private SerializedProperty directionOffsetProp;
    SerializedProperty lifetimeProp;
    SerializedProperty hitLayersProp;
    SerializedProperty maxHitTargetsProp;
    SerializedProperty isTriggerProp;
    SerializedProperty damageProp;
    SerializedProperty knockbackProp;
    SerializedProperty skillDataProp;
    SerializedProperty effectsProp;
    SerializedProperty hitVfxProp;
    SerializedProperty hitSfxProp;

    private void OnEnable()
    {
        prefabProp = serializedObject.FindProperty("prefab");
        hitLayersProp = serializedObject.FindProperty("hitLayers");
        bodyTypeProp = serializedObject.FindProperty("bodyType");
        lifetimeProp = serializedObject.FindProperty("lifetime");
        initialSpeedProp = serializedObject.FindProperty("initialSpeed");
        gravityScaleProp = serializedObject.FindProperty("gravityScale");
        directionOffsetProp = serializedObject.FindProperty("directionOffset");
        isTriggerProp = serializedObject.FindProperty("isTrigger");
        maxHitTargetsProp = serializedObject.FindProperty("maxHitTargets");
        damageProp = serializedObject.FindProperty("damage");
        knockbackProp = serializedObject.FindProperty("knockbackForce");
        skillDataProp = serializedObject.FindProperty("skillData");
        effectsProp = serializedObject.FindProperty("effects");
        hitVfxProp = serializedObject.FindProperty("hitVfxPrefab");
        hitSfxProp = serializedObject.FindProperty("hitSfx");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Projectile Data", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(prefabProp);

        if (prefabProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("建议填写预制体，编辑器提供一键校验预制体是否包含必要组件", MessageType.Info);
        }

        if (GUILayout.Button("Validate Prefab"))
        {
            ValidatePrefab((ProjectileData)target);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Physics / Movement", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(initialSpeedProp, new GUIContent("Initial Speed"));
        EditorGUILayout.PropertyField(bodyTypeProp, new GUIContent("BodyType"));
        EditorGUILayout.PropertyField(gravityScaleProp, new GUIContent("Gravity Scale"));
        EditorGUILayout.PropertyField(directionOffsetProp, new GUIContent("方向偏移"));
        EditorGUILayout.PropertyField(lifetimeProp, new GUIContent("Lifetime"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Collision / Hit", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(hitLayersProp, new GUIContent("Hit Layers"));
        EditorGUILayout.PropertyField(maxHitTargetsProp, new GUIContent("Max Hit Targets"));
        EditorGUILayout.PropertyField(isTriggerProp, new GUIContent("Is Trigger"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Damage / Effects", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(damageProp, new GUIContent("Damage"));
        EditorGUILayout.PropertyField(knockbackProp, new GUIContent("Knockback Force"));
        EditorGUILayout.PropertyField(skillDataProp, new GUIContent("Skill Data"));
        EditorGUILayout.PropertyField(effectsProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("VFX / SFX", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(hitVfxProp, new GUIContent("Hit VFX"));
        EditorGUILayout.PropertyField(hitSfxProp, new GUIContent("Hit SFX"));

        EditorGUILayout.Space();
        if (GUILayout.Button("Ping Data"))
        {
            EditorGUIUtility.PingObject(target);
        }

        if (GUILayout.Button("Ping Prefab"))
        {
            if (prefabProp.objectReferenceValue != null)
            {
                EditorGUIUtility.PingObject(prefabProp.objectReferenceValue);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ValidatePrefab(ProjectileData data)
    {
        if (data.prefab == null)
        {
            Debug.LogWarning("ProjectileData: prefab 未设置");
            return;
        }

        var prefabPath = AssetDatabase.GetAssetPath(data.prefab);
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (go == null)
        {
            Debug.LogWarning("无法加载预制体");
            return;
        }

        var collider = go.GetComponent<Collider2D>();
        var rb = go.GetComponent<Rigidbody2D>();
        var proj = go.GetComponent<ProjectileController>();

        string report = "Prefab Validation:\n";
        report += $"Has Collider2D: {(collider != null)}\n";
        report += $"Has Rigidbody2D: {(rb != null)}\n";
        report += $"Has ProjectileController: {(proj != null)}\n";

        Debug.Log(report);

        if (proj == null)
        {
            if (EditorUtility.DisplayDialog("Attach ProjectileController?", "Prefab 缺少 ProjectileController 组件，是否自动添加？（会修改预制体）", "Yes", "No"))
            {
                var instance = PrefabUtility.LoadPrefabContents(prefabPath);
                if (instance != null)
                {
                    if (instance.GetComponent<ProjectileController>() == null)
                    {
                        instance.AddComponent<ProjectileController>();
                        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                        Debug.Log("已在预制体上添加 ProjectileController");
                    }
                    PrefabUtility.UnloadPrefabContents(instance);
                }
            }
        }
    }
}
#endif
