using SteamSDK;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SteamAchievementEditorWindow : EditorWindow
{
    private List<SteamAchievementDataSO> achievementSOs = new List<SteamAchievementDataSO>();
    private Vector2 scrollPosition;
    private int selectedIndex = -1;
    private SteamAchievementData tempAchievement = new SteamAchievementData();
    private bool isEditing = false;
    private SteamAchievementManager achievementManager;

    [MenuItem("Tools/Steam SDK/Achievement Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<SteamAchievementEditorWindow>("Steam Achievement Editor");
        window.minSize = new Vector2(600, 500);
    }

    void OnGUI()
    {
        // 初始化成就管理器
        if (achievementManager == null && Application.isPlaying)
        {
            achievementManager = SteamAchievementManager.Instance;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Steam Achievement Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Use this window to configure Steam achievements for your game", MessageType.Info);
        EditorGUILayout.Space(10);

        // Load Achievement SOs
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Achievement Data SOs", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Load All Achievement SOs", GUILayout.Height(25)))
        {
            LoadAllAchievementSOs();
        }

        if (GUILayout.Button("Create New Achievement SO", GUILayout.Height(25)))
        {
            CreateNewAchievementSO();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        if (achievementSOs.Count == 0)
        {
            EditorGUILayout.HelpBox("No SteamAchievementDataSO assets found. Click 'Load All Achievement SOs' to load existing ones or 'Create New Achievement SO' to create a new one.", MessageType.Warning);
            return;
        }

        // Achievement List
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Achievements", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        for (int i = 0; i < achievementSOs.Count; i++)
        {
            var achievementSO = achievementSOs[i];
            if (achievementSO == null)
                continue;

            var achievement = achievementSO.AchievementData;
            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Toggle(selectedIndex == i, "", GUILayout.Width(20)))
            {
                selectedIndex = i;
                tempAchievement = new SteamAchievementData(
                    achievement.AchievementId,
                    achievement.AchievementName,
                    achievement.Description,
                    achievement.AssociatedVariableName);
                tempAchievement.IsAchieved = achievement.IsAchieved;
                isEditing = true;
            }

            EditorGUILayout.LabelField(achievement.AchievementId, EditorStyles.boldLabel, GUILayout.MinWidth(50));
            EditorGUILayout.LabelField(achievement.AchievementName, GUILayout.MinWidth(50));
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField($"{achievementSO.GetRunTimeValue()}/{achievementSO.requiredNum}", GUILayout.MinWidth(50));
            }
            else
            {
                EditorGUILayout.LabelField($"0/{achievementSO.requiredNum}", GUILayout.MinWidth(50));
            }
            EditorGUILayout.LabelField(achievement.AssociatedVariableName, GUILayout.MinWidth(50));
            EditorGUILayout.LabelField(achievement.IsAchieved ? "Achieved" : "Not Achieved", GUILayout.MinWidth(30));

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                //选择成就SO资产
                Selection.activeObject = achievementSO;

            }

            if (GUILayout.Button("Edit", GUILayout.Width(50)))
            {
                selectedIndex = i;
                tempAchievement = new SteamAchievementData(
                    achievement.AchievementId,
                    achievement.AchievementName,
                    achievement.Description,
                    achievement.AssociatedVariableName);
                tempAchievement.IsAchieved = achievement.IsAchieved;
                isEditing = true;
            }

            if (GUILayout.Button("Unlock", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Unlock Achievement", "Are you sure you want to unlock this achievement?", "Yes", "No"))
                {
                    // 使用SteamAchievementManager解锁成就
                    achievementManager?.UnlockAchievement(achievement.AchievementId);
                    // 更新UI显示
                    achievement.IsAchieved = true;
                    EditorUtility.SetDirty(achievementSO);
                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("Reset", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Reset Achievement", "Are you sure you want to reset this achievement?", "Yes", "No"))
                {
                    // 使用SteamAchievementManager重置成就
                    achievementManager?.ResetAchievement(achievement.AchievementId);
                    // 更新UI显示
                    achievement.IsAchieved = false;
                    EditorUtility.SetDirty(achievementSO);
                    AssetDatabase.SaveAssets();
                }
            }

            if (GUILayout.Button("Delete", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Delete Achievement", "Are you sure you want to delete this achievement?", "Yes", "No"))
                {
                    DeleteAchievementSO(achievementSO);
                    achievementSOs.RemoveAt(i);
                    selectedIndex = -1;
                    isEditing = false;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        // Achievement Editor
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(isEditing ? "Edit Achievement" : "Add New Achievement", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        tempAchievement.AchievementId = EditorGUILayout.TextField("Achievement ID", tempAchievement.AchievementId);
        tempAchievement.AchievementName = EditorGUILayout.TextField("Achievement Name", tempAchievement.AchievementName);
        tempAchievement.Description = EditorGUILayout.TextArea(tempAchievement.Description, GUILayout.Height(60));
        tempAchievement.AssociatedVariableName = EditorGUILayout.TextField("Associated Variable Name", tempAchievement.AssociatedVariableName);
        tempAchievement.IsAchieved = EditorGUILayout.Toggle("Is Achieved", tempAchievement.IsAchieved);

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (isEditing)
        {
            if (GUILayout.Button("Save Changes", GUILayout.Width(120), GUILayout.Height(30)))
            {
                SaveChanges();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
            {
                CancelEditing();
            }
        }
        else
        {
            if (GUILayout.Button("Add Achievement", GUILayout.Width(120), GUILayout.Height(30)))
            {
                AddAchievement();
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        // Utility Buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset All Achievements", GUILayout.Width(150), GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset All Achievements", "Are you sure you want to reset all achievements?", "Yes", "No"))
            {
                ResetAllAchievements();
            }
        }

        if (GUILayout.Button("Validate All Achievements", GUILayout.Width(150), GUILayout.Height(30)))
        {
            ValidateAllAchievements();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void LoadAllAchievementSOs()
    {
        achievementSOs.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SteamAchievementDataSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SteamAchievementDataSO achievementSO = AssetDatabase.LoadAssetAtPath<SteamAchievementDataSO>(path);
            if (achievementSO != null)
            {
                achievementSOs.Add(achievementSO);
            }
        }
    }

    void CreateNewAchievementSO()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Achievement Data SO",
            "SteamAchievementDataSO",
            "asset",
            "Select a location to create the new achievement data SO asset");

        if (!string.IsNullOrEmpty(path))
        {
            var achievementSO = CreateInstance<SteamAchievementDataSO>();
            AssetDatabase.CreateAsset(achievementSO, path);
            AssetDatabase.SaveAssets();
            achievementSOs.Add(achievementSO);
        }
    }

    void AddAchievement()
    {
        try
        {
            tempAchievement.Validate();
            CreateNewAchievementSOWithData(tempAchievement);
            ResetTempAchievement();
        }
        catch (ArgumentException ex)
        {
            EditorUtility.DisplayDialog("Error", ex.Message, "OK");
        }
    }

    void CreateNewAchievementSOWithData(SteamAchievementData data)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Achievement Data SO",
            "SteamAchievementDataSO_" + data.AchievementId,
            "asset",
            "Select a location to create the new achievement data SO asset");

        if (!string.IsNullOrEmpty(path))
        {
            var achievementSO = CreateInstance<SteamAchievementDataSO>();
            achievementSO.AchievementData = new SteamAchievementData(
                data.AchievementId,
                data.AchievementName,
                data.Description,
                data.AssociatedVariableName);
            achievementSO.AchievementData.IsAchieved = data.IsAchieved;
            AssetDatabase.CreateAsset(achievementSO, path);
            AssetDatabase.SaveAssets();
            achievementSOs.Add(achievementSO);
        }
    }

    new void SaveChanges()
    {
        if (selectedIndex >= 0 && selectedIndex < achievementSOs.Count)
        {
            try
            {
                tempAchievement.Validate();
                var achievementSO = achievementSOs[selectedIndex];
                achievementSO.AchievementData = new SteamAchievementData(
                    tempAchievement.AchievementId,
                    tempAchievement.AchievementName,
                    tempAchievement.Description,
                    tempAchievement.AssociatedVariableName);
                achievementSO.AchievementData.IsAchieved = tempAchievement.IsAchieved;
                EditorUtility.SetDirty(achievementSO);
                AssetDatabase.SaveAssets();
                CancelEditing();
            }
            catch (ArgumentException ex)
            {
                EditorUtility.DisplayDialog("Error", ex.Message, "OK");
            }
        }
    }

    void DeleteAchievementSO(SteamAchievementDataSO achievementSO)
    {
        if (achievementSO == null)
            return;

        string path = AssetDatabase.GetAssetPath(achievementSO);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
    }

    void CancelEditing()
    {
        selectedIndex = -1;
        isEditing = false;
        ResetTempAchievement();
    }

    void ResetTempAchievement()
    {
        tempAchievement = new SteamAchievementData();
    }

    void ValidateAllAchievements()
    {
        try
        {
            foreach (var achievementSO in achievementSOs)
            {
                if (achievementSO != null)
                {
                    achievementSO.AchievementData.Validate();
                }
            }
            EditorUtility.DisplayDialog("Validation Success", "All achievements are valid", "OK");
        }
        catch (ArgumentException ex)
        {
            EditorUtility.DisplayDialog("Validation Error", ex.Message, "OK");
        }
    }

    void ResetAllAchievements()
    {
        // 使用SteamAchievementManager重置所有成就
        achievementManager?.ResetAllAchievement();

        // 更新UI显示
        foreach (var achievementSO in achievementSOs)
        {
            if (achievementSO != null)
            {
                achievementSO.AchievementData.IsAchieved = false;
                EditorUtility.SetDirty(achievementSO);
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Reset Success", "All achievements have been reset", "OK");
    }
}