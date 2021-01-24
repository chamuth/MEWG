using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class IAPManager : MonoBehaviour, IStoreListener
{
    public GameObject PurchasePreloader;

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    public const string REMOVE_ADS_ID = "remove_ads";

    public const string CONSUMABLE1 = "5_hints";
    public const string CONSUMABLE2 = "10_hints";
    public const string CONSUMABLE3 = "50_hints";
    public const string CONSUMABLE4 = "250_hints";

    void Start()
    {
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(REMOVE_ADS_ID, ProductType.NonConsumable);

        builder.AddProduct(CONSUMABLE1, ProductType.Consumable);
        builder.AddProduct(CONSUMABLE2, ProductType.Consumable);
        builder.AddProduct(CONSUMABLE3, ProductType.Consumable);
        builder.AddProduct(CONSUMABLE4, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public static string GetProductPrice(string productId)
    {
        var price = "";

        try { price = m_StoreController.products.WithID(productId).metadata.localizedPriceString; }
        catch (Exception) { };

        return price;
    }

    public void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));

                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, REMOVE_ADS_ID, StringComparison.Ordinal))
        {
            // Remove ads from the app
            PlayerPrefs.SetInt("REMOVE_ADS", 1);
            PlayerPrefs.Save();

            PurchasePreloader.SetActive(false);
        }
        else
        {
            // A consumable product purchased
            int hintCount = 0;

            switch (args.purchasedProduct.definition.id)
            {
                case CONSUMABLE1:
                    hintCount = 5;
                    break;
                case CONSUMABLE2:
                    hintCount = 10;
                    break;
                case CONSUMABLE3:
                    hintCount = 50;
                    break;
                case CONSUMABLE4:
                    hintCount = 250;
                    break;
                default:
                    Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
                    return PurchaseProcessingResult.Complete;
            }

            // Update the database
            var hintRef = FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("hints").Child("count");
            
            hintRef.GetValueAsync().ContinueWith((snap) =>
            {
                var currentHintsCount = (int)snap.Result.Value;
                hintRef.SetValueAsync(currentHintsCount + hintCount).ContinueWith((t) =>
                {
                    if (t.IsCompleted && !t.IsFaulted)
                    {
                        // Updated the database
                        PurchasePreloader.SetActive(false);
                    }
                });
            });
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        PurchasePreloader.SetActive(false);
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
}