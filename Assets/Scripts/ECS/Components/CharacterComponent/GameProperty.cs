using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct CharacterProperty : IComponentData
{
    public GameProperty ProfessionProperty;
    public GameProperty EquipmentProperty;
    public GameProperty OtherAddProperty;
    public GameProperty OtherMulProperty;
    public AttributeType attackAttributeType, defenseAttributeType;
    public GameProperty Property
    {
        get => (ProfessionProperty + EquipmentProperty + OtherAddProperty) * OtherMulProperty;

    }
}

[System.Serializable]
public struct GameProperty
{
    public int HP, MP, Power, MaxHP, MaxMP, MaxPower, AT, DF, Lucky, Speed;
    public int Other;

    public static GameProperty One
    {
        get
        {
            GameProperty characterProperty = new GameProperty
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
                Speed = 1
            };
            return characterProperty;
        }
    }
    public static GameProperty FullPercent
    {
        get
        {
            GameProperty characterProperty = new GameProperty
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
    public static GameProperty operator -(GameProperty property0, GameProperty property1)
    {
        GameProperty CharacterProperty = new GameProperty
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
            Speed = property0.Speed - property1.Speed
        };
        return CharacterProperty;
    }

    public static GameProperty operator +(GameProperty property0, GameProperty property1)
    {
        GameProperty CharacterProperty = new GameProperty
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
            Speed = property0.Speed + property1.Speed
        };
        return CharacterProperty;
    }
    public static GameProperty operator *(GameProperty property0, GameProperty property1)
    {
        GameProperty CharacterProperty = new GameProperty
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
            Speed = property0.Speed * property1.Speed
        };
        return CharacterProperty;
    }
    public static GameProperty operator *(GameProperty property0, float value)
    {
        GameProperty CharacterProperty = new GameProperty
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
            Speed = (int)(property0.Speed * value),
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
     
}
public static class CharacterPropertyExtensions
{
    public static string GetItemProperty(this GameProperty property)
    {
        string result = "";

        if (property.HP != 0)
        {
            string operatorStr = property.HP > 0 ? "+" : "-";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.生命), operatorStr, property.HP.ToString(), " ");
        }

        if (property.MP != 0)
        {
            string operatorStr = property.MP > 0 ? "+" : "-";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.法力), operatorStr, property.MP.ToString(), " ");
        }
        if (property.Power != 0)
        {
            string operatorStr = property.Power > 0 ? "+" : "-";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.体力), operatorStr, property.Power.ToString(), " ");
        }
        if (property.Other != 0)
        {
            bool nullValue = property.AT == 0 && property.DF == 0 && property.Lucky == 0 && property.Speed == 0;

            if (!nullValue)
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = GameCommon.BlendString(LanguageManage.SwitchStr("战斗时"), " ");
                }
                else
                {
                    result = GameCommon.BlendString(result, "\n", LanguageManage.SwitchStr("战斗时"), " ");
                }

                if (property.AT != 0)
                {
                    string operatorStr = property.AT > 0 ? "+" : "";
                    result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.攻击), operatorStr, property.AT.ToString(), " ");
                }
                if (property.DF != 0)
                {
                    string operatorStr = property.DF > 0 ? "+" : "";
                    result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.防御), operatorStr, property.DF.ToString(), " ");
                }
                if (property.Lucky != 0)
                {
                    string operatorStr = property.Lucky > 0 ? "+" : "";
                    result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.幸运), operatorStr, property.Lucky.ToString(), " ");
                }
                if (property.Speed != 0)
                {
                    string operatorStr = property.Speed > 0 ? "+" : "";
                    result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.敏捷), operatorStr, property.Speed.ToString(), " ");
                }
                result = GameCommon.BlendString(result, property.Other.ToString(), LanguageManage.SwitchStr("回合"));
            }
        }
        return result;
    }
    public static string ToString(this GameProperty property)
    {
        string result = "";
        if (property.MaxHP != 0)
        {
            string operatorStr = property.MaxHP > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.最大生命), operatorStr, property.MaxHP.ToString(), " ");
        }
        if (property.HP != 0)
        {
            string operatorStr = property.HP > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.生命), operatorStr, property.HP.ToString(), " ");
        }
        if (property.MaxMP != 0)
        {
            string operatorStr = property.MaxMP > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.最大法力), operatorStr, property.MaxMP.ToString(), " ");
        }
        if (property.MP != 0)
        {
            string operatorStr = property.MP > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.法力), operatorStr, property.MP.ToString(), " ");
        }
        if (property.MaxPower != 0)
        {
            string operatorStr = property.MaxPower > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.最大体力), operatorStr, property.MaxPower.ToString(), " ");
        }
        if (property.Power != 0)
        {
            string operatorStr = property.Power > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.体力), operatorStr, property.Power.ToString(), " ");
        }
        if (property.AT != 0)
        {
            string operatorStr = property.AT > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.攻击), operatorStr, property.AT.ToString(), " ");
        }
        if (property.DF != 0)
        {
            string operatorStr = property.DF > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.防御), operatorStr, property.DF.ToString(), " ");
        }
        if (property.Lucky != 0)
        {
            string operatorStr = property.Lucky > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.幸运), operatorStr, property.Lucky.ToString(), " ");
        }
        if (property.Speed != 0)
        {
            string operatorStr = property.Speed > 0 ? "+" : "";
            result = GameCommon.BlendString(result, LanguageManage.SwitchStr(CharacterPropertyType.敏捷), operatorStr, property.Speed.ToString(), " ");
        }
        return result;
    }
    public static GameProperty Lerp(GameProperty start, GameProperty end, float LerpValue)
    {
        return start + (end - start) * LerpValue;
    }
    public static void AddOverrideProperty(this ref GameProperty prop, CharacterPropertyType type, int value)
    {
        
        switch (type)
        {
            case CharacterPropertyType.体力:
                if (value < 0)
                {
                    prop.Power += value;
                }
                else
                {
                    prop.Power = prop.Power > value ? prop.Power : value;
                }

                break;

            case CharacterPropertyType.生命:
                if (value < 0)
                {
                    prop.HP += value;
                }
                else
                    prop.HP = prop.HP > value ? prop.HP : value;
                break;

            case CharacterPropertyType.法力:
                if (value < 0)
                {
                    prop.MP += value;
                }
                else
                    prop.MP = prop.MP > value ? prop.MP : value;
                break;

            case CharacterPropertyType.最大体力:
                if (value < 0)
                {
                    prop.MaxPower += value;
                }
                else
                    prop.MaxPower = prop.MaxPower > value ? prop.MaxPower : value;
                break;

            case CharacterPropertyType.最大法力:
                if (value < 0)
                {
                    prop.MaxMP += value;
                }
                else
                    prop.MaxMP = prop.MaxMP > value ? prop.MaxMP : value;
                break;

            case CharacterPropertyType.最大生命:
                if (value < 0)
                {
                    prop.MaxHP += value;
                }
                else
                    prop.MaxHP = prop.MaxHP > value ? prop.MaxHP : value;
                break;

            case CharacterPropertyType.攻击:
                if (value < 0)
                {
                    prop.AT += value;
                }
                else
                    prop.AT = prop.AT > value ? prop.AT : value;
                break;

            case CharacterPropertyType.防御:
                if (value < 0)
                {
                    prop.DF += value;
                }
                else
                    prop.DF = prop.DF > value ? prop.DF : value;
                break;

            case CharacterPropertyType.幸运:
                if (value < 0)
                {
                    prop.Lucky += value;
                }
                else
                    prop.Lucky = prop.Lucky > value ? prop.Lucky : value;
                break;
            case CharacterPropertyType.敏捷:
                if (value < 0)
                {
                    prop.Speed += value;
                }
                else
                    prop.Speed = prop.Speed > value ? prop.Speed : value;
                break;
            case CharacterPropertyType.自定义值:
                if (value < 0)
                {
                    prop.Other += value;
                }
                else
                    prop.Other = prop.Other > value ? prop.Other : value;
                break;
        }
    }
    public static void ChangeProperty(this ref GameProperty prop, ChangeCharacterProperty changeCharacterProperty)
    {
        switch (changeCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                prop.Power += changeCharacterProperty.changeValue;
                prop.Power = math.clamp(prop.Power, 0, prop.MaxPower);
                break;

            case CharacterPropertyType.生命:
                prop.HP += changeCharacterProperty.changeValue;
                prop.HP = math.clamp(prop.HP, 0, prop.MaxHP);
                break;

            case CharacterPropertyType.法力:
                prop.MP += changeCharacterProperty.changeValue;
                prop.MP = math.clamp(prop.MP, 0, prop.MaxMP);
                break;

            case CharacterPropertyType.最大体力:
                prop.MaxPower += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.最大法力:
                prop.MaxMP += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.最大生命:
                prop.MaxHP += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.攻击:
                prop.AT += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.防御:
                prop.DF += changeCharacterProperty.changeValue;
                break;

            case CharacterPropertyType.幸运:
                prop.Lucky += changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.敏捷:
                prop.Speed += changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.自定义值:
                prop.Other += changeCharacterProperty.changeValue;
                break;
        }
    }
    public static void SetProperty(this ref GameProperty prop, SetCharacterProperty setCharacterProperty)
    {
        switch (setCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                prop.Power = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.生命:
                prop.HP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.法力:
                prop.MP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.最大体力:
                prop.MaxPower = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.最大法力:
                prop.MaxMP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.最大生命:
                prop.MaxHP = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.攻击:
                prop.AT = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.防御:
                prop.DF = setCharacterProperty.Value;
                break;

            case CharacterPropertyType.幸运:
                prop.Lucky = setCharacterProperty.Value;
                break;
            case CharacterPropertyType.敏捷:
                prop.Speed = setCharacterProperty.Value;
                break;
            case CharacterPropertyType.自定义值:
                prop.Other = setCharacterProperty.Value;
                break;
        }
    }
    public static void AddProperty(this ref GameProperty prop, CharacterPropertyType type, int value)
    {
        switch (type)
        {
            case CharacterPropertyType.体力:
                prop.Power += value;
                break;

            case CharacterPropertyType.生命:
                prop.HP += value;
                break;

            case CharacterPropertyType.法力:
                prop.MP += value;
                break;

            case CharacterPropertyType.最大体力:
                prop.MaxPower += value;
                break;

            case CharacterPropertyType.最大法力:
                prop.MaxMP += value;
                break;

            case CharacterPropertyType.最大生命:
                prop.MaxHP += value;
                break;

            case CharacterPropertyType.攻击:
                prop.AT += value;
                break;

            case CharacterPropertyType.防御:
                prop.DF += value;
                break;

            case CharacterPropertyType.幸运:
                prop.Lucky += value;
                break;
            case CharacterPropertyType.敏捷:
                prop.Speed += value;
                break;
            case CharacterPropertyType.自定义值:
                prop.Other += value;
                break;
        }
    }
}