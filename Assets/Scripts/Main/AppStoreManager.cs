using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class AppStoreManager : MonoBehaviour, IDetailedStoreListener
{
    const string projectName = "com.shanflyer.FantasyTown_EveryDay";
    public static AppStoreManager instance;
    private IStoreController m_StoreController;
    private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

    private MyDic<string, AppStoreProductData> appStoreProductDatas = new MyDic<string, AppStoreProductData>();

    public List<AppStoreProductData> GetAppStoreProductDatas()
    {
        return appStoreProductDatas.GetValueList();
    }
    void OnDisable()
    {

    }
    public  async void Awake()
    {
        instance = this;
        var allProductDatas = await GameDataManager.instance.GetAllAsyncData<AppStoreProductData>();
        for (int i = 0; i < allProductDatas.Count; i++)
        {
            appStoreProductDatas.Add($"{projectName}.{allProductDatas[i].ProductName}", allProductDatas[i]);
        }
        this.appStoreProductData = null;
        InitializePurchasing();
        UpdateWarningMessage();
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.Configure<IGooglePlayConfiguration>().SetServiceDisconnectAtInitializeListener(() =>
        {
            Debug.Log("Unable to connect to the Google Play Billing service. " +
                "User may not have a Google account on their device.");
        });
        builder.Configure<IGooglePlayConfiguration>().SetQueryProductDetailsFailedListener((int retryCount) =>
        {
            Debug.Log("Failed to query product details " + retryCount + " times.");
        });

        builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnDeferredPurchase);
        for (int i = 0; i < appStoreProductDatas.length; i++)
        {
            var productData = appStoreProductDatas[i];
            builder.AddProduct(productData.ProductName, ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    private void OnDeferredPurchase(Product product)
    {
        Debug.Log($"Purchase of {product.definition.id} is deferred");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");

        m_StoreController = controller;
        m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
    }

    private AppStoreProductData appStoreProductData;

    public void BuyProduct(AppStoreProductData appStoreProductData)
    {
        this.appStoreProductData = appStoreProductData;
        var goldProductId = $"{projectName}.{appStoreProductData.ProductName}";
        m_StoreController.InitiatePurchase(goldProductId);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;

        Debug.Log($"Processing Purchase: {product.definition.id}");

        if (m_GooglePlayStoreExtensions.IsPurchasedProductDeferred(product))
        {
            //The purchase is Deferred.
            //Therefore, we do not unlock the content or complete the transaction.
            //ProcessPurchase will be called again once the purchase is Purchased.
            return PurchaseProcessingResult.Pending;
        }

        UnlockContent(product);

        return PurchaseProcessingResult.Complete;
    }

    private void UnlockContent(Product product)
    {
        Debug.Log($"Unlock Content: {product.definition.id}");
        var goldProductId = $"{projectName}.{appStoreProductData.ProductName}";
        if (product.definition.id == goldProductId)
        {
            PayManager.instance.AddDiamond(appStoreProductData.getDiamond);
        }
    }

    private bool IsPurchasedProductDeferred(string productId)
    {
        var product = m_StoreController.products.WithID(productId);
        return m_GooglePlayStoreExtensions.IsPurchasedProductDeferred(product);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        if (message != null)
        {
            errorMessage += $" More details: {message}";
        }

        Debug.Log(errorMessage);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
            $" Purchase failure reason: {failureDescription.reason}," +
            $" Purchase failure details: {failureDescription.message}");
    }

    private void UpdateWarningMessage()
    {
        var currentAppStore = StandardPurchasingModule.Instance().appStore;

        var warningMessage = currentAppStore != AppStore.GooglePlay ?
            "This sample is meant to be tested using the Google Play Store.\n" +
            $"The currently selected store is: {currentAppStore}.\n" +
            "Build the project for Android and use the Google Play Store.\n\n" +
            "See README for more information and instructions on how to test this sample."
            : "";
    }
}