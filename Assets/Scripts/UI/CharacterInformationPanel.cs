using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInformationPanel : GamePanel<CharacterInformationData>
{ 
    [SerializeField]
    private Image characterHead;

    [SerializeField]
    private Image Attribute;

    [SerializeField]
    private TextMeshProUGUI CharacterName;

    [SerializeField]
    private TextMeshProUGUI State;

    [SerializeField]
    private Image HPSliderValue, RPSliderValue, EXPSliderValue;

    [SerializeField]
    private TextMeshProUGUI HPValue, RPValue, EXPValue;

    [SerializeField]
    private TextMeshProUGUI AttackValue, DefenseValue;
    [SerializeField]
    private TextMeshProUGUI SpeedValue, LuckValue;

    [SerializeField]
    private TextMeshProUGUI LevelValue;
    [SerializeField]
    private TextMeshProUGUI FriendshipValue;
    [SerializeField]
    Transform Friendship;
    [SerializeField]
    private TextMeshProUGUI AttackUp, AttackDown, DefenseUp, DefenseDown;

    [SerializeField]
    private TextMeshProUGUI SpeedUp, SpeedDown, LuckUp, LuckDown;

    [SerializeField]
    private EquipBoxReference WeaponBox, ClothesBox,ShoesBox;
    [SerializeField]
    Transform Visit;
    [SerializeField]
    private Button visitButton, closeButton;
    [SerializeField]
    private float infoOffsetY =330f;

    private void SelectEquipReference(Equipment equipment, bool selected = false)
    {
        bool isController = equipment.characterId == CharacterManager.instance.controllerCharacter.instanceId;

        if (equipment.dataId != 0)
        {
            ItemInfo itemInfo = new ItemInfo
            {
                item = new Item
                {
                    itemType = equipment.ItemType,
                    instanceId = equipment.characterId,
                    dataId = equipment.dataId,
                    value = equipment.itemValue,  
                },
                ActionName = isController ? "卸下" : null,
                action = SelectAction,
                OffsetPos=infoOffsetY
            };
            void SelectAction(Item item, bool selected = true)
            {
                Character character = CharacterManager.instance.GetCharacter(equipment.characterId);

                ClearEquip clearEquip = new ClearEquip
                {
                    characterId = equipment.characterId,
                    itemType = equipment.ItemType,
                    outPackageId = character.characterPackage
                };
                GameActionManager.instance.QueueAction(clearEquip,true);
            }
            UIManager.instance.ShowGamePanel<ItemInfoPanel, ItemInfo>(itemInfo);
        }
        else if (isController)
        {
            OpenPackage openPackage = new OpenPackage
            {
                packageId = -1,
                selectActionName = "装备",
                targetObj = equipment.characterId,
                itemMatchData=new ItemMatchData(),
                selectAction = ChangeEquip,
                isMiniShow=true
                //selectActionId = GameCommon.selectEquipBoxAction
            };
            
            openPackage.itemMatchData.itemMatchType = ItemMatchType.ItemType;
            openPackage.itemMatchData.matchValues = new HashSet<int>
            {
                (int)equipment.ItemType
            };

            GameActionManager.instance.QueueAction(openPackage,true);

            async void ChangeEquip(Item item, bool select)
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
                    outPackageId = item.packageId
                };
                GameActionManager.instance.QueueAction(changeEquip,true);
            }

            GameActionManager.instance.QueueAction(openPackage,true);
        }
    }

    private void RefreshCharacterProperty(CharacterPropertyTrigger refreshCharacterProperty)
    {
        if (characterId == refreshCharacterProperty.characterId)
        {
            DisplayProperty(refreshCharacterProperty.characterProperty);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshEquip>(RefreshEquip);
        GameActionManager.instance.AddListener<CharacterPropertyTrigger>(RefreshCharacterProperty);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshEquip>(RefreshEquip);
        GameActionManager.instance.RemoveListener<CharacterPropertyTrigger>(RefreshCharacterProperty);
    }

    private void RefreshEquip(RefreshEquip refreshEquip)
    {
        if (refreshEquip.characterId == characterId)
        {
            Character character = CharacterManager.instance.GetCharacter(characterId);
            InitReferenceData(character.GetInformation());
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
            Close();
        });
    }

    public override void Close()
    {
        base.Close();
        UIManager.instance.CloseGamePanel<TeamPanel>();
        UIManager.instance.CloseGamePanel<MiniPackagePanel>();
        UIManager.instance.CloseGamePanel<ItemInfoPanel>();
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
        EXPSliderValue = FindChildGameObject<Image>("EXPSliderValue");
        HPValue = FindChildGameObject<TextMeshProUGUI>("HPValue");
        RPValue = FindChildGameObject<TextMeshProUGUI>("RPValue");
        EXPValue = FindChildGameObject<TextMeshProUGUI>("EXPValue");
        AttackValue = FindChildGameObject<TextMeshProUGUI>("AttackValue");
        DefenseValue = FindChildGameObject<TextMeshProUGUI>("DefenseValue");
        LevelValue = FindChildGameObject<TextMeshProUGUI>("Level");
        AttackUp = FindChildGameObject<TextMeshProUGUI>("AttackUp");
        AttackDown = FindChildGameObject<TextMeshProUGUI>("AttackDown");
        DefenseUp = FindChildGameObject<TextMeshProUGUI>("DefenseUp");
        DefenseDown = FindChildGameObject<TextMeshProUGUI>("DefenseDown");

        WeaponBox = FindChildGameObject<EquipBoxReference>("Weapon");
        ClothesBox = FindChildGameObject<EquipBoxReference>("Clothes");
        ShoesBox = FindChildGameObject<EquipBoxReference>("Shoes");
        Visit = FindChildGameObject("Visit");
        visitButton = FindChildGameObject<Button>("VisitButton");
        closeButton = FindChildGameObject<Button>("Close");

        Friendship = FindChildGameObject("Friendship");
        FriendshipValue = FindChildGameObject<TextMeshProUGUI>("FriendshipValue");
        SpeedValue = FindChildGameObject<TextMeshProUGUI>("SpeedValue");
        LuckValue = FindChildGameObject<TextMeshProUGUI>("LuckValue");
        SpeedDown = FindChildGameObject<TextMeshProUGUI>("SpeedDown");
        SpeedUp = FindChildGameObject<TextMeshProUGUI>("SpeedUp");
        LuckUp = FindChildGameObject<TextMeshProUGUI>("LuckUp");
        LuckDown = FindChildGameObject<TextMeshProUGUI>("LuckDown");
    }

    private int characterId;

    private void DisplayProperty(CharacterProperty characterProperty)
    {
        int maxHP = characterProperty.MaxHP;
        int maxRP = characterProperty.MaxPower;
        int HP = characterProperty.HP;
        int RP = characterProperty.Power;

        HPSliderValue.fillAmount = HP / (float)maxHP;
        RPSliderValue.fillAmount = RP / (float)maxRP;
        HPValue.text = $"{HP}/{maxHP}";
        RPValue.text = $"{RP}/{maxRP}";

        AttackValue.text = characterProperty.AT.ToString();
        DefenseValue.text = characterProperty.DF.ToString();
        SpeedValue.text = characterProperty.Speed.ToString();
        LuckValue.text = characterProperty.Lucky.ToString();

        AttackUp.enabled = AttackDown.enabled = DefenseDown.enabled = DefenseUp.enabled 
            =SpeedDown.enabled=SpeedUp.enabled=LuckUp.enabled=LuckDown.enabled= false;
        if (data.characterProperty.AT > characterProperty.AT)
            AttackUp.enabled = true;
        if (data.characterProperty.DF > characterProperty.DF)
            DefenseUp.enabled = true;
        if (data.characterProperty.AT < characterProperty.AT)
            AttackDown.enabled = true;
        if (data.characterProperty.DF < characterProperty.DF)
            DefenseDown.enabled = true;
        if (data.characterProperty.Lucky < characterProperty.Lucky)
            LuckDown.enabled = true;
        if (data.characterProperty.Lucky > characterProperty.Lucky)
            LuckUp.enabled = true;
        if (data.characterProperty.Speed > characterProperty.Speed)
            SpeedUp.enabled = true;
        if (data.characterProperty.Speed < characterProperty.Speed)
            SpeedDown.enabled = true;
    }

    private CharacterInformationData data;

    public override async void InitReferenceData(CharacterInformationData v)
    {
        base.InitReferenceData(v);
        data = v;
        v.head.SetImageSprite(characterHead);

        characterId = v.characterId;
        CharacterName.text = v.name;
        if (v.isNpc)
        {
            State.text = v.NPCState.ToString();
            State.color = NPC.GetStateColor(v.NPCState);
        }
        else
        {
            State.text = v.animalState.ToString();
            State.color = Animal.GetStateColor(v.animalState);
        }

        if (characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            Friendship.localScale = Vector3.zero;
        }
        else
        {
            Friendship.localScale = Vector3.one;
            FriendshipValue.text= FriendManager.instance.GetFriendShipLevel(characterId).ToString();
        }

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
        SpeedValue.text = v.characterProperty.Speed.ToString();
        LuckValue.text = v.characterProperty.Lucky.ToString();

        AttackUp.enabled = AttackDown.enabled = DefenseDown.enabled = DefenseUp.enabled
            = SpeedDown.enabled = SpeedUp.enabled = LuckUp.enabled = LuckDown.enabled = false;

        LevelValue.text = v.level.ToString();
        var spriteRenference  = await GameSourceManager.instance.GetScriptableObject<SpriteResourceRenference>($"Reference/AttributeType{(int)v.attributeType}");
        if (spriteRenference == null)
        {
            Attribute.sprite = null;
        }
        else
        {
            Attribute.sprite = spriteRenference.sprite;
        }
       
        WeaponBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.weapon.x,
            itemValue = v.equip.weapon.y / 100.0f,
            ItemType = ItemType.武器
        }, SelectEquipReference); ;
        ClothesBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.clothes.x,
            itemValue = v.equip.clothes.y / 100.0f,
            ItemType = ItemType.防具
        }, SelectEquipReference);

        ShoesBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.shoes.x,
            itemValue = v.equip.shoes.y / 100.0f,
            ItemType = ItemType.鞋子
        }, SelectEquipReference); ;

        if (v.isNpc)
        {
           Visit.localScale = Vector3.one;
            State.transform.localScale = Vector3.one;
        }else if (v.isAnimal)
        {
            State.transform.localScale = Vector3.one;
        }
        else
        {
            Visit.localScale = Vector3.zero;
            State.transform.localScale = Vector3.zero;
        }
    }
}