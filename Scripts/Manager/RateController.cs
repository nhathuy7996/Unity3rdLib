using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Play.Review;
using UnityEngine.Events;

namespace DVAH
{
    public class RateController : MonoBehaviour
    {
        [SerializeField] float _delayTimeShowNoButton;
        [SerializeField] GameObject _noThankButton;
        [SerializeField] Transform _starManTrans;
        [SerializeField] List<GameObject> _starManager = new List<GameObject>();

        int _starRate = 5;

        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;


        Coroutine _waitShowNoThank;

        private void Awake()
        {
            for (int i = 0; i < _starManTrans.transform.childCount; i++)
            {
                _starManTrans.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                _starManager.Add(_starManTrans.transform.GetChild(i).GetChild(0).gameObject);
            }
        }


        private void OnEnable()
        {
            _waitShowNoThank = StartCoroutine(WaitShowNoThank());
        }


        private void OnDisable()
        {
            if(_waitShowNoThank != null)
                StopCoroutine(_waitShowNoThank);
        }

        IEnumerator WaitShowNoThank()
        {
            yield return new WaitForSeconds(_delayTimeShowNoButton);
            _noThankButton.SetActive(true);
        }

        public void ClickChoose(Transform t)
        {


            for (int i = 0; i < _starManager.Count; i++)
            {
                if (i <= t.GetSiblingIndex())
                    _starManager[i]?.SetActive(true);
                else
                    _starManager[i]?.SetActive(false);
            }



            _starRate = t.GetSiblingIndex();


        }

        public void submitRate()
        {
            PlayerPrefs.SetInt(CONSTANT.RATE_CHECK, 1);
            if (_starRate >= 4)
            {
#if UNITY_ANDROID
                StartCoroutine(RequestReviews(() =>
                {
                    this.gameObject.SetActive(false);
                }));

                
#elif UNITY_EDITOR
            this.gameObject.SetActive(false);
#endif
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }


        IEnumerator RequestReviews(UnityAction afterRateAction)
        {
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----1 <==");
            _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----2 <==");
            yield return requestFlowOperation;
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----3 <==");
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                // Log error. For example, using requestFlowOperation.Error.ToString().
                Debug.LogError(CONSTANT.Prefix + $"==> requestFlowOperation-----4-error: " + requestFlowOperation.Error.ToString() + " <==");
                yield break;
            }
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----5 <==");
            _playReviewInfo = requestFlowOperation.GetResult();

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----6 <==");
            yield return launchFlowOperation;
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----7 <==");
            _playReviewInfo = null; // Reset the object
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                // Log error. For example, using requestFlowOperation.Error.ToString().
                Debug.LogError(CONSTANT.Prefix + $"==> launchFlowOperation-----8-error: " + launchFlowOperation.Error.ToString() + " <==");
                yield break;
            }
            Debug.Log(CONSTANT.Prefix + $"==> RequestReviews-----9 <==");
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
            afterRateAction?.Invoke();
        }


    }

}