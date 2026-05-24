using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarehousePanel : GamePanel<PackageList>
{
    [SerializeField]
    private PackageSelectReference packageSelect;

    [SerializeField]
    private Transform packageSelectParent;

    [SerializeField]
    private ToggleGroup packageSelectGroup;

    [SerializeField]
    private Button packageLevelUp;

    [SerializeField]
    private Transform ItemInformation;

    [SerializeField]
    private Button ShortCutActionButton;

    [SerializeField]
    private TextMeshProUGUI Title;

    [SerializeField]
    private TextMeshProUGUI caseCount;

    [SerializeField]
    private Image ItemIcon;

    [SerializeField]
    private TextMeshProUGUI ItemName;

    [SerializeField]
    private TextMeshProUGUI Price;

    [SerializeField]
    private TextMeshProUGUI Type, Property, Info;

    [SerializeField]
    private Button ActionButton, ReturnButton;

    [SerializeField]
    private TextMeshProUGUI ActionName;

    [SerializeField]
    private Transform InfoItemValueBg;

    [SerializeField]
    private Image InfoItemValue;

    [SerializeField]
    private ItemBoxReference itemBoxReference;

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private ToggleGroup itemSelectGroup;

    private DisplayList<ItemBoxReference, Item> itemBoxs;
    private DisplayList<PackageSelectReference, PackageData> packageSelectList;

    private PackageList packageList;
    private PackageData selectPackageData;
    private Item SelectItem;
    private SelectAction<Item> selectItemAction;

    protected override void Awake()
    {
        base.Awake();
        var packageDatas = packageList.packageDatas;

        ReturnButton.onClick.AddListener(ReturenAction);
        itemBoxs = new DisplayList<ItemBoxReference, Item>(itemBoxReference, itemParent);

        ActionButton.onClick.AddListener(() =>
        {
            if (selectItemAction != null)
            {
                selectItemAction(SelectItem,0);
            }
        });

        ItemInformation.localScale = Vector3.zero;

        packageSelectList = new DisplayList<PackageSelectReference, PackageData>(packageSelect, packageSelectParent);
        packageLevelUp.onClick.AddListener(TryPackageLevelUp);

        ShortCutActionButton.onClick.AddListener(() =>
        {
            RunLifecycleTask(async token =>
            {
                if (SelectItem.instanceId != 0)
                {
                    if (SelectItem.dataId != 0)
                    {
                        if (await SelectItem.IsSingleItem())
                        {
                            if (ShouldStopLifecycleTask(token))
                            {
                                return;
                            }

                            SetShortcutItem setShortcutItem = new SetShortcutItem
                            {
                                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                                Item = SelectItem
                            };
                            GameActionManager.instance.QueueAction(setShortcutItem);
                        }
                        else
                        {
                            Item newItem = new Item
                            {
                                dataId = SelectItem.dataId,
                                count = PackageManager.instance.GetPackageItemCount(selectPackageData.instanceId, SelectItem.dataId)
                            };
                            newItem = await Item.SetValue(newItem, SelectItem.value);
                            if (ShouldStopLifecycleTask(token))
                            {
                                return;
                            }

                            SetShortcutItem setShortcutItem = new SetShortcutItem
                            {
                                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                                Item = newItem
                            };
                            GameActionManager.instance.QueueAction(setShortcutItem);
                        }
                    }
                    SetPackageSelectItem setPackageSelectItem = new SetPackageSelectItem
                    {
                        packageId = selectPackageData.instanceId,
                        selectItem = SelectItem.instanceId != 0 ? SelectItem.instanceId : SelectItem.dataId
                    };
                    GameActionManager.instance.QueueAction(setPackageSelectItem);
                }
            }, nameof(ShortCutActionButton));
        });
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPackage>(RefreshPackage);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared && GameActionManager.HasInstance)
        {
            GameActionManager.instance.RemoveListener<RefreshPackage>(RefreshPackage);
            GameActionManager.instance.RemoveListener<RefreshShortcut>(RefreshShortcut);
        }
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        packageSelect = FindChildGameObject<PackageSelectReference>("PackageSelect");
        packageSelectParent = FindChildGameObject("PackageSelectParent");

        Title = FindChildGameObject<TextMeshProUGUI>("Title");
        caseCount = FindChildGameObject<TextMeshProUGUI>("CaseCount");
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ItemName = FindChildGameObject<TextMeshProUGUI>("ItemName");
        Price = FindChildGameObject<TextMeshProUGUI>("Price");
        Type = FindChildGameObject<TextMeshProUGUI>("Type");
        Property = FindChildGameObject<TextMeshProUGUI>("Property");
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionButton = FindChildGameObject<Button>("ActionButton");
        ActionName = FindChildGameObject<TextMeshProUGUI>("ActionName");
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        itemParent = FindChildGameObject("ItemParent");
        itemSelectGroup = FindChildGameObject<ToggleGroup>("ItemParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        ItemInformation = FindChildGameObject("InformationObj");
        packageSelectGroup = FindChildGameObject<ToggleGroup>("PackageSelectParent");
        Price = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
        packageLevelUp = FindChildGameObject<Button>("LevelUp");
        InfoItemValueBg = FindChildGameObject("InfoItemValueBg");
        InfoItemValue = FindChildGameObject<Image>("InfoItemValue");
        ShortCutActionButton = FindChildGameObject<Button>("ShortCutAction");
    }

    public override void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        // 仓库面板可能被连续打开，旧列表刷新必须绑定当前打开 token。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(PackageList v, System.Threading.CancellationToken cancellationToken)
    {
        packageList = v;
        ShortCutActionButton.transform.localScale = v.canSetShortcut ? Vector3.one : Vector3.zero;
        await packageSelectList.InitListData(packageList.packageDatas, SelectPackage, packageSelectGroup, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        SelectPackage(packageList.packageDatas[0],0);
        //this.RefreshPackage();
        //RefreshPackage();
    }

    private void ReturenAction()
    {
        FightController fightController = FightController.instance;
        if (fightController && FightManager.instance.isFight)
        {
            SkillPauseAction skillPauseAction = new SkillPauseAction
            {
                pause = false,
            };
            Debug.Log($"使用道具-暂停");
            GameActionManager.instance.QueueAction(skillPauseAction, true);
        }
        Close();
    }

    public override void Close()
    {
        RunLifecycleTask(_ => CloseAsync(), nameof(Close));
    }

    async System.Threading.Tasks.Task CloseAsync()
    {
        base.Close();
        if (otherSelectItemAction != null)
        {
            //  otherSelectItemAction(default(Item), selectPackageData.instanceId);
        }

        var ShortcutPanel = await UIManager.instance.GetGamePanel<ShortcutPanel>();
        if (ShortcutPanel != null) ShortcutPanel.ChangeLayer(2);
    }

    private void TryPackageLevelUp()
    {
        RunLifecycleTask(TryPackageLevelUpAsync, nameof(TryPackageLevelUp));
    }

    async System.Threading.Tasks.Task TryPackageLevelUpAsync(System.Threading.CancellationToken cancellationToken)
    {
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        if (packageSetData && packageSetData.canLevelUp)
        {
            int cost = packageSetData.levelUpCost * selectPackageData.caseCount;
            var notice = string.Format(LanguageManage.SwitchStr("拓展{0}空间?"),
                LanguageManage.SwitchStr(packageSetData.packageName));
            PayManager.instance.PayAction("空间拓展", notice, cost, PayType.金币, (bool result) =>
            {
                if (result)
                {
                    PackageManager.instance.AddPackageUpLevel(selectPackageData.instanceId);
                }
            });
        }
    }

    private void SelectPackage(PackageData packageData,int index, bool selected = true)
    {
        if (selected)
        {
            selectPackageData = packageData;
            RefreshPackage();
            ItemInformation.localScale = Vector3.zero;
        }
    }

    public void SetSelectItemAction(SelectAction<Item> selectItemAction, string actionName)
    {
        ActionName.SetSWText(actionName);
        this.selectItemAction = selectItemAction;
    }

    public void SetOtherSelectAction(SelectAction<Item> selectItemAction)
    {
        otherSelectItemAction = selectItemAction;
    }

    private SelectAction<Item> otherSelectItemAction;

    private void SelectPackageItem(Item item,int index, bool selected = true)
    {
        RunLifecycleTask(token => SelectPackageItemAsync(item, index, selected, token), nameof(SelectPackageItem));
    }

    async System.Threading.Tasks.Task SelectPackageItemAsync(Item item,int index, bool selected, System.Threading.CancellationToken cancellationToken)
    {
        if (selected)
        {
            if (item.dataId == 0)
            {
                ItemInformation.localScale = Vector3.zero;
                if (otherSelectItemAction != null)
                {
                    otherSelectItemAction.Invoke(default(Item),0);
                }
            }
            else
            {
                ItemInformation.localScale = Vector3.one;
                SelectItem = item;
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                if (ShouldStopLifecycleTask(cancellationToken))
                {
                    return;
                }

                ItemIcon.sprite = itemData.icon;
                ItemIcon.enabled = true;
                //ItemIcon.SetNativeSize();
                ItemName.SetADDText($"+ ", itemData.itemName, " +");
                Type.SetSWText(itemData.type.ToString());
                Info.SetSWText(itemData.GetInfo());
                Property.SetSWText(itemData.GetProperty());
                Price.SetSWText(itemData.sellPrice.ToString());
                InfoItemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
                InfoItemValue.fillAmount = item.value;

                if (otherSelectItemAction != null)
                {
                    otherSelectItemAction.Invoke(item,0);
                }
            }
        }
        else if (SelectItem.instanceId == item.instanceId)
        {
            ItemInformation.localScale = Vector3.zero;
        }
    }

    private void RefreshPackage(RefreshPackage RefreshPackage)
    {
        this.RefreshPackage();
    }

    private void RefreshShortcut(RefreshShortcut refreshShortcut)
    {
        if (refreshShortcut.packageId == selectPackageData.instanceId)
        {
            selectPackageData = PackageManager.instance.GetPackageData(selectPackageData.instanceId);
            RefreshPackage();
        }
    }

    private void RefreshPackage()
    {
        RunLifecycleTask(RefreshPackageAsync, nameof(RefreshPackage));
    }

    async System.Threading.Tasks.Task RefreshPackageAsync(System.Threading.CancellationToken cancellationToken)
    {
        //selectPackageData = packageList.packageDatas[selectIndex];
        if (selectPackageData.dataId == 0)
        {
            return;
        }
        List<Item> items = new List<Item>();
        if (selectPackageData.items != null)
        {
            if (packageList.itemMatchData.matchValues == null)
            {
                items.AddRange(selectPackageData.items);
            }
            else
            {
                for (int i = 0; i < selectPackageData.items.Count; i++)
                {
                    var item = selectPackageData.items[i];
                    item.locked = !await packageList.itemMatchData.MatchAction(item);
                    if (ShouldStopLifecycleTask(cancellationToken))
                    {
                        return;
                    }

                    items.Add(item);
                }
            }
        }

        for (int i = items.Count; i < selectPackageData.caseCount; i++)
        {
            items.Add(default(Item));
        }
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        Title.SetSWText(packageSetData.packageName);

        await itemBoxs.InitListData(items, SelectPackageItem, toggleGroup: itemSelectGroup, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        // if(items.Count>0) { SelectPackageItem(items[0]); }
        itemBoxs.ClearSelect();
        ItemInformation.localScale = Vector3.zero;

        caseCount.SetSWText($"{selectPackageData.items.Count}/{selectPackageData.caseCount}");

        bool canLevelUp = packageSetData.canLevelUp ? selectPackageData.level < packageSetData.maxLevel - 1 : false;
        packageLevelUp.transform.localScale = canLevelUp ? Vector3.one : Vector3.zero;
    }
}
