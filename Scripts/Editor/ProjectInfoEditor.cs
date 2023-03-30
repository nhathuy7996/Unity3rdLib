/* * * * *
 * A simple helper for APERO checklist
 * ------------------------------
 * Written by Huynn7996
 * 2022-09-07
 * 
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) Huy Nguyen Nhat (Huynn7996)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using com.adjust.sdk;
using GoogleMobileAds.Editor;
using System;
using DVAH;
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

    DVAH_Data DVAH_Data;

    static bool usingAdNative = false, usingIAP = false;

    bool isShowKeyStorePass = false, isShowAliasPass = false;
    Adjust adjustGameObject;


    GoogleMobileAdsSettings gg = null;

    DVAH.AdMHighFather adManager = null;
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

        string[] symbolsList;
        PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android,out symbolsList);
        if (symbolsList.ToList().Contains("NATIVE_AD"))
            usingAdNative = true;
        if (symbolsList.ToList().Contains("IAP"))
            usingIAP = true;
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

        if (!DVAH_Data) {
            string[] DVAH_Datas = UnityEditor.AssetDatabase.FindAssets("t:DVAH_Data");
            if (DVAH_Datas.Length != 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(DVAH_Datas[0]);
                DVAH_Data = UnityEditor.AssetDatabase.LoadAssetAtPath<DVAH_Data>(path);

                numberAddOpenAdID = DVAH_Data.AppLovin_ADOpenIDs.Count;
#if NATIVE_AD
                numberNativeADID = DVAH_Data.AppLovin_NativeAdIDs.Count;
#endif
            }
            else
            {
                EditorGUILayout.LabelField("Can not find DVAH data file!");
            }
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
            adManager = GameObject.FindObjectOfType<DVAH.AdMHighFather>();
          
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
        usingAdNative = EditorGUILayout.Toggle("Using Ad Native", usingAdNative);
        usingIAP = EditorGUILayout.Toggle("Using IAP", usingIAP);

        string[] symbols;
        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out symbols);
        List<string> tmpSymbols = symbols.ToList();
        if (usingAdNative)
        { 
            if (!tmpSymbols.Contains("NATIVE_AD"))
            {
                tmpSymbols.Add("NATIVE_AD");
                symbols = tmpSymbols.ToArray();
            }
        }
        else
        {
            if (symbols.Contains("NATIVE_AD"))
            {
                
                tmpSymbols.Remove("NATIVE_AD");
                symbols = tmpSymbols.ToArray();
                
            } 
        }

        if (usingIAP)
        {
            if (!symbols.Contains("IAP"))
            {
                tmpSymbols.Add("IAP");
                symbols = tmpSymbols.ToArray();
            }
        }
        else
        {
            if (symbols.Contains("IAP"))
            {
               
                tmpSymbols.Remove("IAP");
                symbols = tmpSymbols.ToArray();

            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbols);

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

        #region Set data on file
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Adjust:", TextGreenStyles);
        DVAH_Data.Adjust_token = EditorGUILayout.TextField("Adjust Token", DVAH_Data.Adjust_token.Replace(" ", ""));


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Adjust Mode");
        string adjustEvironment = DVAH_Data.AdjustMode.ToString();
        bool dropDownSelected = EditorGUILayout.DropdownButton(content: new GUIContent(adjustEvironment), FocusType.Passive);
        EditorGUILayout.EndHorizontal();

        if (dropDownSelected)
        {

            GenericMenu menu = new GenericMenu();

            AddMenuItemForAdjust(menu, ADJUST_MODE.Production.ToString(), ADJUST_MODE.Production,
                DVAH_Data.AdjustMode == ADJUST_MODE.Production);
            AddMenuItemForAdjust(menu, ADJUST_MODE.Sandbox.ToString(), ADJUST_MODE.Sandbox,
                 DVAH_Data.AdjustMode == ADJUST_MODE.Sandbox);
            menu.ShowAsContext();
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Events Token:", TextGreenStyles);

        DVAH_Data.EventsToken_AdValue = EditorGUILayout.TextField("ad_value", DVAH_Data.EventsToken_AdValue.Replace(" ", ""));
        DVAH_Data.EventsToken_LevelAchives = EditorGUILayout.TextField("level_achived", DVAH_Data.EventsToken_LevelAchives.Replace(" ", ""));

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Google:", TextGreenStyles);
        DVAH_Data.Google_Android_AppID = EditorGUILayout.TextField("Android App ID", DVAH_Data.Google_Android_AppID.Replace(" ", ""));
        DVAH_Data.Google_Event_Paid_AD = EditorGUILayout.TextField("Event Paid AD", DVAH_Data.Google_Event_Paid_AD.Replace(" ", ""));

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("AppLovin (MAX):", TextGreenStyles);
        DVAH_Data.AppLovin_SDK_Key = EditorGUILayout.TextField("MaxSdk key", DVAH_Data.AppLovin_SDK_Key.Replace(" ", ""));
        EditorGUILayout.LabelField("Android AD ID:                       " + DVAH_Data.Google_Android_AppID);


        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Facebook:", TextGreenStyles);

        DVAH_Data.Facebook_AppID = EditorGUILayout.TextField("App ID", DVAH_Data.Facebook_AppID);
        DVAH_Data.Facebook_ClientToken = EditorGUILayout.TextField("Client token", DVAH_Data.Facebook_ClientToken.Replace(" ", ""));

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("AppLovin(MAX) AD IDs:", TextGreenStyles);
        DVAH_Data.AppLovin_BannerID = EditorGUILayout.TextField("Banner ID", DVAH_Data.AppLovin_BannerID.Replace(" ", ""));
        DVAH_Data.AppLovin_InterID = EditorGUILayout.TextField("Inter ID", DVAH_Data.AppLovin_InterID.Replace(" ", ""));
        DVAH_Data.AppLovin_RewardID = EditorGUILayout.TextField("Reward ID", DVAH_Data.AppLovin_RewardID.Replace(" ", ""));


        EditorGUILayout.BeginHorizontal();
        numberAddOpenAdID = EditorGUILayout.IntField("AppOpen AD ID number", numberAddOpenAdID);


        if (numberAddOpenAdID > DVAH_Data.AppLovin_ADOpenIDs.Count)
        {
            DVAH_Data.AppLovin_ADOpenIDs.AddRange(new string[numberAddOpenAdID - DVAH_Data.AppLovin_ADOpenIDs.Count]);
        }

        if (numberAddOpenAdID < DVAH_Data.AppLovin_ADOpenIDs.Count)
        {
            int numberRemove = DVAH_Data.AppLovin_ADOpenIDs.Count - numberAddOpenAdID;
            DVAH_Data.AppLovin_ADOpenIDs.RemoveRange(numberAddOpenAdID, numberRemove);
        }

        EditorGUILayout.BeginVertical();
        for (int i = 0; i < DVAH_Data.AppLovin_ADOpenIDs.Count; i++)
        {
            if (DVAH_Data.AppLovin_ADOpenIDs[i] == null)
                DVAH_Data.AppLovin_ADOpenIDs[i] = "";
            DVAH_Data.AppLovin_ADOpenIDs[i] = EditorGUILayout.TextField("Ad ID " + (i + 1), DVAH_Data.AppLovin_ADOpenIDs[i].Replace(" ", ""));
        }


        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        PrefabUtility.RecordPrefabInstancePropertyModifications(DVAH_Data);
        EditorUtility.SetDirty(DVAH_Data);
#if NATIVE_AD
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        numberNativeADID = EditorGUILayout.IntField("NativeAd ID number", numberNativeADID);


        if (numberNativeADID > DVAH_Data.AppLovin_NativeAdIDs.Count)
        {
            DVAH_Data.AppLovin_NativeAdIDs.AddRange(new string[numberNativeADID - DVAH_Data.AppLovin_NativeAdIDs.Count]);
        }

        if (numberNativeADID < DVAH_Data.AppLovin_NativeAdIDs.Count)
        {
            int numberRemove = DVAH_Data.AppLovin_NativeAdIDs.Count - numberNativeADID;
            DVAH_Data.AppLovin_NativeAdIDs.RemoveRange(numberNativeADID, numberRemove);
        }


        EditorGUILayout.BeginVertical();
        for (int i = 0; i < DVAH_Data.AppLovin_NativeAdIDs.Count; i++)
        {
            if (DVAH_Data.AppLovin_NativeAdIDs[i] == null)
                DVAH_Data.AppLovin_NativeAdIDs[i] = "";
            DVAH_Data.AppLovin_NativeAdIDs[i] = EditorGUILayout.TextField("Ad ID " + (i + 1), DVAH_Data.AppLovin_NativeAdIDs[i].Replace(" ", ""));
        }

        if (EditorGUILayout.LinkButton("ID test: ca-app-pub-3940256099942544/2247696110"))
        {
            for (int i = 0; i < DVAH_Data.AppLovin_NativeAdIDs.Count; i++)
            {
                if (DVAH_Data.AppLovin_NativeAdIDs[i].Equals("ca-app-pub-3940256099942544/2247696110"))
                    continue;
                DVAH_Data.AppLovin_NativeAdIDs[i] = "ca-app-pub-3940256099942544/2247696110";
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal(); 
#endif

        PrefabUtility.RecordPrefabInstancePropertyModifications(DVAH_Data);
        EditorUtility.SetDirty(DVAH_Data);
        #endregion

        #region ADJUST

        if (adjustGameObject)
        {
            adjustGameObject.startManually = false;
            adjustGameObject.appToken = DVAH_Data.Adjust_token;


            int idEnvironment = (int)DVAH_Data.AdjustMode;
            adjustGameObject.environment = (AdjustEnvironment)idEnvironment;

            if (fireBaseManager)
            {
                fireBaseManager.ADValue = DVAH_Data.EventsToken_AdValue;
                fireBaseManager.Level_Achived = DVAH_Data.EventsToken_LevelAchives;
                PrefabUtility.RecordPrefabInstancePropertyModifications(fireBaseManager);
            }
           
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
            gg.GoogleMobileAdsAndroidAppId = DVAH_Data.Google_Android_AppID;
            if (adManager)
            {
                adManager.paid_ad_revenue = DVAH_Data.Google_Event_Paid_AD;
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

            max.SdkKey = DVAH_Data.AppLovin_SDK_Key;
            if (adManager)
                adManager.MaxSdkKey = max.SdkKey;
            if (gg != null)
            {
                max.AdMobAndroidAppId = gg.GoogleMobileAdsAndroidAppId; 
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
            var appIds = facebook.GetType().GetProperty("AppIds");

            if (appIds != null)
            {
                object facebookAppIDProp = null;
                if (string.IsNullOrEmpty(fbAppID))
                {
                    facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
                    fbAppID = ((List<string>)facebookAppIDProp)[0];
                }
                 
                appIds.SetValue(facebook, new List<string>() { DVAH_Data.Facebook_AppID }, null);
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
               
                clientToken.SetValue(facebook, new List<string>() { DVAH_Data.Facebook_ClientToken }, null);
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
            
            adManager.BannerAdUnitID = DVAH_Data.AppLovin_BannerID;
            adManager.InterstitialAdUnitID = DVAH_Data.AppLovin_InterID;
            adManager.RewardedAdUnitID = DVAH_Data.AppLovin_RewardID; 

            if (numberAddOpenAdID > adManager.OpenAdUnitIDs.Count)
            {
                adManager.OpenAdUnitIDs.AddRange(new string[numberAddOpenAdID - adManager.OpenAdUnitIDs.Count]); 
            }

            if (numberAddOpenAdID < adManager.OpenAdUnitIDs.Count)
            {
                int numberRemove = adManager.OpenAdUnitIDs.Count - numberAddOpenAdID;
                adManager.OpenAdUnitIDs.RemoveRange(numberAddOpenAdID, numberRemove); 
            }
             
            for (int i = 0; i < adManager.OpenAdUnitIDs.Count; i++)
            {
                if (adManager.OpenAdUnitIDs[i] == null)
                    adManager.OpenAdUnitIDs[i] = "";
                adManager.OpenAdUnitIDs[i] = DVAH_Data.AppLovin_ADOpenIDs[i];
            } 
 
#if NATIVE_AD
             
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

             
            for (int i = 0; i< adManager.NativeAdID.Count; i++)
            {
                if (adManager.NativeAdID[i] == null)
                    adManager.NativeAdID[i] = "";
                adManager.NativeAdID[i] = DVAH_Data.AppLovin_NativeAdIDs[i];
            } 
#endif


            EditorUtility.SetDirty(adManager);
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

    void AddMenuItemForAdjust(GenericMenu menu, string menuPath, ADJUST_MODE value, bool isSelected = false)
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
        this.DVAH_Data.AdjustMode = (ADJUST_MODE)item;
         
    }


}
#endif