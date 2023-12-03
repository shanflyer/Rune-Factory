using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ClosePanelAction : GameAction
{
    public Type type;
    public SetResult setResult { set; get; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
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
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
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
public struct ShowPanel : GameAction
{
    public bool show;
    public SetResult setResult { get; set; } 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
    {
        if (parameters.Count >= 1)
        {
            show = bool.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this);
    }

}