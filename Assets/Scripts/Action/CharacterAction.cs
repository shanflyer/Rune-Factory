using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;

public struct ChangeCharacter : GameAction
{
    public int instanceId;
    public int newDataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            instanceId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            newDataId = int.Parse(parameters[1].value);
        GameActionManager.instance.QueueAction(this);
    }
}
/// <summary>
/// 设置角色动画
/// </summary>
public struct SetCharacterAnimator : GameAction
{
    public int characterId;
    public string parameter;
    public ParameterType parameterType;
    public bool boolValue;
    public int intValue;
    public float floatValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            parameter = parameters[1].value;
        if (parameters.Count > 2)
            parameterType = (ParameterType)int.Parse(parameters[2].value);

        if (parameters.Count > 3)
        {
            switch (parameterType)
            {
                case ParameterType.BOOL:
                    boolValue = bool.Parse(parameters[3].value);
                    break;
                case ParameterType.INT:
                    intValue = int.Parse(parameters[3].value);
                    break;
                case ParameterType.FLOAT:
                    floatValue = float.Parse(parameters[3].value);
                    break;
            }
        }

        GameActionManager.instance.QueueAction(this);
    }
}
//创建默认地图Npc
public struct CreatDefaultNPC : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DestoryCharacter : GameAction
{
    public int characterId;
    public int dataId;
    public bool isTemp;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            dataId = int.Parse(parameters[1].value);

        GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatTempCharacter : GameAction
{
    public int characterId;
    public int mapInstance;
    public int coordinateX;
    public int coordinateY;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            mapInstance = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            coordinateX = int.Parse(parameters[2].value);
        if (parameters.Count > 3)
            coordinateY = int.Parse(parameters[3].value);

        GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatCharacter : GameAction
{
    public int characterId;
    public int mapInstance;
    public int coordinateX;
    public int coordinateY;
    public bool controller;

    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            mapInstance = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            coordinateX = int.Parse(parameters[2].value);
        if (parameters.Count > 3)
            coordinateY = int.Parse(parameters[3].value);
        if (parameters.Count > 4)
            controller = bool.Parse(parameters[4].value);

        GameActionManager.instance.QueueAction(this);
    }
}

public struct ExploreEnd : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CharacterLevelUp : GameAction
{
    public int characterId;
    public int level;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            level = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CharacterDeath : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatTeamPlayer : GameAction
{
    public List<int> players;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        players = new List<int>();
        for (int i = 0; i < parameters.Count; i++)
        {
            players.Add(int.Parse(parameters[i].value));
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatFightPlayer : GameAction
{
    public List<int> players;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        players = new List<int>();
        for (int i = 0; i < parameters.Count; i++)
        {
            players.Add(int.Parse(parameters[i].value));
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshFightCharacterInfo : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshFightCharactersInfo : GameAction
{
    public List<int> characters;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        characters = new List<int>();
        for (int i = 0; i < parameters.Count; i++)
        {
            characters.Add(int.Parse(parameters[i].value));
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshFightChapter : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshCharacter : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetCharacterProperty : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            propertyType = (CharacterPropertyType)Enum.Parse(typeof(CharacterPropertyType), parameters[1].value);
            setValue = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
    public int characterId;
    public CharacterPropertyType propertyType;
    public int setValue;

}
public struct ChangeCharacterProperty : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            propertyType = (CharacterPropertyType)Enum.Parse(typeof(CharacterPropertyType), parameters[1].value);
            changeValue = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
    public int characterId;
    public CharacterPropertyType propertyType;
    public int changeValue;
}

public struct CharacterPropertyTrigger : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);

            var parameter = parameters[1];

            try
            {
                characterProperty = new CharacterProperty
                {
                    HP = int.Parse(parameter.parameters[0].value),
                    MP = int.Parse(parameter.parameters[1].value),
                    Power = int.Parse(parameter.parameters[2].value),
                    MaxHP = int.Parse(parameter.parameters[3].value),
                    MaxMP = int.Parse(parameter.parameters[4].value),
                    MaxPower = int.Parse(parameter.parameters[5].value),
                    AT = int.Parse(parameter.parameters[6].value),
                    DF = int.Parse(parameter.parameters[7].value),
                    Crit = int.Parse(parameter.parameters[8].value),
                    Dodge = int.Parse(parameter.parameters[9].value),
                    Other = int.Parse(parameter.parameters[10].value),
                };
            }
            finally { }

        }

        GameActionManager.instance.QueueAction(this);
    }
    public int characterId;
    public CharacterProperty characterProperty;

}
public struct SetCharacterCoordinate : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            coordinate.z = int.Parse(parameters[1].value);
            var parameter = parameters[2];
            if (parameter.parameters.Count >= 2)
            {
                coordinate.x = int.Parse(parameter.parameters[0].value);
                coordinate.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
    public int characterId;
    /// <summary>
    /// x:坐标x,y:坐标y,z:地图id
    /// </summary>
    public int3 coordinate;

}
public struct CharacterCoordinateTrigger : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            coordinate.z = int.Parse(parameters[1].value);
            var parameter = parameters[2];
            if (parameter.parameters.Count >= 2)
            {
                coordinate.x = int.Parse(parameter.parameters[0].value);
                coordinate.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
    public int characterId;
    public int3 coordinate;

}
public struct RefreshFriendShip : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        GameActionManager.instance.QueueAction(this);
    }
}
public struct AddFriendShipValue : GameAction
{
    public int characterId;
    public int value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            this.value = int.Parse(parameters[1].value);
        if (source != 0)
        {
            characterId = source;
        }
        if (value != -1)
        {
            this.value = value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct RefreshEquip : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ClearEquip : GameAction
{
    public int characterId;
    public int outPackageId;
    public ItemType itemType;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            outPackageId = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            itemType = (ItemType)int.Parse(parameters[2].value);

        if (source != 0)
            characterId = source;
        if (target != 0)
            outPackageId = target;
        if (value != 0)
            itemType = (ItemType)value;

        GameActionManager.instance.QueueAction(this);
    }
}
public struct ChangeEquip : GameAction
{
    public int characterId;
    public int outPackageId;
    public int itemId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            outPackageId = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            itemId = int.Parse(parameters[2].value);

        if (source != 0)
            characterId = source;
        if (target != 0)
            outPackageId = target;
        if (value != 0)
            itemId = value;

        GameActionManager.instance.QueueAction(this);
    }
}
public struct VisitNPC : GameAction
{
    public int targetId, sourceId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
            sourceId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            targetId = int.Parse(parameters[1].value);
        if (source != 0)
            sourceId = source;
        if (target != 0)
            targetId = target;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct StopCharacterMove : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            characterId = target;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct StartCharacterMove : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            characterId = target;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetCharacterDataId : GameAction
{
    public int characterId;
    public SetValue SetValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetTempCharacterExit : GameAction
{
    public SetInt3Value SetInt3Value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetTempCharacterEnter : GameAction
{
    public SetInt3Value SetInt3Value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetDirection : GameAction
{
    public int characterId;
    public float2 direction;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            direction.x = float.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            direction.y = float.Parse(parameters[1].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetTargetDirection : GameAction
{
    public int characterId;
    public int2 targetCoordinate;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            targetCoordinate.x = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            targetCoordinate.y = int.Parse(parameters[1].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetCreateTempCharacterLevel : GameAction
{
    public int level;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            level = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            level = target;
        }

        GameActionManager.instance.QueueAction(this);
    }
}