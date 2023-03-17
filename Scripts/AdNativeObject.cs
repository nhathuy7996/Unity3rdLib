using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
    public class AdNativeObject : MonoBehaviour
    {
        public RawImage adIcon, adChoice;
        public GameObject adBG;
        public Text callToAction, advertiser, headLine, body, price, store;

        public RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();
            
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

            Transform adBGParent = adBG.transform.parent;
            for (int i = 0; i < adBGParent.childCount; i++)
            {
                adBGParent.GetChild(i).GetComponentInChildren<BoxCollider2D>().size = adBGParent.GetChild(i).GetComponentInChildren<RawImage>().rectTransform.rect.size;
            }
        }

    }
}
