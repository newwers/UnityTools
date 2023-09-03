using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Z.Data
{

    [CreateAssetMenu(fileName = "DataConfig", menuName = "ScriptableObjects/CreateDataConfig", order = 1)]
    public class DataConfig : ScriptableObject
    {
        [Serializable]
        public class DataInfo
        {
            public int guid;
            public string filePath;
            public TextAsset data;
        }

        [NonReorderable]
        public List<DataInfo> vConfigs = new List<DataInfo>();

        public string BuildFilePath;

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < vConfigs.Count; i++)
            {
                var cfg = vConfigs[i];
                cfg.filePath = AssetDatabase.GetAssetPath(cfg.data);
                cfg.guid = cfg.data.GetInstanceID();
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataConfig))]
    public class DataConfigEditor : Editor
    {

        DataConfig m_Target;
        private SerializedProperty m_vConfigs;
        private string m_FilePath;

        private void OnEnable()
        {
            m_Target = (DataConfig)target;
            m_FilePath = m_Target.BuildFilePath;

            m_vConfigs = serializedObject.FindProperty("vConfigs");
        }

        public override void OnInspectorGUI()
        {
            m_FilePath = EditorGUILayout.TextField("生成文件夹路径:", m_FilePath);
            if (GUILayout.Button("选择路径"))
            {
                m_FilePath = EditorUtility.OpenFolderPanel("选择导出文件夹路径", Application.dataPath, "");
                m_Target.BuildFilePath = m_FilePath;
            }

            EditorGUILayout.PropertyField(m_vConfigs);



            //base.OnInspectorGUI();

            if (GUILayout.Button("添加"))
            {
                m_Target.vConfigs.Add(null);
            }

            if (GUILayout.Button("生成代码"))
            {
                var config = target as DataConfig;
                for (int i = 0; i < config.vConfigs.Count; i++)
                {
                    var data = config.vConfigs[i];
                    if (data != null)
                    {
                        BuildConfigCode(data);
                        Debug.Log("生成代码完成");
                        AssetDatabase.Refresh();
                    }
                }
                BuilderManager(config);
            }
            if (GUILayout.Button("测试"))
            {
                //加载数据
                var text = m_Target.vConfigs[0].data;
                CsvData_SystemConfig data = new CsvData_SystemConfig();
                data.LoadData(text.text);
                //打印数据
                Debug.Log($"id:{data.datas[1].id}");

                DataManager.Instance.Init();
                var datas = DataManager.Instance.Text.datas;
                Debug.Log($"id:{datas[1].id}");
            }
        }
        //------------------------------------------------------
        private void BuilderManager(DataConfig config)
        {
            //编译manager
            DataManagerBuilder builder = new DataManagerBuilder(config, m_FilePath);
            builder.Parse();
            Debug.Log("生成 manager 代码完成");
        }

        //------------------------------------------------------
        void BuildConfigCode(DataConfig.DataInfo data)
        {
            Debug.Log("cfg name:" + data.data.name);
            Debug.Log(data.data.text);
            //代码中根据每一项字段.读取填充数据
            CsvBuilder csvBuilder = new CsvBuilder(data.data.text, data.data.name, m_FilePath);
            csvBuilder.Parse();
        }
        
    }
}
#endif
