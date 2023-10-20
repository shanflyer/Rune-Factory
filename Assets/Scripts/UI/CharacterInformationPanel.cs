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
    TextMeshProUGUI AttackUp, AttackDown, DefenseUp, DefenseDown;
    [SerializeField]
    EquipBoxReference WeaponBox, ClothesBox;
    [SerializeField]
    Button visitButton, closeButton;

    void SelectEquipReference(Equipment equipment,bool selected=false)
    {
        bool isController = equipment.characterId == CharacterManager.instance.controllerCharacter.instanceId;

        if (equipment.dataId != 0)
        { 
            ItemInfo itemInfo = new ItemInfo
            {
                otherValue=equipment.characterId,
                itemId = equipment.dataId,
                dataId=(int)equipment.ItemType,
                ActionName = isController ? "卸下" : null,
                action= SelectAction
            };
            void SelectAction(ItemInfo item, bool selected = true)
            {
                ChangeEquip changeEquip = new ChangeEquip
                {
                    characterId = equipment.characterId,
                    itemId = item.itemId, 
                    outPackageId = 0
                };
                GameActionManager.instance.QueueAction(changeEquip);
            }
            UIManager.instance.ShowGamePanel<ItemInfoPanel, ItemInfo>(itemInfo);
        }
        else if(isController)
        {
            OpenPackage openPackage = new OpenPackage
            {
                packageId = -1,
                selectActionName = "装备",
                targetObj = equipment.characterId,
                selectItemTypes=new List<ItemType>(),
                selectAction= ChangeEquip
                //selectActionId = GameCommon.selectEquipBoxAction
            };
            openPackage.selectItemTypes.Add(equipment.ItemType);
            GameActionManager.instance.QueueAction(openPackage);
            
            async void ChangeEquip(Item item, int packageId)
            {
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                if (itemData.type != equipment.ItemType)
                {
                    return;
                }
                ChangeEquip changeEquip = new ChangeEquip
                {
                    characterId = equipment.characterId,
                    itemId = item.instanceId,
                    outPackageId = packageId
                };
                GameActionManager.instance.QueueAction(changeEquip);
            }
           
            GameActionManager.instance.QueueAction(openPackage);
        }
       
    }
    void RefreshCharacterProperty(RefreshCharacterProperty refreshCharacterProperty)
    {
        if(characterId==refreshCharacterProperty.id)
        {
            DisplayProperty(refreshCharacterProperty.characterProperty);
        }
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshCharacterProperty>(RefreshCharacterProperty);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshCharacterProperty>(RefreshCharacterProperty);
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
        AttackUp = FindChildGameObject<TextMeshProUGUI>("AttackUp");
        AttackDown = FindChildGameObject<TextMeshProUGUI>("AttackDown");
        DefenseUp = FindChildGameObject<TextMeshProUGUI>("DefenseUp");
        DefenseDown = FindChildGameObject<TextMeshProUGUI>("DefenseDown");

        WeaponBox = FindChildGameObject<EquipBoxReference>("Weapon");
        ClothesBox = FindChildGameObject<EquipBoxReference>("Clothes");
        visitButton = FindChildGameObject<Button>("Visit");
        closeButton = FindChildGameObject<Button>("Close");
          
    }
    int characterId;

    void DisplayProperty(CharacterProperty characterProperty)
    {
        int maxHP = characterProperty.MaxHP;
        int maxRP = characterProperty.MaxPower;
        int HP =characterProperty.HP;
        int RP = characterProperty.Power;

        HPSliderValue.fillAmount = HP / (float)maxHP;
        RPSliderValue.fillAmount = RP / (float)maxRP;
        HPValue.text = $"{HP}/{maxHP}";
        RPValue.text = $"{RP}/{maxRP}";

        AttackValue.text = characterProperty.AT.ToString();
        DefenseValue.text = characterProperty.DF.ToString();
         
        AttackUp.enabled = AttackDown.enabled = DefenseDown.enabled = DefenseUp.enabled = false;
        if (data.characterProperty.AT > characterProperty.AT)
            AttackUp.enabled = true;
        if (data.characterProperty.DF > characterProperty.DF)
            DefenseUp.enabled = true;
        if (data.characterProperty.AT < characterProperty.AT)
            AttackDown.enabled = true;
        if (data.characterProperty.DF < characterProperty.DF)
            DefenseDown.enabled = true;
    }

    CharacterInformationData data;
    public override void InitReferenceData(CharacterInformationData v)
    {
        base.InitReferenceData(v);
        data = v;
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
            ItemType=ItemType.武器
        }, SelectEquipReference);
        ClothesBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.clothes,
            ItemType = ItemType.防具
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
