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
- [x] Language controller

#### --> Auto check missing file google-service.js <--
for Firebase service working correctly, file google-service.js must inside Assets folder

#### --> Auto check and warning package name is correct format <--
if project packgename not follow format "com.X.Y". It will cause some error on upload for google play store!

#### --> Auto push branch production <--
After build aab, a branch production will auto create and push your code to there!


## Guide:
- Download SDK [from here](https://github.com/nhathuy7996/Unity3rdLib/releases) then import into project 
        (you should delete all the old lib Ad, google, firebase... before) and [Demo project here](https://github.com/nhathuy7996/DemoLib)
- I'm recommend your build setting like this:
![build setting](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/buildSetting.png)
- Reimport All
- Android Resolve -> Force Resolve
- Then you have 2 way to add the code lib to your project:
    - The lazy way:
        ![add Code](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/0.png)
    - The profession way:
        - Add git subtree on project and pull production (_develop branch may not working well yet!_)
        - (Git Add Command: git subtree add --prefix Assets/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git production --squash    )
        - (Git Pull Command: git subtree pull --prefix Assets/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git production --squash  )
        If you already add it, then pull command only is enought!

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
            LoadingManager.Instant.DoneConditionSelf(0, ()=> AdManager.Instant.AdsOpenIsLoaded(0));
           
            AsyncOperation loadingScene = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            loadingScene.allowSceneActivation = true;
            LoadingManager.Instant.SetMaxTimeLoading(30).Init((conditionDone) =>
            {
                AdManager.Instant.ShowAdOpen(0,true, (isSuccess) =>
                {

                   
                    AdManager.Instant.InitializeBannerAdsAsync();
                    AdManager.Instant.ShowBanner();
                });
                
            });
        }

    }

- As you can see, our Loading systems contain a max time loading and a list boolean of condition

Call ***LoadingManager.Instant.DoneCondition(ID);*** for set a condition is success
You can use Coroutine to waitUntil then set done condition or using
***LoadingManager.Instant.DoneConditionSelf(ID, Func);*** 

if all condition is done then loading will stop and invoke callback, which already define in  ***LoadingManager.Instant.Init(callbackDone);***

if some condition still false but loading reach max time then loading still stop and invoke call back. You can process on callback like this:

      LoadingManager.Instant.Init({number of conditions},(conditionDone)=>{
              if(conditionDone.Where(t => t == false ).Any()){
                      //Some condition fail!

              }else{
                      //All condition done!
              }
      });
You can set max time of loading using script right before call loading init:

      LoadingManager.Instant.SetMaxTimeLoading(30).Init();
      
You can using doneConditionSelf to wait something done (it similar with using coroutine wait until):

      LoadingManager.Instant.DoneConditionSelf({ID condition}, ()=> AdManager.Instant.OpenAdLoaded());

## Call function:

![enter ID adopen](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/6.png) 

#### ***Buy product***
        IAPManager.Instant.BuyProductID("IDProduct", (isSuccess) =>
        {
            if(isSuccess)
                Debug.Log("Buy DOne!");
            else
                Debug.Log("Buy Fail!");
        });
Or If you wanna get data of product like receipt for self-check on your server
        IAPManager.Instant.BuyProductID("IDProduct", (isSuccess, Product) =>
        {
            if (isSuccess)
                Debug.Log("Buy DOne! "+Product.receipt);
            else
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
        FireBaseManager.Instant.LogEventWithParameterAsync("Event_name_do_not_using_space", new Hashtable()
        {
            {
                "parameter",1
            }
        });

        //or

        FireBaseManager.Instant.LogEventWithOneParam("Event_name_do_not_using_space" );

#### ***Get value from remote config***
        FireBaseManager.Instant.GetValueRemoteAsync("key", (value) =>
        {
            int true_value = (int)value.LongValue;
        });



#### ***ShowPopUprate***
        DVAH3rdLib.Instant.ShowPopUpRate() //==> Show
        DVAH3rdLib.Instant.ShowPopUpRate(false) //==> Hide

#### ***AdManager***
        AdManager.Instant.InitializeBannerAdsAsync(); // If you wanna call init banner manually
        AdManager.Instant.ShowBanner();
        AdManager.Instant.DestroyBanner();
####

Call anywhwre you need show or hide banner, admanager auto load and show or hide!

On inspector, set bannerADID = blank if game dont have banner
   
####
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
####
showNoAds: true ==> if you wanna show popup "AD not avaiable" - flase or just leave it blank if you dont need popup
####
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
####
showNoAds: true ==> if you wanna show popup "AD not avaiable" - flase or just leave it blank if you dont need popup

****AD OPEN/RESUME*****
![enter ID adopen](https://raw.githubusercontent.com/nhathuy7996/Unity3rdLib/develop/GitImage/5.png) 

Some game have more than one ad open then, you must pass ID for showing

If game have more than one ad open then Ad ID 1 will auto use for open game and the last one use for resume game

####
            AdManager.Instant.ShowAdOpen(ID,true, (id,status) =>
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
            }); 
####

true then lib will know AdOpen treated as Ad open when open game, or as an AD when user return game.

You call check on callback Ad show success or not using isSuccess

####
            AdManager.Instant.LoadNativeADsAsync(0,1,2);
            AdManager.Instant.ShowNativeAsync(ID,true, (nativePanel) =>
            {
                 nativePanel.transform.SetParent(canvas.transform);
                 nativePanel.transform.localScale = Vector3.one;
                 nativePanel.transform.localPosition = Vector3.zero;
                 nativePanel.rectTransform.sizeDelta = Vector2.zero;
                 nativePanel.rectTransform.anchorMax = new Vector2(1, 0.4f);
            }); 
####

ID is index of ID native AD

Actually, Native AD object already create right when native AD success!

Show function using callback for assign Native AD object into right canvas
