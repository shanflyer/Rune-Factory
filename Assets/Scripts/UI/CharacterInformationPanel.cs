using System.Collections.Generic;
using System.Threading.Tasks;
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
    private TextMeshProUGUI FriendshipValue,FriendshipLevel;
    [SerializeField]
    private Button FriendshipInfo;
    [SerializeField]
    private Image FriendshipSlider;
    [SerializeField]
    Transform Friendship;
    [SerializeField]
    private TextMeshProUGUI AttackUp, AttackDown, DefenseUp, DefenseDown;

    [SerializeField]
    private TextMeshProUGUI SpeedUp, SpeedDown, LuckUp, LuckDown;

    [SerializeField]
    private EquipBoxReference WeaponBox, ClothesBox,ShoesBox, HeadgearBox;
    [SerializeField]
    Transform Visit;
    [SerializeField]
    private Button visitButton, closeButton;
    [SerializeField]
    Image BackGround;

    [SerializeField]
    Vector2 headSize = new Vector2(150, 150);
    [SerializeField]
    private float infoOffsetY =330f;

    private async void SelectEquipReference(Equipment equipment, int index, bool selected = false)
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
                },
                
                ActionName = isController ? "卸下" : null,
                action = SelectAction,
                OffsetPos=infoOffsetY
            };
            itemInfo.item=await Item.SetValue(itemInfo.item,(int)equipment.itemValue * 100);
            void SelectAction(Item item, int index, bool selected = true)
            {
                Character character = CharacterManager.instance.GetCharacter(equipment.characterId); 

               

                ClearEquip clearEquip = new ClearEquip
                {
                    characterId = equipment.characterId,
                    itemType = equipment.ItemType,
                    outPackageId = character.characterPackage
                };
                GameActionManager.instance.QueueAction(clearEquip, true);
            }
           await UIManager.instance.ShowGamePanel<ItemInfoPanel, ItemInfo>(itemInfo);
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

            async void ChangeEquip(Item item, int index, bool select)
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
        if (!SingletonType.Cleared)
        {
            GameActionManager.instance.RemoveListener<RefreshEquip>(RefreshEquip);
            GameActionManager.instance.RemoveListener<CharacterPropertyTrigger>(RefreshCharacterProperty);
        }
           
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

        FriendshipInfo.onClick.AddListener(async () =>
        {
            FunctionInfoData functionInfoData=await GameDataManager.instance.GetAsyncData<FunctionInfoData>(1);
            UIManager.instance.ShowGamePanel<FunctionInfoPanel,FunctionInfoData>(functionInfoData);
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
        BackGround = GetComponent<Image>();
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
        HeadgearBox = FindChildGameObject<EquipBoxReference>("Headgear");
        Visit = FindChildGameObject("Visit");
        visitButton = FindChildGameObject<Button>("VisitButton");
        closeButton = FindChildGameObject<Button>("Close");

        FriendshipInfo= FindChildGameObject<Button>("FriendshipInfo");
        Friendship = FindChildGameObject("Friendship");
        FriendshipValue = FindChildGameObject<TextMeshProUGUI>("FriendshipValue");
        FriendshipLevel = FindChildGameObject<TextMeshProUGUI>("FriendshipLevel");
        FriendshipSlider = FindChildGameObject<Image>("FriendshipSlider") ;
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
     
    public void HideBackGround(bool hide)
    {
        BackGround.enabled = !hide;
    }
    
    public override async void InitReferenceData(CharacterInformationData v)
    {
        base.InitReferenceData(v);
        data = v;
        v.head.SetImageSprite(characterHead, headSize,Vector2.zero);
        HideBackGround(UIManager.instance.GamePanelIsShow<TeamPanel>());
        characterId = v.characterId;
        
        
        if (v.isNpc)
        {
            State.SetSWText(v.NPCState);
            State.color = NPC.GetStateColor(v.NPCState);
        }
        if (v.isAnimal)
        {
            State.SetSWText(v.animalState);
            State.color = Animal.GetStateColor(v.animalState);
        }

        if (characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            CharacterName.text=GameDataSaveManager.instance.UserGameSaveData.playerData.name;
            Friendship.localScale = Vector3.zero;
        }
        else
        {
            CharacterName.SetSWText(v.name);
            Friendship.localScale = Vector3.one;
            if(FriendManager.instance.GetFriendShip(characterId,out var friendShip))
            {
                FriendshipLevel.text =$"Lv.{friendShip.friendLevel}";
                FriendshipValue.text = $"{friendShip.nowValue}/{friendShip.needValue}";
                FriendshipSlider.fillAmount = friendShip.nowValue / friendShip.needValue;
            }
            else
            {
                var FriendShipData = await GameDataManager.instance.GetAsyncData<FriendShipData>(2);
                FriendshipLevel.text = $"Lv.1";
                FriendshipValue.text = $"{0}/{FriendShipData.needValue}";
                FriendshipSlider.fillAmount = 0;
            }
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
        var spriteRenference = await GameSourceManager.instance.GetScriptableObject<SpriteResourceRenference>(
            $"Reference/AttributeType{(int)v.attributeType}",
            true);
        if (spriteRenference == null)
        {
            Attribute.sprite = null;
        }
        else
        {
            Attribute.sprite = spriteRenference.sprite;
        }
       
      await  WeaponBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.weapon.x,
            itemValue = v.equip.weapon.y / 100.0f,
            ItemType = ItemType.武器,
            hide= v.isAnimal
      }, SelectEquipReference); ;
      await  ClothesBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.clothes.x,
            itemValue = v.equip.clothes.y / 100.0f,
            ItemType = ItemType.防具,
             hide = v.isAnimal
      }, SelectEquipReference);

      await  ShoesBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.headgear.x,
            itemValue = v.equip.headgear.y / 100.0f,
            ItemType = ItemType.帽子,
          hide = v.isAnimal
      }, SelectEquipReference); ;

      await  HeadgearBox.InitData(new Equipment
        {
            characterId = characterId,
            dataId = v.equip.shoes.x,
            itemValue = v.equip.shoes.y / 100.0f,
            ItemType = ItemType.鞋子,
          hide = v.isAnimal
      }, SelectEquipReference);

       
        if (v.isAnimal)
        {
            Visit.localScale = Vector3.zero;
            State.transform.localScale = Vector3.one;
            Friendship.localScale = Vector3.zero;
        }else
        if (v.isNpc)
        {
            Visit.localScale = Vector3.one;
            State.transform.localScale = Vector3.one; 
            Friendship.localScale = Vector3.one;
        }
        else
        {
            Visit.localScale = Vector3.zero;
            State.transform.localScale = Vector3.zero;
            Friendship.localScale = Vector3.zero;
        }
    }
    public override Task InitData(string dataKey)
    {
        int characerId = int.Parse(dataKey);
        Character character = CharacterManager.instance.GetCharacter(characerId);
        if (character != null)
        {
            CharacterInformationData characterInformationData = character.GetInformation();
            InitReferenceData(characterInformationData);
        }
        else
        {
            Close();
        }
        
        return base.InitData(dataKey);
    }
}