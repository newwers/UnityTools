using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Z.Data
{
    [CreateAssetMenu(fileName = "Config", menuName = "ScriptableObjects/CreateDataConfig")]
    public class DataConfig : ScriptableObject
    {
        [Serializable]
        public class DataInfo
        {
            public string guid;
            public string filePath;
            public TextAsset data;
        }

        [NonReorderable]
        public List<DataInfo> vConfigs = new List<DataInfo>();

        [SerializeField]
        private string m_BuildFilePath;
        public string BuildFilePath
        {
            get
            {
                return m_BuildFilePath;
            }
            set
            {
                m_BuildFilePath = value;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < vConfigs.Count; i++)
            {
                var cfg = vConfigs[i];
                cfg.filePath = AssetDatabase.GetAssetPath(cfg.data);
                cfg.guid = AssetDatabase.AssetPathToGUID(cfg.filePath);
            }
            EditorUtility.SetDirty(this);
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
            this.serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            m_FilePath = EditorGUILayout.TextField("生成文件夹路径:", m_FilePath);
            if (GUILayout.Button("选择路径"))
            {
                m_FilePath = EditorUtility.OpenFolderPanel("选择导出文件夹路径", Application.dataPath, "");
                m_Target.BuildFilePath = m_FilePath;
                EditorUtility.SetDirty(m_Target);
            }

            EditorGUILayout.PropertyField(m_vConfigs);


            //base.OnInspectorGUI();
            //base.DrawDefaultInspector();



            if (GUILayout.Button("添加"))
            {
                m_Target.vConfigs.Add(null);
                EditorUtility.SetDirty(m_Target);
            }

            if (GUILayout.Button("生成代码"))
            {
                if (string.IsNullOrWhiteSpace(m_FilePath))
                {
                    Debug.Log("请选择生成路径!");
                    return;
                }
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

            if (GUILayout.Button("刷新"))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("测试"))
            {
                var filePath = AssetDatabase.GetAssetPath(m_Target);
                //加载数据
                //var text = m_Target.vConfigs[0].data;
                //CsvData_SystemConfig data = new CsvData_SystemConfig();
                //data.LoadData(text.text);
                //Debug.Log($"id:{data.datas[1].id}");
                //打印数据

                DataManager.Instance.Init(filePath);
                //var datas = DataManager.Instance.Text.datas;
                //Debug.Log($"id:{datas[10001000].textCN}");
            }

            //if (GUILayout.Button("保存"))
            //{
            //    EditorUtility.SetDirty(m_Target);
            //    AssetDatabase.SaveAssetIfDirty(m_Target);
            //}


            EditorGUI.EndChangeCheck();
            this.serializedObject.ApplyModifiedProperties();
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

#endif

}