using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public delegate void SelectItemAction(ItemData ItemData, int count);
public struct SelectItemData:IReferenceData
{
    public int itemId;
    public int defaultCount;
    public int maxCount;
    public int minCount;
    public string leftName, rightName;
    public SelectItemAction leftSelectItemAction;
    public SelectItemAction rightSelectItemAction;
}
public class SelectItemCountPanel :GamePanel<SelectItemData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    InputField InputField;
    [SerializeField]
    Button AddButton, ReduceButton, AddToMaxButton, ReduceToMinButton;
    [SerializeField]
    Text Name;
    [SerializeField]
    Image ItemIcon;
    [SerializeField]
    Button ActionButton, ReturnButton;

    [SerializeField]
    Text leftName, rightName;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        InputField = FindChildGameObject<InputField>("InputField");
        AddButton = FindChildGameObject<Button>("AddButton");
        ReduceButton = FindChildGameObject<Button>("ReduceButton");
        AddToMaxButton = FindChildGameObject<Button>("AddToMaxButton");
        ReduceToMinButton = FindChildGameObject<Button>("ReduceToMinButton");
        Name = FindChildGameObject<Text>("Name");
        ItemIcon = FindChildGameObject<Image>("ItemIcon");
        ActionButton = FindChildGameObject<Button>("ActionButton");
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        leftName = FindChildGameObject<Text>("ReturnButtonName");
        rightName = FindChildGameObject<Text>("ActionButtonName");
    }
    protected override void Awake()
    {
        base.Awake();
        ActionButton.onClick.AddListener(()=> { selectItemData.rightSelectItemAction(itemData, selectCount); });
        ReturnButton.onClick.AddListener(() => 
        {
            if (selectItemData.leftSelectItemAction != null)
            {
                selectItemData.leftSelectItemAction(itemData, selectCount);
            }
            Close();
        });
        AddButton.onClick.AddListener(() =>
        {
            selectCount++;
        });
        ReduceButton.onClick.AddListener(() =>
        {
            selectCount--;
        });
        AddToMaxButton.onClick.AddListener(() =>
        {
            selectCount = selectItemData.maxCount;
        });
        ReduceToMinButton.onClick.AddListener(() =>
        {
            selectCount = selectItemData.minCount;
        });
        InputField.onValueChanged.AddListener((string value) =>
        {
            selectCount = int.Parse(value);
        });
        RefreshInitSelectCount();
    }
    int selectCount = 0;
    SelectItemData selectItemData;
    ItemData itemData;
    void RefreshInitSelectCount()
    {
        selectCount = math.clamp(selectCount, selectItemData.minCount, selectItemData.maxCount);
         
        InputField.SetTextWithoutNotify(selectCount.ToString());
        AddButton.interactable = AddToMaxButton.interactable = selectCount < selectItemData.maxCount;
        ReduceButton.interactable = ReduceButton.interactable = selectCount < selectItemData.minCount;
    }
    public override void InitReferenceData(SelectItemData v)
    {
        base.InitReferenceData(v);
        // 面板引用数据入口保持同步，物品数据加载异常统一进入日志。
        AsyncTaskRunner.Run(InitReferenceDataAsync(v), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(SelectItemData v)
    {
        selectItemData = v;
        selectCount = selectItemData.defaultCount;
        itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectItemData.itemId);
        Name.text = itemData.itemName;
        ItemIcon.sprite = itemData.icon;
        InputField.text = selectCount.ToString();

        if (string.IsNullOrEmpty(selectItemData.leftName))
        {
            leftName.text = "取消";
        }
        else
        {
            leftName.text = selectItemData.leftName;
        }
        if (string.IsNullOrEmpty(selectItemData.rightName))
        {
            rightName.text = "出售";
        }
        else
        {
            rightName.text = selectItemData.rightName;
        }
    }
}
