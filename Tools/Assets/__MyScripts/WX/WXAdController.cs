using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeChatWASM;

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
public class WXAdController : MonoBehaviour
{

    public static WXAdController Instance { set; get; }

    public string BannerADID = "adunit-6169d57fcfe9fd13";
    public string RewardedVideoADID = "adunit-77f22f8984aa19e2";
    public string InterstitialAdID = "adunit-77f22f8984aa19e2";//插屏广告
    public string CustomAdID1 = "adunit-91a3ca3c75c4e20e";//格子广告 原生1*1左
    public string CustomAdID2 = "adunit-1a15f35f62100b06";//格子广告 原生1*1右
    public string CustomAdID3 = "adunit-7682d9cd0694be14";//格子广告 原生1*5



    WXBannerAd m_BannerAD;
    WXRewardedVideoAd m_RewardedVideoAd;
    WXInterstitialAd m_InterstitialAd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    private void Start()
    {
        CreateBannerAd(OnBannerLoadSuccess, OnCreateBannerError);

        CreateCustomAd(OnCustomADLoad,new CustomStyle()//左 1*1 广告格子
        {
            left = 10,
            top = 200,
            width = 100,
        },CustomAdID1, null);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//右 1*1 广告格子
        {
            left = (int)WX.GetSystemInfoSync().screenWidth - 60,
            top = 200,
            width = 100,
        }, CustomAdID2, null);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//下1*5 广告格子
        {
            left = 10,
            top = (int)WX.GetSystemInfoSync().screenHeight - 150,
            width = (int)WX.GetSystemInfoSync().screenWidth - 100,
        }, CustomAdID3, null);
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


    void Init()
    {
        WX.InitSDK(OnInitWXSDK);//不需要初始化也能直接显示广告
    }


    private void OnInitWXSDK(int code)
    {
        print("code:" + code);

        StartCoroutine(TestAD());
    }

    IEnumerator TestAD()
    {
        yield return new WaitForSecondsRealtime(3);

        CreateBannerAd(OnBannerLoadSuccess, OnCreateBannerError);
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
        print("OnBannerLoadSuccess");


        ShowBannerAD(m_BannerAD, OnShowBannerSuccess, OnShowBannerFailed);
    }

    private void OnCreateBannerError(WXADErrorResponse response)
    {
        print("OnCreateBannerError");
    }

    private void OnShowBannerFailed(WXTextResponse response)
    {
        print("OnShowBannerFailed");
    }

    private void OnShowBannerSuccess(WXTextResponse response)
    {
        print("OnShowBannerSuccess");

    }

    public WXBannerAd CreateBannerAd( Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //每次创建banner都需要销毁旧banner,
        if (m_BannerAD != null)
        {
            m_BannerAD.Destroy();
            m_BannerAD = null;
        }

        if (string.IsNullOrWhiteSpace(BannerADID))//如果不填bannerID的情况下,读取h5的本地存储localstorage
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

        m_BannerAD = bannerAD;

        return bannerAD;
    }

    public void ShowBannerAD(WXBannerAd bannerAd, System.Action<WXTextResponse> successAction, System.Action<WXTextResponse> failedAction)
    {
        if (bannerAd == null) return;

        bannerAd.Show(successAction,failedAction);
        //bannerAd.OnLoad 显示成功调用onload函数
    }

    public void HideBannerAD(WXBannerAd bannerAd)
    {
        if (bannerAd == null) return;

        bannerAd.Hide();
    }

    #endregion

    #region RewardedVideoAd激励视频广告

    public WXRewardedVideoAd CreateRewardVideoAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //每次创建banner都需要销毁旧banner,
        if (m_RewardedVideoAd != null)
        {
            m_RewardedVideoAd.Destroy();
            m_RewardedVideoAd = null;
        }

        //if (string.IsNullOrWhiteSpace(RewardedVideoADID))//如果不填adID的情况下,读取h5的本地存储localstorage
        //{
        //    RewardedVideoADID = GetBannerIDStorageString();
        //}

        //创建Banner广告,但是还未展示
        WXRewardedVideoAd ad = WX.CreateRewardedVideoAd(new  WXCreateRewardedVideoAdParam()
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
        print("OnRewardedVideoAdClose");
        
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
    }


    #endregion

    #region InterstitialAD插屏广告

    public WXInterstitialAd CreateInterstitialAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //每次创建banner都需要销毁旧banner,
        if (m_InterstitialAd != null)
        {
            m_InterstitialAd.Destroy();
            m_InterstitialAd = null;
        }

        //if (string.IsNullOrWhiteSpace(RewardedVideoADID))//如果不填adID的情况下,读取h5的本地存储localstorage
        //{
        //    RewardedVideoADID = GetBannerIDStorageString();
        //}

        //创建Banner广告,但是还未展示
        WXInterstitialAd ad = WX.CreateInterstitialAd(new  WXCreateInterstitialAdParam()
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

    public WXCustomAd CreateCustomAd(Action<WXADLoadResponse> OnLoadAction, CustomStyle customStyle,string adID, System.Action<WXADErrorResponse> OnErrorAction = null)
    {
        //if (string.IsNullOrWhiteSpace(RewardedVideoADID))//如果不填adID的情况下,读取h5的本地存储localstorage
        //{
        //    RewardedVideoADID = GetBannerIDStorageString();
        //}

        //创建Banner广告,但是还未展示
        WXCustomAd ad = WX.CreateCustomAd(new  WXCreateCustomAdParam()
        {
            adUnitId = adID,
            style= customStyle
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

    
}
