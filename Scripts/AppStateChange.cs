using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

public abstract class AppStateChange : MonoBehaviour
{
    #if UNITY_ANDROID
    protected void Awake()
    {
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        
    }

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
    protected abstract void OnAppStateChanged(AppState state);
 

    private void OnApplicationFocus(bool focus)
    {
        this.OnAppStateChanged(!focus ? AppState.Background : AppState.Foreground);
    }
#endif


}
