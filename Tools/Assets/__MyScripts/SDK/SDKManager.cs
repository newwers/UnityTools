using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WeChatWASM;

namespace Z.SDK
{
    public interface ISDK
    {
        void CreateBannerAdAndShow();
        void CreateCustomAdAndShow(string adID, CustomStyle customStyle);
        void CreateRewardVideoAd();
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
    /// �ṩsdk��������,����Ҫ����sdkƽ̨����,�ɾ���ʵ�ֽ����жϵ���ƽ̨
    /// 
    /// </summary>
    public class SDKManager : MonoBehaviour, ISDK
    {

        ISDK m_CurrentSDK;


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

        void InitializeSDK()
        {
            //todo:���ݺ�ȷ��ʹ��ƽ̨
#if USE_DY_SDK
        m_CurrentSDK = new DouYinSDK();
#else 
            m_CurrentSDK = new WXAdController();
#endif
            var wx = m_CurrentSDK as WXAdController;
            if (wx != null)
            {
                //wx.ShowAllAD();//������ʱû�й��
            }
        }


        public static SDKManager Instance { set; get; }

        public void CreateBannerAdAndShow()
        {
            if (m_CurrentSDK == null)
            {
                return;
            }

            m_CurrentSDK.CreateBannerAdAndShow();
        }

        public void CreateCustomAdAndShow(string adID, CustomStyle customStyle)
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


        //���
        //����
        //����
        //����

        //��½

        //���а�

        //����?

        //¼����Ƶ?

    }

}