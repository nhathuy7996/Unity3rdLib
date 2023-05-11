using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using DVAH; 

public enum CONDITON_LOADING
{
    load_ad_open_done,
    load_native_done
}

public class CustomLib : MonoBehaviour
{
    Scene oldScene;
    // Start is called before the first frame update
    void Start()
    {
        //Wait until ad open load done then set first condition to true
        LoadingManager.Instant.DoneConditionSelf((int)CONDITON_LOADING.load_ad_open_done, ()=> AdManager.Instant.AdsOpenIsLoaded(0));

        //get current scene (loading scene) then after loading done -> show ad open -> user close -> unload scene loading
        oldScene = SceneManager.GetActiveScene();

        //Start running loading bar with 2 condition to skip loading before maxtime
        //condition 1 is loading ad open done
        //condition 2 is loading ad native ID 0 done or dont need load ad native ID 0
        LoadingManager.Instant.SetMaxTimeLoading(30).Init(2,LoadingCompleteCallback);

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
        AdManager.Instant.ShowAdOpen(0,true, (id,state) =>
        {
            // id mean adOpen id
            if (state == OpenAdState.None)
            {
                //adOpen show fail or something wrong
            }

            if (state == OpenAdState.Open)
            {
                //trigger callback when ad open start show
            }

            if (state == OpenAdState.Click)
            {
                //trigger callback when user click ad
            }

            if (state == OpenAdState.Closed)
            {
                //trigger when ad open close
            }

            SceneManager.UnloadSceneAsync(oldScene);
            AdManager.Instant.InitializeBannerAdsAsync();

            if (doneCondition[(int)CONDITON_LOADING.load_ad_open_done]) // It's mean when loading stop, ad open is load success
            {
                
            }

            if (doneCondition[(int)CONDITON_LOADING.load_native_done]) // It's mean when loading stop, ad native is load success or dont need to load ad native
            {

            }
        });
    }

    void CheckLoadNativeAd()
    {
        if (!PlayerPrefs.HasKey(CONSTANT.LANGUAGE_ID))
        {
            AdManager.Instant.LoadNativeADsAsync(0,1,2,3,4,5,6,7,8,9);
        }
        else
        {
            AdManager.Instant.LoadNativeADsAsync(1, 2, 3, 4, 5, 6, 7, 8, 9);
            // Incase user open app second time, language popup doesnt show up, then we should skip load ad native ID 0 to increase show rate
            LoadingManager.Instant.DoneCondition((int)CONDITON_LOADING.load_native_done);
        }

        //listen event from language manager,
        //if this is first open then langugage manager will notify event with data false
        //then we set native object which already created whenever adnative load success, to language panel
        //after that, mark condition 1 to true
        Observer.Instant.Subcribe(CONSTANT.LAN_1ST, (data) =>
        {
            if (!(bool)data)
            {
                AdManager.Instant.ShowNativeAsync(0, (nativePanel) =>
                {
                    nativePanel.transform.SetParent(LanguageManager.Instant.transform);
                    nativePanel.transform.localScale = Vector3.one;
                    nativePanel.transform.localPosition = Vector3.zero;
                    nativePanel.rectTransform.sizeDelta = Vector2.zero;
                    nativePanel.rectTransform.anchorMax = new Vector2(1, 0.4f);

                    LoadingManager.Instant.DoneCondition((int)CONDITON_LOADING.load_native_done);
                });
            }
        });
    }

    
}
