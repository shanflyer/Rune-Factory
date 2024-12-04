using System.Collections.Generic;

public struct ResetGameEvent : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int eventId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            eventId = int.Parse(parameters[0].value);
        }
        if (source > 0)
        {
            eventId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RemoveGameEvent : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int eventId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            eventId = int.Parse(parameters[0].value);
        }
        if (source > 0)
        {
            eventId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SampleGameEvent : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int eventId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            eventId = int.Parse(parameters[0].value);
        }
        if (source > 0)
        {
            eventId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}