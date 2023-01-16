#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using com.adjust.sdk;
using GoogleMobileAds.Editor;
using System;
using HuynnLib;
using Facebook.Unity.Settings;
using Codice.Client.BaseCommands;
using System.IO;
using NUnit.Framework.Internal;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using System.Collections.Generic;
using System.Linq;

class ProjectInfoEditor : EditorWindow
{

    bool isShowKeyStorePass = false, isShowAliasPass = false;
    Adjust adjustGameObject;


    GoogleMobileAdsSettings gg = null;

    HuynnLib.AdManager adManager = null;
    AppLovinSettings max = null;

    BuildPlayerOptions defaultBuildOptions = new BuildPlayerOptions();
    FireBaseManager fireBaseManager;

    FacebookSettings facebook;

    string fbAppID, fbClientToken,fbKeyStore;
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
        if (!adManager)
            adManager = GameObject.FindObjectOfType<HuynnLib.AdManager>();
        #region EDITOR
        EditorGUILayout.LabelField("Build Version:");

        PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("Version Code", PlayerSettings.Android.bundleVersionCode);

        EditorGUILayout.BeginHorizontal();
        PlayerSettings.companyName = EditorGUILayout.TextField("Company Name", PlayerSettings.companyName);
        PlayerSettings.productName = EditorGUILayout.TextField("Product Name", PlayerSettings.productName);
        EditorGUILayout.EndHorizontal();

        PlayerSettings.bundleVersion = EditorGUILayout.TextField("App Version", PlayerSettings.bundleVersion); 
        string applicationIdentifier = EditorGUILayout.TextField("Package Name","Package name should in form \"com.X.Y\" other can cost a build error!",
            PlayerSettings.applicationIdentifier);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);

        EditorGUILayout.BeginHorizontal();
        PlayerSettings.Android.useCustomKeystore = EditorGUILayout.Toggle("Custom KeyStore", PlayerSettings.Android.useCustomKeystore);
        if (PlayerSettings.Android.useCustomKeystore)
        {
            if (EditorGUILayout.LinkButton("Select"))
            {
                string path = EditorUtility.OpenFilePanel("Select keystore file", "", "keystore");
                if (path.Length != 0)
                {
                    PlayerSettings.Android.keystoreName = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("KeyStore Path:                      "+ PlayerSettings.Android.keystoreName);

            KeyStoreInfo();
        }
        else
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("KeyStore Path:                      Debug keystore!!!");
        }
        
        #endregion

        #region ADJUST

        if (adjustGameObject)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Adjust:");
            adjustGameObject.appToken = EditorGUILayout.TextField("Adjust Token", adjustGameObject.appToken);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Adjust Mode");
            string adjustEvironment = adjustGameObject.environment.ToString();
            bool dropDownSelected = EditorGUILayout.DropdownButton(content: new GUIContent(adjustEvironment), FocusType.Passive);
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
                fireBaseManager.ADValue = EditorGUILayout.TextField("ad_value", fireBaseManager.ADValue);
                fireBaseManager.Level_Achived = EditorGUILayout.TextField("level_achived", fireBaseManager.Level_Achived);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(fireBaseManager);
            PrefabUtility.RecordPrefabInstancePropertyModifications(adjustGameObject);
        }
        else
        {
            adjustGameObject = GameObject.FindObjectOfType<Adjust>();
        }
        #endregion

        #region GOOGLE ADS SETTING
       

        if (gg)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Google:");
            gg.GoogleMobileAdsAndroidAppId = EditorGUILayout.TextField("Android AD ID", gg.GoogleMobileAdsAndroidAppId);
            if (adManager)
            {
                adManager.paid_ad_revenue = EditorGUILayout.TextField("Event Paid AD", adManager.paid_ad_revenue);
                PrefabUtility.RecordPrefabInstancePropertyModifications(adManager);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(gg);
        }
        else
        {
            string[] ggSetting = UnityEditor.AssetDatabase.FindAssets("t:GoogleMobileAdsSettings");
            if (ggSetting.Length != 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(ggSetting[0]);
                gg = UnityEditor.AssetDatabase.LoadAssetAtPath<GoogleMobileAdsSettings>(path);
            }
            else
            {
                EditorGUILayout.LabelField("Can not find GoogleMobileAdsSettings!");
            }
        }
        #endregion

      
        #region APPLOVIN
       
        if (max != null)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("AppLovin:");
            max.SdkKey = EditorGUILayout.TextField("MaxSdk key", max.SdkKey);
            if (adManager)
                adManager.MaxSdkKey = max.SdkKey;
            if (gg != null)
            {
                max.AdMobAndroidAppId = gg.GoogleMobileAdsAndroidAppId;
                EditorGUILayout.LabelField("Android AD ID:                       " + max.AdMobAndroidAppId);
            }
            else
            {
                max.AdMobAndroidAppId = EditorGUILayout.TextField("Android AD ID", max.AdMobAndroidAppId);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(max);
        }
        else
        {
            string[] maxSetting = UnityEditor.AssetDatabase.FindAssets("t:AppLovinSettings");
            if (maxSetting.Length != 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(maxSetting[0]);
                max = UnityEditor.AssetDatabase.LoadAssetAtPath<AppLovinSettings>(path);
            }
            else
            {
                Debug.LogError("Can not find MaxSdkSetting!");
            }
        }

        #endregion

        #region FACEBOOK

        if (facebook == null)
        {
            string[] facebookSetting = UnityEditor.AssetDatabase.FindAssets("t:FacebookSettings");
            if (facebookSetting.Length != 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(facebookSetting[0]);
                facebook = UnityEditor.AssetDatabase.LoadAssetAtPath<FacebookSettings>(path);
            }
            else
            {
                Debug.LogError("Can not find MaxSdkSetting!");
            }
        }
        else
        {
          
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Facebook:");

            fbAppID = EditorGUILayout.TextField("App ID", fbAppID);
            var appIds = facebook.GetType().GetProperty("AppIds");

            if (appIds != null)
            {
                object facebookAppIDProp = null;
                if (string.IsNullOrEmpty(fbAppID))
                {
                    facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
                    fbAppID = ((List<string>)facebookAppIDProp)[0];
                }
                appIds.SetValue(facebook, new List<string>() { fbAppID }, null);
            }
            else
                Debug.LogError("Can not find FB app ID field!");

            fbClientToken = EditorGUILayout.TextField("Client token", fbClientToken);
            var clientToken = facebook.GetType().GetProperty("ClientTokens");

            if (clientToken != null)
            {
                object facebookClientTokenProps = null;
                if (string.IsNullOrEmpty(fbClientToken))
                {
                    facebookClientTokenProps = appIds.GetValue(facebookClientTokenProps, null);
                    fbClientToken = ((List<string>)facebookClientTokenProps)[0];
                }
                clientToken.SetValue(facebook, new List<string>() { fbClientToken }, null);
            }
            else
                Debug.LogError("Can not find FB client token field!");

            var keyStorePath = facebook.GetType().GetProperty("AndroidKeystorePath");
            if (keyStorePath != null)
            {
                keyStorePath.SetValue(facebook,  PlayerSettings.Android.keystoreName , null);
            }
        }
        
        

        #endregion

        #region AD ID SETTING
        if (adManager)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("AD IDs:");
            adManager.BannerAdUnitID = EditorGUILayout.TextField("Banner ID", adManager.BannerAdUnitID);
            adManager.InterstitialAdUnitID = EditorGUILayout.TextField("Inter ID", adManager.InterstitialAdUnitID);
            adManager.RewardedAdUnitID = EditorGUILayout.TextField("Reward ID", adManager.RewardedAdUnitID);
            adManager.OpenAdUnitID = EditorGUILayout.TextField("AppOpen ID", adManager.OpenAdUnitID);

            PrefabUtility.RecordPrefabInstancePropertyModifications(adManager);
        }
        #endregion

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check google-services.json"))
        {
            BuildProcess.CheckFirebaseJson();
        }
        if (GUILayout.Button("Check google-services.xml"))
        {
            BuildProcess.FixGoogleXml();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
       
        if (GUILayout.Button("Build"))
        {
            if (!PlayerSettings.applicationIdentifier.StartsWith("com."))
            {
                EditorUtility.DisplayDialog("Attention Pleas?",
                   "Your package name should in form \"com.\". This can make you can't build your project, consider change it ASAP!!", "Ok");
            }

            if (PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
            {
                EditorUtility.DisplayDialog("Attention Pleas?",
                   "Your package name is not in format 'com.X.Y' . This can make you can't build your project, consider change it ASAP!!", "Ok");
            }

            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Close"))
        {
            Close();
            GUIUtility.ExitGUI();
        }
    }

    void KeyStoreInfo()
    {
        EditorGUILayout.BeginHorizontal();
        if(!isShowKeyStorePass)
            PlayerSettings.keystorePass = EditorGUILayout.PasswordField("Keystore Pass", PlayerSettings.keystorePass);
        else
            PlayerSettings.keystorePass = EditorGUILayout.TextField("Keystore Pass", PlayerSettings.keystorePass);
        isShowKeyStorePass = EditorGUILayout.Toggle("Show", isShowKeyStorePass);
        EditorGUILayout.EndHorizontal();

        PlayerSettings.Android.keyaliasName = EditorGUILayout.TextField("Keystore Alias", PlayerSettings.Android.keyaliasName);
        EditorGUILayout.BeginHorizontal();
        if (!isShowAliasPass)
            PlayerSettings.keyaliasPass = EditorGUILayout.PasswordField("Keystore Pass", PlayerSettings.keyaliasPass);
        else
            PlayerSettings.keyaliasPass = EditorGUILayout.TextField("Keystore Pass", PlayerSettings.keyaliasPass);
        isShowAliasPass = EditorGUILayout.Toggle("Show", isShowAliasPass);
        EditorGUILayout.EndHorizontal();
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


}
#endif