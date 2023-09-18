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
        Click,
        Closed,
        Interupt
    }
    public enum RewardVideoState
    {
        None,
        Open,
        Click,
        Watched,
        Closed,
        Interupt
    }

    public enum OpenAdState
    {
        None,
        Open,
        Click,
        Closed
    }

    public enum AD_TYPE
    {
        open,
        resume,
        banner,
        inter,
        reward,
        native
    }


    public abstract class AdMHighFather : Singleton<AdMHighFather>, IChildLib
    {
        #region Lib Properties

        protected bool isShowingAD = false;
        [SerializeField]
        protected bool _isBannerAutoShow = false, _initBannerManually;

         
        protected bool _isBannerCurrentlyAllow = false;
        protected bool[] _offAdPosition = new bool[] { false, false, false, false, false, false };

        public bool isAdBanner => _isBannerCurrentlyAllow;

        [SerializeField] protected GameObject _popUpNoAd;

        #endregion

        #region Properties
        protected DVAH_Data DVAH_Data;
        [SerializeField]
        protected string _paid_ad_revenue = "paid_ad_impression_value";
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

        [Header("---ID---")]
        [Space(10)]
        [SerializeField]
        protected string _MaxSdkKey = "3N4Mt8SNhOzkQnGb9oHsRRG1ItybcZDpJWN1fVAHLdRagxP-_k_ZXVaMAdMe5Otsmp6qJSXskfsrtakfRmPAGW";
        [SerializeField]
        protected string _BannerAdUnitID = "df980c4d809fc01e",
            _InterstitialAdUnitID = "3a70c7be99dade7d",
            _RewardedAdUnitID = "6b7094c5d21fcfe5";

        [SerializeField] protected List<string> _OpenAdUnitIDs = new List<string>();


        [SerializeField]
        protected List<string> _NativeAdID = new List<string>();
#if NATIVE_AD
        [Header("------NativeObject--------")]
        [SerializeField] protected List<AdNativeObject> _adNativePanel = new List<AdNativeObject>();
        protected AdNativeObject adNativeObject;

        protected List<NativeAd> _nativeAd = new List<NativeAd>();

        protected List<AdLoader> _nativeADLoader = new List<AdLoader>();
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

        #endregion

        #region CallBack Action
        protected Action<InterVideoState> _callbackInter = null;
        protected Action<RewardVideoState> _callbackReward = null;
        protected Action<int, OpenAdState> _callbackOpenAD = null;
        protected Action<int, bool> _callbackLoadNativeAd = null;

        protected Action[] _clickADCallback = new Action[6];

        #endregion

        public abstract bool[] getOffAdPosition();
        public abstract void Init(Action onInitDone = null);

        #region ClickCallBack
        public abstract AdMHighFather AssignClickCallBack(Action callback, AD_TYPE adType);

        public abstract AdMHighFather setOffAdPosition(bool isOff, params AD_TYPE[] aD_TYPE);
        #endregion

#if NATIVE_AD
        public abstract void SetAdNativeKeepReload(int ID, bool isKeepReload);
#endif 

        #region FUNCTION LOAD ADs
        public abstract void InitializeBannerAdsAsync();

#if NATIVE_AD
        public abstract void LoadNativeADsAsync(params int[] IDs);

        public abstract void LoadNativeADsAsync(Action<int, bool> callback, params int[] indexes);
#endif

        #endregion

        #region FUNCTION SHOW/HIDE ADs

        public abstract void ShowMRECs();

        public abstract void HideMRECs();


        public abstract void ShowBanner();

        public abstract void DestroyBanner();

        public abstract void ShowInterstitial(Action<InterVideoState> callback = null, bool showNoAds = false);


        public abstract void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false);

        public abstract void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false, Button btnShowAd = null);

        public abstract void ShowAdOpen(int ID = 0, bool isAdOpen = false, Action<int, OpenAdState> callback = null);

        public abstract void ShowAdOpen(Action<int, OpenAdState> callback = null);

#if NATIVE_AD

        public abstract void ShowNativeAsync(int ID, Action<AdNativeObject> callBack = null);


        public abstract void HideNativeAsync(int ID, Action<AdNativeObject> callBack = null);

#endif //NATIVE_AD

        #endregion

        #region FUNCTION CHECK LOAD ADs

        public abstract bool InterstitialIsLoaded();

        public abstract bool VideoRewardIsLoaded();

        public abstract bool AdsOpenIsLoaded(int ID = 0);

        public abstract bool NativeAdLoaded(int ID);

        #endregion


        public abstract bool CheckInternetConnection();
        public abstract Task ShowAdDebugger();

        #region CUSTOM FUNCTION
        #endregion
    }
}


