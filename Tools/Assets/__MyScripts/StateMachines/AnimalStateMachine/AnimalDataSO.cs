using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimalData", menuName = "Animal System/Animal Data", order = 1)]
public class AnimalDataSO : ScriptableObject
{
    [Header("基本信息")]
    [FieldReadOnly]
    public string guid;
    public string animalName;
    public AnimalType animalType;

    public float collisionCheckRadius = 0.01f;


    private void OnEnable()
    {
        // 检查是否有重复的 ID
        var allConfigs = Resources.FindObjectsOfTypeAll<AnimalDataSO>();
        foreach (var config in allConfigs)
        {
            if (config != this && config.guid == this.guid)
            {
                // 如果有重复，生成新的 ID
                guid = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
                break;
            }
        }

        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(AnimalDataSO))]
[CanEditMultipleObjects]
public class AnimalDataSOEditor : Editor
{
    AnimalDataSO so;

    private void OnEnable()
    {
        so = target as AnimalDataSO;
    }

    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector界面
        DrawDefaultInspector();

        // 添加跳转到文件按钮
        if (GUILayout.Button("跳转到文件", GUILayout.Height(24)))
        {
            // 选中并高亮显示对应的 Asset 文件
            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }
    }
}
#endif