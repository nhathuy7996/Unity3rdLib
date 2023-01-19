using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuynnLib
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
    {

        private static T _instant = null;
        public static T Instant
        {
            get
            {
                if (_instant == null)
                {
                    Debug.LogError("==> Singleton doesnt exist!!! <==");
                    _instant = FindObjectOfType<T>();
                    //new GameObject().AddComponent<T>().name = "Singleton_"+  typeof(T).ToString();
                }

                return _instant;
            }
        }
       
        protected virtual void Awake()
        {
            if (_instant != null && _instant.GetInstanceID() != this.gameObject.GetInstanceID())
                Destroy(this.gameObject);
            else
                _instant = this.GetComponent<T>();
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}

