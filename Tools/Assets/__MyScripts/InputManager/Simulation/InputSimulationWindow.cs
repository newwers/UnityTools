using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InputSimulation
{


    public class InputSimulationWindow : EditorWindow
    {
        InputRecorder m_InputRecorder = new InputRecorder();
        Dictionary<int, List<InputRecorder.Info>> m_vInfos = new Dictionary<int, List<InputRecorder.Info>>();

        [MenuItem("Tools/������¼�����")]
        public static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(InputSimulationWindow));
            window.titleContent = new GUIContent("������¼�����");


        }

        [MenuItem("Tools/ֹͣ���¼�Ʋ��� %#q")]
        public static void StopInputPlay()
        {
            InputRecorder.SStop();
            Debug.Log("ֹͣ���¼�Ʋ���");
        }

        [MenuItem("Tools/��ʼ���¼�� %#4")]
        public static void RecordInput()
        {
            InputRecorder.SStartRecord();
            MouseHook.Start();
            Debug.Log("��ʼ���¼��");
        }

        [MenuItem("Tools/�������¼�� %#5")]
        public static void StopRecordInput()
        {
            InputRecorder.SEndRecord();
            MouseHook.Stop();
            Debug.Log("�������¼��");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            InputRecorder.OnStartRecord += InputRecorder_OnStartRecord;
            InputRecorder.OnEndRecord += InputRecorder_OnEndRecord;     
            //EditorApplication.update+= m_Input.Update;
            //Debug.Log("TestEditor OnEnable");
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= m_InputRecorder.Update;
            InputRecorder.OnStartRecord -= InputRecorder_OnStartRecord;
            InputRecorder.OnEndRecord -= InputRecorder_OnEndRecord;
            //Debug.Log("TestEditor OnDisable");
            MouseHook.Stop();
            //KeyboardHook.Stop();
        }


        private void InputRecorder_OnEndRecord()
        {
            m_bIsInputListener = false;
        }
        private void InputRecorder_OnStartRecord()
        {
            m_bIsInputListener = true;
        }





        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            //Debug.LogError("state:" + state);
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    //������Ϸʱ
                    MouseHook.Stop();
                    KeyboardHook.Stop();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        private void OnGUI()
        {
            InputListen();

            // �������̰���
            //Event e = Event.current;

            //if (e != null && e.isKey && e.type == EventType.KeyDown)
            //{
            //    KeyCode keyCode = e.keyCode;
            //    Debug.Log("Key Down: " + keyCode);

            //    Repaint();
            //}
        }

        //------------------------------------------------------
        bool m_bIsInputListener;
        bool m_bIsPlayRecord;
        int m_nSelectIndex;
        List<string> m_vShowItems = new List<string>();
        string m_Name;
        float m_nPlayTimeOffset;
        void InputListen()
        {
            var color = GUI.color;

            if (m_bIsInputListener)
            {
                GUI.color = Color.yellow;
                if (GUILayout.Button("ֹͣ¼�����������(ctrl+shift+5)", new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    m_InputRecorder.EndRecord();
                    MouseHook.Stop();
                    //KeyboardHook.Stop();
                    m_bIsInputListener = false;
                }
            }
            else
            {
                GUI.color = Color.white;
                if (GUILayout.Button("��ʼ¼�����������(ctrl+shift+4)", new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    m_InputRecorder.StartRecord();
                    MouseHook.Start();

                    m_bIsInputListener = true;
                }
            }
            if (m_bIsPlayRecord)
            {
                GUI.color = Color.yellow;
                if (GUILayout.Button("ֹͣ����������(ctrl+shift+q)", new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    MouseHook.Stop();
                    m_bIsInputListener = false;
                    EditorApplication.update -= m_InputRecorder.Update;
                    m_InputRecorder.Stop();
                    m_bIsPlayRecord = false;
                    InputRecorder.OnStopPlay -= InputRecorder_OnStop;
                }
            }
            else
            {
                GUI.color = Color.white;
                if (GUILayout.Button("����¼��������", new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    MouseHook.Stop();
                    m_bIsInputListener = false;
                    EditorApplication.update += m_InputRecorder.Update;
                    InputRecorder.OnStopPlay += InputRecorder_OnStop;
                    m_InputRecorder.Play();
                    m_bIsPlayRecord = true;
                }
            }

            GUI.color = color;

            m_Name = EditorGUILayout.TextField("����", m_Name);
            if (GUILayout.Button("����", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                MouseHook.Stop();
                m_bIsInputListener = false;
                EditorApplication.update -= m_InputRecorder.Update;

                Save();
                

            }
            if (GUILayout.Button("�����б�", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                Load();
            }

            EditorGUILayout.BeginHorizontal();

            m_nSelectIndex = EditorGUILayout.Popup("��¼�б�:",m_nSelectIndex, m_vShowItems.ToArray());

            if (GUILayout.Button("���óɵ�ǰ����", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                if (m_vInfos.TryGetValue(m_nSelectIndex, out var value))
                {
                    m_InputRecorder.Load(value);
                    ShowNotification(new GUIContent("���óɹ�"));
                }
                else
                {
                    ShowNotification(new GUIContent("�Ҳ�����Ӧ����,���߼���һ���б�"));
                }
            }

            if (GUILayout.Button("ɾ����ǰ����", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                if (m_vInfos.TryGetValue(m_nSelectIndex, out var value) && m_nSelectIndex < m_vShowItems.Count)
                {
                    Delete(m_vShowItems[m_nSelectIndex]);
                    m_vInfos.Remove(m_nSelectIndex);
                    m_vShowItems.RemoveAt(m_nSelectIndex);
                    ShowNotification(new GUIContent("ɾ���ɹ�"));
                }
                else
                {
                    ShowNotification(new GUIContent("�Ҳ�����Ӧ����,���߼���һ���б�"));
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            m_nPlayTimeOffset = EditorGUILayout.Slider(new GUIContent("����ƫ��"),m_nPlayTimeOffset, -2f, 2f);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("���ò���ƫ��", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                m_InputRecorder.SetPlayTimeOffset(m_nPlayTimeOffset);
                ShowNotification(new GUIContent("���ò���ƫ�Ƴɹ�:" + m_nPlayTimeOffset));
            }
        }


        private void InputRecorder_OnStop()
        {
            MouseHook.Stop();
            m_bIsInputListener = false;
            EditorApplication.update -= m_InputRecorder.Update;
            m_bIsPlayRecord = false;
            InputRecorder.OnStopPlay -= InputRecorder_OnStop;
        }

        void Save()
        {
            int index = m_vInfos.Count;
            //���ڴ�������,���浽�����ļ�
            string folderPath = Application.dataPath + "/../Local/InputSimulation/";
            string filePath = Path.Combine(folderPath, $"{m_Name}.json");

            // �����ļ���
            Directory.CreateDirectory(folderPath);

            // ��Listת��ΪJSON��ʽ���ַ���
            var data = m_InputRecorder.GetInfos();
            string json = JsonUtility.ToJson(new SerializableList<InputRecorder.Info>(data, m_Name), true);

            // ��JSON�ַ���д���ļ�
            File.WriteAllText(filePath, json);

            Debug.Log("JSON file saved to: " + filePath);

            m_vInfos[index] = data;

            m_vShowItems.Add(m_Name);
        }

        void Load()
        {
            m_vShowItems.Clear();
            m_vInfos.Clear();

            string folderPath = Application.dataPath + "/../Local/InputSimulation/";

            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Folder does not exist: " + folderPath);
                ShowNotification(new GUIContent("����ʧ��,û�и�·���ļ���:" + folderPath));
                return;
            }

            string[] jsonFilePaths = Directory.GetFiles(folderPath, "*.json");

            foreach (string filePath in jsonFilePaths)
            {
                string json = File.ReadAllText(filePath);

                // ��JSON�ַ���ת��Ϊ����
                var dataObject = JsonUtility.FromJson<SerializableList<InputRecorder.Info>>(json);

                // �ڴ˴�����������ݶ���
                Debug.Log("Loaded JSON file: " + filePath + "\nData: " + dataObject.ToString());

                int index = m_vInfos.Count;
                m_vInfos[index] = dataObject.list;
                m_vShowItems.Add(dataObject.name);
            }

            ShowNotification(new GUIContent($"���سɹ�,���μ���{m_vInfos.Count}��!"));
        }

        void Delete(string fileName)
        {
            //ɾ���ļ�

            string folderPath = Application.dataPath + "/../Local/InputSimulation/";

            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Folder does not exist: " + folderPath);
                return;
            }

            string[] jsonFilePaths = Directory.GetFiles(folderPath, "*.json");
            var path = Path.Combine(folderPath, fileName+".json");

            foreach (string filePath in jsonFilePaths)
            {
                if (filePath.Equals(path))
                {
                    Debug.Log("ɾ�����ļ�:" + filePath);
                    File.Delete(filePath);
                    break;
                }
            }
        }

        [System.Serializable]
        private class SerializableList<T>
        {
            public List<T> list;
            public string name;

            public SerializableList(List<T> list, string name)
            {
                this.list = list;
                this.name = name;
            }
        }
    }
}