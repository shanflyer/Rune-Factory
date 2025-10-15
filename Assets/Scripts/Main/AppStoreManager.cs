using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class AppStoreManager : MonoBehaviour 
{
    const string projectName = "com.shanflyer.FantasyTown_EveryDay";
    public static AppStoreManager instance; 

    private MyDic<string, AppStoreProductData> appStoreProductDatas = new MyDic<string, AppStoreProductData>();

    public List<AppStoreProductData> GetAppStoreProductDatas()
    {
        return appStoreProductDatas.GetValueList();
    }
    void OnDisable()
    {
#if !UNITY_EDITOR 
            BillingServices.OnInitializeStoreComplete -= OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange -= OnTransactionStateChange;
            BillingServices.OnRestorePurchasesComplete -= OnRestorePurchasesComplete;
#endif

    }

    private void OnEnable()
    {
        instance = this;
        var allProductDatas = Resources.LoadAll<AppStoreProductData>(DataPath.dataPathDic[typeof(AppStoreProductData)]);
        for (int i = 0; i < allProductDatas.Length; i++)
        {
            appStoreProductDatas.Add($"{allProductDatas[i].ProductName}", allProductDatas[i]);
        }
#if !UNITY_EDITOR
            BillingServices.IsAvailable();
            BillingServices.OnInitializeStoreComplete += OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange += OnTransactionStateChange;
            BillingServices.OnRestorePurchasesComplete += OnRestorePurchasesComplete;
            BillingServices.InitializeStore();
#endif

    }

    public void Awake()
    {
       
    }
     
   // public string goldProductId = "com.shanflyer.FantasyTown_EveryDay.diamond200";
    public string goldProductId = "diamond200";

    public void BuyProduct(AppStoreProductData appStoreProductData)
    {
#if UNITY_EDITOR
        if (appStoreProductData.getDiamond > 0)
        {
            InformationController.instance.AddInformation(string.Format(LanguageManage.SwitchStr($"成功获得{0}钻石!"), appStoreProductData.getDiamond), false, true);
            PayManager.instance.AddDiamond(appStoreProductData.getDiamond);
        }
        else
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr($"感谢您的支持！"), false, true);
        }
        GameActionManager.instance.QueueAction(new PayEndAction());
#else
      var goldProductId = $"{appStoreProductData.ProductName}";
        BillingServices.BuyProduct(goldProductId,options:null);
    
#endif

    }

    public string GetProductPriceStr(string produceName)
    {
        var product = BillingServices.GetProductWithId(produceName);
        return product.Price.LocalizedText;
    }
    public void BuyProduct(string ProductName)
    {
        var goldProductId = $"{ProductName}";
        BillingServices.BuyProduct(goldProductId, options: null);
    }
    private void OnTransactionStateChange(BillingServicesTransactionStateChangeResult result)
    {
        GameActionManager.instance.QueueAction(new PayEndAction());
        var transactions = result.Transactions;
        for (int iter = 0; iter < transactions.Length; iter++)
        {
            var transaction = transactions[iter];
            switch (transaction.TransactionState)
            {
                case BillingTransactionState.Purchased:
                    Debug.Log(string.Format("Buy product with id:{0} finished successfully.", transaction.Product.Id));
                    if (appStoreProductDatas.TryGetValue(transaction.Product.Id,out var appStoreProductData))
                    {
                        if (appStoreProductData.getDiamond > 0)
                        {
                            InformationController.instance.AddInformation(string.Format(LanguageManage.SwitchStr($"成功获得{0}钻石!"), appStoreProductData.getDiamond), false, true);
                            PayManager.instance.AddDiamond(appStoreProductData.getDiamond);
                        }
                        else
                        {
                            InformationController.instance.AddInformation(LanguageManage.SwitchStr($"感谢您的支持！"), false, true);
                        }
                    }

                    /*
                        if(transaction.Product.Id.Equals("REMOVE_ADS")) //Note we used Equals instead of "==" which is always safe!
                        {
                            Debug.Log("REMOVE_ADS product purchased. Proceed with removing ads");
                        }
                    */
                    break;

                case BillingTransactionState.Failed:
                    string log= (string.Format("Buy product with id:{0} failed with error. Error: {1}", transaction.Product.Id, transaction.Error));
                    InformationController.instance.AddInformation(log, false, true);
                    break;
            }
        }
    }
    private void OnRestorePurchasesComplete(BillingServicesRestorePurchasesResult result, Error error)
    {
    }

    private void OnInitializeStoreComplete(BillingServicesInitializeStoreResult result, Error error)
    {
        /*
        if (error == null)
        {
            // update UI
            // show console messages
            var products = result.Products;
            Debug.Log("Store initialized successfully.");
            Debug.Log("Total products fetched: " + products.Length);
            Debug.Log("Below are the available products:");
            for (int iter = 0; iter < products.Length; iter++)
            {
                var product = products[iter];
                Debug.Log(string.Format("[{0}]: {1}", iter, product));
            }
        }
        else
        {
            Debug.Log("Store initialization failed with error. Error: " + error);
        }

        var invalidIds = result.InvalidProductIds;
        Debug.Log("Total invalid products: " + invalidIds.Length);
        if (invalidIds.Length > 0)
        {
            Debug.Log("Here are the invalid product ids:");
            for (int iter = 0; iter < invalidIds.Length; iter++)
            {
                Debug.Log(string.Format("[{0}]: {1}", iter, invalidIds[iter]));
            }
        }*/
    }

}