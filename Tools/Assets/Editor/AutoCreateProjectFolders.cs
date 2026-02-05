using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity项目标准化文件夹一键创建工具
/// 支持创建指定的所有根文件夹及子文件夹结构
/// </summary>
public class AutoCreateProjectFolders : EditorWindow
{
    // 菜单路径：Unity顶部菜单栏 -> Tools -> 一键创建项目文件夹（自定义名称，方便查找）
    [MenuItem("Tools/一键创建项目文件夹", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<AutoCreateProjectFolders>("项目文件夹创建工具");
    }

    private void OnGUI()
    {
        GUILayout.Space(20);
        // 居中显示标题
        GUILayout.Label("Unity标准化文件夹结构", new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        });
        GUILayout.Space(30);

        // 一键创建按钮（占满窗口宽度，突出显示）
        if (GUILayout.Button("点击创建所有文件夹", GUILayout.Height(40)))
        {
            CreateAllProjectFolders();
            EditorUtility.DisplayDialog("创建成功", "所有项目文件夹已一键创建完成！", "确定");
            Debug.Log("<color=green>【文件夹创建工具】</color> 所有标准化文件夹创建成功！");
        }

        GUILayout.Space(20);
        // 提示文本
        GUILayout.Label("说明：\n1. 已存在的文件夹不会重复创建\n2. 所有文件夹创建在Project根目录\n3. 结构严格按指定要求生成",
            new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true });
    }

    /// <summary>
    /// 核心方法：创建所有指定的文件夹及子文件夹
    /// </summary>
    private static void CreateAllProjectFolders()
    {
        // ==================== 定义所有文件夹结构（根文件夹：子文件夹数组）====================
        var folderStructure = new Dictionary<string, string[]>
        {
            // Scripts 系列
            { "Scripts", new[] { "Core", "Game", "Editor" } },
            // Scenes 系列
            { "Scenes", new[] { "Levels", "UI", "Tests" } },
            // Prefabs 系列
            { "Prefabs", new[] { "UI", "Items" } },
            // Art 系列
            { "Art", new[] { "Models", "Materials", "Textures", "Sprites", "Animations" } },
            // Audio 系列
            { "Audio", new[] { "SFX", "Music", "UI" } },
            // Fonts 系列
            { "Fonts", new[] { "ttf", "asset" } },
            // 独立根文件夹（无子文件夹）
            { "Data", null },
            { "Resources", null },
            { "StreamingAssets", null },
            { "Plugins", null },
            { "Shaders", new[] { "ShaderGraph" } }
        };

        // 遍历创建所有文件夹
        foreach (var (rootFolder, subFolders) in folderStructure)
        {
            // 创建根文件夹
            CreateFolder(rootFolder);

            // 如果有子文件夹，遍历创建
            if (subFolders != null && subFolders.Length > 0)
            {
                foreach (var subFolder in subFolders)
                {
                    // 子文件夹路径：根文件夹/子文件夹
                    CreateFolder($"{rootFolder}/{subFolder}");
                }
            }
        }

        // 刷新Unity项目视图，确保文件夹立即显示
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 通用文件夹创建方法（自动判断是否存在，避免重复创建）
    /// </summary>
    /// <param name="folderPath">文件夹相对路径（基于Project根目录）</param>
    private static void CreateFolder(string folderPath)
    {
        // 构建完整的Assets路径用于AssetDatabase操作
        string fullAssetPath = folderPath.StartsWith("Assets/") ? folderPath : $"Assets/{folderPath}";

        // 判断文件夹是否不存在，不存在则创建
        if (!AssetDatabase.IsValidFolder(fullAssetPath))
        {
            // AssetDatabase.CreateFolder 要求拆分「父路径」和「文件夹名称」
            string parentPath = Path.GetDirectoryName(folderPath);
            string folderName = Path.GetFileName(folderPath);

            // 处理根目录情况（父路径为空时，父路径设为"Assets"，Unity项目根目录对应Assets）
            if (string.IsNullOrEmpty(parentPath))
            {
                parentPath = "Assets";
            }
            else
            {
                // 确保父路径也使用Assets前缀
                parentPath = parentPath.StartsWith("Assets/") ? parentPath : $"Assets/{parentPath}";
            }

            // 创建文件夹并获取结果
            string result = AssetDatabase.CreateFolder(parentPath, folderName);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log($"创建文件夹成功：{folderPath}");
            }
            else
            {
                Debug.LogWarning($"创建文件夹失败：{folderPath}");
            }
        }
        else
        {
            Debug.Log($"文件夹已存在，跳过创建：{folderPath}");
        }
    }
}