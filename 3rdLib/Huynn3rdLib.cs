using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuynnLib
{
    public class Huynn3rdLib : MonoBehaviour
    {
        [SerializeField] bool _isShowDebug = false, _isDontDestroyOnLoad = false;
        [SerializeField] GameObject _notiDebug;

        int _devTapCount = 0;

        // Start is called before the first frame update
        void Start()
        {
            if(_isDontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isShowDebug || _notiDebug == null)
                return;
            if (Input.touchCount < 3)
            {
                if(_devTapCount != 0)
                    _devTapCount = 0;
                return;
            }

            if (Screen.width - Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) > Screen.width/4 )
            {
                return;
            }

            if (Screen.height - Vector2.Distance(Input.GetTouch(2).position, Input.GetTouch(1).position) > Screen.height / 4)
            {
                return;
            }

            if (Input.touchCount == 4 && Input.GetTouch(3).phase == TouchPhase.Ended)
                _devTapCount++;

            if (_devTapCount < 5)
                return;

            if (!_notiDebug.activeSelf)
                _notiDebug.SetActive(true);
        }

        public void CloseApplication()
        {
            Application.Quit();
        }

        private void OnDrawGizmosSelected()
        {
            if (_notiDebug == null && this.transform.GetChild(0).GetComponent<NotiManager>() != null)
            {
                _notiDebug = this.transform.GetChild(0).gameObject;
            }
            _notiDebug.SetActive(_isShowDebug);
        }
    }
}

