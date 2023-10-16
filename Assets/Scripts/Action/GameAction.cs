using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using static Cinemachine.DocumentationSortingAttribute;

public interface GameAction 
{ 
    public  void Init(List<Parameter> parameters,int source=0,int target=0); 
}

public delegate void SetValue(int value);
public delegate void SetInt3Value(int3 value);
public delegate void SetResult(bool value);
public struct StopCharacterMove : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetTempCharacterExit : GameAction
{
    public SetInt3Value SetInt3Value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetTempCharacterEnter : GameAction
{
    public SetInt3Value SetInt3Value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryBuyPlayerGood : GameAction
{
    public int characterId;
    public int storeCounterId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    { 
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetDirection : GameAction
{
    public int characterId;
    public float2 direction;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
        {
            direction.x = float.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            direction.y= float.Parse(parameters[1].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetTargetDirection : GameAction
{
    public int characterId;
    public int2 targetCoordinate;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
public struct BuyPlayerGood: GameAction
{
    public int storeCounterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 1)
        {
            storeCounterId = int.Parse(parameters[0].value); 
        }
        if (target != 0)
        {
            storeCounterId = target;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ShowCoin : GameAction
{
    public Vector2 pos;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 1)
        {
            pos.x = float.Parse(parameters[0].value);
            pos.y = float.Parse(parameters[1].value); 
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct UpdateGameTime : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetStoreCounterItem : GameAction,IReferenceData
{
    public int storeCounterId;
    public int itemId;
    public int count;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 2)
        {
            storeCounterId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            count = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            storeCounterId = source;
        }
        if (target != 0)
        {
            itemId = target;
            count = -1;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct StoreCounterSetSelectItemAction : GameAction
{
    public int targetObj;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        targetObj = target;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetStoreCounter : GameAction
{
    public int storeCounterId;
    public int playerId;
    public int nullAction;
    public  void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        nullAction =int.Parse(parameters[0].value);
        playerId = source;
        storeCounterId = target;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct OpenPackage : GameAction
{
    public int packageId;
    public string selectActionName;
    public int selectActionId;
    public int targetObj;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 2)
        {
            packageId = int.Parse(parameters[0].value);
            selectActionName = parameters[1].value;
            selectActionId = int.Parse(parameters[2].value);
        }
        targetObj = target;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DisplayStoreCounter : GameAction
{
    public bool display;
    public int itemInstanceId;
    public Transform transform;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 1)
        {
            display =bool.Parse(parameters[0].value);
            itemInstanceId = int.Parse(parameters[1].value);
            
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryCreatStoreCounter : GameAction
{
    public int itemInstanceId;
    public int itemDataId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 1)
        {
            itemInstanceId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct PlayerTalkItem : GameAction
{
    public int displayTime;
    public int characterId;
    public int ItemId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
            displayTime = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            characterId = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            ItemId = int.Parse(parameters[2].value);

        characterId = source;
        ItemId = target;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SwitchOperateList : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CloseMapObjTips : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ShowMapObjTips : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ShopBuySuccess : GameAction
{
    public int buyCount;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
        {
            buyCount = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshPackage : GameAction
{
    public int packageId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
        {
            packageId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshPlayerGold : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct InitInputAction : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
                    floatValue=float.Parse(parameters[3].value);
                    break;
            }
        }
            
        GameActionManager.instance.QueueAction(this);
    }
}
//创建默认地图Npc
public struct CreatDefaultNPC : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DestoryTempCharacter : GameAction
{
    public int characterId;
    public int dataId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
public struct ChangeWorld : GameAction
{
    public string worldName;
    public int displayMap;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    { 
        if (parameters.Count > 0)
        {
            worldName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            displayMap =int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ExploreEnd : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CharacterLevelUp : GameAction
{
    public int characterId;
    public int level;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
public struct StartRoundFight : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CharacterDeath : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct WaitAction : GameAction
{ 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if(parameters.Count> 0)
        {
            int WaitValue =int.Parse(parameters[0].value);
            var _parameters = parameters[0].parameters;
            for (int i = 0; i < _parameters.Count; i++)
            {
                var Parameter = _parameters[i];
                GameTimerController.instance.DelayAction(WaitValue, () =>
                {
                    GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters,source,target);
                });
                
            }
        }

       
    }
}
public struct ActionList : GameAction
{ 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        for (int i = 0; i < parameters.Count; i++)
        {
            var Parameter = parameters[i];
            GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters, source, target);
        } 
    }
}
public struct DisplayHurt : GameAction
{
    public int targetId;
    public int hurtValue;
    public HurtResultType hurtResultType;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 3)
        {
            targetId = int.Parse(parameters[0].value);
            hurtValue = int.Parse(parameters[1].value);
            hurtResultType =(HurtResultType) int.Parse(parameters[2].value); 
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ActionSkillEstimate : GameAction
{
    public int skillId;
    public int sourceId;
    public int targetId;
    public int index;
    public bool displayHurt;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 5)
        {
            skillId = int.Parse(parameters[0].value);
            sourceId = int.Parse(parameters[1].value);
            targetId = int.Parse(parameters[2].value);
            index = int.Parse(parameters[3].value);
            displayHurt = bool.Parse(parameters[4].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct HideFightScene : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DisplayFightScene : GameAction
{
    
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct JumpFilm : GameAction
{
    public string filmName;
    public float jumpTime;

   
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 2)
        {
            filmName = parameters[0].value;
            jumpTime = float.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SimpleTalk : GameAction
{
    public int talkId, characterId;
    public Action endAction;
    
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
            talkId = int.Parse(parameters[0].value);
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[1].value);
        }
        if(target != 0)
        {
            talkId = target;
        }
        if (source != 0)
        {
            characterId = source;
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct Talk : GameAction
{
    public int talkId, characterId;
    public bool displayFunction;
    public Action endAction;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
            talkId =int.Parse(parameters[0].value);
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[1].value);
        }
        else
        {
            characterId = -1;
        }
        if (parameters.Count >= 3)
        {
            displayFunction = bool.Parse(parameters[2].value);
        }
            GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatTeamPlayer : GameAction
{
    public List<int> players;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        players = new List<int>();
        for(int i = 0; i < parameters.Count; i++)
        {
            players.Add(int.Parse(parameters[i].value));
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SwitchScene : GameAction
{
    public string sceneName;
    public int beforeLoadActionId, afterLoadActionId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
            sceneName = parameters[0].value;
        if (parameters.Count >= 2)
            beforeLoadActionId = int.Parse(parameters[1].value);
        if (parameters.Count >= 3)
            afterLoadActionId = int.Parse(parameters[2].value);

        GameActionManager.instance.QueueAction(this);
    }
}
public struct ChapterStepAction : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct EnterChapter : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshFightCharacterInfo : GameAction
{
    public int characterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        characters = new List<int>();
        for(int i=0;i<parameters.Count; i++)
        {
            characters.Add(int.Parse(parameters[i].value));
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshFightChapter : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshCharacterProperty : GameAction
{
    public int id;
    public CharacterProperty characterProperty;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 0)
        {
            id =int.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}

public struct StopFilm : GameAction
{
    public string filmName;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct PlayFilm : GameAction
{
    public string filmName;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct PauseFilm : GameAction
{
    public string filmName;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetItemAnimation : GameAction
{
    public int id;
    public int keyX;
    public int keyY;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 3)
        {
            id = int.Parse(parameters[0].value);
            keyX = int.Parse(parameters[1].value);
            keyY = int.Parse(parameters[2].value); 
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct RemovePackageItem : GameAction
{
    public int packageId;
    public int itemDataId;
    public int itemCount;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct AddPackageItem : GameAction
{
    public int packageId;
    public int itemDataId;
    public int itemCount;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TriggerEnter : GameAction
{
    public int eventId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
        {
            eventId = int.Parse(parameters[0].value); 
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TriggerExit : GameAction
{
    public int eventId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
        {
            eventId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DeleteMapItem : GameAction
{
    //public int mapId;
    public int mapItemInstanceId;
    public bool triggerClear;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 2)
        { 
            mapItemInstanceId = int.Parse(parameters[0].value);
            triggerClear = bool.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct ChangeMapItem : GameAction
{
    public int itemId;
    public int newDataId;
    public int2 animationKey;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 4)
        {
            itemId = int.Parse(parameters[0].value);
            newDataId = int.Parse(parameters[1].value);

            var parameter = parameters[2];
            if (parameter.parameters.Count >= 2)
            {
                animationKey.x = int.Parse(parameter.parameters[0].value);
                animationKey.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct AddMapItem : GameAction
{
    public int mapId;
    public int dataId;
    public int2 coordinate;

    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 3)
        {
            mapId = int.Parse(parameters[0].value);
            dataId = int.Parse(parameters[1].value);

            var parameter = parameters[2];
            if (parameter.parameters.Count >= 2)
            {
                coordinate.x= int.Parse(parameter.parameters[0].value);
                coordinate.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct AttachMapItemData : GameAction
{
    public int mapItemIntanceId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 1)
        {
            mapItemIntanceId = int.Parse(parameters[0].value); 
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatRuntimePackage: GameAction
{
    public string name;
    public Vector2Int key;
    public int instanceId;
    public int caseCount;
    public List<Item> Items;
    public bool itemPackage;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        /*
        if (parameters.Count >= 6)
        {
            name = parameters[0].value;
            var parameter = parameters[1];
            if (parameter.parameters.Count >= 2)
            {
                key.x = int.Parse(parameter.parameters[0].value);
                key.y = int.Parse(parameter.parameters[1].value);
            }

            instanceId = int.Parse(parameters[2].value);
            caseCount = int.Parse(parameters[3].value);

            Items = new List<Item>();
            var itemParameter = parameters[4];
            if (itemParameter.parameters.Count > 0)
            {
                for(int i=0;i< itemParameter.parameters.Count; i++)
                {
                    var _Parameter = itemParameter.parameters[i];
                    if (_Parameter.parameters.Count >= 3)
                    {
                        Item item = new Item
                        {
                            instanceId = int.Parse(_Parameter.parameters[0].value),
                            dataId = int.Parse(_Parameter.parameters[1].value),
                            count= int.Parse(_Parameter.parameters[2].value)
                        };
                        Items.Add(item);
                    }
                }
            }

            itemPackage = bool.Parse(parameters[5].value);
           
        }

        */

        GameActionManager.instance.QueueAction(this);
    }
}
public struct RemoveRuntimePackage : GameAction
{
    public Vector2Int key;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 1)
        {  
            var parameter = parameters[0];
            if (parameter.parameters.Count >= 2)
            {
                key.x = int.Parse(parameter.parameters[0].value);
                key.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct ItemUseAction : GameAction
{
    public int packageId;
    public int itemId;
    public int itemCount;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }

    public ItemUseAction(int packageId,int itemId,int itemCount)
    {
        this.packageId = packageId;
        this.itemId = itemId;
        this.itemCount = itemCount;
    }
   
}
public struct SwitchFunctionButton : GameAction
{
    public bool fight;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0)
    {
        if (parameters.Count >= 1)
        {
            fight = bool.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct ClosePanelAction : GameAction
{
    public Type type;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 1)
        {
            type = Type.GetType(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }

    public ClosePanelAction(Type type)
    {
        this.type = type;
    } 
}
public struct OpenPanelAction : GameAction
{
    public Type type;
    public string dataId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 2)
        {
            type = Type.GetType(parameters[0].value);
            dataId = parameters[1].value;
        }
        else
        {
            if (parameters.Count >= 1)
            {
                type = Type.GetType(parameters[0].value);
                dataId = null;
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
    public OpenPanelAction(Type type, string dataId = null)
    {
        this.type = type;
        this.dataId = dataId;
    } 
}

public struct SetCharacterProperty : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            propertyType = (CharacterPropertyType) Enum.Parse(typeof(CharacterPropertyType),parameters[1].value);
            setValue= int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
    public int characterId;
    public CharacterPropertyType propertyType;
    public int setValue;
 
}
public struct ChangeCharacterProperty : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
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

public struct CharacterPropertyTrigger:GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
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
public struct CharacterCoordinateTrigger : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0) 
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