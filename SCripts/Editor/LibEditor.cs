using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HuynnLib;
using System.Xml;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(Huynn3rdLib))]
public class LibEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Huynn3rdLib myScript = (Huynn3rdLib)target;
        GUILayout.Space(10);
      
        if (GUILayout.Button("Assign SubLib"))
        {
            myScript.GetSubLib();
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        }


        //if (!myScript.CheckFirebaseJS())
        //{

        //while (reader.Read())
        //{
        //    // Do some work here on the data.
        //    Console.WriteLine(reader.Name);
        //}

        //var style = GUI.skin.GetStyle("label");
        //style.fontSize = 20;
        //style.alignment = TextAnchor.MiddleCenter;
        //GUILayout.Label("Google-Service.js", style);

        //style.fontSize = 10;
        //style.alignment = TextAnchor.MiddleLeft;
        //GUILayout.Label("Google-Service.js", style);

        //GUILayout.Label("Google-Service.xml", style);
        //}

        //myScript.CheckFirebaseXml();
    }




}
