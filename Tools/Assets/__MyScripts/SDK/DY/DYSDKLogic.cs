#if USE_DY_SDK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z.SDK;

public class DYSDKLogic 
{

    Main m_Main;
    /// <summary>
    /// 是否支持侧边栏进入小游戏
    /// </summary>
    bool m_IsSupportSideBar = false;
    /// <summary>
    /// 是否领取过侧边栏进入奖励
    /// </summary>
    bool m_IsReveivedSidebarReward = false;

    DYSDKController m_DYSDK;

    public void Awake(Main main)
    {
        m_Main = main;

        m_DYSDK = SDKManager.Instance.CurSDK as DYSDKController;
        if (m_DYSDK != null)
        {

            m_DYSDK.RefreshGuidePanelAction += RefreshGuidePanel;
            //m_DYSDK.ListenerScene();
        }
    }


    private void OnShowOneParam(Dictionary<string, object> param)
    {
        foreach (var item in param)
        {
            Debug.Log($"OnShowOneParam--> key:{item.Key},value:{item.Value}");
        }


        //根据参数判断是否从侧边栏进入,是则切换成可以领取侧边栏奖励
        //启动场景 （key）         launchFrom返回值     location返回值
        //抖音首页侧边栏（value） "homepage"            sidebar_card
        //启动场景值（「侧边栏」启动返回的场景值为：021036） 011004  我的-小程序列表-最近使用
        //

        bool isLaunchFromSideBar = ((param.ContainsKey("launchFrom") && (string)param["launchFrom"] == "homepage"
             && param.ContainsKey("location") && (string)param["location"] == "sidebar_card")
             || param.ContainsKey("scene") && (string)param["scene"] == "011004"
             || param.ContainsKey("scene") && (string)param["scene"] == "021036");

        if (isLaunchFromSideBar)
        {
            //侧边栏用户，处理发奖逻辑
            RefreshGuidePanel(true);
            Debug.Log("从侧边栏进入!");
        }
        else
        {
            RefreshGuidePanel(false);
            Debug.Log("未侧边栏进入!!");
        }
    }

    public void OnGameStart()
    {
        //todo:判断是否已经领取过奖励?每日一次或者每周一次?需要讨论一下
        m_IsReveivedSidebarReward = false;

        m_Main.dySideBarGuidePanel.SetActive(false);
        m_Main.dySliderRewardButton.gameObject.SetActive(false);

        //判断是否现实侧边栏
        CheckDYSliderRweard();

        Debug.Log($"分辨率:{Screen.width},{Screen.height}");
    }


    public void Update()
    {
        
    }


    private void CheckDYSliderRweard()
    {
        //确认当前宿主版本是否支持跳转侧边栏小游戏入口场景。
        if (m_DYSDK != null)
        {
            m_DYSDK.CheckScene(OnCheckSceneSuccessCallback, OnCheckSceneCompleteCallback, OnCheckSceneErrorCallback);
        }
            

            
         
    }

    private void OnCheckSceneErrorCallback(int arg1, string arg2)
    {
        //接口调用失败的回调函数
    }

    private void OnCheckSceneCompleteCallback()
    {
        //接口调用结束的回调函数（调用成功、失败都会执行）
    }

    private void OnCheckSceneSuccessCallback(bool result)
    {
        //接口调用成功的回调函数, bool为true说明支持，false表示不支持
        m_IsSupportSideBar = result;

        if (m_IsReveivedSidebarReward == true)//已领取奖励
        {
            //todo:不显示领取按钮
            return;
        }

        if (result == false)//不支持抖音侧边栏打开
        {
            return ;
        }

        m_Main.dySliderRewardButton.gameObject.SetActive(true);
        m_Main.dySliderRewardButton.onClick.RemoveAllListeners();
        m_Main.dySliderRewardButton.onClick.AddListener(OndySliderRewardButtonClick);

        //未领取时,判断是否从抖音侧边栏打开
        bool isLaunchFromSlider = false;
        if (m_DYSDK != null)
        {
            isLaunchFromSlider = m_DYSDK.IsLaunchFromSlider();

        }

        RefreshGuidePanel(isLaunchFromSlider);
    }

    void RefreshGuidePanel(bool isLaunchFromSlider)
    {
        //是,显示可以领取按钮
        m_Main.dySliderRewardReceiveButton.gameObject.SetActive(isLaunchFromSlider);
        m_Main.dySliderRewardReceiveButton.onClick.RemoveAllListeners();
        m_Main.dySliderRewardReceiveButton.onClick.AddListener(dySliderRewardReceiveButtonClick);
        //不是,显示引导侧边栏跳转按钮
        m_Main.dySliderRewardNavigationButton.gameObject.SetActive(!isLaunchFromSlider);
        m_Main.dySliderRewardNavigationButton.onClick.RemoveAllListeners();
        m_Main.dySliderRewardNavigationButton.onClick.AddListener(dySliderRewardNavigationButtonClick);
    }

    private void dySliderRewardReceiveButtonClick()
    {
        //领取奖励
        ReceiveSidebarReward();
        Debug.Log("领取侧边栏进入奖励!");
        
        m_Main.dySliderRewardButton.gameObject.SetActive(false);
        m_Main.dySideBarGuidePanel.gameObject.SetActive(false);
    }

    void ReceiveSidebarReward()
    {
        //todo:下发奖励
    }

    private void dySliderRewardNavigationButtonClick()
    {
        if (m_DYSDK != null)
        {
            m_DYSDK.NavigateToSideBarScene(NavigateToSideBarSceneSuccessCallback, null, null);
        }
        
        m_Main.dySideBarGuidePanel.gameObject.SetActive(false);
    }

    private void NavigateToSideBarSceneSuccessCallback()
    {
        //成功跳转
        Debug.Log("成功跳转侧边栏");
    }

    private void OndySliderRewardButtonClick()
    {
        m_Main.dySideBarGuidePanel.SetActive(true);
    }

    public void OnGameEnd()
    {
        //m_DYSDK?.ShowInterstitialAd();
        m_Main.dySliderRewardButton.gameObject.SetActive(false);
    }
}
#endif