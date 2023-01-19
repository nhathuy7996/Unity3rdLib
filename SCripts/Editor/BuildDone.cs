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
using Facebook.Unity.Settings;
using UnityEngine.Networking.Types;

public class BuildDone : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        //Report(report);
    }

    public static void Report(BuildReport report)
    {

        string reportContent = string.Format("Time: " + DateTime.Now + "\n" +
            "Product Name: {0} \n" +
            "App Version: {1} \n" +
            "Version code: {2} \n" +
            "Package Name: {3} \n" +
            "Path: {4} \n\n\n",
            PlayerSettings.productName,
            PlayerSettings.bundleVersion,
            PlayerSettings.Android.bundleVersionCode,
            PlayerSettings.applicationIdentifier,
            report.summary.outputPath);

        Adjust adjustObject = GameObject.FindObjectOfType<Adjust>();
        if (adjustObject)
        {
            string adjust = string.Format("               -------------ADJSUT--------------\n" +
              "Token: {0} \n" +
              "Mode: {1} \n\n\n",
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
            string googleReport = string.Format("               -------------GOOGLE ADMOB--------------\n" +
                "App ID: {0} \n" +
                "Event Paid AD: {1} \n\n\n",
                gg.GoogleMobileAdsAndroidAppId,
                adManagerObject.paid_ad_revenue);

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

            string adjsutReport = string.Format("               -------------MAX--------------\n" +
                "MaxSdk key: {0} \n" +
                "Android appID: {1} \n\n\n",
                max.SdkKey,
                max.AdMobAndroidAppId);

            reportContent += adjsutReport;
        }

        if (adManagerObject != null)
        {
            string adReport = string.Format("               -------------AD MAX ID--------------\n" +
                "Banner ID: {0} \n" +
                "Inter ID: {1} \n" +
                "Reward ID: {2} \n" +
                "AppOpen ID: {3} \n",
                adManagerObject.BannerAdUnitID,
                adManagerObject.InterstitialAdUnitID,
                adManagerObject.RewardedAdUnitID,
                adManagerObject.OpenAdUnitID);

#if NATIVE_AD
            adReport += "Native ID: "+adManagerObject.NativeAdID +"\n\n\n";
#endif

            reportContent += adReport;
        }


        FacebookSettings facebook = null;
        string[] facebookSetting = UnityEditor.AssetDatabase.FindAssets("t:FacebookSettings");
        if (facebookSetting.Length != 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(facebookSetting[0]);
            facebook = UnityEditor.AssetDatabase.LoadAssetAtPath<FacebookSettings>(path);
        }
        else
        {
            Debug.LogError("Can not find MaxSdkSetting!");
        }

        if (facebook != null)
        {
            var appIds = facebook.GetType().GetProperty("AppIds");
            string fbAppID = "";
            if (appIds != null)
            {
                object facebookAppIDProp = null;
                facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
                fbAppID = ((List<string>)facebookAppIDProp)[0];
                
            }
            else
                Debug.LogError("Can not find FB app ID field!");


            var clientToken = facebook.GetType().GetProperty("ClientTokens");
            string fbClientToken = "";
            if (clientToken != null)
            {
                object facebookClientTokenProps = null;
                facebookClientTokenProps = clientToken.GetValue(facebookClientTokenProps, null);
                fbClientToken = ((List<string>)facebookClientTokenProps)[0];
            }
            else
                Debug.LogError("Can not find FB client token field!");

            string facebookReport = string.Format("               -------------FACEBOOK--------------\n" +
                "FB AppID: {0} \n" +
                "FB ClientToken: {1} \n\n\n",
                fbAppID,fbClientToken);

            reportContent += facebookReport;
        }

        string pathReport = report.summary.outputPath.Replace(".apk", "").Replace(".aab", "") + "-buildHuynnReport";
        FileStream stream = new FileStream(pathReport, FileMode.OpenOrCreate);
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(reportContent);

            writer.Flush();
            writer.Close();
        }
    }
}
