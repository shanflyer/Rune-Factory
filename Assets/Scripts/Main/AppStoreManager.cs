using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
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

    private void OnEnable()
    {
        instance = this;
        var allProductDatas = Resources.LoadAll<AppStoreProductData>(DataPath.dataPathDic[typeof(AppStoreProductData)]);
        for (int i = 0; i < allProductDatas.Length; i++)
        {
            appStoreProductDatas.Add($"{projectName}.{allProductDatas[i].ProductName}", allProductDatas[i]);
        }
        this.appStoreProductData = null;
        InitializePurchasing();
        UpdateWarningMessage();
    }

    public void Awake()
    {
       
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.Configure<IGooglePlayConfiguration>().SetServiceDisconnectAtInitializeListener(() =>
        {
            InformationController.instance.AddInformation( "Unable to connect to the Google Play Billing service. " +
                "User may not have a Google account on their device.",false,true);
        });
        builder.Configure<IGooglePlayConfiguration>().SetQueryProductDetailsFailedListener((int retryCount) =>
        {
            InformationController.instance.AddInformation("Failed to query product details " + retryCount + " times.", false, true);
        });

        builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnDeferredPurchase);

      //  builder.AddProduct(goldProductId, ProductType.Consumable);
       for (int i = 0; i < appStoreProductDatas.length; i++)
        {
            var ProductName = $"{projectName}.{appStoreProductDatas[i].ProductName}";
            builder.AddProduct(ProductName, ProductType.Consumable);
        } 
        //var ProductName = $"{projectName}.{appStoreProductDatas[0]}" ;
       // Debug.Log($"ProductName:{ProductName}---{projectName == goldProductId}");
        builder.AddProduct(goldProductId, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }
    public string goldProductId = "com.shanflyer.FantasyTown_EveryDay.diamond200";
    private void OnDeferredPurchase(Product product)
    {
        InformationController.instance.AddInformation($"Purchase of {product.definition.id} is deferred", false, true);
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
            InformationController.instance.AddInformation(string.Format(LanguageManage.SwitchStr($"成功获得{0}钻石!"), appStoreProductData.getDiamond), false, true); 
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

        InformationController.instance.AddInformation(errorMessage, false, true);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        InformationController.instance.AddInformation($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}", false, true);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        InformationController.instance.AddInformation($"Purchase failed - Product: '{product.definition.id}'," +
            $" Purchase failure reason: {failureDescription.reason}," +
            $" Purchase failure details: {failureDescription.message}", false, true);
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