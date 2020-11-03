/********************************************************************
生成日期:	2020-06-12
类    名: 	LocalizationText
作    者:	zdq
描    述:	多语言控制Text显示组件,使用方法,将此脚本挂载到显示的Text控件上
*********************************************************************/
using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using TopGame.Data;
#endif

namespace TopGame.Core
{
    [RequireComponent(typeof(Text))]
    public class LocalizationText : MonoBehaviour
    {
        public uint ID = 0;

        Text m_text;

        private void Awake()
        {
            m_text = GetComponent<Text>();
        }
        //------------------------------------------------------
        private void OnEnable()
        {
            RefreshText();
        }
        //------------------------------------------------------
        private void Start()
        {
            GameInstance.getInstance().localizationMgr.OnLanguageChangeCallback += OnLanguageChangeCallback;
        }
        //------------------------------------------------------
        private void OnLanguageChangeCallback(LocalizationManager.ELanguageType languageType)
        {
            if (m_text == null ||gameObject.activeInHierarchy == false)
            {
                return;
            }
            RefreshText();
        }

        /// <summary>
        /// 提供一个设置id的方法,同时刷新显示
        /// </summary>
        /// <param name="id"></param>
        public void UpdateLocalization(uint id)
        {
            ID = id;
            RefreshText();
        }

        //------------------------------------------------------
        [ContextMenu("测试显示")]
        private void RefreshText()
        {
            if (m_text == null || ID == 0)
            {
                return;
            }
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif
            if (GameInstance.getInstance().localizationMgr == null) return;
            string text = GameInstance.getInstance().localizationMgr.GetLocalization(ID);
            if (text != null)
            {
                m_text.text = text;
            }
        }
#if UNITY_EDITOR

        //------------------------------------------------------
        [ContextMenu("根据中文获取多语言ID")]
        private void GetIDByText()
        {
            Text text = GetComponent<Text>();
            if (text == null)
            {
                return;
            }

            
            var csv = DataManager.getInstance().Text;
            if (csv == null)
            {
                Data.DataManager.ReInit();
                csv = DataManager.getInstance().Text;
            }
            if (csv == null)
            {
                Plugin.Logger.Error("Csv数据为null");
                return;
            }
            var datas = csv.datas;

            foreach (var dataID in datas.Keys)
            {
                var data = DataManager.getInstance().Text.GetData(dataID);
                if (data != null)
                {
                    if (data.textCN.Equals(text.text))
                    {
                        ID = dataID;
                        break;
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        //------------------------------------------------------
        [ContextMenu("根据多语言ID获取中文")]
        private void GetChineseByID()
        {
            Text text = GetComponent<Text>();
            if (text == null)
            {
                return;
            }



            var csv = DataManager.getInstance().Text;
            if (csv == null)
            {
                Data.DataManager.ReInit();
                csv = DataManager.getInstance().Text;
            }
            if (csv == null)
            {
                Plugin.Logger.Error("Csv数据为null");
                return;
            }
            var datas = csv.datas;

            foreach (var dataID in datas.Keys)
            {
                var data = DataManager.getInstance().Text.GetData(dataID);
                if (data != null)
                {
                    if (data.id == ID)
                    {
                        text.text = data.textCN;
                        break;
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
        //------------------------------------------------------
        private void OnDestroy()
        {
            if (Module.ModuleManager.mainModule == null)
            {
                return;
            }
            GameInstance.getInstance().localizationMgr.OnLanguageChangeCallback -= OnLanguageChangeCallback;
        }
    }
}
