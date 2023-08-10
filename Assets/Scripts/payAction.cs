using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class payAction : MonoBehaviour,IStoreListener
{
    public GameObject waitApple;
    private string erroTitle;
    private IStoreController controller;
    void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            var module = StandardPurchasingModule.Instance();
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            builder.AddProduct("com.shanflyer.FantasyTownshipStory.001", ProductType.NonConsumable);
            builder.AddProduct("com.shanflyer.FantasyTownshipStory.002", ProductType.NonConsumable);
            builder.AddProduct("com.shanflyer.FantasyTownshipStory.003", ProductType.NonConsumable);
            builder.AddProduct("com.shanflyer.FantasyTownshipStory.004", ProductType.NonConsumable);
            UnityPurchasing.Initialize(this, builder);
        }
        
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        waitApple.SetActive(false);
        if (error == InitializationFailureReason.PurchasingUnavailable)
        {
            
            
            print("手机设置了禁止APP内购");
            GameNotificationManager.instance.DisplayTips("错误", "手机设置了禁止APP内购");
        }
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        waitApple.SetActive(false);
       

        string notice0, notice1, notice2, notice3, notice4, notice5, notice6, notice7, notice8;
        if (Application.systemLanguage == SystemLanguage.ChineseSimplified ||
            Application.systemLanguage == SystemLanguage.ChineseTraditional ||
            Application.systemLanguage == SystemLanguage.Chinese)
        {
            erroTitle = "购买失败!";
            notice0 = "未知错误!";
            notice1 = "重复交易!";
            notice2= "交易申请已存在!";
            notice3 = "当前苹果账户无法购买商品!(如有疑问，可以询问苹果客服)";
            notice4 = "产品不可用!";
            notice5 = "采购不可用!";
            notice6 = "签名无效!";
            notice7 = "未知的错误，您可能正在使用越狱手机!";
            notice8 = "订单已取消!";
            
        }
        else
        {
            erroTitle = "Failed Purchase!";
            notice0 = "Unknown!";
            notice1 = "DuplicateTransaction!";
            notice2 = "ExistingPurchasePending!";
            notice3 = "PaymentDeclined!";
            notice4 = "ProductUnavailable!";
            notice5 = "PurchasingUnavailable!";
            notice6 = "SignatureInvalid!";
            notice7 = "Unknown!";
            notice8 = "UserCancelled!";
        }

        string notice = "";
        switch (r)
        {
            case PurchaseFailureReason.DuplicateTransaction:
                notice = notice1;
                break;
            case PurchaseFailureReason.ExistingPurchasePending:
                notice = notice2;
                break;
            case PurchaseFailureReason.PaymentDeclined:
                notice = notice3;
                break;
            case PurchaseFailureReason.ProductUnavailable:
                notice = notice4;
                break;
            case PurchaseFailureReason.PurchasingUnavailable:

                notice = notice5;
                break;
            case PurchaseFailureReason.SignatureInvalid:
                notice = notice6;
                break;
            case PurchaseFailureReason.Unknown:
                notice = notice7;
                break;
            case PurchaseFailureReason.UserCancelled:
                notice = notice8;
                break;
            default:
                notice = notice0;
                break;

        }
       GameNotificationManager.instance.DisplayTips(erroTitle,notice);
    }
    public void OnPurchaseClicked()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
           
           // string notice = "网络未连接!";
          


            if (Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                Application.systemLanguage == SystemLanguage.ChineseTraditional ||
                Application.systemLanguage == SystemLanguage.Chinese)
            {
                GameNotificationManager.instance.DisplayTips("购买失败", "网络未连接!");
            }
            else
            {
                GameNotificationManager.instance.DisplayTips("Failed Purchase!", "Network is not connected!");
            }
        }
        else
        {
            waitApple.SetActive(true);

        }
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }
}
