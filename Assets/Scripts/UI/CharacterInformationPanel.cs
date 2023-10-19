using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterInformationPanel :GamePanel<CharacterInformationData>
{
    [SerializeField]
    Image characterHead;
    [SerializeField]
    Image Attribute;
    [SerializeField]
    TextMeshProUGUI CharacterName;
    [SerializeField]
    TextMeshProUGUI State;
    [SerializeField]
    Image HPSliderValue,RPSliderValue,EXPSliderValue;
    [SerializeField]
    TextMeshProUGUI HPValue, RPValue, EXPValue;
    [SerializeField]
    TextMeshProUGUI AttackValue, DefenseValue;
    [SerializeField]
    TextMeshProUGUI LevelValue;
    [SerializeField]
    Image AttackUp, AttackDown, DefenseUp, DefenseDown;
    [SerializeField]
    EquipBoxReference WeaponBox, ClothesBox;
    [SerializeField]
    Button visitButton, closeButton;

    void SelectEquipReference(Equipment equipment,bool selected=false)
    {
        if (equipment.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            OpenPackage openPackage = new OpenPackage
            {
                packageId = -1,
                selectActionName = "×°±¸",
                targetObj = equipment.characterId,
                selectActionId=GameCommon.selectEquipBoxAction
            };
            GameActionManager.instance.QueueAction(openPackage);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(Close);
        visitButton.onClick.AddListener(() =>
        {
            VisitNPC visitNPC = new VisitNPC
            {
                sourceId = CharacterManager.instance.controllerCharacter.instanceId,
                targetId = characterId
            };
            GameActionManager.instance.QueueAction(visitNPC);
        });
        
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        characterHead = FindChildGameObject<Image>("HeadValue");
        Attribute = FindChildGameObject<Image>("Attribute");
        CharacterName = FindChildGameObject<TextMeshProUGUI>("CharacterName");
        State = FindChildGameObject<TextMeshProUGUI>("State");
        RPSliderValue = FindChildGameObject<Image>("HPSliderValue");
        HPSliderValue = FindChildGameObject<Image>("RPSliderValue");
        EXPSliderValue = FindChildGameObject<Image>("RPSliderValue");
        HPValue = FindChildGameObject<TextMeshProUGUI>("HPValue");
        RPValue = FindChildGameObject<TextMeshProUGUI>("RPValue");
        EXPValue = FindChildGameObject<TextMeshProUGUI>("RPValue");
        AttackValue = FindChildGameObject<TextMeshProUGUI>("AttackValue");
        DefenseValue = FindChildGameObject<TextMeshProUGUI>("DefenseValue");
        LevelValue = FindChildGameObject<TextMeshProUGUI>("Level");
        AttackUp = FindChildGameObject<Image>("AttackUp");
        AttackDown = FindChildGameObject<Image>("AttackDown");
        DefenseUp = FindChildGameObject<Image>("DefenseUp");
        DefenseDown = FindChildGameObject<Image>("DefenseDown");

        WeaponBox = FindChildGameObject<EquipBoxReference>("Weapon");
        ClothesBox = FindChildGameObject<EquipBoxReference>("Clothes");
        visitButton = FindChildGameObject<Button>("Visit");
        closeButton = FindChildGameObject<Button>("Close");
          
    }
    int characterId;
    public override void InitReferenceData(CharacterInformationData v)
    {
        base.InitReferenceData(v);
        characterHead.sprite = v.head;
        characterId = v.characterId;
        CharacterName.text = v.name;
        State.text = v.NPCState.ToString();
        State.color = NPC.GetStateColor(v.NPCState);

        int maxHP = v.characterProperty.MaxHP;
        int maxRP = v.characterProperty.MaxPower;
        int HP = v.characterProperty.HP;
        int RP = v.characterProperty.Power;

        HPSliderValue.fillAmount = HP / (float)maxHP;
        RPSliderValue.fillAmount = RP / (float)maxRP;
        HPValue.text = $"{HP}/{maxHP}";
        RPValue.text = $"{RP}/{maxRP}";

        EXPValue.text = $"{v.exp.nowExp}/{v.exp.nowLevelExp}";
        EXPSliderValue.fillAmount = v.exp.nowExp / (float)v.exp.nowLevelExp;

        AttackValue.text = v.characterProperty.AT.ToString();
        DefenseValue.text = v.characterProperty.DF.ToString();
        AttackUp.enabled = AttackDown.enabled = DefenseDown.enabled = DefenseUp.enabled = false;

        LevelValue.text = v.level.ToString();

        WeaponBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.weapon,
            ItemType=ItemType.ÎäÆ÷
        }, SelectEquipReference);
        ClothesBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.clothes,
            ItemType = ItemType.·À¾ß
        }, SelectEquipReference);

        if (v.isNpc)
        {
            visitButton.transform.localScale = Vector3.one;
            State.transform.localScale = Vector3.one;
        }
        else
        {
            visitButton.transform.localScale = Vector3.zero;
            State.transform.localScale = Vector3.zero;
        }
    }
}
