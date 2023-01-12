using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using com.adjust.sdk;
using GoogleMobileAds.Editor;
using System;
using HuynnLib;

class ProjectInfoEditor : EditorWindow
{

    static string AdjustToken;

  
    // Add menu named "My Window" to the Window menu
    [MenuItem("3rdLib/Checklist APERO")]
    public static void InitWindowEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<ProjectInfoEditor>();
        wnd.titleContent = new GUIContent("Huynn 3rdLib - APERO version!"); 

       
    }

    void OnGUI()
    {
        Adjust adjustGameObject = GameObject.FindObjectOfType<Adjust>();
        if (adjustGameObject)
        {
            AdjustToken = EditorGUILayout.TextField("Adjust Token", AdjustToken);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Adjust Mode");
            string adjustEvironment = adjustGameObject.environment.ToString();
            bool dropDownSelected = EditorGUILayout.DropdownButton(content: new GUIContent(adjustEvironment),FocusType.Passive);
            EditorGUILayout.EndHorizontal();

            if (dropDownSelected)
            {
             
                GenericMenu menu = new GenericMenu();
                 
                AddMenuItemForColor(menu, AdjustEnvironment.Production.ToString(), AdjustEnvironment.Production,
                    adjustGameObject.environment == AdjustEnvironment.Production);
                AddMenuItemForColor(menu, AdjustEnvironment.Sandbox.ToString(), AdjustEnvironment.Sandbox,
                     adjustGameObject.environment == AdjustEnvironment.Sandbox);
                menu.ShowAsContext();
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Events Token:");
   

            FireBaseManager fireBaseManager = GameObject.FindObjectOfType<FireBaseManager>();
            if (fireBaseManager)
            {
                fireBaseManager._adValue = EditorGUILayout.TextField("ad_value", fireBaseManager._adValue);
                fireBaseManager._adjsutLevelAchived = EditorGUILayout.TextField("level_achived", fireBaseManager._adjsutLevelAchived);
            }


            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ExampleData");
            if (guids.Length != 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                GoogleMobileAdsSettingsEditor gg = UnityEditor.AssetDatabase.LoadAssetAtPath<GoogleMobileAdsSettingsEditor>(path);


            }
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Save Data"))
        {
            OnClickSave();
            GUIUtility.ExitGUI();
        }
    }

    void AddMenuItemForColor(GenericMenu menu, string menuPath, AdjustEnvironment value, bool isSelected = false)
    {
        // the menu item is marked as selected if it matches the current value of m_Color
        menu.AddItem(new GUIContent(menuPath), isSelected, OnDropBoxAdjustItemClick, value);
    }

    void OnDropBoxAdjustItemClick(object item)
    {
        Adjust adjustGameObject = GameObject.FindObjectOfType<Adjust>();
        adjustGameObject.environment = (AdjustEnvironment)item;
    }

    void OnClickSave()
    {
        Adjust adjustGameObject = GameObject.FindObjectOfType<Adjust>();
        if (adjustGameObject)
        {
            adjustGameObject.appToken = AdjustToken;
        }

        Debug.LogError(AdjustToken);
        Close();
    }

}