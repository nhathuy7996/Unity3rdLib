#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using System.Xml;
using SimpleJSON;
using System;
using System.Reflection;
using com.adjust.sdk;
using Facebook.Unity.Settings;
using GoogleMobileAds.Editor;
using HuynnLib;
using System.Collections.Generic;

class BuildProcess : IPreprocessBuildWithReport
{
    public static string reportContent;
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {

        if (!PlayerSettings.applicationIdentifier.StartsWith("com."))
        {
            EditorUtility.DisplayDialog("Attention Pleas?",
               "Your package name not start with \"com.\". This can make you can't build your project, consider change it ASAP!!", "Ok");
        }

        if (PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
        {
            EditorUtility.DisplayDialog("Attention Pleas?",
               "Your package name is not in format 'com.X.Y' . This can make you can't build your project, consider change it ASAP!!", "Ok");
        }

        if (!CheckFirebaseJson(false))
        {
            
            return;
        }

        FixGoogleXml(false);

        Report(report);
    }

    [MenuItem("3rdLib/Check google-services.xml")]
    public static void FixGoogleXml(bool isShowOk = true)
    {

        XmlDocument xmlDoc = new XmlDocument();
        string googleServiceXmlPath = CheckFirebaseXml();
        if (string.IsNullOrEmpty(googleServiceXmlPath))
        {
            EditorUtility.DisplayDialog("Oop, something wrong?",
                "Missing google-service.xml. All firebase services may not work?", "Ok!");
            return;
        }

        if (!CheckFirebaseJson(false))
            return;

            using (StreamReader reader = new StreamReader(Directory.GetFiles(Application.dataPath, "*google-services.json", SearchOption.AllDirectories)[0]))
        {
            var dataParsed = SimpleJSON.JSON.Parse(reader.ReadToEnd());

            string errors = "";

            xmlDoc.Load(googleServiceXmlPath);
            var root = xmlDoc.GetElementsByTagName("string");

            var project_info = dataParsed["project_info"];
            var client = dataParsed["client"][0];
            var apiKey = client["api_key"][0];
            var default_web_client_id = client["services"]["appinvite_service"]["other_platform_oauth_client"][0]["client_id"];

            foreach (XmlNode e in root)
            {
                if (e.Attributes["name"].Value == "gcm_defaultSenderId")
                {
                    if (e.InnerText != project_info["project_number"])
                    {
                        errors += "gcm_defaultSenderId wrong!   \n";
                    }
                }

                if (e.Attributes["name"].Value == "google_storage_bucket")
                {
                    if (e.InnerText != project_info["storage_bucket"])
                    {
                        errors += "google_storage_bucket wrong! \n";
                    }
                }

                if (e.Attributes["name"].Value == "project_id")
                {
                    if (e.InnerText != project_info["project_id"])
                    {
                        errors += "project_id wrong!  \n";
                    }
                }

                if (e.Attributes["name"].Value == "google_api_key")
                {
                    if (e.InnerText != apiKey["current_key"])
                    {
                        errors += "google_api_key wrong! \n";
                    }
                }

                if (e.Attributes["name"].Value == "google_crash_reporting_api_key")
                {
                    if (e.InnerText != apiKey["current_key"])
                    {
                        errors += "google_crash_reporting_api_key wrong! \n";
                    }
                }

                if (e.Attributes["name"].Value == "google_app_id")
                {
                    if (e.InnerText != client["client_info"]["mobilesdk_app_id"])
                    {
                        errors += "default_web_client_id wrong!  \n";
                    }
                }

                if (e.Attributes["name"].Value == "default_web_client_id")
                {
                    if (e.InnerText != default_web_client_id)
                    {
                        errors += "default_web_client_id wrong! \n";
                    }
                }
            }

            if (!string.IsNullOrEmpty(errors))
            {
                if (EditorUtility.DisplayDialog("Oop, something wrong?",
                    "data different between google-service.xml and google-services.json: \n" +
                    errors +
                    " All firebase services may not work, auto fix it?", "Ok!", "Fuck off"))
                {
                    string data = "<?xml version='1.0' encoding='utf-8'?>\n" +
                        "<resources xmlns:tools=\"http://schemas.android.com/tools\" tools:keep=\"@string/gcm_defaultSenderId," +
                        "@string/google_storage_bucket," +
                        "@string/project_id,@string/google_api_key," +
                        "@string/google_crash_reporting_api_key,@string/google_app_id," +
                        "@string/default_web_client_id\">\n  " +
                        "<string name=\"gcm_defaultSenderId\" translatable=\"false\">" + project_info["project_number"] + "</string>\n  " +
                        "<string name=\"google_storage_bucket\" translatable=\"false\">" + project_info["storage_bucket"] + "</string>\n  " +
                        "<string name=\"project_id\" translatable=\"false\">" + project_info["project_id"] + "</string>\n  " +
                        "<string name=\"google_api_key\" translatable=\"false\">" + client["api_key"][0]["current_key"] + "</string>\n  " +
                        "<string name=\"google_crash_reporting_api_key\" translatable=\"false\">" + client["api_key"][0]["current_key"] + "</string>\n  " +
                        "<string name=\"google_app_id\" translatable=\"false\">" + client["client_info"]["mobilesdk_app_id"] + "</string>\n  " +
                        "<string name=\"default_web_client_id\" translatable=\"false\">" + default_web_client_id + "</string>\n" +
                        "</resources>\n";

                    FileStream stream = new FileStream(CheckFirebaseXml(), FileMode.Create);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(data);

                        writer.Flush();
                        writer.Close();
                    }
                }

            }

            reader.Close();

        }

        if(isShowOk)
            EditorUtility.DisplayDialog("Hi, your captain here!",
               "google-services.xml: Oke oke", "Ok!");
    }

    [MenuItem("3rdLib/Check google-services.json")]
    public static bool CheckFirebaseJson(bool isShowOk = true)
    {

        string[] files = Directory.GetFiles(Application.dataPath, "*.json*", SearchOption.AllDirectories)
                            .Where(f => f.EndsWith("google-services.json")).ToArray();
        if (files.Length == 0)
        {
            Debug.LogError("==>Project doesnt contain google-services.json. Firebase may not work!!!!!<==");
            EditorUtility.DisplayDialog("Oop, something wrong?",
                "Missing google-service.js. All firebase services may not work?", "Ok!");

            return false;
        }

        if (files.Length > 1)
        {
            Debug.LogError("==>Project contain more than one file google-services.json. Firebase may not work wrong!!!!!<==");
            EditorUtility.DisplayDialog("Oop, something wrong?",
                "Too many google-service.js. All firebase services may not work?", "Ok!");

            return false;
        }

        if(isShowOk)
            EditorUtility.DisplayDialog("Ok, Nothing wrong!",
               "You file google-services.json exist and seem to be oke!", "Close");

        return true;
    }

    public static string CheckFirebaseXml()
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*google-services.xml", SearchOption.AllDirectories).ToArray();
        if (files.Length == 1)
        {
            return files[0];
        }

        Debug.LogError("==>Project error google-services.xml. Firebase may not work wrong!!!!!<==");
        return null;
    }


    public static void Report(BuildReport report)
    {

        reportContent = string.Format("Time: " + DateTime.Now + "\n" +
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
            string adReport = string.Format("               -------------AD ID--------------\n" +
                "Banner ID: {0} \n" +
                "Inter ID: {1} \n" +
                "Reward ID: {2} \n\n" ,
                adManagerObject.BannerAdUnitID,
                adManagerObject.InterstitialAdUnitID,
                adManagerObject.RewardedAdUnitID );

            for (int i = 0; i < adManagerObject.OpenAdUnitIDs.Count; i++)
            {
                adReport += string.Format("AppOpen AD ID {0}: {1}", (i + 1), adManagerObject.OpenAdUnitIDs[i]) + "\n\n\n";
            }
#if NATIVE_AD
            for (int i = 0; i< adManagerObject.NativeAdID.Count; i++)
            {
                adReport += string.Format("Native ID {0}: {1}",(i+1), adManagerObject.NativeAdID[i]) + "\n\n\n";
            }
            
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
                fbAppID, fbClientToken);

            reportContent += facebookReport;
        }

       
    }

}
#endif