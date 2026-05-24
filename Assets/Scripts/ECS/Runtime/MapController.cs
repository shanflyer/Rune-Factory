
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public class MapController:Singleton<MapCellController>
{
    EntityManager entityManager;
    NativeParallelHashMap<int3, MapLink> linkMapDatas;
    public override void Init()
    {
        base.Init();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    private void TryCreateRoom(WorldMap worldMap)
    {
        var entity = entityManager.CreateEntity();
        entityManager.AddComponent<MapComponent>(entity);
        entityManager.SetComponentData(entity, new MapComponent
        {
            id = worldMap.id,
            coordinate=worldMap.coordinate

        });
        entityManager.AddBuffer<MapLink>(entity);



        /*
        if (creatRoom.eventId != 0)
        {
            await GameEventManager.instance.AddGameEvent(creatRoom.eventId);
        }
        if (creatRoom.setValue != null)
            creatRoom.setValue(instanceId);*/
    }
}
