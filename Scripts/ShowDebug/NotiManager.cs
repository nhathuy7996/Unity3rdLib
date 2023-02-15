using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace DVAH
{
    public class NotiManager : MonoBehaviour
    {
        private static NotiManager instant = null;
        public static NotiManager Instant => instant;

        [SerializeField]
        RectTransform _panelNoti;

        [SerializeField]
        Text Context = null;

        void Awake()
        {
            instant = this;
            Context = this.GetComponentInChildren<Text>(true);

            if(AdManager.Instant.isAdBanner && AdManager.Instant.BannerPosition.ToString().StartsWith("Bottom"))
                _panelNoti.offsetMin = new Vector2(_panelNoti.offsetMin.x, 320 );
        }
        // Start is called before the first frame update
        public void Log(string S)
        {
            Context.text += "\n";
            Context.text +=  S;
        }
    }
}

