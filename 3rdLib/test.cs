using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HuynnLib;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Listen to application foreground and background events.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(IAPManager.Instant.gameObject.name);
            Debug.LogError(IAPManager.Instant.gameObject.name);
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        UnityEngine.Debug.Log("App State is " + state.ToString());
        if (state == AppState.Foreground)
        {
            AdManager.Instant.ShowAdIfAvailable();
        }
    }


}
