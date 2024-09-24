using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishUIReference:UIObjReference<FishReferenceData>
{
   [SerializeField]
    TextMeshProUGUI FishName,record;
    [SerializeField]
    Image Icon;
    [SerializeField]
    Toggle toggle;
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
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = transform.GetComponentInChildren<Toggle>();
        FishName = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon");
        record = FindChildGameObject<TextMeshProUGUI>("RecordValue");
    }
    public override async Task InitData(FishReferenceData t, SelectAction<FishReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
      await  base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        if (data.record == 0)
        {
            ItemData itemData =await GameDataManager.instance.GetAsyncData<ItemData>(1);
            FishName.text = "???";
            Icon.sprite = itemData.icon;
            record.text = "???";
        }
        else
        {
            FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(data.dataId);
            FishName.text = fishData.fishName;
            Icon.sprite = fishData.iconSprite;
            record.text = $"{data.record}cm";
        }
        
    }

}
public struct FishReferenceData:IReferenceData
{
    public int dataId;
    public int record;
    public List<int> seasons;
    public List<int> places;
}