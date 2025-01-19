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
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeBtn = FindChildGameObject<Button>("Close");
        productParent = FindChildGameObject("Products");
        productReference = FindChildGameObject<StoreProductReference>("productReference");
    }

    public override Task InitData(string dataKey)
    {
        storeProductList.InitListData(AppStoreManager.instance.GetAppStoreProductDatas(), SelectProduct);
        return base.InitData(dataKey);
    }

    private void SelectProduct(AppStoreProductData productData, bool selected)
    {
        AppStoreManager.instance.BuyProduct(productData);
    }

    public override void InitReferenceData(IReferenceData v)
    {
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