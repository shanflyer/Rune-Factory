using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public interface GameAction
{
    public void Clear();
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public void SetResult(bool result)
    {
        if (setResult != null)
        {
            setResult(result);
        }
    }
    public void SetValue(int Value)
    {
        if (setValue != null)
        {
            setValue(Value);
        }
    }
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  
}

public delegate void SetPanelReference(BaseReference baseReference);

public delegate void SetValue(int value);
public delegate void SetFloatValue(float value);

public delegate void SetInt3Value(int3 value);

public delegate void SetResult(bool value);

public struct PayEndAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public void Clear() { this = default; }
}
public struct PlayCharacterTimeLine : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
public struct SetFixedPlayerShaderPos : GameAction
{
    public bool fixedPos;
    public SetValue setValue { get; set; } 
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            fixedPos = bool.Parse(parameters[0].value);
        }
        else
        {
            fixedPos = source != 0;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetCameraPixelValue : GameAction
{
    public SetValue setValue { get; set; } 
    public int pixelValue;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            pixelValue = int.Parse(parameters[0].value);
        }
        
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetCameraConfiner2D : GameAction
{
    public bool enable;
    public SetValue setValue { get; set; } 
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            enable = bool.Parse(parameters[0].value);
        }
         
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshMapCamera : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    { 
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetFixedCamera : GameAction
{
    public SetValue setValue { get; set; }
    public bool fixedCamera;
    public Vector3 fixedPos;
    public Vector3 offsetPos;
    public FlowCameraType flowCameraType;
    public int pixelValue;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct NewHour : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct NewDay : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshShopLevel : GameAction
{
    public string shopName;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            shopName = parameters[0].value; 
         
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct OpenShopItem : GameAction
{
    public int shopId;
    public int itemId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            shopId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            itemId = int.Parse(parameters[1].value);

        if (source != 0&&source!=int.MinValue)
        {
            shopId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            itemId = target;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryVisitShop : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string ShopName;
    public int CharacterId;
    public int ShopObjId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            ShopName = parameters[0].value;
        if (parameters.Count > 1)
            CharacterId = int.Parse(parameters[1].value);

        if (source != 0&&source!=int.MinValue)
        {
            CharacterId = source;
        }
        if (target != 0)
        {
            ShopObjId = target;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGiveGiftOpenPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
public struct SwitchAutoStore : GameAction
{
    public bool isAuto;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            isAuto = bool.Parse(parameters[0].value);
        }
        if (source != int.MinValue)
        {
            isAuto = source == 1;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetPlayerStoreOpen : GameAction
{
    public bool open;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            open = bool.Parse(parameters[0].value);
        }
        if (source != int.MinValue)
        {
            open = source == 1;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryBuyPlayerGood : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public void Clear() { this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public int totalMinute;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetStoreCounterItem : GameAction, IReferenceData
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
public struct CreatStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int itemInstanceId;
    public int storeDataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            itemInstanceId = int.Parse(parameters[0].value);
            storeDataId = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
} 

public struct PlayerTalkItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ShopBuySuccess : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
public struct AddPlayerGold : GameAction
{
    public int value;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            this.value = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshPlayerGold : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct InitInputAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct EndPlayerRound : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopAutoFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PlayerFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct FightCharacterMove : GameAction
{
    public int characterId;
    public int newIndex;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct WaitAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            int WaitValue = int.Parse(parameters[0].value);
            var _parameters = parameters[0].parameters;
            for (int i = 0; i < _parameters.Count; i++)
            {
                var Parameter = _parameters[i];
                if (GameDataManager.instance.GlobalData.debug)
                { 
                    Debug.Log($"wait type:{Parameter.value}--Parameter:{Parameter}");
                }
               
                GameTimerController.instance.DelayAction(WaitValue, () =>
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int targetId;
    public string hurtValue;
    public HurtResultType hurtResultType;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            targetId = int.Parse(parameters[0].value);
            hurtValue = parameters[1].value;
            hurtResultType = (HurtResultType)int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
 
public struct NextActionSkillEstimate : GameAction
{
    public int skillId;
    public int sourceId; 
    public bool displayHurt;
    public List<int> targets;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 5)
        {
            skillId = int.Parse(parameters[0].value);
            sourceId = int.Parse(parameters[1].value); 
            displayHurt = bool.Parse(parameters[2].value);
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayFightScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            filmName = parameters[0].value;
            jumpTime = float.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct SimpleTalk : GameAction
{
    public int talkId, characterId;
    public Action endAction;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int talkId, characterId;
    public bool displayFunction;
    public int nextTalkEventId;
    public List<int> fixedFunctions;
    public Action endAction;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        displayFunction = false;
        if (parameters.Count >= 1)
        {
            talkId = int.Parse(parameters[0].value);
            if (parameters.Count >= 2)
            {
                characterId = int.Parse(parameters[1].value);
            }
            else
            {
                characterId = -1;
            }
        }
        else
        {
            characterId = -1;
        }


       
        if (parameters.Count >= 3)
        {
            displayFunction = bool.Parse(parameters[2].value);
        }
        fixedFunctions = null;
        if (parameters.Count >= 4)
        {
            fixedFunctions = GameCommon.StringToListInt(parameters[3].value);
        }

        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        if(target!=0&& target != int.MinValue)
        {
            talkId = target;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct LoadMapCompleted : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartWorldInit : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {  
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SwitchScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct OpenChapter : GameAction
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct HideFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct StopFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string filmName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct PlayFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
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
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct PauseFilm : GameAction
{
    public string filmName;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }

        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct EndNowRoundFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryStartAutoExplore : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryStartAutoBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
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
    public void Clear() { this = default; }
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
    public void Clear() { this = default; }
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
    public void Clear() { this = default; }
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