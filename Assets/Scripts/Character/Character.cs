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
    public CharacterProperty characterProperty;
}

[Serializable]
public struct CharacterProperty
{
    public int HP, MP, Power, MaxHP, MaxMP, MaxPower, AT, DF, Lucky,Speed;
    public int Other;

    public string GetItemProperty()
    {
        string result = "";

        if (HP != 0)
        {
            string operatorStr = HP > 0 ? "+" : "-";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.生命)}{operatorStr}{HP} ";
        }

        if (MP != 0)
        {
            string operatorStr = MP > 0 ? "+" : "-";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.法力)}{operatorStr}{MP}  ";
        }
        if (Power != 0)
        {
            string operatorStr = Power > 0 ? "+" : "-";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.体力)}{operatorStr}{Power}  ";
        }
        if (Other != 0)
        {
            bool nullValue = AT == 0 && DF == 0 && Lucky == 0 && Speed == 0;

            if (!nullValue)
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = $"{LanguageManage.SwitchStr("战斗时")} ";
                }
                else
                {
                    result = $"{result}\n{LanguageManage.SwitchStr("战斗时")} ";
                }

                if (AT != 0)
                {
                    string operatorStr = AT > 0 ? "+" : "";
                    result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.攻击)}{operatorStr}{AT} ";
                }
                if (DF != 0)
                {
                    string operatorStr = DF > 0 ? "+" : "";
                    result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.防御)}{operatorStr}{DF} ";
                }
                if (Lucky != 0)
                {
                    string operatorStr = Lucky > 0 ? "+" : "";
                    result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.幸运)}{operatorStr}{Lucky} ";
                }
                if (Speed != 0)
                {
                    string operatorStr = Speed > 0 ? "+" : "";
                    result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.敏捷)}{operatorStr}{Speed} ";
                }
                result = $"{result}{Other}{LanguageManage.SwitchStr("回合")}";
            }
        }
        return result;
    }
    public override string ToString()
    {
        string result = "";
        if (MaxHP != 0)
        {
            string operatorStr = MaxHP > 0 ? "+" : "";
            result = $"{LanguageManage.SwitchStr(CharacterPropertyType.最大生命)}{operatorStr}{MaxHP}  ";
        }
        if (HP != 0)
        {
            string operatorStr = HP > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.生命)}{operatorStr}{HP} ";
        }
        if (MaxMP != 0)
        {
            string operatorStr = MaxMP > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.最大法力)}{operatorStr}{MaxMP} ";
        }
        if (MP != 0)
        {
            string operatorStr = MP > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.法力)}{operatorStr}{MP}  ";
        }
        if (MaxPower != 0)
        {
            string operatorStr = MaxPower > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.最大体力)}{operatorStr}{MaxPower}";
        }
        if (Power != 0)
        {
            string operatorStr = Power > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.体力)}{operatorStr}{Power}  ";
        }
        if (AT != 0)
        {
            string operatorStr = AT > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.攻击)}{operatorStr}{AT}  ";
        }
        if (DF != 0)
        {
            string operatorStr = DF > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.防御)}{operatorStr}{DF}  ";
        }
        if (Lucky != 0)
        {
            string operatorStr = Lucky > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.幸运)}{operatorStr}{Lucky}  ";
        }
        if (Speed != 0)
        {
            string operatorStr = Speed > 0 ? "+" : "";
            result = $"{result}{LanguageManage.SwitchStr(CharacterPropertyType.敏捷)}{operatorStr}{Speed}  ";
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
    public void AddProperty(CharacterPropertyType propertyType, int value)
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
    public void AddOverrideProperty(CharacterPropertyType propertyType,int value)
    {
        switch (propertyType)
        {
            case CharacterPropertyType.体力:
                if (value < 0)
                {
                    Power += value;
                }
                else
                {
                    Power = Power > value ? Power : value;
                }

                break;

            case CharacterPropertyType.生命:
                if (value < 0)
                {
                    HP += value;
                }
                else
                    HP =HP> value?HP:value;
                break;

            case CharacterPropertyType.法力:
                if (value < 0)
                {
                    MP += value;
                }
                else
                    MP =MP> value?MP:value;
                break;

            case CharacterPropertyType.最大体力:
                if (value < 0)
                {
                    MaxPower += value;
                }
                else
                    MaxPower =MaxPower> value?MaxPower:value;
                break;

            case CharacterPropertyType.最大法力:
                if (value < 0)
                {
                    MaxMP += value;
                }
                else
                    MaxMP =MaxMP> value?MaxMP:value;
                break;

            case CharacterPropertyType.最大生命:
                if (value < 0)
                {
                    MaxHP += value;
                }
                else
                    MaxHP = MaxHP > value ? MaxHP : value;
                break;

            case CharacterPropertyType.攻击:
                if (value < 0)
                {
                    AT += value;
                }
                else
                    AT = AT > value ? AT : value;
                break;

            case CharacterPropertyType.防御:
                if (value < 0)
                {
                    DF += value;
                }
                else
                    DF = DF > value ? DF : value;
                break;

            case CharacterPropertyType.幸运:
                if (value < 0)
                {
                    Lucky += value;
                }
                else
                    Lucky = Lucky > value ? Lucky : value;
                break;
            case CharacterPropertyType.敏捷:
                if (value < 0)
                {
                    Speed += value;
                }
                else
                    Speed = Speed > value ? Speed : value;
                break;
            case CharacterPropertyType.自定义值:
                if (value < 0)
                {
                    Other += value;
                }
                else
                    Other = Other > value ? Other : value;
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
            packageInstancId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.Package, saveData.packageId);
            if (packageInstancId == 0)
            {
                packageInstancId = saveData.packageId;
            }
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
                // 构造函数不能 await，角色背包创建失败时通过统一异步日志暴露。
                AsyncTaskRunner.Run(CreatCharacterPackage(overridePackage, packageInstancId), nameof(CreatCharacterPackage));
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
    public void ClearEquip(ItemType itemType)
    {
        AsyncTaskRunner.Run(() => ClearEquipAsync(itemType), nameof(ClearEquip));
    }

    public async System.Threading.Tasks.Task ClearEquipAsync(ItemType itemType)
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
    void SetEquip(ItemType itemType,int2 Equip)
    {
        AsyncTaskRunner.Run(() => SetEquipAsync(itemType, Equip), nameof(SetEquip));
    }

    async System.Threading.Tasks.Task SetEquipAsync(ItemType itemType,int2 Equip)
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
    public void ChangeEquip(ItemData itemData, int packageId)
    {
        AsyncTaskRunner.Run(() => ChangeEquipAsync(itemData, packageId), nameof(ChangeEquip));
    }

    public async System.Threading.Tasks.Task ChangeEquipAsync(ItemData itemData, int packageId)
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

    public void ChangeData(int dataId)
    {
        AsyncTaskRunner.Run(() => ChangeDataAsync(dataId), nameof(ChangeData));
    }

    public async System.Threading.Tasks.Task ChangeDataAsync(int dataId)
    {
        if (this.dataId != dataId)
        {
            this.dataId = dataId;

            characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(dataId);

            this.professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
            ProfessionProperty = professionData.GetLevelProperty(level);
        }
    }
    private int3 objCoordinate;
    private int2 forwardCoordinate;

    private AttributeType attackAttributeType,defenceAttributeType;
    public AttributeType AttackAttributeType => attackAttributeType;
    public AttributeType DefenceAttributeType => defenceAttributeType;

    public int3 ObjCoordinate => objCoordinate;
    public int2 coordinate => objCoordinate.xy;
    public int mapInstance => objCoordinate.z;

    public Vector3 pos
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

    public GameObjectCurveController.MoveHandle moveHandle;

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

    /// <summary>
    /// 设置坐标
    /// </summary>
    /// <param name="coordinate">x.y;z:地图id</param>
    public void SetCoordinate(int3 coordinate, bool refreshObj = true, bool refreshMapTemp = true,
        bool fiexedDisplay = false)
    {
        //int2 forwordCoordinate = objCoordinate.xy + 2 * GameCommon.GetDirectionInt2(direction);
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
            // 坐标同步不能等待表现层刷新，刷新任务异常统一记录。
            AsyncTaskRunner.Run(CharacterManager.instance.RefreshNpcRuntimeObj(this, isController, refreshMapTemp, fiexedDisplay), nameof(SetCoordinate));
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


}
