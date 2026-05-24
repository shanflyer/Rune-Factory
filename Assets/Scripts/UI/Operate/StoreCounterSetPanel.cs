using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

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
    void ChangeItemAction()
    {
        RunLifecycleTask(ChangeItemActionAsync, nameof(ChangeItemAction));
    }

    async System.Threading.Tasks.Task ChangeItemActionAsync(System.Threading.CancellationToken cancellationToken)
    {
        int packageId = CharacterManager.instance.controllerCharacter.characterPackage;

        PackageData packageData = PackageManager.instance.GetPackageData(packageId); 

        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>
            {
                packageData
            }
        };
        var WarehousePanel =await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        WarehousePanel.SetSelectItemAction(SelectPackageItem, "更换");
    }
    int changeCount = 0;

    void SelectPackageItem(Item item, int index, bool select)
    {
        RunLifecycleTask(token => SelectPackageItemAsync(item, index, select, token), nameof(SelectPackageItem));
    }

    async System.Threading.Tasks.Task SelectPackageItemAsync(Item item, int index, bool select, System.Threading.CancellationToken cancellationToken)
    {
        SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
        {
            itemId = item.dataId,
            storeCounterId = storeCunterSetData.storeCounterId,
            count = 0
        };
       await UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        UIManager.instance.CloseGamePanel<WarehousePanel>();
    }

    void SetItemCountAction()
    {
        RunLifecycleTask(SetItemCountActionAsync, nameof(SetItemCountAction));
    }

    async System.Threading.Tasks.Task SetItemCountActionAsync(System.Threading.CancellationToken cancellationToken)
    {
         
        if (PlayerStoreManager.instance.GetRuntimeStoreCounter(storeCunterSetData.storeCounterId, out var runtimeStoreCounter))
        {
            int packageId = CharacterManager.instance.controllerCharacter.characterPackage;
            if (runtimeStoreCounter.itemData==null || runtimeStoreCounter.itemData.id == storeCunterSetData.itemId)
            { 
                if (changeCount < 0)
                {
                    int maxChange = runtimeStoreCounter.count - storeCunterSetData.count;
                    changeCount= math.clamp(changeCount, maxChange, 0);

                    changeCount = -math.clamp(-changeCount, 0, runtimeStoreCounter.count);
                    storeCunterSetData.count = runtimeStoreCounter.count + changeCount;
                    GameActionManager.instance.QueueAction(storeCunterSetData);

                    await PackageManager.instance.SetItemInPackage(new Item { dataId = storeCunterSetData.itemId, count = -changeCount }, packageId);
                    if (ShouldStopLifecycleTask(cancellationToken))
                    {
                        return;
                    }

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
                    await PackageManager.instance.SetItemInPackage(new Item { dataId = runtimeStoreCounter.itemData.id, count = runtimeStoreCounter.count }, packageId);
                    if (ShouldStopLifecycleTask(cancellationToken))
                    {
                        return;
                    }

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
    void GetItemDownAction()
    {
        RunLifecycleTask(GetItemDownActionAsync, nameof(GetItemDownAction));
    }

    async System.Threading.Tasks.Task GetItemDownActionAsync(System.Threading.CancellationToken cancellationToken)
    {
        int packageId = CharacterManager.instance.controllerCharacter.characterPackage;
        if (PlayerStoreManager.instance.GetRuntimeStoreCounter(storeCunterSetData.storeCounterId, out var runtimeStoreCounter))
        {
            if (PackageManager.instance.CheckPackageTryItemIn(packageId, storeCunterSetData.itemId,
                    runtimeStoreCounter.count))
            {
              int count=await PackageManager.instance.SetItemInPackage(new Item { 
                    dataId= storeCunterSetData.itemId,
                    count= runtimeStoreCounter.count
                }, packageId);
              if (ShouldStopLifecycleTask(cancellationToken))
              {
                  return;
              }

              storeCunterSetData.count = count;
                GameActionManager.instance.QueueAction(storeCunterSetData);
            }
            else
            {
                InformationController.instance.AddInformation(LanguageManage.SwitchStr("背包空间不足!"), PromptShow: true);
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
    public override void InitReferenceData(SetStoreCounterItem v)
    {
        base.InitReferenceData(v);
        // 柜台设置面板绑定生命周期，关闭后旧物品数据不再写 UI。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(SetStoreCounterItem v, System.Threading.CancellationToken cancellationToken)
    {
        storeCunterSetData = v;
       await itemBoxReference.InitData(new Item
        {
            dataId = storeCunterSetData.itemId, count = 0
        }, null, null, cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        changeCount = 0;

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(storeCunterSetData.itemId);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        if (itemData != null)
        {
            sellItemName.SetSWText(itemData.itemName);
            priceValue.text = itemData.sellPrice.ToString();
            sellCount.text = storeCunterSetData.count.ToString();
        }
    }
}
