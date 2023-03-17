using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
    public class AdNativeObject : MonoBehaviour
    {
        public RawImage adIcon, adChoice, adBGRawImage;
        public GameObject adBGFitter;
        public AspectRatioFitter adBGAspect;

        public Transform adBGManager => adBGFitter.transform.parent;
     
        public Text callToAction, advertiser, headLine, body, price, store;

        public RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();
            adBGRawImage = adBGFitter.GetComponentInChildren<RawImage>();
            adBGAspect = adBGFitter.GetComponentInChildren<AspectRatioFitter>(); 
        }

        public GameObject setAdBG(Texture2D tex = null)
        {
            float aspect = tex == null? 1: tex.width / tex.height;
            adBGAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            adBGAspect.aspectRatio = aspect;

            adBGRawImage.texture = tex;

            return adBGFitter;
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

            Transform adBGParent = adBGFitter.transform.parent;
            for (int i = 0; i < adBGParent.childCount; i++)
            {
                adBGParent.GetChild(i).GetComponent<BoxCollider2D>().size = adBGParent.GetChild(i).GetComponent<RectTransform>().rect.size;
            }
        }

    }
}
