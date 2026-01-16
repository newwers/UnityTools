using System;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ShowIf特性
/// 用于在Unity编辑器中根据其他字段的值来显示或隐藏字段
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ShowIfAttribute : PropertyAttribute
{
    /// <summary>
    /// 要检查的字段名称
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// 字段需要匹配的值
    /// </summary>
    public object MatchValue { get; private set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fieldName">要检查的字段名称</param>
    /// <param name="matchValue">字段需要匹配的值</param>
    public ShowIfAttribute(string fieldName, object matchValue)
    {
        FieldName = fieldName;
        MatchValue = matchValue;
    }
}

#if UNITY_EDITOR

/// <summary>
/// ShowIf特性的编辑器处理类
/// 用于在Inspector中根据其他字段的值来显示或隐藏字段
/// </summary>
[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfPropertyDrawer : PropertyDrawer
{
    /// <summary>
    /// 绘制属性
    /// </summary>
    /// <param name="position">绘制位置</param>
    /// <param name="property">属性</param>
    /// <param name="label">标签</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIfAttribute = attribute as ShowIfAttribute;
        if (showIfAttribute == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // 获取要检查的字段值
        object fieldValue = GetFieldValue(property, showIfAttribute.FieldName);

        // 检查字段值是否匹配
        bool showProperty = IsValueMatch(fieldValue, showIfAttribute.MatchValue);

        if (showProperty)
        {
            // 如果匹配，绘制属性
            EditorGUI.PropertyField(position, property, label, true);
        }
        // 如果不匹配，不绘制属性，保持空白
    }

    /// <summary>
    /// 获取属性高度
    /// </summary>
    /// <param name="property">属性</param>
    /// <param name="label">标签</param>
    /// <returns>属性高度</returns>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIfAttribute = attribute as ShowIfAttribute;
        if (showIfAttribute == null)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // 获取要检查的字段值
        object fieldValue = GetFieldValue(property, showIfAttribute.FieldName);

        // 检查字段值是否匹配
        bool showProperty = IsValueMatch(fieldValue, showIfAttribute.MatchValue);

        if (showProperty)
        {
            // 如果匹配，返回正常高度
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // 如果不匹配，返回0高度
        return 0f;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <param name="property">属性</param>
    /// <param name="fieldName">字段名称</param>
    /// <returns>字段值</returns>
    private object GetFieldValue(SerializedProperty property, string fieldName)
    {
        try
        {
            // 获取序列化对象的类型
            Type targetType = property.serializedObject.targetObject.GetType();

            // 获取字段信息
            FieldInfo fieldInfo = targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return null;
            }

            // 获取字段值
            return fieldInfo.GetValue(property.serializedObject.targetObject);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ShowIfPropertyDrawer] 获取字段值失败: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 检查值是否匹配
    /// </summary>
    /// <param name="fieldValue">字段值</param>
    /// <param name="matchValue">匹配值</param>
    /// <returns>是否匹配</returns>
    private bool IsValueMatch(object fieldValue, object matchValue)
    {
        if (fieldValue == null && matchValue == null)
        {
            return true;
        }

        if (fieldValue == null || matchValue == null)
        {
            return false;
        }

        try
        {
            // 处理枚举类型
            if (fieldValue.GetType().IsEnum && matchValue.GetType().IsEnum)
            {
                return fieldValue.Equals(matchValue);
            }

            // 处理值类型
            if (fieldValue.GetType().IsValueType && matchValue.GetType().IsValueType)
            {
                // 转换为相同类型后比较
                Type targetType = fieldValue.GetType();
                object convertedMatchValue = Convert.ChangeType(matchValue, targetType);
                return fieldValue.Equals(convertedMatchValue);
            }

            // 处理引用类型
            return fieldValue.Equals(matchValue);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ShowIfPropertyDrawer] 比较值失败: {e.Message}");
            return false;
        }
    }
}

#endif