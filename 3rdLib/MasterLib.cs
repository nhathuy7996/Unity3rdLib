using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace HuynnLib
{
    public class MasterLib : Singleton<MasterLib>
{
        [SerializeField]
        bool _isAutoAssign = false;
        [SerializeField]
        bool _isAutoInit = false;
        [SerializeField]
        bool _isInitByOrder = false;
        [SerializeField]
        List<GameObject> _childLibs = new List<GameObject>();

        private void Awake()
        {
            if (!_isAutoInit)
                return;

            InitChildLib(() => { Debug.Log("=====> Init done all!"); });

            
        }

        public void InitChildLib(Action onAllInitDone = null)
        {
            if (_isInitByOrder)
            {
                Queue<IChildLib> orderInit = new Queue<IChildLib>();
                for (int  i = 0; i< _childLibs.Count; i++)
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

            List<GameObject> doneLib = new List<GameObject>();
            for (int i = 0; i < _childLibs.Count; i++)
            {
                _childLibs[i].GetComponent<IChildLib>().Init(() =>
                {
                    doneLib.Add(_childLibs[i].gameObject);
                });
            }
            StartCoroutine(WaitAllLibInitDone(doneLib, onAllInitDone));
            
        }

        IEnumerator WaitAllLibInitDone(List<GameObject> doneLib, Action onAllInitDone)
        {
            yield return new WaitUntil(() => doneLib.Count == _childLibs.Count);
            onAllInitDone?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_isAutoAssign)
                return;

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

            _isAutoAssign = false;
        }
    }


    public interface IChildLib
    {
        public void Init(Action onInitDone = null);
    }
}
