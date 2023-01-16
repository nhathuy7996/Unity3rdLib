# Unity3rdLib
## tích hợp sẵn các thư viện bên thứ 3 của unity:
- [x] AD apploving (MAX)
- [x] Adapter các mạng quảng cáo
- [x] Facebook
- [x] Firebase
- [x] Adjust
- [x] In-App purchase
- [x] Nointernet popup
- [x] NoAds popup
- [x] google Rate

#### --> Tự động check missing file google-service.js <--

## Hướng dẫn sử dụng:
- Add git subtree vào thư mục Assets và pull nhánh production (_develop là các chức năng đang sửa, có thể hoạt động chưa ổn định_)
- (Git Add Command: git subtree add --prefix Assets/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git develop --squash    )
- (Git Pull Command: git subtree pull --prefix Assets/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git develop --squash  )
- Download SDK [tại đây](https://github.com/nhathuy7996/Unity3rdLib/releases) và import vào project 
        (nên xoá toàn bộ plugin và thư viện Ad, IAP ... cũ không còn dùng đễn nữa trước khi import)
- Reimport All
- Android Resolve -> Force Resolve
- Kéo prefab 3rdLib vào scene (check dontDestroyOnload nếu có nhiều scene)

## Cách gọi các chức năng:
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
