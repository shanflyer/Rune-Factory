using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class WorldMapManager : Singleton<WorldMapManager>
{
    private Dictionary<int, RuntimeMapItem> runtimeMapItems;

    private Dictionary<int, MySet<int>> itemInMapDatas = new Dictionary<int, MySet<int>>(); 
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

    public int GetNearestItem(int roomId, HashSet<int> itemDataIds, int2 coordinate)
    {
        if (itemInMapDatas.TryGetValue(roomId, out var itemIds))
        {
            float minDistance = int.MaxValue;
            RuntimeMapItem nearestItem = null;
            for (var i = 0; i < itemIds.length; i++)
            {
                var itemId = itemIds[i];
                if (runtimeMapItems.TryGetValue(itemId, out var item) && item.linkCharacter <= 0)
                    if (itemDataIds.Contains(item.mapItemData.id))
                    {
                        var distance = math.distancesq(coordinate, item.coordinate);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestItem = item;
                        }
                    }
            }

            if (nearestItem != null) return nearestItem.instanceId;
        }

        return 0;
    }

    public int GetNearestItem(int roomId, int itemDataId, int2 coordinate)
    {
        if (itemInMapDatas.TryGetValue(roomId, out var itemIds))
        {
            float maxDistance = int.MinValue;
            RuntimeMapItem nearestItem = null;
            for (var i = 0; i < itemIds.length; i++)
            {
                var itemId = itemIds[i];
                if (runtimeMapItems.TryGetValue(itemId, out var item))
                    if (item.mapItemData.id == itemDataId)
                    {
                        var distance = math.distancesq(coordinate, item.coordinate);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            nearestItem = item;
                        }
                    }
            }

            if (nearestItem != null) return nearestItem.instanceId;
        }

        return 0;
    }
    public override void Init()
    {
        base.Init();
        runtimeMapItems = new Dictionary<int, RuntimeMapItem>();

        GameActionManager.instance.AddListener<RemoveMapItemCollider>(RemoveMapItemCollider);
        GameActionManager.instance.AddListener<ReSetMapItemCollider>(ReSetMapItemCollider);
        GameActionManager.instance.AddAsyncListener<AddMapItem>(AddMapItemAsync, nameof(AddMapItem));
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteMapItem);
        GameActionManager.instance.AddAsyncListener<ChangeMapItem>(ChangeMapItemAsync, nameof(ChangeMapItem));
        GameActionManager.instance.AddListener<SetItemAnimation>(SetItemAnimation);
        GameActionManager.instance.AddAsyncListener<ChangeWorld>(ChangeWorldAsync, nameof(ChangeWorld));
        GameActionManager.instance.AddAsyncListener<TryCreatRoom>(TryCreatRoomAsync, nameof(TryCreatRoom));
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
        GameActionManager.instance.AddListener<CheckMapEditorItemLinkCharacter>(CheckMapEditorItemLinkCharacter);
        GameActionManager.instance.AddListener<RefreshMapTempCharacter>(RefreshMapTempCharacter);
        GameActionManager.instance.AddListener<TryLinkMapItemCharacter>(TryLinkMapItemCharacter);
        GameActionManager.instance.AddListener<TryRemoveLinkMapItemCharacter>(TryRemoveLinkMapItemCharacter);
        GameActionManager.instance.AddListener<ResetOperateData>(ResetOperateData);
    }

    public bool GetRandomItemPlayerTriggerCell(int roomId, int itemEditorInstance, out int2 cell)
    {
        MapCellController.instance.TransTempMap(ref roomId);
        if (editorItemRemapInstanceIds.TryGetValue(new int2(roomId, itemEditorInstance), out var itemInstanceId))
        {
            // Debug.Log($"room:{roomId}-itemEditorInstance{itemEditorInstance}");
            if (MapCellController.instance.GetRandomItemPlayerTriggerCell(roomId, itemInstanceId, out cell))
                return true;
        }

        cell = int.MinValue;
        return false;
    }

    public bool GetRandomItemTriggerCell(int roomId, int itemEditorInstance, out int2 cell)
    {
        MapCellController.instance.TransTempMap(ref roomId);
        if (editorItemRemapInstanceIds.TryGetValue(new int2(roomId, itemEditorInstance), out var itemInstanceId))
            // Debug.Log($"room:{roomId}-itemEditorInstance{itemEditorInstance}");
            if (MapCellController.instance.GetRandomItemTriggerCell(roomId, itemInstanceId, out cell))
                return true;

        cell = int.MinValue;
        return false;
    }
    public int2 GetItemCommonCenterTriggerCellForEditorInstance(int roomId, int itemEditorInstance)
    {
        MapCellController.instance.TransTempMap(ref roomId);
        if (editorItemRemapInstanceIds.TryGetValue(new int2(roomId, itemEditorInstance), out var itemInstanceId))
        {
            return MapCellController.instance.GetItemCommonCenterTriggerCell(roomId, itemInstanceId);
        }
        return new int2(int.MinValue, int.MinValue);
    }
    private void SetMapEditorItemLinkCharacter(SetMapEditorItemLinkCharacter SetMapEditorItemLinkCharacter)
    {
        MapCellController.instance.TransTempMap(ref SetMapEditorItemLinkCharacter.mapId);
        if (editorItemRemapInstanceIds.TryGetValue(new int2(SetMapEditorItemLinkCharacter.mapId, SetMapEditorItemLinkCharacter.mapItemEditorId),
               out var instanceId))
        {
            if (GetRuntimeMapItem(instanceId, out var runtimeMapItem))
            {
                if (runtimeMapItem.linkCharacter != 0)
                {
                    Character character = CharacterManager.instance.GetCharacter(runtimeMapItem.linkCharacter);
                    character.linkItem = 0;
                }
                runtimeMapItem.linkCharacter = SetMapEditorItemLinkCharacter.linkInstanceId;

                if (runtimeMapItem.linkCharacter != 0)
                {
                    Character character = CharacterManager.instance.GetCharacter(runtimeMapItem.linkCharacter);
                    character.linkItem = runtimeMapItem.instanceId;
                }

                if (SetMapEditorItemLinkCharacter.setValue != null) SetMapEditorItemLinkCharacter.setValue(instanceId);
                if (SetMapEditorItemLinkCharacter.setResult != null)
                {
                    SetMapEditorItemLinkCharacter.setResult(true);
                    return;
                }
            }
        }
        if (SetMapEditorItemLinkCharacter.setResult != null)
        {
            SetMapEditorItemLinkCharacter.setResult(false);
        }
    }
    public void CheckMapEditorItemLinkCharacter(CheckMapEditorItemLinkCharacter checkMapEditorItemLinkCharacter)
    {
        MapCellController.instance.TransTempMap(ref checkMapEditorItemLinkCharacter.mapId);
        int2 editorKey = new int2(checkMapEditorItemLinkCharacter.mapId, checkMapEditorItemLinkCharacter.itemEditorId);
        bool result = CanLinkRuntimeMapItem(editorKey, checkMapEditorItemLinkCharacter.characterId);
        if (checkMapEditorItemLinkCharacter.setResult != null)
        {
            checkMapEditorItemLinkCharacter.setResult(result);
        }
    }
    public bool CanLinkRuntimeMapItem(int2 editorKey, int linkCharacterId)
    {
        if (editorItemRemapInstanceIds.TryGetValue(editorKey,
              out var instanceId))
        {
            if (GetRuntimeMapItem(instanceId, out var runtimeMapItem))
            {

                if (runtimeMapItem.linkCharacter <= 0 || runtimeMapItem.linkCharacter == linkCharacterId)
                {
                    return true;
                }
                Debug.LogWarning($"已经有人：{runtimeMapItem.linkCharacter}");
            }
            return false;
        }
        Debug.LogWarning($"找不到：{editorKey}-linkCharacter:{linkCharacterId}");
        return false;
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
                Debug.LogWarning($"找不到：{editorKey}-linkCharacter:{linkCharacterId}");
            }
            return false;
        }
        Debug.LogWarning($"找不到：{editorKey}-linkCharacter:{linkCharacterId}");
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

    void RefreshMapTempCharacter(RefreshMapTempCharacter refreshMapTempCharacter)
    {
        Character character = CharacterManager.instance.GetCharacter(refreshMapTempCharacter.characterId);
        if (character != null&&character.linkItem!=0)
        {
           if(CharacterManager.instance.GetRuntimeCharacterObj(character.instanceId,out var characterRuntimeObj)&&
              GetRuntimeMapItem(character.linkItem, out var runtimeMapItem) &&
              runtimeMapItem.mapInstanceId == character.mapInstance)
            {
                SetCharacterTempPos setCharacterTempPos = new SetCharacterTempPos
                {
                    characterId = character.instanceId,
                };
                if (runtimeMapItem.leftCharacter == character.instanceId)
                {
                    setCharacterTempPos.pos = runtimeMapItem.mapItemData.leftLinkPos +(Vector3) runtimeMapItem.pos;
                    GameActionManager.instance.QueueAction(setCharacterTempPos);
                }
                else if (runtimeMapItem.rightCharacter == character.instanceId)
                {
                    setCharacterTempPos.pos = runtimeMapItem.mapItemData.rightLinkPos+ (Vector3)runtimeMapItem.pos;
                    GameActionManager.instance.QueueAction(setCharacterTempPos);
                }
                else if (runtimeMapItem.linkCharacter == character.instanceId)
                {
                    setCharacterTempPos.pos = runtimeMapItem.mapItemData.offsetLinkPos + (Vector3)runtimeMapItem.pos;
                    GameActionManager.instance.QueueAction(setCharacterTempPos);
                }
                character.SetDirection(runtimeMapItem.mapItemData.linkDirection);
            }
        }

       
    }
    private void TryLinkMapItemCharacter(TryLinkMapItemCharacter tryLinkMapItemCharacter)
    {
        if (GetRuntimeMapItem(tryLinkMapItemCharacter.mapItemInstanceId, out var runtimeMapItem))
        {
            Character character =CharacterManager.instance.controllerCharacter;
            if (runtimeMapItem.linkCharacter != 0)
            {
                character = CharacterManager.instance.GetCharacter(runtimeMapItem.linkCharacter); 
            }
            if (runtimeMapItem.leftCharacter == 0)
            {
                runtimeMapItem.leftCharacter = character.instanceId;
                character.linkItem = runtimeMapItem.instanceId ; 
            }
            else if (runtimeMapItem.rightCharacter == 0)
            {
                runtimeMapItem.rightCharacter = character.instanceId;
                character.linkItem = runtimeMapItem.instanceId;
            }
            else if (runtimeMapItem.linkCharacter == 0)
            {
                runtimeMapItem.linkCharacter = character.instanceId;
                character.linkItem = runtimeMapItem.instanceId;
            }
            RefreshMapTempCharacter RefreshMapTempCharacter = new RefreshMapTempCharacter
            {
                characterId = character.instanceId,
            };
            GameActionManager.instance.QueueAction(RefreshMapTempCharacter);
        }
    }
    void TryRemoveLinkMapItemCharacter(TryRemoveLinkMapItemCharacter tryRemoveLinkMapItemCharacter)
    {
        if (GetRuntimeMapItem(tryRemoveLinkMapItemCharacter.mapItemInstanceId, out var runtimeMapItem))
        {
            Character character = CharacterManager.instance.controllerCharacter;
            if (runtimeMapItem.linkCharacter != 0)
            {
                character = CharacterManager.instance.GetCharacter(runtimeMapItem.linkCharacter);
            }
            if (runtimeMapItem.leftCharacter == character.instanceId)
            {
                runtimeMapItem.leftCharacter = 0;
                character.linkItem = 0;
            }
            else if (runtimeMapItem.rightCharacter == character.instanceId)
            {
                runtimeMapItem.rightCharacter = 0;
                character.linkItem = 0;
            }
            else if (runtimeMapItem.linkCharacter == character.instanceId)
            {
                runtimeMapItem.linkCharacter = 0;
                character.linkItem = 0;
            }
            RefreshCharacterPos refreshCharacterPos = new RefreshCharacterPos
            {
                characterId = character.instanceId,
            };
            GameActionManager.instance.QueueAction(refreshCharacterPos);
            SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
            {
                characterId = character.instanceId,
                parameterType = ParameterType.BOOL,
                parameter = "Set",
                boolValue = false
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator);
            SetCharacterAnimator setCharacterAnimator1 = new SetCharacterAnimator
            {
                characterId = character.instanceId,
                parameterType = ParameterType.INT,
                parameter = "State",
                intValue=0
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator1);
            runtimeMapItem.linkCharacter = 0;
            // runtimeMapItems.SetData(runtimeMapItem);
        }
    }
    private void SetMapItemLinkCharacter(SetMapItemLinkCharacter SetMapItemLinkCharacter)
    {
        if (GetRuntimeMapItem(SetMapItemLinkCharacter.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItem.linkCharacter != 0)
            {
                Character character = CharacterManager.instance.GetCharacter(runtimeMapItem.linkCharacter);
                character.linkItem = 0;
            }
            runtimeMapItem.linkCharacter = SetMapItemLinkCharacter.linkInstanceId;

            if (runtimeMapItem.linkCharacter != 0)
            {
                Character character = CharacterManager.instance.GetCharacter(runtimeMapItem.linkCharacter);
                character.linkItem = runtimeMapItem.instanceId;
            } 
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
                GameDataSaveManager.instance.UserGameSaveData.AddMapItemOperate(new int3(runtimeMapItem.editorKey, AddMapItemOperate.addeOperateId), runtimeMapItem.instanceId);
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
                GameDataSaveManager.instance.UserGameSaveData.RemoveMapItemOperate(new int3(runtimeMapItem.editorKey, removeMapItemOperate.removeOperateId), runtimeMapItem.instanceId);
            }
            //runtimeMapItems.SetData(runtimeMapItem);
        }
    }

    protected override void Clear()
    {
        base.Clear();
        foreach (var runtimeMapItem in runtimeMapItems)
        {
            runtimeMapItem.Value.Dispose();
        }
        runtimeMapItems.Clear();
    }

    public List<int> GetMapItems(int mapId)
    {
        MapCellController.instance.TransTempMap(ref mapId);
        if (!itemInMapDatas.TryGetValue(mapId, out var result))
        {
            return new List<int>();
        }
        return result.GetValueList(true);
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

    private async System.Threading.Tasks.Task TryCreatRoomAsync(TryCreatRoom creatRoom)
    {
        int instanceId = creatRoom.instance;
        if (instanceId == 0)
        {
            instanceId = MyInstance.instance.TempUid;
        }

        var MapRoomData = await GameDataManager.instance.GetAsyncData<MapRoomData>(creatRoom.roomId);
        // roomMapDatas.Add(instanceId, MapRoomData.roomName);
        //创建地图房间
        MapCellController.instance.InitMapData(instanceId, MapRoomData, int3.zero);

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
        if (itemInMapDatas.TryGetValue(roomInstanceid, out var mapItems))
        {
            for (int i = 0; i < mapItems.length; i++)
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

    private async System.Threading.Tasks.Task ChangeWorldAsync(ChangeWorld changeWorld)
    { 
        await InitWorldData(changeWorld.worldName, changeWorld.displayMap);
    }

    private void RefreshManufature(RefreshManufature refreshManufature)
    {
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
            MapCellController.instance.TransTempMap(ref setItemAnimation.mapId);
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

    public bool GetRuntimeMapItem(int2 editorKey, out RuntimeMapItem runtimeMapItem)
    {
        if (editorItemRemapInstanceIds.TryGetValue(editorKey, out var instanceid))
        {
            runtimeMapItem = runtimeMapItems[instanceid];
            return true;
        }

        runtimeMapItem = null;
        return false;
    }
    public bool GetRuntimeMapItem(int instanceId, out RuntimeMapItem runtimeMapItem)
    {
        return runtimeMapItems.TryGetValue(instanceId, out runtimeMapItem);
    }
 

    public void SaveMapItemInstance(int instance)
    {
        if(runtimeMapItems.TryGetValue(instance,out var runtimeMapItem))
        {
            GameDataSaveManager.instance.UserGameSaveData.SaveSpecialMapItem(runtimeMapItem.editorKey, instance);
        }
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
            MapCellController.instance.RemoveBarrierCell(GameCommon.GridToCells(mapItemData.colliderGrids).ToArray(), runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

            GameDataSaveManager.instance.UserGameSaveData.AddRemoveMapItemColliderData(new int2(runtimeMapItem.mapInstanceId, runtimeMapItem.editorInstanceId),
                runtimeMapItem.instanceId);
        }
    }

    private void ReSetMapItemCollider(ReSetMapItemCollider reSetMapItemCollider)
    {
        if (runtimeMapItems.TryGetValue(reSetMapItemCollider.mapItemInstanceId, out var runtimeMapItem))
        {
            var mapItemData = runtimeMapItem.mapItemData;
            MapCellController.instance.AddBarrierCell(GameCommon.GridToCells(mapItemData.colliderGrids).ToArray(), runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

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

                    MapCellController.instance.RemovePlayerTriggerCell(runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                    MapCellController.instance.RemoveTriggerCell(runtimeMapItem.mapInstanceId, deleteMapItem.mapItemInstanceId);
                    MapCellController.instance.RemoveBarrierCell(GameCommon.GridToCells(mapItemData.colliderGrids).ToArray(), runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
                }
            }
        }
    }

    private async Task<int> AddMapItem(MapItem mapItem, int mapId,int fixedInstance=0)
    { 
        int instanceId = 0;
        if (fixedInstance == 0)
        {
            int2 itemkey = new int2(mapId, mapItem.instanceId);
            if (mapItem.instanceId != 0)
            {
                instanceId = GameDataSaveManager.instance.GetSaveMapInstance(itemkey);
            }
            if (instanceId == 0)
            {
                instanceId = MyInstance.instance.Uid;
                GameDataSaveManager.instance.SaveSpecialItem(itemkey, instanceId);
            }
        }else
        {
            instanceId = fixedInstance;
        }
      

        int2 key = new int2(mapId, mapItem.instanceId);
        if (mapItem.instanceId != 0&&!editorItemRemapInstanceIds.ContainsKey(key))
        {
            editorItemRemapInstanceIds.Add(key, instanceId);
        }
        RuntimeMapItem runtimeMapItem = new RuntimeMapItem(instanceId, mapItem.instanceId, await GameDataManager.instance.GetAsyncData<MapItemData>(mapItem.id),
            mapId, mapItem.coordinate, mapItem.animationKey);
        if (GameDataSaveManager.instance.UserGameSaveData.ChangeMapItemCoordinate.TryGetValue(instanceId,
                out var mapItemData))
        {
            runtimeMapItem.mapInstanceId = mapItemData.newMap;
            runtimeMapItem.coordinate = mapItemData.newCoordinate;
        }

        runtimeMapItems.Add(instanceId, runtimeMapItem);
        if (!itemInMapDatas.TryGetValue(mapId, out var items))
        {
            items = new MySet<int>();
            itemInMapDatas.Add(mapId, items);
        }
        items.Add(instanceId);

        if ( mapItem.blindHomeEquipment != 0)
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
            runtimeMapItem.mapItemData.creatAction.Action(instanceId,runtimeMapItem.editorInstanceId, mapId);
        }

        if (runtimeMapItem.mapInstanceId > 0)
        {
            if (runtimeMapItem.mapItemData.triggerGrids.Count > 0)
            {
                MapCellController.instance.AddTriggerCell(GameCommon.GridToCells(runtimeMapItem.mapItemData.triggerGrids).ToArray(), mapId, runtimeMapItem.mapItemData.defaultEnter,
                    runtimeMapItem.mapItemData.defaultExit, EntityType.角色, instanceId, mapItem.coordinate);
            }
            if (runtimeMapItem.mapItemData.playerTriggerGrids != null && runtimeMapItem.mapItemData.playerTriggerGrids.Count > 0)
            {
                MapCellController.instance.AddPlayerTriggerCell(GameCommon.GridToCells(runtimeMapItem.mapItemData.playerTriggerGrids).ToArray(), mapId, runtimeMapItem.mapItemData.playerTriggerEvent,
                    instanceId, mapItem.coordinate,runtimeMapItem.mapItemData.isPlayerForwardTrigger);
            }
            if (runtimeMapItem.mapItemData.colliderGrids.Count > 0)
            {
                MapCellController.instance.AddBarrierCell(GameCommon.GridToCells(runtimeMapItem.mapItemData.colliderGrids).ToArray(), mapItem.coordinate, mapId);
            }

            if (mapId == WorldMapObjManager.instance.displayMap)
            {
                await WorldMapObjManager.instance.DisplayMapItem(runtimeMapItem);
            }
        }
        

        if (mapItem.instanceId != 0)
        {
            await LoadMapItemAsync(mapId, mapItem.instanceId);
        }
      
       
        return runtimeMapItem.instanceId;
    }

    private async System.Threading.Tasks.Task AddMapItemAsync(AddMapItem addMapItem)
    {
        if (addMapItem.mapId>0&&!MapCellController.instance.ContainsRoom(addMapItem.mapId))
        {
            return;
        }

        MapCellController.instance.TransTempMap(ref addMapItem.mapId);
        MapItem mapItem = new MapItem
        {
            id = addMapItem.dataId,
            coordinate = addMapItem.coordinate,
            instanceId= addMapItem.instanceId
        };

        int instanceId = await AddMapItem(mapItem, addMapItem.mapId,addMapItem.fixeInstanceId);

        if (addMapItem.setValue != null)
        {
            addMapItem.setValue(instanceId);
        }
        if (addMapItem.setResult != null)
        {
            addMapItem.setResult(true);
        }
    }

    private  void TrySetMapItem(TrySetMapItem TrySetMapItem)
    {
        var mapItemData = GameDataManager.instance.GetData<MapItemData>(TrySetMapItem.dataId.ToString());
        //var mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(TrySetMapItem.dataId);
        HashSet<int2> newTriggers = new HashSet<int2>();
        HashSet<int2> oldTriggers = new HashSet<int2>();

        bool newItem = false;
        var triggerCells = GameCommon.GridToCells(mapItemData.colliderGrids, new int4(1, 1, 1, 1));
        if (runtimeMapItems.TryGetValue(TrySetMapItem.mapItemInstanceId, out var runtimeMapItem))
        {
            if (runtimeMapItem.mapInstanceId == TrySetMapItem.mapItemInstanceId
                && !runtimeMapItem.coordinate.Equals(TrySetMapItem.coordinate))
            {
                for (int i = 0; i < triggerCells.Count; i++)
                {
                    int2 newTrigger = triggerCells[i] + TrySetMapItem.coordinate;
                    newTriggers.Add(newTrigger);
                    int2 oldTrigger = triggerCells[i] + runtimeMapItem.coordinate;
                    oldTriggers.Add(oldTrigger);
                }
            }
            else
            {
                for (int i = 0; i < triggerCells.Count; i++)
                {
                    int2 newTrigger = triggerCells[i] + TrySetMapItem.coordinate;
                    newTriggers.Add(newTrigger); 
                }
            }
        }
        else
        {
            for (int i = 0; i < triggerCells.Count; i++)
            {
                int2 newTrigger = triggerCells[i] + TrySetMapItem.coordinate;
                newTriggers.Add(newTrigger); 
            }
            newItem = true;
        }
        HashSet<int2> mapCharacterCells = MapCellController.instance.GetMapCharacterCells(TrySetMapItem.mapInstance);
        newTriggers.ExceptWith(oldTriggers);
        var _tempCell=newTriggers.Intersect(mapCharacterCells);
        if (_tempCell.Count() > 0)
        {

            if (TrySetMapItem.setResult != null)
            {
                TrySetMapItem.setResult(false);
            }
        }
        else if (MapCellController.instance.CheckFutureIsWalk(newTriggers, TrySetMapItem.mapInstance))
        {
            MoveMapItem mapItem = new MoveMapItem
            {
                coordinate = TrySetMapItem.coordinate,
                mapInstance = TrySetMapItem.mapInstance,
                mapItemInstanceId = TrySetMapItem.mapItemInstanceId,
                noneTryAdd = newItem,
                dataId = TrySetMapItem.dataId,
                setValue = TrySetMapItem.setValue,
                setResult = TrySetMapItem.setResult
            };

            MoveMapItem(mapItem);

        }
        else
        {
            if (TrySetMapItem.setResult != null)
            {
                TrySetMapItem.setResult(false);
            }
        }  
    }
    void ResetOperateData(ResetOperateData resetOperateData)
    {
        if (runtimeMapItems.TryGetValue(resetOperateData.mapItemInstanceId, out var runtimeMapItem))
        {
            if(resetOperateData.operates!=null)
                runtimeMapItem.ResetOperateData(resetOperateData.operates);
            if (CharacterManager.instance.controllerCharacter.OperateItem== resetOperateData.mapItemInstanceId)
            {
                runtimeMapItem.RefreshItemOperate();
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

                MapCellController.instance.RemovePlayerTriggerCell(runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                MapCellController.instance.RemoveTriggerCell(runtimeMapItem.mapInstanceId, runtimeMapItem.instanceId);
                MapCellController.instance.RemoveBarrierCell(GameCommon.GridToCells(mapItemData.colliderGrids).ToArray(), runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);

                /*
                WorldMapObjManager.instance.DeleteMapItem(new DeleteMapItem
                {
                    mapItemInstanceId = runtimeMapItem.instanceId,
                });*/

                if(itemInMapDatas.TryGetValue(runtimeMapItem.mapInstanceId,out var ints))
                {
                    ints.Remove(runtimeMapItem.instanceId);
                }
                if(itemInMapDatas.TryGetValue(moveMapItem.mapInstance,out var ints1))
                {
                    ints1.Add(runtimeMapItem.instanceId);
                }

                //设置新位置
                runtimeMapItem.mapInstanceId = moveMapItem.mapInstance;
                runtimeMapItem.coordinate = moveMapItem.coordinate;
                if (runtimeMapItem.mapInstanceId > 0)
                {
                    if (mapItemData.triggerGrids.Count > 0)
                    {
                        MapCellController.instance.AddTriggerCell(GameCommon.GridToCells(mapItemData.triggerGrids).ToArray(), moveMapItem.mapInstance, mapItemData.defaultEnter,
                            mapItemData.defaultExit, EntityType.角色, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
                    }
                    if (mapItemData.playerTriggerGrids != null && mapItemData.playerTriggerGrids.Count > 0)
                    {
                        MapCellController.instance.AddPlayerTriggerCell(GameCommon.GridToCells(mapItemData.playerTriggerGrids).ToArray(), moveMapItem.mapInstance, mapItemData.playerTriggerEvent,
                            runtimeMapItem.instanceId, moveMapItem.coordinate, runtimeMapItem.mapItemData.isPlayerForwardTrigger);
                    }
                    if (mapItemData.colliderGrids.Count > 0)
                    {
                        MapCellController.instance.AddBarrierCell(GameCommon.GridToCells(mapItemData.colliderGrids).ToArray(), moveMapItem.coordinate, moveMapItem.mapInstance);
                    }
                }
                //runtimeMapItems.SetData(runtimeMapItem);
                RefreshMapItemDisplay refreshMapItemDisplay = new RefreshMapItemDisplay { runtimeMapItem = runtimeMapItem };
                GameActionManager.instance.QueueAction(refreshMapItemDisplay, true);

                if (GameDataSaveManager.instance.loadDataIsNotNull)
                    GameDataSaveManager.instance.loadGameSaveData.SetMapItemCoordinate(runtimeMapItem.editorKey,
                        runtimeMapItem.instanceId,
                        runtimeMapItem.mapInstanceId, runtimeMapItem.coordinate);
            }

            if (moveMapItem.setResult != null)
            {
                moveMapItem.setResult(true);
            }
        }
        else //if(moveMapItem.noneTryAdd)
        {
            AddMapItem addMapItem = new AddMapItem
            {
                dataId = moveMapItem.dataId,
                mapId = moveMapItem.mapInstance,
                coordinate = moveMapItem.coordinate,
                instanceId = moveMapItem.mapItemInstanceId,
                setValue=LinkHomeEquipId,
                setResult=moveMapItem.setResult
            };
            AsyncTaskRunner.Run(() => AddMapItemAsync(addMapItem), nameof(AddMapItem));
            void LinkHomeEquipId(int instance)
            {
                if (moveMapItem.setValue != null)
                {
                    moveMapItem.setValue(instance);
                }
            }
        }
        SetHomeEquipCoordinate setHomeEquipCoordinate = new SetHomeEquipCoordinate
        {
            characterId = CharacterManager.instance.controllerCharacter.instanceId,
            equipInstanceId = moveMapItem.mapItemInstanceId,
            mapInstanceId = moveMapItem.mapInstance,
            coordinate = moveMapItem.coordinate
        };
        GameActionManager.instance.QueueAction(setHomeEquipCoordinate,true);
    }
 
    public bool InitSmoothMove(ref Vector2 direction, Vector3 nowPos, int mapId, float distance, ref int2 target, ref Vector3 targetPos)
    {
        MapCellController.instance.TransTempMap(ref mapId);
        if (direction == Vector2.zero)
        {
            return false;
        }

        Vector3 checkTargetPos = GameCommon.SetMapPosZ(nowPos + new Vector3(direction.x, direction.y, 0) * distance);
        int2 checkTargetCoordinate = GameCommon.GetMapCoordinateInt(checkTargetPos);
        if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate, mapId))
        {
            Vector2 direction1 = new Vector2(0, direction.y);
            Vector3 checkTargetPos1 = GameCommon.SetMapPosZ(nowPos + new Vector3(direction1.x, direction1.y, 0) * distance);
            int2 checkTargetCoordinate1 = GameCommon.GetMapCoordinateInt(checkTargetPos1);

            if (!MapCellController.instance.CheckIsWalk(checkTargetCoordinate1, mapId))
            {
                Vector2 direction2 = new Vector2(direction.x, 0);
                Vector3 checkTargetPos2 = GameCommon.SetMapPosZ(nowPos + new Vector3(direction2.x, direction2.y, 0) * distance);
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
        initMapLineSet.Remove(DeleteMapLink.linkInstanceId);
        int index = worldMapData.mapLines.FindIndex(m => m.instanceId == DeleteMapLink.linkInstanceId);
        if (index >= 0)
        {
            MapCellController.instance.DeleteMapLink(worldMapData.mapLines[index]);

            GameDataSaveManager.instance.UserGameSaveData.SetMapLineData(DeleteMapLink.linkInstanceId, false);
        }
    }
    HashSet<int> initMapLineSet = new HashSet<int>();
    private void InitMapLink(InitMapLink initMapLink)
    {
        if (initMapLineSet.Contains(initMapLink.linkInstanceId))
        {
            return;
        }
        initMapLineSet.Add(initMapLink.linkInstanceId);
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


    Dictionary<int,HashSet<int>> waitMaps = new Dictionary<int, HashSet<int>>();
    public async Task LoadMapItemAsync(int map,int instanceId)
    {
        if (waitMaps.Count == 0)
        {
            return;
        }
        if(waitMaps.TryGetValue(map,out var ints))
        {
            ints.Remove(instanceId);
            if (ints.Count == 0)
            {
                waitMaps.Remove(map);
            }
        }
        if (waitMaps.Count == 0)
        {
            var mapNpcDataList = await GameDataManager.instance.GetAsyncData<MapNpcDataList>();
            var datas = mapNpcDataList.datas;

            for (int i = 0; i < datas.Count; i++)
            {
                for (int j = 0; j < datas[i].datas.Count; j++)
                {
                    if (datas[i].datas[j].initialBegin)
                    {
                        await CharacterManager.instance.CreateNpc(datas[i].datas[j]);
                    }
                }
            }

            NPCManager.instance.InitNPCBehavior(); 
            GameActionManager.instance.QueueAction(new LoadMapCompleted());
        }
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
        waitMaps.Clear();
        var mapList = worldMapData.worldMapDic.Values.ToList();

        //设置地图障碍
        for (var i = 0; i < mapList.Count; i++)
        {
            var room = mapList[i];
            MapCellController.instance.InitMapData(room.id, room.mapRoomData, room.coordinate);
        }
 
        //设定地图物体
        for (var j = 0; j < mapList.Count; j++)
        {
            var room = mapList[j];
            if (room.mapRoomData.mapItems.Count > 0)
            {
                waitMaps.Add(room.id, new HashSet<int>());
                for (int i = 0; i < room.mapRoomData.mapItems.Count; i++)
                {
                    var item = room.mapRoomData.mapItems[i];
                    waitMaps[room.id].Add(item.instanceId);
                }
            }
        } 
        //roomMapDatas.Clear();
        if (displayMap == 0)
        {
            displayMap = worldMapData.defaultMap;
        }
        var displayRoom = worldMapData.worldMapDic[displayMap];
        await CreateRoomItem(displayRoom, true, displayMap);

        for (var j = 0; j < mapList.Count; j++)
        {
            var room = mapList[j];
            if (room.id != displayMap)
            {
                await CreateRoomItem(room, false, displayMap);
            }
        }
       
        //生成地图链接
        MapCellController.instance.InitLinkMap(worldMapData.mapLines);
        WorldMapObjManager.instance.DisplayTempNpc();
        
    }

    private async Task CreateRoomItem(WorldMap room, bool display, int displayMap = 0)
    {
        //获取房间数据
        var MapRoomData = room.mapRoomData;

        for (var i = 0; i < MapRoomData.mapItems.Count; i++)
        {
            var data = MapRoomData.mapItems[i];
            int itemInstanceId = await AddMapItem(data, room.id);
            if (data.funcItem) GameDataSaveManager.instance.InitMapItemSaveData(itemInstanceId);
        } 
        if (display)
        {
            await WorldMapObjManager.instance.DisplayMap(displayMap, zeroInit: true);
        }
        if (room.eventId != 0)
        {
            await GameEventManager.instance.AddGameEvent(room.eventId);
        } 
      
    }

    private async System.Threading.Tasks.Task ChangeMapItemAsync(ChangeMapItem changeMapItem)
    {
        if (GetRuntimeMapItem(changeMapItem.itemId, out RuntimeMapItem runtimeMapItem))
        {
            if (changeMapItem.newDataId > 0 && runtimeMapItem.mapItemData.id != changeMapItem.newDataId)
            {
                MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(changeMapItem.newDataId);
                runtimeMapItem.mapItemData = mapItemData;
                WorldMapObjManager.instance.ChangeMapItemDisplay(changeMapItem.itemId, changeMapItem.newDataId, changeMapItem.animationKey, runtimeMapItem);

                int3 value = new int3(changeMapItem.newDataId, runtimeMapItem.animationKey);
                GameDataSaveManager.instance.UserGameSaveData.AddChangeMapItem(value, new int2(runtimeMapItem.mapInstanceId, runtimeMapItem.editorInstanceId), runtimeMapItem.instanceId);
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
    public int leftCharacter, rightCharacter;
    public int2 editorKey { get; }
    public Vector3 pos => GameCommon.GetMapPos(coordinate);
    public RuntimeMapItem(int instanceId, int editorInstanceId, MapItemData mapItemData, int mapInstanceId, int2 coordinate, int2 animationKey)
    {
        this.instanceId = instanceId;
        this.editorInstanceId = editorInstanceId;
        this.mapItemData = mapItemData;
        this.mapInstanceId = mapInstanceId;
        this.coordinate = coordinate;
        this.animationKey = animationKey;
        editorKey = new int2(mapInstanceId, editorInstanceId);
        operateDatas = new List<int>();
        EventReferenceData = new Dictionary<string, int>();

        if (mapItemData.leftLinkPos == Vector3.zero)
        {
            leftCharacter = -1;
        }
        if (mapItemData.rightLinkPos == Vector3.zero)
        {
            rightCharacter = -1;
        }
    }

    public void SetAnimationKey(int2 animationKey)
    {
        if (animationKey.x != int.MinValue && animationKey.y != int.MinValue)
        {
            int2 nowAnimationKey = this.animationKey;
            if (animationKey.x != int.MinValue)
            {
                nowAnimationKey.x = animationKey.x;
            }
            if (animationKey.y != int.MinValue)
            {
                nowAnimationKey.y = animationKey.y;
            }
            this.animationKey = nowAnimationKey;
            WorldMapObjManager.instance.SetItemAnimation(this);

            GameDataSaveManager.instance.UserGameSaveData.AddAnimationStateMapItem(animationKey, editorKey, instanceId);
        }
    }

    public List<int> operateDatas;
    public Dictionary<string, int> EventReferenceData;

    public void RefreshItemOperate()
    {
        AsyncTaskRunner.Run(RefreshItemOperateAsync, nameof(RefreshItemOperate));
    }

    public async System.Threading.Tasks.Task RefreshItemOperateAsync()
    {
        //物体交互
        int operateDataLength =operateDatas.Count;
        OperateDataList operateDataList = new OperateDataList
        {
            OperateDatas = new List<OperateDataReferenceData>(),
            eventReferenceDatas = new List<EventReferenceData>()
        };
        foreach (var data in EventReferenceData)
        {
            EventReferenceData EventReferenceData = new EventReferenceData
            {
                name = data.Key.ToString(),
                value = data.Value,
                valueType = ReferenceValueType.Int
            };
            operateDataList.eventReferenceDatas.Add(EventReferenceData);
        }

       
        if (operateDataLength > 0)
        {
            HashSet<int> waitCheck=new HashSet<int>();
            foreach (var id in operateDatas)
            {
                OperateData operateData = GameDataManager.instance.GetData<OperateData>(id.ToString());
                if (operateData.checkActionData != null)
                {
                    waitCheck.Add(id);
                    operateData.checkActionData.Action(setResult: (bool value) =>
                    {
                        waitCheck.Remove(id);
                        if (value)
                        {
                            operateDataList.OperateDatas.Add(new OperateDataReferenceData
                            {
                                targetItem = instanceId,
                                operateData = operateData,
                            });
                        }
                        if (waitCheck.Count == 0)
                        {
                            DisplayItemOperate();
                        }
                    }, immediately: true);
                }
                else
                {
                    operateDataList.OperateDatas.Add(new OperateDataReferenceData
                    {
                        targetItem = instanceId,
                        operateData = operateData,
                    });
                }
               
            }
            if (waitCheck.Count == 0)
            {
                DisplayItemOperate();
            }
            
        }
        else
        {
            DisplayItemOperate(false);
        }

        void DisplayItemOperate(bool display = true)
        {
            // if (display)
            //     WorldMapObjManager.instance.TryDisplayMask(instanceId);
            // else
            //     WorldMapObjManager.instance.RecycleMaskObj(instanceId);

            UIManager.instance.ShowGamePanelImmediately<OperateButtonPanel, OperateDataList>(operateDataList);
        }
    }
 
    public void ResetOperateData(List<int> newOperates)
    {
        operateDatas.Clear();
        for(int i = 0; i < newOperates.Count; i++)
        {
            operateDatas.Add(newOperates[i]);
        }  
    }
    
    public int Key => instanceId;

    public void Dispose()
    {     
    }
}
