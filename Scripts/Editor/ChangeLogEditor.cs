using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

public class ChangeLogEditor : EditorWindow
{
    static EditorWindow wnd;
    public static string ChangeLogText = "";

    Vector2 scroll;

    [MenuItem("3rdLib/Git/Change Log Editor")]
    public static void InitWindowEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        wnd = GetWindow<ChangeLogEditor>();
        wnd.titleContent = new GUIContent("Change log editor");
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        ChangeLogText = EditorGUILayout.TextArea(ChangeLogText, GUILayout.Height(position.height));

        EditorGUILayout.EndScrollView();


    }
}
