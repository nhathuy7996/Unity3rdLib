# DVAH3rdLib
## Integrated some 3rd party of Unity:
- [x] AD apploving (MAX) and some common adapter
- [x] Native Ad (using google admob)
- [x] Facebook
- [x] Firebase
- [x] Adjust
- [x] In-App purchase
- [x] Nointernet popup
- [x] NoAds popup
- [x] google Rate
- [x] Loading controller

#### --> Auto check missing file google-service.js <--
for Firebase service working correctly, file google-service.js must inside Assets folder

#### --> Auto check and warning package name is correct format <--
if project packgename not follow format "com.X.Y". It will cause some error on upload for google play store!


## Guide:
- Add git subtree on project and pull production (_develop branch may not working well yet!_)
- (Git Add Command: git subtree add --prefix Assets/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git develop --squash    )
- (Git Pull Command: git subtree pull --prefix Assets/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git develop --squash  )
 If you already add it, then pull command only is enought!
- Download SDK [from here](https://github.com/nhathuy7996/Unity3rdLib/releases) then import into project 
        (you should delete all the old lib Ad, google, firebase... before)
- Reimport All
- Android Resolve -> Force Resolve
- put prefab 3rdLib into scene index 0 on build setting (check dontDestroyOnload if project have multiple scene)

![put prefab 3rdLib into scene index 0 on build setting](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/1.png)

- If project contain IAP, then you must put IAP prefab as a child of 3rdLibManager (inside 3rdLib prefab) then click to assign sublib

- Feel free to add your code on CustomLib script

- For intergrated with our lib, using DVAH;

- Enter info of APERO checklist on menu 3rdLib/Checklist APERO
![checklist APERO](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/4.png)

## Loading System:
![Loading](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/3.png)
- We provided a loading systems. If you like to use, just click on Is Use Loading
- Loading using for many purpose, mostly is wait load ad open
#### ***For example:***
    public class CustomLib : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(waitAdOpen());
            Scene currentScene = SceneManager.GetActiveScene();
            AsyncOperation loadingScene = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            loadingScene.allowSceneActivation = true;
            LoadingManager.Instant.Init((conditionDone) =>
            {
                AdManager.Instant.ShowAdOpen(0,true, (isSuccess) =>
                {

                    _= SceneManager.UnloadSceneAsync(currentScene,UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                    _ = AdManager.Instant.InitializeBannerAds();
                    _= AdManager.Instant.ShowBanner();
                });
                _= AdManager.Instant.LoadAdOpen(1);
            });
        }


        IEnumerator waitAdOpen()
        {
            yield return new WaitUntil(()=> AdManager.Instant.AdsOpenIsLoaded());
            LoadingManager.Instant.DoneCondition(0);
        }


    }

- As you can see, our Loading systems contain a max time loading and a list boolean of condition

Call LoadingManager.Instant.DoneCondition(ID); for set a condition is success

if all condition is done then loading will stop and invoke callback, which already define in  LoadingManager.Instant.Init(callbackDone);

if some condition still false but loading reach max time then loading still stop and invoke call back. You can process on callback like this:

      LoadingManager.Instant.Init((conditionDone)=>{
              if(conditionDone.Where(t => t == false ).Any()){
                      //Some condition fail!

              }else{
                      //All condition done!
              }
      });

## Call function:
#### ***Buy product***
        IAPManager.Instant.BuyProductID("IDProduct", () =>
        {
            Debug.Log("Buy DOne!");
        }, () =>
        {
            Debug.Log("Buy Fail!");
        });

#### ***restore product, call ASAP*** 
        _= IAPManager.Instant.TryAddRestoreEvent("productID", () =>
        {
            Debug.Log("Restore Done!");
        });

        //---OR if you need check its OK ---
        var restoreSth = IAPManager.Instant.TryAddRestoreEvent("productID", () =>
        {
            Debug.Log("Restore Done!");
        });
        if (restoreSth.Result)
        {
            Debug.Log("It's OK");
        }

        // It's doesn't matter IAP init done or not

        // On IOS must add : 
        IAPManager.Instant.RestorePurchases();


#### ***Log event Firebase***
        FireBaseManager.Instant.LogEventWithParameter("Event_name_do_not_using_space", new Hashtable()
        {
            {
                "parameter",1
            }
        });

        //or

        FireBaseManager.Instant.LogEventWithOneParam("Event_name_do_not_using_space" );

#### ***Get value from remote config***
        _= FireBaseManager.Instant.GetValueRemoteAsync("key", (value) =>
        {
            int true_value = (int)value.LongValue;
        });



#### ***ShowPopUprate***
        MasterLib.Instant.ShowPopUpRate() //==> Show
        MasterLib.Instant.ShowPopUpRate(false) //==> Hide

#### ***AdManager***
        AdManager.Instant.ShowBanner();
        AdManager.Instant.DestroyBanner();
        //Call anywhwre you need show or hide banner, admanager auto load and show or hide!
        //On inspector, set bannerADID = blank if game dont have banner
        
        AdManager.Instant.ShowInterstitial((status) =>
        {
            //Do sth when user watched inter
            if (status == InterVideoState.Closed)
            {
                Debug.Log("Do sth!");
            }
            else
            {
                //Do sth when inter fail to show, not ready, etc....
            }


        }, showNoAds: true);
        // showNoAds: true ==> if you wanna show popup "AD not avaiable" - flase or just leave it blank if you dont need popup

        AdManager.Instant.ShowRewardVideo((status) =>
        {
            //Do sth when user watched reward
            if (status == RewardVideoState.Watched)
            {
                Debug.Log("Do sth!");
            }
            else
            {
                //Do sth when reward fail to show, not ready, etc....
            }


        }, showNoAds: true);
        // showNoAds: true ==> if you wanna show popup "AD not avaiable" - flase or just leave it blank if you dont need popup
        
        Some game have more than one ad open then, you must pass ID for loading/showing
         _= AdManager.Instant.LoadAdOpen(ID); 

            AdManager.Instant.ShowAdOpen(ID,true, (isSuccess) =>
            {
                
            }); // true then lib will know AdOpen treated as Ad open when open game, or as an AD when user return game.
            //You call check on callback Ad show success or not using isSuccess
            
        ![enter ID adopen](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/5.png)
