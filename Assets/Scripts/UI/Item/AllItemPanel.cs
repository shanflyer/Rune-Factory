using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 

public class AllItemPanel : GamePanel<IReferenceData>
{ 
      

    [SerializeField]
    private Transform ItemInformation;
     
     

    [SerializeField]
    private Image ItemIcon;

    [SerializeField]
    private TextMeshProUGUI ItemName;
     

    [SerializeField]
    private TextMeshProUGUI Type, Property, Info;

    [SerializeField]
    private Button ActionButton, ReturnButton; 
    [SerializeField]
    private Transform InfoItemValueBg;

    [SerializeField]
    private Image InfoItemValue;

    [SerializeField]
    private ItemBoxReference itemBoxReference;
    [SerializeField]
    private TMP_InputField countInput,keyInput;
    [SerializeField]
    private Button searchButton;

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private ToggleGroup itemSelectGroup;

    private DisplayList<ItemBoxReference, Item> itemBoxs; 
      
    private Item SelectItem; 

    protected override void Awake()
    {
        base.Awake(); 

        ReturnButton.onClick.AddListener(Close);
        itemBoxs = new DisplayList<ItemBoxReference, Item>(itemBoxReference, itemParent);

        ActionButton.onClick.AddListener(() =>
        {
            if (SelectItem.dataId == 0)
            {
                return;
            }
            AddPackageItem addPackageItem = new AddPackageItem
            {
                itemDataId = SelectItem.dataId,
                itemCount = getCount
            };
            GameActionManager.instance.QueueAction(addPackageItem);
        });

        searchButton.onClick.AddListener(RefreshPackage);
        ItemInformation.localScale = Vector3.zero;
        keyInput.onValueChanged.AddListener((string value) => { keyStr = value; });
        countInput.onValueChanged.AddListener((string value) =>
        {
            getCount = int.Parse(value);
        });
        
    }
    int getCount = 1;
    string keyStr = "";
    public override void OnEnable()
    {
        base.OnEnable(); 
    }

    public override void OnDisable()
    {
        base.OnDisable();
       
    }
    public override async Task InitData(string dataKey)
    {
        await base.InitData(dataKey);
        RefreshPackage();
        getCount = 1;
        keyStr = "";
        countInput.SetTextWithoutNotify("1");
        keyInput.SetTextWithoutNotify("");

    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        keyInput = FindChildGameObject<TMP_InputField>("Key");
        countInput = FindChildGameObject<TMP_InputField>("BuyCountValue");
        searchButton = FindChildGameObject<Button>("SearchButton");

        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ItemName = FindChildGameObject<TextMeshProUGUI>("ItemName"); 
        Type = FindChildGameObject<TextMeshProUGUI>("Type");
        Property = FindChildGameObject<TextMeshProUGUI>("Property");
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton"); 
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        itemParent = FindChildGameObject("ItemParent");
        itemSelectGroup = FindChildGameObject<ToggleGroup>("ItemParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        ItemInformation = FindChildGameObject("InformationObj");  
        InfoItemValueBg = FindChildGameObject("InfoItemValueBg");
        InfoItemValue = FindChildGameObject<Image>("InfoItemValue"); 
    }
     
     
    public override void Close()
    {
        base.Close(); 
    }
  

    private async void SelectPackageItem(Item item, bool selected = true)
    {
        if (selected)
        {
            if (item.dataId == 0)
            {
                ItemInformation.localScale = Vector3.zero;
              
            }
            else
            {
                ItemInformation.localScale = Vector3.one;
                SelectItem = item;
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                ItemIcon.sprite = itemData.icon;
                ItemIcon.enabled = true;
                ItemIcon.SetNativeSize();
                ItemName.SetADDText($"+ ",itemData.itemName," +");
                Type.SetSWText(itemData.type.ToString());
                Info.SetSWText(itemData.info);
                Property.SetSWText(itemData.GetProperty());
                
                InfoItemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
                InfoItemValue.fillAmount = item.value;
 
            }
        }
        else if (SelectItem.instanceId == item.instanceId)
        {
            ItemInformation.localScale = Vector3.zero;
        }
    }

 
    private async void RefreshPackage()
    {
        
        List<Item> items = new List<Item>();
        var itemDatas=await GameDataManager.instance.GetAllAsyncData<ItemData>();
        for(int i = 0; i < itemDatas.Count; i++)
        {
            if (string.IsNullOrEmpty(keyStr))
            {
                Item item = new Item
                {
                    dataId = itemDatas[i].id,
                    instanceId = i,

                };
                items.Add(item);
            }
            else if (itemDatas[i].itemName.Contains(keyStr))
            {
                Item item = new Item
                {
                    dataId = itemDatas[i].id,
                    instanceId = i,

                };
                items.Add(item);
            }
           
        } 
        await itemBoxs.InitListData(items, SelectPackageItem, toggleGroup: itemSelectGroup);
        // if(items.Count>0) { SelectPackageItem(items[0]); }
        itemBoxs.ClearSelect();
        ItemInformation.localScale = Vector3.zero; 
    }
}