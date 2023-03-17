using com.adjust.sdk;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MaxSdkBase;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

namespace DVAH
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
        #region Lib Properties

        [SerializeField]
        string _paid_ad_revenue = "paid_ad_impression_value";
#if UNITY_EDITOR
        public string paid_ad_revenue
        {
            get
            {
                return _paid_ad_revenue;
            }

            set
            {
                _paid_ad_revenue = value;
            }
        }
#endif
        [SerializeField]
        bool _isBannerAutoShow = false;

        #region control AD is Allow
        bool _isBannerCurrentlyAllow = false, _isOffBanner = false, _isOffInter = false,
            _isOffReward = false, _isOffAdsOpen = false, _isOffAdsResume = false;
        public bool isAdBanner => _isBannerCurrentlyAllow;
        #endregion

        [SerializeField]
        MaxSdkBase.BannerPosition _bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
        public MaxSdkBase.BannerPosition BannerPosition => _bannerPosition;

        [SerializeField] GameObject _popUpNoAd;



        [Header("---ID---")]
        [Space(10)]
        [SerializeField]
        private string _MaxSdkKey = "3N4Mt8SNhOzkQnGb9oHsRRG1ItybcZDpJWN1fVAHLdRagxP-_k_ZXVaMAdMe5Otsmp6qJSXskfsrtakfRmPAGW";
        [SerializeField]
        private string _BannerAdUnitID = "df980c4d809fc01e",
            _InterstitialAdUnitID = "3a70c7be99dade7d",
            _RewardedAdUnitID = "6b7094c5d21fcfe5";

        [SerializeField] List<string> _OpenAdUnitIDs = new List<string>();


        [SerializeField]
        private List<string> _NativeAdID = new List<string>();
#if NATIVE_AD
        [Header("------NativeObject--------")]
        [SerializeField] List<AdNativeObject> _adNativePanel = new List<AdNativeObject>();
        [SerializeField] AdNativeObject adNativeObject;

        List<NativeAd> _nativeAd = new List<NativeAd>();

        List<AdLoader> _nativeADLoader = new List<AdLoader>();
#endif

#if UNITY_EDITOR
        public string MaxSdkKey
        {
            get
            {
                return _MaxSdkKey;
            }
            set
            {
                _MaxSdkKey = value;
            }
        }
        public string BannerAdUnitID
        {
            get
            {
                return _BannerAdUnitID;
            }
            set
            {
                _BannerAdUnitID = value;
            }
        }

        public string InterstitialAdUnitID
        {
            get
            {
                return _InterstitialAdUnitID;
            }
            set
            {
                _InterstitialAdUnitID = value;
            }
        }

        public string RewardedAdUnitID
        {
            get
            {
                return _RewardedAdUnitID;
            }
            set
            {
                _RewardedAdUnitID = value;
            }
        }

        public List<string> OpenAdUnitIDs
        {
            get
            {
                return _OpenAdUnitIDs;
            }
            set
            {
                _OpenAdUnitIDs = value;
            }
        }

        public List<string> NativeAdID
        {
            get
            {
                return _NativeAdID;
            }
            set
            {
                _NativeAdID = value;
            }
        }

#if NATIVE_AD
        public List<AdNativeObject> adNativePanel
        {
            get
            {
                return _adNativePanel;
            }

            set
            {
                _adNativePanel = value;
            }
        }
#endif

#endif

        private int bannerRetryAttempt,
            interstitialRetryAttempt,
            rewardedRetryAttempt,
            NativeAdRetryAttemp = 1;

        List<int> AdOpenRetryAttemp = new List<int>();

        #region CallBack
        private Action<InterVideoState> _callbackInter = null;
        private Action<RewardVideoState> _callbackReward = null;
        private Action<bool> _callbackOpenAD = null;
        private Action<int,bool> _callbackLoadNativeAd = null;

        private Action _bannerClickCallback = null;
        private Action _interClickCallback = null;
        private Action _rewardClickCallback = null;
        private Action _adOpenClickCallback = null;

        #endregion

        private bool isShowingAd = false, _isSDKMaxInitDone = false,
            _isSDKAdMobInitDone = false,
            _isBannerInitDone = false;

        bool[] _isnativeKeepReload;

        #endregion


        #region CUSTOM PROPERTIES
        #endregion


        #region Lib Method

        public void Init(Action _onInitDone = null)
        {
            Debug.Log("[Huynn3rdLib]==========> Ad start Init! <==========");

            InitMAX();
            InitAdMob();

            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

            _onInitDone?.Invoke();
        }


        #region ClickCallBack
        public AdManager AssignClickCallBackBanner(Action callback)
        {
            _bannerClickCallback = callback;
            return this;
        }

        public AdManager AssignClickCallBackInter(Action callback)
        {
            _interClickCallback = callback;
            return this;
        }

        public AdManager AssignClickCallBackReward(Action callback)
        {
            _rewardClickCallback = callback;
            return this;
        }

        public AdManager AssignClickCallBackAdOpne(Action callback)
        {
            _adOpenClickCallback = callback;
            return this;
        }
        #endregion

        void InitMAX()
        {

            AdOpenRetryAttemp = new List<int>(new int[_OpenAdUnitIDs.Count]);

            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                // AppLovin SDK is initialized, configure and start loading ads.
                Debug.Log("[Huynn3rdLib]==> MAX SDK Initialized <==");
                _isSDKMaxInitDone = true;
                InitAdOpen();
                if (!_isOffInter)
                    InitializeInterstitialAds();

                //_ = InitializeBannerAds();

                if (!_isOffReward)
                    InitializeRewardedAds();

            };
            MaxSdk.SetSdkKey(_MaxSdkKey);
            MaxSdk.InitializeSdk();
        }

        void InitAdMob()
        {
#if NATIVE_AD

            _nativeAd = new List<NativeAd>(new NativeAd[_NativeAdID.Count]);
            _isnativeKeepReload = new bool[_NativeAdID.Count];
            _nativeADLoader.Clear();
            for (int i = 0; i< _isnativeKeepReload.Length; i++)
            {
                _nativeADLoader.Add(null);
                _isnativeKeepReload[i] = true;
                if (_adNativePanel[i] == null)
                _adNativePanel[i] = Instantiate(adNativeObject, this.transform);
            }
            MobileAds.Initialize(initStatus =>
            {
                _isSDKAdMobInitDone = true;
                //_ = LoadNativeADs(0);

            });
#endif
        }

        public async Task ShowAdDebugger()
        {
            while (!_isSDKMaxInitDone)
            {
                Debug.LogWarning("[Huynn3rdLib]==>Waiting Max SDK init done!<==");
                await Task.Delay(500);
            }

            MaxSdk.ShowMediationDebugger();
        }

        #region Banner Ad Methods

        public async Task InitializeBannerAds()
        {
            while (!_isSDKMaxInitDone)
            {
                Debug.LogWarning("[Huynn3rdLib]==>Waiting Max SDK init done!<==");
                await Task.Delay(500);
            }

            UnityMainThread.wkr.AddJob(() =>
            {
                if (_isOffBanner)
                    return;
                if (string.IsNullOrWhiteSpace(_BannerAdUnitID))
                    return;
                Debug.Log("[Huynn3rdLib]==> Init banner <==");
                FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adState: AD_STATE.load);
                // Attach Callbacks
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
                {
                    Debug.Log("[Huynn3rdLib]==> Banner ad revenue paid <==");
                    TrackAdRevenue(adInfo);
                };

                // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
                // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
                MaxSdk.CreateBanner(_BannerAdUnitID, _bannerPosition);
                MaxSdk.SetBannerExtraParameter(_BannerAdUnitID, "adaptive_banner", "false");
                // Set background or background color for banners to be fully functional.
                MaxSdk.SetBannerBackgroundColor(_BannerAdUnitID, new Color(1, 1, 1, 0));

                if (_isBannerAutoShow) _ = ShowBanner();

                _isBannerInitDone = true;
            });
        }

        void ManuallyLoadBanner()
        {
            MaxSdk.StopBannerAutoRefresh(_BannerAdUnitID);
            MaxSdk.LoadBanner(_BannerAdUnitID);
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (_isBannerCurrentlyAllow)
                _ = this.ShowBanner();
            Debug.Log("[Huynn3rdLib]==> Banner ad loaded " + adUnitId + " <==");
            MaxSdk.StartBannerAutoRefresh(_BannerAdUnitID);

            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adState: AD_STATE.load_done, adNetwork: adInfo.NetworkName);

        }

        private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Banner ad failed to load. MAX will automatically try loading a new ad internally.
            Debug.LogError("[Huynn3rdLib]==>Banner ad failed to load with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adState: AD_STATE.load_fail);
            bannerRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));

            Invoke("ManuallyLoadBanner", (float)retryDelay);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Banner ad clicked <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.banner, adNetwork: adInfo.NetworkName);
            try
            {
                _bannerClickCallback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> invoke banner click callback error: " + e.ToString() + " <==");
            }
            _bannerClickCallback = null;
        }


        #endregion

        #region Interstitial Ad Methods
        private void InitializeInterstitialAds()
        {
            // Attach callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += Interstitial_OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += Interstitial_OnAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
            {
                Debug.Log("[Huynn3rdLib]==> Interstitial revenue paid <==");
                TrackAdRevenue(adInfo);
            };

            // Load the first interstitial
            LoadInterstitial();
        }



        void LoadInterstitial()
        {
            Debug.Log("[Huynn3rdLib]==>Start load Interstitial " + _InterstitialAdUnitID + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.load);
            MaxSdk.LoadInterstitial(_InterstitialAdUnitID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            Debug.Log("[Huynn3rdLib]==> Interstitial loaded <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.load_done, adNetwork: adInfo.NetworkName);
            // Reset retry attempt
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

            Debug.LogError("[Huynn3rdLib]==> Interstitial failed to load with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.load_fail);

            Invoke("LoadInterstitial", (float)retryDelay);
        }

        private void Interstitial_OnAdDisplayedEvent(string arg1, AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Interstitial show! <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.show, adNetwork: adInfo.NetworkName);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Debug.LogError("[Huynn3rdLib]==> Interstitial failed to display with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.show_fail, adNetwork: adInfo.NetworkName);
            LoadInterstitial();

            try
            {
                _callbackInter?.Invoke(InterVideoState.None);
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }

            _callbackInter = null;
        }


        private void Interstitial_OnAdClickedEvent(string arg1, AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Inter ad clicked <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.inter, adNetwork: adInfo.NetworkName);
            try
            {
                _interClickCallback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke click inter callback, error: " + e.ToString() + " <==");
            }
            _interClickCallback = null;
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Interstitial dismissed <==");
            try
            {
                _callbackInter?.Invoke(InterVideoState.Closed);
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke callback inter, error: " + e.ToString() + " <==");
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
                Debug.Log("[Huynn3rdLib]==> Reward paid event! <==");
                TrackAdRevenue(adInfo);
            };


            // Load the first RewardedAd
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            Debug.Log("[Huynn3rdLib]==> Load reward Ad " + _RewardedAdUnitID + " ! <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.load);
            MaxSdk.LoadRewardedAd(_RewardedAdUnitID);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'

            // Reset retry attempt
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.load_done, adNetwork: adInfo.NetworkName);
            rewardedRetryAttempt = 0;
        }

        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).

            rewardedRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));

            Debug.LogError("[Huynn3rdLib]==> Rewarded ad failed to load with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.load_fail);
            Invoke("LoadRewardedAd", (float)retryDelay);

        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. We recommend loading the next ad

            Debug.LogError("[Huynn3rdLib]==> Rewarded ad failed to display with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.show_fail, adInfo.NetworkName);
            LoadRewardedAd();
            try
            {
                _callbackReward?.Invoke(RewardVideoState.None);

            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Reward display success! <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.show, adInfo.NetworkName);
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Reward clicked! <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.reward, adNetwork: adInfo.NetworkName);
            try
            {
                _rewardClickCallback?.Invoke();

            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke reward click callback, error: " + e.ToString() + " <==");
            }

            _rewardClickCallback = null;
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoadRewardedAd();
            isShowingAd = false;
            _callbackReward = null;
            Debug.Log("[Huynn3rdLib]==> Reward closed! <==");
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {

            try
            {
                _callbackReward?.Invoke(RewardVideoState.Watched);

            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
            Debug.Log("[Huynn3rdLib]==> Reward recived!! <==");
        }
        #endregion


        #region AdOpen Methods
        void InitAdOpen()
        {
            Debug.Log("[Huynn3rdLib]==> Ad open/resume init! <==");


            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += AppOpen_OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += AppOpenOnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += AppOpen_OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += AppOpen_OnAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
            {
                Debug.Log("[Huynn3rdLib]==> Ad open/resume paid event! <==");
                TrackAdRevenue(adInfo);
            };

            if(_OpenAdUnitIDs.Count > 0)
                LoadAdOpen(0);
        }

        IEnumerator waitLoadAdOpen(float time, int ID)
        {
            yield return new WaitForSeconds(time);
            LoadAdOpen(ID);
        }

        public void LoadAdOpen(int ID = 0)
        {
            Debug.Log("[Huynn3rdLib]==> Start load ad open/resume! ID:" + _OpenAdUnitIDs[ID] + " <==");

            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.load);

            if (!MaxSdk.IsAppOpenAdReady(_OpenAdUnitIDs[ID]))
            {
                MaxSdk.LoadAppOpenAd(_OpenAdUnitIDs[ID]);
            }
        }


        private void AppOpen_OnAdLoadedEvent(string arg1, AdInfo arg2)
        {
            Debug.Log("[Huynn3rdLib]==>Load ad open/resume success! <==");

            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.load_done, adNetwork: arg2.NetworkName);

            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID] = 0;
        }

        private void AppOpenOnAdLoadFailedEvent(string arg1, ErrorInfo errorInfo)
        {
            Debug.LogError("[Huynn3rdLib]==> Load ad open/resume failed, code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.load_fail);
            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));
            isShowingAd = false;

            waitLoadAdOpen((float)retryDelay, ID);
        }

        private void AppOpen_OnAdDisplayedEvent(string arg1, AdInfo arg2)
        {
            Debug.Log("[Huynn3rdLib]==> Show ad open/resume success! <==");
            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.show, adNetwork: arg2.NetworkName);
            isShowingAd = true;

        }

        private void AppOpen_OnAdClickedEvent(string arg1, AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==>Click open/resume success! <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.open, adNetwork: adInfo.NetworkName);
            try
            {
                _adOpenClickCallback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==>Callback click ad open error: " + e.ToString() + "<==");
            }

            _adOpenClickCallback = null;
        }

        private void AppOpen_OnAdDisplayFailedEvent(string arg1, ErrorInfo errorInfo, AdInfo arg3)
        {
            Debug.LogError("[Huynn3rdLib]==> Show ad open/resume failed, code: " + errorInfo.Code + " <==");

            try
            {
                _callbackOpenAD?.Invoke(false);
                _callbackOpenAD = null;
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==>Callback ad open error: " + e.ToString() + "<==");
            }

            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.show_fail);

            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));
            isShowingAd = false;

            waitLoadAdOpen((float)retryDelay, ID);

        }


        public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("[Huynn3rdLib]==> Ad open/resume close! <==");
            try
            {
                _callbackOpenAD?.Invoke(true);
                _callbackOpenAD = null;
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==>Callback ad open error: " + e.ToString() + "<==");
            }
            isShowingAd = false;
            int ID = _OpenAdUnitIDs.IndexOf(adUnitId);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));
            isShowingAd = false;

            LoadAdOpen(ID);
        }


        #endregion



#if NATIVE_AD

        #region Native Ad Methods

        public async Task LoadNativeADs(Action<int,bool> callback, params int[] indexes)
        {
            while (!_isSDKAdMobInitDone)
            {
                await Task.Delay(50);
            }

            _callbackLoadNativeAd = callback;
           
            foreach (int index in indexes)
            {
                AdLoader adLoader = RequestNativeAd(_NativeAdID[index]);
                _nativeADLoader[index] = (adLoader);
#if UNITY_EDITOR
                this.HandleNativeAdLoaded(adLoader, new NativeAdEventArgs());
#endif
            }

        }

        public async Task LoadNativeADs( params int[] indexes)
        {
            await LoadNativeADs((id, success) => { }, indexes);
        }

        public AdManager SetAdNativeKeepReload(int ID,bool isKeepReload)
        {
            if (ID >= _isnativeKeepReload.Length)
                return this;
            this._isnativeKeepReload[ID] = isKeepReload;
            return this;
        }

        public AdLoader RequestNativeAd(string AdID)
        {

            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.native, adState: AD_STATE.load);

            AdLoader adLoader = new AdLoader.Builder(AdID)
                .ForNativeAd()
                .Build();

            adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
            adLoader.OnNativeAdImpression += this.HandleNativeAdImpression;
            adLoader.OnNativeAdClicked += this.AdLoader_OnNativeAdClicked;

            adLoader.LoadAd(new AdRequest.Builder().Build());

            Debug.Log("[Huynn3rdLib]===>Start load Native " + AdID + " <====");

            return adLoader;
        }


        public bool CreateNativeAd(int adNativeID)
        {
            Debug.Log("[Huynn3rdLib]===>set object native "+adNativeID+" <===");


#if UNITY_EDITOR

            _adNativePanel[adNativeID].body.text = "<color=blue>" + this.NativeAdID[adNativeID] + "</color>\n";
            for (int i = 0; i < 3; i++)
            {
                RawImage bg;
                if (i >= _adNativePanel[adNativeID].adBG.transform.parent.childCount)
                {

                    bg = Instantiate(_adNativePanel[adNativeID].adBG, _adNativePanel[adNativeID].adBG.transform.position,
                       Quaternion.identity, _adNativePanel[adNativeID].adBG.transform.parent).GetComponentInChildren<RawImage>();
                }
                else
                {
                    bg = _adNativePanel[adNativeID].adBG.transform.parent.GetChild(i).GetComponentInChildren<RawImage>();
                }

                float aspect = 1;
                bg.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                bg.GetComponent<AspectRatioFitter>().aspectRatio = aspect;
                bg.transform.parent.gameObject.SetActive(true);
            }
            return true;
#endif

            List<Texture2D> imagetexture = this._nativeAd[adNativeID].GetImageTextures();
            if (imagetexture.Any())
            {
                List<GameObject> Bgs = new List<GameObject>();
                int i = 0;
                foreach (Texture2D texture2D in imagetexture)
                {
                    RawImage bg;
                    if (i >= _adNativePanel[adNativeID].adBG.transform.parent.childCount)
                    {
                        bg = Instantiate(_adNativePanel[adNativeID].adBG, _adNativePanel[adNativeID].adBG.transform.position,
                       Quaternion.identity, _adNativePanel[adNativeID].adBG.transform.parent).GetComponentInChildren<RawImage>();
                    }
                    else
                    {
                        bg = _adNativePanel[adNativeID].adBG.transform.parent.GetChild(i).GetComponentInChildren<RawImage>();
                    }

                    float aspect = texture2D.width / texture2D.height;
                    bg.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                    bg.GetComponent<AspectRatioFitter>().aspectRatio = aspect;

                    bg.texture = texture2D;
                    bg.transform.parent.gameObject.SetActive(true);
                    Bgs.Add(bg.gameObject);
                }

                this._nativeAd[adNativeID].RegisterImageGameObjects(Bgs);
            }


            Texture2D iconTexture = this._nativeAd[adNativeID].GetIconTexture();
            if (iconTexture)
            {
                _adNativePanel[adNativeID].adIcon.texture = iconTexture;

                if (!this._nativeAd[adNativeID].RegisterIconImageGameObject(_adNativePanel[adNativeID].adIcon.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register adIcon error <====");
                    return false;
                }

            }
            else
            {
                _adNativePanel[adNativeID].adIcon.gameObject.SetActive(false);
            }

            string headLineText = this._nativeAd[adNativeID].GetHeadlineText();
            if (!string.IsNullOrEmpty(headLineText))
            {
                _adNativePanel[adNativeID].headLine.text = headLineText;
                if (!this._nativeAd[adNativeID].RegisterHeadlineTextGameObject(_adNativePanel[adNativeID].headLine.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register adHeadline error <====");
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "adHeadline"
                        }
                    });
                    return false;
                }
            }
            else
            {
                _adNativePanel[adNativeID].headLine.gameObject.SetActive(false);
            }


            Texture2D iconChoice = this._nativeAd[adNativeID].GetAdChoicesLogoTexture();
            if (iconChoice != null)
            {
                _adNativePanel[adNativeID].adChoice.texture = iconChoice;
                if (!this._nativeAd[adNativeID].RegisterAdChoicesLogoGameObject(_adNativePanel[adNativeID].adChoice.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register adChoiceIcon error <====");
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "adChoiceIcon"
                        }
                    });
                    return false;
                }
            }
            else
            {
                _adNativePanel[adNativeID].adChoice.gameObject.SetActive(false);
            }

            string CTAText = this._nativeAd[adNativeID].GetCallToActionText();
            if (!string.IsNullOrEmpty(CTAText))
            {
                _adNativePanel[adNativeID].callToAction.text = CTAText;
                if (!this._nativeAd[adNativeID].RegisterCallToActionGameObject(_adNativePanel[adNativeID].callToAction.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register CTA error <====");
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "CTA"
                        }
                    });
                    return false;
                }
            }

            string advertiseText = this._nativeAd[adNativeID].GetAdvertiserText();
            if (!string.IsNullOrEmpty(advertiseText))
            {
                _adNativePanel[adNativeID].advertiser.text = advertiseText;
                if (!this._nativeAd[adNativeID].RegisterAdvertiserTextGameObject(_adNativePanel[adNativeID].advertiser.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register advertise text error!<====");
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "advertise"
                        }
                    });
                    return false;
                }

            }
            else
            {
                _adNativePanel[adNativeID].advertiser.gameObject.SetActive(false);
            }

            string bodyText = this._nativeAd[adNativeID].GetBodyText();
            if (!string.IsNullOrEmpty(bodyText))
            {
                _adNativePanel[adNativeID].body.text = bodyText;
                if (!this._nativeAd[adNativeID].RegisterBodyTextGameObject(_adNativePanel[adNativeID].body.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register body text error!<====");
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "body"
                        }
                    });
                    return false;
                }
            }
            else
            {
                _adNativePanel[adNativeID].body.gameObject.SetActive(false);
            }

            string priceText = this._nativeAd[adNativeID].GetPrice();
            if (!string.IsNullOrEmpty(priceText))
            {
                _adNativePanel[adNativeID].price.text = priceText;
                if (!this._nativeAd[adNativeID].RegisterPriceGameObject(_adNativePanel[adNativeID].price.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register price text error!<====");
                    _adNativePanel[adNativeID].price.gameObject.SetActive(false);
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "price"
                        }
                    });

                    return false;
                }
            }
            else
            {
                _adNativePanel[adNativeID].price.gameObject.SetActive(false);
            }

            string storeText = this._nativeAd[adNativeID].GetStore();
            if (!string.IsNullOrEmpty(storeText))
            {
                _adNativePanel[adNativeID].store.text = storeText;
                if (!this._nativeAd[adNativeID].RegisterStoreGameObject(_adNativePanel[adNativeID].store.gameObject))
                {
                    Debug.LogError("[Huynn3rdLib]===> Native Ad register store text error!<====");
                    _adNativePanel[adNativeID].store.gameObject.SetActive(false);
                    _ = FireBaseManager.Instant.LogEventWithParameter("Native_show_fail", new Hashtable()
                    {
                        {
                            "position", "store"
                        }
                    });

                    return false;
                }
            }
            else
            {
                _adNativePanel[adNativeID].store.gameObject.SetActive(false);
            }


            return true;
        }


        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            Debug.LogError("[Huynn3rdLib]===> NativeAd load Fail! error: " + e.LoadAdError.GetMessage());
            FireBaseManager.Instant.LogADEvent(AD_TYPE.native, AD_STATE.load_fail);

            int ID = _NativeAdID.IndexOf(((AdLoader)sender).AdUnitId);
            if (ID < 0)
            {
                Debug.LogErrorFormat("[Huynn3rdLib]===> HandleAdFailedToLoad cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
                return;
            }

            _callbackLoadNativeAd?.Invoke(ID,false);

            NativeAdRetryAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, NativeAdRetryAttemp));

            StartCoroutine(waitReloadAd((float)retryDelay, () =>
            {
                if (_isnativeKeepReload[ID])
                    RequestNativeAd(_NativeAdID[ID]);
                else
                    Debug.Log("[Huynn3rdLib]===> Native stop reload.");
            }));
        }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            Debug.Log("[Huynn3rdLib]===> Native ad loaded.");


            int ID = _nativeADLoader.IndexOf((AdLoader)sender);
            if (ID < 0)
            {
                Debug.LogErrorFormat("[Huynn3rdLib]===> HandleAdLoaded cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
                return;
            }

            if (!_nativeADLoader[ID].AdUnitId.Equals(_NativeAdID[ID]))
            {
                Debug.LogErrorFormat("[Huynn3rdLib]===> adloaderID {0} doesnt == senderID {1}", _nativeADLoader[ID].AdUnitId,((AdLoader)sender).AdUnitId);
                return;
            }

            this._nativeAd[ID] = e.nativeAd;

            if (this.CreateNativeAd(ID))
            {
                FireBaseManager.Instant.LogADEvent(AD_TYPE.native, AD_STATE.show);
            }
            else
            {
                RequestNativeAd(_NativeAdID[ID]);
                FireBaseManager.Instant.LogADEvent(AD_TYPE.native, AD_STATE.show_fail);
            }

            _callbackLoadNativeAd?.Invoke(ID,true);
            FireBaseManager.Instant.LogADEvent(AD_TYPE.native, AD_STATE.load_done);

        }

        private void HandleNativeAdImpression(object sender, EventArgs e)
        {
            Debug.Log("[Huynn3rdLib]===> Handle ad native impression! ");
        }


        private void AdLoader_OnNativeAdClicked(object sender, EventArgs e)
        {
            Debug.Log("[Huynn3rdLib]===> Handle ad native clicked! ");

        }


        #endregion

#endif
        #region CheckAdLoaded

        public bool InterstitialIsLoaded()
        {
            return MaxSdk.IsInterstitialReady(_InterstitialAdUnitID);
        }

        public bool VideoRewardIsLoaded()
        {
            return MaxSdk.IsRewardedAdReady(_RewardedAdUnitID);
        }

        public bool AdsOpenIsLoaded(int ID = 0)
        {
            return MaxSdk.IsAppOpenAdReady(_OpenAdUnitIDs[ID]);
        }

        public bool NativeAdLoaded(int ID)
        {

#if UNITY_EDITOR
            return true;
#elif NATIVE_AD
            return this._nativeAd[ID] != null;
#else
            return false;
#endif

        }

        #endregion

        #region ShowAd

        /// <summary>
        /// Show AD Banner, It doesn't matter SDK init done or not
        /// <code>
        /// _= AdManager.Instant.ShowBanner();
        /// </code>
        /// </summary>
        public async Task ShowBanner()
        {
            if (_isOffBanner)
                return;

            while (!_isBannerInitDone)
            {
                await Task.Delay(500);
            }

            Debug.Log("[Huynn3rdLib]==> show banner <==");
            _isBannerCurrentlyAllow = true;

            if (!string.IsNullOrWhiteSpace(_BannerAdUnitID))
                MaxSdk.ShowBanner(_BannerAdUnitID);

        }

        /// <summary>
        /// Hide AD Banner, It doesn't matter SDK init done or not
        /// <code>
        /// AdManager.Instant.DestroyBanner();
        /// </code>
        /// </summary>
        public void DestroyBanner()
        {
            Debug.Log("[Huynn3rdLib]==> destroy banner <==");
            _isBannerCurrentlyAllow = false;

            if (!string.IsNullOrWhiteSpace(_BannerAdUnitID))
                MaxSdk.HideBanner(_BannerAdUnitID);
        }


        /// <summary>
        /// Show AD inter, if user watch ad to get reward but ad not load done yet then you must show the popup "AD not avaiable", then set showNoAds = true
        /// <code>
        ///AdManager.Instant.ShowInterstitial((interState, true)=>{
        ///});
        /// </code>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="showNoAds"></param>
        public void ShowInterstitial(Action<InterVideoState> callback = null, bool showNoAds = false)
        {
            if (_isOffInter)
                return;
            if (InterstitialIsLoaded())
            {
                isShowingAd = true;
                _callbackInter = callback;
                MaxSdk.ShowInterstitial(_InterstitialAdUnitID);
                return;
            }

            try
            {
                callback?.Invoke(InterVideoState.None);
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }
            if (_popUpNoAd && showNoAds) _popUpNoAd.SetActive(true);
        }


        /// <summary>
        /// Show AD reward, if user watch ad to get reward but ad not load done yet then you must show the popup "AD not avaiable", then set showNoAds = true
        /// <code>
        ///AdManager.Instant.ShowRewardVideo((interState, true)=>{
        ///});
        /// </code>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="showNoAds"></param>
        public void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false)
        {
            if (_isOffReward)
                return;

            if (VideoRewardIsLoaded())
            {
                isShowingAd = true;
                _callbackReward = callback;
                MaxSdk.ShowRewardedAd(_RewardedAdUnitID);
            }
            else
            {
                try
                {
                    callback?.Invoke(RewardVideoState.None);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Huynn3rdLib]==> Faild invoke callback reward, error: " + e.ToString() + " <==");
                }
                if (_popUpNoAd && showNoAds) _popUpNoAd.SetActive(true);
            }
        }

        /// <summary>
        /// Show ad open or resume, if you need callback must check is ad show done or not
        /// <code>
        /// AdManager.Instant.ShowAdOpen(true,(isSuccess)=>{
        ///       if(isSuccess){
        ///         Debug.Log("Done!");
        ///       }else{
        ///         Debug.Log("Fail!");
        ///       }
        /// })
        /// </code>
        /// </summary>
        /// <param name="isAdOpen">Is Ads treated as an open AD</param>
        /// <param name="callback">Callback when adopen show done or fail pass true if ad show success and false if ad fail</param>
        public void ShowAdOpen(int ID = 0, bool isAdOpen = false, Action<bool> callback = null)
        {
            if (isAdOpen && _isOffAdsOpen)
                return;

            if (!isAdOpen && _isOffAdsResume)
                return;

            if (isShowingAd)
            {
                FireBaseManager.Instant.adTypeShow = AD_TYPE.resume;
                return;
            }

            if (CheckInternetConnection() && AdsOpenIsLoaded(ID))
            {
                FireBaseManager.Instant.adTypeShow = isAdOpen ? AD_TYPE.open : AD_TYPE.resume;
                MaxSdk.ShowAppOpenAd(_OpenAdUnitIDs[ID]);
                _callbackOpenAD = callback;
            }
            else
            {
                FireBaseManager.Instant.adTypeShow = AD_TYPE.resume;
                try
                {
                    callback?.Invoke(false);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Huynn3rdLib]==> Faild invoke callback adopen/resume, error: " + e.ToString() + " <==");
                }
            }
        }

        /// <summary>
        /// Show ad open or resume, if you need callback must check is ad show done or not
        /// <code>
        /// AdManager.Instant.ShowAdOpen((isSuccess)=>{
        ///       if(isSuccess){
        ///         Debug.Log("Done!");
        ///       }else{
        ///         Debug.Log("Fail!");
        ///       }
        /// })
        /// </code>
        /// </summary> 
        /// <param name="callback">Callback when adopen show done or fail pass true if ad show success and false if ad fail</param>
        public void ShowAdOpen(Action<bool> callback = null)
        {
            int ID = _OpenAdUnitIDs.Count - 1;
            if (ID < 0)
                return;
            ShowAdOpen(ID, false, callback);
        }

#if NATIVE_AD
        /// <summary>
        /// ID is index of ID native AD
        /// Actually, Native AD object already create right when native AD success!
        /// Show function using for assign Native AD object into right canvas
        /// <code>
        /// _= AdManager.Instant.ShowNative(0,(nativePanel)=>{
        ///       nativePanel.transform.SetParent(canvas.transform);
        ///       nativePanel.transform.localScale = Vector3.one;
        ///       nativePanel.transform.localPosition = Vector3.zero;
        ///       nativePanel.rectTransform.sizeDelta = Vector2.zero;
        ///       nativePanel.rectTransform.anchorMax = new Vector2(1, 0.4f);
        /// 
        /// })
        /// </code>
        /// </summary> 
        /// <param name="callback">Callback using for assign Native AD object into right canvas</param>
        public async Task ShowNative(int ID, Action<AdNativeObject> callBack = null)
        {
            if (ID >= _adNativePanel.Count || ID >= _nativeAd.Count)
                return;
            if (_adNativePanel[ID] == null)
                return;

#if UNITY_EDITOR
            await Task.Delay(50);
#else
            while (_nativeAd[ID] == null)
            {
                await Task.Delay(500);
            }
#endif
            _adNativePanel[ID].gameObject.SetActive(true);
            try
            {
                callBack?.Invoke(_adNativePanel[ID]);
            }
            catch (Exception e)
            {
                Debug.LogError("[Huynn3rdLib]===>Error on callback show native! error: " + e.ToString() + "<====");
            }
            _adNativePanel[ID].FitCollider();

        }

#endif

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

            FireBaseManager.Instant.LogAdValueAdjust(adInfo.Revenue);
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
            Firebase.Analytics.FirebaseAnalytics.LogEvent(_paid_ad_revenue, impressionParameters);
        }

        #endregion

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

        IEnumerator waitReloadAd(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);

            callback?.Invoke();
        }

#if UNITY_EDITOR

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                if (_OpenAdUnitIDs.Count == 0)
                    return;
                this.ShowAdOpen(_OpenAdUnitIDs.Count - 1);
            }
        }
#endif


        private void OnAppStateChanged(AppState state)
        {
            // Display the app open ad when the app is foregrounded. 
            if (state == AppState.Foreground)
            {
                if (_OpenAdUnitIDs.Count == 0)
                    return;
                this.ShowAdOpen(_OpenAdUnitIDs.Count - 1);
            }
        }

        #endregion

        #region CUSTOM LIB
        #endregion
    }
}


