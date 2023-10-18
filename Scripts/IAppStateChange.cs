using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

namespace DVAH
{
#if UNITY_IOS
    public enum AppState
    {
        Foreground,
        Background
    }

#endif
    public interface IAppStateChange
    {
         

        /// <summary>
        /// Function Invoke when app change state, you can check state param to know what state of application is
        /// <code>
        /// OnAppStateChanged(AppState state){
        ///     if(state == AppState.Foreground){
        ///         Debug.Log("User back to app");
        ///     }
        ///
        ///     if(state == AppState.Background){
        ///         Debug.Log("User hide app to background");
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="state">A state of application. Foreground mean app active, Background mean app deactive</param> 
        public void OnAppStateChanged(AppState state);

    }
}
