using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public struct CharacterEquipAndPropertyData
{
    public int id;
    public string name;
    public Sprite icon;
    public AttributeType attributeType;
    public Equip equip;
    public GameProperty characterProperty;
}



public struct Exp
{
    public int totalExp;
    public int nowExp;
    public int nowLevelExp;

    public bool AddExp(int exp)
    {
        totalExp += exp;
        nowExp += exp;

        if (nowExp >= nowLevelExp)
        {
            nowExp -= nowLevelExp;
            return true;
        }
        return false;
    }
}

public struct Equip
{
    public int2 weapon;
    public int2 clothes;
    public int2 shoes;
    public int2 headgear;
}

public delegate void SetCoordinate(int3 coordinate);

[Serializable]
public partial class Character
{
    public bool isController { get; private set; }
    public int linkItem;
    private int oldOperateItem = -1;

    public int OperateItem
    {
        get => oldOperateItem;
        set
        {
            if (oldOperateItem != 0) WorldMapObjManager.instance.RecycleMaskObj(oldOperateItem);
            oldOperateItem = value;
            if (oldOperateItem != 0) WorldMapObjManager.instance.TryDisplayMask(oldOperateItem);
        }
    }  

    private int2 OldOperaCoordinate = new int2(int.MinValue);

    public CharacterData characterData;
    public bool canMove = true;
    public Equip Equip => equip;
    private Equip equip;
    public int characterPackage;
    public List<int> skills = new List<int>();

    public void UnLinkItem()
    {
        if (WorldMapManager.instance.GetRuntimeMapItem(linkItem, out var item)) item.linkCharacter = 0;
        linkItem = 0;
    }
    public void WakeUp(int sleepHour)
    {
        UnLinkItem();
        if (isController)
        {
            var playerWakeUp = new PlayerWakeUp
            {
                characterId = instanceId
            };
            GameActionManager.instance.QueueAction(playerWakeUp, true);
            WorldMapObjManager.instance.RefreshMapAudio();
            GameTimerController.instance.DelayAction(1200,
                () =>
                {
                    var setCharacterRandomCoordinate = new SetCharacterRandomCoordinate
                    {
                        characterId = instanceId,
                        Coordinate = coordinate,
                        range = 6
                    };
                    GameActionManager.instance.QueueAction(setCharacterRandomCoordinate);

                    if (isController)
                    {
                        var openOrCloseInputMap = new OpenOrCloseInputMap
                        {
                            open = true
                        };
                        GameActionManager.instance.QueueAction(openOrCloseInputMap);
                    }
                });
        }

        var setCharacterAnimator = new SetCharacterAnimator
        {
            characterId = instanceId,
            parameter = "State",
            parameterType = ParameterType.INT,
            intValue = 0
        };
        GameActionManager.instance.QueueAction(setCharacterAnimator, true);

        var changeCharacterProperty = new ChangeCharacterProperty
        {
            characterId = instanceId,
            propertyType = CharacterPropertyType.体力,
            changeValue = (int)(CharacterProperty.MaxPower * 0.1667f * sleepHour) //六小时睡满体力
        };
        AddProperty(changeCharacterProperty);

        var changeCharacterProperty1 = new ChangeCharacterProperty
        {
            characterId = instanceId,
            propertyType = CharacterPropertyType.生命,
            changeValue = (int)(CharacterProperty.MaxHP * 0.1667f * sleepHour) //六小时睡满体力
        };
        AddProperty(changeCharacterProperty1);
        var changeCharacterProperty2 = new ChangeCharacterProperty
        {
            characterId = instanceId,
            propertyType = CharacterPropertyType.法力,
            changeValue = (int)(CharacterProperty.MaxMP * 0.1667f * sleepHour) //六小时睡满体力
        };
        AddProperty(changeCharacterProperty2);
        GameActionManager.instance.QueueAction(changeCharacterProperty2, true);
    }
    public int2 GetMapStartIndex()
    {
        return MapCellController.instance.GetStartIndex(mapInstance, coordinate);
    }
    public void SetController(bool controller)
    {
        isController = controller;
        OperateItem = -1;
    }
    public Character() { }
    public Character(CharacterData characterData,ProfessionData professionData, int instanceId, 
        bool needCreatPackage,int overridePackage = 0)
    {
        this.characterData = characterData;
        this.instanceId = instanceId;
        dataId = characterData.id;
        this.professionData = professionData; 
        name = characterData.characterName;

        var packageInstancId = 0;
        var saveData = GameDataSaveManager.instance.GetCharacterSaveData(dataId);
        if (saveData != null)
        { 
            name = saveData.name;
            packageInstancId = saveData.packageId;
            SetLevel(saveData.level, true);
            SetNowExp(saveData.exp);
            SetEquip(ItemType.武器, saveData.weapon);
            SetEquip(ItemType.防具, saveData.clothes);
            SetEquip(ItemType.鞋子, saveData.shoe);
            SetEquip(ItemType.帽子, saveData.headgear);
            characterPackage = packageInstancId;
            SetProperty(saveData.hp, saveData.mp, saveData.power);
            RefreshShortcut refreshShortcut = new RefreshShortcut
            {
                packageId = packageInstancId,
            };
            GameActionManager.instance.QueueAction(refreshShortcut);
        }
        else
        {
            SetLevel(1, true); 
            if(needCreatPackage)
            {
                CreatCharacterPackage(overridePackage, packageInstancId);
            }
            
        }

        //behavior = characterData.behavior;
         
    }

    public void SetCellOffset(Vector2 offset)
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var characterRuntimeObj))
        {
            characterRuntimeObj.transform.Translate(offset);
        }
    }

    public Vector2 GetCellOffset()
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var characterRuntimeObj))
        {
            Vector3 pos = GameCommon.GetMapPos(coordinate);
            return characterRuntimeObj.transform.position - pos;
        }
        return Vector2.zero;
    }

    protected virtual async Task CreatCharacterPackage(int overridePackage = 0,int instanceId=0)
    {
        characterPackage = await PackageManager.instance.CreatGamePackage(overridePackage == 0 ?
            characterData.packageId : overridePackage, instanceId);
    }

    public int attackType;
    public async void ClearEquip(ItemType itemType)
    {
        int oldItemId = 0;
        switch (itemType)
        {
            case ItemType.武器:
                oldItemId = equip.weapon.x;
                equip.weapon = 0;
                break;
            case ItemType.防具:
                oldItemId = equip.clothes.x;
                equip.clothes = 0;
                break;
            case ItemType.鞋子:
                oldItemId = equip.shoes.x;
                equip.shoes =0;
                break;
            case ItemType.帽子:
                oldItemId = equip.headgear.x;
                equip.headgear = 0;
                break;
        }
        attackAttributeType = AttributeType.无;
        defenceAttributeType = AttributeType.无;
        ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(oldItemId);
        if (oldItemData != null)
        { 
            EquipmentProperty = EquipmentProperty - oldItemData.Property;
        }
        
        GameActionManager.instance.QueueAction(new RefreshEquip
        {
            characterId = instanceId
        },true);
        attackType = 0;
    }
    async void SetEquip(ItemType itemType,int2 Equip)
    {
        if (Equip.x == 0)
        {
            return;
        }
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(Equip.x);
        switch (itemData.type)
        {
            case ItemType.武器:
                attackAttributeType = itemData.attributeType;
                equip.weapon= Equip;
                attackType = itemData.otherType;
                break;
            case ItemType.防具:
                defenceAttributeType = itemData.attributeType; 
                equip.clothes = Equip;  
                break;
            case ItemType.鞋子: 
                equip.shoes = Equip; 
                break;
            case ItemType.帽子:
                equip.headgear = Equip;
                break;
        }
        EquipmentProperty = EquipmentProperty + itemData.Property;
    }
    public async void ChangeEquip(ItemData itemData, int packageId)
    {
        Item item = default(Item);
        int oldItemId = 0;
        switch (itemData.type)
        {
            case ItemType.武器:
                attackAttributeType = itemData.attributeType;
                oldItemId = equip.weapon.x;
                equip.weapon.x = itemData.id;
                equip.weapon.y = 100;
                attackType = itemData.otherType;
                break;
            case ItemType.防具:
                defenceAttributeType = itemData.attributeType;
                oldItemId = equip.clothes.x;
                equip.clothes.x = itemData.id;
                equip.clothes.y = 100;
                break;
            case ItemType.鞋子:
                oldItemId = equip.shoes.x;
                equip.shoes.x = itemData.id;
                equip.shoes.y = 100;
                break;
            case ItemType.帽子:
                oldItemId = equip.headgear.x;
                equip.headgear.x = itemData.id;
                equip.headgear.y = 100;
                break;
        }
        ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(oldItemId); 
        if (oldItemData != null)
        {
            item.count = 1;
            item.dataId = oldItemData.id;
            EquipmentProperty = EquipmentProperty - oldItemData.Property;
        }
         
        
        if (item.dataId != 0)
        {
            await PackageManager.instance.SetItemInPackage(item, packageId);
        }
        if (itemData != null)
        {
            EquipmentProperty = EquipmentProperty + itemData.Property;
        }
        GameActionManager.instance.QueueAction(new RefreshEquip
        {
            characterId = instanceId
        },true);
    }

    public async void ChangeData(int dataId)
    {
        if (this.dataId != dataId)
        {
            this.dataId = dataId;

            characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(dataId);

            this.professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
            ProfessionProperty = professionData.GetLevelProperty(level);
        }
    }
    public CharacterEquipAndPropertyData CharacterEquipAndPropertyData
    {
        get
        {
            return new CharacterEquipAndPropertyData
            {
                id = instanceId,
                name = name,
                characterProperty = CharacterProperty,
                equip = equip,
                //attributeType = AttributeType
            };
        }
    }

    public GameProperty CharacterProperty
    {
        get => (ProfessionProperty+EquipmentProperty+OtherAddProperty)*OtherMulProperty;
        
    }

    private void CharacterPropertyTrigger()
    {
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = CharacterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger,true);
    }

    private GameProperty ProfessionProperty
    {
        get
        {
            return professionProperty;
        }
        set
        {
            professionProperty = value;
            CharacterPropertyTrigger();
        }
    }
    private GameProperty EquipmentProperty
    {
        get
        {
            return equipmentProperty;
        }
        set
        {
            equipmentProperty = value;
            CharacterPropertyTrigger();
        }
    }
    private GameProperty OtherAddProperty
    {
        get
        {
            return otherAddProperty;
        }
        set
        {
            otherAddProperty = value;
            CharacterPropertyTrigger();
        }
    }
    private GameProperty OtherMulProperty
    {
        get
        {
            return otherMulProperty;
        }
        set
        {
            otherMulProperty = value;
            CharacterPropertyTrigger();
        }
    }

    private GameProperty professionProperty;
    private GameProperty equipmentProperty;
    private GameProperty otherAddProperty;
    private GameProperty otherMulProperty = GameProperty.One;

    public int groupId = -1;
    public ProfessionData professionData;
    public int dataId;

    public int Level
    {
        get => level;
    }

    private int level;

    public Exp exp;

    public string name { get; protected set; }
    private int3 objCoordinate;
    private int2 forwardCoordinate;

    private AttributeType attackAttributeType,defenceAttributeType;
    public AttributeType AttackAttributeType => attackAttributeType;
    public AttributeType DefenceAttributeType => defenceAttributeType;

    public int3 ObjCoordinate => objCoordinate;
    public int2 coordinate => objCoordinate.xy;
    public int mapInstance => objCoordinate.z;

    public Vector2 pos
    {
        get
        {
            if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var obj))
                return obj.transform.position;

            return GameCommon.GetMapPos(coordinate);
        }
    }

    public int instanceId;
    public Direction direction { private set; get; }

     

    private float2 _moveDirection;

    public float2 moveDirection
    {
        get
        {
            return _moveDirection;
        }
        set
        {
            // var bool2 = _moveDirection != value;
            // if (bool2.x || bool2.y)
            {
                _moveDirection = value;
                if (_moveDirection.x == float.NaN)
                {
                    _moveDirection.x = 0;
                }
                if (_moveDirection.y == float.NaN)
                {
                    _moveDirection.y = 0;
                }
                if (_moveDirection.Equals(float2.zero))
                {
                    return;
                }
                direction = GameCommon.GetCharacterDirect(moveDirection, direction);
                // Debug.Log($"direction:{moveDirection}--{direction}");
                if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var runtimeObj))
                { 
                    runtimeObj.SetAnimationDirection(_moveDirection, direction); 
                }
            }
        }
    }
    public void SetDirection(Direction direction)
    {
        this.direction = direction;
        moveDirection=GameCommon.GetDirectValue(direction);
    }
  
    private float _nowSpeed;

    public float nowSpeed
    {
        set
        {
            if (_nowSpeed != value)
            {
                _nowSpeed = value;
                if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var runtimeObj))
                {
                    /*
                    if (_nowSpeed == 0)
                    {
                        TryTeamLeaderStop tryTeamLeaderStop = new TryTeamLeaderStop
                        {
                            characterId = instanceId
                        };
                        GameActionManager.instance.QueueAction(tryTeamLeaderStop);
                    }*/
                    float animationSpeed = 0;
                    if (value != 0)
                    {
                        animationSpeed = propertySpeed;
                    }
                    runtimeObj.SetAnimationSpeed(value, animationSpeed);
                }
            }
        }
        get
        {
            return _nowSpeed;
        }
    }

    public float propertySpeed => team != null ? team.speed : CharacterProperty.Speed*0.01f;
    private Team team;
    public bool isInTeam => team != null;
    public void JoinTeam(Team team)
    {
        this.team = team;
    }
    public void LeaveTeam()
    {
        team = null;
    }
    //public string behavior;

    public int moveEnumeratorId; 

    public bool CanMoveCrossMap = true;

    private SetCoordinate SetCoordianteDele;

    public void RemoveSetCoordinateDele(object obj)
    {
        if (SetCoordianteDele != null)
        {
            var deles = SetCoordianteDele.GetInvocationList();
            for (int i = 0; i < deles.Length; i++)
            {
                if (deles[i].Target.GetHashCode() == obj.GetHashCode())
                {
                    SetCoordianteDele -= (SetCoordinate)deles[i];
                    break;
                }
            }
        }
    }

    public void AddSetCoordinateDele(SetCoordinate setCoordinate)
    {
        if (SetCoordianteDele == null)
        {
            SetCoordianteDele = setCoordinate;
        }
        else
        {
            SetCoordianteDele += setCoordinate;
        }
    }

    public void RemoveSetCoordinateDele(SetCoordinate setCoordinate)
    {
        if (SetCoordianteDele != null)
        {
            SetCoordianteDele -= setCoordinate;
        }
        if (SetCoordianteDele != null && SetCoordianteDele.GetInvocationList().Length == 0)
        {
            var InvocationList = SetCoordianteDele.GetInvocationList();
            if (InvocationList.Length == 0)
            {
                SetCoordianteDele = null;
            }
        }
    }

    private void SetObjCoordinate(int3 coordinate, bool refreshPos = true)
    {
        MapCellController.instance.SetCharacterCoordinate(objCoordinate, coordinate, instanceId,this is TempCharacter);
        var changeMap = mapInstance != coordinate.z;
        if (coordinate.z == 0)
        {
            //  Debug.Log("set coordinate.z == 0");
        }

        if (!changeMap && objCoordinate.x == coordinate.x && objCoordinate.y == coordinate.y)
        {
            return;
        }

        objCoordinate = coordinate;
        if (changeMap)
        {
            if (team != null && team.leader == this) team.ChangeMap(coordinate.xy, coordinate.z);
        }
        else
        {
            if (team != null && team.leader == this)
            {
                var tryTeamLeaderMove = new TryTeamLeaderMove
                {
                    characterId = instanceId,
                    targetCoordinate = coordinate.xy,
                    targetPos = pos
                };
                GameActionManager.instance.QueueAction(tryTeamLeaderMove, true);
            }
        }
        //Debug.Log($"{name}--SetObjCoordinate:{coordinate}");
        if (mapInstance == WorldMapObjManager.instance.displayMap)
        {
            if(CharacterManager.instance.GetRuntimeCharacterObj(instanceId,out var characterRuntimeObj))
            {
                characterRuntimeObj.SetCoordinateAction(this.coordinate, mapInstance, WorldMapObjManager.instance.DisplayMapRoomData.defaultGround);
            }
        }
        if (CharacterManager.instance.controllerCharacter == this)
        {
            RefreshNeighborhood();
        }
        else if (!(this is TempCharacter) && (TeamManager.instance.playerTeam == null ||
                                              !TeamManager.instance.playerTeam.CheckCharacter(instanceId)))
        {
            CharacterManager.instance.controllerCharacter.TryRefreshNeighborhood(this);
        }
        // Debug.Log($"setCoordinate0:{coordinate}");
        if (SetCoordianteDele != null)
        {
            SetCoordianteDele.Invoke(coordinate);
        }
    }

    public void SetObjCoordinate(int mapInstance, int2 coordinate)
    {
        if (mapInstance == 0)
        {
            Debug.Log("set mapInstance == 0");
        }
        int3 newCoordinate = new int3(coordinate, mapInstance);
        MapCellController.instance.SetCharacterCoordinate(objCoordinate, newCoordinate, instanceId, this is TempCharacter);
        objCoordinate = newCoordinate;
        if (mapInstance == WorldMapObjManager.instance.displayMap)
        {
            if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var characterRuntimeObj))
            {
                characterRuntimeObj.SetCoordinateAction(this.coordinate, mapInstance, WorldMapObjManager.instance.DisplayMapRoomData.defaultGround);
            }
        }
        if (CharacterManager.instance.controllerCharacter == this)
        {
            RefreshNeighborhood();
        }
        else if (!(this is TempCharacter)||!TeamManager.instance.playerTeam.CheckCharacter(instanceId))
        {
            CharacterManager.instance.controllerCharacter.TryRefreshNeighborhood(this);
        }

        //Debug.Log($"setCoordinate:{newCoordinate}");
        if (SetCoordianteDele != null)
        {
            SetCoordianteDele.Invoke(objCoordinate);
        }
    }

    private HashSet<int> NeighborhoodCharacters=new HashSet<int>();

    public void TryRefreshNeighborhood(Character character, int range = 8)
    {
        if (mapInstance != character.mapInstance)
        {
            if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc) &&
                npc.startSleepHour >= 0) return;
            if (NeighborhoodCharacters.Contains(character.instanceId))
            {
                RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                {
                    characterId = character.instanceId,
                    join = false
                };
                GameActionManager.instance.QueueAction(refreshOperateCharacter);
            }
        }
        else
        {
            var isSleepNpc = false;
            if (!character.isController && NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc))
                if (npc.startSleepHour >= 0)
                    isSleepNpc = true;

            int absX = math.abs(character.coordinate.x - coordinate.x);
            int absY = math.abs(character.coordinate.y - coordinate.y);

            if (NeighborhoodCharacters.Contains(character.instanceId))
            {
                if (isSleepNpc)
                {
                    var refreshOperateCharacter = new RefreshOperateCharacter
                    {
                        characterId = character.instanceId,
                        join = false
                    };
                    GameActionManager.instance.QueueAction(refreshOperateCharacter);
                    NeighborhoodCharacters.Remove(character.instanceId);
                    return;
                }
                if (absX > range || absY > range)
                {
                    RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                    {
                        characterId = character.instanceId,
                        join = false
                    };
                    GameActionManager.instance.QueueAction(refreshOperateCharacter);
                    NeighborhoodCharacters.Remove(character.instanceId);
                }
            }
            else
            {
                if (absX <= range && absY <= range)
                {
                    if (!character.isController || isSleepNpc)
                    {
                        RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                        {
                            characterId = character.instanceId,
                            join = false
                        };
                        GameActionManager.instance.QueueAction(refreshOperateCharacter);
                        NeighborhoodCharacters.Remove(character.instanceId);
                    }
                    else
                    {
                        RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                        {
                            characterId = character.instanceId,
                            join = true
                        };
                        GameActionManager.instance.QueueAction(refreshOperateCharacter);
                        NeighborhoodCharacters.Add(character.instanceId);
                    }
                  
                }
            }
        }
       
    }

    private void RefreshNeighborhood()
    {
        RefreshOperateCharacters refreshOperateCharacters = new RefreshOperateCharacters();
        var NeighborhoodCharacters1 = MapCellController.instance.GetCharacters(objCoordinate);
        
        NeighborhoodCharacters1.Remove(instanceId);
        if (TeamManager.instance.playerTeam != null)
            NeighborhoodCharacters1.ExceptWith(TeamManager.instance.playerTeam.TeamCharacters);

        HashSet<int> sleepCharacters = new HashSet<int>();
        foreach(var id in NeighborhoodCharacters1)
        {
            if (NPCManager.instance.GetNPCFormInstance(id, out var npc))
            {
                if (npc.startSleepHour >= 0)
                {
                    sleepCharacters.Add(id);
                }
            }
        }
        NeighborhoodCharacters1.ExceptWith(sleepCharacters);
        
        {
            refreshOperateCharacters.leaveCharacters = NeighborhoodCharacters.Except(NeighborhoodCharacters1).ToHashSet<int>();
            refreshOperateCharacters.joinCharacters= NeighborhoodCharacters1.Except(NeighborhoodCharacters).ToHashSet<int>(); 
        }
        NeighborhoodCharacters = NeighborhoodCharacters1;
        GameActionManager.instance.QueueAction(refreshOperateCharacters);

        EnvironmentManger.instance.UpDataAudio2DPolygon();
    }

    public async void SetNeighborhood(int characterId)
    {
        Character character = CharacterManager.instance.GetCharacter(characterId);
        if (character != null)
        {
            EventReferenceData eventReferenceData = new EventReferenceData
            {
                name = "CharacterId",
                value = character.instanceId
            };
            EventReferenceData targetReferenceData = new EventReferenceData
            {
                name = "TargetCharacter",
                value = instanceId
            };

            int nextTalkEventId = 0;
            int eventId = 0;
            if (character is TempCharacter tempCharacter)
            {
                nextTalkEventId = tempCharacter.tempCharacterData.nextTalkEventId;
                eventId = tempCharacter.tempCharacterData.tempTalkEventId;
            }
            else if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var NPC))
            {
                nextTalkEventId = NPC.nextTalkEventId;
                eventId = NPC.playerOperateEventId;
                /*AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                {
                    characterId = character.instanceId,
                    friendAddType = FriendAddType.对话,
                    value = 1
                };
                GameActionManager.instance.QueueAction(addFriendShipValue);*/
            }

            EventReferenceData NextTalkReferenceData = new EventReferenceData
            {
                name = "NextTalkEventId",
                value = nextTalkEventId
            };
            await GameEventManager.instance.AddGameEvent(
                eventId, new List<EventReferenceData>
                {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
                });
             
        }
         
    }
     

    public void StopMove()
    {
        GameObjectCurveController.instance.StopObjectMove(instanceId);
        if (GameObjectCurveController.instance.StopLineMove(moveEnumeratorId))
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(0, this);
        }; 
    }

    public void RemoveMove()
    {
        GameObjectCurveController.instance.RemoveLineMove(moveEnumeratorId);
        CharacterManager.instance.SetCharacterAnimationSpeed(0, this);
        moveEnumeratorId = 0;

        moveTarget = int3.zero;
        this.moveEndAction = null;
        this.changeCoordinateAction = null;
        this.failedMoveAction = null;
    }

    public void StartMove()
    {
        if (GameObjectCurveController.instance.StartLineMove(moveEnumeratorId))
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(1, this);
        }
        // GameController.instance.StopCoroutine(moveEnumerator);
    }

    public  void AddExp(int value)
    {
        bool levelUp = false;
         
        while (exp.AddExp(value))
        {
            exp.nowLevelExp = professionData.GetLevelExp(level) - professionData.GetLevelExp(level - 1);
            value = 0;
            SetLevel(level + 1);
            levelUp = true;
        }
        if (levelUp)
        {
        }
    }

    public void SetNowExp(int nowExp)
    {
        if (level > 0)
        {
            exp.totalExp = professionData.GetLevelExp(level - 1) + nowExp;
            exp.nowExp = nowExp;
        }
        else
        {
            exp.totalExp = nowExp;
            exp.nowExp = nowExp;
        }
        
    }
    public void SetLevel(int level, bool zero = false)
    {
        if (level != this.level)
        { 
            //attributeType = profressionData.attributeType;
            if (professionData!=null)
            {
                if (zero)
                {
                    skills.Clear();
                    for (int i = 1; i <= level; i++)
                    {
                        int skillId = professionData.GetLevelSkill(i);
                        if (skillId >0)
                        {
                            skills.Add(skillId);
                        } 
                    } 
                }
                else
                {
                    int skillId = professionData.GetLevelSkill(level);
                    if (skillId >0)
                    {
                        skills.Add(skillId);
                    } 
                }
                ProfessionProperty = professionData.GetLevelProperty(level);
            }
            exp.nowLevelExp = professionData.GetLevelExp(level) - professionData.GetLevelExp(level - 1);
            this.level = level;
            CharacterLevelUp characterLevelUp = new CharacterLevelUp
            {
                characterId = instanceId,
                level = level
            };
            GameActionManager.instance.QueueAction(characterLevelUp);
        }
    }

    public void SetProperty(int HP = -1, int MP = -1, int Power = -1, int MaxHP = -1, int MaxMP = -1, int MaxPower = -1, int AT = -1, int DF = -1, int Lucky = -1
        ,int Speed=-1, int Other = -1)
    {
        if (HP >= 0)
            professionProperty.HP = HP;
        if (MP >= 0)
            professionProperty.MP = MP;
        if (Power >= 0)
            professionProperty.Power = Power;
        if (MaxHP >= 0)
            professionProperty.MaxHP = MaxHP;
        if (MaxMP >= 0)
            professionProperty.MaxMP = MaxMP;
        if (MaxPower >= 0)
            professionProperty.MaxPower = MaxPower;
        if (AT >= 0)
            professionProperty.AT = AT;
        if (DF >= 0)
            professionProperty.DF = DF;
        if (Other >= 0)
            professionProperty.Other = Other;
        if (Speed >= 0)
            professionProperty.Speed = Other;

        this.ProfessionProperty = professionProperty;
    }

    public void SetProperty(SetCharacterProperty setCharacterProperty)
    {
        professionProperty.SetProperty(setCharacterProperty);
        CharacterPropertyTrigger();
        RefreshCharacter refreshCharacter = new RefreshCharacter
        {
            id = instanceId
        };
        GameActionManager.instance.QueueAction(refreshCharacter, true);
    }

    public void AddProperty(ChangeCharacterProperty changeCharacterProperty)
    {
        professionProperty.ChangeProperty(changeCharacterProperty);
        CharacterPropertyTrigger();
        RefreshCharacter refreshCharacter = new RefreshCharacter
        {
            id = instanceId
        };
        GameActionManager.instance.QueueAction(refreshCharacter, true);
    }


    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="eventid">事件id</param>
    /// <param name="reference">数据id</param>
    /// <param name="enter">是否进入事件</param>
    private async void TriggerEventAction(int eventid, int reference, bool enter, bool controller = false)
    {
        if (team!=null && this != CharacterManager.instance.controllerCharacter)
        {
            return;
        }
        if (eventid == 0 && reference == 0)
        {
            return;
        }

        if (controller)
        {
            if (enter)
            {
                //Debug.Log($"进入触发：{reference}");

                OperateItem = reference;
                ShowMapObjTips showMapObjTips = new ShowMapObjTips
                {
                    id = reference
                };
                GameActionManager.instance.QueueAction(showMapObjTips, true);

                TriggerEnter triggerEnter = new TriggerEnter
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerEnter,true);
            }
            else
            {
                // Debug.Log($"离开触发：{reference}");
                if (OperateItem == reference)
                {
                    OperateItem = -1;
                }
                CloseMapObjTips closeMapObjTips = new CloseMapObjTips
                {
                    id = reference
                };
                GameActionManager.instance.QueueAction(closeMapObjTips, true);
                TriggerExit triggerExit = new TriggerExit
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerExit, true);
            }
        }
        else
        {
            if (enter)
            {
                TriggerEnter triggerEnter = new TriggerEnter
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerEnter, true);
            }
            else
            {
                TriggerExit triggerExit = new TriggerExit
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerExit, true);
            }
        }
        List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>(2);
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.characterTriggerRenferenceName,
            value = instanceId
        });
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.triggerRenferenceName,
            value = reference
        });

        await GameEventManager.instance.AddGameEvent(eventid, eventReferenceDatas);
    }

    public void SetTriggerMapItem(int reference, int eventId)
    {
        OperateItem = reference;
        var showMapObjTips = new ShowMapObjTips
        {
            id = reference
        };
        GameActionManager.instance.QueueAction(showMapObjTips, true);

        var triggerEnter = new TriggerEnter
        {
            eventId = reference
        };
        GameActionManager.instance.QueueAction(triggerEnter, true);
        var eventReferenceDatas = new List<EventReferenceData>(2);
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.characterTriggerRenferenceName,
            value = instanceId
        });
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.triggerRenferenceName,
            value = reference
        });

        GameEventManager.instance.AddGameEvent(eventId, eventReferenceDatas);
    }
    /// <summary>
    /// 设置坐标
    /// </summary>
    /// <param name="coordinate">x.y;z:地图id</param>
    public void SetCoordinate(int3 coordinate, bool refreshObj = true, bool refreshMapTemp = true,
        bool fiexedDisplay = false)
    {
        //int2 forwordCoordinate = objCoordinate.xy + 2 * GameCommon.GetDirectionInt2(direction);
        int2 oldCoordinate = objCoordinate.xy;
        bool mapCheckRefresh = false;
        if (mapInstance != coordinate.z)
        {
            mapCheckRefresh = true;
            MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色,
                objCoordinate.z, oldCoordinate, true, TriggerEventAction);

            if (isController)
            {
                int2 oldOperaCoordinate = objCoordinate.xy;
                if (OldOperaCoordinate.x != int.MinValue)
                {
                    oldOperaCoordinate = OldOperaCoordinate;
                }
                MapCellController.instance.CheckPlayerTriggerEvent(
                    objCoordinate.z, oldCoordinate, true, TriggerEventAction, false, OperateItem);

                /* DisplayMap displayMap = new DisplayMap
                 {
                     displayMap = coordinate.z
                 };
                 GameActionManager.instance.QueueAction(displayMap);*/
            }
            OldOperaCoordinate = new int2(int.MinValue);
            oldCoordinate = new int2(int.MinValue);

        }

        MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色, coordinate.z, oldCoordinate, coordinate.xy,
            TriggerEventAction);
        if (isController)
        {
            int2 oldOperaCoordinate = oldCoordinate.xy;
            if (OldOperaCoordinate.x != int.MinValue)
            {
                oldOperaCoordinate = OldOperaCoordinate;
            }
            int2 offsetCoordinate = int2.zero;
            switch (direction)
            {
                case Direction.UP:
                    offsetCoordinate = new int2(0, 1);
                    break;

                case Direction.RIGHT:
                    offsetCoordinate = new int2(1, 0);
                    break;

                case Direction.LEFT:
                    offsetCoordinate = new int2(-1, 0);
                    break;

                case Direction.DOWN:
                    offsetCoordinate = new int2(0, -1);
                    break;
            }
            int3 checkCoordinate = coordinate;
        

            MapCellController.instance.CheckPlayerTriggerEvent(coordinate.z, oldOperaCoordinate, checkCoordinate.xy,
                TriggerEventAction, false, OperateItem);
            oldCoordinate = OldOperaCoordinate = checkCoordinate.xy;

            checkCoordinate.xy += offsetCoordinate * 2;
            MapCellController.instance.CheckPlayerTriggerEvent(coordinate.z,forwardCoordinate, checkCoordinate.xy,
                TriggerEventAction, true, OperateItem);
            forwardCoordinate = checkCoordinate.xy;
        }
        SetObjCoordinate(coordinate);
        CharacterCoordinateTrigger characterCoordinateTrigger = new CharacterCoordinateTrigger
        {
            characterId = instanceId,
            coordinate = coordinate
        };
        GameActionManager.instance.QueueAction(characterCoordinateTrigger,true);

        if (refreshObj)
        {
            CharacterManager.instance.RefreshNpcRuntimeObj(this, isController, refreshMapTemp, fiexedDisplay);
        }
       
        // ForwardTrigger(coordinate, direction);
    }

    public void SetCoordinate(int2 coordinate, bool refreshPos = true)
    {
        int2 oldCoordinate = objCoordinate.xy;
        int3 checkCoordinate = new int3(coordinate.xy, mapInstance);
        MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色, mapInstance, oldCoordinate, coordinate.xy,
            TriggerEventAction);

        SetObjCoordinate(checkCoordinate,refreshPos);
        CharacterCoordinateTrigger characterCoordinateTrigger = new CharacterCoordinateTrigger
        {
            characterId = instanceId,
            coordinate = checkCoordinate
        };
        GameActionManager.instance.QueueAction(characterCoordinateTrigger);

        // ForwardTrigger(coordinate, direction);
    }

    private void ForwardTrigger(int3 coordinate, Direction direction)
    {
        int2 offsetCoordinate = int2.zero;
        switch (direction)
        {
            case Direction.UP:
                offsetCoordinate = new int2(0, 1);
                break;

            case Direction.RIGHT:
                offsetCoordinate = new int2(1, 0);
                break;

            case Direction.LEFT:
                offsetCoordinate = new int2(-1, 0);
                break;

            case Direction.DOWN:
                offsetCoordinate = new int2(0, -1);
                break;
        }
        coordinate.xy += offsetCoordinate;
    }

    public int3 moveTarget { get; private set; }
    public MoveEndAction moveEndAction { get; private set; }
    public MoveEndAction changeCoordinateAction { get; private set; }
    public Int3Action failedMoveAction { get; private set; }
  
    public bool TryMove(int2 targetCoordinate, MoveEndAction moveEndAction = null, MoveEndAction changeCoordinateAction = null,
        Int3Action failedMoveAction = null)
    {
        return TryMove(mapInstance, targetCoordinate, moveEndAction, changeCoordinateAction, failedMoveAction);
    }
    public bool TryMove(int targetMap, int2 targetCoordinate, MoveEndAction moveEndAction = null, MoveEndAction changeCoordinateAction = null,
        Int3Action failedMoveAction = null)
    {
        if (targetMap == 948 && targetCoordinate.x == 0 && targetCoordinate.y == 0) Debug.Log("Error");
        if (targetMap == mapInstance && targetCoordinate.x == coordinate.x && targetCoordinate.y == coordinate.y)
        {
            if (moveEndAction != null)
            {
                moveEndAction.Invoke();
            }
            return true;
        }
        this.moveEndAction = moveEndAction;
        this.changeCoordinateAction = changeCoordinateAction;
        this.failedMoveAction = failedMoveAction;
        if (!CanMoveCrossMap)
        {
            Debug.Log($"NoCanMoveCrossMap");
            return false;
        }
        void FailedMoveAction()
        {
            moveTarget = new int3(-1, -1, -1);
            if (failedMoveAction != null)
            {
                failedMoveAction(new int3(targetCoordinate.xy, targetMap));
            }
            moveEndAction = null;
            changeCoordinateAction = null;
            failedMoveAction = null;
        }
        if (targetMap== objCoordinate.z)
        {
            MapCellJobController.instance.AddPathRequest(objCoordinate.xy, targetCoordinate, targetMap,
                (Stack<int2> path, int map, int2 start, int2 end) =>
            {
                PlayerMove(path, () =>
                {
                    // Debug.Log($"character:{name}--MovePathEnd");
                    if (this == CharacterManager.instance.controllerCharacter)
                    {
                        canMove = false;
                        GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 1000), () =>
                        {
                            if (this.moveEndAction != null)
                            {
                                this.moveEndAction.Invoke();
                                moveEndAction = null;
                                changeCoordinateAction = null;
                                failedMoveAction = null;
                            }
                        });
                    }
                    else
                    {
                        if (this.moveEndAction != null)
                        {
                            this.moveEndAction.Invoke();
                            moveEndAction = null;
                            changeCoordinateAction = null;
                            failedMoveAction = null;
                        }
                    }
                }, changeCoordinateAction, FailedMoveAction);
            });

            return true;
        }
        else
        {
            Dictionary<int,Stack<int2>> roadCells = new Dictionary<int,Stack<int2>>();
            Queue<int> roomQueue = new Queue<int>();
            if(MapCellController.instance.FindRoomList(objCoordinate.z, targetMap, out var roomList))
            {
                int nowMap = objCoordinate.z;
                int2 startCoordinate = objCoordinate.xy;
                int roomCount = roomList.Count+1;
                int nextMap= nowMap;
                int2 targetMapCell = int2.zero;
                for (int i = 0; i <= roomList.Count; i++)
                {
                    roomQueue.Enqueue(nowMap);
                    if (i < roomList.Count)
                    { 
                        nextMap = roomList[i];
                        var nowCoordinate = startCoordinate;
                        var endCoordinate = targetCoordinate;
                        if (i != 0) nowCoordinate = new int2(int.MinValue, int.MinValue);
                        if (i != roomList.Count - 1) endCoordinate = new int2(int.MinValue, int.MinValue);

                        if (MapCellController.instance.GetLinkMapInCoordinate(mapInstance, targetMap,
                                nowMap, nextMap, nowCoordinate, endCoordinate,
                                out var changeCoordinate))
                        {
                            targetMapCell = changeCoordinate.zw;
#if UNITY_EDITOR 
                            var mapRange = MapCellController.instance.GetRoomRange(nowMap);
                            if (startCoordinate.x < mapRange.x || startCoordinate.y < mapRange.y ||
                                startCoordinate.x > mapRange.z || startCoordinate.y > mapRange.w)
                                Debug.Log("错误：起始超出地图范围！");

                            if (changeCoordinate.x < mapRange.x || changeCoordinate.y < mapRange.y ||
                                changeCoordinate.x > mapRange.z || changeCoordinate.y > mapRange.w)
                                Debug.Log("错误：目标超出地图范围！");

                            var nextMapRange = MapCellController.instance.GetRoomRange(nextMap);
                            if (targetMapCell.x < nextMapRange.x || targetMapCell.y < nextMapRange.y ||
                                targetMapCell.x > nextMapRange.z || targetMapCell.y > nextMapRange.w)
                                Debug.Log("错误：起始超出地图范围！");
 
 
#endif

                           
                            MapCellJobController.instance.AddPathRequest(startCoordinate, changeCoordinate.xy, nowMap,
                                MoveWithPath);

                            void MoveWithPath(Stack<int2> path, int map, int2 start, int2 end)
                            {
                                if (path.Count == 0)
                                    Debug.Log($"PlayerMove：startCoordinate{start}targetCoordinate{end}-nowMap{map}");
                                roadCells.Add(map, path);
                                roomCount--;
                                if (roomCount == 0) Move(true);
                            }
                        }
                    }
                    else
                    {
#if UNITY_EDITOR
                        var mapRange = MapCellController.instance.GetRoomRange(nowMap);
                        if (startCoordinate.x < mapRange.x || startCoordinate.y < mapRange.y ||
                            startCoordinate.x > mapRange.z || startCoordinate.y > mapRange.w)
                            Debug.Log("错误：起始超出地图范围！");

                        if (targetCoordinate.x < mapRange.x || targetCoordinate.y < mapRange.y ||
                            targetCoordinate.x > mapRange.z || targetCoordinate.y > mapRange.w)
                            Debug.Log("错误：目标超出地图范围！");
#endif
                        MapCellJobController.instance.AddPathRequest(startCoordinate, targetCoordinate, nowMap,
                            (Stack<int2> path, int map, int2 start, int2 end) =>
                        {
                            if (path.Count == 0)
                                Debug.Log(
                                    $"PlayerMove：startCoordinate{start}targetCoordinate{end}-nowMap{map}");

                            roadCells.Add(map, path);
                            roomCount--;
                            if (roomCount == 0)
                            {
                                Move(true);
                            }
                        });
                    } 
                      
                    nowMap = nextMap;
                    startCoordinate = targetMapCell;
                }

                return true;

            }
            else
            {
                return false;
            }
            
            void Move(bool zero)
            {
                if (roomQueue.Count > 0)
                {
                    int map = roomQueue.Dequeue();
                    if(roadCells.TryGetValue(map,out var path))
                    {
                        if (path.Count == 0)
                        {
                            Debug.Log($"{characterData.characterName}map{map}寻路失败:path.Count == 0");
                            FailedMoveAction();
                            return;
                        }
                        if (!zero)
                        {
                            var coordinate = path.Pop(); 
                            SetCoordinate(new int3(coordinate.xy, map));
                        }
                        PlayerMove(path, () =>
                        {
                            //Debug.Log($"character:{name}--PlayerMovePathEnd");
                            if (this == CharacterManager.instance.controllerCharacter)
                            {
                                canMove = false;
                                GameTimerController.instance.DelayAction((int)(GameCommon.mapChangeLerpTime * 1000), () =>
                                {
                                    if (this.moveEndAction != null)
                                    {
                                        this.moveEndAction.Invoke();
                                        moveEndAction = null;
                                        changeCoordinateAction = null;
                                        failedMoveAction = null;
                                    }
                                });
                            }
                            else
                            {
                                Move(false);
                            }
                        }, changeCoordinateAction, FailedMoveAction);
                    }

                }
                else
                {
                    if (this.moveEndAction != null)
                    {
                        this.moveEndAction.Invoke();
                        moveEndAction = null;
                        changeCoordinateAction = null;
                        failedMoveAction = null;
                    }
                }
            } 
        }
         
    }
     

    public void PlayerMove(Stack<int2> pathNodes, MoveEndAction endAction = null, MoveEndAction changeCoordinateAction = null,
        MoveEndAction failedMoveAction = null)
    {
        canMove = true;
        if (pathNodes.Count > 0)
        {
            CharacterManager.instance.CharacterMoveTarget(this, pathNodes, endAction, changeCoordinateAction, failedMoveAction);
        }
        else
        {
          
            moveTarget = new int3(0, 0, 0);
            if (endAction != null)
            {
                endAction.Invoke();
            }
            this.moveEndAction = null;
            this.changeCoordinateAction = null;
            this.failedMoveAction = null;
        }
    }
}