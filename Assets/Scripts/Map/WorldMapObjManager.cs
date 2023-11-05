using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class WorldMapObjManager:Singleton<WorldMapObjManager>
{
    Dictionary<int, RuntimeObj> nowRuntimeMapItemObjs = new Dictionary<int, RuntimeObj>();
    RuntimeObj nowMapRoomObj; 
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<TryDeleteRoom>(TryDeleteRoomObj);
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteMapItem);
    }
    protected override void Clear()
    {
        base.Clear();
        nowRuntimeMapItemObjs.Clear();
    }
    public int displayMap { get; set; }
    public void DefaultDisplayMap(int defaultMap)
    {
        if (displayMap == 0)
        {
            displayMap = defaultMap;
        }
        DisplayMap(displayMap);
    }
    private async Task<RuntimeObj> CreatMapRunTime(string roomName, int instanceId)
    {
        MapRoomData mapRoomData = await GameDataManager.instance.GetAsyncData<MapRoomData>(roomName);
        if (mapRoomData != null)
        {
            var mapRuntimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.MAPGROUND.ToString(), roomName, mapRoomData.mapObj.transform, instanceId);
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
            var mapItemRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.MAPITEM.ToString(), dataId.ToString(), mapItemData.itemObj.transform, instanceId);
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
    public async Task DisplayMap(int mapId)
    {
        displayMap = mapId;
        await CharacterManager.instance.RefreshNpcRuntimeObj();
        string dataId = WorldMapManager.instance.GerMapDataName(mapId);
        if (!string.IsNullOrEmpty(dataId))
        {
            var coordinate = MapCellController.instance.GetRoomCoordinate(mapId);
            //Vector3 pos = GameCommon.GetMapPos(coordinate.x, coordinate.y) ;
            nowMapRoomObj = await CreatMapRunTime(dataId, mapId);

            (nowMapRoomObj.obj as Transform).localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize);
            List<int> mapItems = WorldMapManager.instance.GetMapItems(mapId);
            for (int i = 0; i < mapItems.Count; i++)
            { 
                if (WorldMapManager.instance.GetRuntimeMapItem(mapItems[i], out RuntimeMapItem mapItem))
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
#if UNITY_EDITOR
            // if (MapCellTestDisplay.Instance)
            // {
            //     MapCellTestDisplay.Instance.RefreshTileMap(MapCellController.instance.GetRoomCellData(mapId));
            // }
#endif
        }
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

    public void RecycleMap()
    {
        GameRuntimeObjManager.instance.RecycleRuntimeObj(nowMapRoomObj);

        foreach (var runTimeMapItemData in nowRuntimeMapItemObjs)
        {
            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
            {
                display = false,
                itemInstanceId = runTimeMapItemData.Key,
            };
            GameActionManager.instance.QueueAction(displayStoreCounter, true);
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runTimeMapItemData.Value);
        }
        nowRuntimeMapItemObjs.Clear();
        MyAnimationController.instance.ClearAnimation();
    }

    private void TryDeleteRoomObj(TryDeleteRoom deleteRoom)
    {
        if (displayMap == deleteRoom.roomId)
        {
            RecycleMap();
        } 
    }

    public async void SetItemAnimation(RuntimeMapItem runtimeMapItem)
    {
        if (nowRuntimeMapItemObjs.ContainsKey(runtimeMapItem.instanceId))
        {
            await SetItemAimation(runtimeMapItem.animationKey, runtimeMapItem.dataId, runtimeMapItem.instanceId);
        }
    }
    public bool GetRuntimeMapItemObj(int instanceId, out RuntimeObj runtimeObj)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(instanceId, out runtimeObj))
        {
            return true;
        }
        runtimeObj = default(RuntimeObj);

        return false;
    }

    public void DeleteMapItem(DeleteMapItem deleteMapItem)
    { 
        if (nowRuntimeMapItemObjs.TryGetValue(deleteMapItem.mapItemInstanceId, out RuntimeObj RuntimeObj))
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(RuntimeObj);
            nowRuntimeMapItemObjs.Remove(deleteMapItem.mapItemInstanceId);

            RemoveRuntimePackage removeRuntimePackage = new RemoveRuntimePackage
            {
                key = new Vector2Int(WorldMapObjManager.instance.displayMap, deleteMapItem.mapItemInstanceId)
            };
            GameActionManager.instance.QueueAction(removeRuntimePackage);

            MyAnimationController.instance.RemoveItemAnimation(deleteMapItem.mapItemInstanceId);
        }
    }
    public async Task DisplayMapItem(RuntimeMapItem runtimeMapItem)
    {
        if (!nowRuntimeMapItemObjs.ContainsKey(runtimeMapItem.instanceId))
        {
            var runtimeObj = await CreatMapItemRuntime(runtimeMapItem.dataId, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
            nowRuntimeMapItemObjs[runtimeMapItem.instanceId] = runtimeObj;

            await RuntimeMapItemPlay(runtimeMapItem, runtimeObj);
        }
    }
    public async Task ChangeMapItemDisplay(int mapItemId,int newId,int2 animationKey, RuntimeMapItem runtimeMapItem)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(mapItemId, out RuntimeObj runtimeObj))
        {

            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
            {
                display = false,
                itemInstanceId = runtimeMapItem.instanceId,
            };
            GameActionManager.instance.QueueAction(displayStoreCounter, true);



            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
            var newObj = await CreatMapItemRuntime(runtimeMapItem.dataId, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
            nowRuntimeMapItemObjs[mapItemId] = newObj;

            MyAnimationController.instance.RemoveItemAnimation(mapItemId);

            Animator animator = (runtimeObj.obj as Transform).GetComponentInChildren<Animator>(true);
            if (animator)
            {
                MyAnimationController.instance.AddItemAnimation(mapItemId, animator, newId.ToString());
                await SetItemAimation(animationKey, newId, mapItemId);
            }
        }
    }
}