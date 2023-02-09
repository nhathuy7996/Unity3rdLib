using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DVAH
{
    internal class UnityMainThread : MonoBehaviour
    {
        internal static UnityMainThread wkr;
        Queue<Action> jobs = new Queue<Action>();

        void Awake()
        {
            wkr = this;
        }

        void Update()
        {
            while (jobs.Count > 0)
                try
                {
                    jobs.Dequeue()?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("==> Error on invoke some action on main thread!:"+e.ToString()+" <==");
                }
        }

        internal void AddJob(Action newJob)
        {
            jobs.Enqueue(newJob);
        }
    }
}