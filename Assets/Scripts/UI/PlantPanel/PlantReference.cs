using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.UI;

public class PlantReference : UIObjReference<PlantData>
{
    [SerializeField]
    TextMeshProUGUI PlantName;
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
        PlantName = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon"); 
    }
    public override async Task InitData(PlantData t, SelectAction<PlantData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup; 
        PlantName.SetSWText(t.plantName);
        Icon.sprite = t.icon;

        if(t.icon.rect.size.y>40)
        {
            float sizeY = 40;
            float sizeX = t.icon.rect.size.x * 40 / t.icon.rect.size.y;
            Icon.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
        }
        else
        {
            Icon.SetNativeSize();
        } 
    }
}