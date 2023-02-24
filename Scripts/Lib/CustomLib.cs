using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using DVAH;
using static Facebook.Unity.FB;

public class CustomLib : MonoBehaviour
{
    Scene oldScene;
    // Start is called before the first frame update
    void Start()
    {
        //Wait until ad open load done then set first condition to true
        StartCoroutine(waitLoadOpenAd());

        //get current scene (loading scene) then after loading done -> show ad open -> user close -> unload scene loading
        oldScene = SceneManager.GetActiveScene();

        //Start running loading bar with 2 condition to skip loading before maxtime 
        LoadingManager.Instant.Init(2,LoadingCompleteCallback);

        //Start loadscene Gameplay async and additive
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        //Check if is this first open of user, then start load ad native and wait until adnative load done.
        //after that, mark second condition to true,
        //Otherwise mark mark second condition to true right away
        CheckLoadNativeAd();
    }

    void LoadingCompleteCallback(List<bool> doneCondition)
    {
        AdManager.Instant.SetAdNativeKeepReload(0, false);
        AdManager.Instant.ShowAdOpen(0,true, (isSuccess) =>
        {
            SceneManager.UnloadSceneAsync(oldScene);
            _= AdManager.Instant.InitializeBannerAds();
        });
    }

    void CheckLoadNativeAd()
    {
        if (!PlayerPrefs.HasKey(CONSTANT.LANGUAGE_ID))
        {
            _ = AdManager.Instant.LoadNativeADs(0,1,2,3,4,5,6,7,8,9);
        }
        else
        {
            _ = AdManager.Instant.LoadNativeADs(1, 2, 3, 4, 5, 6, 7, 8, 9);
            LoadingManager.Instant.DoneCondition(1);
        }

        //listen event from language manager,
        //if this is first open then langugage manager will notify event with data false
        //then we set native object which already created whenever adnative load success, to language panel
        //after that, mark condition 1 to true
        Observer.Instant.Subcribe(CONSTANT.LAN_1ST, (data) =>
        {
            if (!(bool)data)
            {
                _ = AdManager.Instant.ShowNative(0, (nativePanel) =>
                {
                    nativePanel.transform.SetParent(LanguageManager.Instant.transform);
                    nativePanel.transform.localScale = Vector3.one;
                    nativePanel.transform.localPosition = Vector3.zero;
                    nativePanel.rectTransform.sizeDelta = Vector2.zero;
                    nativePanel.rectTransform.anchorMax = new Vector2(1, 0.4f);

                    LoadingManager.Instant.DoneCondition(1);
                });
            }
        });
    }

    IEnumerator waitLoadOpenAd()
    {
        yield return new WaitUntil(() => AdManager.Instant.AdsOpenIsLoaded(0));
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        LoadingManager.Instant.DoneCondition(0);
    }
  
}
