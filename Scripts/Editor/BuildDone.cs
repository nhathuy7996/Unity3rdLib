using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using System;
using com.adjust.sdk;
using GoogleMobileAds.Editor;
using DVAH;
using System.IO;
using Facebook.Unity.Settings;
using UnityEngine.Networking.Types;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

public class BuildDone : IPostprocessBuildWithReport
{
    static BuildReport _report;
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
        MenuEditor.PushBackUp(report.summary.outputPath);
        if (EditorUserBuildSettings.buildAppBundle && EditorUtility.DisplayDialog("Push to production!",
               "Your .aab build succed! Would u like to push to branch production?", "Ok", "No"))
        {

            MenuEditor.PushGit(report);
            
        }
    }

    

}
