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
            if(!EditorUtility.DisplayDialog("Attention Pleas?",
               "Your package name not start with \"com.\". This can make your project not working correctly!", "Continue Build!","Stop build!"))
            {
                MenuEditor.StopBuildWithMessage("package name not start with \"com.\"!"); 
            }

        }

        if (PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
        {
            if(!EditorUtility.DisplayDialog("Attention Pleas?",
               "Your package name is not in format 'com.X.Y' . This can make your project not working correctly!", "Continue Build!", "Stop build!"))
            {
                MenuEditor.StopBuildWithMessage("package name not correct format!");
            }
        }

        if (!MenuEditor.CheckFirebaseJson(false))
        {
            
            return;
        }

        MenuEditor.FixAndroidManifestFB();

        MenuEditor.FixGoogleXml(false);

        MenuEditor.Report(report);
    }

    
}
#endif