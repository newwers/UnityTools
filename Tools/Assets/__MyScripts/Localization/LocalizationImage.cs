/********************************************************************
生成日期:	2020-06-12
类    名: 	LocalizationImage
作    者:	zdq
描    述:	多语言控制Image显示组件,使用方法,将此脚本挂载到显示的Image控件上
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopGame.Core
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class LocalizationImage : MonoBehaviour
    {

        public uint ID = 0;

        Image m_image;

        void Awake()
        {
            m_image = GetComponent<Image>();
        }

        //------------------------------------------------------
        private void OnEnable()
        {
            RefreshText();
        }
        //------------------------------------------------------
        private void Start()
        {
            if (GameInstance.getInstance() != null && GameInstance.getInstance().localizationMgr != null)
            {
                GameInstance.getInstance().localizationMgr.OnLanguageChangeCallback += OnLanguageChangeCallback;
            }
        }
        //------------------------------------------------------
        private void OnLanguageChangeCallback(LocalizationManager.ELanguageType languageType)
        {
            if (m_image == null || gameObject.activeInHierarchy == false)
            {
                return;
            }
            RefreshText();
        }
        //------------------------------------------------------
        private void OnDestroy()
        {
            if (Module.ModuleManager.mainModule == null || GameInstance.getInstance() == null || GameInstance.getInstance().localizationMgr == null)
            {
                return;
            }
            GameInstance.getInstance().localizationMgr.OnLanguageChangeCallback -= OnLanguageChangeCallback;
        }
        //------------------------------------------------------
        [ContextMenu("测试显示")]
        private void RefreshText()
        {
            if (m_image == null || ID == 0)
            {
                return;
            }
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif
            if (GameInstance.getInstance().localizationMgr == null) return;
            string strPath = GameInstance.getInstance().localizationMgr.GetLocalization(ID);
            if (strPath != null)
            {
                //加载图片资源
                Core.AssetOperiaon pOp =  Core.FileSystem.getInstance().AsyncReadFile(strPath, OnLoadAssetCallback);
                if (pOp != null)
                {
                    pOp.userData = new Core.VariableObj() { pGO = m_image };
                }
            }
        }
        //------------------------------------------------------
        private void OnLoadAssetCallback(Core.AssetOperiaon pCB)
        {
            if (pCB == null) return;
            UnityEngine.Object pObject = ((Core.VariableObj)pCB.userData).pGO;

            if (pObject == null)
            {
                if (pCB.pAsset != null)
                {
                    pCB.pAsset.Release();
                }
                return;
            }

            if (!pCB.isValid()) return;


            if (OnLoadAsset(pObject, pCB.objectAsset) && pObject)
            {
                pCB.pAsset.Release();
            }
            else
            {
                pCB.pAsset.Release();
            }
        }
        //------------------------------------------------------
        protected virtual bool OnLoadAsset(UnityEngine.Object pObj, UnityEngine.Object objectAsset)
        {
            if (pObj == null) return false;

            System.Type type = pObj.GetType();
            if (type == typeof(UnityEngine.UI.RawImage))
            {
                UnityEngine.UI.RawImage image = (UnityEngine.UI.RawImage)pObj;

                Texture texture = objectAsset as Texture;
                if (texture)
                {
                    image.texture = texture;
                    return true;
                }
                return false;
            }
            if (type == typeof(UnityEngine.UI.Image))
            {
                UnityEngine.UI.Image image = (UnityEngine.UI.Image)pObj;

                Sprite spr = objectAsset as Sprite;
                if (spr)
                {
                    image.sprite = spr;
                    return true;
                }
                else
                {
#if UNITY_EDITOR
                    //解决unity编辑器模式下,Image组件不能动态加载图标的问题
                    //原因是由于编辑器模式下资源根据路径加载时,加载类型是 Object ,自动加载成Texture2D的形式导致的
                    //代码: UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path);
                    Texture2D texture2d = objectAsset as Texture2D;
                    if (texture2d)
                    {
                        //通过原本Image组件上的图片九宫格,解决九宫格缺失的问题
                        Vector4 border = image.sprite.border;
                        Sprite sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.zero, 100, 0, SpriteMeshType.Tight, border);
                        if (sprite)
                        {
                            image.sprite = sprite;
                            return true;
                        }
                    }
#endif
                }
            }

            return false;
        }
        //------------------------------------------------------
#if UNITY_EDITOR
        [ContextMenu("打印当前图片路径")]
        void PrintCurImagePath()
        {
            m_image = GetComponent<Image>();
            if (m_image == null)
            {
                return;
            }
            //m_image.sprite
            Debug.Log(UnityEditor.AssetDatabase.GetAssetPath(m_image.sprite));
        }
#endif
    }
}
