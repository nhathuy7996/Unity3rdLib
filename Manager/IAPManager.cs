﻿#if IAP
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Purchasing.Security;
using System.Collections;

namespace HuynnLib
{

    public class IAPManager : Singleton<IAPManager>, IStoreListener, IChildLib
    {

        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.


        private Dictionary<string, Action> RestoreItemEvent = new Dictionary<string, Action>();
        [SerializeField]
        List<string> _restoreItemCheck = new List<string>();

        private Action _onBuyDone = null, _onBuyFail = null;

        public bool IsInitDone => IsInitialized();
        private bool _isBuying = false;

        private bool _isRestoreDone = false;

        Action _onInitDone;

        public void Init(Action onInitDone)
        {
            Debug.Log("==========> IAP start Init! <==========");
            _onInitDone = onInitDone;
            InitializePurchasing();
        }


        public bool TryAddRestoreEvent(string productID, Action eventRestore = null )
        {
            if (RestoreItemEvent.ContainsKey(productID))
            {
                Debug.LogErrorFormat("==> Product {0} already has event restore <==", productID);
                return false;
             
            }

            if (!_isRestoreDone)
            {
                _restoreItemCheck.Add(productID);
                eventRestore?.Invoke();
            }

            RestoreItemEvent.Add(productID, eventRestore);
            return true;
        }

        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());



            var catalog = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalog.allValidProducts)
            {
                if (product.allStoreIDs.Count > 0)
                {
                    var ids = new IDs();
                    foreach (var storeID in product.allStoreIDs)
                    {
                        ids.Add(storeID.id, storeID.store);
                    }
                    builder.AddProduct(product.id, product.type, ids);
                }
                else
                {
                    builder.AddProduct(product.id, product.type);
                }

                
            }



            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }


        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }


        public bool CheckRestoredProduct(string productId)
        {
            return this._restoreItemCheck.Contains(productId);
        }


        public void BuyProductID(string productId, Action onBuyDone = null, Action onBuyFail = null)
        {
            if (_isBuying) return;

            _onBuyDone = onBuyDone;
            _onBuyFail = onBuyFail;

            if (!string.IsNullOrEmpty(productId))
                Debug.Log("==> buy productId : " + productId+" <==");
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                _isBuying = true;
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    //Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product, productId);
                    return;
                }

                _isBuying = false;
                // ... report the product look-up failure situation  
                Debug.LogError("==> BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase <==");

                return;
            }

            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            _isBuying = false;
            Debug.LogError("==> BuyProductID FAIL. Not initialized <==");
            //NoticeManager.Instance.LogNotice("BuyProductID FAIL. Not initialized.");

        }


        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.LogError("==> RestorePurchases FAIL. Not initialized <==");
                //NoticeManager.Instance.LogNotice("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("==> RestorePurchases started ...<==");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Debug.Log("==> RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore <==");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.LogError("==> RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform+" <==");
            }
        }

     
        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            _onInitDone?.Invoke();

        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.LogError("==> OnInitializeFailed InitializationFailureReason:" + error+" <==");
            _onInitDone?.Invoke();
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var catalog = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalog.allValidProducts)
            {

                if (String.Equals(args.purchasedProduct.definition.id, product.id, StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("==> ProcessPurchase: PASS. Product: '{0}' <==", args.purchasedProduct.definition.id));
                    //NoticeManager.Instance.LogNotice(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                    //#if  UNITY_EDITOR
                    if (!_isBuying)
                    {
                        if (RestoreItemEvent.ContainsKey(product.id))
                        {
                            RestoreItemEvent[product.id]?.Invoke();
                            _restoreItemCheck.Add(product.id);
                            continue;
                        }
                    }

                    if (_onBuyDone != null)
                    {
                        _onBuyDone.Invoke();
                        _onBuyDone = null;
                    }


                    return PurchaseProcessingResult.Complete;
                }
            }

            if (!_isBuying)
            {
                _isRestoreDone = true;
            }
            _isBuying = false;
            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Pending;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            _isBuying = false;
            _onBuyDone = null;
            if (_onBuyFail != null)
            {
                _onBuyFail.Invoke();
                _onBuyFail = null;
            }

            Debug.LogError(string.Format("==> OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1} <==", product.definition.storeSpecificId, failureReason));

        }
    }

}

#endif