using Firebase;
using Firebase.Messaging;
using Firebase.Analytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using ConfigValue = Firebase.RemoteConfig.ConfigValue;
using Firebase.RemoteConfig;
using System.ComponentModel;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.Device;

namespace HuynnLib
{
    public class FireBaseManager : Singleton<FireBaseManager>, IChildLib
    {

        #region For AD event

        AD_TYPE _adTypeLoaded = AD_TYPE.open;
        public AD_TYPE adTypeShow = AD_TYPE.resume;

        #endregion

        private bool _isFetchDone = false;

        public bool isFetchDOne => _isFetchDone;

        private Dictionary<string, ConfigValue> _keyConfigs = new Dictionary<string, ConfigValue>();


        Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;


        public void Init(Action _onActionDone)
        {
            Debug.Log("==========> Firebase start Init! <==========");
#if UNITY_EDITOR
            _onActionDone?.Invoke();
#endif
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                    InitializeFirebase();

                    _onActionDone?.Invoke();
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "==> Could not resolve all Firebase dependencies: {0} <==", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.

                    _onActionDone?.Invoke();
                }
            });
        }


        void InitializeFirebase()
        {
            System.Collections.Generic.Dictionary<string, object> defaults =
                new System.Collections.Generic.Dictionary<string, object>();
            FetchFireBase();
        }

        public void FetchFireBase()
        {
            FetchDataAsync();
        }


        public async Task GetValueRemoteAsync(string key, Action<ConfigValue> waitOnDone = null) 
        {
            while (!_isFetchDone)
                await Task.Delay(1000);

            var obj = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            waitOnDone?.Invoke(obj);
        }

        // Start a fetch request.
        public Task FetchDataAsync()
        {
            Debug.Log("Fetching data...");
            // FetchAsync only fetches new data if the current data is older than the provided
            // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
            // By default the timespan is 12 hours, and for production apps, this is a good
            // number.  For this example though, it's set to a timespan of zero, so that
            // changes in the console will always show up immediately.
            System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWith(FetchComplete);
        }

        void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log("==> Fetch canceled. <==");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("==> Fetch encountered an error <==");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("==> Fetch completed successfully! <==");
            }

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    _isFetchDone = true;
                    Debug.Log(String.Format("==> Remote data loaded and ready (last fetch time {0}).<==",
                        info.FetchTime));

                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.Log("==> Fetch failed for unknown reason <==");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.Log("==> Fetch throttled until " + info.ThrottledEndTime+" <==");
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Debug.Log("==> Latest Fetch call still pending. <==");
                    break;
            }
        }

        #region Firebase Logevent
        public void LogEventWithOneParam(string eventName)
        {
            Debug.LogError("==> LogEvent " + eventName+" <==");
            _= this.LogEventWithParameter(eventName, new Hashtable() { { "value", 1 } });

        }

        public async Task LogEventWithParameter(string event_name, Hashtable hash)
        {
            while (!_isFetchDone)
                await Task.Delay(1000);
            Firebase.Analytics.Parameter[] parameter = new Firebase.Analytics.Parameter[hash.Count];
            //List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();
            if (hash != null && hash.Count > 0)
            {
                int i = 0;
                foreach (DictionaryEntry item in hash)
                {
                    if (item.Equals((DictionaryEntry)default)) continue;
                    parameter[i] = (new Firebase.Analytics.Parameter(item.Key.ToString(), item.Value.ToString()));
                    Debug.Log("==> LogEvent " + event_name.ToString() + "- Key = " + item.Key + " -  Value =" + item.Value + " <==");
                    i++;
                }

                Firebase.Analytics.FirebaseAnalytics.LogEvent(
                           event_name,
                           parameter);
            }
        }


        #endregion

        #region FIREBASE CUSTOM EVENT

        /// <summary>
        ///   state: Trạng thái của level sau khi người chơi chơi qua
        /// </summary>
        public void LogEventLevel(int level, LEVEL_STATE_EVENT state)
        {
            _= this.LogEventWithParameter(state.ToString(), new Hashtable()
            {
                {"id_level", level}
            });
        }

        ///<param name="name">tên button</param>
        ///<param name="screen">vị trí màn hình của user</param>
        ///<param name="level">level hiện tại (nếu trong gameplay) hoặc đã pass (nếu ngoài gameplay) của user</param>
        ///<param name="customParam">Hậu tố nếu cần thêm</param>
        /// <summary>
        /// Log event when user click a button
        /// <code>
        ///  name: "tên button"
        ///  screen: "vị trí màn hình của user",
        ///  level: "level hiện tại (nếu trong gameplay) hoặc đã pass (nếu ngoài gameplay) của user"
        ///  customParam: "Hậu tố nếu cần thêm"
        /// </code>
        /// <example>
        /// For example:
        /// <code>
        ///     - add_new_melee_gameplay_4 : user thêm unit melee trong gameplay tại level 4
        ///     - claim_x2_speed_gameplay_15 : user click x2 speed trong gameplay tại level 15
        /// </code> 
        /// </example>
        /// </summary>
        public void LogClickBtnEvent(string name, string screen, int level = -1, string customParam = "")
        {
            _ = this.LogEventWithParameter("btn_click", new Hashtable()
            {
                {"id_click",string.Format("{0}_{1}",name, screen) + (level < 0 ?"":"_"+level) + (string.IsNullOrWhiteSpace(customParam)?"":"_"+customParam )}
            });
        }


        ///<param name="name">tên button</param>
        ///<param name="screen">vị trí màn hình của user</param>
        ///<param name="level">level hiện tại (nếu trong gameplay) hoặc đã pass (nếu ngoài gameplay) của user</param>
        ///<param name="customParam">Hậu tố nếu cần thêm</param>
        /// <summary>
        /// Log event when user click button AD
        /// <code>
        ///  name: "tên button"
        ///  screen: "vị trí màn hình của user",
        ///  level: "level hiện tại (nếu trong gameplay) hoặc đã pass (nếu ngoài gameplay) của user"
        ///  customParam: "Hậu tố nếu cần thêm"
        /// </code>
        /// <example>
        /// For example:
        /// <code>
        ///     - add_new_melee_gameplay_4 : user xem ad reward thêm unit melee trong gameplay tại level 4
        ///     - claim_x2_speed_gameplay_15 : user xem ad reward x2 speed trong gameplay tại level 15
        /// </code> 
        /// </example>
        /// </summary>
        public void LogClickRewardBtnEvent(string name, string screen, int level= -1, string customParam = "")
        {
            _ = this.LogEventWithParameter("reward_ad_on_click", new Hashtable()
            {
                 {"id_click",string.Format("{0}_{1}",name, screen) + (level < 0 ?"":"_"+level) + (string.IsNullOrWhiteSpace(customParam)?"":"_"+customParam )}
            });
        }

        public void LogADEvent(AD_TYPE adType, AD_STATE adState)
        {
            _ = this.LogEventWithParameter("ad_event", new Hashtable()
            {
                 {"ad_load_stats",string.Format( "ad_{0}_{1}",adType.ToString(), adState.ToString() )}
            });
        }


        public void LogADResumeEvent( AD_STATE adState)
        {
            AD_TYPE adType = this._adTypeLoaded;
              
            if (adState == AD_STATE.show)
            {
                if (adTypeShow == AD_TYPE.open)
                    adState = AD_STATE.show_open;
                else
                    adState = AD_STATE.show_resume;


                this._adTypeLoaded = AD_TYPE.resume;
                adTypeShow = AD_TYPE.resume;
            }

            if (adState == AD_STATE.show_fail)
            {
                if (adTypeShow == AD_TYPE.open)
                    adState = AD_STATE.show_open_fail;
                else
                    adState = AD_STATE.show_resume_fail;

                this._adTypeLoaded = AD_TYPE.resume;
                adTypeShow = AD_TYPE.resume;
            }

            LogADEvent(adType,adState); 
        }

        #endregion

    }
}


public enum LEVEL_STATE_EVENT
{
    start_level,
    fail_level,
    win_level
}

public enum AD_TYPE
{
    open,
    resume,
    banner,
    inter,
    reward
}

public enum AD_STATE
{
    load,
    load_done,
    load_fail,
    show,
    show_fail,
    show_open,
    show_open_fail,
    show_resume,
    show_resume_fail,
}