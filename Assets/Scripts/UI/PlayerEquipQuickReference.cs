 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct TeamerEquipAndProperty : IReferenceData
{
    public CharacterEquipAndPropertyData[] characterEquipAndPropertyDatas;
}
public class PlayerEquipQuickReference : UIObjReference<TeamerEquipAndProperty>
{
    [SerializeField]
    Dropdown playerSelcet;
    [SerializeField]
    Image icon;
    [SerializeField]
    Text type, Name;
    [SerializeField]
    Text Hp, Rp, At, Df;
    [SerializeField]
    Text Weapon, Clothes;
    [SerializeField]
    Image HpUp, HpDown, AtUp, AtDown, DfUp, DfDown;
    [SerializeField]
    Text Attribute;

    private void Awake()
    {
        playerSelcet = FindChildGameObject<Dropdown>("");
        icon = FindChildGameObject<Image>("");
        type = FindChildGameObject<Text>("Type");
        Name=FindChildGameObject<Text>("Name");
        Hp = FindChildGameObject<Text>("Hp");
        Rp = FindChildGameObject<Text>("Rp");
        At = FindChildGameObject<Text>("At");
        Df = FindChildGameObject<Text>("Df");
        Weapon = FindChildGameObject<Text>("Weapon");
        Clothes = FindChildGameObject<Text>("Clothes");
        HpUp = FindChildGameObject<Image>("HpUp");
        HpDown = FindChildGameObject<Image>("HpDown");
        AtUp = FindChildGameObject<Image>("AtUp");
        AtDown = FindChildGameObject<Image>("AtDown");
        DfUp = FindChildGameObject<Image>("DfUp");
        DfDown = FindChildGameObject<Image>("DfDown");
        Attribute = FindChildGameObject<Text>("Attribute");
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        playerSelcet = FindChildGameObject<Dropdown>("playerSelcet");
        icon = FindChildGameObject<Image>("icon");
        type = FindChildGameObject<Text>("type");
        Name = FindChildGameObject<Text>("Name");
        Hp = FindChildGameObject<Text>("Hp");
        Rp = FindChildGameObject<Text>("Rp");
        At = FindChildGameObject<Text>("At");
        Df = FindChildGameObject<Text>("Df");
        Weapon = FindChildGameObject<Text>("Weapon");
        Clothes = FindChildGameObject<Text>("Clothes");

        HpUp = FindChildGameObject<Image>("HpUp");
        HpDown = FindChildGameObject<Image>("HpDown");
        AtDown = FindChildGameObject<Image>("AtDown");
        AtUp = FindChildGameObject<Image>("AtUp");
        DfUp = FindChildGameObject<Image>("DfUp");
        DfDown = FindChildGameObject<Image>("DfDown");

        Attribute = FindChildGameObject<Text>("Attribute");
    }
    TeamerEquipAndProperty teamerEquipAndProperty;

    int selectIndex=-1;
    public override  void InitData(TeamerEquipAndProperty t, SelectAction<TeamerEquipAndProperty> SelectAction = null)
    {
        base.InitData(t, SelectAction);
        teamerEquipAndProperty = t;
        playerSelcet.options.Clear();
        for (int i = 0; i < teamerEquipAndProperty.characterEquipAndPropertyDatas.Length; i++)
        {
           
            CharacterEquipAndPropertyData characterEquipAndPropertyData = teamerEquipAndProperty.characterEquipAndPropertyDatas[i];
             
            playerSelcet.options.Add(new Dropdown.OptionData(characterEquipAndPropertyData.name));
        }
        playerSelcet.onValueChanged.AddListener((int index) =>
        {
            if (selectIndex != index)
            {
                selectIndex = index;
                RefreshCharacter();
            } 
        });
    }
    async void RefreshCharacter()
    {
        if (selectIndex < teamerEquipAndProperty.characterEquipAndPropertyDatas.Length)
        {
            CharacterEquipAndPropertyData characterEquipAndPropertyData = teamerEquipAndProperty.characterEquipAndPropertyDatas[selectIndex];
            icon.sprite = characterEquipAndPropertyData.icon;
            icon.SetNativeSize();
            Name.text = characterEquipAndPropertyData.name;
            Hp.text = $"HP:{characterEquipAndPropertyData.characterProperty.HP}/{characterEquipAndPropertyData.characterProperty.MaxHP}";
            Rp.text = $"RP:{characterEquipAndPropertyData.characterProperty.Power}/{characterEquipAndPropertyData.characterProperty.MaxPower}";
            At.text = $"AT:{characterEquipAndPropertyData.characterProperty.AT}";
            Df.text = $"DF:{characterEquipAndPropertyData.characterProperty.DF}";

            Equip equip = characterEquipAndPropertyData.equip;
            ItemData weapon = await GameDataManager.instance.GetAsyncData<ItemData>(equip.weapon);
            string weaponStr =weapon? weapon.name:"ÎÞ";
            Weapon.text = $"{LanguageManage.SwitchStr("ÎäÆ÷:")}{weaponStr}";

            ItemData colthes = await GameDataManager.instance.GetAsyncData<ItemData>(equip.clothes);
            string colthesStr = colthes ? colthes.name : "ÎÞ";
            Clothes.text = $"{LanguageManage.SwitchStr("·À¾ß:")}{colthesStr}";
            Attribute.text = characterEquipAndPropertyData.attributeType.ToString();
            HpDown.enabled = HpUp.enabled = AtUp.enabled = AtDown.enabled = DfUp.enabled = DfDown.enabled = false;
        }
    }

}
