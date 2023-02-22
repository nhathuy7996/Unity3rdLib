using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;

namespace DVAH
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        [SerializeField]
        bool _isUseLoading = true, _isLoadingAutoStart = true;

        [SerializeField]
        float _maxTimeLoading = 0;

        [SerializeField]
        int _numberCondition = 0;

        [SerializeField]
        List<bool> _conditionDone = new List<bool>();


        [Header("---------Config---------")]
        [Space(10)]
        [SerializeField]
        GameObject _loadingPopUp;
        [SerializeField]
        Slider _loading;

        [SerializeField]
        Text _loadingText;

        Action<List<bool>> _onDone = null;

        float _loadingMaxvalue;

        // Start is called before the first frame update


        private void Start()
        {
            if(_isLoadingAutoStart)
                Init(); 
        }

        public LoadingManager Init(Action<List<bool>> onDone = null)
        {
            
            _conditionDone.Clear();
            for (int i = 0; i < _numberCondition; i++)
            {
                _conditionDone.Add(false);
            }


            _loadingMaxvalue = _loading.maxValue;
            _loading.value = 0;
            _loading.onValueChanged.RemoveAllListeners();
            _onDone = onDone;
            _onDone += (doneCondition) =>
            {
                _ = FireBaseManager.Instant.LogEventWithParameter("screen_view_data", new Hashtable()
                {
                    {"id_screen","loading_end" }
                });

                _ = FireBaseManager.Instant.LogEventWithParameter("screen_view_data", new Hashtable()
                { 
                    {"id_screen","lobby_start" }
                });
            };
            _loading.onValueChanged.AddListener((value) =>
            {
                if (value == 100)
                {
                    Debug.Log("[Huynn3rdLib]==>Loading Done!<==");
                    _loadingPopUp.SetActive(false);

                    try
                    {
                        _onDone?.Invoke(_conditionDone);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Huynn3rdLib]==> callback ondone loading error: " + e.ToString() + " <==");
                    }

                    _loading.onValueChanged.RemoveAllListeners();

                }
            });
            return this;
        }

        // Update is called once per frame
        void Update()
        {
            _loading.value += _loadingMaxvalue * Time.deltaTime / _maxTimeLoading;
            _loadingText.text = string.Format("{0:0.0}%", _loading.value);


        }

        public void StopLoading()
        {
           
            if (_maxTimeLoading == 0.2f)
                return;
            Debug.Log("[Huynn3rdLib]==> Force stop loading! <==");
            _loadingMaxvalue = _loading.maxValue - _loading.value;
            _maxTimeLoading = 0.2f;

        }

        public LoadingManager AddOnDoneLoading(Action<List<bool>> callback)
        {
            if (_loading.value >= 99f)
            {
                callback?.Invoke(_conditionDone);
                return this;
            }
            _onDone += (callback);
            return this;
        }

        public LoadingManager DoneCondition(int id)
        {
            if (id >= _conditionDone.Count)
            {
                Debug.LogError("[Huynn3rdLib]==> ID condition not exist, check number of conditon on inspector! <==");
                return this;
            }

            if (_loading.value >= 99f)
            {
                Debug.LogWarning("[Huynn3rdLib]==> Loading already stop, maybe check your game flow! <==");
                return this;
            }

            _conditionDone[id] = true;
            if (_conditionDone.Where(c => c == false).Count() == 0)
            {
                Debug.Log("[Huynn3rdLib]==> All condition is done! stop loading! <==");
                StopLoading();
            }

            return this;
        }

        private void OnDrawGizmosSelected()
        {
     
            if (_isUseLoading && _loading.value < 100)
            {
                _loadingPopUp.SetActive(true);
            }
            else
            {
                _loadingPopUp.SetActive(false);
            }
        }
    }
}
