// container - WaitAction (with delay)
using System;
using UnityEngine;

public class WaitActionNode : ContainerNode
{
    public int delay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new WaitAction { setValue = setValue, setResult = setResult };
        GameActionManager.instance.QueueAction(action, immediately);

        if (children != null)
        {
            foreach (var entry in children)
            {
                if (entry?.node != null)
                {
                    var child = entry;
                    GameTimerController.instance.DelayAction(delay, () =>
                    {
                        child.node.CreateAction(source, target, value, setResult, setValue, immediately);
                    });
                }
            }
        }

        return action;
    }
}
