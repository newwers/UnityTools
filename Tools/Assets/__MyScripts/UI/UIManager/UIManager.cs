using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z.UI
{
    public class UIManager : BaseMonoSingleClass<UIManager>
    {
        /// <summary>
        /// 存放所有生成出来的界面
        /// </summary>
        public Dictionary<EUIInstanceID, BaseUIController> m_AllInstantiateUI;

        public UIScriptable pUIConfig;

        public Canvas StaticCanvas;
        public Canvas DynamicCanvas;
        public Camera UICamera;

        Vector3 m_HidePos = new Vector3(9999f, 9999f, 9999f);
        Vector3 m_ShowPos = new Vector3(0, 0, 0);


        protected override void Awake()
        {
            base.Awake();
        }
        //------------------------------------------------------
        public void Init()
        {
            m_AllInstantiateUI = new Dictionary<EUIInstanceID, BaseUIController>();
        }
        //------------------------------------------------------
        private void OnDestroy()
        {
            //todo:销毁所有UI界面
            m_AllInstantiateUI.Clear();
        }
        //------------------------------------------------------
        public static void Show(EUIInstanceID uiInstanceID)
        {
            if (Instance != null)
            {
                Instance.ShowUI(uiInstanceID);
            }
        }
        //------------------------------------------------------
        public static void Hide(EUIInstanceID uiInstanceID)
        {
            if (Instance != null)
            {
                Instance.HideUI(uiInstanceID);
            }
        }
        //------------------------------------------------------
        /// <summary>
        /// 显示UI
        /// </summary>
        /// <param name="uiInstanceID">UI唯一实例ID</param>
        /// <param name="uiGameObject">UI界面游戏物体</param>
        /// <param name="args">传递参数</param>
        public BaseUIController ShowUI(EUIInstanceID uiInstanceID)
        {
            if (!m_AllInstantiateUI.ContainsKey(uiInstanceID))//如果界面没有生成过
            {
                //根据界面枚举找到对应加载预制体的路径

                if (pUIConfig == null)
                {
                    LogManager.LogError("没有读取到界面配置");
                    return null;
                }

                foreach (var item in pUIConfig.vUIConfigs)
                {
                    if (item.eUIInstanceID == uiInstanceID)
                    {
                        BaseUIController uiGameObject = ResourceLoadManager.Instance.Load<BaseUIController>(item.PrefabPath);
                        BaseUIController controller = Instantiate<BaseUIController>(uiGameObject, transform, false);

                        controller.OnCreated();
                        controller.IsShowState = true;
                        controller.OnShow();

                        var view = controller.View;
                        view.UIInstanceID = uiInstanceID;
                        view.OnCreated(item);
                        view.SetAnchoredPosition(m_ShowPos);
                        view.OnShow();

                        //todo: 在这边也进行model层的初始化,不一定所有界面都有model(数据层)
                        m_AllInstantiateUI.Add(view.UIInstanceID, controller);

                        return controller;
                    }
                }

                LogManager.LogError("没有注册界面:" + uiInstanceID);
                return null;

            }
            else//已经生成过界面
            {

                BaseUIController controller = m_AllInstantiateUI[uiInstanceID];
                controller.IsShowState = true;
                controller.OnShow();

                BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
                view.SetAnchoredPosition(m_ShowPos);
                view.OnShow();

                return controller;
            }
        }



        /// <summary>
        /// 隐藏UI界面
        /// </summary>
        /// <param name="ui"></param>
        public void HideUI(EUIInstanceID uiInstanceID)
        {
            if (m_AllInstantiateUI.ContainsKey(uiInstanceID))//如果界面生成过
            {

                BaseUIController controller = m_AllInstantiateUI[uiInstanceID];
                controller.IsShowState = false;
                controller.transform.localPosition = m_HidePos;
                controller.OnHide();

                BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
                view.SetAnchoredPosition(m_HidePos);
                view.OnHide();
            }
        }

        /// <summary>
        /// 隐藏并且摧毁界面,同时会清除缓存的记录
        /// </summary>
        /// <param name="uiInstanceID"></param>
        public void HideAndDestroy(EUIInstanceID uiInstanceID)
        {
            if (m_AllInstantiateUI.ContainsKey(uiInstanceID))//如果界面生成过
            {
                BaseUIController controller = m_AllInstantiateUI[uiInstanceID];
                controller.IsShowState = false;
                controller.transform.localPosition = m_HidePos;
                controller.OnHide();

                BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
                view.SetAnchoredPosition(m_HidePos);
                view.OnHide();



                Destroy(view.gameObject);
                m_AllInstantiateUI.Remove(uiInstanceID);
            }
        }

        /// <summary>
        /// 判断界面是否显示
        /// </summary>
        /// <param name="ui">ui的对象</param>
        /// <returns></returns>
        public bool IsShowUI(EUIInstanceID uiInstanceID)
        {
            if (m_AllInstantiateUI.ContainsKey(uiInstanceID))
            {
                return m_AllInstantiateUI[uiInstanceID].IsShowState;
            }

            LogManager.LogError("没有找到对应UI界面的ID:" + (int)uiInstanceID);
            return false;
        }
        /// <summary>
        /// 根据界面id判断界面是否显示
        /// </summary>
        /// <param name="uiInstanceID">界面唯一id</param>
        /// <returns></returns>
        public bool IsShowUI(int uiInstanceID)
        {
            if (m_AllInstantiateUI.ContainsKey((EUIInstanceID)uiInstanceID))
            {
                return m_AllInstantiateUI[(EUIInstanceID)uiInstanceID].IsShowState;
            }
            LogManager.LogError("没有找到对应UI界面的ID");
            return false;
        }

        /// <summary>
        /// 获取某个界面控制
        /// </summary>
        /// <param name="uiInstanceID"></param>
        /// <returns></returns>
        public BaseUIController GetUIController(EUIInstanceID uiInstanceID)
        {
            if (m_AllInstantiateUI.ContainsKey(uiInstanceID))
            {
                return m_AllInstantiateUI[uiInstanceID];
            }
            return null;
        }
    }
}