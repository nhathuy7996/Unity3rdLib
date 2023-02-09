using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DVAH;
using System.Xml;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(DVAH3rdLib))]
public class LibEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DVAH3rdLib myScript = (DVAH3rdLib)target;
        GUILayout.Space(10);
      
        if (GUILayout.Button("Assign SubLib"))
        {
            myScript.GetSubLib();
            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
        }

        myScript.CheckFirebaseJS();

        
    }




}
