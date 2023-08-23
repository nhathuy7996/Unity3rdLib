using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using com.adjust.sdk;
using Facebook.Unity.Settings;
using GoogleMobileAds.Editor;
using DVAH;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Diagnostics;
using static UnityEditor.PlayerSettings;
using System.Threading.Tasks;

public class MenuEditor
{
    [MenuItem("3rdLib/Play")]
    public static void PlayGame()
    {
        if (EditorBuildSettings.scenes.Count() == 0)
        {
            EditorUtility.DisplayDialog("Error", "You must start at add a scene to build setting!", "Got it!");
            return;
        }

        if (EditorUtility.DisplayDialog("Attention", "Do you want clear all playerPrefb before run?", "Yes", "No"))
            PlayerPrefs.DeleteAll();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        UnityEditor.EditorApplication.isPlaying = true;
    }

    [MenuItem("3rdLib/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        if (!EditorUtility.DisplayDialog("Attention", "Clear all player prefb!", "Ok!", "Cancel"))
            return;

        PlayerPrefs.DeleteAll();
    }

    [MenuItem("3rdLib/Git/Push production")]
    public static void MenuPushGit()
    {
        if (!EditorUtility.DisplayDialog("Attention Please!", "It will commit all change then push to branch production_hnn on remote. " +
           "You can check and merge to Production later!", "Got it!", "Stop"))
        {
            return;
        }

        PushGit(null);
    }
     
    public static void PushBackUp(string nameAPK)
    {
        string cmdPath = FindCommand();
        if (string.IsNullOrEmpty(cmdPath))
            return;

        string cmdLines = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            cmdLines = "#!/bin/sh\n\n" +
            "cd ../../\n" +
            "cd " + Application.dataPath + "\n" +
            "git add -A\n" +
            $"git commit -m \"build_{nameAPK} \"\n" +
            $"git push origin HEAD:production_{PlayerSettings.bundleVersion} -f";
        }
        else
        {

            cmdLines = "/C git add -A&" +
            $"git commit -m \"build_{nameAPK} \"&" +
            $"git push origin HEAD:production_{PlayerSettings.bundleVersion} -f";
        }

        string terminal = @"cmd.exe";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            FileStream stream = new FileStream(cmdPath, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(cmdLines);

                writer.Flush();
                writer.Close();
            }

            System.Diagnostics.Process uploadProc = new System.Diagnostics.Process();
            uploadProc.StartInfo.FileName = terminal;
            uploadProc.StartInfo.Arguments = cmdPath;
            uploadProc.StartInfo.UseShellExecute = false;
            uploadProc.StartInfo.CreateNoWindow = false;
            uploadProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            uploadProc.Start();
        }
        else
        {
            terminal = @"C:\Windows\system32\cmd.exe";
            Process.Start(terminal, cmdLines);
        }


    }

    [MenuItem("3rdLib/Git/Update Lib/production*")]
    public static void UpdateLib()
    {
        if (!EditorUtility.DisplayDialog("Attention Please!", "Before update, all change will be commit (not push yet)!", "Got it!", "Stop"))
        {
            return;
        }

        UpdateLibCommand("production");
    }

    [MenuItem("3rdLib/Git/Update Lib/develop")]
    public static void UpdateLibDev()
    {
        if (!EditorUtility.DisplayDialog("Attention Please!", "Before update, all change will be commit (not push yet)!", "Got it!", "Stop"))
        {
            return;
        }

        UpdateLibCommand("develop");
    }

    public static void UpdateLibCommand(string branch)
    {
        string cmdPath = FindCommand();
        if (string.IsNullOrEmpty(cmdPath))
            return;

        var directory = new DirectoryInfo(Application.dataPath);
        while (directory.GetDirectories(".git").Length == 0)
        {
            directory = directory.Parent;

            if (directory == null)
            {
                throw new DirectoryNotFoundException("We went all the way up to the system root directory and didn't find any \".git\" directory!");
            }
        }
        var repositoryPath = directory.FullName;

        string cmdLines = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            cmdLines = "#!/bin/sh\n\n" +
            "cd ../../\n" +
            "cd " + Application.dataPath + "\n" +
            "cd $(git rev-parse --show-cdup)\n" +
            "git add -A\n" +
            "git commit -m \"prepare update lib!!!!!!\"\n" +
            "git subtree pull --prefix " + Application.dataPath.Replace(repositoryPath + "/", "") + "/DVAH/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git " + branch + " --squash";
        }
        else
        {
            cmdLines = "/K cd " + repositoryPath + "&" +
            "git add -A&" +
            "git commit -m \"prepare update lib!!!!!!\"&" +
            "git subtree pull --prefix " + Application.dataPath.Replace(repositoryPath.Replace("\\", "/") + "/", "") + "/DVAH/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git " + branch + " --squash";
        }

        string terminal = @"cmd.exe";
        System.Diagnostics.Process updateProc = new System.Diagnostics.Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            FileStream stream = new FileStream(cmdPath, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(cmdLines);

                writer.Flush();
                writer.Close();
            }


            updateProc.StartInfo.FileName = terminal;
            updateProc.StartInfo.Arguments = cmdPath;
            updateProc.StartInfo.UseShellExecute = false;
            updateProc.StartInfo.CreateNoWindow = false;
            updateProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
            updateProc.EnableRaisingEvents = true;

            updateProc.Exited += UploadProc_Exited;
            updateProc.Start();
        }
        else
        {
            terminal = @"C:\Windows\system32\cmd.exe";
            updateProc.StartInfo.FileName = terminal;
            updateProc.StartInfo.Arguments = cmdLines;
            updateProc.EnableRaisingEvents = true;

            updateProc.Exited += UploadProc_Exited;
            updateProc.Start();

        }

    }

    public static void UploadProc_Exited(object sender, EventArgs e)
    {
        Debug.LogError("Process done!");
    }

    public static void PushGit(BuildReport _report)
    {
        string cmdPath = FindCommand();
        if (string.IsNullOrEmpty(cmdPath))
            return;

        string cmdLines = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            cmdLines = "#!/bin/sh\n\n" +
            "cd ../../\n" +
            "cd " + Application.dataPath + "\n" +
            "git add -A\n" +
            $"git commit -m \"release_{ PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode} \"\n" +
            "git push origin HEAD:production_doNotCreateBranchFromHere -f";
        }
        else
        {
            
            cmdLines = "/C git add -A&" +
            $"git commit -m \"release_{ PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode} \"&" +
            "git push origin HEAD:production_doNotCreateBranchFromHere -f";
        }

        string terminal = @"cmd.exe";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            FileStream stream = new FileStream(cmdPath, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(cmdLines);

                writer.Flush();
                writer.Close();
            }

            System.Diagnostics.Process uploadProc = new System.Diagnostics.Process();
            uploadProc.StartInfo.FileName = terminal;
            uploadProc.StartInfo.Arguments = cmdPath;
            uploadProc.StartInfo.UseShellExecute = false;
            uploadProc.StartInfo.CreateNoWindow = false;
            uploadProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            uploadProc.Start();
        }
        else
        {
            terminal = @"C:\Windows\system32\cmd.exe";
            Process.Start(terminal, cmdLines);
        }


    }

    public static void FixAndroidManifestFB()
    {

        string[] facebookSetting = UnityEditor.AssetDatabase.FindAssets("t:FacebookSettings");
        if (facebookSetting.Length == 0)
        {
            return;
        }

        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(facebookSetting[0]);
        FacebookSettings facebook = UnityEditor.AssetDatabase.LoadAssetAtPath<FacebookSettings>(path);

        var appIds = facebook.GetType().GetProperty("AppIds");
        object facebookAppIDProp = null;
        facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
        string fbAppID = ((List<string>)facebookAppIDProp)[0];

        if (string.IsNullOrEmpty(fbAppID))
        {
            fbAppID = PlayerSettings.applicationIdentifier;
        }


        string[] files = Directory.GetFiles(Application.dataPath, "AndroidManifest.xml", SearchOption.AllDirectories).ToArray();
        if (files.Length == 0)
        {
            return;
        }

        foreach (string filePath in files)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);


            foreach (XmlNode e in xmlDoc.GetElementsByTagName("meta-data"))
            {
                if (!e.Attributes["android:name"].Value.Equals("com.facebook.sdk.ApplicationId"))
                {
                    continue;
                }

                if (e.Attributes["android:value"].Value.Equals("fb" + fbAppID))
                {
                    break;
                }

                e.Attributes["android:value"].Value = ("fb" + fbAppID);
            }


            foreach (XmlNode e in xmlDoc.GetElementsByTagName("provider"))
            {

                if (e.Attributes["android:authorities"].Value.Equals("com.facebook.app.FacebookContentProvider" + fbAppID))
                {
                    continue;
                }

                e.Attributes["android:authorities"].Value = ("com.facebook.app.FacebookContentProvider" + fbAppID);
            }

            FileStream stream = new FileStream(filePath, FileMode.Create);

            xmlDoc.Save(stream);
        }
        
    }

    public static void FixGoogleXml(bool isShowOk = true)
    {

        XmlDocument xmlDoc = new XmlDocument();
        string googleServiceXmlPath = CheckFirebaseXml();
        if (string.IsNullOrEmpty(googleServiceXmlPath))
        {
            if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                "Missing google-service.xml. All firebase services may not work?", "Continue", "Stop"))
            {
                StopBuildWithMessage("Missing google-service.xml");
            }
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

        if (isShowOk)
            EditorUtility.DisplayDialog("Hi, your captain here!",
               "google-services.xml: Oke oke", "Ok!");
    }


    public static bool CheckFirebaseJson(bool isShowOk = true)
    {

        string[] files = Directory.GetFiles(Application.dataPath, "*.json*", SearchOption.AllDirectories)
                            .Where(f => f.EndsWith("google-services.json")).ToArray();
        if (files.Length == 0)
        {
            UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project doesnt contain google-services.json. Firebase may not work!!!!!<==");
            if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                "Missing google-service.js. All firebase services may not work?", "Continue", "Stop"))
            {
                StopBuildWithMessage("Missing google-service.js");
            }

            return false;
        }

        if (files.Length > 1)
        {
            UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project contain more than one file google-services.json. Firebase may not work wrong!!!!!<==");
            if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                "Too many google-service.js. All firebase services may not work?", "Continue", "Stop"))
            {
                StopBuildWithMessage("Too many google-service.js");
            }
            return false;
        }

        if (isShowOk)
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

        UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project error google-services.xml. Firebase may not work wrong!!!!!<==");
        return null;
    }


    public static string Report(BuildReport report)
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

        if (!string.IsNullOrWhiteSpace(ChangeLogEditor.ChangeLogText) && !string.IsNullOrEmpty(ChangeLogEditor.ChangeLogText))
        {
            reportContent += string.Format("               -------------CHANGE LOG--------------\n");
            reportContent += ChangeLogEditor.ChangeLogText + "\n\n\n";
        }



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
        AdMHighFather adManagerObject = GameObject.FindObjectOfType<DVAH.AdMHighFather>();

        if (ggSetting.Length != 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(ggSetting[0]);
            gg = UnityEditor.AssetDatabase.LoadAssetAtPath<GoogleMobileAdsSettings>(path);
        }
        else
        {
            EditorGUILayout.LabelField(CONSTANT.Prefix + $":Can not find GoogleMobileAdsSettings!");
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
            UnityEngine.Debug.LogError(CONSTANT.Prefix + $":Can not find MaxSdkSetting!");
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
                "Reward ID: {2} \n",
                adManagerObject.BannerAdUnitID,
                adManagerObject.InterstitialAdUnitID,
                adManagerObject.RewardedAdUnitID);

            if (adManagerObject.OpenAdUnitIDs.Count != 0)
            {
                adReport += "AppOpen AD ID:\n";
                for (int i = 0; i < adManagerObject.OpenAdUnitIDs.Count; i++)
                {
                    adReport += string.Format("          {0}: {1}", (i + 1), adManagerObject.OpenAdUnitIDs[i]) + "\n";
                }

            }

#if NATIVE_AD
            if (adManagerObject.NativeAdID.Count != 0)
            {
                adReport += "Native AD ID:\n";
                for (int i = 0; i < adManagerObject.NativeAdID.Count; i++)
                {
                    adReport += string.Format("          {0}: {1}", (i + 1), adManagerObject.NativeAdID[i]) + "\n";
                }
                adReport += "\n";
            }

#endif
            adReport += "\n\n";
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
            UnityEngine.Debug.LogError(CONSTANT.Prefix + $":Can not find MaxSdkSetting!");
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
                UnityEngine.Debug.LogError(CONSTANT.Prefix + $":Can not find FB app ID field!");


            var clientToken = facebook.GetType().GetProperty("ClientTokens");
            string fbClientToken = "";
            if (clientToken != null)
            {
                object facebookClientTokenProps = null;
                facebookClientTokenProps = clientToken.GetValue(facebookClientTokenProps, null);
                fbClientToken = ((List<string>)facebookClientTokenProps)[0];
            }
            else
                UnityEngine.Debug.LogError(CONSTANT.Prefix + $":Can not find FB client token field!");

            string facebookReport = string.Format("               -------------FACEBOOK--------------\n" +
                "FB AppID: {0} \n" +
                "FB ClientToken: {1} \n\n\n",
                fbAppID, fbClientToken);

            reportContent += facebookReport;

        }

        return reportContent;
    }

    public static void StopBuildWithMessage(string message)
    {
        string prefix = CONSTANT.Prefix + $"";
#if UNITY_2017_1_OR_NEWER
        throw new BuildFailedException(prefix + message);
#else
        throw new OperationCanceledException(prefix + message);
#endif
    }


    static string FindCommand()
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*push_git_cmd.sh", SearchOption.AllDirectories).ToArray();
        if (files.Length == 1)
        {
            return files[0];
        }

        UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project dont have require .sh file. Can't auto push git!!!!!<==");
        return null;
    }

}


