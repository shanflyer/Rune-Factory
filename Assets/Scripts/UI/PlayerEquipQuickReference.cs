 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEquipQuickReference : UIObjReference<IReferenceData>
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
}
