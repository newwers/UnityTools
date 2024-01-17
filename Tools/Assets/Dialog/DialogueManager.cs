using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Z.UI;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Z.Dialog
{

    public class DialogueManager : MonoBehaviour
    {
        public TextMeshProUGUI speakerNameText;
        public Image speakerIconImage;
        public Image bgImage;
        public TextMeshProUGUI dialogueText;
        public AudioSource dialogAudioSource;

        public List<DialogueData> dialogues = new List<DialogueData>();
        private int currentDialogueIndex = 0;

        private void Start()
        {
            LoadDialogue();
            DisplayCurrentDialogue();
        }

        public void LoadDialogue()
        {
            // 从文件或其他数据源加载对话数据到dialogues数组中
        }

        public void AddDialogueData(DialogueData data)
        {
            dialogues.Add(data);
        }

        public void ClearDialogueData()
        {
            dialogues.Clear();
        }

        public void DisplayCurrentDialogue()
        {
            DialogueData currentDialogue = dialogues[currentDialogueIndex];
            
            if (currentDialogue.isDefaultSet)//缺省设置
            {
                speakerIconImage.sprite = currentDialogue.speakerIcon;
                speakerNameText.text = currentDialogue.speakerName;
                bgImage.sprite = currentDialogue.bgIcon;
                bgImage.color = currentDialogue.bgColor;
            }
            dialogueText.text = currentDialogue.dialogue;
            dialogAudioSource.clip = currentDialogue.dialogAudioClip;
            if (dialogAudioSource.clip)
            {
                dialogAudioSource.time = 0;
                dialogAudioSource.Play();
            }
        }

        public void NextDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex < dialogues.Count)
            {
                DisplayCurrentDialogue();
            }
            else
            {
                // 对话结束的逻辑
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(DialogueManager))]
    public class DialogueManagerEditor:Editor
    {
        private DialogueManager m_Manager;

        private void OnEnable()
        {
            m_Manager = target as DialogueManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("加载文件夹里面所有ScriptableObject"))
            {
                var folderPath = EditorUtility.OpenFolderPanel("加载文件夹", Application.dataPath, "");
                folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length); // 将绝对路径转换为相对路径

                string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new string[] { folderPath }); // 查找文件夹内所有ScriptableObject文件的GUID

                m_Manager.ClearDialogueData();

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid); // 获取文件路径
                    DialogueData data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath); // 加载ScriptableObject文件
                                                                                                                    
                    Debug.Log("Loaded ScriptableObject: " + data.name);

                    
                    m_Manager.AddDialogueData(data);
                }
            }
        }
    }

#endif
}