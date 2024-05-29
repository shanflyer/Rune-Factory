using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.ReloadAttribute;

public class StoreCounterSetPanel : GamePanel<SetStoreCounterItem>
{
    [SerializeField]
    ItemBoxReference itemBoxReference;
    [SerializeField] 
    TextMeshProUGUI sellItemName;
    [SerializeField]
    TextMeshProUGUI priceValue;
    [SerializeField]
    TMP_InputField sellCount;
    [SerializeField]
    Button reduceButton, addButton,topButton;
    [SerializeField]
    Button changeItemButton, getItemDownButton;
    [SerializeField]
    Button CloseButton;
    protected override void Awake()
    {
        base.Awake();
        sellCount.onValueChanged.AddListener((string value) =>
        {
            int count = int.Parse(value);
            changeCount = count-storeCunterSetData.count;
            RefreshChangeCount(); 
        });
        addButton.onClick.AddListener(() =>
        {
            changeCount++;
            RefreshChangeCount(); 
        });
        reduceButton.onClick.AddListener(() =>
        { 
            changeCount--;
            RefreshChangeCount();
        });
        topButton.onClick.AddListener(() =>
        {
            changeCount = 99999; 
            RefreshChangeCount(); 
        });

        getItemDownButton.onClick.AddListener(GetItemDownAction);
        changeItemButton.onClick.AddListener(ChangeItemAction);

        CloseButton.onClick.AddListener(()=> {
            SetItemCountAction();
            Close();
        });
    }
    async void ChangeItemAction()
    {
        int packageId = CharacterManager.instance.controllerCharacter.characterPackage;

        PackageData packageData = PackageManager.instance.GetPackageData(packageId); 

        PackageList packageList = new PackageList
        {
            packageDatas = new System.Collections.Generic.List<PackageData>
            {
                packageData
            }
        };
        var WarehousePanel =await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
        WarehousePanel.SetSelectItemAction(SelectPackageItem, "更换");
    }
    int changeCount = 0;

    async void SelectPackageItem(Item item,bool select)
    {
        SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
        {
            itemId = item.dataId,
            storeCounterId = storeCunterSetData.storeCounterId,
            count = 0
        };
        UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
        UIManager.instance.CloseGamePanel<WarehousePanel>();
    }

    async void SetItemCountAction()
    {
         
        if (PlayerStoreManager.instance.GetRuntimeStoreCounter(storeCunterSetData.storeCounterId, out var runtimeStoreCounter))
        {
            int packageId = CharacterManager.instance.controllerCharacter.characterPackage;
            if (runtimeStoreCounter.itemId == 0 || runtimeStoreCounter.itemId == storeCunterSetData.itemId)
            { 
                if (changeCount < 0)
                {
                    int maxChange = runtimeStoreCounter.count - storeCunterSetData.count;
                    changeCount= math.clamp(changeCount, maxChange, 0);

                    changeCount = -math.clamp(-changeCount, 0, runtimeStoreCounter.count);
                    storeCunterSetData.count = runtimeStoreCounter.count + changeCount;
                    GameActionManager.instance.QueueAction(storeCunterSetData);

                    await PackageManager.instance.SetItemInPackage(new Item { dataId = storeCunterSetData.itemId, count = -changeCount }, packageId);

                }
                else
                {
                    int maxCount = PackageManager.instance.GetPackageItemCount(packageId, storeCunterSetData.itemId);
                    changeCount = math.clamp(changeCount, changeCount, maxCount);

                    if (PackageManager.instance.GetOutItenFromPackage(packageId, storeCunterSetData.itemId,  changeCount))
                    {
                        storeCunterSetData.count = runtimeStoreCounter.count + changeCount;
                        GameActionManager.instance.QueueAction(storeCunterSetData);
                    }
                }
            }
            else
            {
              
                storeCunterSetData.count = changeCount;
                if (PackageManager.instance.GetOutItenFromPackage(packageId, storeCunterSetData.itemId, storeCunterSetData.count))
                {
                    await PackageManager.instance.SetItemInPackage(new Item { dataId = runtimeStoreCounter.itemId, count = runtimeStoreCounter.count }, packageId);
                    GameActionManager.instance.QueueAction(storeCunterSetData);
                }
            }
        }
        else
        {
           
        }
        //changeCount = count - storeCunterSetData.count;
        //RefreshChangeCount(); 
    }
    async void GetItemDownAction()
    {
        int packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        if (PlayerStoreManager.instance.GetRuntimeStoreCounter(storeCunterSetData.storeCounterId, out var runtimeStoreCounter))
        {
            if (await PackageManager.instance.CheckPackageTryItemIn(packageId, storeCunterSetData.itemId, runtimeStoreCounter.count))
            {
              int count=await PackageManager.instance.SetItemInPackage(new Item { 
                    dataId= storeCunterSetData.itemId,
                    count= runtimeStoreCounter.count
                }, packageId);
              storeCunterSetData.count = count;
                GameActionManager.instance.QueueAction(storeCunterSetData);
            }
            else
            {
                InformationController.instance.AddInformation("背包空间不足!", PromptShow: true);
            }
        }
        Close();
    }
    void RefreshChangeCount()
    {
        int packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        int packageItemCount= PackageManager.instance.GetPackageItemCount(packageId, storeCunterSetData.itemId);
        int counterItemCount = 0;
        if (PlayerStoreManager.instance.GetRuntimeStoreCounter(storeCunterSetData.storeCounterId,out var runtimeStoreCounter))
        {
            counterItemCount = runtimeStoreCounter.count;
        }
        int totalCount = storeCunterSetData.count + changeCount;
        totalCount = math.clamp(totalCount, 0, packageItemCount + counterItemCount);
        changeCount = totalCount - storeCunterSetData.count;
        sellCount.SetTextWithoutNotify(totalCount.ToString());
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        sellItemName = FindChildGameObject<TextMeshProUGUI>("ItemName");
        priceValue = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        sellCount = FindChildGameObject<TMP_InputField>("SellCountValue");
        reduceButton = FindChildGameObject<Button>("ReduceButton");
        addButton = FindChildGameObject<Button>("AddButton");
        topButton = FindChildGameObject<Button>("Max");
        changeItemButton = FindChildGameObject<Button>("Change");
        getItemDownButton = FindChildGameObject<Button>("GetDown");
        CloseButton = FindChildGameObject<Button>("CloseButton");
    }
    SetStoreCounterItem storeCunterSetData;
    public override async void InitReferenceData(SetStoreCounterItem v)
    {
        base.InitReferenceData(v);
        storeCunterSetData = v;
        itemBoxReference.InitData(new Item
        {
            dataId = storeCunterSetData.itemId, count = 0
        });

        changeCount = 0;

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(storeCunterSetData.itemId);
        if (itemData != null)
        {
            sellItemName.text = itemData.itemName;
            priceValue.text = itemData.sellPrice.ToString();
            sellCount.text = storeCunterSetData.count.ToString();
        }
    }
}