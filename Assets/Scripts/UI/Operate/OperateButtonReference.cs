using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperateButtonReference : UIObjReference<OperateDataReferenceData>
{
    [SerializeField]
    TextMeshProUGUI nameText;
    [SerializeField]
    Button button;
    [SerializeField]
    Transform ValueBg;
    [SerializeField]
    Image Value;
    [SerializeField]
    Image Icon;  
    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(data);
            }
        });
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshItemValue>(RefreshItemValue);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshItemValue>(RefreshItemValue);
    }
    void RefreshItemValue(RefreshItemValue refreshItemValue)
    {
        if (refreshItemValue.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            if (refreshItemValue.itemId == linkItemId)
            {
                Value.fillAmount = refreshItemValue.itemValue;
            }
        }
    }
    int linkItemId = 0;
    
    public override async void InitData(OperateDataReferenceData t, SelectAction<OperateDataReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup); 
        nameText.text = t.operateData.operateName;
        if (t.operateData.linkItem == 0)
        {
            Icon.enabled = false;
            ValueBg.localScale = Vector3.zero;
        }
        else
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(t.operateData.linkItem);
            if(itemData!= null)
            {
                linkItemId = t.operateData.linkItem;
                Icon.enabled = true;
                Icon.sprite = itemData.icon; 
                ValueBg.localScale =itemData.itemValue? Vector3.one:Vector3.zero;
            }
            else
            {
                Icon.enabled = false;
                ValueBg.localScale = Vector3.zero;
            } 
        } 
        this.SelectAction = SelectAction;
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        nameText = FindChildGameObject<TextMeshProUGUI>("Name");
        button = GetComponent<Button>();
        ValueBg = FindChildGameObject("ValueBg");
        Value = FindChildGameObject<Image>("Value");
        Icon = FindChildGameObject<Image>("Icon");
    }
}
