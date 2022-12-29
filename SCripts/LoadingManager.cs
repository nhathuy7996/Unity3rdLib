using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;

namespace HuynnLib
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        [SerializeField]
        float _maxTimeLoading = 0;

        [SerializeField]
        int _numberCondition = 0;

        [SerializeField]
        bool _isUseLoading = true;

        List<bool> _conditionDone = new List<bool>(); 

        [SerializeField]
        Slider _loading;

        [SerializeField]
        Text _loadingText;


        [SerializeField]
        UnityEvent _onDone = new UnityEvent();

        float _loadingMaxvalue;

        // Start is called before the first frame update


        private void Start()
        {
          
            Init(); 
        }

        public LoadingManager Init(UnityEvent onDone = null)
        {
            _conditionDone = new List<bool>();
            for (int i = 0; i< _numberCondition; i++)
            {
                _conditionDone.Add(false);
            }


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

                    _loading.onValueChanged.RemoveAllListeners();
                    _loading.transform.parent.gameObject.SetActive(false);
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
            Debug.Log("==> Force stop loading! <==");
          
            _loadingMaxvalue = _loading.maxValue - _loading.value;
            _maxTimeLoading = 1;
            
        }

        public LoadingManager AddOnDoneLoading(UnityAction callback)
        {
            if (_loading.value >= 99f)
            {
                callback?.Invoke();
                return this;
            }
            _onDone.AddListener(callback);
            return this;
        }

        public LoadingManager DoneCondition(int id)
        {
            if (id >= _conditionDone.Count)
            {
                Debug.LogError("==> ID condition not exist, check number of conditon on inspector! <==");
                return this;
            }

            if (_loading.value >= 99f)
            {
                Debug.LogWarning("==> Loading already stop, maybe check your game flow! <==");
                return this;
            }

            _conditionDone[id] = true;
            if (_conditionDone.Where(c => c == false).Count() == 0)
            {
                Debug.Log("==> All condition is done! stop loading! <==");
                StopLoading();
            }

            return this;
        }

        private void OnDrawGizmosSelected()
        {
            if (_isUseLoading)
            {
                _loading.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                _loading.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
