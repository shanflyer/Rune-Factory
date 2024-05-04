using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        if (roomMapDatas.TryGetValue(id, out var result))
        {
            return result;
        }
        return null;
    }

    private Dictionary<int2, int> editorItemRemapInstanceIds = new Dictionary<int2, int>();

    private MyInstance mapItemInstance;
    private MyInstance mapRoomInstance;

    public int GetInstanceFromItem()
    {
        return mapItemInstance.CreatInstanceId();
    }

    public override void Init()
    {
        base.Init();
        runtimeMapItems.Init(128);

        mapItemInstance = new MyInstance();

        GameActionManager.instance.AddListener<RemoveMapItemCollider>(RemoveMapItemCollider);
        GameActionManager.instance.AddListener<ReSetMapItemCollider>(ReSetMapItemCollider);
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
        GameActionManager.instance.AddListener<InitMapLink>(InitMapLink);
        GameActionManager.instance.AddListener<SetMapItemLinkCharacter>(SetMapItemLinkCharacter);
        GameActionManager.instance.AddListener<SetMapEditorItemLinkCharacter>(SetMapEditorItemLinkCharacter);
        GameActionManager.instance.AddListener<DeleteMapLink>(DeleteMapLink);
    }

    
    private void SetMapEditorItemLinkCharacter(SetMapEditorItemLinkCharacter SetMapEditorItemLinkCharacter)
    {
        if (editorItemRemapInstanceIds.TryGetValue(new int2(SetMapEditorItemLinkCharacter.mapId, SetMapEditorItemLinkCharacter.mapItemEditorId),
               out var instanceId))
        {
            if (GetRuntimeMapItem(instanceId, out var runtimeMapItem))
            {
                runtimeMapItem.linkCharacter = SetMapEditorItemLinkCharacter.linkInstanceId;
                runtimeMapItems.SetData(runtimeMapItem);
            }
        }
    }

    public bool IsCheckRuntimeMapItemLink(int2 editorKey, int linkCharacterId = -1)
    {
        if (editorItemRemapInstanceIds.TryGetValue(editorKey,
              out var instanceId))
        {
            if (GetRuntimeMapItem(instanceId, out var runtimeMapItem))
            {
                if (linkCharacterId == -1 && runtimeMapItem.linkCharacter != 0)
                {
                    return true;
                }
                if (linkCharacterId != -1 && runtimeMapItem.linkCharacter == linkCharacterId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsCheckRuntimeMapItemLink(int instanceId, int linkCharacterId = -1)
    {
        if (GetRuntimeMapItem(instanceId, out var runtimeMapItem))
        {
            if (linkCharacterId == -1 && runtimeMapItem.linkCharacter != 0)
            {
                return true;
            }
            if (linkCharacterId != -1 && runtimeMapItem.linkCharacter == linkCharacterId)
            {
                return true;
            }
        }
        return false;
    }

    private void SetMapItemLinkCharacter(SetMapItemLinkCharacter SetMapItemLinkCharacter)
    {
        if (GetRuntimeMapItem(SetMapItemLinkCharacter.mapItemInstanceId, out var runtimeMapItem))
        {
            runtimeMapItem.linkCharacter = SetMapItemLinkCharacter.linkInstanceId;
            runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    private void AddMapItemOperate(AddMapItemOperate AddMapItemOperate)
    {
        if (GetRuntimeMapItem(AddMapItemOperate.mapItemId, out var runtimeMapItem))
        {
            runtimeMapItem.operateDatas.Add(AddMapItemOperate.addeOperateId);
            runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    private void RemoveMapItemOperate(RemoveMapItemOperate removeMapItemOperate)
    {
        if (GetRuntimeMapItem(removeMapItemOperate.mapItemId, out var runtimeMapItem))
        {
            runtimeMapItem.operateDatas.Remove(removeMapItemOperate.removeOperateId);
            runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    protected override void Clear()
    {
        base.Clear();
        runtimeMapItems.Dispose();
    }

    public List<int> GetMapItems(int mapId)
    {
        if (!itemInMapDatas.TryGetValue(mapId, out var result))
        {
            result = new List<int>();
        }
        return result;
    }

    public int2 GetNearestItemCell(int itemInstanceId, int2 cell, CellType cellType)
    {
        if (runtimeMapItems.GetData(itemInstanceId, out var runtimeMapItem))
        {
            switch (cellType)
            {
                case CellType.CommonTrigger:
                    return MapCellController.instance.GetNearestItemCommonTriggerCell(runtimeMapItem.mapInstanceId, itemInstanceId, cell);

                case CellType.PlayerTrigger:
                    return MapCellController.instance.GetNearestItemPlayerTriggerCell(runtimeMapItem.mapInstanceId, itemInstanceId, cell);
            }
        }
        return cell;
    }

    private async void TryCreatRoom(TryCreatRoom creatRoom)
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

    private async void ChangeWorld(ChangeWorld changeWorld)
    {
        await InitWorldData(changeWorld.worldName, changeWorld.displayMap);
    }

    private void SetItemAnimation(SetItemAnimation setItemAnimation)
    {
        int instanceid = setItemAnimation.id;

        if (setItemAnimation.mapId != 0)
        {
            if (editorItemRemapInstanceIds.TryGetValue(new int2(setItemAnimation.mapId, setItemAnimation.editorId),
                out instanceid))
            {
            }
        }
        if (runtimeMapItems.GetData(instanceid, out RuntimeMapItem runtimeMapItem))
        {
            int2 oldKey = runtimeMapItem.animationKey;
             
            runtimeMapItem.animationKey = new int2(setItemAnimation.keyX!=int.MinValue? setItemAnimation.keyX:oldKey.x,
                setItemAnimation.keyY!=int.MinValue?setItemAnimation.keyY:oldKey.y);

            WorldMapObjManager.instance.SetItemAnimation(runtimeMapItem);
            if (setItemAnimation.setResult != null)
            {
                setItemAnimation.setResult(true);
            }
            runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    public int GetInstanceFromEditorId(int2 editorKey)
    {
        if (editorItemRemapInstanceIds.TryGetValue(editorKey, out var instanceid))
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
        if (editorItemRemapInstanceIds.TryGetValue(new int2(mapId, editorInstanceId), out var instance))
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
            objCoordinate = new int3(mapItem.coordinate, mapItem.mapInstanceId);
            return true;
        }
        return false;
    }

    public bool GetMapItemPos(int2 key, out int3 objCoordinate)
    {
        objCoordinate = int3.zero;
        if (editorItemRemapInstanceIds.TryGetValue(key, out var instance))
        {
            if (runtimeMapItems.GetData(instance, out var mapItem))
            {
                objCoordinate = new int3(mapItem.coordinate, mapItem.mapInstanceId);
                return true;
            }
        }

        return false;
    }

    private async void RemoveMapItemCollider(RemoveMapItemCollider removeMapItemCollider)
    {
        if (runtimeMapItems.GetData(removeMapItemCollider.mapItemInstanceId, out var runtimeMapItem))
        {
            var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeMapItem.dataId);
            MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
        }
    }

    private async void ReSetMapItemCollider(ReSetMapItemCollider reSetMapItemCollider)
    {
        if (runtimeMapItems.GetData(reSetMapItemCollider.mapItemInstanceId, out var runtimeMapItem))
        {
            var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeMapItem.dataId);
            MapCellController.instance.AddBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
        }
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
                    MapCellController.instance.RemoveTriggerCell(mapItemData.triggerCells, runtimeMapItem.mapInstanceId, deleteMapItem.mapItemInstanceId);
                    MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
                }

                TryDeleteFishPond tryDeleteFishPond = new TryDeleteFishPond
                {
                    instanceId = deleteMapItem.mapItemInstanceId
                };
                GameActionManager.instance.QueueAction(tryDeleteFishPond);
            }
        }
    }

    private async Task<int> AddMapItem(MapItem mapItem, int mapId)
    {
        int instanceId = mapItemInstance.CreatInstanceId();
        if (mapItem.instanceId != 0)
        {
            editorItemRemapInstanceIds.Add(new int2(mapId, mapItem.instanceId), instanceId);
        }

        RuntimeMapItem runtimeMapItem = new RuntimeMapItem
        {
            coordinate = mapItem.coordinate,
            dataId = mapItem.id,
            instanceId = instanceId,
            editorInstanceId = mapItem.instanceId,
            animationKey = mapItem.animationKey,
            mapInstanceId = mapId,
            operateDatas = new NativeHashSet<int>(8, Allocator.Persistent),
            EventReferenceData=new NativeHashMap<FixedString128Bytes, int>(2,Allocator.Persistent),
        };
        runtimeMapItems.AddData(runtimeMapItem);
        if (!itemInMapDatas.TryGetValue(mapId, out List<int> items))
        {
            items = new List<int>();
            itemInMapDatas.Add(mapId, items);
        }
        items.Add(instanceId);

        if (mapItem.blindHomeEquipment != 0)
        {
            CreatHomeEquip creatHomeEquip = new CreatHomeEquip
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                equipDataId = mapItem.blindHomeEquipment,
                instanceId = instanceId,
                itemDataId = 0,
            };
            GameActionManager.instance.QueueAction(creatHomeEquip, true);

            SetHomeEquipCoordinate setHomeEquipCoordinate = new SetHomeEquipCoordinate
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                equipInstanceId = instanceId,
                mapInstanceId = mapId,
                coordinate = mapItem.coordinate,
            };
            GameActionManager.instance.QueueAction(setHomeEquipCoordinate);
        }

        //尝试创建柜台
        TryCreatStoreCounter tryCreatStoreCounter = new TryCreatStoreCounter
        {
            itemDataId = mapItem.id,
            itemInstanceId = instanceId
        };
        GameActionManager.instance.QueueAction(tryCreatStoreCounter, true);

        var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(mapItem.id);

        
        if (mapItemData.operateIds != null)
        {
            for (int i = 0; i < mapItemData.operateIds.Count; i++)
            {
                runtimeMapItem.operateDatas.Add(mapItemData.operateIds[i]);
            }
        }
        if (mapItem.eventReferenceDatas != null)
        {
            for (int i = 0; i < mapItem.eventReferenceDatas.Count; i++)
            {
                var data = mapItem.eventReferenceDatas[i];
                runtimeMapItem.EventReferenceData.Add(data.name, data.value);
            }
        }
        if (mapItemData.creatAction != null)
        {
            mapItemData.creatAction.Action(instanceId);
        }



        if (mapItemData.triggerCells.Length > 0)
        {
            MapCellController.instance.AddTriggerCell(mapItemData.triggerCells, mapId, mapItemData.defaultEnter,
                mapItemData.defaultExit, EntityType.角色, instanceId, mapItem.coordinate);
        }
        if (mapItemData.playerTriggerCells != null && mapItemData.playerTriggerCells.Length > 0)
        {
            MapCellController.instance.AddPlayerTriggerCell(mapItemData.playerTriggerCells, mapId, mapItemData.playerTriggerEvent,
                instanceId, mapItem.coordinate);
        }
        if (mapItemData.colliderCells.Length > 0)
        {
            MapCellController.instance.AddBarrierCell(mapItemData.colliderCells, mapItem.coordinate, mapId);
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

        TryCreatFishPond tryCreatFishPond = new TryCreatFishPond
        {
            room = mapId,
            itemId = mapItem.instanceId,
            instanceId = instanceId
        };
        GameActionManager.instance.QueueAction(tryCreatFishPond);
        return runtimeMapItem.instanceId;
    }

    private async void AddMapItem(AddMapItem addMapItem)
    {
        if (!MapCellController.instance.ContainsRoom(addMapItem.mapId))
        {
            return;
        }
        MapItem mapItem = new MapItem
        {
            id = addMapItem.dataId,
            coordinate = addMapItem.coordinate,
            instanceId = addMapItem.instanceId
        };

        int instanceId = await AddMapItem(mapItem, addMapItem.mapId);

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
        else //if(moveMapItem.noneTryAdd)
        {
            AddMapItem addMapItem = new AddMapItem
            {
                dataId = moveMapItem.dataId,
                mapId = moveMapItem.mapInstance,
                coordinate = moveMapItem.coordinate,
                instanceId = moveMapItem.mapItemInstanceId
            };
            AddMapItem(addMapItem);
        }
        SetHomeEquipCoordinate setHomeEquipCoordinate = new SetHomeEquipCoordinate
        {
            characterId = CharacterManager.instance.controllerCharacter.instanceId,
            equipInstanceId = moveMapItem.mapItemInstanceId
        };
        GameActionManager.instance.QueueAction(setHomeEquipCoordinate);
    }

    public bool InitNewSmoothMove(ref float2 direction, int2 coordinate, int mapId, out int2 targetCoordinate)
    {
        if (direction.x == 0 && direction.y == 0)
        {
            targetCoordinate = coordinate;
            return false;
        }

        int2 offsetCoordinate = (int2)direction;

        int2 checkTargetCoordinate = coordinate + offsetCoordinate;
        targetCoordinate = checkTargetCoordinate;
        if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
        {
            offsetCoordinate = new int2(0, (int)direction.y);
            checkTargetCoordinate = coordinate + offsetCoordinate;
            targetCoordinate = checkTargetCoordinate;
            if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
            {
                offsetCoordinate = new int2((int)direction.x, 0);
                checkTargetCoordinate = coordinate + offsetCoordinate;
                if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
                {
                    targetCoordinate = checkTargetCoordinate;
                    return false;
                }
                direction = new float2(direction.x, 0);
                return true;
            }
            direction = new float2(0, direction.y);
            return true;
        }
        return true;
    }

    public bool InitSmoothMove(ref Vector2 direction, Vector2 nowPos, int mapId, float distance)
    {
        if (direction == Vector2.zero)
        {
            return false;
        }

        Vector2 checkTargetPos = nowPos + direction * distance;
        int2 checkTargetCoordinate = GameCommon.GetMapCoordinateInt(checkTargetPos);
        if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
        {
            Vector2 direction1 = new Vector2(0, direction.y);
            Vector2 checkTargetPos1 = nowPos + direction1 * distance;
            int2 checkTargetCoordinate1 = GameCommon.GetMapCoordinateInt(checkTargetPos1);

            if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate1, mapId))
            {
                Vector2 direction2 = new Vector2(direction.x, 0);
                Vector2 checkTargetPos2 = nowPos + direction2 * distance;
                int2 checkTargetCoordinate2 = GameCommon.GetMapCoordinateInt(checkTargetPos2);

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

    public bool InitSmoothMove(ref Vector2 direction, Vector2 nowPos, int mapId, float distance, ref int2 target, ref Vector2 targetPos)
    {
        if (direction == Vector2.zero)
        {
            return false;
        }

        Vector2 checkTargetPos = nowPos + direction * distance;
        int2 checkTargetCoordinate = GameCommon.GetMapCoordinateInt(checkTargetPos);
        if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
        {
            Vector2 direction1 = new Vector2(0, direction.y);
            Vector2 checkTargetPos1 = nowPos + direction1 * distance;
            int2 checkTargetCoordinate1 = GameCommon.GetMapCoordinateInt(checkTargetPos1);

            if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate1, mapId))
            {
                Vector2 direction2 = new Vector2(direction.x, 0);
                Vector2 checkTargetPos2 = nowPos + direction2 * distance;
                int2 checkTargetCoordinate2 = GameCommon.GetMapCoordinateInt(checkTargetPos2);

                if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate2, mapId))
                {
                    return false;
                }
                direction = direction2;
                target = checkTargetCoordinate2;
                targetPos = checkTargetPos2;
                return true;
            }
            target = checkTargetCoordinate1;
            targetPos = checkTargetPos1;
            direction = direction1;
            return true;
        }

        target = checkTargetCoordinate;
        targetPos = checkTargetPos;
        return true;
    }

    private void DeleteMapLink(DeleteMapLink DeleteMapLink)
    {
        int index = worldMapData.mapLines.FindIndex(m => m.instanceId == DeleteMapLink.linkInstanceId);
        if (index >= 0)
        {
            MapCellController.instance.DeleteMapLink(worldMapData.mapLines[index]);
        }
    }
    private void InitMapLink(InitMapLink initMapLink)
    {
        int index = worldMapData.mapLines.FindIndex(m => m.instanceId == initMapLink.linkInstanceId);
        if (index >= 0 && !worldMapData.mapLines[index].zeroInit)
        {
            MapCellController.instance.InitLinkMap(worldMapData.mapLines[index]);
        }
    }

    private WorldMapData worldMapData;

    //初始化世界数据
    private async Task InitWorldData(string worldName, int displayMap = 0)
    {
        worldMapData = await GameDataManager.instance.GetAsyncData<WorldMapData>(worldName);
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
    public int linkCharacter;
    public NativeHashSet<int> operateDatas; 
    public NativeHashMap<FixedString128Bytes, int> EventReferenceData;
    public int Key => instanceId;

    public void Dispose()
    {
        operateDatas.Dispose();
        EventReferenceData.Dispose();
    }
}