using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z.Data
{
    public abstract class ADataManager
    {
        public static Action OnLoaded;

        public bool bInited { get; set; }

        public float Progress
        {
            get
            {
                if (m_nTotalCnt <= 0)
                {
                    return 1f;
                }

                return (float)m_nLoadCnt / (float)m_nTotalCnt;
            }
        }

        protected int m_nLoadCnt = 0;

        protected int m_nTotalCnt = 0;

        private Dictionary<int, ConfigDataBase> m_vDatas = new Dictionary<int, ConfigDataBase>();

        public ADataManager()
        {
            bInited = false;
        }

        public bool Init(string dataFile = "Assets/DataManager/DataConfig.asset")
        {
            //加载文件


#if UNITY_EDITOR
            //先使用编辑器模式加载
            var cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<DataConfig>(dataFile);
            if (cfg != null && cfg.vConfigs != null)
            {
                m_nTotalCnt = cfg.vConfigs.Count;
                m_nLoadCnt = 0;
                CsvParser csvParser = new CsvParser();
                for (int i = 0; i < cfg.vConfigs.Count; i++)
                {
                    var data = Parser(csvParser, cfg.vConfigs[i]);
                    m_vDatas.Add(cfg.vConfigs[i].guid, data);
                    m_nLoadCnt++;
                }
            }
#else
            //这边需要接入资源加载模块,否则打包后,找不到配置文件
            var cfg = ResourceLoadManager.Instance.ResourceLoad<DataConfig>(dataFile);
            if (cfg != null && cfg.vConfigs != null)
            {
                m_nTotalCnt = cfg.vConfigs.Count;
                m_nLoadCnt = 0;
                CsvParser csvParser = new CsvParser();
                for (int i = 0; i < cfg.vConfigs.Count; i++)
                {
                    var data = Parser(csvParser, cfg.vConfigs[i]);
                    m_vDatas.Add(cfg.vConfigs[i].guid, data);
                    m_nLoadCnt++;
                }
            }
#endif



            //更新加载数量

            return true;
        }

        protected abstract ConfigDataBase Parser(CsvParser csvParser, DataConfig.DataInfo data);

        protected virtual void Awake()
        {
            bInited = false;
            m_nLoadCnt = 0;
            m_nTotalCnt = 0;
            m_vDatas.Clear();
            OnAwake();
        }

        protected abstract void OnAwake();

        protected virtual void OnDestroy()
        {
            OnLoaded = null;
            m_nLoadCnt = 0;
            m_nTotalCnt = 0;
            bInited = false;
        }
    }
}