using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using DVAH;

public class CustomLib : MonoBehaviour
{
    Scene oldScene;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(waitLoadOpenAd());
        oldScene = SceneManager.GetActiveScene();

        _= AdManager.Instant.LoadNativeADs(0);

        LoadingManager.Instant.Init(LoadingCompleteCallback);

        SceneManager.LoadSceneAsync(1);
    }

    void LoadingCompleteCallback(List<bool> doneCondition)
    {
        //AdManager.Instant.SetAdNativeKeepReload(0, false);
        AdManager.Instant.ShowAdOpen(0,true, (isSuccess) =>
        {
            SceneManager.UnloadSceneAsync(oldScene);
            _= AdManager.Instant.InitializeBannerAds();
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
