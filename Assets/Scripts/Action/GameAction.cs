using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using UnityEngine.Analytics;
using System.Security.Cryptography;

public interface GameAction 
{ 
    public  void Init(List<Parameter> parameters); 
}

public delegate void SetValue(int value);
public struct WaitAction 
{ 
    public void Init(List<Parameter> parameters)
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
                    GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters);
                });
                
            }
        }

       
    }
}
public struct ActionList : GameAction
{ 
    public void Init(List<Parameter> parameters)
    {
        for (int i = 0; i < parameters.Count; i++)
        {
            var Parameter = parameters[i];
            GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters);
        } 
    }
}
public struct HideFightScene : GameAction
{
    public void Init(List<Parameter> parameters)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DisplayFightScene : GameAction
{
    
    public void Init(List<Parameter> parameters)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct JumpFilm : GameAction
{
    public string filmName;
    public float jumpTime;

   
    public void Init(List<Parameter> parameters)
    {
        if (parameters.Count >= 2)
        {
            filmName = parameters[0].value;
            jumpTime = float.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct Talk : GameAction
{
    public int talkId, characterId;
    public void Init(List<Parameter> parameters)
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
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CreatTeamPlayer : GameAction
{
    public List<int> players;
    public void Init(List<Parameter> parameters)
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

    public void Init(List<Parameter> parameters)
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

    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct EnterChapter : GameAction
{
    public int id;
    public void Init(List<Parameter> parameters)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct RefreshFightCharactersInfo : GameAction
{
    public List<int> characters;
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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

    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters)
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
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
    public void Init(List<Parameter> parameters) 
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            mapId= int.Parse(parameters[1].value);
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
    public int mapId;
    public int2 coordinate;
    
}
public struct CharacterCoordinateTrigger : GameAction
{
    public void Init(List<Parameter> parameters) 
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            mapId = int.Parse(parameters[1].value);
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
    public int mapId;
    public int2 coordinate;
 
}