using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HuynnLib
{

    public interface ActionBase
    {
        public void CallListener(params object[] objects);
    }

    public class Observer : Singleton<Observer>
    {
        Dictionary<string, Action<object[]>> Listeners = new Dictionary<string, Action<object[]>>();


        private void Start()
        {
           
        }
    }

    
}
