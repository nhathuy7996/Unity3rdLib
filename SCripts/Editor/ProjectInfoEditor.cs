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
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

class ProjectInfoEditor : EditorWindow
{
    Vector2 scrollPos;
    bool isShowKeyStorePass = false, isShowAliasPass = false;
    Adjust adjustGameObject;


    GoogleMobileAdsSettings gg = null;

    HuynnLib.AdManager adManager = null;
    AppLovinSettings max = null;

    FireBaseManager fireBaseManager;

    FacebookSettings facebook;

    string fbAppID = null, fbClientToken = null ,fbKeyStore = null;
    static EditorWindow wnd;
    GUIStyle TextRedStyles, TextGreenStyles, ButtonTextStyles;

    int numberNativeADID = 0, numberAddOpenAdID = 0;

    // Add menu named "My Window" to the Window menu
    [MenuItem("3rdLib/Checklist APERO",priority = 0)]
    public static void InitWindowEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        wnd = GetWindow<ProjectInfoEditor>();
        wnd.titleContent = new GUIContent("Huynn 3rdLib - APERO version!");

        if (EditorBuildSettings.scenes.Count() > 0)
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        }
    }

    void OnGUI()
    {
        if (TextRedStyles == null)
        {
            TextRedStyles = new GUIStyle(EditorStyles.label);
            TextRedStyles.normal.textColor = Color.red;
        }

        if (TextGreenStyles == null)
        {
            TextGreenStyles = new GUIStyle(EditorStyles.label);
            TextGreenStyles.normal.textColor = Color.green;
        }

        if (ButtonTextStyles == null)
        {
            ButtonTextStyles = new GUIStyle(GUI.skin.button);
            ButtonTextStyles.normal.textColor = Color.green;
        }

      
        if (!wnd || EditorApplication.isPlaying)
        {
            Close();
            GUIUtility.ExitGUI();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scene:", TextGreenStyles);
        if(EditorGUILayout.DropdownButton(content: new GUIContent(EditorSceneManager.GetActiveScene().path), FocusType.Passive) && EditorBuildSettings.scenes.Count() > 0)
        {
            GenericMenu menu = new GenericMenu();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                AddMenuItemForScenes(menu,scene.path , scene, SceneManager.GetActiveScene().path.Equals(scene.path));
            }
            
            
            menu.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(wnd.position.width), GUILayout.Height(wnd.position.height-20));
        if (!adManager)
        {
            adManager = GameObject.FindObjectOfType<HuynnLib.AdManager>();
            if (adManager)
            {
                numberNativeADID = adManager.NativeAdID.Count;
                numberAddOpenAdID = adManager.OpenAdUnitIDs.Count;
            }
        }
        #region EDITOR
        EditorGUILayout.LabelField("Build Version:", TextGreenStyles);

        PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("Version Code", PlayerSettings.Android.bundleVersionCode);

        EditorGUILayout.BeginHorizontal();
        PlayerSettings.companyName = EditorGUILayout.TextField("Company Name", PlayerSettings.companyName);
        PlayerSettings.productName = EditorGUILayout.TextField("Product Name", PlayerSettings.productName);
        EditorGUILayout.EndHorizontal();

        PlayerSettings.bundleVersion = EditorGUILayout.TextField("App Version", PlayerSettings.bundleVersion);

        EditorGUILayout.BeginHorizontal();
        string applicationIdentifier = EditorGUILayout.TextField("Package Name", PlayerSettings.applicationIdentifier);
        
        EditorGUILayout.LabelField("Package name should in form \"com.X.Y\" other can cost a build error!", TextRedStyles);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
        EditorGUILayout.EndHorizontal();

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
            EditorGUILayout.LabelField("Adjust:", TextGreenStyles);
            adjustGameObject.appToken = EditorGUILayout.TextField("Adjust Token", adjustGameObject.appToken);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Adjust Mode");
            string adjustEvironment = adjustGameObject.environment.ToString();
            bool dropDownSelected = EditorGUILayout.DropdownButton(content: new GUIContent(adjustEvironment), FocusType.Passive);
            EditorGUILayout.EndHorizontal();

            if (dropDownSelected)
            {

                GenericMenu menu = new GenericMenu();

                AddMenuItemForAdjust(menu, AdjustEnvironment.Production.ToString(), AdjustEnvironment.Production,
                    adjustGameObject.environment == AdjustEnvironment.Production);
                AddMenuItemForAdjust(menu, AdjustEnvironment.Sandbox.ToString(), AdjustEnvironment.Sandbox,
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
            EditorGUILayout.LabelField("Google:", TextGreenStyles);
            gg.GoogleMobileAdsAndroidAppId = EditorGUILayout.TextField("Android AD ID", gg.GoogleMobileAdsAndroidAppId);
            if (adManager)
            {
                adManager.paid_ad_revenue = EditorGUILayout.TextField("Event Paid AD", adManager.paid_ad_revenue);
                PrefabUtility.RecordPrefabInstancePropertyModifications(adManager);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(gg);
            EditorUtility.SetDirty(gg);
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
            EditorGUILayout.LabelField("AppLovin:", TextGreenStyles);
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
            EditorUtility.SetDirty(max);
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
                Debug.LogError("[Huynn3rdLib]:Can not find MaxSdkSetting!");
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
                Debug.LogError("[Huynn3rdLib]:Can not find MaxSdkSetting!");
            }
        }
        else
        {
          
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Facebook:", TextGreenStyles);

            
            var appIds = facebook.GetType().GetProperty("AppIds");

            if (appIds != null)
            {
                object facebookAppIDProp = null;
                if (string.IsNullOrEmpty(fbAppID))
                {
                    facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
                    fbAppID = ((List<string>)facebookAppIDProp)[0];
                }
                fbAppID = EditorGUILayout.TextField("App ID", fbAppID);
                appIds.SetValue(facebook, new List<string>() { fbAppID }, null);
            }
            else
                Debug.LogError("[Huynn3rdLib]:Can not find FB app ID field!");

            
            var clientToken = facebook.GetType().GetProperty("ClientTokens");

            if (clientToken != null)
            {
                object facebookClientTokenProps = null;
                if (string.IsNullOrEmpty(fbClientToken))
                {
                    facebookClientTokenProps = clientToken.GetValue(facebookClientTokenProps, null);
                    fbClientToken = ((List<string>)facebookClientTokenProps)[0];
                }
                fbClientToken = EditorGUILayout.TextField("Client token", fbClientToken);
                clientToken.SetValue(facebook, new List<string>() { fbClientToken }, null);
            }
            else
                Debug.LogError("[Huynn3rdLib]:Can not find FB client token field!");

            var keyStorePath = facebook.GetType().GetProperty("AndroidKeystorePath");
            if (keyStorePath != null)
            {
                keyStorePath.SetValue(facebook,  PlayerSettings.Android.keystoreName , null);
            }

            EditorUtility.SetDirty(facebook);
        }
        
        

        #endregion

        #region AD ID SETTING
        if (adManager)
        {
    
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("AD IDs:", TextGreenStyles);
            adManager.BannerAdUnitID = EditorGUILayout.TextField("Banner ID", adManager.BannerAdUnitID);
            adManager.InterstitialAdUnitID = EditorGUILayout.TextField("Inter ID", adManager.InterstitialAdUnitID);
            adManager.RewardedAdUnitID = EditorGUILayout.TextField("Reward ID", adManager.RewardedAdUnitID);


            EditorGUILayout.BeginHorizontal();
            numberAddOpenAdID = EditorGUILayout.IntField("AppOpen AD ID number", numberAddOpenAdID);


            if (numberAddOpenAdID > adManager.OpenAdUnitIDs.Count)
            {
                adManager.OpenAdUnitIDs.AddRange(new string[numberAddOpenAdID - adManager.OpenAdUnitIDs.Count]); 
            }

            if (numberAddOpenAdID < adManager.OpenAdUnitIDs.Count)
            {
                int numberRemove = adManager.OpenAdUnitIDs.Count - numberAddOpenAdID;
                adManager.OpenAdUnitIDs.RemoveRange(numberAddOpenAdID, numberRemove); 
            }


            EditorGUILayout.BeginVertical();
            for (int i = 0; i < adManager.OpenAdUnitIDs.Count; i++)
            {
                adManager.OpenAdUnitIDs[i] = EditorGUILayout.TextField("Ad ID " + (i + 1), adManager.OpenAdUnitIDs[i]);
            }

             
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal(); 
#if NATIVE_AD
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            numberNativeADID = EditorGUILayout.IntField("NativeAd ID number", numberNativeADID);


            if (numberNativeADID > adManager.NativeAdID.Count)
            {
                adManager.NativeAdID.AddRange(new string[numberNativeADID - adManager.NativeAdID.Count]);
                adManager.adNativePanel.AddRange(new AdNativeObject[numberNativeADID - adManager.adNativePanel.Count]);
            }

            if (numberNativeADID < adManager.NativeAdID.Count)
            {
                int numberRemove = adManager.NativeAdID.Count - numberNativeADID;
                adManager.NativeAdID.RemoveRange(numberNativeADID, numberRemove);
                adManager.adNativePanel.RemoveRange(numberNativeADID, adManager.adNativePanel.Count - numberNativeADID);
            }


            EditorGUILayout.BeginVertical();
            for (int i = 0; i< adManager.NativeAdID.Count; i++)
            {
                adManager.NativeAdID[i] = EditorGUILayout.TextField("Ad ID "+(i+1), adManager.NativeAdID[i]);
            }

            if( EditorGUILayout.LinkButton("ID test: ca-app-pub-3940256099942544/2247696110"))
            {
                for (int i = 0; i < adManager.NativeAdID.Count; i++)
                {
                    if (adManager.NativeAdID[i].Equals("ca-app-pub-3940256099942544/2247696110"))
                        continue;
                    adManager.NativeAdID[i] = "ca-app-pub-3940256099942544/2247696110";
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
#endif
            PrefabUtility.RecordPrefabInstancePropertyModifications(adManager);
        }
#endregion

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check google-services.json"))
        {
            MenuEditor.CheckFirebaseJson();
        }
        if (GUILayout.Button("Check google-services.xml"))
        {
            MenuEditor.FixGoogleXml();
        }

        if (GUILayout.Button("Fix AndroidManifest FbID"))
        {
            EditorUtility.DisplayDialog("Attention Pleas?",
                  "This will change your AndroidManifest for match FBID!!", "Ok");
            MenuEditor.FixAndroidManifestFB();
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
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Close();
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
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

    void AddMenuItemForAdjust(GenericMenu menu, string menuPath, AdjustEnvironment value, bool isSelected = false)
    {
        // the menu item is marked as selected if it matches the current value of m_Color
        menu.AddItem(new GUIContent(menuPath), isSelected, OnDropBoxAdjustItemClick, value);
    }

    void AddMenuItemForScenes(GenericMenu menu, string menuPath, EditorBuildSettingsScene value, bool isSelected = false)
    {
        // the menu item is marked as selected if it matches the current value of m_Color
        menu.AddItem(new GUIContent(menuPath), isSelected, OnDropBoxSceneItemClick, value);
    }

    void OnDropBoxSceneItemClick(object item)
    {
        EditorSceneManager.OpenScene(((EditorBuildSettingsScene)item).path);
    }

    void OnDropBoxAdjustItemClick(object item)
    {
        Adjust adjustGameObject = GameObject.FindObjectOfType<Adjust>();
        adjustGameObject.environment = (AdjustEnvironment)item;
    }


}
#endif