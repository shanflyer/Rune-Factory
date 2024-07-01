using ProFlares;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class WorldMapManager : Singleton<WorldMapManager>
{
    private Dictionary<int,RuntimeMapItem> runtimeMapItems;

    private Dictionary<int, List<int>> itemInMapDatas = new Dictionary<int, List<int>>();

    //每个地图对应的地图数据
   /* private Dictionary<int, string> roomMapDatas = new Dictionary<int, string>();

    public string GerMapDataName(int id)
    {
        if (roomMapDatas.TryGetValue(id, out var result))
        {
            return result;
        }
        return null;
    }*/

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
        runtimeMapItems=new Dictionary<int, RuntimeMapItem>();

        mapItemInstance = new MyInstance();
        GameDataSaveManager.instance.InitMapInstanceData(mapItemInstance);

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
        GameActionManager.instance.AddListener<TrySetMapItem>(TrySetMapItem);
        GameActionManager.instance.AddListener<RefreshManufature>(RefreshManufature);
    }

    private void SetMapEditorItemLinkCharacter(SetMapEditorItemLinkCharacter SetMapEditorItemLinkCharacter)
    {
        if (editorItemRemapInstanceIds.TryGetValue(new int2(SetMapEditorItemLinkCharacter.mapId, SetMapEditorItemLinkCharacter.mapItemEditorId),
               out var instanceId))
        {
            if (GetRuntimeMapItem(instanceId, out var runtimeMapItem))
            {
                runtimeMapItem.linkCharacter = SetMapEditorItemLinkCharacter.linkInstanceId;
               // runtimeMapItems.SetData(runtimeMapItem);
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
           // runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    private void AddMapItemOperate(AddMapItemOperate AddMapItemOperate)
    {
        if (GetRuntimeMapItem(AddMapItemOperate.mapItemId, out var runtimeMapItem))
        {
            runtimeMapItem.operateDatas.Add(AddMapItemOperate.addeOperateId);
            if (AddMapItemOperate.needSave)
            {
                GameDataSaveManager.instance.UserGameSaveData.AddMapItemOperate(new int3(runtimeMapItem.editorKey, AddMapItemOperate.addeOperateId),runtimeMapItem.instanceId);
            }
            // runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    private void RemoveMapItemOperate(RemoveMapItemOperate removeMapItemOperate)
    {
        if (GetRuntimeMapItem(removeMapItemOperate.mapItemId, out var runtimeMapItem))
        {
            runtimeMapItem.operateDatas.Remove(removeMapItemOperate.removeOperateId);
            if (removeMapItemOperate.needSave)
            {
                GameDataSaveManager.instance.UserGameSaveData.RemoveMapItemOperate(new int3(runtimeMapItem.editorKey, removeMapItemOperate.removeOperateId),runtimeMapItem.instanceId);
            }
           //runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    protected override void Clear()
    {
        base.Clear();
        foreach(var runtimeMapItem in runtimeMapItems)
        {
            runtimeMapItem.Value.Dispose();
        }
        runtimeMapItems.Clear();
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
        if (runtimeMapItems.TryGetValue(itemInstanceId, out var runtimeMapItem))
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
       // roomMapDatas.Add(instanceId, MapRoomData.roomName);
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
                runtimeMapItems.Remove(mapItemInstanceId);
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
    void RefreshManufature(RefreshManufature refreshManufature){

        SetItemAnimation setItemAnimation = default(SetItemAnimation);
        setItemAnimation.id = refreshManufature.manufature.instanceId;
        if (refreshManufature.manufature.product.x != 0 && refreshManufature.manufature.waitTime > GameTimeManager.instance.totalMinute)
        {
            setItemAnimation.keyX = 1;
        }
        SetItemAnimation(setItemAnimation);
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
        if (runtimeMapItems.TryGetValue(instanceid, out RuntimeMapItem runtimeMapItem))
        {
            runtimeMapItem.SetAnimationKey(new int2(setItemAnimation.keyX, setItemAnimation.keyY));

           // WorldMapObjManager.instance.SetItemAnimation(runtimeMapItem);
            if (setItemAnimation.setResult != null)
            {
                setItemAnimation.setResult(true);
            }
           // runtimeMapItems.SetData(runtimeMapItem);
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
       
    }

    public bool GetRuntimeMapItem(int instanceId, out RuntimeMapItem runtimeMapItem)
    {
        return runtimeMapItems.TryGetValue(instanceId, out runtimeMapItem);
    }

    public bool GetMapItemPos(int mapId, int editorInstanceId, out int3 objCoordinate)
    {
        objCoordinate = int3.zero;
        if (editorItemRemapInstanceIds.TryGetValue(new int2(mapId, editorInstanceId), out var instance))
        {
            if (runtimeMapItems.TryGetValue(instance, out var mapItem))
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
        if (runtimeMapItems.TryGetValue(id, out var mapItem))
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
            if (runtimeMapItems.TryGetValue(instance, out var mapItem))
            {
                objCoordinate = new int3(mapItem.coordinate, mapItem.mapInstanceId);
                return true;
            }
        }

        return false;
    }

    private void RemoveMapItemCollider(RemoveMapItemCollider removeMapItemCollider)
    {
        if (runtimeMapItems.TryGetValue(removeMapItemCollider.mapItemInstanceId, out var runtimeMapItem))
        { 
            var mapItemData = runtimeMapItem.mapItemData;
            MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

            GameDataSaveManager.instance.UserGameSaveData.AddRemoveMapItemColliderData(new int2(runtimeMapItem.mapInstanceId,runtimeMapItem.editorInstanceId),
                runtimeMapItem.instanceId);
        }
    }

    private void ReSetMapItemCollider(ReSetMapItemCollider reSetMapItemCollider)
    {
        if (runtimeMapItems.TryGetValue(reSetMapItemCollider.mapItemInstanceId, out var runtimeMapItem))
        { 
            var mapItemData=runtimeMapItem.mapItemData;
            MapCellController.instance.AddBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

            GameDataSaveManager.instance.UserGameSaveData.AddReSetMapItemColliderData(new int2(runtimeMapItem.mapInstanceId, runtimeMapItem.editorInstanceId),
                runtimeMapItem.instanceId);
        }
    }

    private void DeleteMapItem(DeleteMapItem deleteMapItem)
    {
        //Vector2Int key = new Vector2Int(deleteMapItem.mapId, deleteMapItem.mapItemInstanceId);

        if (runtimeMapItems.TryGetValue(deleteMapItem.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItems.Remove(deleteMapItem.mapItemInstanceId))
            {
                itemInMapDatas[runtimeMapItem.mapInstanceId].Remove(deleteMapItem.mapItemInstanceId);

                if (deleteMapItem.triggerClear)
                {
                    var mapItemData = runtimeMapItem.mapItemData;

                    MapCellController.instance.RemovePlayerTriggerCell(mapItemData.playerTriggerCells, runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                    MapCellController.instance.RemoveTriggerCell(mapItemData.triggerCells, runtimeMapItem.mapInstanceId, deleteMapItem.mapItemInstanceId);
                    MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
                }
            }
        }
    }

    private async Task<int> AddMapItem(MapItem mapItem, int mapId)
    {
        bool isInSaveData = true;
        int instanceId = GameDataSaveManager.instance.GetSaveMapInstance(new int2(mapId, mapItem.instanceId));
        if (instanceId == 0) 
        {
            isInSaveData = false;
            instanceId = mapItemInstance.CreatInstanceId();
        }
        
        if (mapItem.instanceId != 0)
        {
            editorItemRemapInstanceIds.Add(new int2(mapId, mapItem.instanceId), instanceId);
        }
        RuntimeMapItem runtimeMapItem = new RuntimeMapItem(instanceId, mapItem.instanceId, await GameDataManager.instance.GetAsyncData<MapItemData>(mapItem.id),
            mapId, mapItem.coordinate, mapItem.animationKey);
        
        runtimeMapItems.Add(instanceId,runtimeMapItem);
        if (!itemInMapDatas.TryGetValue(mapId, out List<int> items))
        {
            items = new List<int>();
            itemInMapDatas.Add(mapId, items);
        }
        items.Add(instanceId);

        if (!isInSaveData&&mapItem.blindHomeEquipment != 0)
        {
            CreatHomeEquip creatHomeEquip = new CreatHomeEquip
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                equipDataId = mapItem.blindHomeEquipment,
                instanceId = instanceId,
                setResult = (bool value) =>
                {
                    SetHomeEquipCoordinate setHomeEquipCoordinate = new SetHomeEquipCoordinate
                    {
                        characterId = CharacterManager.instance.controllerCharacter.instanceId,
                        equipInstanceId = instanceId,
                        mapInstanceId = mapId,
                        coordinate = mapItem.coordinate,
                    };
                    GameActionManager.instance.QueueAction(setHomeEquipCoordinate, true);
                }
            };
            GameActionManager.instance.QueueAction(creatHomeEquip, true);
        }
         
        if (runtimeMapItem.mapItemData.operateIds != null)
        {
            for (int i = 0; i < runtimeMapItem.mapItemData.operateIds.Count; i++)
            {
                runtimeMapItem.operateDatas.Add(runtimeMapItem.mapItemData.operateIds[i]);
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
        if (runtimeMapItem.mapItemData.creatAction != null)
        {
            runtimeMapItem.mapItemData.creatAction.Action(instanceId);
        }

        if (runtimeMapItem.mapItemData.triggerCells.Length > 0)
        {
            MapCellController.instance.AddTriggerCell(runtimeMapItem.mapItemData.triggerCells, mapId, runtimeMapItem.mapItemData.defaultEnter,
                runtimeMapItem.mapItemData.defaultExit, EntityType.角色, instanceId, mapItem.coordinate);
        }
        if (runtimeMapItem.mapItemData.playerTriggerCells != null && runtimeMapItem.mapItemData.playerTriggerCells.Length > 0)
        {
            MapCellController.instance.AddPlayerTriggerCell(runtimeMapItem.mapItemData.playerTriggerCells, mapId, runtimeMapItem.mapItemData.playerTriggerEvent,
                instanceId, mapItem.coordinate);
        }
        if (runtimeMapItem.mapItemData.colliderCells.Length > 0)
        {
            MapCellController.instance.AddBarrierCell(runtimeMapItem.mapItemData.colliderCells, mapItem.coordinate, mapId);
        }

        if (mapId == WorldMapObjManager.instance.displayMap)
        {
            WorldMapObjManager.instance.DisplayMapItem(runtimeMapItem);
        }
        if (!isInSaveData)
        {
            TryCreatField tryCreatField = new TryCreatField
            {
                roomId = mapId,
                itemInstanceId = mapItem.instanceId,
            };
            GameActionManager.instance.QueueAction(tryCreatField, true);
        }
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

    private async void TrySetMapItem(TrySetMapItem TrySetMapItem)
    {
        var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(TrySetMapItem.dataId);
        HashSet<int2> oldColliders = null;
        int2[] newTriggers = null;
        bool newItem=false;
        if (runtimeMapItems.TryGetValue(TrySetMapItem.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItem.mapInstanceId == TrySetMapItem.mapItemInstanceId
                && !runtimeMapItem.coordinate.Equals(TrySetMapItem.coordinate))
            {
                oldColliders = new HashSet<int2>();
                for (int i = 0; i < mapItemData.colliderCells.Length; i++)
                {
                    int2 oldCollider = mapItemData.colliderCells[i] + runtimeMapItem.coordinate;
                    oldColliders.Add(oldCollider);
                }
            } 
        }
        else
        {
            newItem = true;
        }
        newTriggers = new int2[mapItemData.triggerCells.Length];
        for (int i = 0; i < mapItemData.triggerCells.Length; i++)
        {
            int2 newTrigger = mapItemData.triggerCells[i] + TrySetMapItem.coordinate;
            newTriggers[i] = newTrigger;
        }

        if (MapCellController.instance.CheckFutureIsWalk(newTriggers, TrySetMapItem.mapInstance, oldColliders))
        {
            MoveMapItem mapItem = new MoveMapItem
            {
                coordinate = TrySetMapItem.coordinate,
                mapInstance = TrySetMapItem.mapInstance,
                mapItemInstanceId = TrySetMapItem.mapItemInstanceId,
                noneTryAdd=newItem,
                dataId=TrySetMapItem.dataId, 
            };
            MoveMapItem(mapItem);
            if (TrySetMapItem.setResult != null)
            {
                TrySetMapItem.setResult(true);
            }
        }
        else
        {
            if (TrySetMapItem.setResult != null)
            {
                TrySetMapItem.setResult(false);
            }
        }
    }

    private void MoveMapItem(MoveMapItem moveMapItem)
    {
        if (runtimeMapItems.TryGetValue(moveMapItem.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItem.mapInstanceId == moveMapItem.mapItemInstanceId &&
                runtimeMapItem.coordinate.Equals(moveMapItem.coordinate))
            {
            }
            else
            {
                var mapItemData = runtimeMapItem.mapItemData;

                MapCellController.instance.RemovePlayerTriggerCell(mapItemData.playerTriggerCells, runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                MapCellController.instance.RemoveTriggerCell(mapItemData.triggerCells, runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells, runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

                /*
                WorldMapObjManager.instance.DeleteMapItem(new DeleteMapItem
                {
                    mapItemInstanceId = runtimeMapItem.instanceId,
                });*/
                //设置新位置
                runtimeMapItem.mapInstanceId = moveMapItem.mapInstance;
                runtimeMapItem.coordinate = moveMapItem.coordinate;
                if (moveMapItem.mapItemInstanceId > 0)
                {
                    if (mapItemData.triggerCells.Length > 0)
                    {
                        MapCellController.instance.AddTriggerCell(mapItemData.triggerCells, moveMapItem.mapInstance, mapItemData.defaultEnter,
                            mapItemData.defaultExit, EntityType.角色, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
                    }
                    if (mapItemData.playerTriggerCells != null && mapItemData.playerTriggerCells.Length > 0)
                    {
                        MapCellController.instance.AddPlayerTriggerCell(mapItemData.playerTriggerCells, moveMapItem.mapInstance, mapItemData.playerTriggerEvent,
                            runtimeMapItem.instanceId, moveMapItem.coordinate);
                    }
                    if (mapItemData.colliderCells.Length > 0)
                    {
                        MapCellController.instance.AddBarrierCell(mapItemData.colliderCells, moveMapItem.coordinate, moveMapItem.mapInstance);
                    }
                }
                //runtimeMapItems.SetData(runtimeMapItem);
                RefreshMapItemDisplay refreshMapItemDisplay = new RefreshMapItemDisplay { runtimeMapItem = runtimeMapItem };
                GameActionManager.instance.QueueAction(refreshMapItemDisplay,true);
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
            equipInstanceId = moveMapItem.mapItemInstanceId,
            mapInstanceId = moveMapItem.mapInstance,
            coordinate = moveMapItem.coordinate
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

            GameDataSaveManager.instance.UserGameSaveData.SetMapLineData(DeleteMapLink.linkInstanceId, false); 
        }
    }

    private void InitMapLink(InitMapLink initMapLink)
    {
        int index = worldMapData.mapLines.FindIndex(m => m.instanceId == initMapLink.linkInstanceId);
        if (index >= 0 && !worldMapData.mapLines[index].zeroInit)
        {
            MapCellController.instance.InitLinkMap(worldMapData.mapLines[index]);
            GameDataSaveManager.instance.UserGameSaveData.SetMapLineData(initMapLink.linkInstanceId, true);
        }
    }

    private WorldMapData worldMapData;

    public WorldMap GetWorldMap(int id)
    {
        return worldMapData.worldMapDic[id];
    }
    /// <summary>
    /// 初始化世界数据
    /// </summary>
    /// <param name="worldName"></param>
    /// <param name="displayMap"></param>
    /// <returns></returns>
    private async Task InitWorldData(string worldName, int displayMap = 0)
    {
        worldMapData = await GameDataManager.instance.GetAsyncData<WorldMapData>(worldName);
        //MapCellController.instance.InitWorldRoomDatas(worldMapData.worldMaps.Count);

        //roomMapDatas.Clear();
        if (displayMap == 0)
        {
            displayMap = worldMapData.defaultMap;
        }
        var displayRoom=worldMapData.worldMapDic[displayMap];
        await CreatRoomRuntime(displayRoom,true);

        foreach (var room in worldMapData.worldMapDic.Values)
        {
            if (room.id != displayMap)
            {
              await  CreatRoomRuntime(room, false); 
            }
           
        }
       

        async Task CreatRoomRuntime(WorldMap room,bool display)
        {
            //获取房间数据
            var MapRoomData = room.mapRoomData;
            //roomMapDatas.Add(room.id, room.map);

            //创建地图房间
            MapCellController.instance.InitMapData(room.id, MapRoomData.mapCells.ToArray(),
                MapRoomData.startCoordinate, MapRoomData.endCoordinate, room.coordinate);

            foreach (var data in MapRoomData.mapItems)
            {
                int itemInstanceId= await AddMapItem(data, room.id);
                GameDataSaveManager.instance.InitMapItemSaveData(itemInstanceId); 
            }
            if (display)
            {
                WorldMapObjManager.instance.DefaultDisplayMap(displayMap);
            }
            if (room.eventId != 0)
            {
                await GameEventManager.instance.AddGameEvent(room.eventId);
            }
        }

        //生成地图链接
        MapCellController.instance.InitLinkMap(worldMapData.mapLines);
        // return true;

       
        //WorldMapObjManager.instance.DefaultDisplayMap(displayMap);
    }

    private async void ChangeMapItem(ChangeMapItem changeMapItem)
    {
        if (GetRuntimeMapItem(changeMapItem.itemId, out RuntimeMapItem runtimeMapItem))
        {
            if (changeMapItem.newDataId > 0 && runtimeMapItem.mapItemData.id != changeMapItem.newDataId)
            {
                MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(changeMapItem.newDataId);
                runtimeMapItem.mapItemData =mapItemData;
                WorldMapObjManager.instance.ChangeMapItemDisplay(changeMapItem.itemId, changeMapItem.newDataId, changeMapItem.animationKey, runtimeMapItem);


                int3 value = new int3(changeMapItem.newDataId, runtimeMapItem.animationKey);
                GameDataSaveManager.instance.UserGameSaveData.AddChangeMapItem(value,new int2(runtimeMapItem.mapInstanceId,runtimeMapItem.editorInstanceId),runtimeMapItem.instanceId);
            }
            else
            {
                runtimeMapItem.SetAnimationKey(changeMapItem.animationKey);   
            }
           // runtimeMapItems.SetData(runtimeMapItem);
        }
    }
}

public class RuntimeMapItem : INativeData
{
    public int instanceId;
    public int editorInstanceId;
    public MapItemData mapItemData;
    public int mapInstanceId;
    public int2 coordinate;
    public int2 animationKey { get; private set; }
    public int linkCharacter;
    public int2 editorKey { get => new int2(mapInstanceId, editorInstanceId); }

    public RuntimeMapItem(int instanceId, int editorInstanceId,  MapItemData mapItemData, int mapInstanceId, int2 coordinate,int2 animationKey)
    {
        this.instanceId = instanceId;
        this.editorInstanceId = editorInstanceId;
        this.mapItemData = mapItemData;
        this.mapInstanceId = mapInstanceId;
        this.coordinate = coordinate;
        this.animationKey = animationKey;
        operateDatas = new NativeHashSet<int>(8, Allocator.Persistent);
        EventReferenceData = new NativeHashMap<FixedString128Bytes, int>(2, Allocator.Persistent);
    }
    public void SetAnimationKey(int2 animationKey)
    { 
        if (animationKey.x != int.MinValue || animationKey.y != int.MinValue)
        {
            int2 nowAnimationKey = this.animationKey;
            if (animationKey.x != int.MinValue)
            {
                nowAnimationKey.x= animationKey.x;
            }
            if (animationKey.y != int.MinValue)
            {
                nowAnimationKey.y = animationKey.y;
            }
            this.animationKey = nowAnimationKey;
            WorldMapObjManager.instance.SetItemAnimation(this);

            GameDataSaveManager.instance.UserGameSaveData.AddAnimationStateMapItem(animationKey,editorKey,instanceId);
        } 
    }
    public NativeHashSet<int> operateDatas;
    public NativeHashMap<FixedString128Bytes, int> EventReferenceData;
    public int Key => instanceId;

    public void Dispose()
    {
        operateDatas.Dispose();
        EventReferenceData.Dispose();
    }
}