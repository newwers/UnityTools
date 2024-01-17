#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Z.UI;
using System.IO;

namespace Z.Dialog
{


    public class DialogueEditor : EditorWindow
    {
        private DialogueData dialogueData;

        [MenuItem("Tools/对话系统/Dialogue Editor")]
        public static void ShowWindow()
        {
            GetWindow<DialogueEditor>("Dialogue Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Dialogue Editor", EditorStyles.boldLabel);

            dialogueData = EditorGUILayout.ObjectField("Dialogue Data", dialogueData, typeof(DialogueData), false) as DialogueData;

            if (GUILayout.Button("Create New Dialogue"))
            {
                CreateNewDialogue();
            }

            if (dialogueData != null)
            {
                DisplayDialogue();
            }
        }

        private void CreateNewDialogue()
        {
            var data = ScriptableObject.CreateInstance<DialogueData>();
            string path = "Assets";
            if (dialogueData != null)
            {
                data.isDefaultSet = dialogueData.isDefaultSet;
                data.speakerIcon = dialogueData.speakerIcon;
                data.speakerName = dialogueData.speakerName;
                path = AssetDatabase.GetAssetPath(dialogueData);//Assets/Dialog/so/Test/Dialogue1.asset
                path = Path.GetDirectoryName(path);//Assets\Dialog\so\Test
            }
            
            string filePath = EditorUtility.SaveFilePanelInProject("Save Dialogue Data", "New Dialogue", "asset", "Save Dialogue Data", path);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }
            AssetDatabase.CreateAsset(data, filePath);
            AssetDatabase.SaveAssets();
            dialogueData = data;
        }

        private void DisplayDialogue()
        {
            GUILayout.Label("Edit Dialogue Here", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            dialogueData.isDefaultSet = EditorGUILayout.Toggle("设置默认头像", dialogueData.isDefaultSet);
            dialogueData.speakerName = EditorGUILayout.TextField("Speaker Name", dialogueData.speakerName);
            dialogueData.speakerIcon = (Sprite)EditorGUILayout.ObjectField("Speaker Icon", dialogueData.speakerIcon, typeof(Sprite), false);
            dialogueData.bgIcon = (Sprite)EditorGUILayout.ObjectField("bg Icon", dialogueData.bgIcon, typeof(Sprite), false);
            dialogueData.bgColor = EditorGUILayout.ColorField("bg Color", dialogueData.bgColor);
            dialogueData.dialogAudioClip = (AudioClip)EditorGUILayout.ObjectField("Dialogue Audio Clip", dialogueData.dialogAudioClip, typeof(AudioClip), false);

            string newDialogue = EditorGUILayout.TextArea(dialogueData.dialogue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(dialogueData, "Changed Dialogue");
                dialogueData.dialogue = newDialogue;
                EditorUtility.SetDirty(dialogueData);
            }
        }
    }
}
#endif