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


class BuildProcess : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!PlayerSettings.applicationIdentifier.StartsWith("com."))
        {
            EditorUtility.DisplayDialog("Attention Pleas?",
               "Your package name not start with \"com.\". This can make you can't build your project, consider change it ASAP!!", "Ok");
        }
        if (!CheckFirebaseJson())
        {
            
            return;
        }

        FixGoogleXml();

    }

    [MenuItem("3rdLib/Check google-services.xml")]
    public static void FixGoogleXml()
    {

        XmlDocument xmlDoc = new XmlDocument();
        string googleServiceXmlPath = CheckFirebaseXml();
        if (string.IsNullOrEmpty(googleServiceXmlPath))
        {
            EditorUtility.DisplayDialog("Oop, something wrong?",
                "Missing google-service.xml. All firebase services may not work?", "Ok!");
            return;
        }

        if (!CheckFirebaseJson())
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
                        "</resources>";

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



    }

    [MenuItem("3rdLib/Check google-services.json")]
    public static bool CheckFirebaseJson()
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

}
#endif