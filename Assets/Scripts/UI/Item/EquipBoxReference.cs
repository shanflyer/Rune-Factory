 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipBoxReference:UIObjReference<Equipment>
{
    [SerializeField]
    TextMeshProUGUI typeText,NameText;
    [SerializeField]
    Image equipMentIcon;
    [SerializeField]
    Button clickButton;
    [SerializeField]
    Transform itemValueBg;
    [SerializeField]
    Image itemValue;

    private void Awake()
    {
        clickButton.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(data);
            }
        });
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        typeText = FindChildGameObject<TextMeshProUGUI>("Type");
        equipMentIcon = FindChildGameObject<Image>("Icon");
        NameText = FindChildGameObject<TextMeshProUGUI>("Name");
        clickButton = GetComponent<Button>();
        itemValueBg = FindChildGameObject("ItemValueBg");
        itemValue = FindChildGameObject<Image>("ItemValue");
    }
    public override async Task InitData(Equipment t, SelectAction<Equipment> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        typeText.text = data.ItemType.ToString();
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(data.dataId);
        if(itemData!=null)
        {
            NameText.text = itemData.itemName;
            equipMentIcon.sprite = itemData.icon;

            NameText.enabled = true;
            equipMentIcon.enabled = true;
            itemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
           
        }
        else
        {
            NameText.enabled = false;
            equipMentIcon.enabled = false;
            itemValueBg.localScale = Vector3.zero;
            itemValue.fillAmount = t.itemValue;
        }
    }
}