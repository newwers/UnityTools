using System;
using UnityEngine;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 为方法添加按钮特性，在 Inspector 中显示可点击按钮
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class FunctionButtonAttribute : PropertyAttribute
{
    public readonly string buttonName;
    public readonly ButtonMode mode;

    /// <summary>
    /// 创建按钮特性
    /// </summary>
    /// <param name="name">按钮显示文本</param>
    /// <param name="mode">按钮模式 (默认 = 标准按钮)</param>
    public FunctionButtonAttribute(string name = "", ButtonMode mode = ButtonMode.Normal)
    {
        this.buttonName = name;
        this.mode = mode;
    }
}

/// <summary>
/// 按钮模式选项
/// </summary>
public enum ButtonMode
{
    Normal,      // 标准按钮 (默认)
    Wide,        // 宽按钮 (高度不变)
    Tall,        // 高按钮 (宽度不变)
    Big,         // 大按钮 (高度加倍)
    Warning,     // 警告按钮 (黄色)
    Danger,      // 危险按钮 (红色)
    Success,     // 成功按钮 (绿色)
    Info         // 信息按钮 (蓝色)
}

#if UNITY_EDITOR


[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class FunctionButtonDrawer : Editor
{
    private const float WideButtonHeight = 40f;
    private const float BigButtonHeight = 60f;

    public override void OnInspectorGUI()
    {
        // 先绘制默认的Inspector内容
        base.OnInspectorGUI();

        // 为每个选中的对象绘制按钮
        foreach (var obj in targets)
        {
            if (obj is MonoBehaviour mono)
            {
                DrawButtonsForMonoBehaviour(mono);
            }
        }
    }

    private void DrawButtonsForMonoBehaviour(MonoBehaviour target)
    {
        var methods = target.GetType().GetMethods(
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic);

        // 绘制标题分隔符
        bool hasButtons = false;

        foreach (var method in methods)
        {
            var buttonAttr = (FunctionButtonAttribute)Attribute.GetCustomAttribute(method, typeof(FunctionButtonAttribute));
            if (buttonAttr == null) continue;

            if (!hasButtons)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Custom Actions", EditorStyles.boldLabel);
                hasButtons = true;
            }

            DrawButtonForMethod(buttonAttr, method, target);
        }

        if (hasButtons)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }

    private void DrawButtonForMethod(FunctionButtonAttribute attr, MethodInfo method, MonoBehaviour target)
    {
        // 获取按钮文本
        string buttonText = string.IsNullOrWhiteSpace(attr.buttonName)
            ? ObjectNames.NicifyVariableName(method.Name)
            : attr.buttonName;

        // 获取按钮尺寸
        float height = GetButtonHeight(attr.mode);

        // 计算并应用按钮样式
        GUIStyle buttonStyle = GetButtonStyle(attr.mode, height);

        // 绘制按钮布局
        if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(height)))
        {
            try
            {
                // 调用方法
                method.Invoke(target, null);

                // 保存场景变更（如果是编辑器状态下的方法）
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"执行按钮方法时出错: {method.Name}\n{ex}");
            }
        }
    }

    private float GetButtonHeight(ButtonMode mode)
    {
        return mode switch
        {
            ButtonMode.Tall => WideButtonHeight,
            ButtonMode.Big => BigButtonHeight,
            _ => EditorGUIUtility.singleLineHeight
        };
    }

    private GUIStyle GetButtonStyle(ButtonMode mode, float height)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = (height > EditorGUIUtility.singleLineHeight) ? 14 : 12,
            alignment = TextAnchor.MiddleCenter
        };

        // 根据模式应用样式
        switch (mode)
        {
            case ButtonMode.Wide:
                break;

            case ButtonMode.Tall:
                style.fixedHeight = height;
                break;

            case ButtonMode.Big:
                style.fontSize = 15;
                style.fixedHeight = height;
                break;

            case ButtonMode.Warning:
                ApplyColorStyle(style, new Color(0.96f, 0.75f, 0.18f));
                break;

            case ButtonMode.Danger:
                ApplyColorStyle(style, new Color(0.91f, 0.31f, 0.23f));
                break;

            case ButtonMode.Success:
                ApplyColorStyle(style, new Color(0.29f, 0.69f, 0.31f));
                break;

            case ButtonMode.Info:
                ApplyColorStyle(style, new Color(0.23f, 0.56f, 0.91f));
                break;
        }

        return style;
    }

    private void ApplyColorStyle(GUIStyle style, Color color)
    {
        var normalTex = MakeTex(2, 2, color);
        var activeTex = MakeTex(2, 2, Color.Lerp(color, Color.black, 0.1f));
        var hoverTex = MakeTex(2, 2, Color.Lerp(color, Color.white, 0.1f));

        style.normal.background = normalTex;
        style.active.background = activeTex;
        style.hover.background = hoverTex;
        style.normal.textColor = Color.white;
        style.active.textColor = Color.white;
        style.hover.textColor = Color.white;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}

#endif
