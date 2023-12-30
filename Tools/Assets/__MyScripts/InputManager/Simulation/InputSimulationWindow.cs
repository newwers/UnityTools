#if UNITY_EDITOR


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

        [MenuItem("Tools/鼠标操作录制面板")]
        public static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(InputSimulationWindow));
            window.titleContent = new GUIContent("鼠标操作录制面板");


        }

        [MenuItem("Tools/停止鼠标录制播放 %6")]
        public static void StopInputPlay()
        {
            InputRecorder.SStop();
            Debug.Log("停止鼠标录制播放");
        }

        [MenuItem("Tools/开始鼠标录制 %4")]
        public static void RecordInput()
        {
            InputRecorder.SStartRecord();
            MouseHook.Start();
            Debug.Log("开始鼠标录制");
        }

        [MenuItem("Tools/结束鼠标录制 %5")]
        public static void StopRecordInput()
        {
            InputRecorder.SEndRecord();
            MouseHook.Stop();
            Debug.Log("结束鼠标录制");
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
                    //进入游戏时
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

            // 监听键盘按键
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
        void InputListen()
        {
            var color = GUI.color;

            if (m_bIsInputListener)
            {
                GUI.color = Color.yellow;
                if (GUILayout.Button("停止录制鼠标点击操作", new GUILayoutOption[] { GUILayout.Height(50) }))
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
                if (GUILayout.Button("开始录制鼠标点击操作", new GUILayoutOption[] { GUILayout.Height(50) }))
                {
                    m_InputRecorder.StartRecord();
                    MouseHook.Start();

                    m_bIsInputListener = true;
                }
            }
            if (m_bIsPlayRecord)
            {
                GUI.color = Color.yellow;
                if (GUILayout.Button("停止播放鼠标操作", new GUILayoutOption[] { GUILayout.Height(50) }))
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
                if (GUILayout.Button("播放录制鼠标操作", new GUILayoutOption[] { GUILayout.Height(50) }))
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

            m_Name = EditorGUILayout.TextField("名称", m_Name);
            if (GUILayout.Button("保存", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                MouseHook.Stop();
                m_bIsInputListener = false;
                EditorApplication.update -= m_InputRecorder.Update;

                Save();


            }
            if (GUILayout.Button("加载列表", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                Load();
            }

            EditorGUILayout.BeginHorizontal();

            m_nSelectIndex = EditorGUILayout.Popup("记录列表:", m_nSelectIndex, m_vShowItems.ToArray());

            if (GUILayout.Button("设置成当前对象", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                if (m_vInfos.TryGetValue(m_nSelectIndex, out var value))
                {
                    m_InputRecorder.Load(value);
                    ShowNotification(new GUIContent("设置成功"));
                }
                else
                {
                    ShowNotification(new GUIContent("找不到对应数据,或者加载一下列表"));
                }
            }

            if (GUILayout.Button("删除当前对象", new GUILayoutOption[] { GUILayout.Height(50) }))
            {
                if (m_vInfos.TryGetValue(m_nSelectIndex, out var value) && m_nSelectIndex < m_vShowItems.Count)
                {
                    Delete(m_vShowItems[m_nSelectIndex]);
                    m_vInfos.Remove(m_nSelectIndex);
                    m_vShowItems.RemoveAt(m_nSelectIndex);
                    ShowNotification(new GUIContent("删除成功"));
                }
                else
                {
                    ShowNotification(new GUIContent("找不到对应数据,或者加载一下列表"));
                }
            }

            EditorGUILayout.EndHorizontal();
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
            //将内存中数据,保存到本地文件
            string folderPath = Application.dataPath + "/../Local/InputSimulation/";
            string filePath = Path.Combine(folderPath, $"{m_Name}.json");

            // 创建文件夹
            Directory.CreateDirectory(folderPath);

            // 将List转换为JSON格式的字符串
            var data = m_InputRecorder.GetInfos();
            string json = JsonUtility.ToJson(new SerializableList<InputRecorder.Info>(data, m_Name), true);

            // 将JSON字符串写入文件
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
                return;
            }

            string[] jsonFilePaths = Directory.GetFiles(folderPath, "*.json");

            foreach (string filePath in jsonFilePaths)
            {
                string json = File.ReadAllText(filePath);

                // 将JSON字符串转换为对象
                var dataObject = JsonUtility.FromJson<SerializableList<InputRecorder.Info>>(json);

                // 在此处处理你的数据对象
                Debug.Log("Loaded JSON file: " + filePath + "\nData: " + dataObject.ToString());

                int index = m_vInfos.Count;
                m_vInfos[index] = dataObject.list;
                m_vShowItems.Add(dataObject.name);
            }
        }

        void Delete(string fileName)
        {
            //删除文件

            string folderPath = Application.dataPath + "/../Local/InputSimulation/";

            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Folder does not exist: " + folderPath);
                return;
            }

            string[] jsonFilePaths = Directory.GetFiles(folderPath, "*.json");
            var path = Path.Combine(folderPath, fileName + ".json");

            foreach (string filePath in jsonFilePaths)
            {
                if (filePath.Equals(path))
                {
                    Debug.Log("删除该文件:" + filePath);
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
#endif