using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="DVAH/Create Data File",fileName ="DVAH_Data")]
public class DVAH_Data : ScriptableObject
{
    public string LinkGoogleSheet = "";
    //Adjust
    public string Adjust_token = "";
    public ADJUST_MODE AdjustMode = ADJUST_MODE.Sandbox;

    //Firebase
    public string EventsToken_AdValue = "";
    public string EventsToken_LevelAchives = "";

    //Google
    public string Google_Android_AppID = "";
    public string Google_Event_Paid_AD = "paid_ad_impression_value";

   
    //FaceBook
    public string Facebook_AppID = "";
    public string Facebook_ClientToken = "";

    //MAX
    public string AppLovin_SDK_Key = "";
    public string AppLovin_BannerID = "";
    public string AppLovin_InterID = "";
    public string AppLovin_RewardID = "";

    public List<string> AppLovin_ADOpenIDs = new List<string>();

#if NATIVE_AD
    public List<string> AppLovin_NativeAdIDs = new List<string>();
#endif

    public bool CHEAT_BUILD = false;
}

public enum ADJUST_MODE
{
    Sandbox = 0,
    Production = 1
}
