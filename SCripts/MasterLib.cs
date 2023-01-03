using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace HuynnLib
{
    public class MasterLib : MonoBehaviour
    {

      
        bool _isAutoInit = (Huynn3rdLib.Instant != null) ? Huynn3rdLib.Instant.isAutoInit : false;
     
        bool _isInitByOrder = (Huynn3rdLib.Instant != null) ? Huynn3rdLib.Instant.isInitByOrder : false;

        List<GameObject> _childLibs = (Huynn3rdLib.Instant != null) ? Huynn3rdLib.Instant.ChildLibs : new List<GameObject>();

        [SerializeField]
        List<GameObject> _doneLib = new List<GameObject>(); //Runtime check
        [SerializeField] GameObject _popUpRate;

        private void Awake()
        {
            if (!_isAutoInit)
                return;

            InitChildLib(() => { Debug.Log("=====> Init done all! <====="); });


        }

        public void InitChildLib(Action onAllInitDone = null)
        {
            
            if (_isInitByOrder)
            {
                Queue<IChildLib> orderInit = new Queue<IChildLib>();
                

                for (int i = 0; i < _childLibs.Count; i++)
                {
                    orderInit.Enqueue(_childLibs[i].GetComponent<IChildLib>());
                }

                Action<IChildLib> onInitDone = null;

                onInitDone = (childLib) =>
                {

                    childLib.Init(() =>
                    {
                        if (orderInit.Count != 0)
                            onInitDone.Invoke(orderInit.Dequeue());
                        else
                            onAllInitDone?.Invoke();
                    });
                };

                onInitDone.Invoke(orderInit.Dequeue());


                return;
            }


            for (int i = 0; i < _childLibs.Count; i++)
            {
                GameObject g = _childLibs[i].gameObject;
                try
                {
                    _childLibs[i].GetComponent<IChildLib>()?.Init(() =>
                    {
                        _doneLib.Add(g);
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("==> Init child lib {0} error: {1} <==", g.name, e.ToString()));
                }

            }
            StartCoroutine(WaitAllLibInitDone(_doneLib, onAllInitDone));

        }

        IEnumerator WaitAllLibInitDone(List<GameObject> doneLib, Action onAllInitDone)
        {
            yield return new WaitUntil(() => doneLib.Count == _childLibs.Count);
            onAllInitDone?.Invoke();
        }

 

        public List<GameObject> GetChildLib()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Transform Ichild = this.transform.GetChild(i);
                if (Ichild.GetComponent<IChildLib>() == null)
                    DestroyImmediate(Ichild.gameObject);
            }

            this._childLibs.Clear();
            IChildLib[] childLib = this.GetComponentsInChildren<IChildLib>();

            for (int i = 0; i < childLib.Count(); i++)
            {

                _childLibs.Add(this.transform.GetChild(i).gameObject);
            }

            return _childLibs;
        }

    
    }


    public interface IChildLib
    {
        public void Init(Action onInitDone = null);
    }
}
