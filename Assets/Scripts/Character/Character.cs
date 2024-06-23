using System.Collections.Generic;
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
    public CharacterProperty characterProperty;
}

[System.Serializable]
public struct CharacterProperty
{
    public int HP, MP, Power, MaxHP, MaxMP, MaxPower, AT, DF, Lucky,Speed;
    public int Other;

    public override string ToString()
    {
        string result = "";
        if (MaxHP != 0)
        {
            string operatorStr = MaxHP > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.最大生命}{operatorStr}{MaxHP}  ";
        }
        if (HP != 0)
        {
            string operatorStr = HP > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.生命}{operatorStr}{HP} ";
        }
        if (MaxMP != 0)
        {
            string operatorStr = MaxMP > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.最大法力}{operatorStr}{MaxMP} ";
        }
        if (MP != 0)
        {
            string operatorStr = MP > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.法力}{operatorStr}{MP}  ";
        }
        if (MaxPower != 0)
        {
            string operatorStr = MaxPower > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.最大体力}{operatorStr}{MaxPower}";
        }
        if (Power != 0)
        {
            string operatorStr = Power > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.体力}{operatorStr}{Power}  ";
        }
        if (AT != 0)
        {
            string operatorStr = AT > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.攻击}{operatorStr}{AT}  ";
        }
        if (DF != 0)
        {
            string operatorStr = DF > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.防御}{operatorStr}{DF}  ";
        }
        if (Lucky != 0)
        {
            string operatorStr = Lucky > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.幸运}{operatorStr}{Lucky}  ";
        }
        if (Speed != 0)
        {
            string operatorStr = Speed > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.敏捷}{operatorStr}{Speed}  ";
        }
        return result;
    }

    public static CharacterProperty One
    {
        get
        {
            CharacterProperty characterProperty = new CharacterProperty
            {
                HP = 1,
                MP = 1,
                Power = 1,
                MaxHP = 1,
                MaxMP = 1,
                MaxPower = 1,
                AT = 1,
                DF = 1,
                Lucky = 1,
                Other = 1,
                Speed=1
            };
            return characterProperty;
        }
    }
    public static CharacterProperty FullPercent
    {
        get
        {
            CharacterProperty characterProperty = new CharacterProperty
            {
                HP = 100,
                MP = 100,
                Power = 100,
                MaxHP = 100,
                MaxMP = 100,
                MaxPower = 100,
                AT = 100,
                DF = 100,
                Lucky = 100,
                Other = 100,
                Speed = 100
            };
            return characterProperty;
        }
    }
    public static CharacterProperty operator -(CharacterProperty property0, CharacterProperty property1)
    {
        CharacterProperty CharacterProperty = new CharacterProperty
        {
            HP = property0.HP - property1.HP,
            MP = property0.MP - property1.MP,
            AT = property0.AT - property1.AT,
            DF = property0.DF - property1.DF,
            Power = property0.Power - property1.Power,
            MaxHP = property0.MaxHP - property1.MaxHP,
            MaxMP = property0.MaxMP - property1.MaxMP,
            MaxPower = property0.MaxPower - property1.MaxPower,
            Lucky = property0.Lucky - property1.Lucky,
            Other = property0.Other - property1.Other,
            Speed=property0.Speed-property1.Speed
        };
        return CharacterProperty;
    }

    public static CharacterProperty operator +(CharacterProperty property0, CharacterProperty property1)
    {
        CharacterProperty CharacterProperty = new CharacterProperty
        {
            HP = property0.HP + property1.HP,
            MP = property0.MP + property1.MP,
            AT = property0.AT + property1.AT,
            DF = property0.DF + property1.DF,
            Power = property0.Power + property1.Power,
            MaxHP = property0.MaxHP + property1.MaxHP,
            MaxMP = property0.MaxMP + property1.MaxMP,
            MaxPower = property0.MaxPower + property1.MaxPower,
            Lucky = property0.Lucky + property1.Lucky,
            Other = property0.Other + property1.Other,
            Speed=property0.Speed+property1.Speed
        };
        return CharacterProperty;
    }
    public static CharacterProperty operator *(CharacterProperty property0, CharacterProperty property1)
    {
        CharacterProperty CharacterProperty = new CharacterProperty
        {
            HP = property0.HP * property1.HP,
            MP = property0.MP * property1.MP,
            AT = property0.AT * property1.AT,
            DF = property0.DF * property1.DF,
            Power = property0.Power * property1.Power,
            MaxHP = property0.MaxHP * property1.MaxHP,
            MaxMP = property0.MaxMP * property1.MaxMP,
            MaxPower = property0.MaxPower * property1.MaxPower,
            Lucky = property0.Lucky * property1.Lucky,
            Other = property0.Other * property1.Other,
            Speed=property0.Speed*property1.Speed
        };
        return CharacterProperty;
    }
    public static CharacterProperty operator *(CharacterProperty property0, float value)
    {
        CharacterProperty CharacterProperty = new CharacterProperty
        {
            HP = (int)(property0.HP * value),
            MP = (int)(property0.MP * value),
            AT = (int)(property0.AT * value),
            DF = (int)(property0.DF * value),
            Power = (int)(property0.Power * value),
            MaxHP = (int)(property0.MaxHP * value),
            MaxMP = (int)(property0.MaxMP * value),
            MaxPower = (int)(property0.MaxPower * value),
            Lucky = (int)(property0.Lucky * value),
            Other = (int)(property0.Other * value),
            Speed= (int)(property0.Speed * value),
        };
        return CharacterProperty;
    }

    public int GetValue(CharacterPropertyType CharacterPropertyType)
    {
        switch (CharacterPropertyType)
        {
            case CharacterPropertyType.体力:
                return Power;

            case CharacterPropertyType.生命:
                return HP;

            case CharacterPropertyType.法力:
                return MP;

            case CharacterPropertyType.攻击:
                return AT;

            case CharacterPropertyType.防御:
                return DF;

            case CharacterPropertyType.幸运:
                return Lucky;

            case CharacterPropertyType.最大体力:
                return MaxPower;

            case CharacterPropertyType.最大生命:
                return MaxHP;

            case CharacterPropertyType.最大法力:
                return MaxMP;
            case CharacterPropertyType.敏捷:
                return Speed;
            default:
                return Other;
        }
    }

    public static CharacterProperty Lerp(CharacterProperty start, CharacterProperty end, float LerpValue)
    {
        return start + (end - start) * LerpValue;
    }
    public void AddProperty(CharacterPropertyType propertyType,int value)
    {
        switch (propertyType)
        {
            case CharacterPropertyType.体力:
                Power += value;
                break;

            case CharacterPropertyType.生命:
                HP += value;
                break;

            case CharacterPropertyType.法力:
                MP += value;
                break;

            case CharacterPropertyType.最大体力:
                MaxPower += value;
                break;

            case CharacterPropertyType.最大法力:
                MaxMP += value;
                break;

            case CharacterPropertyType.最大生命:
                MaxHP += value;
                break;

            case CharacterPropertyType.攻击:
                AT += value;
                break;

            case CharacterPropertyType.防御:
                DF += value;
                break;

            case CharacterPropertyType.幸运:
                Lucky += value;
                break;
            case CharacterPropertyType.敏捷:
                Speed += value;
                break;
            case CharacterPropertyType.自定义值:
                Other += value;
                break;
        }
    }
    public void SetProperty(SetCharacterProperty setCharacterProperty)
    {
        switch (setCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                Power = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.生命:
                HP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.法力:
                MP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.最大体力:
                MaxPower = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.最大法力:
                MaxMP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.最大生命:
                MaxHP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.攻击:
                AT = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.防御:
                DF = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.幸运:
                Lucky = setCharacterProperty.Value;
                break;
            case CharacterPropertyType.敏捷:
                Speed = setCharacterProperty.Value;
                break;
            case CharacterPropertyType.自定义值:
                Other = setCharacterProperty.Value;
                break;
        }
    }

    public void ChangeProperty(ChangeCharacterProperty changeCharacterProperty)
    {
        switch (changeCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                Power += changeCharacterProperty.changeValue;
                Power = math.clamp(Power, 0, MaxPower);
                break;

            case CharacterPropertyType.生命:
                HP += changeCharacterProperty.changeValue;
                HP = math.clamp(HP, 0, MaxHP);
                break;

            case CharacterPropertyType.法力:
                MP += changeCharacterProperty.changeValue;
                MP = math.clamp(MP, 0, MaxMP);
                break;

            case CharacterPropertyType.最大体力:
                MaxPower += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.最大法力:
                MaxMP += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.最大生命:
                MaxHP += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.攻击:
                AT += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.防御:
                DF += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.幸运:
                Lucky += changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.敏捷:
                Speed += changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.自定义值:
                Other += changeCharacterProperty.changeValue;
                break;
        }
    }
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
}

public delegate void SetCoordinate(int3 coordinate);

public partial class Character
{
    private bool isController = false;
    private int oldOperateItem = -1;
    public int OperateItem => oldOperateItem;

    private int2 OldOperaCoordinate = new int2(int.MinValue);

    public CharacterData characterData;
    public bool canMove = true;
    public Equip Equip => equip;
    private Equip equip;
    public int characterPackage;
    public List<int> skills = new List<int>();

    public void SetController(bool controller)
    {
        isController = controller;
        oldOperateItem = -1;
    }

    public Character()
    {
    }

    public Character(CharacterData characterData, int instanceId, int overridePackage = 0)
    {
        this.characterData = characterData;
        this.instanceId = instanceId;
        dataId = characterData.id;
        professionId = characterData.profession;
        name = characterData.characterName;
        //behavior = characterData.behavior;
        SetLevel(1, true);
        CreatCharacterPackage(overridePackage);
    }

    public void SetCellOffset(Vector2 offset)
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var characterRuntimeObj))
        {
            characterRuntimeObj.animator.transform.Translate(offset);
        }
    }

    public Vector2 GetCellOffset()
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var characterRuntimeObj))
        {
            Vector3 pos = GameCommon.GetMapPos(coordinate);
            return characterRuntimeObj.animator.transform.position - pos;
        }
        return Vector2.zero;
    }

    protected virtual async Task CreatCharacterPackage(int overridePackage = 0)
    {
        characterPackage = await PackageManager.instance.CreatGamePackage(overridePackage == 0 ?
            characterData.packageId : overridePackage, 0);
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
        }
        attackAttributeType = AttributeType.无;
        defenceAttributeType = AttributeType.无;
        ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(oldItemId);
        if (oldItemData != null)
        { 
            EquipmentProperty = EquipmentProperty - oldItemData.property;
        }
        
        GameActionManager.instance.QueueAction(new RefreshEquip
        {
            characterId = instanceId
        },true);
        attackType = 0;
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
        }
        ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(oldItemId); 
        if (oldItemData != null)
        {
            item.count = 1;
            item.dataId = oldItemData.id;
            EquipmentProperty = EquipmentProperty - oldItemData.property;
        }
         
        
        if (item.dataId != 0)
        {
            PackageManager.instance.SetItemInPackage(item, packageId);
        }
        if (itemData != null)
        {
            EquipmentProperty = EquipmentProperty + itemData.property;
        }
        GameActionManager.instance.QueueAction(new RefreshEquip
        {
            characterId = instanceId
        },true);
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

    public CharacterProperty CharacterProperty
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

    private CharacterProperty ProfessionProperty
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
    private CharacterProperty EquipmentProperty
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
    private CharacterProperty OtherAddProperty
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
    private CharacterProperty OtherMulProperty
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

    private CharacterProperty professionProperty;
    private CharacterProperty equipmentProperty;
    private CharacterProperty otherAddProperty;
    private CharacterProperty otherMulProperty = CharacterProperty.One;

    public int groupId = -1;
    public int professionId;
    public int dataId;

    public int Level
    {
        get => level;
    }

    private int level;

    public Exp exp;

    public string name;
    private int3 objCoordinate;

    private AttributeType attackAttributeType,defenceAttributeType;
    public AttributeType AttackAttributeType => attackAttributeType;
    public AttributeType DefenceAttributeType => defenceAttributeType;

    public int3 ObjCoordinate => objCoordinate;
    public int2 coordinate => objCoordinate.xy;
    public int mapInstance => objCoordinate.z;

    public int instanceId;
    public Direction direction { private set; get; }

    public void SetDirection(Direction direction)
    {
        this.direction = direction;

        switch (direction)
        {
            case Direction.UP:
                _moveDirection = new int2(0, 1);
                break;

            case Direction.LEFT:
                _moveDirection = new int2(-1, 0);
                break;

            case Direction.DOWN:
                _moveDirection = new int2(0, -1);
                break;

            case Direction.RIGHT:
                _moveDirection = new int2(1, 0);
                break;
        }
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var runtimeObj))
        {
            runtimeObj.SetAnimationDirection(_moveDirection);
        }
    }

    private float2 _moveDirection;

    public float2 moveDirection
    {
        get
        {
            return _moveDirection;
        }
        set
        {
            var bool2 = _moveDirection != value;
            if (bool2.x || bool2.y)
            {
                _moveDirection = value;
                if (_moveDirection.Equals(float2.zero))
                {
                    return;
                }
                direction = GameCommon.GetCharacterDirect(moveDirection, direction);
                //Debug.Log($"direction:{moveDirection}--{direction}");
                if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var runtimeObj))
                {
                    runtimeObj.SetAnimationDirection(GameCommon.GetDirectValue(direction));
                }
            }
        }
    }

    public void SetDataDirection(float2 value)
    {
        _moveDirection = value;
    }

    public void SetAnimationDirection(float2 value)
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var runtimeObj) &&
            CanMoveCrossMap)
        {
            direction = GameCommon.GetCharacterDirect(value, direction);
            runtimeObj.SetAnimationDirection(GameCommon.GetDirectValue(direction));
        }
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
                    if (_nowSpeed == 0)
                    {
                        TryTeamLeaderStop tryTeamLeaderStop = new TryTeamLeaderStop
                        {
                            characterId = instanceId
                        };
                        GameActionManager.instance.QueueAction(tryTeamLeaderStop);
                    }
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

    private void SetObjCoordinate(int3 coordinate)
    {
        MapCellController.instance.SetCharacterCoordinate(objCoordinate, coordinate, instanceId);
        objCoordinate = coordinate;
        if (CharacterManager.instance.controllerCharacter == this)
        {
            CheckNeighborhood();
        }
        // Debug.Log($"setCoordinate0:{coordinate}");
        if (SetCoordianteDele != null)
        {
            SetCoordianteDele.Invoke(coordinate);
        }
    }

    public void SetObjCoordinate(int mapInstance, int2 coordinate)
    {
        int3 newCoordinate = new int3(coordinate, mapInstance);
        MapCellController.instance.SetCharacterCoordinate(objCoordinate, newCoordinate, instanceId);
        objCoordinate = newCoordinate;

        if (CharacterManager.instance.controllerCharacter == this)
        {
            CheckNeighborhood();
        }

        //Debug.Log($"setCoordinate:{newCoordinate}");
        if (SetCoordianteDele != null)
        {
            SetCoordianteDele.Invoke(objCoordinate);
        }
    }

    private int NeighborhoodCharacter;

    private void CheckNeighborhood()
    {
        int clickCharacter = MapCellController.instance.GetClickCharacter(objCoordinate);
        if (clickCharacter != -1 && clickCharacter != instanceId && clickCharacter != NeighborhoodCharacter)
        {
            Character character = CharacterManager.instance.GetCharacter(clickCharacter);

            if (character != null)
            {
                if (character.team!=null)
                {
                    return;
                }
                EventReferenceData eventReferenceData = new EventReferenceData
                {
                    name = "CharacterId",
                    value = clickCharacter
                };
                EventReferenceData targetReferenceData = new EventReferenceData
                {
                    name = "TargetCharacter",
                    value = instanceId
                };
                EventReferenceData NextTalkReferenceData = new EventReferenceData
                {
                    name = "NextTalkEventId",
                    value = character.characterData.nextTalkEventId
                };
                bool temp = character is TempCharacter;

                if (!temp)
                {
                    AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                    {
                        characterId = character.instanceId,
                        friendAddType = FriendAddType.对话,
                        value = 1
                    };
                    GameActionManager.instance.QueueAction(addFriendShipValue);
                }

                GameEventManager.instance.AddGameEvent(
                temp ? character.characterData.tempTalkEventId : character.characterData.playerOperateEventId, new List<EventReferenceData>
                {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
                });
            }
        }
        NeighborhoodCharacter = clickCharacter;
    }

    public void SetNeighborhood(int characterId)
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
            EventReferenceData NextTalkReferenceData = new EventReferenceData
            {
                name = "NextTalkEventId",
                value = character.characterData.nextTalkEventId
            };
            bool temp = character is TempCharacter;
            GameEventManager.instance.AddGameEvent(
            temp ? character.characterData.playerOperateEventId : character.characterData.playerOperateEventId, new List<EventReferenceData>
            {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
            });

            NeighborhoodCharacter = character.instanceId;
        }
        else
        {
            NeighborhoodCharacter = 0;
        }
    }

    public void SetNeighborhood(Character character)
    {
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
            EventReferenceData NextTalkReferenceData = new EventReferenceData
            {
                name = "NextTalkEventId",
                value = character.characterData.nextTalkEventId
            };
            bool temp = character is TempCharacter;
            GameEventManager.instance.AddGameEvent(
            temp ? character.characterData.playerOperateEventId : character.characterData.playerOperateEventId, new List<EventReferenceData>
            {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
            });

            NeighborhoodCharacter = character.instanceId;
        }
        else
        {
            NeighborhoodCharacter = 0;
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
    }

    public void StartMove()
    {
        if (GameObjectCurveController.instance.StartLineMove(moveEnumeratorId))
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(1, this);
        }
        // GameController.instance.StopCoroutine(moveEnumerator);
    }

    public async void AddExp(int value)
    {
        bool levelUp = false;
        var profressionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(professionId);
        while (exp.AddExp(value))
        {
            exp.nowLevelExp = profressionData.GetLevelExp(level) - profressionData.GetLevelExp(level - 1);
            value = 0;
            SetLevel(level + 1);
            levelUp = true;
        }
        if (levelUp)
        {
        }
    }

    public async void SetLevel(int level, bool zero = false)
    {
        if (level != this.level)
        {
            var profressionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(professionId);
            //attributeType = profressionData.attributeType;
            if (profressionData.id == professionId)
            {
                if (zero)
                {
                    skills.Clear();
                    for (int i = 1; i <= level; i++)
                    {
                        int skillId = profressionData.GetLevelSkill(i);
                        if (skillId != -1)
                        {
                            skills.Add(skillId);
                        } 
                    } 
                }
                else
                {
                    int skillId = profressionData.GetLevelSkill(level);
                    if (skillId != -1)
                    {
                        skills.Add(skillId);
                    } 
                }
                ProfessionProperty =  profressionData.GetLevelProperty(level);
            }
            exp.nowLevelExp = profressionData.GetLevelExp(level) - profressionData.GetLevelExp(level - 1);
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

    public void SetPlayerOperate(int2 targetCoordinate)
    {
        int2 oldCoordinate = objCoordinate.xy;
        if (OldOperaCoordinate.x != int.MinValue)
        {
            oldCoordinate = OldOperaCoordinate;
        }
        MapCellController.instance.CheckPlayerTriggerEvent(objCoordinate.z,
            oldCoordinate, targetCoordinate,
          TriggerEventAction, oldOperateItem, false);
        OldOperaCoordinate = targetCoordinate;
    }

    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="eventid">事件id</param>
    /// <param name="reference">数据id</param>
    /// <param name="enter">是否进入事件</param>
    private void TriggerEventAction(int eventid, int reference, bool enter, bool controller = false)
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
                oldOperateItem = reference;
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
                if (oldOperateItem == reference)
                {
                    oldOperateItem = -1;
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

        GameEventManager.instance.AddGameEvent(eventid, eventReferenceDatas);
    }

    /// <summary>
    /// 设置坐标
    /// </summary>
    /// <param name="coordinate">x.y;z:地图id</param>
    public void SetCoordinate(int3 coordinate)
    {
        int2 oldCoordinate = objCoordinate.xy;
        if (mapInstance != coordinate.z)
        {
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
                objCoordinate.z, oldCoordinate, true, TriggerEventAction, oldOperateItem);

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
            checkCoordinate.xy += offsetCoordinate;

            MapCellController.instance.CheckPlayerTriggerEvent(coordinate.z, oldOperaCoordinate, checkCoordinate.xy,
           TriggerEventAction, oldOperateItem);
            oldCoordinate = OldOperaCoordinate = checkCoordinate.xy;
        }
        SetObjCoordinate(coordinate);
        CharacterCoordinateTrigger characterCoordinateTrigger = new CharacterCoordinateTrigger
        {
            characterId = instanceId,
            coordinate = coordinate
        };
        GameActionManager.instance.QueueAction(characterCoordinateTrigger,true);

        // ForwardTrigger(coordinate, direction);
    }

    public void SetCoordinate(int2 coordinate)
    {
        int2 oldCoordinate = objCoordinate.xy;
        int3 checkCoordinate = new int3(coordinate.xy, mapInstance);
        MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色, mapInstance, oldCoordinate, coordinate.xy,
           TriggerEventAction);

        SetObjCoordinate(checkCoordinate);
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

    public bool MoveCrossMap(int targetMap, int2 targetCoordinate, MoveEndAction moveEndAction = null, MoveEndAction changeCoordinateAction = null)
    {
        if (!CanMoveCrossMap)
        {
            return false;
        }

        bool result = false;
        Queue<int> resultList = MapCellController.instance.FindRoomList(objCoordinate.z, targetMap, ref result);
        if (result)
        {
            void FailedMoveAction()
            {
                CharacterMoveFailed characterMoveFailed = new CharacterMoveFailed
                {
                    characterId = instanceId,
                    oldTargetCoordinate = targetCoordinate,
                    oldTargetMapInstance = targetMap,
                };
                GameActionManager.instance.QueueAction(characterMoveFailed, true);
            }

            MoveCrossMap(resultList, targetCoordinate, moveEndAction, changeCoordinateAction, FailedMoveAction);
        }
        return result;
    }

    private void MoveCrossMap(Queue<int> moveRoomList, int2 targetCoordinate, MoveEndAction moveEndAction = null,
        MoveEndAction changeCoordinateAction = null, MoveEndAction failedMoveAction = null)
    {
        int nowMap = objCoordinate.z;
        if (moveRoomList.Count > 0)
        {
            int target = moveRoomList.Dequeue();

            int2 inCoordinate = int2.zero;
            if (MapCellController.instance.GetLinkMapInCoordinate(nowMap, target, ref inCoordinate))
            {
                Stack<int2> pathNodes = MapCellController.instance.FindPathNode(objCoordinate.xy, inCoordinate, nowMap);

                PlayerMove(pathNodes, () =>
                {
                    if (this == CharacterManager.instance.controllerCharacter)
                    {
                        canMove = false;
                        GameTimerController.instance.DeleyActionMain((int)(GameCommon.mapChangeLerpTime * 1000), () =>
                        {
                            MoveCrossMap(moveRoomList, targetCoordinate, moveEndAction);
                        });
                    }
                    else
                    {
                        MoveCrossMap(moveRoomList, targetCoordinate, moveEndAction);
                    }
                }, changeCoordinateAction, failedMoveAction);
            }
        }
        else
        {
            Stack<int2> pathNodes = MapCellController.instance.FindPathNode(objCoordinate.xy, targetCoordinate, nowMap);

            /*
            if (CellDebugDisplay.Instance)
            {
                CellDebugDisplay.Instance.DisplayPath(pathNodes.ToArray());
             }*/
            PlayerMove(pathNodes, moveEndAction, changeCoordinateAction, failedMoveAction);
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
            if (endAction != null)
            {
                endAction.Invoke();
            }
        }
    }
}