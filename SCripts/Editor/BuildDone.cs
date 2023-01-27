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
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

public class BuildDone : IPostprocessBuildWithReport
{
    static BuildReport _report;
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        _report = report;
        string pathReport = report.summary.outputPath.Replace(".apk", "").Replace(".aab", "") + "-buildHuynnReport";
        FileStream stream = new FileStream(pathReport, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(BuildProcess.reportContent);

            writer.Flush();
            writer.Close();
        }

        if (EditorUserBuildSettings.buildAppBundle && EditorUtility.DisplayDialog("Push to production!",
               "Your .aab build succed! Would u like to push to branch production?", "Ok", "No"))
        {

            PushGit();
        }
    }

    [MenuItem("3rdLib/Push production",priority =1)]
    static void PushGit()
    {
        string cmdPath = FindCommand();
        if (string.IsNullOrEmpty(cmdPath))
            return;

        string cmdLines = "#!/bin/sh\n\n" +
            "cd ../../\n" +
            "cd " + Application.dataPath + "\n" +
            "git add -A\n" +
            "git commit -m \"release " + _report.summary.outputPath + "_" + PlayerSettings.bundleVersion + "\"\n" +
        "git push origin HEAD:production_hnn -f";

        FileStream stream = new FileStream(cmdPath, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(cmdLines);

            writer.Flush();
            writer.Close();
        }

        string terminal = @"cmd.exe";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
        }

        else
        {
            terminal = @"cmd.exe";
        }

        System.Diagnostics.Process uploadProc = new System.Diagnostics.Process
        {
            StartInfo = {
                FileName = terminal,
                    Arguments = cmdPath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                }
        };

        uploadProc.Start();
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


}
