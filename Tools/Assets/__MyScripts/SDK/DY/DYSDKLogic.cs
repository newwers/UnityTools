#if USE_DY_SDK
using System;
using System.Collections;
using System.Collections.Generic;
using TTSDK;
using UnityEngine;
using Z.SDK;

public class DYSDKLogic
{

    SDKManager m_Main;
    /// <summary>
    /// 是否支持侧边栏进入小游戏
    /// </summary>
    bool m_IsSupportSideBar = false;
    /// <summary>
    /// 是否领取过侧边栏进入奖励
    /// </summary>
    bool m_IsReveivedSidebarReward = false;

    DYSDKController m_DYSDK;

    public void Awake(SDKManager main)
    {
        m_Main = main;

        m_DYSDK = SDKManager.Instance.CurSDK as DYSDKController;
        if (m_DYSDK != null)
        {

            m_DYSDK.RefreshGuidePanelAction += RefreshGuidePanel;
        }


        m_Main.dySliderRewardButton.onClick.AddListener(OndySliderRewardButtonClick);

        m_Main.dySliderRewardNavigationButton.onClick.AddListener(dySliderRewardNavigationButtonClick);


        m_Main.dySliderRewardReceiveButton.onClick.AddListener(dySliderRewardReceiveButtonClick);

        TT.InitSDK(OnInitSDK);
        
        
    }

    private void OnInitSDK(int code, ContainerEnv env)
    {
        /*
        callback：初始化完成的回调，类型为 OnTTContainerInitCallback，默认值为 null，非必填。 错误码：
        0：无错误
        1：TT Unity SDK 版本不支持
        2：Unity Engine 版本不被支持 代码示例：
         */
        Debug.Log("Unity 初始化抖音sdk callback code:" + code);
        OnGameStart();
    }


    public void OnGameStart()
    {
        //todo:判断是否已经领取过奖励?每日一次或者每周一次?需要讨论一下
        m_IsReveivedSidebarReward = false;

        m_Main.dySideBarGuidePanel.SetActive(false);
        m_Main.dySliderRewardButton.gameObject.SetActive(false);

        //判断是否现实侧边栏
        CheckScene();

        Debug.Log($"分辨率:{Screen.width},{Screen.height}");
    }


    public void Update()
    {

    }

    /// <summary>
    /// //判断是否现实侧边栏
    /// </summary>
    private void CheckScene()
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

        m_Main.dySliderRewardButton.gameObject.SetActive(m_IsSupportSideBar);

        if (m_IsReveivedSidebarReward == true)//已领取奖励
        {
            //todo:不显示领取按钮
            return;
        }

        if (result == false)//不支持抖音侧边栏打开
        {
            return;
        }

        m_Main.dySliderRewardButton.gameObject.SetActive(true);

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
        //不是,显示引导侧边栏跳转按钮
        m_Main.dySliderRewardNavigationButton.gameObject.SetActive(!isLaunchFromSlider);
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
        var gm = GameObject.FindObjectOfType<GameManager>();
        gm.itemManager.AddEnergy(100);
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

    public void GamePause(bool pause)
    {
        
    }

    public void SetRankData(int index)
    {
        TT.SetImRankData(new TTSDK.UNBridgeLib.LitJson.JsonData(index));
    }

    public void GetRankData(TTRank.OnGetRankDataSuccessCallback success, TTRank.OnGetRankDataFailCallback fail)
    {
        TT.GetImRankData(new TTSDK.UNBridgeLib.LitJson.JsonData(),success,fail);
    }

    public void GetRankList(string json)
    {
        TT.GetImRankList(new TTSDK.UNBridgeLib.LitJson.JsonData(json));
    }
}
#endif