using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Xml;
using SimpleJSON;
using System;

namespace HuynnLib
{
    public class Huynn3rdLib : Singleton<Huynn3rdLib>
    {
        [SerializeField] bool _isShowDebug = false, _isDontDestroyOnLoad = false;
        [SerializeField] GameObject _notiDebug, _noInternetDebug;
        MasterLib _masterLib;
        int _devTapCount = 0;

        [SerializeField] GameObject _popupRate;

        [Header("------------LIB-------------")]
        [SerializeField]
        bool _isAutoInit = false;
        public bool isAutoInit => _isAutoInit;
        [SerializeField]
        bool _isInitByOrder = false;
        public bool isInitByOrder => _isInitByOrder;

        [SerializeField] List<GameObject> _childLibs;
        public List<GameObject> ChildLibs => _childLibs;

        // Start is called before the first frame update
        void Start()
        {
            if (_isDontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

#if UNITY_EDITOR
            this.CheckFirebaseJS();
            this.CheckFirebaseXml();
#endif
        }

        // Update is called once per frame
        void Update()
        {
            if (_noInternetDebug && !AdManager.Instant.CheckInternetConnection())
                _noInternetDebug.SetActive(true);


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

            if (!_notiDebug.activeSelf)
                _notiDebug.SetActive(true);
        }

        public void ShowPopUpRate(bool isShow = true)
        {
            _popupRate.SetActive(isShow);
        }

        public void CloseApplication()
        {
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
                Debug.LogError("==>Project contain more than one file google-services.json. Firebase may not work wrong!!!!!<==");
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

