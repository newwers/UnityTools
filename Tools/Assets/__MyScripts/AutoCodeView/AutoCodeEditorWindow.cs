using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Z.UI;

namespace AutoCode
{
    public class AutoCodeEditorWindow : EditorWindow
    {
        UIReferenceComponent m_Ui;
        string m_FileName;
        string m_FilePath;

        [MenuItem("Tools/UI/UI�����������")]
        public static void ShowWindow2()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(AutoCodeEditorWindow));
            window.titleContent = new GUIContent("��������");
        }

        private void OnGUI()
        {
            m_Ui =  EditorGUILayout.ObjectField(m_Ui, typeof(UIReferenceComponent),true) as UIReferenceComponent;
            m_FileName = EditorGUILayout.TextField("�ļ���", m_FileName);
            m_FilePath = EditorGUILayout.TextField("�ļ�·��", m_FilePath);
            if (GUILayout.Button("ѡ������·��"))
            {
                m_FilePath = EditorUtility.OpenFolderPanel("ѡ���ļ�·��", Application.dataPath, "");

                
            }
            if (GUILayout.Button("����"))
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

            ShowNotification(new GUIContent("�����������!"));
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}