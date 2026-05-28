using System;
using Unity.Mathematics;

[Serializable]
public struct CharacterMoveState
{
    public int3 Target { get; private set; }
    public MoveEndAction MoveEndAction { get; private set; }
    public MoveEndAction ChangeCoordinateAction { get; private set; }
    public Int3Action FailedMoveAction { get; private set; }
    public bool IsMoving { get; private set; }

    public void Begin(int3 target, MoveEndAction moveEndAction, MoveEndAction changeCoordinateAction,
        Int3Action failedMoveAction)
    {
        Target = target;
        MoveEndAction = moveEndAction;
        ChangeCoordinateAction = changeCoordinateAction;
        FailedMoveAction = failedMoveAction;
        IsMoving = true;
    }

    public void Clear()
    {
        Target = int3.zero;
        MoveEndAction = null;
        ChangeCoordinateAction = null;
        FailedMoveAction = null;
        IsMoving = false;
    }

    public void Complete()
    {
        var callback = MoveEndAction;
        Clear();
        callback?.Invoke();
    }

    public void Fail(int3 target)
    {
        var callback = FailedMoveAction;
        Clear();
        Target = new int3(-1, -1, -1);
        callback?.Invoke(target);
    }

    public void ChangeCoordinate()
    {
        ChangeCoordinateAction?.Invoke();
    }
}
