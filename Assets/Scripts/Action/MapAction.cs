using System;
using System.Collections.Generic;
using Unity.Mathematics;

public struct ZeroWorld : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TrySetTempMapItem : GameAction
{
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CheckTempMapItemSet : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceId;
}

public struct CreatTempMapItem : GameAction
{
    public int characterId;
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DestoryTempMapItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceId;
}

public struct RefreshTempMapItemCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceId;
    public int chatacterId;
}

public struct RefreshTempMapItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceId;
}

public struct TryDeleteRoom : GameAction
{
    public int roomId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            roomId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            roomId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatRoom : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int roomId;
    public string roomName;
    public int eventId;
    public int instance; 

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            roomId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            roomId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CloseMapObjTips : GameAction
{
    public int id;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct AddMapItemOperate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int mapItemId;
    public int addeOperateId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            mapItemId = int.Parse(parameters[0].value);
            if (parameters.Count > 1)
            {
                addeOperateId = int.Parse(parameters[1].value);
            }
        }
        else
        {
            mapItemId = source;
            addeOperateId = target;
        }
        if (setResult != null)
        {
            this.setResult = setResult;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RemoveMapItemOperate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int mapItemId;
    public int removeOperateId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            mapItemId = int.Parse(parameters[0].value);
            if (parameters.Count > 1)
            {
                removeOperateId = int.Parse(parameters[1].value);
            }
        }
        else
        {
            mapItemId = source;
            removeOperateId= target;
        }
        if(setResult != null)
        {
            this.setResult = setResult;
        }
        
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct ShowMapObjTips : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            id = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayMap : GameAction
{
    public int displayMap;
    public int actionId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            displayMap = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            actionId = int.Parse(parameters[1].value);
        }
        if (source != 0)
        {
            displayMap = source;
        }
        if (target != 0)
        {
            actionId = target;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChangeWorld : GameAction
{
    public string worldName;
    public int displayMap;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            worldName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            displayMap = int.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetItemAnimation : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int mapId;
    public int editorId;
    public int id;
    public int keyX;
    public int keyY;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TriggerEnter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int eventId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            eventId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TriggerExit : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int eventId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            eventId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DeleteMapItem : GameAction
{
    //public int mapId;
    public int mapItemInstanceId;

    public bool triggerClear;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 2)
        {
            mapItemInstanceId = int.Parse(parameters[0].value);
            triggerClear = bool.Parse(parameters[1].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChangeMapItem : GameAction
{
    public int itemId;
    public int newDataId;
    public int2 animationKey;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct AddMapItem : GameAction
{
    public int mapId;
    public int dataId;
    public int2 coordinate;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct MoveMapItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int mapItemInstanceId;
    public int mapInstance;
    public int2 coordinate;
}

public struct AttachMapItemData : GameAction
{
    public int mapItemIntanceId;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            mapItemIntanceId = int.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct GetMapItemPos : GameAction
{
    public int mapItemIntanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            mapItemIntanceId = int.Parse(parameters[0].value);
        }
        if (setResult!=null)
        {
            this.setResult = setResult;
        }
        if (setValue != null)
        {
            this.setValue = setValue;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}