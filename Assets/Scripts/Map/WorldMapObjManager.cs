
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class WorldMapObjManager:Singleton<WorldMapObjManager>
{
    Dictionary<int, MapItemRuntimeObj> nowRuntimeMapItemObjs = new Dictionary<int, MapItemRuntimeObj>();
    Dictionary<int, MapItemRuntimeObj> tempRuntimeMapItemObjs = new Dictionary<int, MapItemRuntimeObj>();
     
    RuntimeObj nowMapRoomObj; 
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<TryDeleteRoom>(TryDeleteRoomObj);
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteMapItem);
        GameActionManager.instance.AddListener<DestoryTempMapItem>(DestoryTempMapItem);
        GameActionManager.instance.AddListener<DisplayMap>(DisplayMap);
    }
    protected override void Clear()
    {
        base.Clear();
        nowRuntimeMapItemObjs.Clear();
    }

    public void RefreshTempMapItem(TempMapItem tempMapItem)
    {
        if (tempMapItem.roomId == displayMap)
        {
            if(tempRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId,out var mapItemRuntimeObj))
            {
                mapItemRuntimeObj.transform.position = GameCommon.GetMapPos(tempMapItem.coordinate);
            }
            else
            {
                CreatTempMapObjItem(tempMapItem);
            }
            RefreshTempMapItemColor(tempMapItem);
        }
        else
        {
            if (tempRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId, out var mapItemRuntimeObj))
            {
                tempRuntimeMapItemObjs.Remove(tempMapItem.instanceId);
                mapItemRuntimeObj.Recycle();
            }
        }
    }
    void DestoryTempMapItem(DestoryTempMapItem destoryTempMapItem)
    {
        if(tempRuntimeMapItemObjs.TryGetValue(destoryTempMapItem.instanceId,out var mapItemRuntimeObj))
        {
            mapItemRuntimeObj.Recycle();
            tempRuntimeMapItemObjs.Remove(destoryTempMapItem.instanceId);
            if(nowRuntimeMapItemObjs.TryGetValue(destoryTempMapItem.instanceId,out var mapItemRuntimeObj1))
            {
                mapItemRuntimeObj1.ResetColor();
            }
        }
    }
    bool CreatTempMapObjItem(TempMapItem tempMapItem)
    {
        if(nowRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId,out var mapItemRuntimeObj))
        {
            Transform overrideParent = null;
            if(CharacterManager.instance.GetRuntimeCharacterObj(tempMapItem.characterId, out var characterRuntimeObj))
            {
                overrideParent = characterRuntimeObj.animator.transform;
            }

            var itemObj = GameRuntimeObjManager.instance.CreatRuntimeObj<Transform>(RuntimeObjType.MAPITEM.ToString(),
                 tempMapItem.dataId.ToString(), mapItemRuntimeObj.transform, -1, overrideParent);
            MapItemRuntimeObj tempMapItemObj = new MapItemRuntimeObj(itemObj);
            tempMapItemObj.SetCoordinate(tempMapItem.coordinate);
            tempRuntimeMapItemObjs.Add(tempMapItem.instanceId, tempMapItemObj);

            mapItemRuntimeObj.SetColor(new Color(1, 1, 1, 0.5f)); ;
            tempMapItemObj.SetColor(tempMapItem.CanSet ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f));
            return true;
        }
        return false;
    }

    public void RefreshTempMapItemColor(TempMapItem tempMapItem)
    {
        if(tempRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId,out var mapItemRuntimeObj))
        {
            mapItemRuntimeObj.SetColor(tempMapItem.CanSet ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f));
        }
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
    async void SetMapOverrideEnvirmentData(string roomName)
    {
        MapRoomData mapRoomData = await GameDataManager.instance.GetAsyncData<MapRoomData>(roomName);
        DisplaySky displaySky = new DisplaySky
        {
            display = mapRoomData.displaySky
        };
        GameActionManager.instance.QueueAction(displaySky,true);

        SetFixedCamera setFixedCamera = new SetFixedCamera
        {
            fixedCamera = mapRoomData.fixedCamera,
            fixedPos=mapRoomData.fixedCameraPos
        };
        GameActionManager.instance.QueueAction(setFixedCamera);

        if (!string.IsNullOrEmpty(mapRoomData.dawnEnvironmentDataName))
        { 
            SetMapOverrideEnvironment setMapOverrideEnvironment = new SetMapOverrideEnvironment
            {
                dawnEnvironmentDataName = mapRoomData.dawnEnvironmentDataName,
                dayEnvironmentDataName = mapRoomData.dayEnvironmentDataName,
                duskEnvironmentDataName = mapRoomData.duskEnvironmentDataName,
                nightEnvironmentDataName = mapRoomData.nightEnvironmentDataName
            };
            GameActionManager.instance.QueueAction(setMapOverrideEnvironment,true); 
        }
        else
        {
            GameActionManager.instance.QueueAction(new ClearOverrideEnvironment(), true);
        }
    }

    async void DisplayMap(DisplayMap displayMap)
    {
        if (this.displayMap != displayMap.displayMap)
        {
            RecycleMap();
            await DisplayMap(displayMap.displayMap);
            if (displayMap.actionId != 0)
            {
                GameActionData gameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(displayMap.actionId);
                gameActionData.Action();
            }
        } 
    }

    public async Task DisplayMap(int mapId)
    {
        displayMap = mapId;
        await CharacterManager.instance.RefreshNpcRuntimeObj();
        string dataId = WorldMapManager.instance.GerMapDataName(mapId);
        SetMapOverrideEnvirmentData(dataId);
        if (!string.IsNullOrEmpty(dataId))
        {
            var coordinate = MapCellController.instance.GetRoomCoordinate(mapId);
            //Vector3 pos = GameCommon.GetMapPos(coordinate.x, coordinate.y) ;
            nowMapRoomObj = await CreatMapRunTime(dataId, mapId);
           // (nowMapRoomObj.obj as Transform).localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize);
           
            PolygonCollider2D polygonCollider2D= (nowMapRoomObj.obj as Transform).GetComponent<PolygonCollider2D>();
            CameraManager.instance.SetConfiner2DCollider(polygonCollider2D);
            List<int> mapItems = WorldMapManager.instance.GetMapItems(mapId);
            for (int i = 0; i < mapItems.Count; i++)
            { 
                if (WorldMapManager.instance.GetRuntimeMapItem(mapItems[i], out RuntimeMapItem mapItem))
                {
                    if (!nowRuntimeMapItemObjs.ContainsKey(mapItem.instanceId))
                    {
                        var itemObj = await CreatMapItemRuntime(mapItem.dataId, mapItem.instanceId, mapItem.coordinate);
                        nowRuntimeMapItemObjs.Add(mapItems[i],new MapItemRuntimeObj(itemObj));

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

            var tempItems = TempMapItemController.instance.GetTempMapItems(mapId);
            for(int i = 0; i < tempItems.Count; i++)
            {
                RefreshTempMapItem(tempItems[i]);
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
        if (animationData != null)
        {
            AnimationClip animationClip = animationData.GetAnimationClip(key, out int count);
            MyAnimationController.instance.PlayAnimation(instaceId, animationClip);
        }
        
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
            runTimeMapItemData.Value.Recycle(); 
        }
        var temps = tempRuntimeMapItemObjs.Keys.ToArray();
        
        foreach(var runtimeObj in tempRuntimeMapItemObjs)
        {
            runtimeObj.Value.Recycle();
        }
        nowRuntimeMapItemObjs.Clear();
        tempRuntimeMapItemObjs.Clear();
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
    public bool GetRuntimeMapItemObj(int instanceId, out MapItemRuntimeObj runtimeObj)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(instanceId, out runtimeObj))
        {
            return true;
        }
        runtimeObj = default(MapItemRuntimeObj);

        return false;
    }

    public void DeleteMapItem(DeleteMapItem deleteMapItem)
    { 
        if(tempRuntimeMapItemObjs.TryGetValue(deleteMapItem.mapItemInstanceId,out var tempObj))
        {
            tempObj.Recycle();
            tempRuntimeMapItemObjs.Remove(deleteMapItem.mapItemInstanceId);
        }
        if (nowRuntimeMapItemObjs.TryGetValue(deleteMapItem.mapItemInstanceId, out var RuntimeObj))
        {
            RuntimeObj.Recycle();
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

            MapItemRuntimeObj MapItemRuntimeObj = new MapItemRuntimeObj(runtimeObj);
            nowRuntimeMapItemObjs[runtimeMapItem.instanceId] = MapItemRuntimeObj;

            await RuntimeMapItemPlay(runtimeMapItem, runtimeObj);
        }
    }
    public async Task ChangeMapItemDisplay(int mapItemId,int newId,int2 animationKey, RuntimeMapItem runtimeMapItem)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(mapItemId, out MapItemRuntimeObj runtimeObj))
        {
            if (tempRuntimeMapItemObjs.TryGetValue(mapItemId, out var tempObj))
            {
                tempObj.Recycle();
                tempRuntimeMapItemObjs.Remove(mapItemId);
            }

            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
            {
                display = false,
                itemInstanceId = runtimeMapItem.instanceId,
            };
            GameActionManager.instance.QueueAction(displayStoreCounter, true);

            runtimeObj.Recycle();
            var newObj = await CreatMapItemRuntime(runtimeMapItem.dataId, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
            MapItemRuntimeObj MapItemRuntimeObj=new MapItemRuntimeObj(newObj);
            nowRuntimeMapItemObjs[mapItemId] = MapItemRuntimeObj;

            MyAnimationController.instance.RemoveItemAnimation(mapItemId);

            Animator animator = MapItemRuntimeObj.animator;
            if (animator)
            {
                MyAnimationController.instance.AddItemAnimation(mapItemId, animator, newId.ToString());
                await SetItemAimation(animationKey, newId, mapItemId);
            }
        }
    }
}
public struct MapItemRuntimeObj
{
    SpriteRenderer[] spriteRenderers;
    Color[] rendererColors;
    public Animator animator;
    public Transform transform;
    RuntimeObj runtimeObj; 
    public int2 coordinate;
    public string key => runtimeObj.key;

    public MapItemRuntimeObj(RuntimeObj runtimeObj)
    {
        this.runtimeObj = runtimeObj;
        coordinate = int2.zero;
        transform = (runtimeObj.obj as Transform);
        animator = transform.GetComponentInChildren<Animator>();
        spriteRenderers = transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>(true);
        rendererColors = new Color[spriteRenderers.Length];
        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            rendererColors[i] = spriteRenderers[i].color;
        }
    }

    public void SetCoordinate(int2 coordinate)
    {
        this.coordinate = coordinate;
        transform.position= GameCommon.GetMapPos(coordinate);
    }
    public void ResetColor()
    {
        if (rendererColors.Length != spriteRenderers.Length)
        {
            return;
        }
        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = rendererColors[i];
        }
    }
    public void SetColor(Color color)
    {
        if (spriteRenderers != null)
        {
            for(int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].color = color;
            }
        }
    }
    public void AddItemAnimation()
    {

    }
    public void Recycle()
    {
        if(spriteRenderers != null)
        {
           ResetColor();
        }
        spriteRenderers = null;
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
    }
}