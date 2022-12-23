using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace HuynnLib
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        [SerializeField]
        Slider _loading;

        [SerializeField]
        Text _loadingText;

        [SerializeField]
        float _maxTimeLoading = 0;

        [SerializeField]
        UnityEvent _onDone = null;

        float _loadingMaxvalue;

        // Start is called before the first frame update


        private void Start()
        {
            Init();
        }

        void Init(UnityEvent onDone = null)
        {
            _loadingMaxvalue = _loading.maxValue;
            _loading.value = 0;
            _loading.onValueChanged.RemoveAllListeners();
            _onDone = onDone;
            _loading.onValueChanged.AddListener((value) =>
            {
                if (value == 100)
                {
                    try
                    {
                        _onDone?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("==> callback ondone loading error: "+e.ToString()+" <==");
                    }

                    _loading.transform.parent.gameObject.SetActive(false);
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
            _loading.value += _loadingMaxvalue * Time.deltaTime / _maxTimeLoading;
            _loadingText.text = string.Format("{0:0.0}%", _loading.value);


        }

        public void StopLoading(UnityAction  callback)
        {
            Debug.Log("==> Force stop loading! <==");
            if (_loading.value >= 99f)
            {
                callback?.Invoke();
                return;
            }
            _onDone.AddListener(callback);
            _loadingMaxvalue = _loading.maxValue - _loading.value;
            _maxTimeLoading = 1;
            
        }

        
    }
}
