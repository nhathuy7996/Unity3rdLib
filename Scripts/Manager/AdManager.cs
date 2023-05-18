 using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
namespace DVAH
{ 

    public class AdManager: Singleton<AdManager> {

       

        public bool[] offAdPositions => AdMHighFather.Instant.getOffAdPosition();

        #region INJECT FUNCTION
        public AdManager AssignClickCallBack(Action callback, AD_TYPE adType)
        {
            _ = AdMHighFather.Instant.AssignClickCallBack(callback, adType);
            return this;
        }

        public AdManager setOffAdPosition(bool isOff, params AD_TYPE[] aD_TYPE)
        {
            _ = AdMHighFather.Instant.setOffAdPosition(isOff, aD_TYPE);
            return this;
        }

#if NATIVE_AD
        public AdManager SetAdNativeKeepReload(int ID, bool isKeepReload) {
            AdMHighFather.Instant.SetAdNativeKeepReload(ID, isKeepReload);
            return this;
        }
#endif
#endregion

        #region FUNCTION LOAD ADs
        public void InitializeBannerAdsAsync() {
            AdMHighFather.Instant.InitializeBannerAdsAsync();
        }

#if NATIVE_AD
        public void LoadNativeADsAsync(params int[] IDs) {
            AdMHighFather.Instant.LoadNativeADsAsync(IDs);
        }

        public void LoadNativeADsAsync(Action<int, bool> callback, params int[] indexes) {
            AdMHighFather.Instant.LoadNativeADsAsync(callback,indexes);
        }
#endif

#endregion

        #region FUNCTION SHOW/HIDE ADs

        /// <summary>
        /// Show AD Banner, It doesn't matter SDK init done or not
        /// <code>
        /// AdManager.Instant.ShowBanner();
        /// </code>
        /// </summary>
        public void ShowBanner()
        {
            AdMHighFather.Instant.ShowBanner();
        }

        /// <summary>
        /// Hide AD Banner, It doesn't matter SDK init done or not
        /// <code>
        /// AdManager.Instant.DestroyBanner();
        /// </code>
        /// </summary>
        public void DestroyBanner()
        {
            AdMHighFather.Instant.DestroyBanner();
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
            AdMHighFather.Instant.ShowInterstitial(callback, showNoAds);
        }


        /// <summary>
        /// Show AD reward, if user watch ad to get reward but ad not load done yet then you must show the popup "AD not avaiable", then set showNoAds = true
        /// <code>
        ///AdManager.Instant.ShowRewardVideo((interState, true)=>{
        ///});
        /// </code>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="showNoAds">if you wanna show a popup "ad not avaiable!"</param>
        public void ShowRewardVideo(Action<RewardVideoState> callback = null, bool showNoAds = false)
        {
            AdMHighFather.Instant.ShowRewardVideo(callback, showNoAds);
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
        public void ShowAdOpen(int ID = 0, bool isAdOpen = false, Action<int, OpenAdState> callback = null)
        {
            AdMHighFather.Instant.ShowAdOpen(ID, isAdOpen,callback);
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
        public void ShowAdOpen(Action<int, OpenAdState> callback = null)
        {
            AdMHighFather.Instant.ShowAdOpen(callback);
        }

#if NATIVE_AD
        /// <summary>
        /// ID is index of ID native AD
        /// Actually, Native AD object already create right when native AD success!
        /// Show function using for assign Native AD object into right canvas
        /// <code>
        /// AdManager.Instant.ShowNative(0,(nativePanel)=>{
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
        public void ShowNativeAsync(int ID, Action<AdNativeObject> callBack = null)
        {
            AdMHighFather.Instant.ShowNativeAsync(ID, callBack);

        }

        /// <summary>
        /// ID is index of ID native AD
        /// Actually, Native AD object already create right when native AD success!
        /// Hide function using for deactive then assign Native AD to admanager
        /// <code>
        /// AdManager.Instant.HideNative(0,(nativePanel)=>{
        ///        
        /// })
        /// </code>
        /// </summary> 
        /// <param name="callback">Callback return adNative panel, dont destroy it!!</param>
        public void HideNativeAsync(int ID, Action<AdNativeObject> callBack = null)
        {
            AdMHighFather.Instant.HideNativeAsync(ID, callBack);
        }

#endif //NATIVE_AD

        #endregion

        #region FUNCTION CHECK LOAD ADs

        public bool InterstitialIsLoaded()
        {
            return AdMHighFather.Instant.InterstitialIsLoaded();
        }

        public bool VideoRewardIsLoaded()
        {
            return AdMHighFather.Instant.VideoRewardIsLoaded();
        }

        public bool AdsOpenIsLoaded(int ID = 0)
        {
            return AdMHighFather.Instant.AdsOpenIsLoaded(ID);
        }

        public bool NativeAdLoaded(int ID)
        {

            return AdMHighFather.Instant.NativeAdLoaded(ID);

        }

        #endregion

        #region CUSTOM FUNCTION
        #endregion
    }
}


