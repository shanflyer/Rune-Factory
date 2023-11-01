using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics; 
using UnityEngine;

public class WorldMapManager : Singleton<WorldMapManager>
{
    private Dictionary<int, RuntimeObj> nowRuntimeMapItemObjs = new Dictionary<int, RuntimeObj>();

    private MyNativeData<RuntimeMapItem> runtimeMapItems;

    private Dictionary<int, List<int>> itemInMapDatas = new Dictionary<int, List<int>>();

    //每个地图对应的地图数据
    private Dictionary<int, string> roomMapDatas = new Dictionary<int, string>(); 

    private Dictionary<int2, int> editorItemRemapInstanceIds = new Dictionary<int2, int>();

    public RuntimeObj nowMapRoomObj;

    public int displayMap { get;set; }

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
    }

    protected override void Clear()
    {
        base.Clear();
        nowRuntimeMapItemObjs.Clear();
        runtimeMapItems.Dispose();
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
                if (nowRuntimeMapItemObjs.TryGetValue(mapItemInstanceId, out RuntimeObj RuntimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(RuntimeObj);
                }
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
    private async void SetItemAnimation(SetItemAnimation setItemAnimation)
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

            if (nowRuntimeMapItemObjs.ContainsKey(instanceid))
            {
                await SetItemAimation(runtimeMapItem.animationKey, runtimeMapItem.dataId, instanceid);
            }
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
    private async Task RuntimeMapItemPlay(RuntimeMapItem mapItem, RuntimeObj runtimeObj)
    {
        
        Animator animator = (runtimeObj.obj as Transform).GetComponent<Animator>();
        if (animator)
        {
            MyAnimationController.instance.AddItemAnimation(mapItem.instanceId, animator, mapItem.dataId.ToString());
            await SetItemAimation(mapItem.animationKey, mapItem.dataId, mapItem.instanceId);
        }
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

    public bool GetRuntimeMapItemObj(int instanceId, out RuntimeObj runtimeObj)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(instanceId, out runtimeObj))
        {
            return true;
        }
        runtimeObj =default(RuntimeObj);

        return false;
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
                    MapCellController.instance.RemoveTriggerCell(mapItemData.triggerCells,runtimeMapItem.mapInstanceId, deleteMapItem.mapItemInstanceId);
                    MapCellController.instance.RemoveBarrierCell(mapItemData.colliderCells,runtimeMapItem.coordinate, runtimeMapItem.mapInstanceId);
                }
            }
        }

        if (nowRuntimeMapItemObjs.TryGetValue(deleteMapItem.mapItemInstanceId, out RuntimeObj RuntimeObj))
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(RuntimeObj);
            nowRuntimeMapItemObjs.Remove(deleteMapItem.mapItemInstanceId);

            RemoveRuntimePackage removeRuntimePackage = new RemoveRuntimePackage
            {
                key = new Vector2Int(WorldMapManager.instance.displayMap, deleteMapItem.mapItemInstanceId)
            };
            GameActionManager.instance.QueueAction(removeRuntimePackage);

            MyAnimationController.instance.RemoveItemAnimation(deleteMapItem.mapItemInstanceId);
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
            mapInstanceId = mapId
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

        if (mapId == WorldMapManager.instance.displayMap)
        {
            DisplayMapItem(runtimeMapItem);
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

    private async Task<RuntimeObj> CreatMapRunTime(string roomName, int instanceId)
    {
        MapRoomData mapRoomData = await GameDataManager.instance.GetAsyncData<MapRoomData>(roomName);
        if (mapRoomData != null)
        {
            var mapRuntimeObj=  GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.MAPGROUND.ToString(), roomName, mapRoomData.mapObj.transform, instanceId);
            return mapRuntimeObj;
        }
        return default(RuntimeObj);
    }

    private async Task<RuntimeObj> CreatMapItemRuntime(int dataId, int instanceId, int2 coordinate)
    {
        Vector3 pos = GameCommon.GetMapPos(coordinate);
        //pos.z = -100;

        MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(dataId);
        if (mapItemData != null)
        {
            var mapItemRuntime= GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.MAPITEM.ToString(), dataId.ToString(), mapItemData.itemObj.transform, instanceId);
            (mapItemRuntime.obj as Transform).localPosition = pos;


            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
            {
                display = true,
                itemInstanceId = instanceId,
                transform = mapItemRuntime.obj as Transform
            };
            GameActionManager.instance.QueueAction(displayStoreCounter);

            return mapItemRuntime;
        }
        return default(RuntimeObj);
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
        await DisplayMap(displayMap);
    }

    private async Task DisplayMapItem(RuntimeMapItem runtimeMapItem)
    {
        if (!nowRuntimeMapItemObjs.ContainsKey(runtimeMapItem.instanceId))
        {
            var runtimeObj = await CreatMapItemRuntime(runtimeMapItem.dataId,runtimeMapItem.instanceId,runtimeMapItem.coordinate);
            nowRuntimeMapItemObjs[runtimeMapItem.instanceId] = runtimeObj;

            await RuntimeMapItemPlay(runtimeMapItem, runtimeObj); 
        }
    }
    public void DisplayMap()
    {
        DisplayMap(displayMap);
    }
    public async Task DisplayMap(int mapId)
    {
        displayMap = mapId;
        await CharacterManager.instance.RefreshNpcRuntimeObj();

        if (roomMapDatas.TryGetValue(mapId, out string dataId))
        {
            var coordinate = MapCellController.instance.GetRoomCoordinate(mapId);
            //Vector3 pos = GameCommon.GetMapPos(coordinate.x, coordinate.y) ;
            nowMapRoomObj = await CreatMapRunTime(dataId, mapId);
             
            (nowMapRoomObj.obj as Transform).localPosition =new Vector3(GameCommon.cellSize,GameCommon.cellSize);

            if (itemInMapDatas.TryGetValue(mapId, out List<int> mapItems))
            {
                for (int i = 0; i < mapItems.Count; i++)
                {
                    if (runtimeMapItems.GetData(mapItems[i], out RuntimeMapItem mapItem))
                    {
                        if (!nowRuntimeMapItemObjs.ContainsKey(mapItem.instanceId))
                        {
                            var itemObj = await CreatMapItemRuntime(mapItem.dataId, mapItem.instanceId, mapItem.coordinate);
                            nowRuntimeMapItemObjs.Add(mapItems[i], itemObj);

                            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
                            {
                                display = true,
                                itemInstanceId = mapItem.instanceId,
                                transform = itemObj.obj as Transform
                            };
                            GameActionManager.instance.QueueAction(displayStoreCounter);

                            await RuntimeMapItemPlay(mapItem, itemObj); 
                        } 
                    }
                }
            }
#if UNITY_EDITOR
            // if (MapCellTestDisplay.Instance)
            // {
            //     MapCellTestDisplay.Instance.RefreshTileMap(MapCellController.instance.GetRoomCellData(mapId));
            // }
#endif
        }
    }

    public void RecycleMap()
    {
        GameRuntimeObjManager.instance.RecycleRuntimeObj(nowMapRoomObj);

        foreach(var runTimeMapItemData in nowRuntimeMapItemObjs)
        {
            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
            {
                display = false,
                itemInstanceId = runTimeMapItemData.Key, 
            };
            GameActionManager.instance.QueueAction(displayStoreCounter,true); 
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runTimeMapItemData.Value);
        } 
        nowRuntimeMapItemObjs.Clear();
        MyAnimationController.instance.ClearAnimation();
    }

    private async void ChangeMapItem(ChangeMapItem changeMapItem)
    {
        if (GetRuntimeMapItem(changeMapItem.itemId, out RuntimeMapItem runtimeMapItem))
        {
            if (changeMapItem.newDataId > 0 && runtimeMapItem.dataId != changeMapItem.newDataId)
            {
                runtimeMapItem.dataId = changeMapItem.newDataId;
                if (nowRuntimeMapItemObjs.TryGetValue(changeMapItem.itemId, out RuntimeObj runtimeObj))
                {

                    DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
                    {
                        display = false,
                        itemInstanceId = runtimeMapItem.instanceId,
                    };
                    GameActionManager.instance.QueueAction(displayStoreCounter, true); 



                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                    var newObj = await CreatMapItemRuntime(runtimeMapItem.dataId,runtimeMapItem.instanceId,runtimeMapItem.coordinate);
                    nowRuntimeMapItemObjs[changeMapItem.itemId] = newObj;

                    MyAnimationController.instance.RemoveItemAnimation(changeMapItem.itemId);

                    Animator animator = (runtimeObj.obj as Transform).GetComponentInChildren<Animator>(true);
                    if (animator)
                    {
                        MyAnimationController.instance.AddItemAnimation(changeMapItem.itemId, animator, changeMapItem.newDataId.ToString());
                        await SetItemAimation(changeMapItem.animationKey, changeMapItem.newDataId, changeMapItem.itemId);
                    }
                }
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
    public int Key => instanceId;
    public void Dispose()
    {
    }
}