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
using System.Diagnostics;
using System.Linq;
using System.Text;

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

        if (!EditorUserBuildSettings.buildAppBundle)
        {
            string cmdPath = FindCommand();
            if (string.IsNullOrEmpty(cmdPath))
                return;
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmdPath,
                Arguments = string.Format("{0}_{1}/{2}_{3}:{4}",
                report.summary.outputPath, DateTime.Now.Date, DateTime.Now.Month,DateTime.Now.Hour,DateTime.Now.Minute)
            };

            startInfo.UseShellExecute = true;
            Process proc = new Process()
            {
                StartInfo = startInfo,
            };
            
            proc.Start();

        }
    }

    static string FindCommand()
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*push_git_cmd.sh", SearchOption.AllDirectories).ToArray();
        if (files.Length == 1)
        {
            return files[0];
        }

        Debug.LogError("==>Project dont have require .sh file. Can't auto push git!!!!!<==");
        return null;
    }

    [MenuItem("3rdLib/Test")]
    static void TestRunbash()
    {
        if (!EditorUserBuildSettings.buildAppBundle)
        {
            string cmdPath = FindCommand();
            if (string.IsNullOrEmpty(cmdPath))
                return;
 
            System.Diagnostics.Process uploadProc = new System.Diagnostics.Process
            {
                StartInfo = {
                    FileName = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal",
                    Arguments = cmdPath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                }
            };

            uploadProc.Start();
        }
    }

    
}
