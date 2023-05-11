using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DVAH
{

    public class Observer : Singleton<Observer>
    {
        Dictionary<string, List<Action<object>>> Listeners = new Dictionary<string, List<Action<object>>>();


        public Observer Subcribe(string key, Action<object> callback)
        {
            if (!Listeners.ContainsKey(key))
            {
                Listeners[key] = new List<Action<object>>();
            }

            Listeners[key].Add(callback);

            return this;
        }

        public Observer UnSubcribe(string key, Action<object> callback)
        {
            if (!Listeners.ContainsKey(key))
            {
                return this;
            }
            Listeners[key].Remove(callback);

            return this;
        }

        public Observer Notify(string key, object value)
        {
            if (!Listeners.ContainsKey(key))
            {
                return this;
            }
            foreach (var callback in Listeners[key])
            {
                try
                {
                    callback?.Invoke(value);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat(CONSTANT.Prefix + "====>Notify action on key {0} error: {1}<====", key, e.ToString());
                }
               
            }
            return this;
        }
    }

 
}


