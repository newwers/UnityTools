using UnityEditor;
using UnityEngine;

namespace SteamSDK.Editor
{
    [CustomEditor(typeof(SteamAchievementDataSO), true)]
    public class SteamAchievementDataSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 绘制默认Inspector
            base.OnInspectorGUI();
            
            // 在最底部添加一个按钮
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if (GUILayout.Button("跳转到自身SO文件"))
            {
                // 选中当前SO文件
                Selection.activeObject = target;
                //  Ping该对象，使其在Project窗口中高亮显示
                EditorGUIUtility.PingObject(target);
            }
        }
    }
}