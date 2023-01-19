using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using System;
using com.adjust.sdk;
using GoogleMobileAds.Editor;
using HuynnLib;
using System.IO;

public class BuildDone : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        string reportContent = string.Format("Time: " + DateTime.Now +
            "Product Name: {0} \n" +
            "App Version: {1} \n" +
            "Version code: {2} \n" +
            "Package Name: {3} \n",
            PlayerSettings.productName,
            PlayerSettings.bundleVersion,
            PlayerSettings.Android.bundleVersionCode,
            PlayerSettings.applicationIdentifier);

        Adjust adjustObject = GameObject.FindObjectOfType<Adjust>();
        if (adjustObject)
        {
            string adjust = string.Format("-------------ADJSUT--------------\n" +
              "Token: {0} \n" +
              "Mode: {1} \n",
            adjustObject.appToken,
             adjustObject.environment);

            reportContent += adjust;
        }

        GoogleMobileAdsSettings gg = null;
        string[] ggSetting = UnityEditor.AssetDatabase.FindAssets("t:GoogleMobileAdsSettings");
        AdManager adManagerObject = GameObject.FindObjectOfType<HuynnLib.AdManager>();

        if (ggSetting.Length != 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(ggSetting[0]);
            gg = UnityEditor.AssetDatabase.LoadAssetAtPath<GoogleMobileAdsSettings>(path);
        }
        else
        {
            EditorGUILayout.LabelField("Can not find GoogleMobileAdsSettings!");
        }

        if (gg && adManagerObject)
        {
            string googleReport = string.Format("-------------GOOGLE ADMOB--------------\n" +
                "Android AD ID: {0} \n" +
                "Event Paid AD: {1} \n",
                gg.GoogleMobileAdsAndroidAppId,
                adManagerObject.paid_ad_revenue );

            reportContent += googleReport;
        }

        AppLovinSettings max = null;
        string[] maxSetting = UnityEditor.AssetDatabase.FindAssets("t:AppLovinSettings");
        if (maxSetting.Length != 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(maxSetting[0]);
            max = UnityEditor.AssetDatabase.LoadAssetAtPath<AppLovinSettings>(path);
        }
        else
        {
            Debug.LogError("Can not find MaxSdkSetting!");
        }

        if (max != null)
        {

            string adjsutReport = string.Format("-------------MAX--------------\n" +
                "MaxSdk key: {0} \n" +
                "Android AD ID: {1} \n",
                max.SdkKey,
                max.AdMobAndroidAppId);

            reportContent += adjsutReport;
        }

        if(adManagerObject != null)
        {
            string adReport = string.Format("-------------AD MAX ID--------------\n" +
                "Banner ID: {0} \n" +
                "Inter ID: {1} \n" +
                "Reward ID: {2} \n" +
                "AppOpen ID: {3} \n",
                adManagerObject.BannerAdUnitID,
                adManagerObject.InterstitialAdUnitID,
                adManagerObject.RewardedAdUnitID,
                adManagerObject.OpenAdUnitID);

            reportContent += adReport;
        }

        FileStream stream = new FileStream(Application.dataPath +"/HuynnReportBuild", FileMode.Create);
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(reportContent);

            writer.Flush();
            writer.Close();
        }
    }
}
