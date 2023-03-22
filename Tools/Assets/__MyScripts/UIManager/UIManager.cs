using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : BaseMonoSingleClass<UIManager>
{
    /// <summary>
    /// 存放所有生成出来的界面
    /// </summary>
    public Dictionary<EUIInstanceID,BaseUIController> m_AllInstantiateUI;

    public UIScriptable pUIConfig;


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
    public void ShowUI(EUIInstanceID uiInstanceID)
    {
        if (!m_AllInstantiateUI.ContainsKey(uiInstanceID))//如果界面没有生成过
        {
            //根据界面枚举找到对应加载预制体的路径

            if (pUIConfig == null)
            {
                LogManager.LogError("没有读取到界面配置");
                return;
            }

            foreach (var item in pUIConfig.vUIConfigs)
            {
                if (item.eUIInstanceID == uiInstanceID)
                {
                    GameObject uiGameObject = ResourceLoadManager.Instance.Load<GameObject>(item.PrefabPath);
                    BaseUIView view = Instantiate(uiGameObject,transform,false).GetComponent<BaseUIView>();
                    view.UIInstanceID = uiInstanceID;
                    view.OnCreated(item);
                    view.OnShow();
                    BaseUIController controller = view.GetComponent<BaseUIController>();
                    controller.OnCreated();
                    controller.OnShow();
                    //todo: 在这边也进行model层的初始化,不一定所有界面都有model(数据层)
                    m_AllInstantiateUI.Add(view.UIInstanceID, controller);
                }
            }



        }
        else//已经生成过界面
        {
            BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
            view.OnShow();
            BaseUIController controller = view.GetComponent<BaseUIController>();
            controller.OnShow();
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
            BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
            view.OnHide();
            BaseUIController controller = view.GetComponent<BaseUIController>();
            controller.OnHide();
            view.gameObject.SetActive(false);//todo:隐藏方式修改为移动位置
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
            BaseUIView view = m_AllInstantiateUI[uiInstanceID].View;
            view.OnHide();
            BaseUIController controller = view.GetComponent<BaseUIController>();
            controller.OnHide();
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
        if (m_AllInstantiateUI.ContainsKey((EUIInstanceID)uiInstanceID))
        {
            return m_AllInstantiateUI[(EUIInstanceID)uiInstanceID].View.isShowState;
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
