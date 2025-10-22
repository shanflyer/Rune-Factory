using System.Collections.Generic;
using UnityEngine; 

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


    }

    private void OnEnable()
    {
        instance = this;
        var allProductDatas = Resources.LoadAll<AppStoreProductData>(DataPath.dataPathDic[typeof(AppStoreProductData)]);
        for (int i = 0; i < allProductDatas.Length; i++)
        {
            appStoreProductDatas.Add($"{allProductDatas[i].ProductName}", allProductDatas[i]);
        }


    }

    public void Awake()
    {
       
    }
     
   // public string goldProductId = "com.shanflyer.FantasyTown_EveryDay.diamond200";
    public string goldProductId = "diamond200";

    public void BuyProduct(AppStoreProductData appStoreProductData)
    {
 
    }

   
}