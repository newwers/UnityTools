using GoogleMobileAds.Api;
using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Z.SDK;
/// <summary>
/// Google广告+IAP 通过unity IAP实现Google内购,需要自己实现去广告功能,用户支付后,屏蔽广告
/// </summary>
public class GoogleSDKManager : ISDK, IDetailedStoreListener
{
    private static IStoreController m_StoreController;          // 控制整个IAP系统
    private static IExtensionProvider m_StoreExtensionProvider; // 提供特定平台的扩展功能

    public string noAdsProductId = "免广告商品的id";    // 替换为你在Unity IAP中设置的产品ID

    private BannerView bannerView;
    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;



    Action m_OnInitCallback;


    public void OnClear()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
    }

    public void Init(Action action)
    {
        // 初始化移动广告SDK
        m_OnInitCallback = action;
        MobileAds.Initialize(OnInit);

        // 如果IAP系统还未初始化，则进行初始化
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }

    }

    private void OnInit(InitializationStatus status)
    {
        m_OnInitCallback?.Invoke();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // 添加免广告产品
        builder.AddProduct(noAdsProductId, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyNoAds()
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(noAdsProductId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }



    public void ShowAllAD()
    {
        CreateBannerAdAndShow();
        CreateInterstitialAd();
        CreateRewardVideoAd();
    }

    public void CreateBannerAdAndShow()
    {
        if (IsNoAdsPurchased() == true)
        {
            return;
        }
        // If we already have a banner, destroy the old one.
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        // 创建一个底部的横幅广告
        this.bannerView = new BannerView(SDKManager.Instance.BannerADID, AdSize.Banner, AdPosition.Bottom);

        // 创建一个广告请求
        AdRequest request = new AdRequest();

        // 加载广告
        this.bannerView.LoadAd(request);
    }

    public void CreateCustomAdAndShow(string adID, CustomStyle_Z customStyle)
    {
        throw new NotImplementedException();
    }

    public void CreateInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(SDKManager.Instance.InterstitialAdID, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
            });
    }

    public void ShowInterstitialAd()
    {
        if (IsNoAdsPurchased() == true)
        {
            return;
        }

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    public void CreateRewardVideoAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(SDKManager.Instance.RewardedVideoADID, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
            });

    }


    public void ShowRewardVideoAd(Action successAction, Action failedAction)
    {
        if (IsNoAdsPurchased() == true)
        {
            return;
        }

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // TODO: 在回调中设置用户奖励
                //Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                successAction?.Invoke();
            });
        }
    }

    public void GetRankData()
    {
        throw new NotImplementedException();
    }


    public void Login()
    {
        throw new NotImplementedException();
    }

    public void SetRankData()
    {
        throw new NotImplementedException();
    }

    #region IDetailedStoreListener接口实现
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.id, failureReason));
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error + ",message:" + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, noAdsProductId, System.StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            DisableAds();
        }
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.id, failureReason));
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");

        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;

        // 检查是否已经购买了免广告
        Product noAdsProduct = m_StoreController.products.WithID(noAdsProductId);
        if (noAdsProduct.hasReceipt)
        {
            DisableAds();
        }
    }



    #endregion


    private void DisableAds()
    {
        OnClear();
    }

    // 判断是否已经购买去除广告
    public bool IsNoAdsPurchased()
    {
        if (IsInitialized())
        {
            Product noAdsProduct = m_StoreController.products.WithID(noAdsProductId);
            return noAdsProduct.hasReceipt;
        }
        return false;
    }
}
