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
        string pathReport = report.summary.outputPath.Replace(".apk", "").Replace(".aab", "") + "-buildHuynnReport";
        FileStream stream = new FileStream(pathReport, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(BuildProcess.reportContent);

            writer.Flush();
            writer.Close();
        }
    }

    
}
