# Unity3rdLib
## tích hợp sẵn các thư viện bên thứ 3 của unity:
- [x] AD apploving (MAX)
- [ ] Adapter các mạng quảng cáo
- [x] Facebook
- [x] Firebase
- [x] Adjust
- [x] In-App purchase
- [x] Nointernet popup
- [x] NoAds popup
- [x] google Rate

#### --> Tự động check missing file google-service.js <--

## Hướng dẫn sử dụng:
- Add git remote vào thư mục Assets và pull nhánh production (_develop là các chức năng đang sửa, có thể hoạt động chưa ổn định_)
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
        IAPManager.Instant.TryAddRestoreEvent("productID", () =>
        {
            Debug.Log("Restore Done!");
        });

        //Doesn't matter IAP init done or not
        //On IOS must add :
        IAPManager.Instant.RestorePurchases();

        //Check is product restored or not:
        IAPManager.Instant.CheckRestoredProduct("ProductID");


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
        FireBaseManager.Instant.GetValueRemote("key", (value) =>
        {
            int true_value = (int)value;
        });


#### ***ShowPopUprate***
        MasterLib.Instant.ShowPopUpRate() //==> Show
        MasterLib.Instant.ShowPopUpRate(false) //==> Hide
