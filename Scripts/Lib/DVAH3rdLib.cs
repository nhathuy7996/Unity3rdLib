using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq; 
using SimpleJSON;
using System;
using UnityEngine.UI;
using UnityEngine.Android;
#if UNITY_ANDROID
using GoogleMobileAds.Common;
using GoogleMobileAds.Api;
using Facebook.Unity;
#endif

namespace DVAH
{
    public class DVAH3rdLib : Singleton<DVAH3rdLib>, IAppStateChange
    {
        [SerializeField] bool _isShowDebug = false, _isDontDestroyOnLoad = false;
        [SerializeField] GameObject _notiDebug, _noInternetDebug;
        MasterLib _masterLib;
        int _devTapCount = 0;

        [Header("------------POPUP-------------")]
        [SerializeField] RateController _popupRate;
        [SerializeField] GameObject _popupForceUpdate;
        [SerializeField] Button _forceUpdateBlackPanel, _forceUpdateNo;


        [Header("------------LIB-------------")]
        [SerializeField]
        bool _isAutoInit = false;
        public bool isAutoInit => _isAutoInit;
        [SerializeField]
        bool _isInitByOrder = false;
        public bool isInitByOrder => _isInitByOrder;

        [SerializeField] List<GameObject> _childLibs;
        public List<GameObject> ChildLibs => _childLibs;

        int _countOpenApp = 0;

        public int countOpenApp => _countOpenApp;

        protected override void Awake()
        {
            base.Awake();

#if UNITY_ANDROID
            AppStateEventNotifier.AppStateChanged += this.OnAppStateChanged;
            BindReport();
#endif

            if (_isDontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);


            if (!_masterLib)
                _masterLib = this.GetComponentInChildren<MasterLib>();

            _countOpenApp = PlayerPrefs.GetInt(CONSTANT.COUNT_OPEN_APP,-1) + 1;

        }

        void BindReport()
        {
            DVAH_Data data = Resources.Load<DVAH_Data>("DVAH_Data");
            try
            {

                TextWriter tw = new StreamWriter(Application.persistentDataPath + "/report.txt");
                tw.Write(data.Report);
                tw.Close();
            }
            catch
            {
                // ignored
            }

        }


        // Start is called before the first frame update
        void Start()
        {

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
 
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 120;

            if (_isAutoInit)
                _masterLib.InitChildLib(() => { Debug.Log("=====> <color=#00FF00>Init done all!</color> <====="); });

            FireBaseManager.Instant.GetValueRemoteAsync(CONSTANT.FORCE_UPDATE, (value) =>
            {
                try
                {
                    var data = JSON.Parse(value.StringValue);
                    if (!Application.version.Equals(data["version"]))
                    {
                        _popupForceUpdate.SetActive(true);
                    }

                    if (data["force"].AsBool)
                    {
                        _forceUpdateNo.onClick.AddListener(() =>
                        {
                            CloseApplication();
                        });
                        _forceUpdateBlackPanel.onClick.AddListener(() =>
                        {
                            CloseApplication();
                        });

                    }

                }
                catch (Exception e)
                {
                    Debug.LogError("===>Error on set forceupdate popup!<==== " + e.ToString());
                }

            });

#if UNITY_EDITOR
            this.CheckFirebaseJS();
            this.CheckFirebaseXml();
#endif

#if UNITY_ANDROID
            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(() =>
                {
                    FB.ActivateApp();
                });
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
#endif
        }

        // Update is called once per frame
        void Update()
        {
            if (_noInternetDebug)
            {
                _noInternetDebug.SetActive(!this.CheckInternetConnection());
            }


            if (!_isShowDebug  )
                return;
            if (Input.touchCount < 3)
            {
                if (_devTapCount != 0)
                    _devTapCount = 0;
                return;
            }

            if (Screen.width - Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) > Screen.width / 4)
            {
                return;
            }

            if (Screen.height - Vector2.Distance(Input.GetTouch(2).position, Input.GetTouch(1).position) > Screen.height / 4)
            {
                return;
            }

            if (Input.touchCount == 4 && Input.GetTouch(3).phase == TouchPhase.Ended)
                _devTapCount++;

            if (_devTapCount < 5)
                return;

            _devTapCount = 0;
            _ = AdMHighFather.Instant.ShowAdDebugger();

            if (_notiDebug != null && !_notiDebug.activeSelf)
                _notiDebug.SetActive(true);
        }

        public void ShowPopUpRate(bool isShow = true, Action<bool> _callback = null)
        {
            if (isShow && PlayerPrefs.HasKey(CONSTANT.RATE_CHECK))
                return;

            _popupRate.setCallBack(_callback);
            _popupRate.gameObject.SetActive(isShow);
        }

        public void GotoMarket()
        {
            Application.OpenURL("market://details?id=" + Application.identifier);
        }

        public void CloseApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_masterLib)
                _masterLib = this.GetComponentInChildren<MasterLib>();

            //CheckFirebaseJS();

            if (_notiDebug == null && this.transform.GetChild(0).GetComponent<NotiManager>() != null)
            {
                _notiDebug = this.transform.GetChild(0).gameObject;
            }
            //_notiDebug.SetActive(_isShowDebug);


        }

        public void GetSubLib()
        {
            if (!_masterLib)
                _masterLib = this.GetComponentInChildren<MasterLib>();
            for (int i = 0; i < _masterLib.transform.childCount; i++)
            {
                Transform Ichild = _masterLib.transform.GetChild(i);
                if (Ichild.GetComponent<IChildLib>() == null)
                    DestroyImmediate(Ichild.gameObject);
            }

            this._childLibs = new List<GameObject>();
            IChildLib[] childLib = this.GetComponentsInChildren<IChildLib>();

            for (int i = 0; i < childLib.Count(); i++)
            {

                _childLibs.Add(_masterLib.transform.GetChild(i).gameObject);
            }

        }

        public bool CheckInternetConnection()
        {
            return AdMHighFather.Instant.CheckInternetConnection();
        }

        public bool CheckFirebaseJS()
        {

            string[] files = Directory.GetFiles(Application.dataPath, "*.json*", SearchOption.AllDirectories)
                                .Where(f => f.EndsWith("google-services.json")).ToArray();
            if (files.Length == 0)
            {
                Debug.LogError("==>Project doesnt contain google-services.json. Firebase may not work!!!!!<==");
                return false;
            }

            if (files.Length > 1)
            {
                Debug.LogError(string.Format("==>Project contain more than one file google-services.json: \n{0} \n{1} . Firebase may not work wrong!!!!!<==", files[0], files[1]));
                return false;
            }

            return true;
        }

        public string CheckFirebaseXml()
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*google-services.xml", SearchOption.AllDirectories).ToArray();
            if (files.Length == 1)
            {
                return files[0];
            }

            Debug.LogError("==>Project error google-services.xml. Firebase may not work wrong!!!!!<==");
            return null;
        }


        public void OnAppStateChanged(AppState state)
        {
            var appStateObject = FindObjectsOfType<MonoBehaviour>().OfType<IAppStateChange>();
            foreach (IAppStateChange singleObject in appStateObject)
            {
                if ((singleObject as MonoBehaviour).gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
                    continue;
                singleObject.OnAppStateChanged(state);
            }

            try
            {
                if (state == AppState.Foreground)
                {

                    FireBaseManager.Instant.LogEventWithParameterAsync("on_game_resume_focus", new Hashtable()
                        {
                            { "id_screen", AdManager.Instant.ScreenName }
                        });

                }
                else
                {
                    FireBaseManager.Instant.LogEventWithParameterAsync("on_game_out_focus", new Hashtable()
                        {
                            { "id_screen", AdManager.Instant.ScreenName }
                        });
                }

            }
            catch (Exception e)
            {

            }
        }

#if UNITY_EDITOR || UNITY_IOS

        private void OnApplicationFocus(bool focus)
        {
            try
            {
                var appStateObject = FindObjectsOfType<MonoBehaviour>().OfType<IAppStateChange>();
                foreach (IAppStateChange singleObject in appStateObject)
                {
                    if ((singleObject as MonoBehaviour).gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
                        continue;
                    singleObject.OnAppStateChanged(focus ? AppState.Foreground : AppState.Background);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
#endif
    }
}

