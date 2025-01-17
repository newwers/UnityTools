using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Z.SDK
{
    public struct CustomStyle_Z
    {
        //
        // 摘要:
        //     原生模板广告组件的左上角横坐标
        public int left;

        //
        // 摘要:
        //     原生模板广告组件的左上角纵坐标
        public int top;

        //
        // 摘要:
        //     原生模板广告组件的宽度
        public int width;
    }

    public interface ISDK
    {
        void ShowAllAD();
        void CreateBannerAdAndShow();
        void CreateCustomAdAndShow(string adID, CustomStyle_Z customStyle);
        void CreateInterstitialAd();
        void ShowInterstitialAd();
        void CreateRewardVideoAd();
        void ShowRewardVideoAd(System.Action successAction, System.Action failedAction);
        void Login();

        void SetRankData();

        void GetRankData();
    }


    public interface ISDKUI
    {

        public Button LoginButton
        {
            get;
        }

        public Button GetRankDataButton
        {
            get;
        }

        public Button SetRankDataButton
        {
            get;
        }

    }

    public interface IDY_SDK_UI
    {

        public GameObject dySideBarGuidePanel
        {
            get;
        }

        public Button dySliderRewardButton
        {
            get;
        }

        public Button dySliderRewardReceiveButton
        {
            get;
        }

        public Button dySliderRewardNavigationButton
        {
            get;
        }
    }

    /// <summary>
    /// 提供sdk基础功能,不需要考虑sdk平台问题,由具体实现进行判断调用平台
    /// 
    /// </summary>
    public class SDKManager : MonoBehaviour, ISDK
    {
        [Header("banner广告")]
        public string BannerADID = "adunit-6169d57fcfe9fd13";
        [Header("激励广告")]
        public string RewardedVideoADID = "adunit-507f956db4fdd2a1";
        [Header("插屏广告")]
        public string InterstitialAdID = "adunit-77f22f8984aa19e2";//插屏广告
        [Header("格子广告 原生1*1左")]
        public string CustomAdID1 = "adunit-7d240ca18682a11d";//格子广告 原生1*1左
        [Header("格子广告 原生1*1右")]
        public string CustomAdID2 = "adunit-1a15f35f62100b06";//格子广告 原生1*1右
        [Header("格子广告 原生1*5")]
        public string CustomAdID3 = "adunit-7682d9cd0694be14";//格子广告 原生1*5

        public RectTransform GameClubBtnRectTransform;

        ISDK m_CurrentSDK;


        public ISDK CurSDK
        {
            get
            {
                return m_CurrentSDK;
            }
        }
        public static SDKManager Instance { set; get; }
        

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

            InitializeSDK();
        }

        private void Start()
        {
#if !UNITY_EDITOR
            Invoke("ShowAllAD", 4f);
#endif
        }

        public void ShowAllAD()
        {
            if (m_CurrentSDK != null)
            {
                m_CurrentSDK.ShowAllAD();
            }
        }

        void InitializeSDK()
        {
            //根据宏确定使用平台
#if USE_DY_SDK
        m_CurrentSDK = new DYSDKController();
#else
            m_CurrentSDK = new WXAdController();
#endif
            
        }




        public void CreateBannerAdAndShow()
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.CreateBannerAdAndShow();
        }

        public void CreateCustomAdAndShow(string adID, CustomStyle_Z customStyle)
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.CreateCustomAdAndShow(adID, customStyle);
        }

        public void CreateRewardVideoAd()
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.CreateRewardVideoAd();
        }
        public void ShowRewardVideoAd(System.Action successAction, System.Action failedAction)
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.ShowRewardVideoAd(successAction,failedAction);
        }


        public void CreateInterstitialAd()
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.CreateInterstitialAd();
        }

        public void ShowInterstitialAd()
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.ShowInterstitialAd();
        }


        public void Login()
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.Login();
        }

        public void SetRankData()
        {
            
        }

        public void GetRankData()
        {
            
        }



        //广告
        //激励
        //插屏
        //格子

        //登陆

        //排行榜

        //分享?

        //录制视频?

    }

}