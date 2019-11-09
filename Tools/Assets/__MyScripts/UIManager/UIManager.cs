using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    /// <summary>
    /// 存放所有注册的界面
    /// </summary>
    private List<UIInstanceIDEnum> m_AllRegisterUI;
    /// <summary>
    /// 存放所有生成出来的界面
    /// </summary>
    public Dictionary<UIInstanceIDEnum,BaseUIController> m_AllInstantiateUI;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            m_AllRegisterUI = new List<UIInstanceIDEnum>();
            m_AllInstantiateUI = new Dictionary<UIInstanceIDEnum, BaseUIController>();
        }


        //注册界面
        StartRegionUI();
    }



    private void StartRegionUI()
    {
        RegisterUI(UIInstanceIDEnum.PlantInfoPanel);
        RegisterUI(UIInstanceIDEnum.Bag);
        RegisterUI(UIInstanceIDEnum.MainUIPanel);
        RegisterUI(UIInstanceIDEnum.Soil1);
        RegisterUI(UIInstanceIDEnum.Soil2);
        RegisterUI(UIInstanceIDEnum.Soil3);
        RegisterUI(UIInstanceIDEnum.Soil4);
        RegisterUI(UIInstanceIDEnum.Soil5);
        RegisterUI(UIInstanceIDEnum.Soil6);
        RegisterUI(UIInstanceIDEnum.Soil7);
        RegisterUI(UIInstanceIDEnum.Soil8);
        RegisterUI(UIInstanceIDEnum.Soil9);
        RegisterUI(UIInstanceIDEnum.Soil10);
    }

    /// <summary>
    /// 注册UI界面
    /// </summary>
    public void RegisterUI(UIInstanceIDEnum uIInstanceID)
    {
        if (!m_AllRegisterUI.Contains(uIInstanceID))
        {
            m_AllRegisterUI.Add(uIInstanceID);
        }
        else
        {
            LogManager.LogError("重复注册界面:" + uIInstanceID);
        }
    }


    /// <summary>
    /// 界面取消注册
    /// </summary>
    /// <param name="ui"></param>
    public void UnRegisterUI(BaseUIView ui)
    {
        if (!m_AllRegisterUI.Contains((UIInstanceIDEnum)ui.UIInstanceID))
        {
            LogManager.LogError("移除不存在的界面:" + ui.UIInstanceID);
        }
        else
        {
            m_AllRegisterUI.Remove((UIInstanceIDEnum)ui.UIInstanceID);
            m_AllInstantiateUI.Remove((UIInstanceIDEnum)ui.UIInstanceID);
        }
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <param name="uiInstanceID">UI唯一实例ID</param>
    /// <param name="uiGameObject">UI界面游戏物体</param>
    /// <param name="args">传递参数</param>
    public void ShowUI(UIInstanceIDEnum uiInstanceID, object args = null)
    {
        if (m_AllRegisterUI.Contains(uiInstanceID))//如果界面有注册
        {
            if (!m_AllInstantiateUI.ContainsKey((UIInstanceIDEnum)uiInstanceID))//如果界面没有生成过
            {
                //根据界面枚举找到对应加载预制体的路径
                if (FarmGameManager.Instance.m_UIPathConfig == null)
                {
                    LogManager.LogError("没有读取到界面配置");
                    return;
                }
                foreach (var item in FarmGameManager.Instance.m_UIPathConfig.allUIPath)
                {
                    if (item.id == (int)uiInstanceID)
                    {
                        //LogManager.Log("path=" + item.path);
                        GameObject uiGameObject = ResourceLoadManager.Instance.Load(item.path) as GameObject;
                        BaseUIView view = Instantiate(uiGameObject).GetComponent<BaseUIView>();
                        view.UIInstanceID = (int)uiInstanceID;
                        view.OnCreated(args);
                        view.OnShow(args);
                        BaseUIController controller = view.GetComponent<BaseUIController>();
                        controller.OnCreated(args);
                        controller.OnShow(args);
                        m_AllInstantiateUI.Add((UIInstanceIDEnum)view.UIInstanceID, controller);
                    }
                }

                
                
            }
            else//已经生成过界面
            {
                BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
                view.OnShow(args);
                BaseUIController controller = view.GetComponent<BaseUIController>();
                controller.OnShow(args);
            }
        }
        else
        {
            LogManager.LogError("显示没有注册的界面:" + uiInstanceID);
        }
    }

    

    /// <summary>
    /// 隐藏UI界面
    /// </summary>
    /// <param name="ui"></param>
    public void Hide(UIInstanceIDEnum uiInstanceID)
    {
        if (m_AllRegisterUI.Contains(uiInstanceID))//如果界面有注册
        {
            if (m_AllInstantiateUI.ContainsKey((UIInstanceIDEnum)uiInstanceID))//如果界面生成过
            {
                BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
                view.OnHide();
                BaseUIController controller = view.GetComponent<BaseUIController>();
                controller.OnHide();
                view.gameObject.SetActive(false);
            }
        }
        else
        {
            LogManager.LogError("显示没有注册的界面:" + uiInstanceID);
        }
    }

    /// <summary>
    /// 判断界面是否显示
    /// </summary>
    /// <param name="ui">ui的对象</param>
    /// <returns></returns>
    public bool IsShowUI(UIInstanceIDEnum uiInstanceID)
    {
        if (m_AllInstantiateUI.ContainsKey(uiInstanceID))
        {
            return m_AllInstantiateUI[uiInstanceID].View.isShowState;
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
        if (m_AllInstantiateUI.ContainsKey((UIInstanceIDEnum)uiInstanceID))
        {
            return m_AllInstantiateUI[(UIInstanceIDEnum)uiInstanceID].View.isShowState;
        }
        LogManager.LogError("没有找到对应UI界面的ID");
        return false;
    }
}
