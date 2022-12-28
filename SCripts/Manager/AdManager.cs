using com.adjust.sdk;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MaxSdkBase;



namespace HuynnLib
{
    public enum InterVideoState
    {
        None,
        Open,
        Closed
    }
    public enum RewardVideoState
    {
        None,
        Open,
        Watched
    }

    public class AdManager : Singleton<AdManager>, IChildLib
    {


        [SerializeField]
        bool  _isAdsOpen = true, _isBannerAutoShow =false;
        bool _isAdsBanner = true;
        public bool isAdBanner => _isAdsBanner;


        [SerializeField]
        MaxSdkBase.BannerPosition _bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
        public MaxSdkBase.BannerPosition BannerPosition => _bannerPosition;

        [SerializeField] GameObject _popUpNoAd;

        #region Max

        [Header("---ID---")]
        [Space(10)]
        [SerializeField]
        private string MaxSdkKey = "3N4Mt8SNhOzkQnGb9oHsRRG1ItybcZDpJWN1fVAHLdRagxP-_k_ZXVaMAdMe5Otsmp6qJSXskfsrtakfRmPAGW";
        [SerializeField]
        private string BannerAdUnitID = "df980c4d809fc01e",
            InterstitialAdUnitID = "3a70c7be99dade7d",
            RewardedAdUnitID = "6b7094c5d21fcfe5",
            OpenAdUnitID = "";


        private int bannerRetryAttempt,
            interstitialRetryAttempt,
            rewardedRetryAttempt,
            AdOpenRetryAttemp = 1; 
        private Action<InterVideoState> _callbackInter = null;
        private Action<RewardVideoState> _callbackReward = null;
        #endregion

        private bool isShowingAd = false;


        public void Init(Action _onInitDone = null)
        {
            Debug.Log("==========> Ad start Init! <==========");

            InitMAX();

            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

            _onInitDone?.Invoke();
        }

        #region MAX

        void InitMAX()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                // AppLovin SDK is initialized, configure and start loading ads.
                Debug.Log("==> MAX SDK Initialized <==");

                if(!string.IsNullOrWhiteSpace(BannerAdUnitID))
                    InitializeBannerAds();

                InitializeInterstitialAds();
                InitializeRewardedAds();
                InitAdOpen();

            };
            MaxSdk.SetSdkKey(MaxSdkKey);
            MaxSdk.InitializeSdk();
        }
        #region Banner Ad Methods

        private void InitializeBannerAds()
        {
            Debug.Log("==> Init banner <=="); 
            // Attach Callbacks
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
            {
                Debug.Log("==> Banner ad revenue paid <==");
                TrackAdRevenue(adInfo);
            };

            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
            MaxSdk.CreateBanner(BannerAdUnitID, _bannerPosition);
            MaxSdk.SetBannerExtraParameter(BannerAdUnitID, "adaptive_banner", "false");
            // Set background or background color for banners to be fully functional.
            MaxSdk.SetBannerBackgroundColor(BannerAdUnitID, new Color(1, 1, 1, 0));

            if (_isBannerAutoShow) ShowBanner();
        }

        void ManuallyLoadBanner()
        {
            MaxSdk.StopBannerAutoRefresh(BannerAdUnitID);
            MaxSdk.LoadBanner(BannerAdUnitID);
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (_isAdsBanner)
                this.ShowBanner();
            Debug.Log("==> Banner ad loaded <==");
            MaxSdk.StartBannerAutoRefresh(BannerAdUnitID);
        }

        private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Banner ad failed to load. MAX will automatically try loading a new ad internally.
            Debug.LogError("==>Banner ad failed to load with error code: " + errorInfo.Code+" <==");
            bannerRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt)); 

            Invoke("ManuallyLoadBanner", (float)retryDelay);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("==> Banner ad clicked <==");
        }


        #endregion

        #region Interstitial Ad Methods
        private void InitializeInterstitialAds()
        {
            // Attach callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
            {
                Debug.Log("==> Interstitial revenue paid <==");
                TrackAdRevenue(adInfo);
            };

            // Load the first interstitial
            LoadInterstitial();
        }
        void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(InterstitialAdUnitID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            Debug.Log("==> Interstitial loaded <==");
            // Reset retry attempt
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

            Debug.LogError("==> Interstitial failed to load with error code: " + errorInfo.Code+" <==");

            Invoke("LoadInterstitial", (float)retryDelay);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Debug.LogError("==> Interstitial failed to display with error code: " + errorInfo.Code+" <==");
            
            LoadInterstitial();

            try
            {
                _callbackInter?.Invoke(InterVideoState.None);
            }
            catch (Exception e)
            {
                Debug.LogError("==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }

            _callbackInter = null;
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("==> Interstitial dismissed <==");
            try
            {
                _callbackInter?.Invoke(InterVideoState.None);
            }
            catch (Exception e)
            {
                Debug.LogError("==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }
            _callbackInter = null;
            LoadInterstitial();
            isShowingAd = false;
        }


        #endregion

        #region Reward Ad Methods
        private void InitializeRewardedAds()
        {
            // Attach callbacks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
            {
                Debug.Log("==> Reward paid event! <==");
                TrackAdRevenue(adInfo);
            };


            // Load the first RewardedAd
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(RewardedAdUnitID);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'

            // Reset retry attempt
            rewardedRetryAttempt = 0;
        }

        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            
            rewardedRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));

            Debug.LogError("==> Rewarded ad failed to load with error code: " + errorInfo.Code+" <==");

            Invoke("LoadRewardedAd", (float)retryDelay);
            
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
             
            Debug.LogError("==> Rewarded ad failed to display with error code: " + errorInfo.Code+" <==");
            LoadRewardedAd();
            try
            {
                _callbackReward?.Invoke(RewardVideoState.None);

            }
            catch (Exception e)
            {
                Debug.LogError("==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("==> Reward display success! <==");
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("==> Reward clicked! <==");
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoadRewardedAd();
            isShowingAd = false;
            _callbackReward = null;
            Debug.Log("==> Reward closed! <==");
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {

            try
            {
                _callbackReward?.Invoke(RewardVideoState.Watched);

            }
            catch (Exception e)
            {
                Debug.LogError("==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
            Debug.Log("==> Reward recived!! <==");
        }
        #endregion


        #region AdOpen Methods
        void InitAdOpen()
        {
            Debug.Log("==> Ad open/resume init! <==");


            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += AppOpen_OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += AppOpenOnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += AppOpen_OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
            {
                Debug.Log("==> Ad open/resume paid event! <==");
                TrackAdRevenue(adInfo);
            };

            LoadAdOpen();
        }

        void LoadAdOpen()
        {
            Debug.Log("==> Start load ad open/resume! <==");
            if (!MaxSdk.IsAppOpenAdReady(OpenAdUnitID))
            {
                MaxSdk.LoadAppOpenAd(OpenAdUnitID);
            }
        }


        private void AppOpen_OnAdLoadedEvent(string arg1, AdInfo arg2)
        {
            Debug.Log("==>Load ad open/resume success! <==");
            AdOpenRetryAttemp = 0;
            if (_isAdsOpen)
            {
                if (LoadingManager.Instant != null)
                {
                    LoadingManager.Instant.StopLoading(() =>
                    {
                        ShowAdOpen();
                        _isAdsOpen = false;
                    });
                    return;
                }
                ShowAdOpen();
                _isAdsOpen = false;
            }
        }

        private void AppOpen_OnAdDisplayedEvent(string arg1, AdInfo arg2)
        {
            Debug.Log("==> Show ad open/resume success! <==");
            isShowingAd = true;
            
        }

        private void AppOpen_OnAdDisplayFailedEvent(string arg1, ErrorInfo arg2, AdInfo arg3)
        {
            Debug.LogError("==> Show ad open/resume failed, code: " + arg2.Code+" <==");
            AdOpenRetryAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp));
            Invoke("LoadAdOpen", (float)retryDelay);
          
        }

        private void AppOpenOnAdLoadFailedEvent(string arg1, ErrorInfo arg2)
        {
            Debug.LogError("==> Load ad open/resume failed, code: " + arg2.Code+ " <==");
            AdOpenRetryAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp));
            Invoke("LoadAdOpen", (float)retryDelay);
        }

        
        public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("==> Ad open/resume close! <==");
            isShowingAd = false;
            LoadAdOpen();
        }


        #endregion

        #endregion


        #region CheckAdLoaded

        public bool InterstitialIsLoaded()
        {
            return MaxSdk.IsInterstitialReady(InterstitialAdUnitID);
        }

        public bool VideoRewardIsLoaded()
        {
            return MaxSdk.IsRewardedAdReady(RewardedAdUnitID);
        }

        public bool AdsOpenIsLoaded()
        {
            return MaxSdk.IsAppOpenAdReady(OpenAdUnitID);
        }

        #endregion

        #region ShowAd

        public void ShowBanner()
        {
            Debug.Log("==> show banner <==");
            _isAdsBanner = true;

            if (!string.IsNullOrWhiteSpace(BannerAdUnitID))
                MaxSdk.ShowBanner(BannerAdUnitID);

        }

        public void DestroyBanner()
        {
            Debug.Log("==> destroy banner <==");
            _isAdsBanner = false;

            if (!string.IsNullOrWhiteSpace(BannerAdUnitID))
                MaxSdk.DestroyBanner(BannerAdUnitID);
        }

        public void ShowInterstitial(Action<InterVideoState> callback = null, bool showNoAds = false)
        {
            if (CheckInternetConnection() && InterstitialIsLoaded())
            {
                isShowingAd = true;
                _callbackInter = callback;
                MaxSdk.ShowInterstitial(InterstitialAdUnitID);
            }
            else
            {
                try
                {
                    callback?.Invoke(InterVideoState.None);
                }
                catch (Exception e)
                {
                    Debug.LogError("==> Faild invoke callback inter, error: " + e.ToString() + " <==");
                }
                if (_popUpNoAd && showNoAds) _popUpNoAd.SetActive(true);
            }
        }

        public void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false)
        {
            if (VideoRewardIsLoaded())
            {
                isShowingAd = true;
                _callbackReward = callback;
                MaxSdk.ShowRewardedAd(RewardedAdUnitID);
            }
            else
            {
                try
                {
                    callback?.Invoke(RewardVideoState.None);
                }
                catch (Exception e)
                {
                    Debug.LogError("==> Faild invoke callback reward, error: " + e.ToString() + " <==");
                }
                if (_popUpNoAd && showNoAds) _popUpNoAd.SetActive(true);
            }
        }

        public void ShowAdOpen()
        {
            if (isShowingAd)
                return;
            if (MaxSdk.IsAppOpenAdReady(OpenAdUnitID))
            {
                MaxSdk.ShowAppOpenAd(OpenAdUnitID);
            }
        }
        #endregion

      
        #region Track Revenue

        private void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
        {
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);

            adjustAdRevenue.setRevenue(adInfo.Revenue, "USD");
            adjustAdRevenue.setAdRevenueNetwork(adInfo.NetworkName);
            adjustAdRevenue.setAdRevenueUnit(adInfo.AdUnitIdentifier);
            adjustAdRevenue.setAdRevenuePlacement(adInfo.Placement);

            Adjust.trackAdRevenue(adjustAdRevenue);
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
        {
            double revenue = impressionData.Revenue;
            var impressionParameters = new[] {
              new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
              new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
              new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
              new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
              new Firebase.Analytics.Parameter("value", revenue),
              new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("paid_ad_impression_value", impressionParameters);
        }

        #endregion


#if ADMOB
        //app open
        public void LoadAdOpen()
        {

            if (!_isAdsOpen)
                return;

            AdRequest request = new AdRequest.Builder().Build();

            // Load an app open ad for portrait orientation
            AppOpenAd.LoadAd(AD_UNIT_ID, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
                {

                    if (error != null)
                    {
                        // Handle the error.
                        Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
                        return;
                    }

                    // App open ad is loaded.
                    ad = appOpenAd;
                    loadTime = DateTime.UtcNow;

                }));
        }
        private bool IsAdAvailable
        {
            get
            {
                return ad != null && (System.DateTime.UtcNow - loadTime).TotalHours < 4;
            }
        }
        public void ShowAdIfAvailable()
        {

            if (!CheckInternetConnection())
            {
                return;
            }
            if (!IsAdAvailable)
            {
                LoadAdOpen();
            }
            if (!IsAdAvailable || isShowingAd)
            {
                return;
            }
            ad.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
            ad.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
            ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
            ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
            ad.OnPaidEvent += HandlePaidEvent;
            ad.OnPaidEvent += OnAdResumeRevenuePaidEvent;
            ad.Show();

        }



        private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
        {
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            ad = null;
            isShowingAd = false;
            LoadAdOpen();
        }

        private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
        {
            Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            ad = null;
            LoadAdOpen();
        }

        private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
        {
            isShowingAd = true;
        }

        private void HandleAdDidRecordImpression(object sender, EventArgs args)
        {
        }


        private void HandlePaidEvent(object sender, AdValueEventArgs args)
        {
            Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                    args.AdValue.CurrencyCode, args.AdValue.Value);

            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);

            adjustAdRevenue.setRevenue(args.AdValue.Value/1000000f, "USD");
            adjustAdRevenue.setAdRevenueNetwork("Admob");

            Adjust.trackAdRevenue(adjustAdRevenue);
        }

        private void OnAdResumeRevenuePaidEvent(object sender, AdValueEventArgs args)
        {
            double revenue = args.AdValue.Value / 1000000f;
                var impressionParameters = new[] {
                new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                new Firebase.Analytics.Parameter("ad_format", "APPRESUME"),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("paid_ad_impression_value", impressionParameters);
        }

#endif
        public bool CheckInternetConnection()
        {
            var internet = false;
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                internet = true;
            }
            else
            {
                internet = false;
            }
            return internet;
        }


        private void OnAppStateChanged(AppState state)
        {

            // Display the app open ad when the app is foregrounded. 
            if (state == AppState.Foreground)
            {
                this.ShowAdOpen();
            }


        }
    }
}


