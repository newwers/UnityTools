using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 自定义只读属性特性
public class FieldReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
// 自定义属性绘制器，用于在Inspector面板中绘制只读属性
[CustomPropertyDrawer(typeof(FieldReadOnlyAttribute))]
public class FieldReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 保存GUI的原有状态
        GUI.enabled = false;
        // 绘制属性
        EditorGUI.PropertyField(position, property, label, true);
        // 恢复GUI的原有状态
        GUI.enabled = true;
    }
}
#endif

