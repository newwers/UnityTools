/********************************************************************
生成日期:	2020-06-12
类    名: 	LocalizationManager
作    者:	zdq
描    述:	多语言管理器
*********************************************************************/

using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Z.Data;

namespace Z.Core.Localization
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
            TW, // 繁体中文
            /// <summary>
            /// 英语
            /// </summary>
            EN,
            RU, // 俄语
            ES, // 西班牙语
            BR, // 巴西葡萄牙语
            DE, // 德语
            JA, // 日语
            FR, // 法语
            PL, // 波兰语
            KO, // 韩语
            TR, // 土耳其语
            es419, // 西班牙语（拉丁美洲）
            UK,// 乌克兰语
            IT, // 意大利语
            CS, // 捷克语
            PT, // 葡萄牙语
            HU, // 匈牙利语
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

        public void OnSetDefultLanguage()
        {
            // 新游戏设置默认语言
            ELanguageType defaultLanguage = ELanguageType.EN; // 默认备用语言为英语

            try
            {
                // 检查是否在Steam平台上运行
                if (SteamManager.Initialized)
                {
                    string steamLanguage = SteamUtils.GetSteamUILanguage();

                    // 将Steam语言代码映射到ELanguageType
                    switch (steamLanguage.ToLower())
                    {
                        case "schinese":
                            defaultLanguage = ELanguageType.CN;
                            break;
                        case "tchinese":
                            defaultLanguage = ELanguageType.TW;
                            break;
                        case "english":
                            defaultLanguage = ELanguageType.EN;
                            break;
                        case "russian":
                            defaultLanguage = ELanguageType.RU;
                            break;
                        case "spanish":
                            defaultLanguage = ELanguageType.ES;
                            break;
                        case "brazilian":
                            defaultLanguage = ELanguageType.BR;
                            break;
                        case "german":
                            defaultLanguage = ELanguageType.DE;
                            break;
                        case "japanese":
                            defaultLanguage = ELanguageType.JA;
                            break;
                        case "french":
                            defaultLanguage = ELanguageType.FR;
                            break;
                        case "polish":
                            defaultLanguage = ELanguageType.PL;
                            break;
                        case "koreana":
                            defaultLanguage = ELanguageType.KO;
                            break;
                        case "turkish":
                            defaultLanguage = ELanguageType.TR;
                            break;
                        case "latam":
                            defaultLanguage = ELanguageType.es419;
                            break;
                        case "ukrainian":
                            defaultLanguage = ELanguageType.UK;
                            break;
                        case "italian":
                            defaultLanguage = ELanguageType.IT;
                            break;
                        case "czech":
                            defaultLanguage = ELanguageType.CS;
                            break;
                        case "portuguese":
                            defaultLanguage = ELanguageType.PT;
                            break;
                        case "hungarian":
                            defaultLanguage = ELanguageType.HU;
                            break;
                        default:
                            Debug.LogWarning($"Unsupported Steam language: {steamLanguage}, falling back to English.");
                            defaultLanguage = ELanguageType.EN;
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning("Steam is not initialized, using fallback language.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error getting Steam language: {ex.Message}");
            }

            // 设置默认语言
            LogManager.Log($"设置默认语言: {defaultLanguage}");
            SetCurrentLanguage(defaultLanguage);
        }
        public void Awake()
        {

            m_vLanguageDic = new Dictionary<uint, List<string>>();
            //获取多语言表
            var datas = DataManager.Instance.Text.datas;

            CsvData_Text.TextData data;
            List<string> tempList;
            foreach (var dataID in datas.Keys)
            {
                data = DataManager.Instance.Text.GetData(dataID);
                if (data != null)
                {
                    //todo:如果新增语言类型,这边要新增对应字段,要注意数组添加顺序,会影响到后面读取
                    tempList = new List<string>();
                    tempList.Add(data.textzhCN);
                    tempList.Add(data.textzhTW);
                    tempList.Add(data.textEN);
                    tempList.Add(data.textRU);
                    tempList.Add(data.textesES);
                    tempList.Add(data.textptBR);
                    tempList.Add(data.textDE);
                    tempList.Add(data.textJA);
                    tempList.Add(data.textFR);
                    tempList.Add(data.textPL);
                    tempList.Add(data.textKO);
                    tempList.Add(data.textTR);
                    tempList.Add(data.textes419);
                    tempList.Add(data.textUK);
                    tempList.Add(data.textIT);
                    tempList.Add(data.textCS);
                    tempList.Add(data.textptPT);
                    tempList.Add(data.textHU);
                    m_vLanguageDic.Add(dataID, tempList);
                }
            }
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
                int index = ((int)m_eLanguageType) - 1;//根据枚举类型id,取对应语言,-1是枚举第一个类型是None
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
