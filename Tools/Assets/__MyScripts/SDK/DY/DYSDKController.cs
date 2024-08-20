using StarkSDKSpace;
using StarkSDKSpace.UNBridgeLib.LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static StarkSDKSpace.StarkAccount;

public class DYSDKController : MonoBehaviour
{
    public string bannerAdId;
    public string videoAdId;
    public string interstitialAdId;


    private StarkAdManager.BannerAd m_BannerAD;
    private StarkAdManager.InterstitialAd m_InterstitialAD;

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
        StarkSDKSpace.MockSetting.SwithMockModule(StarkSDKSpace.MockModule.FollowDouyin, true);

        //调用API时会弹出调试框
        StarkSDK.API.FollowDouYinUserProfile(null, null);//Editor下,测试关注抖音号api
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
        StarkSDK.API.GetAccountManager().AuthenticateRealName(successCallback, failedCallback);
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
        StarkSDK.API.GetAccountManager().GetSetting(onGetSettingSuccess, onGetSettingFail);
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
        StarkSDK.API.GetAccountManager().OpenSetting(onOpenSettingSuccess,onOpenSettingFail);
    }

    /// <summary>
    /// 提供小游戏获取抖音权限的能力，展示出抖音权限授权弹窗。
    /// 在使用在接口前，需要小游戏拥有者登录抖音开发平台申请开通小游戏需要的权限。
    /// </summary>
    /// <param name="scopes"></param>
    /// <param name="successCallback"></param>
    /// <param name="failedCallback"></param>
    public void ShowDouyinOpenAuth(Dictionary<string, DouyinPermissionScopeStatus> scopes, OnShowDouyinOpenAuthSuccessCallback successCallback, OnShowDouyinOpenAuthFailedCallback failedCallback)
    {
        /*
         
        new Dictionary<string, DouyinPermissionScopeStatus>()
        {
            {"im", DouyinPermissionScopeStatus.Required},
            {"im.media", DouyinPermissionScopeStatus.OptionalUnselected}, 
        },
         
         */
        StarkSDK.API.GetAccountManager().ShowDouyinOpenAuth(scopes,successCallback,failedCallback);
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
        StarkSDK.API.GetStarkSideBarManager().CheckScene( StarkSideBar.SceneEnum.SideBar,success,complete,error);
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
        StarkSDK.API.GetStarkSideBarManager().NavigateToScene(StarkSideBar.SceneEnum.SideBar, success, complete, error);
    }

    /// <summary>
    /// 感知用户是否是从侧边栏场景进入
    /// 首次及从侧边栏场景中进入
    /// 需要游戏一启动就监听
    /// </summary>
    public void ListenerScene()
    {
        StarkSDK.API.GetStarkAppLifeCycle().OnShowWithDict += OnShowOneParam;
    }

    private void OnShowOneParam(Dictionary<string, object> param)
    {
        print($"OnShowOneParam-->${param.ToString()}");

        //todo:根据参数判断是否从侧边栏进入,是则切换成可以领取侧边栏奖励
        //启动场景 （key）         launchFrom返回值     location返回值
        //抖音首页侧边栏（value） "homepage"            sidebar_card

        //还需要判断奖励是否已经领过?
    }

    /// <summary>
    /// 判断是否从侧边栏访问
    /// 游戏中途从侧边栏中复访
    /// </summary>
    /// <returns></returns>
    private bool IsLaunchFromSlider()
    {
        string launchForm = StarkSDK.s_ContainerEnv.GetLaunchFrom();
        string location = StarkSDK.s_ContainerEnv.GetLocation();

        print($"launchForm:{launchForm},location:{location}");

        return launchForm.Equals("homepage") || location.Equals("homepage");
    }

    #endregion


    #region 屏幕亮度

    /// <summary>
    /// 设置是否保持屏幕常亮状态
    /// </summary>
    public void SetKeepScreenOn()
    {
        StarkSDK.API.GetStarkScreenManager().SetKeepScreenOn(true, null, null);
    }

    /// <summary>
    /// 设置屏幕亮度
    /// </summary>
    /// <param name="successCallback">在回调函数的参数中获取亮度值</param>
    /// <param name="failCallback"></param>
    public void GetScreenBrightness(Action<double> successCallback,Action<string> failCallback)
    {
        StarkSDK.API.GetStarkScreenManager().GetScreenBrightness(successCallback,failCallback);
    }

    /// <summary>
    /// 设置屏幕亮度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public void SetScreenBrightness(float value,Action successCallback, Action<string> failCallback)
    {
        StarkSDK.API.GetStarkScreenManager().SetScreenBrightness(value,successCallback, failCallback);
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

        StarkSDK.API.GetStarkRank().SetImRankDataV2(paramJson,callback);
    }

    public void GetImRankListV2(JsonData paramJson, Action<bool, string> callback)
    {
        StarkSDK.API.GetStarkRank().GetImRankListV2(paramJson,callback);
    }


    #endregion

    #region 登陆

    public void Login(OnLoginSuccessCallback successCallback, OnLoginFailedCallback failedCallback, bool forceLogin = true)
    {
        StarkSDK.API.GetAccountManager().Login(successCallback, failedCallback, forceLogin );
    }

    #endregion

    #region 分享

    /// <summary>
    /// 设置小游戏转发按钮为显示状态。 转发按钮位于小游戏页面右上角的“更多”中。 
    /// 详细信息：https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/api/retweet/tt-show-share-menu
    /// </summary>
    public void ShowShareMenu()
    {
        StarkSDK.API.GetStarkShare().ShowShareMenu();
    }

    public void HideShareMenu()
    {
        StarkSDK.API.GetStarkShare().HideShareMenu();
    }

    /// <summary>
    /// 跳转到抖音视频
    /// </summary>
    /// <param name="videoId">抖音视频videoId，复制抖音视频链接到浏览器即可得到</param>
    public void NavigateToVideoView(string videoId)
    {
        StarkSDK.API.NavigateToVideoView(videoId, JumpCallback);
    }

    /// <summary>
    /// 跳转结果，参数为是否成功，true表示成功，false表示失败
    /// </summary>
    /// <param name="result"></param>
    private void JumpCallback(bool result)
    {
        
    }

    /// <summary>
    /// 注：如果只是视频分享，可以直接调用StarkSDK.API.GetStarkGameRecorder().ShareVideo、StarkSDK.API.GetStarkGameRecorder().ShareVideoWithTitleTopics接口。
    /// 不需要调用StarkSDK.API.GetStarkShare()接口。
    /// StarkSDK.API.GetStarkShare() 是一个通用的分享接口，它可以分享视频，也可以分享其它类型的内容，具体参考小程序开发文档，不过就需要自己封装Json参数。
    /// </summary>
    public StarkShare GetStarkShare()
    {
        return StarkSDK.API.GetStarkShare();
    }

    /// <summary>
    /// 
    /// </summary>
    void ExampleShare()
    {
        /*//分享视频json数据格式
         {
            "channel": "video",
            "title": "Some Title",
            "extra": {
                "videoPath": "/xxx/xxx.mp4",//OnRecordComplete拿到 录屏文件 所在路径
                "videoTopics": ["Some Topic1", "Some Topic2"],
                "hashtag_list": ["Some Topic1", "Some Topic2"],
            }
          }

         */

        JsonData shareJson = new JsonData();
        shareJson["channel"] = "video";
        shareJson["title"] = "Some Title";
        shareJson["extra"] = new JsonData();
        shareJson["extra"]["videoPath"] = "/xxx/xxx.mp4";//OnRecordComplete拿到 录屏文件 所在路径
        JsonData videoTopics = new JsonData();
        videoTopics.SetJsonType(JsonType.Array);
        videoTopics.Add("Some Topic1");
        videoTopics.Add("Some Topic2");
        shareJson["extra"]["videoTopics"] = videoTopics;
        shareJson["extra"]["hashtag_list"] = videoTopics;
        StarkSDK.API.GetStarkShare().ShareAppMessage((data) =>
        {
            // Share succeed
        }, (errMsg) =>
        {
            // Share failed
        }, () =>
        {
            // Share cancelled
        },
            shareJson);
    }

    /// <summary>
    /// 带标题和话题的分享视频. 分享的视频文件是调用StopVideo后生成的文件。 注意：视频分享需要录制至少3s的视频，低于3s的视频将会分享失败。
    /// </summary>
    public void ShareVideoWithTitleTopics(string title, List<string> topics)
    {
        StarkSDK.API.GetStarkGameRecorder().ShareVideoWithTitleTopics(SuccessCallback, FailedCallback, CancelledCallback,title,topics);
    }

    /// <summary>
    /// 分享视频。分享的视频文件是调用StopVideo后生成的文件。 注意：视频分享需要录制至少3s的视频，低于3s的视频将会分享失败
    /// </summary>
    public void ShareVideo()
    {
        StarkSDK.API.GetStarkGameRecorder().ShareVideo(SuccessCallback, FailedCallback, CancelledCallback);
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
        StarkSDK.API.CreateShortcut(OnCreateShortcut);
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
        StarkSDK.API.FollowDouYinUserProfile(OnFollowAwemeCallback, OnFollowAwemeError);

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
        StarkSDK.API.GetStarkGameRecorder().StartRecord(m_IsRecordAudio, m_MaxRecordTime, OnRecordStart, OnRecordError, OnRecordTimeout);
    }

    public void StopRecord()
    {
        m_IsRecordAudio = StarkSDK.API.GetStarkGameRecorder().StopRecord();
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

    /// <summary>
    /// 正常激励广告
    /// </summary>
    public void CreateVideoAD()
    {
        var adManager = StarkSDK.API.GetStarkAdManager();
        adManager.ShowVideoAdWithId(videoAdId, OnVideoCloseCallback, OnVideoErrorCallback , null);
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
        var adManager = StarkSDK.API.GetStarkAdManager();
        adManager.ShowVideoAdWithId(videoAdId, multiton, multitonRewardMsg, multitonRewardTime, progressTip, closeCallback, errCallback);
    }

    private void OnVideoErrorCallback(int arg1, string arg2)
    {
        
    }

    private void OnVideoCloseCallback(bool obj)
    {
        
    }

    #endregion

    #region InterstitialAd

    public void CreateInterstitialAD()
    {
        var adManager = StarkSDK.API.GetStarkAdManager();
        m_InterstitialAD = adManager.CreateInterstitialAd(interstitialAdId, OnInterstitialErrorCallback, OnInterstitialCloseCallback, OnInterstitialLoadCallback);
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
        print("销毁插屏AD");
        if (m_InterstitialAD != null)
            m_InterstitialAD.Destroy();
        m_InterstitialAD = null;
    }

    public void ShowInterstitialAd()
    {
        print("显示插屏AD");
        if (m_InterstitialAD != null)
            m_InterstitialAD.Show();
        else
        {
            print("插屏AD未创建");
        }
    }

    void LoadInterstitialAd()
    {
        if (m_InterstitialAD != null)
            m_InterstitialAD.Load();
        else
        {
            print("插屏AD未创建");
        }
    }

    #endregion

    #region BannerAD



    public void CreateBannerAD()
    {
        var adManager = StarkSDK.API.GetStarkAdManager();
        m_BannerAD = adManager.CreateBannerAd(bannerAdId, new StarkAdManager.BannerStyle()
        {
            left = 0,
            top = 0,
            width = 320
        },60,OnBannerErrorCallback,OnBannerLoadCallback,OnBannerResizeCallback);
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
}
