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
#if UNITY_ANDROID
using Facebook.Unity.Settings;
using GoogleMobileAds.Editor;
#endif
using DVAH;
using System.Collections.Generic;

class BuildProcess : IPreprocessBuildWithReport
{
    public static string reportContent;
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        ProjectInfoEditor.InitWindowEditor();
        if (!PlayerSettings.applicationIdentifier.StartsWith("com."))
        {
            if(!EditorUtility.DisplayDialog("Attention Please?",
               "Your package name not start with \"com.\". This can make your project not working correctly!", "Continue Build!","Stop build!"))
            {
                MenuEditor.StopBuildWithMessage("package name not start with \"com.\"!"); 
            }

        }

        if (PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
        {
            if(!EditorUtility.DisplayDialog("Attention Please?",
               "Your package name is not in format 'com.X.Y' . This can make your project not working correctly!", "Continue Build!", "Stop build!"))
            {
                MenuEditor.StopBuildWithMessage("package name not correct format!");
            }
        }

        if (EditorUserBuildSettings.buildAppBundle && !PlayerSettings.Android.useCustomKeystore)
        {
            if(!EditorUtility.DisplayDialog("Attention Please?",
               "Are you sure wanna build an .aab file with DEBUG keystore? It's definitely can't upload to google play!", "Continue Build!", "Stop build!"))
            {
                MenuEditor.StopBuildWithMessage("build aab with debug keystore!");
            }
        }

        DVAH_Data DVAH_Data;
        string[] DVAH_Datas = UnityEditor.AssetDatabase.FindAssets("t:DVAH_Data");
        if (DVAH_Datas.Length != 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(DVAH_Datas[0]);
            DVAH_Data = UnityEditor.AssetDatabase.LoadAssetAtPath<DVAH_Data>(path);

        }
        else
        {
            MenuEditor.StopBuildWithMessage("Cannot find DVAH_Data!");
            return;
        }

        if (EditorUserBuildSettings.buildAppBundle && DVAH_Data.AdjustMode == ADJUST_MODE.Sandbox)
        {
            if (!EditorUtility.DisplayDialog("Attention Please?",
               "Are you sure wanna build an .aab file with adjut on SANDBOX mode?", "Continue Build!", "Stop build!"))
            {
                MenuEditor.StopBuildWithMessage("build aab with adjut on SANDBOX mode!");
            }
        }


        if (!MenuEditor.CheckFirebaseJson(false))
        { 
            return;
        }

#if UNITY_ANDROID
        MenuEditor.FixAndroidManifestFB();
#endif
        MenuEditor.FixGoogleXml(false);

        reportContent = MenuEditor.Report(report);

        try
        {
            DVAH_Data.Report = reportContent;
        }
        catch
        {
            
        }


    }


}
#endif