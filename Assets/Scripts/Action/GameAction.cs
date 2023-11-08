using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public interface GameAction 
{ 
    public  void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this);
    }
    public SetResult setResult { get; set; }
}
public delegate void SetPanelReference(BaseReference baseReference);
public delegate void SetValue(int value);
public delegate void SetInt3Value(int3 value);
public delegate void SetResult(bool value);

public struct RefreshGameSaveData : GameAction
{
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct NewDay : GameAction
{
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
 
public struct TryVisitShop : GameAction
{
    public SetResult setResult { get; set; }
    public string ShopName;
    public int CharacterId;

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
            ShopName = parameters[0].value;
        if (parameters.Count > 1)
            CharacterId = int.Parse(parameters[1].value);

        if (source != 0)
        {
            CharacterId = source;
        }


        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryGiveGiftOpenPackage : GameAction
{
    public SetResult setResult { get; set; }
    public int fromCharacterId;
    public int toCharacterId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
            fromCharacterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            toCharacterId = int.Parse(parameters[1].value);

        if (source != 0)
            fromCharacterId = source;
        if (target != 0)
            toCharacterId = target;

        GameActionManager.instance.QueueAction(this);
    }
}
public struct GiveGift : GameAction
{
    public SetResult setResult { get; set; }
    public int giveCharacter, receiveCharacter;
    public int giftId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if(parameters.Count > 0)
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

        GameActionManager.instance.QueueAction(this);
    }

  
}

public struct TryBuyPlayerGood : GameAction
{
    public SetResult setResult { get; set; }
    public int characterId;
    public int storeCounterId; 
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    { 
        GameActionManager.instance.QueueAction(this);
    }
}

public struct BuyPlayerGood: GameAction
{
    public int storeCounterId;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public Vector2 pos;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetStoreCounterItem : GameAction,IReferenceData
{
    public SetResult setResult { get; set; }
    public int storeCounterId;
    public int itemId;
    public int count;

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int targetObj;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        targetObj = target;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetStoreCounter : GameAction
{
    public SetResult setResult { get; set; }
    public int storeCounterId;
    public int playerId;
    public int nullAction;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        nullAction =int.Parse(parameters[0].value);
        playerId = source;
        storeCounterId = target;
        GameActionManager.instance.QueueAction(this);
    }
} 
public struct DisplayStoreCounter : GameAction
{
    public SetResult setResult { get; set; }
    public bool display;
    public int itemInstanceId;
    public Transform transform;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int itemInstanceId;
    public int itemDataId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int displayTime;
    public int characterId;
    public int ItemId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct ShopBuySuccess : GameAction
{
    public SetResult setResult { get; set; }
    public int buyCount;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int packageId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct InitInputAction : GameAction
{
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct StartRoundFight : GameAction
{
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct WaitAction : GameAction
{
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int targetId;
    public int hurtValue;
    public HurtResultType hurtResultType;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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

    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DisplayFightScene : GameAction
{
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct JumpFilm : GameAction
{
    public string filmName;
    public float jumpTime;
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
public struct DynamicTalk : GameAction
{
    public SetResult setResult { get; set; }
    public string content;
    public Sprite icon;
    public string name;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    { 
        GameActionManager.instance.QueueAction(this);
    }
}
public struct Talk : GameAction
{
    public SetResult setResult { get; set; }
    public int talkId, characterId;
    public bool displayFunction;
    public int nextTalkEventId;
    public Action endAction;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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


public struct SwitchScene : GameAction
{
    public SetResult setResult { get; set; }
    public string sceneName;
    public int beforeLoadActionId, afterLoadActionId;

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct EnterChapter : GameAction
{
    public SetResult setResult { get; set; }
    public int id;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
 
public struct StopFilm : GameAction
{
    public SetResult setResult { get; set; }
    public string filmName;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public string filmName;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }

        GameActionManager.instance.QueueAction(this);
    }
}
 
public struct SwitchFunctionButton : GameAction
{
    public bool fight;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null) 
    {
        if (parameters.Count >= 1)
        {
            type = Type.GetType(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }

     
}
public struct OpenPanelAction : GameAction
{
    public SetResult setResult { get; set; }
    public Type type;
    public string dataId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null) 
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
    
}
