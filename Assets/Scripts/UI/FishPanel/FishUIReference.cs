using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishUIReference : UIObjReference<FishReferenceData>
{
    [SerializeField]
    private TextMeshProUGUI FishName, record;

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Toggle toggle;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, isOn);
            }
        });
    }

    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.SetIsOnWithoutNotify(true);
        if (SelectAction != null)
        {
            SelectAction(data, true);
        }
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = transform.GetComponentInChildren<Toggle>();
        FishName = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon");
        record = FindChildGameObject<TextMeshProUGUI>("RecordValue");
    }

    public override async void InitData(FishReferenceData t, SelectAction<FishReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        if (data.record == 0)
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(1);
            FishName.text = "???";
            Icon.sprite = itemData.icon;
            record.text = "???";
        }
        else
        {
            FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(data.dataId);
            FishName.SetSWText(fishData.fishName);
            Icon.sprite = fishData.iconSprite;
            record.text = $"{data.record}cm";
        }
    }
}

public struct FishReferenceData : IReferenceData
{
    public int dataId;
    public int record;
    public List<int> seasons;
    public List<int> places;
}