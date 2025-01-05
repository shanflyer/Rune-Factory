using System.Collections.Generic;

public struct SaveGuideFilmIndexAction : GameAction
{
    public int id;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            id= int.Parse(parameters[0].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            id = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CheckGuideFilmIndex : GameAction
{
    public int id;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            id = int.Parse(parameters[0].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            id = source;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct GameGuideAction : GameAction
{
    public int guidKey;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
public struct CheckGameGuideAction : GameAction
{
    public int guidKey;
    public bool isEnd;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            guidKey = int.Parse(parameters[0].value);
            if (parameters.Count > 1)
                isEnd = bool.Parse(parameters[0].value);
        }
        else 
        {
            if (source != 0 && source != int.MinValue)
            {
                guidKey = source;
            }
            isEnd = target != 0;
        }
        this.setResult = setResult;
        this.setValue = setValue;
        
        GameActionManager.instance.QueueAction(this, immediately);
    }
}