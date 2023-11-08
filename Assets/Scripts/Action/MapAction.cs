using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;

public struct TrySetTempMapItem : GameAction
{
    public int instanceId;
    public SetResult setResult { set; get; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
    {
        
        if (parameters.Count > 0)
        {
            instanceId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            instanceId = target;
        }
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CheckTempMapItemSet : GameAction
{
    public SetResult setResult { set; get; }
    public int instanceId;
}
public struct CreatTempMapItem : GameAction
{ 
    public int characterId;
    public int instanceId;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            instanceId = int.Parse(parameters[1].value);
        }
        if (source != 0)
        {
            characterId = source;
        }
        if (target != 0)
        {
            instanceId = target;
        }
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct DestoryTempMapItem : GameAction
{
    public SetResult setResult { set; get; }
    public int instanceId;
}
public struct RefreshTempMapItemCoordinate : GameAction
{
    public SetResult setResult { set; get; }
    public int instanceId;
    public int chatacterId;
}
public struct RefreshTempMapItem : GameAction
{
    public SetResult setResult { set; get; }
    public int instanceId;
}

 
public struct TryDeleteRoom : GameAction
{
    public int roomId;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
        {
            roomId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            roomId = source;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryCreatRoom : GameAction
{
    public SetResult setResult { set; get; }
    public int roomId;
    public string roomName;
    public int eventId;
    public int instance;
    public SetValue setValue;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
        {
            roomId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            roomId = source;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct CloseMapObjTips : GameAction
{
    public int id;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int id;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct ChangeWorld : GameAction
{
    public string worldName;
    public int displayMap;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count > 0)
        {
            worldName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            displayMap = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetItemAnimation : GameAction
{
    public SetResult setResult { get; set; }
    public int mapId;
    public int editorId;
    public int id;
    public int keyX;
    public int keyY;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count >= 3)
        {
            id = int.Parse(parameters[0].value);
            keyX = int.Parse(parameters[1].value);
            keyY = int.Parse(parameters[2].value);
        }
        if (source != 0)
            id = source;
        if (target != 0)
            keyX = target;
        if (value != 0)
            keyY = value;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TriggerEnter : GameAction
{
    public SetResult setResult { get; set; }
    public int eventId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { get; set; }
    public int eventId;
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
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

    public SetResult setResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count >= 3)
        {
            mapId = int.Parse(parameters[0].value);
            dataId = int.Parse(parameters[1].value);

            var parameter = parameters[2];
            if (parameter.parameters.Count >= 2)
            {
                coordinate.x = int.Parse(parameter.parameters[0].value);
                coordinate.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct MoveMapItem : GameAction
{
    public SetResult setResult { set; get; }
    public int mapItemInstanceId;
    public int mapInstance;
    public int2 coordinate;
}
public struct AttachMapItemData : GameAction
{
    public int mapItemIntanceId;

    public SetResult setResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult=null)
    {
        if (parameters.Count >= 1)
        {
            mapItemIntanceId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}