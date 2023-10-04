using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxReference : UIObjReference<Item>
{
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Image mask;

    [SerializeField]
    private Image backGround;

    [SerializeField]
    private Image lockImage;

    [SerializeField]
    private Text count;

    private Item item;
    public Item Item => item;
    private SelectAction<Item> SelectAction;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        mask = FindChildGameObject<Image>("mask");
        backGround = FindChildGameObject<Image>("Image");
        count = FindChildGameObject<Text>("count");
        lockImage = FindChildGameObject<Image>("lock");
    }

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction.Invoke(item);
            }
        });
    }
    public void ClearData()
    {
        item = default(Item);
        mask.enabled = true;
        icon.enabled = false;
        count.enabled = false;
    }
   
    public override async void InitData(Item t, SelectAction<Item> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        item = t;

        toggle.group=toggleGroup;
        this.SelectAction = SelectAction;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());

        if (itemData != null)
        {
            mask.enabled = false;
            lockImage.enabled = false;
            icon.sprite = itemData.icon;
            icon.color = Color.white;
            icon.enabled = true;
            icon.SetNativeSize();
            count.text = item.count.ToString();
            count.enabled = true;
            toggle.enabled = true;
        }
        else
        {
            toggle.enabled = t.instanceId<0;
            lockImage.enabled = t.instanceId < 0;
            mask.enabled = true;
            icon.enabled = false;
            count.enabled = false;
        }
    }

    public void SetEnableColor(bool enable)
    {
        mask.enabled = enable;
        backGround.color = enable ? Color.white : new Color(1, 0.506f, 0.506f);
    }

    public void DisPlayFormulaItem(int _itemid)
    {
        //InitData(_itemid);
        count.enabled = false;
        GetComponentInChildren<Toggle>().enabled = true;
        mask.enabled = true;
    }

    public void DisPlayFormulaItem()
    {
        if (item.instanceId != 0)
        {
            InitData(item);
            count.enabled = false;
            GetComponentInChildren<Toggle>().enabled = true;
            mask.enabled = true;
            icon.color = new Color(0.624f, 0.624f, 0.624f, 0.5f);
        }
    }
 

    public void Hide()
    {
        mask.enabled = true;
        icon.enabled = false;
        count.enabled = false;
    }
}