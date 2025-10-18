using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine; 
using UnityEngine.UI;

public class PlantReference : UIObjReference<PlantData>
{
    [SerializeField]
    TextMeshProUGUI PlantName;
    [SerializeField]
    Image Icon;
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    TextMeshProUGUI fruitCount;
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data,index, isOn);
            }
        });
    }
    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.SetIsOnWithoutNotify(true);
        if (SelectAction != null)
        {
            SelectAction(data, index, true);
        }
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = transform.GetComponentInChildren<Toggle>();
        PlantName = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon");
        fruitCount = FindChildGameObject<TextMeshProUGUI>("FruitCount");
    }
    public override async Task InitData(PlantData t, SelectAction<PlantData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup; 

        if(GameDataSaveManager.instance.GetPlantFruitCount(t.id,out var count))
        {
            PlantName.SetSWText(t.plantName);
            Icon.sprite = t.icon;

            if (t.icon.rect.size.y > 40)
            {
                float sizeY = 40;
                float sizeX = t.icon.rect.size.x * 40 / t.icon.rect.size.y;
                Icon.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
            }
            else
            {
                Icon.SetNativeSize();
            }
            fruitCount.SetADDText("总收获:", count);
        }
        else
        {
            PlantName.text = "????";
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(1);
            Icon.sprite = itemData.icon;
            Icon.SetNativeSize();
            fruitCount.SetADDText("收获数量:", 0);
        }
       
    }
}