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

        if (EditorUserBuildSettings.buildAppBundle && EditorUtility.DisplayDialog("Push to production!",
               "Your .aab build succed! Would u like to push to branch production?", "Ok", "No"))
        {
            string cmdPath = FindCommand();
            if (string.IsNullOrEmpty(cmdPath))
                return;

            string cmdLines = "#!/bin/sh\n\n" +
                "cd ../../\n" +
                "cd " + Application.dataPath + "\n" +
                "git add -A\n" +
                "git commit -m \"release " + report.summary.outputPath + "_" + PlayerSettings.bundleVersion + "\"\n" +
                "git push origin HEAD:production_hnn -f";

            stream = new FileStream(cmdPath, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(cmdLines);

                writer.Flush();
                writer.Close();
            }

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
