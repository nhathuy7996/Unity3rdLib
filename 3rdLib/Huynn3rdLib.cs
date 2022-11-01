using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuynnLib
{
    public class Huynn3rdLib : MonoBehaviour
    {
        [SerializeField] bool _isShowDebug = false;
        [SerializeField] GameObject _notiDebug;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount < 3)
                return;

            if (Screen.width - Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) > Screen.width/4 )
            {
                return;
            }

            if (Screen.height - Vector2.Distance(Input.GetTouch(2).position, Input.GetTouch(1).position) > Screen.height / 4)
            {
                return;
            }

            if (!_notiDebug.activeSelf)
                _notiDebug.SetActive(true);
        }

        private void OnDrawGizmosSelected()
        {
            if (_notiDebug == null) _notiDebug = this.transform.GetChild(0).gameObject;
            _notiDebug.SetActive(_isShowDebug);
        }
    }
}

