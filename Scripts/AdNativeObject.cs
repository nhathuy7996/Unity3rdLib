using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
    public class AdNativeObject : MonoBehaviour
    {
        public RawImage adIcon, adChoice;
        public GameObject adBGFitter;

        public Transform adBGManager => adBGFitter.transform.parent;

        public Text callToAction, advertiser, headLine, body, price, store;

        public RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();

        }

        private void CheckCanvas()
        {
            Transform parent = this.transform;
            Canvas c = null;
            parent.TryGetComponent<Canvas>(out c);

            while (parent != null && c == null)
            {
                parent = parent.parent;
                if (parent == null)
                    break;
                parent.TryGetComponent<Canvas>(out c);
            }
            if (c == null || c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                string error = CONSTANT.Prefix + $"====>If native object doesnt on canvas OR canvas using RenderMode.ScreenSpaceOverlay, " +
                    "then native AD not clickable which make your impression not record eventhought you saw native AD show up on editor/device!!! <====";
                Debug.LogError(error);
                callToAction.text = error;
                advertiser.text = error;
                headLine.text = error;
                body.text = error;

                callToAction.color = Color.red;
                advertiser.color = Color.red;
                headLine.color = Color.red;
                body.color = Color.red;
            }
        }

        public List<GameObject> setAdBG(List<Texture2D> texs)
        {
            List<GameObject> BGs = new List<GameObject>();
#if UNITY_EDITOR
            for (int i = 0; i < 3; i++)
            {

                GameObject bg;
                if (i >= this.adBGManager.childCount)
                {
                    bg = Instantiate(this.adBGFitter, this.adBGFitter.transform.position,
                       Quaternion.identity, this.adBGManager);
                }
                else
                {
                    bg = this.adBGManager.GetChild(i).gameObject;
                }
                float aspect = 1;

                bg.GetComponentInChildren<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                bg.GetComponentInChildren<AspectRatioFitter>().aspectRatio = aspect;
                bg.GetComponentInChildren<RawImage>().texture = null;
                bg.SetActive(true);

                BGs.Add(bg.transform.GetChild(0).gameObject);
            }
#else
            for (int i = 0; i< texs.Count; i++) {

                Texture2D tex = texs[i];
                GameObject bg;
                if (i >= this.adBGManager.childCount) {
                    bg = Instantiate(this.adBGFitter, this.adBGFitter.transform.position,
                       Quaternion.identity, this.adBGManager);
                } else {
                    bg = this.adBGManager.GetChild(i).gameObject;
                }
                float aspect = texs[i].width / texs[i].height;

                bg.GetComponentInChildren<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                bg.GetComponentInChildren<AspectRatioFitter>().aspectRatio = aspect; 

                bg.GetComponentInChildren<RawImage>().texture = tex;
                bg.SetActive(true);

                BGs.Add(bg.transform.GetChild(0).gameObject);
            }
#endif 

            return BGs;
        }

        public void FitCollider()
        {
            StartCoroutine(wait());
        }

        IEnumerator wait()
        {
            yield return new WaitForEndOfFrame();
            adIcon.GetComponent<BoxCollider2D>().size = adIcon.rectTransform.rect.size;
            adChoice.GetComponent<BoxCollider2D>().size = adChoice.rectTransform.rect.size;

            callToAction.GetComponent<BoxCollider2D>().size = callToAction.rectTransform.rect.size;
            advertiser.GetComponent<BoxCollider2D>().size = advertiser.rectTransform.rect.size;
            headLine.GetComponent<BoxCollider2D>().size = headLine.rectTransform.rect.size;
            body.GetComponent<BoxCollider2D>().size = body.rectTransform.rect.size;
            price.GetComponent<BoxCollider2D>().size = price.rectTransform.rect.size;
            store.GetComponent<BoxCollider2D>().size = store.rectTransform.rect.size;


            for (int i = 0; i < adBGManager.childCount; i++)
            {
                adBGManager.GetChild(i).GetChild(0).GetComponent<BoxCollider2D>().size = adBGManager.GetChild(i).GetChild(0).GetComponent<RectTransform>().rect.size;
            }

            CheckCanvas();
        }

    }
}
