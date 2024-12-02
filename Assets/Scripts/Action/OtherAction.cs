using System.Collections.Generic;

public struct GameGuideAction : GameAction
{
    public int guidKey;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            guidKey = int.Parse(parameters[0].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            guidKey = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}