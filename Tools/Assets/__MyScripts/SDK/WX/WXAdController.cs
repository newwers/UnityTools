//#if USE_WX_SDK

using System;
using UnityEngine;
using WeChatWASM;
using Z.SDK;

/// <summary>
/// 微信广告
/// 
/// 测试 appid wxa32d167ad36c905f
/// banner：adunit-6169d57fcfe9fd13
/// 激励：adunit-77f22f8984aa19e2
/// 原生1*1左：adunit-91a3ca3c75c4e20e
/// 原生1*1右：adunit-1a15f35f62100b06
/// 原生1*5：adunit-7682d9cd0694be14
/// </summary>
public class WXAdController : ISDK
{


    public bool IsUseLocalStorageADID = false;

    public string BannerADID = "adunit-6169d57fcfe9fd13";
    public string RewardedVideoADID = "adunit-77f22f8984aa19e2";
    public string InterstitialAdID = "adunit-77f22f8984aa19e2";//插屏广告
    public string CustomAdID1 = "adunit-91a3ca3c75c4e20e";//格子广告 原生1*1左
    public string CustomAdID2 = "adunit-1a15f35f62100b06";//格子广告 原生1*1右
    public string CustomAdID3 = "adunit-7682d9cd0694be14";//格子广告 原生1*5



    WXBannerAd m_BannerAD;
    WXRewardedVideoAd m_RewardedVideoAd;
    WXInterstitialAd m_InterstitialAd;

    
    public WXAdController()
    {
        WX.InitSDK(OnInitCallback);
    }

    private void OnInitCallback(int obj)
    {
        Debug.Log("微信sdk初始化完成");
    }

    public void ShowAllAD()
    {
        CreateBannerAd(OnBannerLoadSuccess, OnCreateBannerError);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//左 1*1 广告格子
        {
            left = 10,
            top = 200,
            width = 100,
        }, CustomAdID1, GetLeftGrid1IDStorageString, null);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//右 1*1 广告格子
        {
            left = (int)WX.GetSystemInfoSync().screenWidth - 60,
            top = 200,
            width = 100,
        }, CustomAdID2, GetRightGrid1IDStorageString, null);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//下1*5 广告格子
        {
            left = 10,
            top = (int)WX.GetSystemInfoSync().screenHeight - 150,
            width = (int)WX.GetSystemInfoSync().screenWidth - 100,
        }, CustomAdID3, GetGrid5IDStorageString, null);

        CreateRewardVideoAd();//创建激励视频广告.等待展示
    }


    private void OnDestroy()
    {
        if (m_BannerAD != null)
        {
            m_BannerAD.Destroy();
            m_BannerAD = null;
        }
        if (m_RewardedVideoAd != null)
        {
            m_RewardedVideoAd.Destroy();
            m_RewardedVideoAd = null;
        }
        if (m_InterstitialAd != null)
        {
            m_InterstitialAd.Destroy();
            m_InterstitialAd = null;
        }
    }




    #region Banner

    /// <summary>
    /// unity端，可以用wx.StorageGetStringSync方法获取广告id
    /// js端（写在game.js文件），wx.setStorageSync("settingAds_bannerId","adunit-8d5a854a7b1296bc") 设置广告id
    /// 原理是使用H5的本地存储localstorage，做中间js和unity的数据传递
    /// </summary>
    /// <returns></returns>
    string GetBannerIDStorageString()
    {
        return WX.StorageGetStringSync("settingAds_bannerId", BannerADID);
    }

    void SetBannerIDStorageString()
    {
        WX.StorageSetStringSync("settingAds_bannerId", BannerADID);
    }

    private void OnBannerLoadSuccess(WXADLoadResponse response)
    {
        //print("OnBannerLoadSuccess");


        ShowBannerAD(m_BannerAD, OnShowBannerSuccess, OnShowBannerFailed);
    }

    private void OnCreateBannerError(WXADErrorResponse response)
    {
        //print("OnCreateBannerError");
    }

    private void OnShowBannerFailed(WXTextResponse response)
    {
        //print("OnShowBannerFailed");
    }

    private void OnShowBannerSuccess(WXTextResponse response)
    {
        //print("OnShowBannerSuccess");

    }


    public void CreateBannerAdAndShow()
    {
        CreateBannerAd(null,null,true);
    }

    public WXBannerAd CreateBannerAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction,bool isShow = false)
    {
        //每次创建banner都需要销毁旧banner,
        if (m_BannerAD != null)
        {
            m_BannerAD.Destroy();
            m_BannerAD = null;
        }

        if (IsUseLocalStorageADID)//如果使用localStorage的情况下,读取h5的本地存储localstorage
        {
            BannerADID = GetBannerIDStorageString();
        }

        //创建Banner广告,但是还未展示
        WXBannerAd bannerAD = WX.CreateBannerAd(new WXCreateBannerAdParam()
        {
            adUnitId = BannerADID,
            style = new Style()
            {
                left = 0,
                top = 0,
                width = (int)WX.GetSystemInfoSync().screenWidth,//获取屏幕宽度,
            },
            //adIntervals = 30//广告刷新间隔
        });



        bannerAD.OnError(OnErrorAction);
        //bannerAD.onErrorAction = OnErrorAction;
        bannerAD.OnLoad(OnLoadAction);

        if (isShow)
        {
            bannerAD.Show();
        }

        m_BannerAD = bannerAD;

        return bannerAD;
    }

    public void ShowBannerAD(WXBannerAd bannerAd, System.Action<WXTextResponse> successAction, System.Action<WXTextResponse> failedAction)
    {
        if (bannerAd == null) return;

        bannerAd.Show(successAction, failedAction);
        //bannerAd.OnLoad 显示成功调用onload函数
    }

    public void HideBannerAD(WXBannerAd bannerAd)
    {
        if (bannerAd == null) return;

        bannerAd.Hide();
    }

    #endregion

    #region RewardedVideoAd激励视频广告

    public void CreateRewardVideoAd()
    {
        CreateRewardVideoAd(null, null);
    }

    public void ShowRewardVideoAd()
    {
        ShowRewardedVideoAD(m_RewardedVideoAd, null, null);
    }

    string GetRewardVideoIDStorageString()
    {
        return WX.StorageGetStringSync("settingAds_rewardId", RewardedVideoADID);
    }

    public WXRewardedVideoAd CreateRewardVideoAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //每次创建banner都需要销毁旧banner,
        //if (m_RewardedVideoAd != null)
        //{
        //    m_RewardedVideoAd.Destroy();
        //    m_RewardedVideoAd = null;
        //}

        if (IsUseLocalStorageADID)//如果使用localStorage的情况下,读取h5的本地存储localstorage
        {
            RewardedVideoADID = GetRewardVideoIDStorageString();
        }

        //创建Banner广告,但是还未展示
        WXRewardedVideoAd ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
        {
            adUnitId = RewardedVideoADID
        });



        ad.OnError(OnErrorAction);
        ad.OnLoad(OnLoadAction);

        m_RewardedVideoAd = ad;

        return ad;
    }

    public void ShowRewardedVideoAD(WXRewardedVideoAd ad, System.Action<WXTextResponse> successAction, System.Action<WXTextResponse> failedAction)
    {
        if (ad == null) return;

        ad.Show(successAction, failedAction);

        //设置监听是否完成观看
        ad.onCloseAction = OnRewardedVideoAdClose;
    }

    /// <summary>
    /// //用户需要观看一定时长的广告后,才能点击关闭按钮获得奖励,所以需要进行判断是否观看
    /// </summary>
    /// <param name="response"></param>
    private void OnRewardedVideoAdClose(WXRewardedVideoAdOnCloseResponse response)
    {
        //print("OnRewardedVideoAdClose");

        // 用户点击了【关闭广告】按钮
        // 小于 2.1.0 的基础库版本，res 是一个 undefined
        if (response != null && response.isEnded || response == null)
        {
            // 正常播放结束，可以下发游戏奖励
        }
        else
        {
            // 播放中途退出，不下发游戏奖励
        }

        //CreateRewardVideoAd();//播放完后,创建新的激励视频广告等待展示
    }


    #endregion

    #region InterstitialAD插屏广告

    string GetInterstitiaIDStorageString()
    {
        return WX.StorageGetStringSync("settingAds_interstitiaId", InterstitialAdID);
    }

    public WXInterstitialAd CreateInterstitialAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //每次创建banner都需要销毁旧banner,
        if (m_InterstitialAd != null)
        {
            m_InterstitialAd.Destroy();
            m_InterstitialAd = null;
        }

        if (IsUseLocalStorageADID)//如果不填adID的情况下,读取h5的本地存储localstorage
        {
            InterstitialAdID = GetInterstitiaIDStorageString();
        }

        //创建Banner广告,但是还未展示
        WXInterstitialAd ad = WX.CreateInterstitialAd(new WXCreateInterstitialAdParam()
        {
            adUnitId = InterstitialAdID
        });



        ad.OnError(OnErrorAction);
        ad.OnLoad(OnLoadAction);

        m_InterstitialAd = ad;

        return ad;
    }

    public void ShowInterstitialAd(WXInterstitialAd ad, System.Action<WXTextResponse> successAction, System.Action<WXTextResponse> failedAction)
    {
        if (ad == null) return;

        ad.Show(successAction, failedAction);
    }

    #endregion

    #region CustomAD格子广告

    string GetGrid5IDStorageString()
    {
        return WX.StorageGetStringSync("settingAds_moreId", CustomAdID3);
    }

    string GetLeftGrid1IDStorageString()
    {
        return WX.StorageGetStringSync("settingAds_leftId", CustomAdID1);
    }

    string GetRightGrid1IDStorageString()
    {
        return WX.StorageGetStringSync("settingAds_rightId", CustomAdID2);
    }


    public void CreateCustomAdAndShow(string adID,CustomStyle customStyle)
    {
        CreateCustomAd(null, customStyle, adID, null, null);
    }

    public WXCustomAd CreateCustomAd(Action<WXADLoadResponse> OnLoadAction, CustomStyle customStyle, string adID, Func<string> getIDFunc, System.Action<WXADErrorResponse> OnErrorAction = null)
    {
        if (IsUseLocalStorageADID && getIDFunc != null)//如果不填adID的情况下,读取h5的本地存储localstorage
        {
            adID = getIDFunc();
        }

        //创建Banner广告,但是还未展示
        WXCustomAd ad = WX.CreateCustomAd(new WXCreateCustomAdParam()
        {
            adUnitId = adID,
            style = customStyle
        });


        ad.OnError(OnErrorAction);
        ad.OnLoad(OnLoadAction);

        ad.Show();

        return ad;
    }

    public void ShowCustomAd(WXCustomAd ad, System.Action<WXTextResponse> successAction, System.Action<WXTextResponse> failedAction)
    {
        if (ad == null) return;

        ad.Show(successAction, failedAction);
    }

    public void HideCustomAd(WXCustomAd ad)
    {
        if (ad == null) return;

        ad.Hide();
    }


    private void OnCustomADLoad(WXADLoadResponse response)
    {
        //ShowCustomAd(m_CustomAd, null, null);
    }


    #endregion


    public void Login()
    {
        Debug.Log("微信登陆");
        LoginOption loginOption = new LoginOption();
        loginOption.success = OnLoginSuccess;
        WX.Login(loginOption);
    }

    private void OnLoginSuccess(LoginSuccessCallbackResult result)
    {
        Debug.Log("OnLoginSuccess.code: " + result.code + ",errMsg:" +  result.errMsg);//这将生成一个登录凭证（code），您可以使用该凭证换取用户信息
        //查看授权：在获取用户信息之前，您需要检查用户是否已经授权您的游戏获取其信息。使用WX.GetSetting()接口查询用户的授权情况：
        //WX.auth
    }

    public void SetRankData()
    {
        //WX.SetUserCloudStorage() 设置用户数据
    }

    public void GetRankData()
    {
        //wx.getFriendCloudStorage()获取好友的游戏数据列表。但请注意，在Unity的SDK中没有wx.getFriendCloudStorage()，这个API只能在开放数据域中使用 。
        WX.GetOpenDataContext().PostMessage("向开放数据域发送的消息");//主域和开放数据域之间需要通过特定的通信机制来交换数据。
    }

    public void ShowOpenData()
    {
        //WX.ShowOpenData()//现实排行榜
    }

    public void HideOpenData()
    {
        WX.HideOpenData();//隐藏排行榜
    }



}
//#endif