using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public int HP, MP, Power, MaxHP,MaxMP,MaxPower, AT, DF, Crit, Dodge;
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
        if (Crit != 0)
        {
            string operatorStr = Crit > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.暴击}{operatorStr}{Crit}  ";
        }
        if (Dodge != 0)
        {
            string operatorStr = Dodge > 0 ? "+" : "-";
            result = $"{CharacterPropertyType.闪避}{operatorStr}{Dodge}  ";
        }
        return result;
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
            Crit = property0.Crit - property1.Crit,
            Dodge = property0.Dodge - property1.Dodge,
            Other = property0.Other - property1.Other
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
            Crit = property0.Crit + property1.Crit,
            Dodge = property0.Dodge + property1.Dodge,
            Other = property0.Other + property1.Other
        };
        return CharacterProperty;
    }
    public static CharacterProperty operator *(CharacterProperty property0, float value)
    {
        CharacterProperty CharacterProperty = new CharacterProperty
        {
            HP =(int)(property0.HP *value),
            MP = (int)(property0.MP * value),
            AT = (int)(property0.AT * value),
            DF = (int)(property0.DF * value),
            Power = (int)(property0.Power * value),
            MaxHP = (int)(property0.MaxHP * value),
            MaxMP = (int)(property0.MaxMP * value),
            MaxPower = (int)(property0.MaxPower * value),
            Crit = (int)(property0.Crit * value),
            Dodge = (int)(property0.Dodge * value),
            Other = (int)(property0.Other * value)
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
            case CharacterPropertyType.暴击:
                return Crit;
            case CharacterPropertyType.闪避:
                return Dodge;
            case CharacterPropertyType.最大体力:
                return MaxPower;
            case CharacterPropertyType.最大生命:
                return MaxHP;
            case CharacterPropertyType.最大法力:
                return MaxMP;
            default:
                return Other;
        }
    }
    public static CharacterProperty Lerp(CharacterProperty start, CharacterProperty end,float LerpValue)
    {
        return start + (end - start) * LerpValue;
    }

    public void SetProperty(SetCharacterProperty setCharacterProperty)
    {
        switch (setCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                Power = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.生命:
                HP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.法力:
                MP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.最大体力:
                MaxPower = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.最大法力:
                MaxMP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.最大生命:
                MaxHP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.攻击:
                AT = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.防御:
                DF = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.闪避:
                Crit = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.暴击:
                Dodge = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.自定义值:
                Other = setCharacterProperty.setValue;
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
    public int weapon;
    public int clothes;
}
public partial class Character
{
    private bool isController = false;
    private int oldOperateItem = -1; 
    public int OperateItem => oldOperateItem;

    private int2 OldOperaCoordinate=new int2(int.MinValue);

    public CharacterData characterData;

    public Equip Equip => equip;
    private Equip equip;
    public int characterPackage;
    public List<int> skills = new List<int>();

    public void SetController(bool controller)
    {
        isController = controller;
        oldOperateItem= -1;
    }
    public Character() 
    {
    }
   
    public Character(CharacterData characterData,int instanceId,int overridePackage=0)
    {
        this.characterData = characterData;
        this.instanceId = instanceId;
        dataId = characterData.id;
        professionId = characterData.profession;
        name = characterData.characterName;
        behavior = characterData.behavior;
        SetLevel(1,true);
        CreatCharacterPackage(overridePackage);
       
    }
   
    protected virtual async Task CreatCharacterPackage(int overridePackage = 0)
    {
        characterPackage = await PackageManager.instance.CreatGamePackage(overridePackage == 0?
            characterData.packageId:overridePackage, 0);
    }

    public async void ClearEquip(ItemType itemType)
    {
        switch(itemType)
        {
            case ItemType.武器:
                {
                    ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(equip.weapon);
                    if (oldItemData != null)
                        characterProperty = characterProperty - oldItemData.property;
                }
                equip.weapon = 0;
                break;
            case ItemType.防具:
                {
                    ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(equip.clothes);
                    if (oldItemData != null)
                        characterProperty = characterProperty - oldItemData.property;
                }
                equip.clothes = 0;
                break;
        }
        GameActionManager.instance.QueueAction(new RefreshEquip
        {
            characterId = instanceId
        }) ;
    }
    public async void ChangeEquip(ItemData itemData, int packageId)
    {
        Item item = new Item();
        if (itemData.type == ItemType.武器)
        {
            ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(equip.weapon);
            if (oldItemData != null)
            {
                item.count = 1;
                item.dataId = oldItemData.id;
                characterProperty = characterProperty - oldItemData.property;
            }
            equip.weapon = itemData.id;
        }
        else if (itemData.type == ItemType.防具)
        {
            ItemData oldItemData = await GameDataManager.instance.GetAsyncData<ItemData>(equip.clothes);
            if (oldItemData != null)
            {
                item.count = 1;
                item.dataId = oldItemData.id;
                CharacterProperty = characterProperty - oldItemData.property;
            }
            equip.clothes = itemData.id;
        }
        if (item.dataId != 0)
        {
            PackageManager.instance.SetItemInPackage(item, packageId);
        }
        if (itemData != null)
        {
            CharacterProperty = characterProperty + itemData.property;
        }
        GameActionManager.instance.QueueAction(new RefreshEquip
        {
            characterId = instanceId
        });
    }
    

    public CharacterEquipAndPropertyData CharacterEquipAndPropertyData
    {
        get
        {
            return new CharacterEquipAndPropertyData 
            { 
                id=instanceId,
                name=name,
                characterProperty = characterProperty,
                equip = equip 
            };
        }
    }

    public CharacterProperty CharacterProperty
    {
        get => characterProperty;
        set
        {
            characterProperty=value;
            CharacterPropertyTrigger();
        }
    }
    void CharacterPropertyTrigger()
    {
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = CharacterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger);
    }

    private CharacterProperty characterProperty;

    public int groupId=-1;
    public int professionId;
    public int dataId;
    public int Level
    {
        get => level;
    }
    private int level;

    public Exp exp;
    public int bag;

    public string name;
    private int3 objCoordinate;

    public int3 ObjCoordinate =>objCoordinate;
    public int2 coordinate => objCoordinate.xy;
    public int mapInstance => objCoordinate.z;

    public int instanceId;
    public Direction direction {private set;get; }

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
                direction = GameCommon.GetCharacterDirect(moveDirection);

                if (CharacterManager.instance.GetRuntimeCharacterObj(instanceId, out var runtimeObj))
                {
                    runtimeObj.SetAnimationDirection(value);
                }
            }
           
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
                    runtimeObj.SetAnimationSpeed(value);
                }
            } 
        }
        get
        {
            return _nowSpeed;
        }
    }

    public int behavior;

    public int moveEnumeratorId;  
    private CharacterProperty nowProperty;
    public void SetObjCoordinate(int3 coordinate)
    { 
        MapCellController.instance.SetCharacterCoordinate(objCoordinate, coordinate, instanceId);
        objCoordinate = coordinate;
    }
    public void SetObjCoordinate(int mapInstance, int2 coordinate)
    {
        int3 newCoordinate = new int3(coordinate, mapInstance);
        MapCellController.instance.SetCharacterCoordinate(objCoordinate,newCoordinate,instanceId);
        objCoordinate=newCoordinate;
    }
    public void StopMove()
    {
        if (GameObjectCurveController.instance.StopLineMove(moveEnumeratorId))
        {
            CharacterManager.instance.SetCharacterAnimationSpeed(0, this);
        }
       // GameController.instance.StopCoroutine(moveEnumerator);
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
            exp.nowLevelExp = profressionData.GetLevelExp(level) - profressionData.GetLevelExp(level-1);
            value = 0;
            SetLevel(level+1);
            levelUp = true;
        }
        if (levelUp)
        {

        }
    }
    public async void SetLevel(int level,bool zero=false)
    {
        if (level != this.level)
        {
            var profressionData =await GameDataManager.instance.GetAsyncData<ProfessionData>(professionId);
            if (profressionData.id == professionId)
            {
                if (zero)
                {
                    skills.Clear();
                    for(int i = 0; i <= level; i++)
                    {
                        int skillId = profressionData.GetLevelSkill(i);
                        if (skillId != -1)
                        {
                            skills.Add(skillId);
                        }
                        CharacterProperty = CharacterProperty + profressionData.GetLevelProperty(level);
                    }
                }
                else
                {
                    int skillId = profressionData.GetLevelSkill(level);
                    if (skillId != -1)
                    {
                        skills.Add(skillId);
                    }
                    CharacterProperty = CharacterProperty - nowProperty;
                    nowProperty = profressionData.GetLevelProperty(level);
                    CharacterProperty = CharacterProperty + nowProperty;
                    
                }
               
            }
            exp.nowLevelExp = profressionData.GetLevelExp(level) - profressionData.GetLevelExp(level - 1);
            this.level = level;
        }
    }

    public void SetProperty(int HP=-1, int MP = -1, int Power = -1, int MaxHP = -1, int MaxMP = -1, int MaxPower = -1, int AT = -1, int DF = -1, int Crit = -1, int Dodge = -1
        ,int Other=-1)
    {
        if(HP>=0)
            characterProperty.HP = HP;
        if (MP >= 0)
            characterProperty.MP = MP;
        if (Power >= 0)
            characterProperty.Power = Power;
        if (MaxHP >= 0)
            characterProperty.MaxHP = MaxHP;
        if (MaxMP >= 0)
            characterProperty.MaxMP = MaxMP;
        if (MaxPower >= 0)
            characterProperty.MaxPower = MaxPower;
        if (AT >= 0)
            characterProperty.AT = AT;
        if (DF >= 0)
            characterProperty.DF = DF;
        if (Crit >= 0)
            characterProperty.Crit = Crit;
        if (Dodge >= 0)
            characterProperty.Dodge = Dodge;
        if (Other >= 0)
            characterProperty.Other = Other;

        this.characterProperty = characterProperty;
    }
    public void SetProperty(SetCharacterProperty setCharacterProperty)
    {
        characterProperty.SetProperty(setCharacterProperty); 
    }
    
    public void AddProperty(ChangeCharacterProperty changeCharacterProperty)
    {
        switch (changeCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                characterProperty.Power = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.生命:
                characterProperty.HP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.法力:
                characterProperty.MP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.最大体力:
                characterProperty.MaxPower = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.最大法力:
                characterProperty.MaxMP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.最大生命:
                characterProperty.MaxHP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.攻击:
                characterProperty.AT = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.防御:
                characterProperty.DF = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.闪避:
                characterProperty.Crit = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.暴击:
                characterProperty.Dodge = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.自定义值:
                characterProperty.Other = changeCharacterProperty.changeValue;
                break;
        }
        this.characterProperty = characterProperty;
    }

    public void SetPlayerOperate(int2 targetCoordinate)
    {
        int2 oldCoordinate = objCoordinate.xy;
        if (OldOperaCoordinate.x != int.MinValue)
        {
            oldCoordinate=OldOperaCoordinate;
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
    private void TriggerEventAction(int eventid, int reference, bool enter,bool controller=false)
    {
        if (eventid == 0&&reference==0)
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
                GameActionManager.instance.QueueAction(showMapObjTips);

                TriggerEnter triggerEnter = new TriggerEnter
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerEnter);
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
                GameActionManager.instance.QueueAction(closeMapObjTips);
                TriggerExit triggerExit = new TriggerExit
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerExit);
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
                GameActionManager.instance.QueueAction(triggerEnter);
            }
            else
            {
                TriggerExit triggerExit = new TriggerExit
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerExit);
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
     
    public void SetCoordinate(int3 coordinate)
    {
        int2 oldCoordinate = objCoordinate.xy;
        if (mapInstance != objCoordinate.z)
        {
            MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色,
                objCoordinate.z, oldCoordinate, true, TriggerEventAction);
             
            if(isController)
            {
                int2 oldOperaCoordinate = objCoordinate.xy;
                if (OldOperaCoordinate.x != int.MinValue)
                {
                    oldOperaCoordinate = OldOperaCoordinate;
                }
                MapCellController.instance.CheckPlayerTriggerEvent(
                objCoordinate.z, oldCoordinate, true, TriggerEventAction, oldOperateItem);
            }
            oldCoordinate = OldOperaCoordinate = new int2(int.MinValue);
        } 


        MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色, mapInstance, oldCoordinate, coordinate.xy,
           TriggerEventAction);
        if (isController)
        {
            int2 oldOperaCoordinate = objCoordinate.xy;
            if (OldOperaCoordinate.x != int.MinValue)
            {
                oldOperaCoordinate = OldOperaCoordinate;
            }
            MapCellController.instance.CheckPlayerTriggerEvent(mapInstance, oldOperaCoordinate, coordinate.xy,
           TriggerEventAction,oldOperateItem);
            oldCoordinate = OldOperaCoordinate = coordinate.xy;
        }
        SetObjCoordinate(coordinate);
        CharacterCoordinateTrigger characterCoordinateTrigger = new CharacterCoordinateTrigger
        {
            characterId = instanceId, 
            coordinate = coordinate
        };
        GameActionManager.instance.QueueAction(characterCoordinateTrigger);
    }
    public bool MoveCrossMap(int targetMap, int2 targetCoordinate,MoveEndAction moveEndAction=null)
    {
        Queue<int> moveRoomList = new Queue<int>();
        bool result = false;
        Queue<int> resultList = MapCellController.instance.FindRoomList(objCoordinate.z, targetMap, moveRoomList, ref result);
        if (result)
        {
            MoveCrossMap(resultList, targetCoordinate,moveEndAction);
        }
        return result;
    }


    void MoveCrossMap(Queue<int> moveRoomList, int2 targetCoordinate, MoveEndAction moveEndAction = null)
    {
        int nowMap = objCoordinate.z;
        if (moveRoomList.Count > 0)
        {
            int target = moveRoomList.Dequeue();

            int2 inCoordinate = int2.zero;
            if (MapCellController.instance.GetLinkMapInCoordinate(nowMap, target, ref inCoordinate))
            {
                Stack<int2> pathNodes =MapCellController.instance.FindPathNode(objCoordinate.xy, inCoordinate,nowMap);


                PlayerMove(pathNodes, () => { 
                    MoveCrossMap(moveRoomList, targetCoordinate,moveEndAction);
                });
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
            PlayerMove(pathNodes, moveEndAction);
        }


    }

    public void PlayerMove(Stack<int2> pathNodes, MoveEndAction endAction = null)
    {
        if (pathNodes.Count > 0)
        {
            CharacterManager.instance.CharacterMoveTarget(this, pathNodes, endAction);
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

public class TempCharacter : Character
{
    public int tempDataId => _tempDataId;
    private int _tempDataId;
    public int templevel
    {
        set
        {
            if (_templevel != value)
            {
                _templevel = value;
                RefreshBehavior();
            }
        }
        get => _templevel;
    }
    public int _templevel;
    public TempCharacter(CharacterData characterData, int instanceId, int tempDataId) : base(characterData, instanceId)
    {
        this._tempDataId = tempDataId;
    }
    
    async void RefreshBehavior()
    {
        var tempCharacterData = await GameDataManager.instance.GetAsyncData<TempCharacterData>(tempDataId);
        if (!tempCharacterData.levelBehavior.TryGetValue(templevel,out var externalBehaviorTree))
        {
            externalBehaviorTree = tempCharacterData.defaultBehavior;
        }
        CharacterBehaviorManager.instance.DestroyBehavior(instanceId);
        CharacterBehaviorManager.instance.AddBehavior(instanceId, externalBehaviorTree);
    } 
}


public class Player : Character
{ 
    public Player(CharacterData characterData, int instanceId) : base(characterData, instanceId)
    {
       
    }
    protected override async Task CreatCharacterPackage(int overridePackageId=0)
    {
       await base.CreatCharacterPackage();
        PackageManager.instance.AddPlayerPackage(overridePackageId == 0?characterPackage:overridePackageId);
    }
    public Player(string name)
    {
        this.name = name;
        //bag = PackageManager.instance.CreatGamePackage(10, "PlayerBag",instanceId);
        //临时
    } 
    public Player(CharacterSaveData characterSaveData)
    {
        this.name = characterSaveData.name;
        //SetProperty(characterSaveData.energy, characterSaveData.health, characterSaveData.satiety); 
        //bag = PackageManager.instance.CreatGamePackage(characterSaveData.packageCount, characterSaveData.packageName, instanceId);
        //foreach(var item in characterSaveData.items)
        //{
        //    PackageManager.instance.SetItemInPackage(item, bag); 
        //}
    }


}