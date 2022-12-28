using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HuynnLib;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        //-----Buy product-----
        //IAPManager.Instant.BuyProductID("IDProduct", () =>
        //{
        //    Debug.Log("Buy DOne!");
        //}, () =>
        //{
        //    Debug.Log("Buy Fail!");
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



    }


}
