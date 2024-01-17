#if ADMOB
using com.adjust.sdk;
 
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using GoogleMobileAds.Mediation.LiftoffMonetize.Api;
using GoogleMobileAds.Ump.Api;
//using GoogleMobileAds.Api.Mediation.AppLovin;

namespace DVAH
{
    public class AdMHighFather_Admob : AdMHighFather, IAppStateChange
    {
        [SerializeField] string _mrecAdUnitId;

        [SerializeField]
        GoogleMobileAds.Api.AdPosition _bannerPosition = AdPosition.Bottom;
        public AdPosition BannerPosition => _bannerPosition;

        BannerView _bannerView;
        InterstitialAd _interstitialAd;
        RewardedAd _rewardedAd;
        List<AppOpenAd> _appOpenAds;

        private int bannerRetryAttempt,
            interstitialRetryAttempt,
            rewardedRetryAttempt,
            NativeAdRetryAttemp = 1;

        List<int> AdOpenRetryAttemp = new List<int>();

        private int  _isSDKAdMobInitDone = 0,
            _isBannerInitDone = 0;

        bool[] _isnativeKeepReload;

        bool _isClickedBanner = false;

        Button btnAdReward = null;


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
                this.setOffAdPosition(true, AD_TYPE.banner, AD_TYPE.inter, AD_TYPE.reward, AD_TYPE.open, AD_TYPE.resume, AD_TYPE.native);
            }

            //InitMAX();
            InitAdMob();

            //AppStateEventNotifier.AppStateChanged += this.OnAppStateChanged;
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

       
        void InitAdMob()
        {

            // Create a ConsentRequestParameters object.
            ConsentRequestParameters request = new ConsentRequestParameters();

            // Check the current consent information status.
            ConsentInformation.Update(request, OnConsentInfoUpdated);

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
            
#endif

            _appOpenAds = new List<AppOpenAd>();
            for (int i = 0; i< _OpenAdUnitIDs.Count; i++)
            {
                _appOpenAds.Add(null);
            }

            //AppLovin.SetHasUserConsent(true);
            //AppLovin.SetIsAgeRestrictedUser(true);
            //AppLovin.SetDoNotSell(true);


            LiftoffMonetize.UpdateCCPAStatus(VungleCCPAStatus.OPTED_IN);
            LiftoffMonetize.UpdateConsentStatus(VungleConsentStatus.OPTED_IN, "1.0.0");

         
        }

        void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                UnityEngine.Debug.LogError(consentError);
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    UnityEngine.Debug.LogError(consentError);
                    return;
                }

                // Consent has been gathered.
                if (ConsentInformation.CanRequestAds())
                {
                    MobileAds.Initialize(initStatus =>
                    {
                        System.Threading.Interlocked.Exchange(ref _isSDKAdMobInitDone, 1);
                        Debug.Log(CONSTANT.Prefix + $"==> Admob SDK Initialized <==");

                        InitAdOpen();

                        //if (!string.IsNullOrEmpty(_mrecAdUnitId))
                        //    InitializeMRecAds();


                        if (!_offAdPosition[(int)AD_TYPE.inter])
                            InitializeInterstitialAds();

                        if (!_offAdPosition[(int)AD_TYPE.banner] && !_initBannerManually)
                            InitializeBannerAdsAsync();

                        if (!_offAdPosition[(int)AD_TYPE.reward])
                            InitializeRewardedAds();

                    });
                }
            });

        }

        public void ShowPrivacyOptionsForm()
        {
            Debug.Log("Showing privacy options form.");

            ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
            {
                if (showError != null)
                {
                    Debug.LogError("Error showing privacy options form with error: " + showError.Message);
                }
                
            });
        }

        public override async Task ShowAdDebugger()
        {
            float timer = 0;
            while (System.Threading.Interlocked.Add(ref _isSDKAdMobInitDone, 0) == 0 && timer < 240000)
            {
                Debug.LogWarning(CONSTANT.Prefix + $"==>Waiting Max SDK init done!<==");
                await Task.Delay(500);
                timer += 500;
            }

            MobileAds.OpenAdInspector(error => {
                Debug.LogError(CONSTANT.Prefix + $"==>{error}!<==");
            });
        }

#if MRECs
        #region MREC Ad Methods
        public void InitializeMRecAds()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Init MRECs banner <==");
            // MRECs are sized to 300x250 on phones and tablets
            MaxSdk.CreateMRec(_mrecAdUnitId, MaxSdkBase.AdViewPosition.BottomCenter);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;

            MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
        }

        public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MaxSdk.StartMRecAutoRefresh(adUnitId);
            Debug.Log(CONSTANT.Prefix + $"==> MRECs Banner ad loaded " + adUnitId + " <==");
        }

        public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> MRECs Banner ad loaded " + adUnitId + " <==");
        }

        public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> MRECs Banner ad click " + adUnitId + " <==");
        }



        public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        #endregion
#endif
#region Banner Ad Methods

        public override void InitializeBannerAdsAsync()
        {
            _ = InitializeBannerAds();
        }

        public async Task InitializeBannerAds()
        {
            float timer = 0;
            while (System.Threading.Interlocked.Add(ref _isSDKAdMobInitDone, 0) == 0 && timer < 240000)
            {
                Debug.LogWarning(CONSTANT.Prefix + $"==>Waiting Admob SDK init done!<==");
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
                FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.load, adStatus: AD_STATUS.init, adUnit: _BannerAdUnitID);
                // Attach Callbacks
                CreateBannerView();

                string adID = _BannerAdUnitID;

                _bannerView.OnBannerAdLoaded += OnBannerAdLoadedEvent;
                _bannerView.OnBannerAdLoadFailed += OnBannerAdFailedEvent;
                _bannerView.OnAdClicked += OnBannerAdClickedEvent;
                _bannerView.OnAdPaid += (AdValue adValue) =>
                {  
                    Debug.Log(CONSTANT.Prefix + $"==> Banner ad revenue paid <==");
                   
                    string adNetWork = _bannerView.GetResponseInfo().GetMediationAdapterClassName();
                    TrackAdRevenue(adValue,"Banner", adNetWork, adID); 
                    FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.impression,
                        adStatus: AD_STATUS.success, adNetwork:adNetWork, adUnit: adID, value: adValue.Value.ToString());
                };

                _bannerView.OnAdPaid += (AdValue adValue) =>
                { 
                    string adNetWork = _bannerView.GetResponseInfo().GetMediationAdapterClassName();
                    OnAdRevenuePaidEvent(adValue,"banner", adNetWork, adID);
                };

                // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
                // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.

                ManuallyLoadBanner();
                if (_isBannerAutoShow)
                    ShowBanner();
                else
                    DestroyBanner();

                System.Threading.Interlocked.Exchange(ref _isBannerInitDone, 1); 
            });
        }
        public void CreateBannerView()
        {
            Debug.Log("Creating banner view");

            if (_bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                _bannerView.Destroy();
                _bannerView = null;
            }

            // Create a 320x50 banner at top of the screen
            _bannerView = new BannerView(_BannerAdUnitID, AdSize.Banner, _bannerPosition);
        }

        void ManuallyLoadBanner()
        {
            // create an instance of a banner view first.
            if (_bannerView == null)
            {
                CreateBannerView();
            }

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            Debug.Log("Loading banner ad.");
            _bannerView.LoadAd(adRequest);
        }
 
        private void OnBannerAdLoadedEvent()
        {
            if (_isBannerCurrentlyAllow)
                this.ShowBanner();

            var bannerInfo = _bannerView.GetResponseInfo();
            Debug.Log(CONSTANT.Prefix + $"==> Banner ad loaded " +_BannerAdUnitID+ " <=="); 
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.load, adStatus: AD_STATUS.success,
                adUnit: _BannerAdUnitID, adNetwork: bannerInfo.GetMediationAdapterClassName());
        }

        private void OnBannerAdFailedEvent(LoadAdError error)
        {
            // Banner ad failed to load. MAX will automatically try loading a new ad internally.
            Debug.LogError(CONSTANT.Prefix + $"==>Banner ad failed to load with error code: " + error.GetCode() + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.load,
                adStatus: AD_STATUS.fail, adUnit: _BannerAdUnitID, errorCode: error.GetCode().ToString());
            bannerRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));

            Invoke("ManuallyLoadBanner", (float)retryDelay);
        }

        private void OnBannerAdClickedEvent()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Banner ad clicked <==");
            string adNetWork = _bannerView.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.banner, adNetwork: adNetWork);
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
            // Load the first interstitial
            LoadInterstitial();
        }



        void LoadInterstitial()
        {
            Debug.Log(CONSTANT.Prefix + $"==>Start load Interstitial " + _InterstitialAdUnitID + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adAction: AD_ACTION.load,
                adStatus: AD_STATUS.init, adUnit: _InterstitialAdUnitID);

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
            InterstitialAd.Load(_InterstitialAdUnitID, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        OnInterstitialFailedEvent(ad, error);
                        return;
                    }

                    _interstitialAd = ad;
                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());
                    OnInterstitialLoadedEvent(ad);

                    string adID = _InterstitialAdUnitID;
                    // Raised when the ad is estimated to have earned money.
                    _interstitialAd.OnAdPaid += (AdValue adValue) =>
                    {

                       
                        string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();

                        Debug.Log(CONSTANT.Prefix + $"==> Inter ad revenue paid <==");
                        TrackAdRevenue(adValue, "Inter", adNetWork,adID);
                     
                        FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.impression,
                            adStatus: AD_STATUS.success, adNetwork: adNetWork, adUnit: adID, value: adValue.Value.ToString());
                    };

                    _interstitialAd.OnAdPaid += (AdValue adValue) =>
                    {
                         
                        string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();
                        OnAdRevenuePaidEvent(adValue, "Inter", adNetWork, adID);
                    };
                    // Raised when an impression is recorded for an ad.
                    _interstitialAd.OnAdImpressionRecorded += () =>
                    {
                        Debug.Log("Interstitial ad recorded an impression.");
                    };
                    // Raised when a click is recorded for an ad.
                    _interstitialAd.OnAdClicked += Interstitial_OnAdClickedEvent;
                    // Raised when an ad opened full screen content.
                    _interstitialAd.OnAdFullScreenContentOpened += Interstitial_OnAdDisplayedEvent;
                    // Raised when the ad closed full screen content.
                    _interstitialAd.OnAdFullScreenContentClosed += OnInterstitialDismissedEvent;
                    // Raised when the ad failed to open full screen content.
                    _interstitialAd.OnAdFullScreenContentFailed += InterstitialFailedToDisplayEvent;

                    
                });
        }

        private void OnInterstitialLoadedEvent(InterstitialAd ad)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            Debug.Log(CONSTANT.Prefix + $"==> Interstitial loaded <==");
            string adID = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adAction: AD_ACTION.load, adStatus: AD_STATUS.success,
                adUnit: adID, adNetwork: adNetWork);

            // Reset retry attempt
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(InterstitialAd ad, LoadAdError error)
        {
            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
            string adID = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId; 
            Debug.LogError(CONSTANT.Prefix + $"==> Interstitial failed to load with error code: " + error.GetCode() + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adAction: AD_ACTION.load,
                adStatus: AD_STATUS.fail, adUnit: adID, errorCode: error.GetCode().ToString());


            Invoke("LoadInterstitial", (float)retryDelay);
        }

        private void Interstitial_OnAdDisplayedEvent()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Interstitial show! <==");
            _callbackInter?.Invoke(InterVideoState.Open);
            string networkName = _interstitialAd.GetResponseInfo().GetMediationAdapterClassName();
            string adID = _interstitialAd.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adAction: AD_ACTION.show,
                adStatus: AD_STATUS.success, adUnit: adID, adNetwork: networkName);

        }

        private void InterstitialFailedToDisplayEvent(AdError error )
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Debug.LogError(CONSTANT.Prefix + $"==> Interstitial failed to display with error code: " + error.GetCode() + " <==");
            string networkName = _interstitialAd.GetResponseInfo().GetMediationAdapterClassName();
            string adID = _interstitialAd.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId; 
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.inter, adAction: AD_ACTION.show,
                adStatus: AD_STATUS.fail, adUnit: adID, errorCode: error.GetCode().ToString(), adNetwork: networkName);

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


        private void Interstitial_OnAdClickedEvent()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Inter ad clicked <==");
            string networkName = _interstitialAd.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogEventClickAds(ad_type: AD_TYPE.inter, adNetwork: networkName);
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

        private void OnInterstitialDismissedEvent()
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
            
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Load reward Ad " + _RewardedAdUnitID + " ! <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adAction: AD_ACTION.load,
                adStatus: AD_STATUS.init, adUnit: _RewardedAdUnitID);
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
            RewardedAd.Load(_RewardedAdUnitID, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        OnRewardedAdFailedEvent(error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedAd = ad;
                    OnRewardedAdLoadedEvent();
                    string adID = _RewardedAdUnitID;
                    _rewardedAd.OnAdPaid += (AdValue adValue) =>
                    {
                        string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();
                      
                        Debug.Log(CONSTANT.Prefix + $"==> Reward ad revenue paid <==");
                        TrackAdRevenue(adValue, "Reward", adNetWork, adID);
                      
                        FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.impression,
                            adStatus: AD_STATUS.success, adNetwork: adNetWork, adUnit: adID, value: adValue.Value.ToString());
                    };

                    _rewardedAd.OnAdPaid += (AdValue adValue) =>
                    {
                        string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName(); 
                        OnAdRevenuePaidEvent(adValue, "Reward", adNetWork, adID); 
                    };
                    // Raised when an impression is recorded for an ad. 
            
                    // Raised when a click is recorded for an ad.
                    _rewardedAd.OnAdClicked += OnRewardedAdClickedEvent;
                    // Raised when an ad opened full screen content.
                    _rewardedAd.OnAdFullScreenContentOpened += OnRewardedAdDisplayedEvent;
                    // Raised when the ad closed full screen content.
                    _rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdDismissedEvent;
                    // Raised when the ad failed to open full screen content.
                    _rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdFailedToDisplayEvent;
                });
        }

        private void OnRewardedAdLoadedEvent()
        { 
            // Reset retry attempt
            string networkName = _rewardedAd.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adUnit: _RewardedAdUnitID, adAction: AD_ACTION.load, adStatus: AD_STATUS.success,
                adNetwork: networkName);
            rewardedRetryAttempt = 0;
        }

        private void OnRewardedAdFailedEvent(AdError error)
        {
            // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).

            rewardedRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));

            Debug.LogError(CONSTANT.Prefix + $"==> Rewarded ad failed to load with error code: " + error.GetCode() + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adUnit: _RewardedAdUnitID, adAction: AD_ACTION.load,
                adStatus: AD_STATUS.fail, errorCode: error.GetCode().ToString());
            Invoke("LoadRewardedAd", (float)retryDelay);

        }

        private void OnRewardedAdFailedToDisplayEvent(AdError error)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            string adID = _rewardedAd.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string adNetWork = _rewardedAd.GetResponseInfo().GetMediationAdapterClassName();
            Debug.LogError(CONSTANT.Prefix + $"==> Rewarded ad failed to display with error code: " + error.GetCode() + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adUnit: adID, adAction: AD_ACTION.show,
               adStatus: AD_STATUS.fail, errorCode: error.GetCode().ToString(), adNetwork: adNetWork);
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

        private void OnRewardedAdDisplayedEvent()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Reward display success! <==");
            string adID = _rewardedAd.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string networKName = _rewardedAd.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.reward, adUnit: adID, adAction: AD_ACTION.show,
              adStatus: AD_STATUS.success, adNetwork: networKName);
            try
            {
                _callbackReward?.Invoke(RewardVideoState.Open);

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback display reward, error: " + e.ToString() + " <==");
            }
        }

        private void OnRewardedAdClickedEvent()
        {
            Debug.Log(CONSTANT.Prefix + $"==> Reward clicked! <==");

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

        private void OnRewardedAdDismissedEvent()
        {
            LoadRewardedAd();
            isShowingAD = false;

            try
            {
                _callbackReward?.Invoke(RewardVideoState.Closed);
                if (this.btnAdReward != null) this.btnAdReward.interactable = true;

            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Faild invoke callback reward, error: " + e.ToString() + " <==");
            }

            _callbackReward = null;
            Debug.Log(CONSTANT.Prefix + $"==> Reward closed! <==");
        }

        private void OnRewardedAdReceivedRewardEvent()
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

            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.open, adAction: AD_ACTION.load, adStatus: AD_STATUS.init,
                adUnit: _OpenAdUnitIDs[ID]);

            if (_appOpenAds[ID] != null  && _appOpenAds[ID].CanShowAd())
            {
                return;
            }

            // Clean up the old ad before loading a new one.
            if (_appOpenAds[ID] != null)
            {
                _appOpenAds[ID].Destroy();
                _appOpenAds[ID] = null;
            }

            Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(_OpenAdUnitIDs[ID], adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("app open ad failed to load an ad " +
                                       "with error : " + error);
                        AppOpenOnAdLoadFailedEvent(error, _OpenAdUnitIDs[ID]);
                        return;
                    }

                    Debug.Log("App open ad loaded with response : "
                              + ad.GetResponseInfo());

               
                    AppOpen_OnAdLoadedEvent(ad);
                    string adID = _OpenAdUnitIDs[ID];
                    // Raised when the ad is estimated to have earned money.
                    ad.OnAdPaid += (AdValue adValue) =>
                    {
                        Debug.Log(CONSTANT.Prefix + $"==> AppOpen ad revenue paid <==");
                        string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();
                        
                        TrackAdRevenue(adValue, "AppOpen", adNetWork,adID);
                   
                        FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.banner, adAction: AD_ACTION.impression,
                            adStatus: AD_STATUS.success, adNetwork: adNetWork, adUnit: adID, value: adValue.Value.ToString());
                    };

                    ad.OnAdPaid += (AdValue adValue) =>
                    {
                        string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName(); 
                        OnAdRevenuePaidEvent(adValue, "AppOpen", adNetWork,adID);
                    };
                    // Raised when an impression is recorded for an ad.
                    ad.OnAdImpressionRecorded += () =>
                    {
                        Debug.Log("App open ad recorded an impression.");
                    };
                    // Raised when a click is recorded for an ad.
                    ad.OnAdClicked += () =>
                    {
                        Debug.Log("App open ad was clicked.");
                        AppOpen_OnAdClickedEvent(ad);
                    };
                    // Raised when an ad opened full screen content.
                    ad.OnAdFullScreenContentOpened += () =>
                    {
                        Debug.Log("App open ad full screen content opened.");
                        AppOpen_OnAdDisplayedEvent(ad);
                    };
                    // Raised when the ad closed full screen content.
                    ad.OnAdFullScreenContentClosed += () =>
                    {
                        Debug.Log("App open ad full screen content closed.");
                        OnAppOpenDismissedEvent(ad);
                    };
                    // Raised when the ad failed to open full screen content.
                    ad.OnAdFullScreenContentFailed += (AdError error) =>
                    {
                        Debug.LogError("App open ad failed to open full screen content " +
                                       "with error : " + error);
                        AppOpen_OnAdDisplayFailedEvent(ad, error);
                    };

                    _appOpenAds[ID] = ad;
                });
        }


        private void AppOpen_OnAdLoadedEvent(AppOpenAd ad)
        {
            Debug.Log(CONSTANT.Prefix + $"==>Load ad open/resume success! <==");
            string adID = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string netWorkName = ad.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.open, adAction: AD_ACTION.load, adStatus: AD_STATUS.success,
               adUnit: adID, adNetwork: netWorkName);

            int ID = _appOpenAds.IndexOf(ad);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID] = 0;
        }

        private void AppOpenOnAdLoadFailedEvent(LoadAdError e, string adID)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> Load ad open/resume failed, code: " + e.GetCode() + " <==");
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.open, adAction: AD_ACTION.load, adStatus: AD_STATUS.fail,
              adUnit: adID, errorCode: e.GetCode().ToString());
            int ID = _OpenAdUnitIDs.IndexOf(adID);
            if (ID < 0)
                return;
            AdOpenRetryAttemp[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6, AdOpenRetryAttemp[ID]));


            waitLoadAdOpen((float)retryDelay, ID);
        }

        private void AppOpen_OnAdDisplayedEvent(AppOpenAd ad)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Show ad open/resume success! <==");
            string adID = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.open, adAction: AD_ACTION.show, adStatus: AD_STATUS.success,
           adUnit: adID, adNetwork: adNetWork);

            int ID = _appOpenAds.IndexOf(ad);
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

        private void AppOpen_OnAdClickedEvent(AppOpenAd ad)
        {
            Debug.Log(CONSTANT.Prefix + $"==>Click open/resume success! <==");
            string adID = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.open, adAction: AD_ACTION.click, adStatus: AD_STATUS.success,
         adUnit: adID, adNetwork: adNetWork);
            int ID = _appOpenAds.IndexOf(ad);
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

        private void AppOpen_OnAdDisplayFailedEvent(AppOpenAd ad, AdError error)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> Show ad open/resume failed, code: " + error.GetCode() + " <==");
            string adID = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
            string adNetwork = ad.GetResponseInfo().GetMediationAdapterClassName();
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.open, adAction: AD_ACTION.show, adStatus: AD_STATUS.fail,
         adUnit: adID, adNetwork: adNetwork, errorCode: error.GetCode().ToString());

            int ID = _appOpenAds.IndexOf(ad);
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


        public void OnAppOpenDismissedEvent(AppOpenAd ad)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Ad open/resume close! <==");

            isShowingAD = false;
            int ID = _appOpenAds.IndexOf(ad);
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

            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.native, adUnit: AdID, adAction: AD_ACTION.load,
              adStatus: AD_STATUS.init, adNetwork: "ADMOB");

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
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.native, adUnit: ((AdLoader)sender).AdUnitId, adAction: AD_ACTION.load,
               adStatus: AD_STATUS.fail, errorCode: e.LoadAdError.GetCode().ToString() , adNetwork: "ADMOB");

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
                FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.native, adUnit: _NativeAdID[ID], adAction: AD_ACTION.show,
             adStatus: AD_STATUS.init, adNetwork: "ADMOB");
            }
            else
            {
                RequestNativeAd(_NativeAdID[ID]);
                FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.native, adUnit: _NativeAdID[ID], adAction: AD_ACTION.show,
                adStatus: AD_STATUS.fail, adNetwork: "ADMOB");
            }

            _callbackLoadNativeAd?.Invoke(ID, true);
            FireBaseManager.Instant.LogADEvent(adType: AD_TYPE.native, adUnit: _NativeAdID[ID], adAction: AD_ACTION.load,
             adStatus: AD_STATUS.success, adNetwork: "ADMOB");

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
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        public override bool VideoRewardIsLoaded()
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }

        public override bool AdsOpenIsLoaded(int ID = 0)
        {
            return _appOpenAds[ID] != null && _appOpenAds[ID].CanShowAd();
        }

        public override bool NativeAdLoaded(int ID)
        {
            if (System.Threading.Interlocked.Add(ref _isSDKAdMobInitDone, 0) == 0)
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


        public override void ShowMRECs()
        {
            Debug.LogWarning(CONSTANT.Prefix + $"==>MRecs show call!<==");
#if MRECs
            MaxSdk.ShowMRec(_mrecAdUnitId);
#endif
        }

        public override void HideMRECs()
        {
            Debug.LogWarning(CONSTANT.Prefix + $"==>MRecs hide call!<==");
#if MRECs
            MaxSdk.HideMRec(_mrecAdUnitId);
#endif
        }


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
            while (System.Threading.Interlocked.Add(ref _isBannerInitDone, 0) == 0 && timer < 240000)
            {
                await Task.Delay(500);
                timer += 500;
            }

            Debug.Log(CONSTANT.Prefix + $"==> show banner <==");
            _isBannerCurrentlyAllow = true;

            if (!string.IsNullOrWhiteSpace(_BannerAdUnitID))
                _bannerView.Show();

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

            if (_bannerView != null && !string.IsNullOrWhiteSpace(_BannerAdUnitID))
                _bannerView.Hide();
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
                _interstitialAd.Show();
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

            if (isShowingAD)
                return;

            if (VideoRewardIsLoaded())
            {
                isShowingAD = true;
                _callbackReward = callback;
                _rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
                {
                    OnRewardedAdReceivedRewardEvent();
                });
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


        public override void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false, Button btnShowAd = null)
        {
            this.btnAdReward = btnShowAd;
            if (this.btnAdReward != null) this.btnAdReward.interactable = false;
            this.ShowRewardVideo(callback, showNoAds);
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
                _appOpenAds[ID].Show();
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

        private void TrackAdRevenue(AdValue adValue, string adPlacement, string adNetWork, string adUnitID)
        {
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);

            adjustAdRevenue.setRevenue(adValue.Value / 1000000f, "USD");
            adjustAdRevenue.setAdRevenueNetwork(adNetWork);
            adjustAdRevenue.setAdRevenueUnit(adUnitID);
            adjustAdRevenue.setAdRevenuePlacement(adPlacement);

            Adjust.trackAdRevenue(adjustAdRevenue);

            FireBaseManager.Instant.LogAdValueAdjust(adValue.Value);
        }

        private void OnAdRevenuePaidEvent(AdValue adValue, string adPlacement, string adNetWork, string adUnitID)
        {
             
            var impressionParameters = new[] { 
              new Firebase.Analytics.Parameter("ad_source", adNetWork),
              new Firebase.Analytics.Parameter("ad_unit_name", adUnitID),
              new Firebase.Analytics.Parameter("ad_format", adPlacement),
              new Firebase.Analytics.Parameter("value", adValue.Value/1000000f),
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


        public void OnAppStateChanged( AppState state)
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
                    if (this.btnAdReward != null) this.btnAdReward.interactable = true;

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

#endif
