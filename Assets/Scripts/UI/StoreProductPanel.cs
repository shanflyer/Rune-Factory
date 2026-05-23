using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.EssentialKit;

public class StoreProductPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private Button closeBtn;

    [SerializeField]
    private Transform productParent;
    [SerializeField]
    private Transform mask;

    [SerializeField]
    private StoreProductReference productReference;

    private DisplayList<StoreProductReference, AppStoreProductData> storeProductList;

    protected override void Awake()
    {
        base.Awake();
        storeProductList = new DisplayList<StoreProductReference, AppStoreProductData>(productReference, productParent);
        closeBtn.onClick.AddListener(Close);
 
    }
    public override void Close()
    {
        base.Close();
        storeProductList.ClearSelect();
        GameActionManager.instance.RemoveListener<PayEndAction>(PayEndAction); 
        gameObject.SetActive(false);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeBtn = FindChildGameObject<Button>("Close");
        productParent = FindChildGameObject("Products");
        productReference = FindChildGameObject<StoreProductReference>("productReference");
        mask = FindChildGameObject("Mask");
    }

    void PayEndAction(PayEndAction payEndAction)
    {
        mask.gameObject.SetActive(false);
    }
    public override async Task InitData(string dataKey)
    {
        mask.gameObject.SetActive(false);
        GameActionManager.instance.AddListener<PayEndAction>(PayEndAction);
        await storeProductList.InitListData(AppStoreManager.instance.GetAppStoreProductDatas(), SelectProduct);
        await base.InitData(dataKey);
    }

    private void SelectProduct(AppStoreProductData productData,int index, bool selected)
    {
        AppStoreManager.instance.BuyProduct(productData);
        mask.gameObject.SetActive(true);
    }

    public override void InitReferenceData(IReferenceData v)
    {
        mask.gameObject.SetActive(false);
        GameActionManager.instance.AddListener<PayEndAction>(PayEndAction);
        base.InitReferenceData(v);
    }

    private void OnTransactionStateChange(BillingServicesTransactionStateChangeResult result)
    {
        var transactions = result.Transactions;
        for (int iter = 0; iter < transactions.Length; iter++)
        {
            var transaction = transactions[iter];
            switch (transaction.TransactionState)
            {
                case BillingTransactionState.Purchased:
                    Debug.Log(string.Format("Buy product with id:{0} finished successfully.", transaction.Product.Id));
                    /*
                        if(transaction.Product.Id.Equals("REMOVE_ADS")) //Note we used Equals instead of "==" which is always safe!
                        {
                            Debug.Log("REMOVE_ADS product purchased. Proceed with removing ads");
                        }
                    */
                    break;

                case BillingTransactionState.Failed:
                    Debug.Log(string.Format("Buy product with id:{0} failed with error. Error: {1}", transaction.Product.Id, transaction.Error));
                    break;
            }
        }
    }
}
