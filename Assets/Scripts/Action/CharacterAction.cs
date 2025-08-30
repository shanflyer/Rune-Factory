using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct SetCharacterStopCreate : GameAction
{
    public bool hide;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
    SetValue setValue = null, bool immediately = false)
    {  
        if (parameters.Count >=1)
            hide = bool.Parse(parameters[0].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshOperateCharacter : GameAction
{
    public int characterId;
    public bool join;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
     SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 2)
            join = bool.Parse(parameters[2].value);
         
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshOperateCharacters : GameAction
{
    public HashSet<int> joinCharacters,leaveCharacters; 
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TraceCharacterResult : GameAction
{
    public int characterId;
    public int targetId;
    public bool successed;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
      SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
            characterId= int.Parse(parameters[0].value);
        if (parameters.Count > 2)
            targetId = int.Parse(parameters[1].value);
        if (parameters.Count > 3)
            successed = bool.Parse(parameters[2].value);

        if (source != 0 && source != int.MinValue)
            characterId = source;
        if (target != 0 && target != int.MinValue)
            targetId = source;
        if (value >= 0 && value != int.MinValue)
            successed = value==1;

        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetTempCharacterTarget : GameAction
{
    public int characterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int area;
    public int2 targetCoordinate;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {        
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CheckNpcShopLink : GameAction
{
    public int characterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; } 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            characterId = int.Parse(parameters[0].value);


        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }

        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TempCharacterTalk : GameAction
{
    public int characterId; 
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public Action endAction;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            characterId = int.Parse(parameters[0].value);
       

        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
      
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RemoveCellCharacter : GameAction
{
    public int3 cell;
    public int characterId;
    public bool isTemp;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
            cell = GameCommon.StringToInt3(parameters[0].value);
        if (parameters.Count > 2)
            characterId = int.Parse(parameters[1].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryContinueBehavior : GameAction
{
    public int characterId; 

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryTeamLeaderMove : GameAction
{
    public int characterId;
    public float length;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryTeamLeaderStop : GameAction
{
    public int characterId;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null,
        SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryTeamLeaderSetCoordinate : GameAction
{
    public int characterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshTeam : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct LeaveTeam : GameAction
{
    public int teamCharacterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            teamCharacterId = int.Parse(parameters[0].value);
        if (source != 0)
            teamCharacterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct DestroyTeam : GameAction
{
    public int teamCharacterId;
    public bool holdDisplay;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    { 
        if (parameters.Count > 0)
            teamCharacterId = int.Parse(parameters[0].value);       
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryNPCJoinTeam : GameAction
{
    public int characterId;
    public int teamCharacterId; 
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            teamCharacterId = int.Parse(parameters[1].value);
        if (source != 0)
            characterId = source;
        if (target != 0)
            teamCharacterId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct JoinTeam : GameAction
{
    public int characterId;
    public int teamCharacterId;
    public bool holdDisplay;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            teamCharacterId = int.Parse(parameters[1].value);
        if (source != 0)
            characterId = source;
        if (target != 0)
            teamCharacterId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PauseCharacterBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct StopCharacterBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ReStartCharacterBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartCharacterBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0)
            characterId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChangeCharacterNewMap : GameAction
{
    public int characterInstance;
    public int newMap;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterInstance = int.Parse(parameters[0].value);
        if (source != 0 && source != int.MinValue)
            characterInstance = source;

        if (parameters.Count >= 2)
        {
            newMap = int.Parse(parameters[0].value);
        }
        if (target != 0 && target != int.MinValue)
            newMap = target;

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetCharacterRandomCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int2 Coordinate;
    public int range;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetCharacterRandomPos : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public Vector3 pos;
    public int range;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshCharacterPos : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (source != 0 && source != int.MinValue)
            characterId = source;



        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetCharacterTempPos : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public Vector3 pos;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChangeCharacter : GameAction
{
    public int instanceId;
    public int newDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            instanceId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            newDataId = int.Parse(parameters[1].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
 

/// <summary>
/// 设置角色动画
/// </summary>
public struct SetCharacterAnimator : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public string parameter;
    public ParameterType parameterType;
    public bool boolValue;
    public int intValue;
    public float floatValue;

    public void Init(List<Parameter> parameters, int source = 1, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public void SetAnimator(Animator animator)
    {
        if (animator == null) { return; }
        switch (parameterType)
        {
            case ParameterType.BOOL:
                animator.SetBool(parameter, boolValue);
                break;

            case ParameterType.INT:
                animator.SetInteger(parameter, intValue);
                break;

            case ParameterType.FLOAT:
                animator.SetFloat(parameter, floatValue);
                break;

            case ParameterType.TRIGGER:
                animator.SetTrigger(parameter);
                break;
        }
    }
}

/// <summary>
/// 设置角色动画
/// </summary>
public struct SetFightCharacterAnimator : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public string parameter;
    public ParameterType parameterType;
    public bool boolValue;
    public int intValue;
    public float floatValue;

    public void Init(List<Parameter> parameters, int source = 1, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public void SetAnimator(Animator animator)
    {
        if (animator == null) { return; }
        switch (parameterType)
        {
            case ParameterType.BOOL:
                animator.SetBool(parameter, boolValue);
                break;

            case ParameterType.INT:
                animator.SetInteger(parameter, intValue);
                break;

            case ParameterType.FLOAT:
                animator.SetFloat(parameter, floatValue);
                break;

            case ParameterType.TRIGGER:
                animator.SetTrigger(parameter);
                break;
        }
    }
}

public struct SetPlayerShaderPos : GameAction
{
    public Vector3 pos;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        pos = GameCommon.StringToVector3(parameters[0].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopTempCharacterCreat : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}
public struct ClearTempCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct DestoryCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int dataId;
    public bool isTemp;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            dataId = int.Parse(parameters[1].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetTempCharacterUpdata : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public bool canUpdata;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            canUpdata = parameters[0].value == "1";
        }
        else
        {
            canUpdata = source == 1;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct StartCreatSpecialTempCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int creatDataId;
    public int4 gridRange;
    public int2 areaKey;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct StartCreatTempCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int creatDataId;
    public bool clearAll;
    public bool prewarm;
    public int overrideMaxCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            creatDataId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            clearAll = bool.Parse(parameters[1].value);
        if (parameters.Count > 2)
            prewarm = bool.Parse(parameters[2].value);

        if (source > 0)
        {
            creatDataId = source;
        }
        if (target >= 0)
        {
            clearAll = target==1;
        }
        if (value >= 0)
        {
            prewarm = value == 1;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatTempCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int mapInstance;
    public int coordinateX;
    public int coordinateY;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            mapInstance = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            coordinateX = int.Parse(parameters[2].value);
        if (parameters.Count > 3)
            coordinateY = int.Parse(parameters[3].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
 
public struct CreatCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int mapInstance;
    public int coordinateX;
    public int coordinateY;
    public bool controller;
    public bool isPlayer;
    public int instanceId;
    public bool hideData;
    public string playerName;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopAllCharacterAutoFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct AllCharacterTryAutoFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct ExploreEnd : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct RefreshFightCharacterList : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CharacterLevelUp : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int level;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            level = int.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CharacterDeath : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatTeamPlayer : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public List<int> players;
    public bool holdDisplay;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        players = new List<int>();
        if (parameters.Count > 1)
        {
            try
            {
                holdDisplay = int.Parse(parameters[0].value)==1;

                for (int i = 1; i < parameters.Count; i++)
                {
                    players.Add(int.Parse(parameters[i].value));
                }
            }
            catch
            {
                holdDisplay = false;
                for (int i = 0; i < parameters.Count; i++)
                {
                    players.Add(int.Parse(parameters[i].value));
                }
            }
        }        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CreatFightPlayerInstance : GameAction
{
    public SetValue setValue { get; set; }
    public List<int> players;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        players = new List<int>();
        for (int i = 0; i < parameters.Count; i++)
        {
            players.Add(int.Parse(parameters[i].value));
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CreatFightPlayer : GameAction
{
    public SetValue setValue { get; set; }
    public List<int> players;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        players = new List<int>();
        for (int i = 0; i < parameters.Count; i++)
        {
            players.Add(int.Parse(parameters[i].value));
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshFightCharacterInfo : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshFightCharactersInfo : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public List<int> characters;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        characters = new List<int>();
        for (int i = 0; i < parameters.Count; i++)
        {
            characters.Add(int.Parse(parameters[i].value));
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshFightChapter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            id = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetCharacterProperty : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            propertyType = (CharacterPropertyType)Enum.Parse(typeof(CharacterPropertyType), parameters[1].value);
            Value = int.Parse(parameters[2].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public int characterId;
    public CharacterPropertyType propertyType;
    public int Value;
}

public struct ChangeCharacterProperty : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            propertyType = (CharacterPropertyType)Enum.Parse(typeof(CharacterPropertyType), parameters[1].value);
            changeValue = int.Parse(parameters[2].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public int characterId;
    public CharacterPropertyType propertyType;
    public int changeValue;
}

public struct CharacterPropertyTrigger : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
                    Lucky = int.Parse(parameter.parameters[8].value),
                    Other = int.Parse(parameter.parameters[9].value),
                };
            }
            finally { }
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public int characterId;
    public CharacterProperty characterProperty;
}

public struct DisplayOrHideCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public int characterId;
    public bool display;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            display = bool.Parse(parameters[1].value);
        }
        else
        {
            if (source != 0)
            {
                characterId = source;
            }

            display = target == 1;
        }       
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetCharacterTriggerItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Clear()
    {
        this = default;
    }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            mapItemEditId.x = int.Parse(parameters[1].value);
            mapItemEditId.y = int.Parse(parameters[2].value);
        }

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public int characterId;
    public int2 mapItemEditId;
}
public struct SetCharacterCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            try
            {
                coordinate.z = int.Parse(parameters[1].value);
                var parameter = parameters[1];
                if (parameter.parameters.Count >= 2)
                {
                    coordinate.x = int.Parse(parameter.parameters[0].value);
                    coordinate.y = int.Parse(parameter.parameters[1].value);
                }
            }
            catch
            {
                coordinate = GameCommon.StringToInt3(parameters[1].value);
            }
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public int characterId;
    public int3 coordinate;
}

public struct RefreshFriendShip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct AddFriendShipValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public FriendAddType friendAddType;
    public int value;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            friendAddType = (FriendAddType)Enum.Parse(typeof(FriendAddType), parameters[1].value);
        if (parameters.Count > 2)
        {
            this.value = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            characterId = source;
        }
        if (target > 0)
        {
            friendAddType = (FriendAddType)target;
        }
        if (value != -1)
        {
            this.value = value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ClearEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int outPackageId;
    public ItemType itemType;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayCharacterItemRenderer : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int itemId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            itemId = int.Parse(parameters[1].value);

        if (source != 0 && source != int.MinValue)
            characterId = source;
        if (target != 0 && target != int.MinValue)
            itemId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChangeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int outPackageId;
    public int itemId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct VisitNPC : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int targetId, sourceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            sourceId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            targetId = int.Parse(parameters[1].value);
        if (source != 0)
            sourceId = source;
        if (target != 0)
            targetId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RemoveCharacterMove : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            characterId = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopCharacterMove : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            characterId = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartCharacterMove : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            characterId = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct GetCharacterDataId : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public SetValue SetValue;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct GetTempCharacterExit : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public SetInt3Value SetInt3Value;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct GetTempCharacterEnter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public SetInt3Value SetInt3Value;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetDirection : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public float2 direction;
    public Direction directionEnum;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 2)
        {
            characterId = int.Parse(parameters[0].value);
            direction.x = float.Parse(parameters[1].value);
            direction.y = float.Parse(parameters[2].value);
        }
        else
        if (parameters.Count > 1)
        {
            characterId = int.Parse(parameters[0].value);
            directionEnum= (Direction)int.Parse(parameters[1].value); 
        }

        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetTargetDirection : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int2 targetCoordinate;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            targetCoordinate.x = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            targetCoordinate.y = int.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetCreateTempCharacterLevel : GameAction
{
    public int level;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            level = int.Parse(parameters[0].value);
        }
        else
        {
            level = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
   
}
public struct JoinInMultiNPCBehaviorGroup : GameAction
{
    public int characterId;
    public int groupId;
    public bool faceCenter;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if(parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            groupId = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            faceCenter = bool.Parse(parameters[2].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            groupId = target;
        }
        if(value!=0&&value!=int.MinValue)
        {
            faceCenter = value == 1;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}
public struct LeaveMultiNPCBehaviorGroup : GameAction
{
    public int characterId;
    public int groupId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CreatMultiNPCBehaviorGroup : GameAction
{
    public List<int> characters;
    public int dataId;
    public bool faceCenter;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}
public struct DestoryMultiNPCBehaviorGroup : GameAction
{
    public int groupId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}