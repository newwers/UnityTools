#if USE_DY_SDK

using System;
using System.Collections.Generic;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using Z.SDK;
using static TTSDK.TTAccount;
using static TTSDK.TTAppLifeCycle;

public class DYSDKController : ISDK
{

    public Action<bool> RefreshGuidePanelAction;


    private TTBannerAd m_BannerAD;
    private TTInterstitialAd m_InterstitialAD;

    /// <summary>
    /// 录屏时,是否录制音频
    /// </summary>
    private bool m_IsRecordAudio = true;
    /// <summary>
    /// 最大录屏时间
    /// </summary>
    private int m_MaxRecordTime = 10;


    #region 调试运行

    public void SwithMockModule()
    {
        //开启关注抖音号API的MOCK
        TTSDK.MockSetting.SwithMockModule(TTSDK.MockModule.FollowDouyin, true);

        //调用API时会弹出调试框
        TT.OpenAwemeUserProfile(null, null);//Editor下,测试关注抖音号api
    }

    #endregion

    #region 实名认证

    /// <summary>
    /// 拉起实名认证窗口。
    /// 注意：调用该接口前请确保用户已登录。
    /// </summary>
    /// <param name="successCallback">实名认证窗口拉起成功回调,说明用户实名认证成功</param>
    /// <param name="failedCallback">用户实名认证失败</param>
    public void AuthenticateRealName(OnAuthenticateRealNameSuccessCallback successCallback, OnAuthenticateRealNameFailedCallback failedCallback)
    {
        TT.AuthenticateRealName(successCallback, failedCallback);
    }


    #endregion


    #region 授权相关

    /// <summary>
    /// 获取用户已经授权过的配置。
    /// 结果中只会包含向用户请求过的权限。
    /// 与 OpenSetting 的区别是 GetSetting 只会获取配置，而不会打开配置页面。
    /// </summary>
    /// <param name="onGetSettingSuccess"></param>
    /// <param name="onGetSettingFail"></param>
    public void GetSetting(OnGetSettingSuccess onGetSettingSuccess, OnGetSettingFail onGetSettingFail)
    {
        TT.GetSetting(onGetSettingSuccess, onGetSettingFail);
    }

    /// <summary>
    /// 打开设置页面，返回用户设置过的授权结果。
    /// 结果中只会包含用户请求过的权限。
    /// 与 GetSetting 的区别是，OpenSetting 会打开设置页面，而 GetSetting 只会返回用户授权的设置信息。
    /// </summary>
    /// <param name="onOpenSettingSuccess"></param>
    /// <param name="onOpenSettingFail"></param>
    public void OpenSetting(OnOpenSettingSuccess onOpenSettingSuccess, OnOpenSettingFail onOpenSettingFail)
    {
        TT.OpenSetting(onOpenSettingSuccess, onOpenSettingFail);
    }

    /// <summary>
    /// 提供小游戏获取抖音权限的能力，展示出抖音权限授权弹窗。
    /// 在使用在接口前，需要小游戏拥有者登录抖音开发平台申请开通小游戏需要的权限。
    /// </summary>
    /// <param name="scopes"></param>
    /// <param name="successCallback"></param>
    /// <param name="failedCallback"></param>
    public void ShowDouyinOpenAuth(Dictionary<string, TTSDK.DouyinPermissionScopeStatus> scopes, TTAccount.OnShowDouyinOpenAuthSuccessCallback successCallback, TTAccount.OnShowDouyinOpenAuthFailedCallback failedCallback)
    {
        /*
         
        new Dictionary<string, DouyinPermissionScopeStatus>()
        {
            {"im", DouyinPermissionScopeStatus.Required},
            {"im.media", DouyinPermissionScopeStatus.OptionalUnselected}, 
        },
         
         */
        TT.ShowDouyinOpenAuth(scopes, successCallback, failedCallback);
    }

    #endregion


    #region 抖音侧边栏复访相关

    /// <summary>
    /// 判断当前宿主是否支持跳转侧边栏。
    /// </summary>
    /// <param name="success">在成功回调函数中获取结果,true就是支持,false时，不展示奖励入口</param>
    /// <param name="complete"></param>
    /// <param name="error"></param>
    public void CheckScene(Action<bool> success, Action complete, Action<int, string> error)
    {
        TT.CheckScene(TTSideBar.SceneEnum.SideBar, success, complete, error);
    }

    /// <summary>
    /// 跳转到侧边栏小游戏入口场景。
    /// 执行前需要监听场景跳转
    /// 
    /// </summary>
    /// <param name="success"></param>
    /// <param name="complete"></param>
    /// <param name="error"></param>
    public void NavigateToSideBarScene(Action success, Action complete, Action<int, string> error)
    {
        JsonData json = new JsonData();
        json["scene"] = "sidebar";
        TT.NavigateToScene(json, success, complete, error);
    }

    /// <summary>
    /// 感知用户是否是从侧边栏场景进入
    /// 首次及从侧边栏场景中进入
    /// 需要游戏一启动就监听
    /// </summary>
    public void ListenerScene(OnShowEventWithDict action)
    {
        TT.GetAppLifeCycle().OnShow += action;
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
             || param.ContainsKey("scene") && (string)param["scene"] == "011004"//011004 我的 - 小程序列表 - 最近使用
             || param.ContainsKey("scene") && (string)param["scene"] == "021036");//021036 抖音个人页我的小程序（个人简介下方）/ 抖音首页侧边栏

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

    void RefreshGuidePanel(bool isLaunchFromSlider)
    {
        RefreshGuidePanelAction?.Invoke(isLaunchFromSlider);
    }

    /// <summary>
    /// 判断是否从侧边栏访问
    /// 游戏中途从侧边栏中复访
    /// </summary>
    /// <returns></returns>
    public bool IsLaunchFromSlider()
    {
        string launchForm = TT.s_ContainerEnv.GetLaunchFrom();
        string location = TT.s_ContainerEnv.GetLocation();

        Debug.Log($"launchForm:{launchForm},location:{location}");

        return launchForm.Equals("homepage") || location.Equals("homepage");
    }

    #endregion


    #region 屏幕亮度

    /// <summary>
    /// 设置是否保持屏幕常亮状态
    /// </summary>
    public void SetKeepScreenOn()
    {
        TT.SetKeepScreenOn(true, null, null);
    }

    /// <summary>
    /// 设置屏幕亮度
    /// </summary>
    /// <param name="successCallback">在回调函数的参数中获取亮度值</param>
    /// <param name="failCallback"></param>
    public void GetScreenBrightness(Action<double> successCallback, Action<string> failCallback)
    {
        TT.GetScreenBrightness(successCallback, failCallback);
    }

    /// <summary>
    /// 设置屏幕亮度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public void SetScreenBrightness(float value, Action successCallback, Action<string> failCallback)
    {
        TT.SetScreenBrightness(value, successCallback, failCallback);
    }

    #endregion


    #region 排行榜（注：使用前请先保证授权登录！）

    /// https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/open-capacity/game-rank/setImRankData
    /// <summary>
    /// 在关键的游戏场景，设置写入用户的排行榜数据（游戏成绩信息），该数据会上传到服务端
    /// </summary>
    /// <param name="dataType">0：数字类型、1：枚举类型;数字类型（0）往往适用于游戏的通关分数（103分、105分），枚举类型（1）适用于段位信息（青铜、白银）；--(Require)</param>
    /// <param name="value">展示出来的数值，dataType == 0 时只能传正数的字符串，否则会报错。value为具体的值，若dataType为0，请传入数字（eg：103、105）；若dataType为1，则传入字符串（eg：青铜、白银）--(Require)</param>
    /// <param name="priority">dataType 为 1 时，需要传入这个值判断权重，dataType 为 0 时，不填即可--(Require)</param>
    /// <param name="extra">预留字段--(Nullable)</param>
    /// <param name="zoneId">排行榜分区标识--(Nullable)默认值为default</param>
    /// <param name="paramJson">以上参数使用json格式传入，例如"{"dataType":0,"value":"100","priority":0,"zoneId":"default"}"</param>
    /// <param name="action">回调函数</param>
    public void SetImRankDataV2(JsonData paramJson, Action<bool, string> callback)
    {
        //JsonData json = new JsonData();
        //json["dataType"] = 0;
        //json["value"] = 0;
        //json["priority"] = 0;
        //json["extra"] = 0;
        //json["zoneId"] = 0;

        TT.SetImRankData(paramJson, callback);
    }

    public void GetImRankListV2(JsonData paramJson, Action<bool, string> callback)
    {
        TT.GetImRankList(paramJson, callback);
    }


    #endregion

    #region 登陆

    public void Login(OnLoginSuccessCallback successCallback, OnLoginFailedCallback failedCallback, bool forceLogin = true)
    {
        TT.Login(successCallback, failedCallback, forceLogin);
    }

    #endregion

    #region 分享

    /// <summary>
    /// 设置小游戏转发按钮为显示状态。 转发按钮位于小游戏页面右上角的“更多”中。 
    /// 详细信息：https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/api/retweet/tt-show-share-menu
    /// </summary>
    public void ShowShareMenu()
    {
        TT.ShowShareMenu();
    }

    public void HideShareMenu()
    {
        TT.HideShareMenu();
    }

    /// <summary>
    /// 跳转到抖音视频
    /// </summary>
    /// <param name="videoId">抖音视频videoId，复制抖音视频链接到浏览器即可得到</param>
    public void NavigateToVideoView(string videoId)
    {
        TT.NavigateToVideoView(videoId, JumpCallback);
    }

    /// <summary>
    /// 跳转结果，参数为是否成功，true表示成功，false表示失败
    /// </summary>
    /// <param name="result"></param>
    private void JumpCallback(bool result)
    {

    }

    /// <summary>
    /// 注：如果只是视频分享，可以直接调用TT.GetGameRecorder().ShareVideo、TT.GetGameRecorder().ShareVideoWithTitleTopics接口。
    /// 不需要调用TT接口。
    /// TT 是一个通用的分享接口，它可以分享视频，也可以分享其它类型的内容，具体参考小程序开发文档，不过就需要自己封装Json参数。
    /// </summary>
    //public StarkShare GetStarkShare()
    //{
    //    return TT;
    //}

    /// <summary>
    /// 
    /// </summary>
    //void ExampleShare()
    //{
    //    /*//分享视频json数据格式
    //     {
    //        "channel": "video",
    //        "title": "Some Title",
    //        "extra": {
    //            "videoPath": "/xxx/xxx.mp4",//OnRecordComplete拿到 录屏文件 所在路径
    //            "videoTopics": ["Some Topic1", "Some Topic2"],
    //            "hashtag_list": ["Some Topic1", "Some Topic2"],
    //        }
    //      }

    //     */

    //    JsonData shareJson = new JsonData();
    //    shareJson["channel"] = "video";
    //    shareJson["title"] = "Some Title";
    //    shareJson["extra"] = new JsonData();
    //    shareJson["extra"]["videoPath"] = "/xxx/xxx.mp4";//OnRecordComplete拿到 录屏文件 所在路径
    //    JsonData videoTopics = new JsonData();
    //    videoTopics.SetJsonType(JsonType.Array);
    //    videoTopics.Add("Some Topic1");
    //    videoTopics.Add("Some Topic2");
    //    shareJson["extra"]["videoTopics"] = videoTopics;
    //    shareJson["extra"]["hashtag_list"] = videoTopics;
    //    TT.ShareAppMessage((data) =>
    //    {
    //        // Share succeed
    //    }, (errMsg) =>
    //    {
    //        // Share failed
    //    }, () =>
    //    {
    //        // Share cancelled
    //    },
    //        shareJson);
    //}

    /// <summary>
    /// 带标题和话题的分享视频. 分享的视频文件是调用StopVideo后生成的文件。 注意：视频分享需要录制至少3s的视频，低于3s的视频将会分享失败。
    /// </summary>
    public void ShareVideoWithTitleTopics(string title, List<string> topics)
    {
        TT.GetGameRecorder().ShareVideoWithTitleTopics(SuccessCallback, FailedCallback, CancelledCallback, title, topics);
    }

    /// <summary>
    /// 分享视频。分享的视频文件是调用StopVideo后生成的文件。 注意：视频分享需要录制至少3s的视频，低于3s的视频将会分享失败
    /// </summary>
    public void ShareVideo()
    {
        TT.GetGameRecorder().ShareVideo(SuccessCallback, FailedCallback, CancelledCallback);
    }

    private void CancelledCallback()
    {

    }

    private void FailedCallback(string errMsg)
    {

    }

    private void SuccessCallback(Dictionary<string, object> result)
    {

    }

    #endregion


    #region 创建快捷方式

    public void CreateShortcut()
    {
        TT.AddShortcut(OnCreateShortcut);
    }

    private void OnCreateShortcut(bool bSuccess)
    {
        if (bSuccess)
        {
            //创建快捷方式成功
        }
    }

    #endregion

    #region 关注抖音号
    /// <summary>
    /// 关注抖音号功能，平台限制要在日活100以后才能开通，所以入口在没完成开通前，需先关闭
    /// </summary>
    public void FollowDouYinUserProfile()
    {
        TT.OpenAwemeUserProfile(OnFollowAwemeCallback, OnFollowAwemeError);

    }

    private void OnFollowAwemeError(int arg1, string arg2)
    {

    }

    private void OnFollowAwemeCallback()
    {

    }

    #endregion

    #region 录屏

    public void StartRecord()
    {
        TT.GetGameRecorder().Start(m_IsRecordAudio, m_MaxRecordTime, OnRecordStart, OnRecordError, OnRecordTimeout);
    }

    public void StopRecord()
    {
        m_IsRecordAudio = TT.GetGameRecorder().Stop();
    }

    private void OnRecordTimeout(string videoPath)
    {

    }

    private void OnRecordError(int errCode, string errMsg)
    {

    }

    private void OnRecordStart()
    {
        //录屏开始
    }

    #endregion

    #region VideoAD

    public void ShowRewardedVideoAd()
    {
        Debug.Log("请求显示激励视频广告");
        if (m_RewardedVideoAd != null)
        {
            m_RewardedVideoAd.Show();
        }
        else
        {
            Debug.Log("激励视频广告未创建");
        }
    }

    /// <summary>
    /// 正常激励广告
    /// </summary>
    public void CreateVideoAD()
    {
        m_RewardedVideoAd = TT.CreateRewardedVideoAd(SDKManager.Instance.RewardedVideoADID, OnVideoCloseCallback, OnVideoErrorCallback, false, null, 1, false);
        Debug.Log("创建激励视频广告");

    }
    /// <summary>
    /// 连续观看激励广告
    /// 
    /// 该方法调用后，会在 RewardedVideoAd.onClose 返回两个数值，您可以根据数值给用户发放对应的奖励
    /// （isEnded：用户是否完整观看了最后一次拉起的广告” 、 count： 用户一共看了几次广告）
    /// </summary>
    /// <param name="multiton">// 代表创建再得广告</param>
    /// <param name="multitonRewardMsg">多个奖励道具名,例如 "500金币","700金币","900金币","1100金币"</param>
    /// <param name="multitonRewardTime">可以额外再看次数广告 例如:4</param>
    /// <param name="progressTip">代表需要进度提示  例如: false</param>
    /// <param name="closeCallback"></param>
    /// <param name="errCallback"></param>
    public void CreateVideoAD(bool multiton, string[] multitonRewardMsg, int multitonRewardTime, bool progressTip, Action<bool, int> closeCallback = null, Action<int, string> errCallback = null)
    {
        m_RewardedVideoAd = TT.CreateRewardedVideoAd(SDKManager.Instance.RewardedVideoADID, closeCallback, errCallback, multiton, multitonRewardMsg, multitonRewardTime, progressTip);
        Debug.Log("创建激励视频广告");
    }

    private void OnVideoErrorCallback(int errCode, string errMsg)
    {
        Debug.Log($"激励视频Error回调函数：errCode:{errCode},errMsg:{errMsg}");
        m_ShowRewardVideoAdFailedAction?.Invoke();
    }

    private void OnVideoCloseCallback(bool isCompleted, int code)
    {
        if (isCompleted)
        {
            m_ShowRewardVideoAdSuccessAction?.Invoke();
        }
        else
        {
            m_ShowRewardVideoAdFailedAction?.Invoke();
        }
        Debug.Log($"激励视频Close回调函数：isCompleted:{isCompleted},code:{code}");
    }

    #endregion

    #region InterstitialAd

    public void CreateInterstitialAd()
    {
        CreateInterstitialAdParam param = new CreateInterstitialAdParam();
        param.InterstitialAdId = SDKManager.Instance.InterstitialAdID;
        m_InterstitialAD = TT.CreateInterstitialAd(param);
        Debug.Log("创建插屏AD");
    }

    private void OnInterstitialCloseCallback()
    {

    }

    private void OnInterstitialLoadCallback()
    {

    }

    private void OnInterstitialErrorCallback(int arg1, string arg2)
    {

    }

    void DestoryInterstitialAd()
    {
        Debug.Log("销毁插屏AD");
        if (m_InterstitialAD != null)
            m_InterstitialAD.Destroy();
        m_InterstitialAD = null;
    }

    public void ShowInterstitialAd()
    {
        Debug.Log("显示插屏AD");
        if (m_InterstitialAD != null)
            m_InterstitialAD.Show();
        else
        {
            Debug.Log("插屏AD未创建");
        }
    }

    void LoadInterstitialAd()
    {
        if (m_InterstitialAD != null)
            m_InterstitialAD.Load();
        else
        {
            Debug.Log("插屏AD未创建");
        }
    }

    #endregion

    #region BannerAD



    public void CreateBannerAD()
    {
        CreateBannerAdParam param = new CreateBannerAdParam();
        param.BannerAdId = SDKManager.Instance.BannerADID;
        param.Style = new TTBannerStyle()
        {
            left = 0,
            top = 0,
            width = 320
        };
        param.AdIntervals = 60;


        m_BannerAD = TT.CreateBannerAd(param);
    }

    private void OnBannerResizeCallback(int arg1, int arg2)
    {

    }

    private void OnBannerLoadCallback()
    {
        if (m_BannerAD != null)
        {
            m_BannerAD.Show();
        }
    }

    private void OnBannerErrorCallback(int arg1, string arg2)
    {

    }



    #endregion

    #region SDK接口函数

    public void ShowAllAD()
    {
        CreateInterstitialAd();
        CreateRewardVideoAd();
        SetKeepScreenOn();

        ListenerScene(OnShowOneParam);
        Debug.Log("初始化抖音sdk函数");
    }

    public void CreateBannerAdAndShow()
    {
        throw new NotImplementedException();
    }

    public void CreateCustomAdAndShow(string adID, CustomStyle_Z customStyle)
    {
        throw new NotImplementedException();
    }


    public void CreateRewardVideoAd()
    {
        CreateVideoAD();
    }
    Action m_ShowRewardVideoAdSuccessAction;
    Action m_ShowRewardVideoAdFailedAction;
    private TTSDK.TTRewardedVideoAd m_RewardedVideoAd;

    public void ShowRewardVideoAd(Action successAction, Action failedAction)
    {
        m_ShowRewardVideoAdSuccessAction = successAction;
        m_ShowRewardVideoAdFailedAction = failedAction;
        ShowRewardedVideoAd();
    }

    public void Login()
    {
        throw new NotImplementedException();
    }

    public void SetRankData()
    {
        throw new NotImplementedException();
    }

    public void GetRankData()
    {
        throw new NotImplementedException();
    }

    public void OnClear()
    {

    }

    public void Init(Action action)
    {
        action?.Invoke();
    }

    #endregion
}
#endif