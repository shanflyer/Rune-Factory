// SetFixedCamera
using System;
using UnityEngine;

public class SetFixedCameraNode : ActionNode
{
        public bool fixedCamera;
        public Vector3 fixedPos;
        public Vector3 offsetPos;
        public int pixelValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetFixedCamera
        {
                fixedCamera = this.fixedCamera,
                fixedPos = this.fixedPos,
                offsetPos = this.offsetPos,
                pixelValue = this.pixelValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}