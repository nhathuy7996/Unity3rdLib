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
using UnityEditor;
using System.IO;

namespace DVAH
{ 
    public class AdMHighFather_AppLovin : AdMHighFather
    {
          
        [SerializeField]
        MaxSdkBase.BannerPosition _bannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
        public MaxSdkBase.BannerPosition BannerPosition => _bannerPosition;

        private int bannerRetryAttempt,
            interstitialRetryAttempt,
            rewardedRetryAttempt,
            NativeAdRetryAttemp = 1;

        List<int> AdOpenRetryAttemp = new List<int>(); 

        private bool _isSDKMaxInitDone = false,
            _isSDKAdMobInitDone = false,
            _isBannerInitDone = false;

        bool[] _isnativeKeepReload;

        bool _isClickedBanner = false;
          

        #region CUSTOM PROPERTIES
        #endregion


        #region Lib Method

        public override void Init(Action _onInitDone = null)
        {
            Debug.Log(CONSTANT.Prefix + $"==========><color=#00FF00>Ad start Init!</color><==========");

            DVAH_Data = Resources.Load<DVAH_Data>("DVAH_Data");
            if (!DVAH_Data)
            {
               Debug.LogError(CONSTANT.Prefix + "===>Can not find DVAH data file!<====");
            }

            if (DVAH_Data.CHEAT_BUILD)
            {
                this.setOffAdPosition(true,AD_TYPE.banner, AD_TYPE.inter,AD_TYPE.reward, AD_TYPE.open, AD_TYPE.resume, AD_TYPE.native);
            }

            InitMAX();
            InitAdMob();

            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

            _onInitDone?.Invoke();
        }


        #region ClickCallBack
        public override AdMHighFather AssignClickCallBack(Action callback, AD_TYPE adType)
        {
            _clickADCallback[(int)adType] = callback;
            return this;
        }

        public override AdMHighFather setOffAdPosition(bool isOff, params AD_TYPE[] aD_TYPE)
        {
            foreach (AD_TYPE aD in aD_TYPE)
            {
                _offAdPosition[(int)aD] = isOff;
            }

            return this;
        }

        public override bool[] getOffAdPosition()
        {
            return this._offAdPosition;
        }
        #endregion

        void InitMAX()
        {

            AdOpenRetryAttemp = new List<int>(new int[_OpenAdUnitIDs.Count]);

            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                // AppLovin SDK is initialized, configure and start loading ads.
                Debug.Log(CONSTANT.Prefix + $"==> MAX SDK Initialized <==");
                _isSDKMaxInitDone = true;
                InitAdOpen();
                if (!_offAdPosition[(int)AD_TYPE.inter])
                    InitializeInterstitialAds();

                if (!_offAdPosition[(int)AD_TYPE.banner] && !_initBannerManually)
                    InitializeBannerAdsAsync();

                if (!_offAdPosition[(int)AD_TYPE.reward])
                    InitializeRewardedAds();

            };
            MaxSdk.SetSdkKey(_MaxSdkKey);
            MaxSdk.InitializeSdk();
        }

        void InitAdMob()
        {
#if NATIVE_AD

            if (!adNativeObject)
                adNativeObject = Resources.Load<AdNativeObject>("item_ad");

            _nativeAd = new List<NativeAd>(new NativeAd[_NativeAdID.Count]);
            _isnativeKeepReload = new bool[_NativeAdID.Count];
            _nativeADLoader.Clear();

            for (int i = 0; i < _isnativeKeepReload.Length; i++)
            {
                _nativeADLoader.Add(null);
                _isnativeKeepReload[i] = true;
                if (i >= _adNativePanel.Count || _adNativePanel[i] == null)
                {
                    AdNativeObject g = Instantiate(adNativeObject, this.transform);
                    if (i >= _adNativePanel.Count)
                        _adNativePanel.Add(g);
                    else
                        _adNativePanel[i] = g;
                }
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
            float timer = 0;
            while (!_isSDKMaxInitDone && timer < 240000)
            {
                Debug.LogWarning(CONSTANT.Prefix + $"==>Waiting Max SDK init done!<==");
                await Task.Delay(500);
                timer += 500;
            }

            MaxSdk.ShowMediationDebugger();
        }

        #region Banner Ad Methods

        public override void InitializeBannerAdsAsync()
        {
            _ = InitializeBannerAds();
        }

        public async Task InitializeBannerAds()
        {
            float timer = 0;
            while (!_isSDKMaxInitDone && timer < 240000)
            {
                Debug.LogWarning(CONSTANT.Prefix + $"==>Waiting Max SDK init done!<==");
                await Task.Delay(500);
                timer += 500;
            }

            UnityMainThread.wkr.AddJob(() =>
            {
                if (_offAdPosition[(int)AD_TYPE.banner])
                    return;
                if (string.IsNullOrWhiteSpace(_BannerAdUnitID))
                    return;
                Debug.Log(CONSTANT.Prefix + $"==> Init banner <==");
                FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adState: AD_STATE.load);
                // Attach Callbacks
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
                {
                    Debug.Log(CONSTANT.Prefix + $"==> Banner ad revenue paid <==");
                    TrackAdRevenue(adInfo);
                };

                // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
                // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
                MaxSdk.CreateBanner(_BannerAdUnitID, _bannerPosition);
                MaxSdk.SetBannerExtraParameter(_BannerAdUnitID, "adaptive_banner", "false");
                // Set background or background color for banners to be fully functional.
                MaxSdk.SetBannerBackgroundColor(_BannerAdUnitID, new Color(1, 1, 1, 0));

                if (_isBannerAutoShow) ShowBanner();

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
                 this.ShowBanner();
            Debug.Log(CONSTANT.Prefix + $"==> Banner ad loaded " + adUnitId + " <==");
            MaxSdk.StartBannerAutoRefresh(_BannerAdUnitID);

            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adState: AD_STATE.load_done, adNetwork: adInfo.NetworkName);

        }

        private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Banner ad failed to load. MAX will automatically try loading a new ad internally.
            Debug.LogError(CONSTANT.Prefix + $"==>Banner ad failed to load with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adState: AD_STATE.load_fail);
            bannerRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));

            Invoke("ManuallyLoadBanner", (float)retryDelay);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Banner ad clicked <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.banner, adNetwork: adInfo.NetworkName);
            try
            {
                _clickADCallback[(int)AD_TYPE.banner]?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> invoke banner click callback error: " + e.ToString() + " <==");
            }
            _isClickedBanner = true;
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
                Debug.Log(CONSTANT.Prefix + $"==> Interstitial revenue paid <==");
                TrackAdRevenue(adInfo);
            };

            // Load the first interstitial
            LoadInterstitial();
        }



        void LoadInterstitial()
        {
            Debug.Log(CONSTANT.Prefix + $"==>Start load Interstitial " + _InterstitialAdUnitID + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.load);
            MaxSdk.LoadInterstitial(_InterstitialAdUnitID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            Debug.Log(CONSTANT.Prefix + $"==> Interstitial loaded <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.load_done, adNetwork: adInfo.NetworkName);
            // Reset retry attempt
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

            Debug.LogError(CONSTANT.Prefix + $"==> Interstitial failed to load with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.load_fail);

            Invoke("LoadInterstitial", (float)retryDelay);
        }

        private void Interstitial_OnAdDisplayedEvent(string arg1, AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Interstitial show! <==");
            _callbackInter?.Invoke(InterVideoState.Open);
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.show, adNetwork: adInfo.NetworkName);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Debug.LogError(CONSTANT.Prefix + $"==> Interstitial failed to display with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adState: AD_STATE.show_fail, adNetwork: adInfo.NetworkName);
            LoadInterstitial();

            try
            {
                _callbackInter?.Invoke(InterVideoState.None);
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }

            _callbackInter = null;
            isShowingAD = false;
        }


        private void Interstitial_OnAdClickedEvent(string arg1, AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Inter ad clicked <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.inter, adNetwork: adInfo.NetworkName);
            try
            {
                _callbackInter?.Invoke(InterVideoState.Click);
                _clickADCallback[(int)AD_TYPE.inter]?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke click inter callback, error: " + e.ToString() + " <==");
            }
             
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Interstitial dismissed <==");
            try
            {
                _callbackInter?.Invoke(InterVideoState.Closed);
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }
            _callbackInter = null;
            LoadInterstitial();
            isShowingAD = false;
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
                Debug.Log(CONSTANT.Prefix + $"==> Reward paid event! <==");
                TrackAdRevenue(adInfo);
            };


            // Load the first RewardedAd
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Load reward Ad " + _RewardedAdUnitID + " ! <==");
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

            Debug.LogError(CONSTANT.Prefix + $"==> Rewarded ad failed to load with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.load_fail);
            Invoke("LoadRewardedAd", (float)retryDelay);

        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. We recommend loading the next ad

            Debug.LogError(CONSTANT.Prefix + $"==> Rewarded ad failed to display with error code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.show_fail, adInfo.NetworkName);
            LoadRewardedAd();
            try
            {
                _callbackReward?.Invoke(RewardVideoState.None);

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
            isShowingAD = false;
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Reward display success! <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adState: AD_STATE.show, adInfo.NetworkName);
            try
            {
                _callbackReward?.Invoke(RewardVideoState.Open);

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback display reward, error: " + e.ToString() + " <==");
            }
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Reward clicked! <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.reward, adNetwork: adInfo.NetworkName);
            try
            {
                _clickADCallback[(int)AD_TYPE.reward]?.Invoke();
                _callbackReward?.Invoke(RewardVideoState.Click);
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke reward click callback, error: " + e.ToString() + " <==");
            }

             
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoadRewardedAd();
            isShowingAD = false;
          
            try
            {
                _callbackReward?.Invoke(RewardVideoState.Closed);

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
            Debug.Log(CONSTANT.Prefix + $"==> Reward closed! <==");
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {

            try
            {
                _callbackReward?.Invoke(RewardVideoState.Watched);

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }
             
            Debug.Log(CONSTANT.Prefix + $"==> Reward recived!! <==");
        }
        #endregion


        #region AdOpen Methods
        void InitAdOpen()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Ad open/resume init! <==");


            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += AppOpen_OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += AppOpenOnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += AppOpen_OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += AppOpen_OnAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (adUnit, adInfo) =>
            {
                Debug.Log(CONSTANT.Prefix + $"==> Ad open/resume paid event! <==");
                TrackAdRevenue(adInfo);
            };

            if (_OpenAdUnitIDs.Count > 0)
                LoadAdOpen(0);
        }

        IEnumerator waitLoadAdOpen(float time, int ID)
        {
            yield return new WaitForSeconds(time);
            LoadAdOpen(ID);
        }

        public void LoadAdOpen(int ID = 0)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Start load ad open/resume! ID:" + _OpenAdUnitIDs[ID] + " <==");

            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.load);

            if (!MaxSdk.IsAppOpenAdReady(_OpenAdUnitIDs[ID]))
            {
                MaxSdk.LoadAppOpenAd(_OpenAdUnitIDs[ID]);
            }
        }


        private void AppOpen_OnAdLoadedEvent(string arg1, AdInfo arg2)
        {
            Debug.Log(CONSTANT.Prefix + $"==>Load ad open/resume success! <==");

            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.load_done, adNetwork: arg2.NetworkName);

            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID] = 0;
        }

        private void AppOpenOnAdLoadFailedEvent(string arg1, ErrorInfo errorInfo)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> Load ad open/resume failed, code: " + errorInfo.Code + " <==");
            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.load_fail);
            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));
             

            waitLoadAdOpen((float)retryDelay, ID);
        }

        private void AppOpen_OnAdDisplayedEvent(string arg1, AdInfo arg2)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Show ad open/resume success! <==");
            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.show, adNetwork: arg2.NetworkName);
            
            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            try
            {
                _callbackOpenAD?.Invoke(ID, OpenAdState.Open); 
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==>Callback click ad open error: " + e.ToString() + "<==");
            }
        }

        private void AppOpen_OnAdClickedEvent(string arg1, AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==>Click open/resume success! <==");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.open, adNetwork: adInfo.NetworkName);
            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;
            try
            {
                _callbackOpenAD?.Invoke(ID, OpenAdState.Click);
                _clickADCallback[(int)AD_TYPE.open]?.Invoke();

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==>Callback click ad open error: " + e.ToString() + "<==");
            }

            
        }

        private void AppOpen_OnAdDisplayFailedEvent(string arg1, ErrorInfo errorInfo, AdInfo arg3)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> Show ad open/resume failed, code: " + errorInfo.Code + " <==");
             
            FireBaseManager.Instant.LogADResumeEvent(adState: AD_STATE.show_fail);

            int ID = _OpenAdUnitIDs.IndexOf(arg1);
            if (ID < 0)
                return;

            try
            {

                _callbackOpenAD?.Invoke(ID, OpenAdState.None);
                _callbackOpenAD = null;
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==>Callback ad open error: " + e.ToString() + "<==");
            }

            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));
            isShowingAD = false;

            waitLoadAdOpen((float)retryDelay, ID);

        }


        public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Ad open/resume close! <==");
           
            isShowingAD = false;
            int ID = _OpenAdUnitIDs.IndexOf(adUnitId);
            if (ID < 0)
                return;

            try
            {
                _callbackOpenAD?.Invoke(ID, OpenAdState.Closed);
                _callbackOpenAD = null;
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==>Callback ad open error: " + e.ToString() + "<==");
            }
            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));
            

            LoadAdOpen(ID);
        }


        #endregion



#if NATIVE_AD

        #region Native Ad Methods
        public override void LoadNativeADsAsync(params int[] IDs)
        {
            _ = LoadNativeADs(IDs);
        }

        public override void LoadNativeADsAsync(Action<int, bool> callback, params int[] indexes)
        {
            _ = LoadNativeADs(callback, indexes);
        }

        public async Task LoadNativeADs(Action<int, bool> callback, params int[] indexes)
        {
            float timer = 0;
            while (!_isSDKAdMobInitDone && timer < 240000)
            {
                await Task.Delay(50);
                timer += 50;
            }


            _callbackLoadNativeAd += callback;

            foreach (int index in indexes)
            {
                AdLoader adLoader = RequestNativeAd(_NativeAdID[index]);
                _nativeADLoader[index] = (adLoader);
#if UNITY_EDITOR
                this.HandleNativeAdLoaded(adLoader, new NativeAdEventArgs());
#endif
            }

        }

        public async Task LoadNativeADs(params int[] indexes)
        {
            await LoadNativeADs((id, success) => { }, indexes);
        }

        public override void SetAdNativeKeepReload(int ID, bool isKeepReload)
        {
            if (ID >= _isnativeKeepReload.Length)
                return;
            this._isnativeKeepReload[ID] = isKeepReload;
             
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

            Debug.Log(CONSTANT.Prefix + $"===>Start load Native " + AdID + " <====");

            return adLoader;
        }


        public bool CreateNativeAd(int adNativeID)
        {
            Debug.Log(CONSTANT.Prefix + $"===>set object native " + adNativeID + " <===");


#if UNITY_EDITOR

            _adNativePanel[adNativeID].body.text = "<color=blue>" + this.NativeAdID[adNativeID] + "</color>\n";
            _adNativePanel[adNativeID].setAdBG(new Texture2D[3].ToList());

            return true;
#endif

            List<Texture2D> imagetexture = this._nativeAd[adNativeID].GetImageTextures();
            if (imagetexture.Any())
            {
                List<GameObject> Bgs = _adNativePanel[adNativeID].setAdBG(imagetexture);

                this._nativeAd[adNativeID].RegisterImageGameObjects(Bgs);
            }


            Texture2D iconTexture = this._nativeAd[adNativeID].GetIconTexture();
            if (iconTexture)
            {
                _adNativePanel[adNativeID].adIcon.texture = iconTexture;

                if (!this._nativeAd[adNativeID].RegisterIconImageGameObject(_adNativePanel[adNativeID].adIcon.gameObject))
                {
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register adIcon error <====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register adHeadline error <====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register adChoiceIcon error <====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register CTA error <====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register advertise text error!<====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register body text error!<====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register price text error!<====");
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
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register store text error!<====");
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
            Debug.LogError(CONSTANT.Prefix + $"===> NativeAd load Fail! error: " + e.LoadAdError.GetMessage());
            FireBaseManager.Instant.LogADEvent(AD_TYPE.native, AD_STATE.load_fail);

            int ID = _NativeAdID.IndexOf(((AdLoader)sender).AdUnitId);
            if (ID < 0)
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> HandleAdFailedToLoad cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
                return;
            }

            _callbackLoadNativeAd?.Invoke(ID, false);

            NativeAdRetryAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, NativeAdRetryAttemp));

            StartCoroutine(waitReloadAd((float)retryDelay, () =>
            {
                if (_isnativeKeepReload[ID])
                    RequestNativeAd(_NativeAdID[ID]);
                else
                    Debug.Log(CONSTANT.Prefix + $"===> Native stop reload.");
            }));
        }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            Debug.Log(CONSTANT.Prefix + $"===> Native ad loaded.");


            int ID = _nativeADLoader.IndexOf((AdLoader)sender);
            if (ID < 0)
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> HandleAdLoaded cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
                return;
            }

            if (!_nativeADLoader[ID].AdUnitId.Equals(_NativeAdID[ID]))
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> adloaderID {0} doesnt == senderID {1}", _nativeADLoader[ID].AdUnitId, ((AdLoader)sender).AdUnitId);
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

            _callbackLoadNativeAd?.Invoke(ID, true);
            FireBaseManager.Instant.LogADEvent(AD_TYPE.native, AD_STATE.load_done);

        }

        private void HandleNativeAdImpression(object sender, EventArgs e)
        {
            Debug.Log(CONSTANT.Prefix + $"===> Handle ad native impression! ");
        }


        private void AdLoader_OnNativeAdClicked(object sender, EventArgs e)
        {
            Debug.Log(CONSTANT.Prefix + $"===> Handle ad native clicked! ");
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.native, adNetwork: "ADMOB");
            try
            {
                _clickADCallback[(int)AD_TYPE.native]?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke click inter callback, error: " + exception.Message + " <==");
            }
           
        }


        #endregion

#endif
        #region CheckAdLoaded

        public override bool InterstitialIsLoaded()
        {
            return MaxSdk.IsInitialized() && MaxSdk.IsInterstitialReady(_InterstitialAdUnitID);
        }

        public override bool VideoRewardIsLoaded()
        {
            return MaxSdk.IsInitialized() && MaxSdk.IsRewardedAdReady(_RewardedAdUnitID);
        }

        public override bool AdsOpenIsLoaded(int ID = 0)
        {
            return MaxSdk.IsInitialized() && MaxSdk.IsAppOpenAdReady(_OpenAdUnitIDs[ID]);
        }

        public override bool NativeAdLoaded(int ID)
        {
            if (!_isSDKAdMobInitDone)
                return false;
#if UNITY_EDITOR
            return true;
#elif NATIVE_AD
            if (ID >= this._nativeAd.Count)
                            return false;
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
        /// _= AdMHighFather.Instant.ShowBanner();
        /// </code>
        /// </summary>
        public override void ShowBanner()
        {
            _ = ShowBannerAsync();
        }

        async Task ShowBannerAsync()
        {
            if (_offAdPosition[(int)AD_TYPE.banner])
                return;

            float timer = 0;
            while (!_isBannerInitDone && timer < 240000)
            {
                await Task.Delay(500);
                timer += 500;
            }

            Debug.Log(CONSTANT.Prefix + $"==> show banner <==");
            _isBannerCurrentlyAllow = true;

            if (!string.IsNullOrWhiteSpace(_BannerAdUnitID))
                MaxSdk.ShowBanner(_BannerAdUnitID);

        }

        /// <summary>
        /// Hide AD Banner, It doesn't matter SDK init done or not
        /// <code>
        /// AdMHighFather.Instant.DestroyBanner();
        /// </code>
        /// </summary>
        public override void DestroyBanner()
        {
            Debug.Log(CONSTANT.Prefix + $"==> destroy banner <==");
            _isBannerCurrentlyAllow = false;

            if (!string.IsNullOrWhiteSpace(_BannerAdUnitID))
                MaxSdk.HideBanner(_BannerAdUnitID);
        }


        /// <summary>
        /// Show AD inter, if user watch ad to get reward but ad not load done yet then you must show the popup "AD not avaiable", then set showNoAds = true
        /// <code>
        ///AdMHighFather.Instant.ShowInterstitial((interState, true)=>{
        ///});
        /// </code>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="showNoAds"></param>
        public override void ShowInterstitial(Action<InterVideoState> callback = null, bool showNoAds = false)
        {
            if (_offAdPosition[(int)AD_TYPE.inter])
            {
                callback?.Invoke(InterVideoState.None);
                return;
            }
            if (InterstitialIsLoaded())
            {
                isShowingAD = true;
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
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback inter, error: " + e.ToString() + " <==");
            }
            if (_popUpNoAd && showNoAds) _popUpNoAd.SetActive(true);
        }


        /// <summary>
        /// Show AD reward, if user watch ad to get reward but ad not load done yet then you must show the popup "AD not avaiable", then set showNoAds = true
        /// <code>
        ///AdMHighFather.Instant.ShowRewardVideo((interState, true)=>{
        ///});
        /// </code>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="showNoAds"></param>
        public override void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false)
        {
            if (_offAdPosition[(int)AD_TYPE.reward])
            {
                callback?.Invoke(RewardVideoState.Watched);
                return;
            }

            if (VideoRewardIsLoaded())
            {
                isShowingAD = true;
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
                    Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback reward, error: " + e.ToString() + " <==");
                }
                if (_popUpNoAd && showNoAds) _popUpNoAd.SetActive(true);
            }
        }

        /// <summary>
        /// Show ad open or resume, if you need callback must check is ad show done or not
        /// <code>
        /// AdMHighFather.Instant.ShowAdOpen(true,(isSuccess)=>{
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
        public override void ShowAdOpen(int ID = 0, bool isAdOpen = false, Action<int, OpenAdState> callback = null)
        {
            if (isAdOpen && _offAdPosition[(int)AD_TYPE.open])
            {
                callback?.Invoke(ID, OpenAdState.None);
                return;
            }

            if (!isAdOpen && _offAdPosition[(int)AD_TYPE.resume])
            {
                callback?.Invoke(ID, OpenAdState.None);
                return;
            }

            if (isShowingAD)
            {
                FireBaseManager.Instant.adTypeShow = AD_TYPE.resume;
                return;
            }

            if (CheckInternetConnection() && AdsOpenIsLoaded(ID))
            {
                isShowingAD = true;
                FireBaseManager.Instant.adTypeShow = isAdOpen ? AD_TYPE.open : AD_TYPE.resume;
                MaxSdk.ShowAppOpenAd(_OpenAdUnitIDs[ID]);
                _callbackOpenAD = callback;
            }
            else
            {
                FireBaseManager.Instant.adTypeShow = AD_TYPE.resume;
                try
                {
                    callback?.Invoke(ID, OpenAdState.None);
                }
                catch (Exception e)
                {
                    Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback adopen/resume, error: " + e.ToString() + " <==");
                }
            }
        }

        /// <summary>
        /// Show ad open or resume, if you need callback must check is ad show done or not
        /// <code>
        /// AdMHighFather.Instant.ShowAdOpen((isSuccess)=>{
        ///       if(isSuccess){
        ///         Debug.Log("Done!");
        ///       }else{
        ///         Debug.Log("Fail!");
        ///       }
        /// })
        /// </code>
        /// </summary> 
        /// <param name="callback">Callback when adopen show done or fail pass true if ad show success and false if ad fail</param>
        public override void ShowAdOpen(Action<int, OpenAdState> callback = null)
        {
            int ID = _OpenAdUnitIDs.Count - 1;
            if (ID < 0)
                return;
            ShowAdOpen(ID, false, callback);
        }

#if NATIVE_AD

        public override void ShowNativeAsync(int ID, Action<AdNativeObject> callBack = null)
        {
            _ = ShowNative(ID,callBack);
        }
        /// <summary>
        /// ID is index of ID native AD
        /// Actually, Native AD object already create right when native AD success!
        /// Show function using for assign Native AD object into right canvas
        /// <code>
        /// _= AdMHighFather.Instant.ShowNative(0,(nativePanel)=>{
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
            if (_offAdPosition[(int)AD_TYPE.native])
                return;

            if (ID >= _adNativePanel.Count || ID >= _nativeAd.Count)
                return;
            if (_adNativePanel[ID] == null)
                return;

#if UNITY_EDITOR
            await Task.Delay(50);
#else
            float timer = 0;
            while (_nativeAd[ID] == null  && timer < 240000)
            {
                await Task.Delay(500);
                timer += 500;
            }
#endif
            _adNativePanel[ID].gameObject.SetActive(true);
            try
            {
                callBack?.Invoke(_adNativePanel[ID]);
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"===>Error on callback show native! error: " + e.ToString() + "<====");
            }
            _adNativePanel[ID].FitCollider();

        }

        public override void HideNativeAsync(int ID, Action<AdNativeObject> callBack = null)
        {
            _ = HideNative(ID,callBack);
        }

        public async Task HideNative(int ID, Action<AdNativeObject> callBack = null)
        {
            if (ID >= _adNativePanel.Count || _adNativePanel[ID] == null)
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> item ad native ID {0} doesnt exist! ", ID);
            }


#if UNITY_EDITOR
            await Task.Delay(50);
#else
            float timer = 0;
            while (_nativeAd[ID] == null  && timer < 240000)
            {
                await Task.Delay(500);
                timer += 500;
            }
#endif 
            try
            {
                callBack?.Invoke(_adNativePanel[ID]);
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"===>Error on callback show native! error: " + e.ToString() + "<====");
            }
            _adNativePanel[ID].gameObject.SetActive(false);
            _adNativePanel[ID].transform.SetParent(this.transform);
        }

#endif //NATIVE_AD

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

        public override bool CheckInternetConnection()
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

                if (_isClickedBanner)
                {
                    _isClickedBanner = false;
                    return;
                }
                if (isShowingAD)
                {
                    _callbackInter?.Invoke(InterVideoState.Interupt);
                    //_callbackInter = null;

                    _callbackReward?.Invoke(RewardVideoState.Interupt); 

                    _callbackOpenAD?.Invoke(_OpenAdUnitIDs.Count - 1, OpenAdState.None);
                    //_callbackOpenAD = null;
                    return;
                }
                this.ShowAdOpen(_OpenAdUnitIDs.Count - 1);
            }
        }

        #endregion

        #region CUSTOM LIB
        #endregion
    }
}


