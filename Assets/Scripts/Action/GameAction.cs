using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

public interface GameAction 
{ 
    public  void Init(List<Parameter> parameters,int source=0,int target=0,int value=-1); 
}

public delegate void SetValue(int value);
public delegate void SetInt3Value(int3 value);
public delegate void SetResult(bool value);


public struct NewDay : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
 
public struct TryVisitShop : GameAction
{
    public string ShopName;
    public int CharacterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public int fromCharacterId;
    public int toCharacterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public int giveCharacter, receiveCharacter;
    public int giftId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public int characterId;
    public int storeCounterId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    { 
        GameActionManager.instance.QueueAction(this);
    }
}

public struct BuyPlayerGood: GameAction
{
    public int storeCounterId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetStoreCounterItem : GameAction,IReferenceData
{
    public int storeCounterId;
    public int itemId;
    public int count;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public  void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        nullAction =int.Parse(parameters[0].value);
        playerId = source;
        storeCounterId = target;
        GameActionManager.instance.QueueAction(this);
    }
} 
public struct DisplayStoreCounter : GameAction
{
    public bool display;
    public int itemInstanceId;
    public Transform transform;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct ShopBuySuccess : GameAction
{
    public int buyCount;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct InitInputAction : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct StartRoundFight : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct WaitAction : GameAction
{ 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DisplayFightScene : GameAction
{
    
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct JumpFilm : GameAction
{
    public string filmName;
    public float jumpTime;

   
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public string content;
    public Sprite icon;
    public string name;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    { 
        GameActionManager.instance.QueueAction(this);
    }
}
public struct Talk : GameAction
{
    public int talkId, characterId;
    public bool displayFunction;
    public int nextTalkEventId;
    public Action endAction;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public string sceneName;
    public int beforeLoadActionId, afterLoadActionId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct EnterChapter : GameAction
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
 
public struct StopFilm : GameAction
{
    public string filmName;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1) 
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1) 
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
