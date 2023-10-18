using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor; 

using DVAH;
using System.IO;

#if UNITY_ANDROID
using Facebook.Unity.Settings;
using GoogleMobileAds.Editor;
#endif

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

        if (EditorPrefs.GetBool("NEW_BRANCH",false) && PlayerPrefs.GetString("Version") != PlayerSettings.bundleVersion) {
            EditorPrefs.SetString("Version", PlayerSettings.bundleVersion);
            MenuEditor.PushBackUp(report.summary.outputPath);
        }
        if (EditorUserBuildSettings.buildAppBundle && EditorUtility.DisplayDialog("Push to production!",
               "Your .aab build succed! Would u like to push to branch production?", "Ok", "No"))
        {

            MenuEditor.PushGit(report);
            
        }
    }

    

}
