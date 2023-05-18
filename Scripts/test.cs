using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DVAH;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System.Text.RegularExpressions;

public class test : MonoBehaviour
{
    private Vector3 touchPosition;
    private Quaternion rotateModel;
    private float rotationSpeed = 360;
    private Vector3 angle;
    // Start is called before the first frame update
    void Start()
    {
         
    }

    

    // Update is called once per frame
    void Update()
    {

        //------ Buy Product -----
        //IAPManager.Instant.BuyProductID("IDProduct", (isSuccess) =>
        //{
        //    if(isSuccess)
        //        Debug.Log("Buy DOne!");
        //    else
        //        Debug.Log("Buy Fail!");
        //});

        //IAPManager.Instant.BuyProductID("IDProduct", (isSuccess, Product) =>
        //{
        //    if (isSuccess)
        //        Debug.Log("Buy DOne! "+Product.receipt);
        //    else
        //        Debug.Log("Buy Fail!");
        //});

        //------ restore product, call ASAP -----
        //_= IAPManager.Instant.TryAddRestoreEvent("productID", () =>
        //{
        //    Debug.Log("Restore Done!");
        //});
        //---OR if you need check its OK ---
        //var restoreSth = IAPManager.Instant.TryAddRestoreEvent("productID", () =>
        //{
        //    Debug.Log("Restore Done!");
        //});
        //if (restoreSth.Result)
        //{
        //    Debug.Log("It's OK");
        //}
        // Doesn't matter IAP init done or not
        // On IOS must add : IAPManager.Instant.RestorePurchases();

        //Check is product restored or not:
        //IAPManager.Instant.CheckRestoredProduct("ProductID");


        //-----Log event Firebase ----
        //FireBaseManager.Instant.LogEventWithParameter("Event_name_do_not_using_space", new Hashtable() {
        //{
        //    "parameter",1
        //}});

        //or

        //FireBaseManager.Instant.LogEventWithOneParam("Event_name_do_not_using_space" );

        //----- get value from remote config-----
        //_= FireBaseManager.Instant.GetValueRemoteAsync("key", (value) =>
        //{
        //    int true_value = (int)value.LongValue;
        //});


        //----- ShowPopUprate------ 
        //MasterLib.Instant.ShowPopUpRate() ==> Show
        //MasterLib.Instant.ShowPopUpRate(false) ==> Hide


        //AdManager.Instant.ShowInterstitial((status) =>
        //{
        //    //Do sth when user watched inter
        //    if (status == InterVideoState.Closed)
        //    {
        //        Debug.Log("Do sth!");
        //    }
        //    else
        //    {
        //        //Do sth when inter fail to show, not ready, etc....
        //    }


        //}, showNoAds: true);
        // showNoAds: true ==> if you wanna show popup "AD not avaiable" - flase or just leave it blank if you dont need popup

        //AdManager.Instant.ShowRewardVideo((status) =>
        //{
        //    //Do sth when user watched reward
        //    if (status == RewardVideoState.Watched)
        //    {
        //        Debug.Log("Do sth!");
        //    }
        //    else
        //    {
        //        //Do sth when reward fail to show, not ready, etc....
        //    }


        //}, showNoAds: true);
        // showNoAds: true ==> if you wanna show popup "AD not avaiable" - flase or just leave it blank if you dont need popup
    }


}
