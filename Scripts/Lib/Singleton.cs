using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace DVAH
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
                    Debug.LogWarning(CONSTANT.Prefix + "==> Singleton doesnt exist!!! <==");
                    _instant = FindObjectOfType<T>();
                    //if (B == true)
                    //{
                    //    new GameObject().AddComponent<T>().name = "Singleton_" + typeof(T).ToString();
                    //    Debug.LogWarning(CONSTANT.Prefix + "==> Auto create "+typeof(T).Name+" !!! <==");
                    //}
                }
                
                return _instant;
            }
        } 
       
        protected virtual void Awake()
        {
            if (_instant != null && _instant.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
                Destroy(this.gameObject);
            else
                _instant = this.GetComponent<T>();
        }
                 
       
    }
}

