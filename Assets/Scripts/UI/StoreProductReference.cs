using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreProductReference : UIObjReference<AppStoreProductData>
{
    [SerializeField]
    Image image;
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    TextMeshProUGUI count;
    [SerializeField]
    Button actionBtn;
    [SerializeField]
    Transform particle;

    [SerializeField] private TextMeshProUGUI price;
    private void Awake()
    {
        actionBtn.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(data,index, true);
            }
        });
    }
    public override void ClearSelect()
    {
        base.ClearSelect();
        particle.localScale = Vector3.zero;
    } 
    public override Task InitData(AppStoreProductData t, SelectAction<AppStoreProductData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        text.text = LanguageManage.SwitchStr(t.showName);
        count.text = t.getDiamond.ToString();
        image.sprite = t.icon;
        // image.SetNativeSize();
        particle.localScale = Vector3.one;
        price.text = AppStoreManager.instance.GetProductPriceStr(t.ProductName);
        return base.InitData(t, SelectAction, toggleGroup);
    }
    public override void InitChildObjData()
    {
        base.InitChildObjData();
        image = FindChildGameObject<Image>("Icon");
        text = FindChildGameObject<TextMeshProUGUI>("Name");
        count = FindChildGameObject<TextMeshProUGUI>("Count");
        actionBtn = FindChildGameObject<Button>("Action");
        particle = FindChildGameObject("Effect");
    }
}