 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics; 
using UnityEngine;

public class WorldMapManager : Singleton<WorldMapManager>
{ 
    private MyNativeData<RuntimeMapItem> runtimeMapItems;

    private Dictionary<int, List<int>> itemInMapDatas = new Dictionary<int, List<int>>();

    //每个地图对应的地图数据
    private Dictionary<int, string> roomMapDatas = new Dictionary<int, string>(); 

    public string GerMapDataName(int id)
    {
        if(roomMapDatas.TryGetValue(id,out var result))
        {
            return result;
        }
        return null;
    }

    private Dictionary<int2, int> editorItemRemapInstanceIds = new Dictionary<int2, int>();
    
    private MyInstance mapItemInstance;
    private MyInstance mapRoomInstance;
    public override void Init()
    {
        base.Init();
        runtimeMapItems.Init(128);

        mapItemInstance = new MyInstance();

        GameActionManager.instance.AddListener<AddMapItem>(AddMapItem);
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteMapItem);
        GameActionManager.instance.AddListener<ChangeMapItem>(ChangeMapItem);
        GameActionManager.instance.AddListener<SetItemAnimation>(SetItemAnimation);
        GameActionManager.instance.AddListener<ChangeWorld>(ChangeWorld);
        GameActionManager.instance.AddListener<TryCreatRoom>(TryCreatRoom);
        GameActionManager.instance.AddListener<TryDeleteRoom>(TryDeleteRoom);
        GameActionManager.instance.AddListener<MoveMapItem>(MoveMapItem);
        GameActionManager.instance.AddListener<RemoveMapItemOperate>(RemoveMapItemOperate);
        GameActionManager.instance.AddListener<AddMapItemOperate>(AddMapItemOperate);
    }
    void AddMapItemOperate(AddMapItemOperate AddMapItemOperate)
    {

        if (GetRuntimeMapItem(AddMapItemOperate.mapItemId, out var runtimeMapItem))
        {
            runtimeMapItem.operateDatas.Add(AddMapItemOperate.addeOperateId);
        }
    }
    void RemoveMapItemOperate(RemoveMapItemOperate removeMapItemOperate)
    {
        if(GetRuntimeMapItem(removeMapItemOperate.mapItemId,out var runtimeMapItem))
        {
            runtimeMapItem.operateDatas.Remove(removeMapItemOperate.removeOperateId);
        }
    }
    protected override void Clear()
    {
        base.Clear(); 
        runtimeMapItems.Dispose();
    }

    public List<int> GetMapItems(int mapId)
    {
        List<int> result = new List<int>();
        itemInMapDatas.TryGetValue(mapId, out result);
        return result;
    }
   
    async void TryCreatRoom(TryCreatRoom creatRoom)
    {
        int instanceId = creatRoom.instance;
        if (instanceId == 0)
        {
             instanceId = mapRoomInstance.CreatInstanceId();
        }
        
        var MapRoomData = await GameDataManager.instance.GetAsyncData<MapRoomData>(creatRoom.roomId); 
        roomMapDatas.Add(instanceId, MapRoomData.roomName); 
        //创建地图房间
        MapCellController.instance.InitMapData(instanceId, MapRoomData.mapCells.ToArray(),
            MapRoomData.startCoordinate, MapRoomData.endCoordinate, int3.zero);

        foreach (var data in MapRoomData.mapItems)
        {
            await AddMapItem(data, instanceId);
        }

        if (creatRoom.eventId != 0)
        {
          await GameEventManager.instance.AddGameEvent(creatRoom.eventId);
        }
        if (creatRoom.setValue != null)
            creatRoom.setValue(instanceId);
    }
    private void TryDeleteRoom(TryDeleteRoom deleteRoom)
    {
        int roomInstanceid = deleteRoom.roomId;
        if (itemInMapDatas.TryGetValue(roomInstanceid, out List<int> mapItems))
        {
            for (int i = 0; i < mapItems.Count; i++)
            {
                int mapItemInstanceId = mapItems[i];
                runtimeMapItems.RemoveData(mapItemInstanceId);
            }

            itemInMapDatas.Remove(roomInstanceid);
            if (deleteRoom.setResult != null)
                deleteRoom.setResult(true);

            return;
        }
        if (deleteRoom.setResult != null)
            deleteRoom.setResult(false);
    }
    async void ChangeWorld(ChangeWorld changeWorld)
    {
       
       await InitWorldData(changeWorld.worldName,changeWorld.displayMap); 
    }
    private void SetItemAnimation(SetItemAnimation setItemAnimation)
    {
        int instanceid = setItemAnimation.id;
        
        if (setItemAnimation.mapId !=0)
        {
            if(editorItemRemapInstanceIds.TryGetValue(new int2(setItemAnimation.mapId,setItemAnimation.editorId),
                out instanceid))
            {

            }
        }
        if (runtimeMapItems.GetData(instanceid, out RuntimeMapItem runtimeMapItem))
        {
            runtimeMapItem.animationKey = new int2(setItemAnimation.keyX, setItemAnimation.keyY);

            WorldMapObjManager.instance.SetItemAnimation(runtimeMapItem); 
        }
    }
    public int GetInstanceFromEditorId(int2 editorKey)
    {
        if(editorItemRemapInstanceIds.TryGetValue(editorKey,out  var instanceid))
        {
            return instanceid;
        }
        return -1;
    }
  

    private async Task SetItemAimation(int2 key, int dataId, int instaceId)
    {
        var animationData = await GameDataManager.instance.GetAsyncData<ItemAnimationData>(dataId);
        AnimationClip animationClip = animationData.GetAnimationClip(key, out int count);
        MyAnimationController.instance.PlayAnimation(instaceId, animationClip);
    }

    public bool GetRuntimeMapItem(int instanceId, out RuntimeMapItem runtimeMapItem)
    {
        return runtimeMapItems.GetData(instanceId, out runtimeMapItem);
    } 
    public bool GetMapItemPos(int mapId, int editorInstanceId, out int3 objCoordinate)
    {
        objCoordinate = int3.zero;
        if(editorItemRemapInstanceIds.TryGetValue(new int2(mapId,editorInstanceId),out var instance))
        {
            if (runtimeMapItems.GetData(instance, out var mapItem))
            {
                objCoordinate = new int3(mapItem.coordinate, mapItem.mapInstanceId);
                return true;
            }
        } 
        return false;
    }
    public bool GetMapItemPos(int id, out int3 objCoordinate)
    {
        objCoordinate = int3.zero;
        if (runtimeMapItems.GetData(id, out var mapItem))
        {
            objCoordinate=new int3(mapItem.coordinate, mapItem.mapInstanceId);
            return true;
        }
        return false;
    } 
    private async void DeleteMapItem(DeleteMapItem deleteMapItem)
    {
        //Vector2Int key = new Vector2Int(deleteMapItem.mapId, deleteMapItem.mapItemInstanceId);

        if (runtimeMapItems.GetData(deleteMapItem.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItems.RemoveData(deleteMapItem.mapItemInstanceId))
            {
                itemInMapDatas[runtimeMapItem.mapInstanceId].Remove(deleteMapItem.mapItemInstanceId);

                if (deleteMapItem.triggerClear)
                {
                    var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeMapItem.dataId);

                    MapCellController.instance.RemovePlayerTriggerCell(mapItemData.playerTriggerCells, runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                    MapCellController.instance.RemoveTriggerCell(mapItemData.triggerCells,runtimeMapItem.mapInstanceId, deleteMapItem.mapItemInstanceId);
                    MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells,runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
                }
            }
        } 
    }

    private async Task AddMapItem(MapItem mapItem, int mapId)
    {
        int instanceId = mapItemInstance.CreatInstanceId();
        editorItemRemapInstanceIds.Add(new int2(mapId, mapItem.instanceId), instanceId);
        RuntimeMapItem runtimeMapItem = new RuntimeMapItem
        {
            coordinate = mapItem.coordinate,
            dataId = mapItem.id,
            instanceId = instanceId,
            editorInstanceId = mapItem.instanceId,
            animationKey = mapItem.animationKey,
            mapInstanceId = mapId,
            operateDatas=new NativeHashSet<int>(8,Allocator.Persistent)
        };
        runtimeMapItems.AddData(runtimeMapItem);
        if (!itemInMapDatas.TryGetValue(mapId, out List<int> items))
        {
            items = new List<int>();
            itemInMapDatas.Add(mapId, items);
        }
        items.Add(instanceId);
      
        //尝试创建柜台
        TryCreatStoreCounter tryCreatStoreCounter = new TryCreatStoreCounter
        {
            itemDataId = mapItem.id,
            itemInstanceId = instanceId
        };
        GameActionManager.instance.QueueAction(tryCreatStoreCounter,true);

        var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(mapItem.id);

        for(int i = 0; i < mapItemData.operateIds.Count; i++)
        {
            runtimeMapItem.operateDatas.Add(mapItemData.operateIds[i]);
        }

        if (mapItemData.triggerCells.Length > 0)
        {
            MapCellController.instance.AddTriggerCell(mapItemData.triggerCells, mapId, mapItemData.defaultEnter,
                mapItemData.defaultExit, EntityType.角色, instanceId, mapItem.coordinate);
        }
        if (mapItemData.playerTriggerCells!=null&&mapItemData.playerTriggerCells.Length > 0)
        {
            MapCellController.instance.AddPlayerTriggerCell(mapItemData.playerTriggerCells,mapId,mapItemData.playerTriggerEvent,
                instanceId,mapItem.coordinate); 
        }
        if (mapItemData.colliderCells.Length > 0)
        {
            MapCellController.instance.AddBarrierCell(mapItemData.colliderCells,mapItem.coordinate, mapId);
        }

        if (mapId == WorldMapObjManager.instance.displayMap)
        {
            WorldMapObjManager.instance.DisplayMapItem(runtimeMapItem);
        }

        

        TryCreatField tryCreatField = new TryCreatField
        {
            roomId = mapId,
            itemInstanceId = mapItem.instanceId,
        };
        GameActionManager.instance.QueueAction(tryCreatField, true);
    }

    private async void AddMapItem(AddMapItem addMapItem)
    { 
        if (!MapCellController.instance.ContainsRoom(addMapItem.mapId))
        {
            return;
        }

        int instanceId = mapItemInstance.CreatInstanceId();

        MapItem mapItem = new MapItem
        {
            id = addMapItem.dataId,
            coordinate = addMapItem.coordinate,
        };

       await AddMapItem(mapItem, addMapItem.mapId);
          
        if (addMapItem.setValue != null)
        {
            addMapItem.setValue(instanceId);
        }
    }

    private async void MoveMapItem(MoveMapItem moveMapItem)
    {
        if (runtimeMapItems.GetData(moveMapItem.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItem.mapInstanceId == moveMapItem.mapItemInstanceId &&
                runtimeMapItem.coordinate.Equals(moveMapItem.coordinate))
            {

            }
            else
            {
                var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeMapItem.dataId);

                MapCellController.instance.RemovePlayerTriggerCell(mapItemData.playerTriggerCells, runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                MapCellController.instance.RemoveTriggerCell(mapItemData.triggerCells, runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

                WorldMapObjManager.instance.DeleteMapItem(new DeleteMapItem
                {
                    mapItemInstanceId = runtimeMapItem.instanceId,
                });
                  
                //设置新位置
                runtimeMapItem.mapInstanceId = moveMapItem.mapInstance;
                runtimeMapItem.coordinate = moveMapItem.coordinate;

                if (runtimeMapItem.mapInstanceId != 0)
                {
                    if (mapItemData.triggerCells.Length > 0)
                    {
                        MapCellController.instance.AddTriggerCell(mapItemData.triggerCells, moveMapItem.mapInstance, mapItemData.defaultEnter,
                            mapItemData.defaultExit, EntityType.角色, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
                    }
                    if (mapItemData.playerTriggerCells != null && mapItemData.playerTriggerCells.Length > 0)
                    {
                        MapCellController.instance.AddPlayerTriggerCell(mapItemData.playerTriggerCells, runtimeMapItem.mapInstanceId, mapItemData.playerTriggerEvent,
                            runtimeMapItem.instanceId, moveMapItem.coordinate);
                    }
                    if (mapItemData.colliderCells.Length > 0)
                    {
                        MapCellController.instance.AddBarrierCell(mapItemData.colliderCells, moveMapItem.coordinate, moveMapItem.mapInstance);
                    }

                    if (moveMapItem.mapInstance == WorldMapObjManager.instance.displayMap)
                    {
                        WorldMapObjManager.instance.DisplayMapItem(runtimeMapItem);
                    }
                }

                runtimeMapItems.SetData(runtimeMapItem);
            }
        }
       
    }

    public bool InitSmoothMove(ref Vector2 direction, Vector2 nowPos, int mapId)
    {
        if (direction == Vector2.zero)
        {
            return false;
        }

        Vector2 checkTargetPos = nowPos + direction * GameCommon.cellSize;
        Vector2Int checkTargetCoordinate = GameCommon.GetMapCoordinate(checkTargetPos);
        if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
        {
            Vector2 direction1 = new Vector2(0, direction.y);
            Vector2 checkTargetPos1 = nowPos + direction1 * GameCommon.cellSize;
            Vector2Int checkTargetCoordinate1 = GameCommon.GetMapCoordinate(checkTargetPos1);

            if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate1, mapId))
            {
                Vector2 direction2 = new Vector2(direction.x, 0);
                Vector2 checkTargetPos2 = nowPos + direction2 * GameCommon.cellSize;
                Vector2Int checkTargetCoordinate2 = GameCommon.GetMapCoordinate(checkTargetPos2);

                if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate2, mapId))
                {
                    return false;
                }
                direction = direction2;
                return true;
            }
            direction = direction1;
            return true;
        }
        return true;
    }
     

    //初始化世界数据
    async Task InitWorldData(string worldName,int displayMap=0)
    {
        var worldMapData = await GameDataManager.instance.GetAsyncData<WorldMapData>(worldName);
        MapCellController.instance.InitWorldRoomDatas(worldMapData.worldMaps.Count);

        roomMapDatas.Clear();

        foreach (var room in worldMapData.worldMaps)
        {
            //获取房间数据
            var MapRoomData = await GameDataManager.instance.GetAsyncData<MapRoomData>(room.map);
            roomMapDatas.Add(room.id, room.map);



            //创建地图房间
            MapCellController.instance.InitMapData(room.id, MapRoomData.mapCells.ToArray(),
                MapRoomData.startCoordinate, MapRoomData.endCoordinate, room.coordinate);

            foreach (var data in MapRoomData.mapItems)
            {
                await AddMapItem(data, room.id);
            }

            if (room.eventId != 0)
            {
                GameEventManager.instance.AddGameEvent(room.eventId);
            }
        }
        //生成地图链接
        MapCellController.instance.InitLinkMap(worldMapData.mapLines);
        // return true;

        if (displayMap == 0)
        {
            displayMap = worldMapData.defaultMap;
        }
        WorldMapObjManager.instance.DefaultDisplayMap(displayMap);
    } 

    private async void ChangeMapItem(ChangeMapItem changeMapItem)
    {
        if (GetRuntimeMapItem(changeMapItem.itemId, out RuntimeMapItem runtimeMapItem))
        {
            if (changeMapItem.newDataId > 0 && runtimeMapItem.dataId != changeMapItem.newDataId)
            {
                runtimeMapItem.dataId = changeMapItem.newDataId;
                WorldMapObjManager.instance.ChangeMapItemDisplay(changeMapItem.itemId, changeMapItem.newDataId, changeMapItem.animationKey, runtimeMapItem);
            }
            else
            {
                runtimeMapItem.animationKey = changeMapItem.animationKey;

                SetItemAimation(changeMapItem.animationKey, runtimeMapItem.dataId, runtimeMapItem.instanceId);
            }
            runtimeMapItems.SetData(runtimeMapItem);
        }
    }
}

public struct RuntimeMapItem : INativeData
{
    public int instanceId;
    public int editorInstanceId;
    public int dataId;
    public int mapInstanceId;
    public int2 coordinate;
    public int2 animationKey;
    public NativeHashSet<int> operateDatas;
    public int Key => instanceId;
    public void Dispose()
    {
        operateDatas.Dispose();
    }
}