using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DVAH
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        [SerializeField]
        bool _isUseLoading = true;

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
        Text _loadingText, _versionText;

        Action<List<bool>> _onDone = null;

        float _loadingMaxvalue;
        bool _isLoadingStart = false;

        private void Start()
        {
            _versionText.text = "Version <color=red>" + Application.version+"</color>";
        }

        /// <summary>
        /// Start Loading bar with an action call back
        /// action will trigger when loading done (time out or done on condition)
        /// action get a list<bool> stand for list of condition done or not
        /// </summary>
        /// <param name="onDone">callback when all condition done or timeout</param>
        /// <returns> LoadingManager component</returns>
        public LoadingManager Init(Action<List<bool>> onDone = null)
        {
            if(_maxTimeLoading == 0.2f)
            {
                Debug.Log(CONSTANT.Prefix + $"==><color=yellow>Your time loading is to fast! You should call SetMaxTimeLoading first!</color><==");
            }
            _conditionDone.Clear();
            for (int i = 0; i < _numberCondition; i++)
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
                    Debug.Log(CONSTANT.Prefix + $"==>Loading Done!<==");
                    _loadingPopUp.SetActive(false);
                    _isLoadingStart = false;
                    try
                    {
                     
                        _ = FireBaseManager.Instant.LogEventWithParameter("screen_view_data", new Hashtable()
                        {
                            {"id_screen","loading_end" }
                        });

                        _ = FireBaseManager.Instant.LogEventWithParameter("screen_view_data", new Hashtable()
                        {
                            {"id_screen","lobby_start" }
                        });
                        _onDone?.Invoke(_conditionDone);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(CONSTANT.Prefix + $"==> callback ondone loading error: " + e.ToString() + " <==");
                    }

                    _loading.onValueChanged.RemoveAllListeners();

                }
            });

            _loadingPopUp.SetActive(true);
            _isLoadingStart = true;
            return this;
        }

        /// <summary>
        /// Start Loading bar with an action call back
        /// action will trigger when loading done (time out or done on condition)
        /// action get a list<bool> stand for list of condition done or not
        /// RECOMMEND: using enum to define condition ID
        /// </summary>
        /// <param name="numberCondition">number of condition which loading will wait all done then stop (in amout of time)</param>
        /// <param name="onDone">callback when all condition done or timeout</param>
        /// <returns></returns>
        public LoadingManager Init(int numberCondition, Action<List<bool>> onDone = null)
        {
            _numberCondition = numberCondition;
            return Init(onDone);
        }

        public LoadingManager SetMaxTimeLoading(float maxTime)
        {
            _maxTimeLoading = maxTime;
            return this;
        }

        // Update is called once per frame
        void Update()
        {
            _loading.value += _loadingMaxvalue * Time.deltaTime / _maxTimeLoading;
            _loadingText.text = string.Format("{0:0.0}%", _loading.value); 
        }

        /// <summary>
        /// Stop loading immediately
        /// </summary>
        public void StopLoading()
        { 
            if (_maxTimeLoading == 0.2f)
                return;
            Debug.Log(CONSTANT.Prefix + $"==> <color=red>Force stop loading!</color> <==");
            _loadingMaxvalue = _loading.maxValue - _loading.value;
            _maxTimeLoading = 0.2f;

        }

        /// <summary>
        /// add callback when loading done
        /// </summary>
        /// <param name="callback">callback invoke when loading done</param>
        /// <returns></returns>
        public LoadingManager AddOnDoneLoadingAsync(Action<List<bool>> callback)
        {
            if (_loading.value >= 99f)
            {
                callback?.Invoke(_conditionDone);
                return this;
            }
            _ = AddDoneLoading(callback);
            return this;
        }

        async Task AddDoneLoading(Action<List<bool>> callback)
        {
            float timer = 0;
            while (!_isLoadingStart && timer < 240000)
            {
                await Task.Delay(500);
                timer += 500;
            }

            _onDone += (callback);
        }

        /// <summary>
        /// Using this to set a condition is done. If all condition is done,
        /// then loading will stop immediately
        /// </summary>
        /// <param name="id">ID of condition you wanna mark done</param>
        /// <returns></returns>
        public LoadingManager DoneCondition(int id)
        {
            _ = waitToSetDoneCondition(id);
            return this;
        }

        /// <summary>
        /// Using this to wait and set a condition is done. If all condition is done,
        /// then loading will stop immediately
        /// It simmilar using Coroutine to wait
        /// </summary>
        /// <param name="id">ID of condition you wanna mark done</param>
        /// <param name="predicate">like wait until of coroutine</param>
        /// <returns>LoadingManager</returns>
        /// <code>
        /// LoadingManager.Instant.DoneConditionSelf(0,()=> AdManager.Instant.AdsOpenIsLoaded(0));
        /// </code>
        public LoadingManager DoneConditionSelf(int id, Func<bool> predicate)
        {
            _ = waitSelfDoneCondition(id, predicate);
            return this;
        }

        async Task waitSelfDoneCondition(int id, Func<bool> predicate)
        {
            try
            {
                float timer = 0;
                do
                {
                    await Task.Delay(500);
                    timer += 500;

                    if (timer > 240000)
                        break;

                } while (!_isLoadingStart || !predicate.Invoke());
            }
            catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> Error on invoke predicate waitSelfDoneCondition, error: {e.ToString()}! <==");
            }
           

            await Task.Delay(1500);
            DoneCondition(id);
        }

        async Task waitToSetDoneCondition(int id)
        {
            while (!_isLoadingStart)
            {
                await Task.Delay(500);
            }

            if (id >= _conditionDone.Count)
            {
                Debug.LogError(CONSTANT.Prefix + $"==> ID condition not exist, check number of conditon on inspector! <==");
                return;
            }

            if (_loading.value >= 99f)
            {
                Debug.LogWarning(CONSTANT.Prefix + $"==> Loading already stop, maybe check your game flow! <==");
                return;
            }

            _conditionDone[id] = true;
            if (_conditionDone.Where(c => c == false).Count() == 0)
            {
                Debug.Log(CONSTANT.Prefix + $"==> All condition is done! stop loading! <==");
                StopLoading();
            }
        }

        private void OnDrawGizmosSelected()
        {
     
            if (_isUseLoading && !Application.isPlaying)
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
