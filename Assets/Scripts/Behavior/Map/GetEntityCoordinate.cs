using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Map")]
[TaskName("获取个体坐标")]
public class GetEntityCoordinate : Action
{ 

    [Header("保存坐标的共享变量")]
    public SharedInt3 targetCoordinate;
    [Header("个体id")]
    public SharedInt entityId;

    public EntityType entityType;

    private bool getSuccess;
    // Start is called before the first frame update
    public override void OnStart()
    {
        if (entityId==null|| entityId.IsNull())
        {
            getSuccess = false;
            return;
        }

        int3 coordinate=int3.zero;
        switch (entityType)
        {
            case EntityType.地图道具:
                getSuccess = WorldMapManager.instance.GetMapItemPos(entityId.Value, out coordinate);
                break;
            case EntityType.角色:
                getSuccess = CharacterManager.instance.GetCharacterCoordiante(entityId.Value, out coordinate);
                break;
        }
        targetCoordinate.Value = coordinate;
    }

    public override TaskStatus OnUpdate()
    {
        return getSuccess? TaskStatus.Success:TaskStatus.Failure;
    }
}
