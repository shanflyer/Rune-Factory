using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
}