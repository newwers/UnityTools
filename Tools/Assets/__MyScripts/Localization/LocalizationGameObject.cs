/********************************************************************
��������:	2020-06-12
��    ��: 	LocalizationGameObject
��    ��:	zdq
��    ��:	�����Կ���GameObject����,ʹ�÷���,�˽ű����ص��������Ϊ������
*********************************************************************/
using Framework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopGame.Core
{
    public class LocalizationGameObject : LocalizationBase
    {
        DynamicLoader m_pLoader = new DynamicLoader();
        AInstanceAble m_pInstance = null;
        public Vector3 Scale = Vector3.one;
        public Vector3 Pos = Vector3.zero;
        //------------------------------------------------------
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_pLoader = null;
            if (m_pInstance != null)
            {
                m_pInstance.RecyleDestroy();
                m_pInstance = null;
            }

        }
        //------------------------------------------------------
        public override void OnLanguageChangeCallback(SystemLanguage languageType)
        {
            //���վ���Դ
            //���¼�����Դ
        }
        //------------------------------------------------------
        [ContextMenu("������ʾ")]
        public override void RefreshShow()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif
            if (ID == 0)
            {
                return;
            }
            string strPath = Base.GlobalUtil.ToLocalization((int)ID);
            if (strPath != null)
            {
                m_pLoader.LoadInstance(strPath, transform,true,OnCallback);
            }
        }
        //------------------------------------------------------
        private void OnCallback(InstanceOperiaon instanceOperiaon)
        {
            if (instanceOperiaon == null)
            {
                return;
            }

            if (m_pInstance != null)
            {
                m_pInstance.RecyleDestroy();
            }

            m_pInstance = instanceOperiaon.pPoolAble;

            if (m_pInstance)
            {
                m_pInstance.SetScale(Scale);
                m_pInstance.SetPosition(Pos, true);
            }
        }
        //------------------------------------------------------
        [ContextMenu("����������ʾ")]
        public void TestZhShow()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif
            
            string strPath = "Assets/Datas/Particles/UIEffect/UI_logo.prefab";
            if (strPath != null)
            {
                m_pLoader.LoadInstance(strPath, transform, true, OnCallback);
            }
        }
        //------------------------------------------------------
        [ContextMenu("����Ӣ����ʾ")]
        public void TestEnShow()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif

            string strPath = "Assets/Datas/Particles/UIEffect/UI_logo_english.prefab";
            if (strPath != null)
            {
                m_pLoader.LoadInstance(strPath, transform, true, OnCallback);
            }
        }
    }
}
