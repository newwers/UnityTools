/********************************************************************
生成日期:	2020-06-12
类    名: 	LocalizationManager
作    者:	zdq
描    述:	多语言管理器
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopGame.Core
{
    public class LocalizationManager
    {
        public enum ELanguageType
        {
            /// <summary>
            /// 无
            /// </summary>
            None,
            /// <summary>
            /// 汉语
            /// </summary>
            CN,
            /// <summary>
            /// 英语
            /// </summary>
            EN,
            Count,
        }

        public delegate void OnLanguageChange(ELanguageType languageType);
        public OnLanguageChange OnLanguageChangeCallback;

        public ELanguageType CurrentLanguage
        {
            get
            {
                return m_eLanguageType;
            }
        }

        Dictionary<uint, List<string>> m_vLanguageDic;

        ELanguageType m_eLanguageType;

        public void Awake()
        {
            //获取默认的语言类型
            //var cfg = SystemSettingManager.getInstance().GetSystemSettingData();
            //if (cfg != null)
            //{
            //    if (cfg.Language >= (int)ELanguageType.Count)
            //    {
            //        m_eLanguageType = ELanguageType.CN;//默认中文
            //    }
            //    else
            //    {
            //        m_eLanguageType = (ELanguageType)cfg.Language;
            //    }
            //}
            //else
            //{
            //    m_eLanguageType = ELanguageType.CN;//默认中文
            //}
            SetCurrentLanguage(m_eLanguageType);

            m_vLanguageDic = new Dictionary<uint, List<string>>();
            //获取多语言表
            //var datas = DataManager.getInstance().Text.datas;

            //CsvData_Text.TextData data;
            //List<string> tempList;
            //foreach (var dataID in datas.Keys)
            //{
            //    data = DataManager.getInstance().Text.GetData(dataID);
            //    if (data != null)
            //    {
            //        tempList = new List<string>();
            //        tempList.Add(data.textCN);
            //        tempList.Add(data.textEN);//如果新增语言类型,这边要新增对应字段,要注意数组添加顺序,会影响到后面读取
            //        m_vLanguageDic.Add(dataID, tempList);
            //    }
            //}
        }
        //------------------------------------------------------
        public void Destroy()
        {
            if (m_vLanguageDic != null)
            {
                m_vLanguageDic.Clear();
            }
            if (OnLanguageChangeCallback != null)
            {
                OnLanguageChangeCallback = null;
            }
        }
        //------------------------------------------------------
        public void SetCurrentLanguage(ELanguageType languageType)
        {
            if (m_eLanguageType != languageType)
            {
                if (languageType >= ELanguageType.Count)
                {
                    Debug.LogError("设置的语言类型不对:" + languageType);
                    return;
                }
                m_eLanguageType = languageType;
                if (OnLanguageChangeCallback != null)
                {
                    OnLanguageChangeCallback.Invoke(m_eLanguageType);
                }
            }
        }
        //------------------------------------------------------
        public string GetLocalization(uint id)
        {
            List<string> tempList = new List<string>();
            if (m_vLanguageDic.TryGetValue(id, out tempList))
            {
                int index = ((int)m_eLanguageType) -1;//根据枚举类型id,取对应语言,-1是枚举第一个类型是None
                if (tempList.Count > index)
                {
                    return tempList[index];
                }
            }
            return null;
        }
        //------------------------------------------------------
        public uint GetIDByChinese(string str)
        {
            foreach (var key in m_vLanguageDic.Keys)
            {
                if (m_vLanguageDic[key][0].Equals(str))
                {
                    return key;
                }
            }
            return 0;
        }
    }

}
