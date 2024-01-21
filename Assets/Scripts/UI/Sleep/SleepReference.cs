using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SleepReference : UIObjReference<SleepSetData>
{
    [SerializeField]
    Image Icon;
    [SerializeField]
    TextMeshProUGUI sleepText;
    [SerializeField]
    Button sleepButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        sleepButton = FindChildGameObject<Button>("Sleep");
        sleepText = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon"); 
    }
    void Awake() 
    {
        sleepButton.onClick.AddListener(()=> { SelectAction(data, true); });
        sleepText.text = data.text;
    } 
    public override void InitData(SleepSetData t, SelectAction<SleepSetData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        sleepText.text = data.text;
        Icon.sprite = t.icon;
    }
}