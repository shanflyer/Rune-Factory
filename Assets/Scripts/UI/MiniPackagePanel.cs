using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniPackagePanel : GamePanel<PackageList>
{
    [SerializeField]
    private Button packageLevelUp;

    [SerializeField]
    private TextMeshProUGUI Title;

    [SerializeField]
    private TextMeshProUGUI caseCount;

    [SerializeField]
    private Button ReturnButton;

    [SerializeField]
    private ItemBoxReference itemBoxReference;

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private ToggleGroup itemSelectGroup;
    [SerializeField]
    private float infoOffsetY =330f;
    private DisplayList<ItemBoxReference, Item> itemBoxs;

    private PackageList packageList;
    private PackageData selectPackageData;
    private Item SelectItem;
    private SelectAction<Item> selectItemAction;
    private string actionName;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        Title = FindChildGameObject<TextMeshProUGUI>("Title");
        caseCount = FindChildGameObject<TextMeshProUGUI>("CaseCount");
        itemBoxReference = FindChildGameObject<ItemBoxReference>("ItemBoxReference");
        itemParent = FindChildGameObject("ItemParent");
        itemSelectGroup = FindChildGameObject<ToggleGroup>("ItemParent");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        packageLevelUp = FindChildGameObject<Button>("LevelUp");
    }

    protected override void Awake()
    {
        base.Awake();
        var packageDatas = packageList.packageDatas;

        ReturnButton.onClick.AddListener(Close);
        itemBoxs = new DisplayList<ItemBoxReference, Item>(itemBoxReference, itemParent);
        packageLevelUp.onClick.AddListener(TryPackageLevelUp);
    }

    private void TryPackageLevelUp()
    {
        RunLifecycleTask(TryPackageLevelUpAsync, nameof(TryPackageLevelUp));
    }

    private async System.Threading.Tasks.Task TryPackageLevelUpAsync(System.Threading.CancellationToken cancellationToken)
    {
        PackageSetData packageSetData = await GameDataManager.instance.GetAsyncData<PackageSetData>(selectPackageData.dataId);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        if (packageSetData && packageSetData.canLevelUp)
        {
            int cost = packageSetData.levelUpCost * selectPackageData.caseCount;
            string notice = string.Format(LanguageManage.SwitchStr("拓展{0}空间?"), packageSetData.packageName);
            PayManager.instance.PayAction("空间拓展", notice, cost, PayType.金币, (bool result) =>
            {
                if (result)
                {
                    PackageManager.instance.AddPackageUpLevel(selectPackageData.instanceId);
                }
            });
        }
    }
    private static HidePanels hidePanels = new HidePanels
    {
        type = new List<Type>
        {
        typeof(ShortcutPanel),
        typeof(OperateButtonPanel),
        typeof(OtherFuntionPanel),
        typeof(PermissionPanel),
        typeof(ScreenControllerPanel),
        typeof(CharacterButtonPanel)
        }
    };
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPackage>(RefreshPackage);
        GameActionManager.instance.AddListener<RefreshShortcut>(RefreshShortcut);
        hidePanels.hide = true;
        GameActionManager.instance.QueueAction(hidePanels);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared && GameActionManager.HasInstance)
        {
            GameActionManager.instance.RemoveListener<RefreshPackage>(RefreshPackage);
            GameActionManager.instance.RemoveListener<RefreshShortcut>(RefreshShortcut);
            hidePanels.hide = false;
            GameActionManager.instance.QueueAction(hidePanels);
        }

    }

    private SelectAction<Item> otherSelectItemAction;
    public override void InitReferenceData(PackageList v)
    {
        base.InitReferenceData(v);
        packageList = v;

        selectPackageData = packageList.packageDatas[0];
        RefreshPackage();

        //this.RefreshPackage();
        //RefreshPackage();
    }
    private void SelectPackageItem(Item item, int index, bool selected = true)
    {
        RunLifecycleTask(token => SelectPackageItemAsync(item, index, selected, token), nameof(SelectPackageItem));
    }

    private async System.Threading.Tasks.Task SelectPackageItemAsync(Item item, int index, bool selected, System.Threading.CancellationToken cancellationToken)
    {
        if (selected)
        {
            SelectItem = item;
            if (item.dataId == 0)
            {
                UIManager.instance.CloseGamePanel<ItemInfoPanel>();
            }
            else
            {
                ItemInfo itemInfo = new ItemInfo
                {
                    item = item,
                    showClose = false,
                    action = selectItemAction,
                    ActionName = actionName,
                    OffsetPos=infoOffsetY
                };
                await UIManager.instance.ShowGamePanel<ItemInfoPanel, ItemInfo>(itemInfo);
                if (ShouldStopLifecycleTask(cancellationToken))
                {
                    return;
                }

                if (otherSelectItemAction != null)
                {
                    otherSelectItemAction.Invoke(item, index);
                }
            }
        }
        else if (SelectItem.instanceId == item.instanceId)
        {
            SelectItem = default(Item);
            UIManager.instance.CloseGamePanel<ItemInfoPanel>();
        }
    }

    public void SetSelectItemAction(SelectAction<Item> selectItemAction, string actionName)
    {
        this.actionName = actionName;
        this.selectItemAction = selectItemAction;
    }

    public void SetOtherSelectAction(SelectAction<Item> selectItemAction)
    {
        otherSelectItemAction = selectItemAction;
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

    public void DefaultSelect()
    {
        itemBoxs.SelectDefault();
    }
    private void RefreshPackage()
    {
        RunLifecycleTask(RefreshPackageAsync, nameof(RefreshPackage));
    }

    private async System.Threading.Tasks.Task RefreshPackageAsync(System.Threading.CancellationToken cancellationToken)
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

        itemBoxs.ClearSelect();

        caseCount.text = $"{selectPackageData.items.Count}/{selectPackageData.caseCount}";

        bool canLevelUp = packageSetData.canLevelUp ? selectPackageData.level < packageSetData.maxLevel - 1 : false;
        packageLevelUp.transform.localScale = canLevelUp ? Vector3.one : Vector3.zero;
    }
    public override void Close()
    {
        base.Close();
        UIManager.instance.CloseGamePanel<ItemInfoPanel>();
    }
}
