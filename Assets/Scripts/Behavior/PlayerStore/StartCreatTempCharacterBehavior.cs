using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Character")]
[TaskName("开始创建temp角色")] 
public class StartCreatTempCharacterBehavior : Action
{
    public SharedInt mapId;
    public override void OnStart()
    {
        var displayMapRoomData = WorldMapManager.instance.GetWorldMap(mapId.Value).mapRoomData;
        StartCreatTempCharacter startCreatTempCharacter = new StartCreatTempCharacter
        {
            creatDataId = displayMapRoomData.creatTempCharacterId,
            clearAll = false,
        };
        GameActionManager.instance.QueueAction(startCreatTempCharacter);
    }
  
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}