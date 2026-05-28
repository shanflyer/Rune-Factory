using System;
using Unity.Mathematics;

[Serializable]
public struct CharacterDebugSnapshot
{
    public int instanceId;
    public int dataId;
    public string name;
    public bool isController;
    public bool isInTeam;
    public bool canMove;
    public int level;
    public int3 coordinate;
    public int3 moveTarget;
    public string behaviorName;
    public string npcTaskName;
    public NPCBehaviorState npcBehaviorState;
    public CharacterProperty property;

    public override string ToString()
    {
        return $"CharacterDebugSnapshot(instanceId={instanceId}, dataId={dataId}, name={name}, map={coordinate.z}, coordinate={coordinate.xy}, moveTarget={moveTarget}, behavior={behaviorName}, task={npcTaskName}, state={npcBehaviorState})";
    }
}
