using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct TryTeamLeaderMove : GameAction
{
    public int characterId;
    public float length;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryTeamLeaderSetCoordinate : GameAction
{
    public int characterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct JoinTeam : GameAction
{
    public int characterId;
    public int teamCharacterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            teamCharacterId = int.Parse(parameters[1].value);
        if (source != 0)
            characterId = source;
        if (target != 0)
            teamCharacterId = target;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct StopCharacterBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct StartCharacterBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetCharacterRandomCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int2 Coordinate;
    public int range;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;

        if (parameters.Count >= 4)
        {
            Coordinate.x = int.Parse(parameters[1].value);
            Coordinate.y = int.Parse(parameters[2].value); 
            range = int.Parse(parameters[3].value);
        }
        else if (parameters.Count >= 3)
        {
            Coordinate = GameCommon.StringToInt2(parameters[1].value);
            range = int.Parse(parameters[2].value);
        }
        else
        {
            Coordinate.x = target;
            Coordinate.y = value;
        }


        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetCharacterRandomPos : GameAction 
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public Vector3 pos;
    public int range;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;

        if (parameters.Count >= 5)
        {
            pos.x = float.Parse(parameters[1].value);
            pos.y = float.Parse(parameters[2].value);
            pos.z = float.Parse(parameters[3].value);
            range = int.Parse(parameters[4].value);
        }
        else if (parameters.Count >= 3)
        {
            pos = GameCommon.StringToVector3(parameters[1].value);
            range = int.Parse(parameters[2].value);
        }
        else
        {
            pos.x = target;
            pos.y = value;
        }
        

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetCharacterTempPos : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public Vector3 pos;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;

        if (parameters.Count >= 4)
        {
            pos.x = float.Parse(parameters[1].value);
            pos.y = float.Parse(parameters[2].value);
            pos.z = float.Parse(parameters[3].value);
        }
        else if (parameters.Count >= 2)
        {
            pos = GameCommon.StringToVector3(parameters[1].value);
        }
        else
        {
            pos.x = target;
            pos.y = value;
        }
        

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ChangeCharacter : GameAction
{
    public int instanceId;
    public int newDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public string parameter;
    public ParameterType parameterType;
    public bool boolValue;
    public int intValue;
    public float floatValue;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
        if (source != 0)
        {
            characterId = source;
        }

        GameActionManager.instance.QueueAction(this);
    }
}

//创建默认地图Npc
public struct CreatDefaultNPC : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct DestoryCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int dataId;
    public bool isTemp;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int mapInstance;
    public int coordinateX;
    public int coordinateY;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int mapInstance;
    public int coordinateX;
    public int coordinateY;
    public bool controller;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct CharacterLevelUp : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int level;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public List<int> players;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public List<int> players;
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public List<int> characters;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            propertyType = (CharacterPropertyType)Enum.Parse(typeof(CharacterPropertyType), parameters[1].value);
            Value = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }

    public int characterId;
    public CharacterPropertyType propertyType;
    public int Value;
}

public struct ChangeCharacterProperty : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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

public struct DisplayOrHideCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public int characterId;
    public bool display;
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            display = bool.Parse(parameters[1].value);
        }
        if (source != 0)
        {
            characterId = source;
        }
        display = target == 1;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetCharacterCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            coordinate.z = int.Parse(parameters[1].value);
            var parameter = parameters[1];
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        GameActionManager.instance.QueueAction(this);
    }
}

public struct AddFriendShipValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int value;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        GameActionManager.instance.QueueAction(this);
    }
}

public struct ClearEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int outPackageId;
    public ItemType itemType;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int outPackageId;
    public int itemId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int targetId, sourceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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

public struct RemoveCharacterMove : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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

public struct StopCharacterMove : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public SetValue SetValue;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct GetTempCharacterExit : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public SetInt3Value SetInt3Value;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct GetTempCharacterEnter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public SetInt3Value SetInt3Value;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetDirection : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public float2 direction;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int2 targetCoordinate;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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

public struct CharacterMoveFailed : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int oldTargetMapInstance;
    public int2 oldTargetCoordinate;
}