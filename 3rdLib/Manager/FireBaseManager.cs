using Firebase;
using Firebase.Messaging;
using Firebase.Analytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using ConfigValue = Firebase.RemoteConfig.ConfigValue;

namespace HuynnLib
{
    public class FireBaseManager : Singleton<FireBaseManager>, IChildLib
    {

        private bool _isFetchDone = false;

        public bool isFetchDOne => _isFetchDone;

        private Dictionary<string, ConfigValue> _keyConfigs = new Dictionary<string, ConfigValue>();


        Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;


        public void Init(Action _onActionDone)
        {
            Debug.Log("==========> Firebase start Init!");
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
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
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


        public void GetValueRemote(string key, Action<object> waitOnDone = null)
        {
           
            if (!_isFetchDone)
            {
                StartCoroutine(GetValueOnDone(key,waitOnDone));
                return;
            }

            if (_keyConfigs.ContainsKey(key))
                waitOnDone?.Invoke(_keyConfigs[key]);
            else
                Debug.LogError("Remote firebase doesnt containt key " + key);
        }

        IEnumerator GetValueOnDone(string key, Action<object> onDone)
        {
            yield return new WaitUntil(()=> _isFetchDone);
            if (_keyConfigs.ContainsKey(key))
                onDone?.Invoke(_keyConfigs[key]);
            else
                Debug.LogError("Remote firebase doesnt containt key "+key);
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
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case Firebase.RemoteConfig.LastFetchStatus.Success:
                    Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    _isFetchDone = true;
                    Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                        info.FetchTime));

                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case Firebase.RemoteConfig.FetchFailureReason.Error:
                            Debug.Log("Fetch failed for unknown reason");
                            break;
                        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case Firebase.RemoteConfig.LastFetchStatus.Pending:
                    Debug.Log("Latest Fetch call still pending.");
                    break;
            }
        }
        #region Firebase Logevent
        public void LogEventWithOneParam(string eventName)
        {
            Debug.LogError("LogEvent " + eventName);
            this.LogEventWithParameter(eventName, new Hashtable() { { "value", 1 } });

        }

        public void LogEventWithParameter(string event_name, Hashtable hash)
        {
            StartCoroutine(waitInitDone(event_name, hash));
        }

        IEnumerator waitInitDone(string event_name, Hashtable hash)
        {
            yield return new WaitUntil(() => _isFetchDone);
            Firebase.Analytics.Parameter[] parameter = new Firebase.Analytics.Parameter[hash.Count];
            //List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();
            if (hash != null && hash.Count > 0)
            {
                int i = 0;
                foreach (DictionaryEntry item in hash)
                {
                    if (item.Equals((DictionaryEntry)default)) continue;
                    parameter[i] = (new Firebase.Analytics.Parameter(item.Key.ToString(), item.Value.ToString()));
                    Debug.Log("LogEvent " + event_name.ToString() + "- Key = " + item.Key + " -  Value =" + item.Value);
                    i++;
                }

                Firebase.Analytics.FirebaseAnalytics.LogEvent(
                           event_name,
                           parameter);
            }
        }


        #endregion
    }
}
