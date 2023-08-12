using System.Collections;
using System.Collections.Generic;
using System.Text;
using TopGame.UI;
using UnityEditor;
using UnityEngine;

namespace AutoCode
{
    public class AutoCodeEditorWindow : EditorWindow
    {
        UISerialized m_Ui;
        string m_FileName;
        string m_FilePath;

        [MenuItem("Tools/UI/UI代码生成面板")]
        public static void ShowWindow2()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(AutoCodeEditorWindow));
            window.titleContent = new GUIContent("代码生成");
        }

        private void OnGUI()
        {
            m_Ui =  EditorGUILayout.ObjectField(m_Ui, typeof(UISerialized),true) as UISerialized;
            m_FileName = EditorGUILayout.TextField("文件名", m_FileName);
            m_FilePath = EditorGUILayout.TextField("文件路径", m_FilePath);
            if (GUILayout.Button("选择生成路径"))
            {
                m_FilePath = EditorUtility.OpenFolderPanel("选择文件路径", Application.dataPath, "");

                
            }
            if (GUILayout.Button("生成"))
            {
                BuilderAutoCode();
            }
        }
        //------------------------------------------------------
        void BuilderAutoCode()
        {
            if (m_Ui == null)
            {
                ShowNotification(new GUIContent("m_ui is null!"));
                return;
            }

            ShowNotification(new GUIContent($"m_FileName:{m_FileName}"));

            AutoCodeViewBuilder builder = new AutoCodeViewBuilder(m_FileName, m_FilePath, m_Ui);
            builder.Builder();

            ShowNotification(new GUIContent("代码生成完成!"));
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}