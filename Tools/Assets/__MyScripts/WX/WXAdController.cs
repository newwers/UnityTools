using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeChatWASM;

/// <summary>
/// ΢�Ź��
/// 
/// ���� appid wxa32d167ad36c905f
/// banner��adunit-6169d57fcfe9fd13
/// ������adunit-77f22f8984aa19e2
/// ԭ��1*1��adunit-91a3ca3c75c4e20e
/// ԭ��1*1�ң�adunit-1a15f35f62100b06
/// ԭ��1*5��adunit-7682d9cd0694be14
/// </summary>
public class WXAdController : MonoBehaviour
{

    public static WXAdController Instance { set; get; }

    public string BannerADID = "adunit-6169d57fcfe9fd13";
    public string RewardedVideoADID = "adunit-77f22f8984aa19e2";
    public string InterstitialAdID = "adunit-77f22f8984aa19e2";//�������
    public string CustomAdID1 = "adunit-91a3ca3c75c4e20e";//���ӹ�� ԭ��1*1��
    public string CustomAdID2 = "adunit-1a15f35f62100b06";//���ӹ�� ԭ��1*1��
    public string CustomAdID3 = "adunit-7682d9cd0694be14";//���ӹ�� ԭ��1*5



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

        CreateCustomAd(OnCustomADLoad,new CustomStyle()//�� 1*1 ������
        {
            left = 10,
            top = 200,
            width = 100,
        },CustomAdID1, null);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//�� 1*1 ������
        {
            left = (int)WX.GetSystemInfoSync().screenWidth - 60,
            top = 200,
            width = 100,
        }, CustomAdID2, null);

        CreateCustomAd(OnCustomADLoad, new CustomStyle()//��1*5 ������
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
        WX.InitSDK(OnInitWXSDK);//����Ҫ��ʼ��Ҳ��ֱ����ʾ���
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
    /// unity�ˣ�������wx.StorageGetStringSync������ȡ���id
    /// js�ˣ�д��game.js�ļ�����wx.setStorageSync("settingAds_bannerId","adunit-8d5a854a7b1296bc") ���ù��id
    /// ԭ����ʹ��H5�ı��ش洢localstorage�����м�js��unity�����ݴ���
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
        //ÿ�δ���banner����Ҫ���پ�banner,
        if (m_BannerAD != null)
        {
            m_BannerAD.Destroy();
            m_BannerAD = null;
        }

        if (string.IsNullOrWhiteSpace(BannerADID))//�������bannerID�������,��ȡh5�ı��ش洢localstorage
        {
            BannerADID = GetBannerIDStorageString();
        }

        //����Banner���,���ǻ�δչʾ
        WXBannerAd bannerAD = WX.CreateBannerAd(new WXCreateBannerAdParam()
        {
            adUnitId = BannerADID,
            style = new Style()
            {
                left = 0,
                top = 0,
                width = (int)WX.GetSystemInfoSync().screenWidth,//��ȡ��Ļ���,
            },
            //adIntervals = 30//���ˢ�¼��
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
        //bannerAd.OnLoad ��ʾ�ɹ�����onload����
    }

    public void HideBannerAD(WXBannerAd bannerAd)
    {
        if (bannerAd == null) return;

        bannerAd.Hide();
    }

    #endregion

    #region RewardedVideoAd������Ƶ���

    public WXRewardedVideoAd CreateRewardVideoAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //ÿ�δ���banner����Ҫ���پ�banner,
        if (m_RewardedVideoAd != null)
        {
            m_RewardedVideoAd.Destroy();
            m_RewardedVideoAd = null;
        }

        //if (string.IsNullOrWhiteSpace(RewardedVideoADID))//�������adID�������,��ȡh5�ı��ش洢localstorage
        //{
        //    RewardedVideoADID = GetBannerIDStorageString();
        //}

        //����Banner���,���ǻ�δչʾ
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

        //���ü����Ƿ���ɹۿ�
        ad.onCloseAction = OnRewardedVideoAdClose;
    }

    /// <summary>
    /// //�û���Ҫ�ۿ�һ��ʱ���Ĺ���,���ܵ���رհ�ť��ý���,������Ҫ�����ж��Ƿ�ۿ�
    /// </summary>
    /// <param name="response"></param>
    private void OnRewardedVideoAdClose(WXRewardedVideoAdOnCloseResponse response)
    {
        print("OnRewardedVideoAdClose");
        
        // �û�����ˡ��رչ�桿��ť
        // С�� 2.1.0 �Ļ�����汾��res ��һ�� undefined
        if (response != null && response.isEnded || response == null)
        {
            // �������Ž����������·���Ϸ����
        }
        else
        {
            // ������;�˳������·���Ϸ����
        }
    }


    #endregion

    #region InterstitialAD�������

    public WXInterstitialAd CreateInterstitialAd(Action<WXADLoadResponse> OnLoadAction, System.Action<WXADErrorResponse> OnErrorAction)
    {
        //ÿ�δ���banner����Ҫ���پ�banner,
        if (m_InterstitialAd != null)
        {
            m_InterstitialAd.Destroy();
            m_InterstitialAd = null;
        }

        //if (string.IsNullOrWhiteSpace(RewardedVideoADID))//�������adID�������,��ȡh5�ı��ش洢localstorage
        //{
        //    RewardedVideoADID = GetBannerIDStorageString();
        //}

        //����Banner���,���ǻ�δչʾ
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

    #region CustomAD���ӹ��

    public WXCustomAd CreateCustomAd(Action<WXADLoadResponse> OnLoadAction, CustomStyle customStyle,string adID, System.Action<WXADErrorResponse> OnErrorAction = null)
    {
        //if (string.IsNullOrWhiteSpace(RewardedVideoADID))//�������adID�������,��ȡh5�ı��ش洢localstorage
        //{
        //    RewardedVideoADID = GetBannerIDStorageString();
        //}

        //����Banner���,���ǻ�δչʾ
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
