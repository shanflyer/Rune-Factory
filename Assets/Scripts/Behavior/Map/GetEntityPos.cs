using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
[TaskCategory("Game/Map")]
[TaskName("获取个体位置")]
public class GetEntityPos : Action
{

    [Header("保存位置的共享变量")]
    public SharedVector3 targetPos;
    [Header("个体id")]
    public SharedInt entityId;

    public EntityType entityType;

    private bool getSuccess;
    // Start is called before the first frame update
    public override void OnStart()
    {
        if (entityId == null || entityId.IsNull())
        {
            getSuccess = false;
            return;
        }

        int3 coordinate = int3.zero;
        switch (entityType)
        {
            case EntityType.地图道具:
                getSuccess = WorldMapManager.instance.GetMapItemPos(entityId.Value, out coordinate);
                break;
            case EntityType.角色:
                getSuccess = CharacterManager.instance.GetCharacterCoordinate(entityId.Value, out coordinate);
                break;
        }
        targetPos.Value =GameCommon.GetMapPos(coordinate.x,coordinate.y);
    }

    public override TaskStatus OnUpdate()
    {
        return getSuccess ? TaskStatus.Success : TaskStatus.Failure;
    }
}
