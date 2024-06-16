

using System.Collections.Generic;

public struct ManualSkillAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; } 

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    { 
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SkillPauseAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public bool pause;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            pause = bool.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct NoSelectSkillAction: GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; } 

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {

        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SelectSkillAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public SkillRuntime skillRuntime;
    public int ActionCharacter;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
         
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
