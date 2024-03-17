using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;

public interface GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public delegate void SetPanelReference(BaseReference baseReference);

public delegate void SetValue(int value);

public delegate void SetInt3Value(int3 value);

public delegate void SetResult(bool value);

public struct PlayCharacterTimeLine : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public string playName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            characterId = int.Parse(parameters[0].value);
            playName = parameters[1].value;
        }
        if (source != 0)
        {
            characterId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetFixedCamera : GameAction
{
    public SetValue setValue { get; set; }
    public bool fixedCamera;
    public Vector3 fixedPos;
    public FlowCameraType flowCameraType;
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            fixedCamera = bool.Parse(parameters[0].value);
        }
        if (parameters.Count > 3)
        {
            fixedPos = new Vector3(float.Parse(parameters[1].value), float.Parse(parameters[2].value),
                float.Parse(parameters[3].value));
        }
        else
        {
            fixedPos.x = float.MinValue;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshGameSaveData : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct NewDay : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryVisitShop : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string ShopName;
    public int CharacterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            ShopName = parameters[0].value;
        if (parameters.Count > 1)
            CharacterId = int.Parse(parameters[1].value);

        if (source != 0)
        {
            CharacterId = source;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGiveGiftOpenPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int fromCharacterId;
    public int toCharacterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            fromCharacterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            toCharacterId = int.Parse(parameters[1].value);

        if (source != 0)
            fromCharacterId = source;
        if (target != 0)
            toCharacterId = target;

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct GiveGift : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int giveCharacter, receiveCharacter;
    public int giftId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            giveCharacter = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            receiveCharacter = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            giftId = int.Parse(parameters[2].value);

        if (source != 0)
            giveCharacter = source;
        if (target != 0)
            receiveCharacter = target;
        if (value != 0)
            giftId = value;

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryBuyPlayerGood : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int storeCounterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct BuyPlayerGood : GameAction
{
    public SetValue setValue { get; set; }
    public int storeCounterId;
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            storeCounterId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            storeCounterId = target;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ShowCoin : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public Vector2 pos;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            pos.x = float.Parse(parameters[0].value);
            pos.y = float.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct UpdateGameTime : GameAction
{
    public int year, season, day;
    public int hour, minute;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetStoreCounterItem : GameAction, IReferenceData
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int storeCounterId;
    public int itemId;
    public int count;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StoreCounterSetSelectItemAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int targetObj;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        targetObj = target;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int storeCounterId;
    public int playerId;
    public int nullAction;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        nullAction = int.Parse(parameters[0].value);
        playerId = source;
        storeCounterId = target;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public bool display;
    public int itemInstanceId;
    public Transform transform;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            display = bool.Parse(parameters[0].value);
            itemInstanceId = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int itemInstanceId;
    public int itemDataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            itemInstanceId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PlayerTalkItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int displayTime;
    public int characterId;
    public int ItemId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            displayTime = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            characterId = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            ItemId = int.Parse(parameters[2].value);

        characterId = source;
        ItemId = target;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SwitchOperateList : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ShopBuySuccess : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int buyCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            buyCount = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            packageId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshPlayerGold : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct InitInputAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct EndPlayerRound : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct StopAutoFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct PlayerFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct FightCharacterMove: GameAction
{
    public int characterId;
    public int newIndex;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            newIndex = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartRoundFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct WaitAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            int WaitValue = int.Parse(parameters[0].value);
            var _parameters = parameters[0].parameters;
            for (int i = 0; i < _parameters.Count; i++)
            {
                var Parameter = _parameters[i];
                GameTimerController.instance.DeleyActionMain(WaitValue, () =>
                {
                    GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters, source, target);
                });
            }
        }
    }
}

public struct ActionList : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int targetId;
    public int hurtValue;
    public HurtResultType hurtResultType;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            targetId = int.Parse(parameters[0].value);
            hurtValue = int.Parse(parameters[1].value);
            hurtResultType = (HurtResultType)int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ActionSkillEstimate : GameAction
{
    public int skillId;
    public int sourceId;
    public int targetId;
    public int index;
    public bool displayHurt;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 5)
        {
            skillId = int.Parse(parameters[0].value);
            sourceId = int.Parse(parameters[1].value);
            targetId = int.Parse(parameters[2].value);
            index = int.Parse(parameters[3].value);
            displayHurt = bool.Parse(parameters[4].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct HideFightScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayFightScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct JumpFilm : GameAction
{
    public string filmName;
    public float jumpTime;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            filmName = parameters[0].value;
            jumpTime = float.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SimpleTalk : GameAction
{
    public int talkId, characterId;
    public Action endAction;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            talkId = int.Parse(parameters[0].value);
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[1].value);
        }
        if (target != int.MinValue)
        {
            talkId = target;
        }
        if (source != int.MinValue)
        {
            characterId = source;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DynamicTalk : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string content;
    public Sprite icon;
    public string name;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct Talk : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int talkId, characterId;
    public bool displayFunction;
    public int nextTalkEventId;
    public Action endAction;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            talkId = int.Parse(parameters[0].value);
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SwitchScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string sceneName;
    public int beforeLoadActionId, afterLoadActionId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            sceneName = parameters[0].value;
        if (parameters.Count >= 2)
            beforeLoadActionId = int.Parse(parameters[1].value);
        if (parameters.Count >= 3)
            afterLoadActionId = int.Parse(parameters[2].value);

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChapterStepAction : GameAction
{
    public SetResult setResult { set; get; }

    public SetValue setValue { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct OpenChapter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            id = source;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct EnterChapter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string filmName;
    public string path;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            filmName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            path = parameters[1].value;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct HideFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string filmName;
    public string path;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            filmName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            path = parameters[1].value;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string filmName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PlayFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string filmName;
    public string assetName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            filmName = parameters[0].value;
        }
        if (parameters.Count >= 2)
        {
            assetName = parameters[1].value;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PauseFilm : GameAction
{
    public string filmName;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryStartAutoExplore : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryStartAutoBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    { 
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SwitchAutoExplore : GameAction
{
    public bool explore;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
       

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetAutoExplore : GameAction
{
    public bool auto;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            auto = bool.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SwitchFunctionButton : GameAction
{
    public bool fight;
    public bool auto;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            fight = bool.Parse(parameters[0].value);
        }
        if (parameters.Count >= 2)
        {
            auto = bool.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}