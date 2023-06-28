using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Xml;
using SimpleJSON;
using System;
using UnityEngine.UI;
using Facebook.Unity;

namespace DVAH
{
    public class DVAH3rdLib : Singleton<DVAH3rdLib>
    {
        [SerializeField] bool _isShowDebug = false, _isDontDestroyOnLoad = false;
        [SerializeField] GameObject _notiDebug, _noInternetDebug;
        MasterLib _masterLib;
        int _devTapCount = 0;

        [Header("------------POPUP-------------")]
        [SerializeField] GameObject _popupRate;
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
            if (_isDontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);


            if (!_masterLib)
                _masterLib = this.GetComponentInChildren<MasterLib>();

            _countOpenApp = PlayerPrefs.GetInt(CONSTANT.COUNT_OPEN_APP,-1) + 1;

        }
        // Start is called before the first frame update
        void Start()
        {
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
        }

        // Update is called once per frame
        void Update()
        {
            if (_noInternetDebug)
            {
                _noInternetDebug.SetActive(!this.CheckInternetConnection());
            }


            if (!_isShowDebug || _notiDebug == null)
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

            //_ = AdMHighFather.Instant.ShowAdDebugger();
            if (!_notiDebug.activeSelf)
                _notiDebug.SetActive(true);
        }

        public void ShowPopUpRate(bool isShow = true)
        {
            if (isShow && PlayerPrefs.HasKey(CONSTANT.RATE_CHECK))
                return;
            _popupRate.SetActive(isShow);
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
            _notiDebug.SetActive(_isShowDebug);


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

        
    }
}

