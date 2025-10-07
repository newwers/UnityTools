using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BulkRenameWindow : EditorWindow
{
    private string baseName = "NewName";
    private int startNumber = 1;
    private int numberDigits = 2;
    private bool keepOriginalExtension = true;
    private Vector2 scrollPosition;

    [MenuItem("Tools/批量重命名工具")]
    public static void ShowWindow()
    {
        GetWindow<BulkRenameWindow>("批量重命名");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("批量重命名工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 显示当前选中对象数量
        int selectedCount = Selection.objects.Length;
        EditorGUILayout.LabelField($"已选择 {selectedCount} 个对象");

        EditorGUILayout.Space();

        // 基础名称输入
        baseName = EditorGUILayout.TextField("基础名称", baseName);

        // 起始编号
        startNumber = EditorGUILayout.IntField("起始编号", startNumber);

        // 编号位数
        numberDigits = EditorGUILayout.IntSlider("编号位数", numberDigits, 1, 5);

        // 保持原始扩展名
        keepOriginalExtension = EditorGUILayout.Toggle("保持原始扩展名", keepOriginalExtension);

        EditorGUILayout.Space();

        // 预览区域
        EditorGUILayout.LabelField("预览:", EditorStyles.boldLabel);

        if (selectedCount > 0)
        {
            EditorGUI.BeginDisabledGroup(true);
            for (int i = 0; i < Mathf.Min(selectedCount, 5); i++)
            {
                string newName = GenerateNewName(i);
                EditorGUILayout.TextField(newName);
            }

            if (selectedCount > 5)
            {
                EditorGUILayout.LabelField("... 还有 " + (selectedCount - 5) + " 个");
            }
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.HelpBox("请在Project窗口中选择要重命名的对象", MessageType.Info);
        }

        EditorGUILayout.Space();

        // 重命名按钮
        EditorGUI.BeginDisabledGroup(selectedCount == 0);
        if (GUILayout.Button("应用重命名", GUILayout.Height(30)))
        {
            BulkRename();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("注意: 重命名操作无法撤销，请谨慎操作!", MessageType.Warning);

        EditorGUILayout.EndScrollView();
    }

    private string GenerateNewName(int index)
    {
        string numberPart = (startNumber + index).ToString().PadLeft(numberDigits, '0');
        string extension = "";

        if (keepOriginalExtension && Selection.objects[index] != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.objects[index]);
            extension = Path.GetExtension(path);
        }

        return $"{baseName}_{numberPart}{extension}";
    }

    private void BulkRename()
    {
        if (Selection.objects.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "请先选择要重命名的对象", "确定");
            return;
        }

        // 确认对话框
        if (!EditorUtility.DisplayDialog("确认重命名",
            $"确定要重命名 {Selection.objects.Length} 个对象吗?\n此操作无法撤销。",
            "确定", "取消"))
        {
            return;
        }

        // 获取排序后的选择对象(按名称)
        var sortedObjects = Selection.objects.OrderBy(obj => obj.name).ToArray();

        // 开始资产操作(批量处理，提高性能)
        AssetDatabase.StartAssetEditing();

        try
        {
            for (int i = 0; i < sortedObjects.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(sortedObjects[i]);
                string newName = GenerateNewName(i);

                // 重命名资源
                string error = AssetDatabase.RenameAsset(path, newName);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"重命名失败: {path} -> {error}");
                }
                else
                {
                    Debug.Log($"重命名成功: {path} -> {newName}");
                }

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar(
                    "批量重命名",
                    $"重命名对象 {i + 1}/{sortedObjects.Length}",
                    (float)i / sortedObjects.Length))
                {
                    break;
                }
            }
        }
        finally
        {
            // 确保始终停止资产编辑
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();

            // 刷新AssetDatabase
            AssetDatabase.Refresh();

            Debug.Log("批量重命名完成");
        }
    }
}