public partial class Character
{
    public CharacterDebugSnapshot GetDebugSnapshot()
    {
        string behaviorName = null;
        string taskName = null;
        var behaviorState = NPCBehaviorState.NULL;

        if (!SingletonType.Cleared)
        {
            var behaviorManager = CharacterBehaviorManager.instance;
            behaviorName = behaviorManager != null ? behaviorManager.GetCharacterBehaviorName(instanceId) : null;

            var scheduleManager = NPCTaskScheduleManager.instance;
            if (scheduleManager != null)
            {
                taskName = scheduleManager.NowTaskName(instanceId);
                behaviorState = scheduleManager.GetNPCBehaviorState(instanceId);
            }
        }

        return new CharacterDebugSnapshot
        {
            instanceId = instanceId,
            dataId = dataId,
            name = name,
            isController = isController,
            isInTeam = isInTeam,
            canMove = canMove,
            level = Level,
            coordinate = ObjCoordinate,
            moveTarget = moveTarget,
            behaviorName = behaviorName,
            npcTaskName = taskName,
            npcBehaviorState = behaviorState,
            property = CharacterProperty
        };
    }
}
