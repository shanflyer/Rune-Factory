using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldMapObjManager : Singleton<WorldMapObjManager>
{
    private Dictionary<int, MapItemRuntimeObj> nowRuntimeMapItemObjs = new Dictionary<int, MapItemRuntimeObj>();
    private Dictionary<int, MapItemRuntimeObj> tempRuntimeMapItemObjs = new Dictionary<int, MapItemRuntimeObj>();

    private RuntimeObj nowMapRoomObj;
    private Dictionary<int, SpriteRenderer[]> mapPackageItemRenders = new Dictionary<int, SpriteRenderer[]>();

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<TryDeleteRoom>(TryDeleteRoomObj);
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteMapItem);
        GameActionManager.instance.AddListener<DestoryTempMapItem>(DestoryTempMapItem);
        GameActionManager.instance.AddListener<DisplayMap>(DisplayMap);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpDateGameTime);
        GameActionManager.instance.AddListener<RefreshManufature>(RefreshManufature);
        GameActionManager.instance.AddListener<RefreshMapPackageItemRender>(RefreshMapPackageItemRender);
        GameActionManager.instance.AddListener<RefreshMapItemDisplay>(RefreshMapItemDisplay);
        GameActionManager.instance.AddListener<ChangeMapItemObjLayer>(ChangeMapItemObjLayer);
    }

    protected override void Clear()
    {
        base.Clear();
        nowRuntimeMapItemObjs.Clear();
        tempRuntimeMapItemObjs.Clear();
        mapPackageItemRenders.Clear();
    }

    private void ChangeMapItemObjLayer(ChangeMapItemObjLayer changeMapItemObjLayer)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(changeMapItemObjLayer.mapItemId, out var mapItemRuntimeObj))
        {
            mapItemRuntimeObj.SetLayer(changeMapItemObjLayer.layerId);
        }
    }

    private void RefreshMapItemDisplay(RefreshMapItemDisplay refreshMapItemDisplay)
    {
        if (refreshMapItemDisplay.runtimeMapItem.mapInstanceId != displayMap)
        {
            if (nowRuntimeMapItemObjs.TryGetValue(refreshMapItemDisplay.runtimeMapItem.instanceId, out var mapItemRuntimeObj))
            {
                mapItemRuntimeObj.Recycle();
                nowRuntimeMapItemObjs.Remove(refreshMapItemDisplay.runtimeMapItem.instanceId);
            }
        }
        else if (refreshMapItemDisplay.runtimeMapItem.mapInstanceId == displayMap)
        {
            if (nowRuntimeMapItemObjs.TryGetValue(refreshMapItemDisplay.runtimeMapItem.instanceId, out var mapItemRuntimeObj))
            {
                mapItemRuntimeObj.coordinate = refreshMapItemDisplay.runtimeMapItem.coordinate;
                mapItemRuntimeObj.transform.position = GameCommon.GetMapPos(refreshMapItemDisplay.runtimeMapItem.coordinate);
            }
            else
            {
                DisplayMapItem(refreshMapItemDisplay.runtimeMapItem);
            }
        }
    }

    private void TryAddMapPackageItemRender(Transform obj, int id)
    {
        if (!mapPackageItemRenders.TryGetValue(id, out var spriteRenderers))
        {
            var PackageItem = obj.GetChild(obj.childCount - 1);
            if (PackageItem.name == "PackageItem")
            {
                spriteRenderers = PackageItem.gameObject.GetComponentsInChildren<SpriteRenderer>(true);
                mapPackageItemRenders[id] = spriteRenderers;
                RefreshMapPackageItemRender(id);
            }
        }
        else
        {
            mapPackageItemRenders[id] = spriteRenderers;
            RefreshMapPackageItemRender(id);
        }
    }

    private async void RefreshMapPackageItemRender(int id)
    {
        if (mapPackageItemRenders.TryGetValue(id, out var spriteRenderers))
        {
            if (PackageManager.instance.GetPackageItemCounts(id, out var items))
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    spriteRenderers[i].enabled = false;
                }
                if (items.Count <= spriteRenderers.Length)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(items[i].x);
                        spriteRenderers[i].sprite = itemData.icon;
                        spriteRenderers[i].enabled = true;
                    }
                }
                else
                {
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(items[i].x);
                        spriteRenderers[i].sprite = itemData.icon;
                        spriteRenderers[i].enabled = true;
                    }
                }
            }
        }
    }

    private void DeleteMapPackageItemRender(int id)
    {
        mapPackageItemRenders.Remove(id);
    }

    private void RefreshMapPackageItemRender(RefreshMapPackageItemRender refreshMapPackageItemRender)
    {
        RefreshMapPackageItemRender(refreshMapPackageItemRender.linkInstanceId);
    }

    public void RefreshTempMapItem(TempMapItem tempMapItem)
    {
        if (tempMapItem.roomId == displayMap)
        {
            if (tempRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId, out var mapItemRuntimeObj))
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

    private void DestoryTempMapItem(DestoryTempMapItem destoryTempMapItem)
    {
        if (tempRuntimeMapItemObjs.TryGetValue(destoryTempMapItem.instanceId, out var mapItemRuntimeObj))
        {
            mapItemRuntimeObj.Recycle();
            tempRuntimeMapItemObjs.Remove(destoryTempMapItem.instanceId);
            if (nowRuntimeMapItemObjs.TryGetValue(destoryTempMapItem.instanceId, out var mapItemRuntimeObj1))
            {
                mapItemRuntimeObj1.ResetColor();
            }
        }
    }

    private async void CreatTempMapObjItem(TempMapItem tempMapItem)
    {
        Transform overrideParent = null;
        if (CharacterManager.instance.GetRuntimeCharacterObj(tempMapItem.characterId, out var characterRuntimeObj))
        {
            overrideParent = characterRuntimeObj.transform;
        }
        Transform ProfabTransform;
        if (nowRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId, out var mapItemRuntimeObj))
        {
            ProfabTransform = mapItemRuntimeObj.transform;
            mapItemRuntimeObj.SetColor(new Color(1, 1, 1, 0.5f));
        }
        else
        {
            MapItemData mapItemData = tempMapItem.MapItemData;
            ProfabTransform = mapItemData.itemObj.transform;
        }
        var itemObj =await GameRuntimeObjManager.instance.CreatRuntimeObj<Transform>(RuntimeObjType.MAPITEM.ToString(),
                tempMapItem.MapItemData.id.ToString(), ProfabTransform, -1, overrideParent);
        MapItemRuntimeObj tempMapItemObj = new MapItemRuntimeObj(itemObj, tempMapItem.instanceId, tempMapItem.MapItemData.id, tempMapItem.coordinate);
        tempMapItemObj.SetDefaultLayer();
        tempMapItemObj.SetCoordinate(tempMapItem.coordinate);
        tempRuntimeMapItemObjs.Add(tempMapItem.instanceId, tempMapItemObj);
        tempMapItemObj.SetColor(tempMapItem.CanSet ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f));
    }

    public void RefreshTempMapItemColor(TempMapItem tempMapItem)
    {
        if (tempRuntimeMapItemObjs.TryGetValue(tempMapItem.instanceId, out var mapItemRuntimeObj))
        {
            mapItemRuntimeObj.SetColor(tempMapItem.CanSet ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f));
        }
    }

    public int displayMap 
    {
        get; 
        set; 
    }
    public MapRoomData displayMapRoomData { get; private set; }
  
    private async Task<RuntimeObj> CreatMapRunTime(MapRoomData mapRoomData, int instanceId)
    {
        if (mapRoomData != null)
        {
            var mapRuntimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.MAPGROUND.ToString(), mapRoomData.roomName, mapRoomData.mapObj.transform, instanceId);
            return mapRuntimeObj;
        }
        return null;
    }

    private async Task<RuntimeObj> CreatMapItemRuntime(MapItemData mapItemData, int instanceId, int2 coordinate)
    {
        Vector3 pos = GameCommon.GetMapPos(coordinate);
        //pos.z = -100;

        if (mapItemData != null)
        {
           // Debug.Log($"CreatRuntimeObj:{mapItemData.itemName}");
            var mapItemRuntime =await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.MAPITEM.ToString(), mapItemData.id.ToString(), mapItemData.itemObj.transform, instanceId);
            mapItemRuntime.obj.transform.localPosition = pos;

            DisplayStoreCounter displayStoreCounter = new DisplayStoreCounter
            {
                display = true,
                itemInstanceId = instanceId,
                transform = mapItemRuntime.obj.transform
            };
            GameActionManager.instance.QueueAction(displayStoreCounter);

            return mapItemRuntime;
        }
        return null;
    }

    private void SetMapOverrideEnvirmentData(MapRoomData mapRoomData)
    {
        if (mapRoomData == null)
        {
            return;
        }

        int2 mapStartCoordinate = mapRoomData.startCoordinate;
        int2 mapEndCoordinate = mapRoomData.endCoordinate;

        var mapStartPos = GameCommon.GetMapPos(mapStartCoordinate);
        var mapEndPos = GameCommon.GetMapPos(mapEndCoordinate);

        DisplaySky displaySky = new DisplaySky
        {
            display = mapRoomData.displaySky,
            displaySunlight = mapRoomData.displaySunlight,
            skyId = mapRoomData.skyBackGroundId,
            startPos = mapStartPos,
            endPos = mapEndPos
        };
        GameActionManager.instance.QueueAction(displaySky, true);

        SetFixedCamera setFixedCamera = new SetFixedCamera
        {
            fixedCamera = mapRoomData.fixedCamera,
            fixedPos = mapRoomData.fixedCameraPos,
            flowCameraType = mapRoomData.flowCameraType
        };
        GameActionManager.instance.QueueAction(setFixedCamera, true);

        SetFixedSeason SetFixedSeason = new SetFixedSeason
        {
            season = mapRoomData.fixedSeason
        };
        GameActionManager.instance.QueueAction(SetFixedSeason);

        if (!string.IsNullOrEmpty(mapRoomData.dawnEnvironmentDataName))
        {
            SetMapOverrideEnvironment setMapOverrideEnvironment = new SetMapOverrideEnvironment
            {
                dawnEnvironmentDataName = mapRoomData.dawnEnvironmentDataName,
                dayEnvironmentDataName = mapRoomData.dayEnvironmentDataName,
                duskEnvironmentDataName = mapRoomData.duskEnvironmentDataName,
                nightEnvironmentDataName = mapRoomData.nightEnvironmentDataName
            };
            GameActionManager.instance.QueueAction(setMapOverrideEnvironment, true);
        }
        else
        {
            GameActionManager.instance.QueueAction(new ClearOverrideEnvironment(), true);
        }
    }

    private async void DisplayMap(DisplayMap displayMap)
    {
        RecycleMap();
        await DisplayMap(displayMap.displayMap);
        if (displayMap.actionId != 0)
        {
            GameActionData gameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(displayMap.actionId);
            gameActionData.Action();
        }
    }

    public async Task DisplayMap(int mapId)
    {
        if (displayMap == mapId)
        {
            return;
        }
        displayMap = mapId;
        displayMapRoomData = WorldMapManager.instance.GetWorldMap(mapId).mapRoomData;
        if (displayMapRoomData.autoCreatTempNpc)
        {
            StartCreatTempCharacter startCreatTempCharacter = new StartCreatTempCharacter
            {
                creatDataId = displayMapRoomData.creatTempCharacterId,
                clearAll = true,
                prewarm=displayMapRoomData.tempNpcPrewarm
            };
            GameActionManager.instance.QueueAction(startCreatTempCharacter);
            
            for(int i = 0; i < displayMapRoomData.specialNpcBehaviorAreas.Count; i++)
            {
                var area = displayMapRoomData.specialNpcBehaviorAreas[i];
                StartCreatSpecialTempCharacter startCreatSpecialTempCharacter = new StartCreatSpecialTempCharacter
                {
                    creatDataId = area.tempCreatId,
                    gridRange = new int4(area.grids[0] + area.pos.x, area.grids[1] + area.pos.y, area.grids[2] + area.pos.x, area.grids[3] + area.pos.y)
                };
                GameActionManager.instance.QueueAction(startCreatSpecialTempCharacter);
            }
        }
        else
        {
            ClearTempCharacter clearTempCharacter = new ClearTempCharacter();
            GameActionManager.instance.QueueAction(clearTempCharacter, true);
        } 
        string dataId = displayMapRoomData.name;
        SetMapOverrideEnvirmentData(displayMapRoomData);
        if (!string.IsNullOrEmpty(dataId))
        {
            var coordinate = MapCellController.instance.GetRoomCoordinate(mapId);
            //Vector3 pos = GameCommon.GetMapPos(coordinate.x, coordinate.y) ;
            nowMapRoomObj =await CreatMapRunTime(displayMapRoomData, mapId);
            // (nowMapRoomObj.obj as Transform).localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize);

            List<int> mapItems = WorldMapManager.instance.GetMapItems(mapId);
           
            for (int i = 0; i < mapItems.Count; i++)
            {
                if (WorldMapManager.instance.GetRuntimeMapItem(mapItems[i], out RuntimeMapItem mapItem))
                {
                   DisplayMapItem(mapItem);
                }
            }

            var tempItems = TempMapItemController.instance.GetTempMapItems(mapId);
            for (int i = 0; i < tempItems.Count; i++)
            {
                RefreshTempMapItem(tempItems[i]);
            }

            Collider2D collider2D = (nowMapRoomObj.obj as Transform).GetComponent<Collider2D>();
            CameraManager.instance.SetConfiner2DCollider(collider2D);
#if UNITY_EDITOR
            // if (MapCellTestDisplay.Instance)
            // {
            //     MapCellTestDisplay.Instance.RefreshTileMap(MapCellController.instance.GetRoomCellData(mapId));
            // }
#endif
        }

        await CharacterManager.instance.RefreshNpcRuntimeObj();
    }

    private async Task RuntimeMapItemPlay(RuntimeMapItem mapItem, RuntimeObj runtimeObj)
    {
        Animator animator = (runtimeObj.obj.transform).GetComponent<Animator>();
        if (animator)
        {
            MyAnimationController.instance.AddItemAnimation(mapItem.instanceId, animator, mapItem.mapItemData.id.ToString());
            await SetItemAimation(mapItem.animationKey, mapItem.mapItemData.id, mapItem.instanceId);
        }
    }

    /// <summary>
    /// 播放物体动画
    /// </summary>
    /// <param name="key">动画key</param>
    /// <param name="dataId">物体数据id</param>
    /// <param name="instaceId">物体实例id</param>
    /// <returns></returns>
    private async Task SetItemAimation(int2 key, int dataId, int instaceId)
    {
        var animationData = await GameDataManager.instance.GetAsyncData<ItemAnimationData>(dataId);
        if (animationData != null)
        {
            if (animationData.nativeAnimator)
            {
                if (GetRuntimeMapItemObj(instaceId, out MapItemRuntimeObj runtimeObj))
                {
                    animationData.PlayAnimator(runtimeObj.animator, key);
                }
            }
            else
            {
                AnimationClip animationClip = animationData.GetAnimationClip(key, out int count);
                MyAnimationController.instance.PlayAnimation(instaceId, animationClip);
            }
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

            DeleteMapPackageItemRender(runTimeMapItemData.Key);
            GameActionManager.instance.QueueAction(displayStoreCounter, true);
            runTimeMapItemData.Value.Recycle();
            EmoteManager.instance.TryRecycleItemEmote(runTimeMapItemData.Key);
            manufatureObjs.Remove(runTimeMapItemData.Key);
        }
        var temps = tempRuntimeMapItemObjs.Keys.ToArray();

        foreach (var runtimeObj in tempRuntimeMapItemObjs)
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

    /// <summary>
    /// 已经显示的物体播放动画
    /// </summary>
    /// <param name="runtimeMapItem"></param>
    public async void SetItemAnimation(RuntimeMapItem runtimeMapItem)
    {
        if (nowRuntimeMapItemObjs.ContainsKey(runtimeMapItem.instanceId))
        {
            await SetItemAimation(runtimeMapItem.animationKey, runtimeMapItem.mapItemData.id, runtimeMapItem.instanceId);
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

    public bool GetTempRuntimeMapItemObj(int instanceId, out MapItemRuntimeObj runtimeObj)
    {
        if (tempRuntimeMapItemObjs.TryGetValue(instanceId, out runtimeObj))
        {
            return true;
        }
        runtimeObj = default(MapItemRuntimeObj);

        return false;
    }

    public void DeleteMapItem(DeleteMapItem deleteMapItem)
    {
        if (tempRuntimeMapItemObjs.TryGetValue(deleteMapItem.mapItemInstanceId, out var tempObj))
        {
            tempObj.Recycle();
            tempRuntimeMapItemObjs.Remove(deleteMapItem.mapItemInstanceId);
        }
        if (nowRuntimeMapItemObjs.TryGetValue(deleteMapItem.mapItemInstanceId, out var RuntimeObj))
        {
            RuntimeObj.Recycle();
            nowRuntimeMapItemObjs.Remove(deleteMapItem.mapItemInstanceId);
            EmoteManager.instance.TryRecycleItemEmote(deleteMapItem.mapItemInstanceId);
            manufatureObjs.Remove(deleteMapItem.mapItemInstanceId);

            RemoveRuntimePackage removeRuntimePackage = new RemoveRuntimePackage
            {
                key = new Vector2Int(WorldMapObjManager.instance.displayMap, deleteMapItem.mapItemInstanceId)
            };
            GameActionManager.instance.QueueAction(removeRuntimePackage);
            DeleteMapPackageItemRender(deleteMapItem.mapItemInstanceId);
            MyAnimationController.instance.RemoveItemAnimation(deleteMapItem.mapItemInstanceId);
        }
    }

    private void RefreshManufature(RefreshManufature refreshManufature)
    {
        bool isDisplay = false;
        if (manufatureObjs.ContainsKey(refreshManufature.manufature.instanceId))
        {
            isDisplay = true;
        }
        else if (nowRuntimeMapItemObjs.ContainsKey(refreshManufature.manufature.instanceId))
        {
            isDisplay = true;
        }
        if (isDisplay)
        {
            if (refreshManufature.manufature.product.x == 0)
            {
                EmoteManager.instance.TryRecycleItemEmote(refreshManufature.manufature.instanceId);
            }
            else if (refreshManufature.manufature.waitTime > GameTimeManager.instance.totalMinute)
            {
                SetItemAnimation SetItemAnimation = new SetItemAnimation
                {
                    keyX = 1,
                    keyY = 0,
                    id = refreshManufature.manufature.instanceId,
                };
                GameActionManager.instance.QueueAction(SetItemAnimation);

                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = GameCommon.ManufatureWorkingEmote,
                    entityType = EntityType.地图道具,
                    id = refreshManufature.manufature.instanceId,
                };
                GameActionManager.instance.QueueAction(showEmote);
            }
            else if (refreshManufature.manufature.product.x > 0)
            {
                refreshManufature.manufature.waitTime = 0;
                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = GameCommon.ManufatureWorkendEnote,
                    entityType = EntityType.地图道具,
                    id = refreshManufature.manufature.instanceId,
                };
                GameActionManager.instance.QueueAction(showEmote);
            }
            manufatureObjs[refreshManufature.manufature.instanceId] = refreshManufature.manufature;
        }
    }

    private void UpDateGameTime(UpdateGameTime updateGameTime)
    {
        var keys = manufatureObjs.Keys.ToList();
        foreach (var m in keys)
        {
            Manufature manufature = manufatureObjs[m];
            if (manufature.product.x > 0 && manufature.waitTime > 0)
            {
                if (manufature.waitTime < updateGameTime.totalMinute)
                {
                    manufature.waitTime = 0;
                    ShowEmote showEmote = new ShowEmote
                    {
                        emoteId = 14,
                        entityType = EntityType.地图道具,
                        id = m,
                    };
                    GameActionManager.instance.QueueAction(showEmote);

                    SetItemAnimation setItemAnimation = new SetItemAnimation
                    {
                        id = manufature.instanceId,
                        keyX = 0,
                    };
                    GameActionManager.instance.QueueAction(setItemAnimation);

                    manufatureObjs[m] = manufature;
                }
            }
        }
    }

    private Dictionary<int, Manufature> manufatureObjs = new Dictionary<int, Manufature>();

    public async void DisplayMapItem(RuntimeMapItem runtimeMapItem)
    {
        if (!nowRuntimeMapItemObjs.ContainsKey(runtimeMapItem.instanceId))
        {
            var runtimeObj =await CreatMapItemRuntime(runtimeMapItem.mapItemData, runtimeMapItem.instanceId, runtimeMapItem.coordinate);

            MapItemRuntimeObj MapItemRuntimeObj = new MapItemRuntimeObj(runtimeObj, runtimeMapItem.instanceId, runtimeMapItem.mapItemData.id, runtimeMapItem.coordinate);
            nowRuntimeMapItemObjs[runtimeMapItem.instanceId] = MapItemRuntimeObj;

         

            await RuntimeMapItemPlay(runtimeMapItem, runtimeObj);

            var manufature = ManufatureManager.instance.GetManufature(runtimeMapItem.instanceId);
            if (manufature != null)
            {
                if (manufature.waitTime > GameTimeManager.instance.totalMinute)
                {
                    ShowEmote showEmote = new ShowEmote
                    {
                        emoteId = 72,
                        entityType = EntityType.地图道具,
                        id = runtimeMapItem.instanceId,
                    };
                    GameActionManager.instance.QueueAction(showEmote);
                }
                else if (manufature.product.x > 0)
                {
                    manufature.waitTime = 0;
                    ShowEmote showEmote = new ShowEmote
                    {
                        emoteId = 14,
                        entityType = EntityType.地图道具,
                        id = runtimeMapItem.instanceId,
                    };
                    GameActionManager.instance.QueueAction(showEmote);
                }
                manufatureObjs[runtimeMapItem.instanceId] = manufature;
            }

            TryAddMapPackageItemRender(runtimeObj.obj as Transform, runtimeMapItem.instanceId);
        }
    }

    public async void ChangeMapItemDisplay(int mapItemId, int newId, int2 animationKey, RuntimeMapItem runtimeMapItem)
    {
        if (nowRuntimeMapItemObjs.TryGetValue(mapItemId, out MapItemRuntimeObj runtimeObj))
        {
            if (tempRuntimeMapItemObjs.TryGetValue(mapItemId, out var tempObj))
            {
                tempObj.Recycle();
                tempRuntimeMapItemObjs.Remove(mapItemId);
            }
             

            runtimeObj.Recycle();
            var newObj =await CreatMapItemRuntime(runtimeMapItem.mapItemData, runtimeMapItem.instanceId, runtimeMapItem.coordinate);
            MapItemRuntimeObj MapItemRuntimeObj = new MapItemRuntimeObj(newObj, runtimeMapItem.instanceId, runtimeMapItem.mapItemData.id, runtimeMapItem.coordinate);
            nowRuntimeMapItemObjs[mapItemId] = MapItemRuntimeObj;

            MyAnimationController.instance.RemoveItemAnimation(mapItemId);

            Animator animator = MapItemRuntimeObj.animator;
            if (animator)
            {
                MyAnimationController.instance.AddItemAnimation(mapItemId, animator, newId.ToString());
                await SetItemAimation(animationKey, newId, mapItemId);
            }

            TryAddMapPackageItemRender(runtimeObj.animator.transform, runtimeMapItem.instanceId);
        }
    }

    public bool GetClickMapItemRuntimeObj(Vector2 clickPos, out MapItemRuntimeObj mapItemRuntimeObj)
    {
        foreach (var runtimeMapItem in nowRuntimeMapItemObjs)
        {
            if (runtimeMapItem.Value.polygonCollider2D)
            {
                if (runtimeMapItem.Value.polygonCollider2D.OverlapPoint(clickPos))
                {
                    mapItemRuntimeObj = runtimeMapItem.Value;
                    return true;
                }
            }
        }
        mapItemRuntimeObj = null;
        return false;
    }
}

public struct MapItemRuntimeObjRect
{
    public int id;
    public int2 pos;
    public int4 rect;
}

public struct CheckMapItemRuntomeObjPosJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<MapItemRuntimeObjRect> mapItemRuntimeObjRects;

    [ReadOnly]
    public int2 clickPos;

    [WriteOnly]
    public NativeArray<bool> result;

    public void Execute(int index)
    {
        var mapItemRuntimeObjRect = mapItemRuntimeObjRects[index];
        var minX = mapItemRuntimeObjRect.rect.x + mapItemRuntimeObjRect.pos.x;
        var maxX = mapItemRuntimeObjRect.rect.z + mapItemRuntimeObjRect.pos.x;
        var minY = mapItemRuntimeObjRect.rect.y + mapItemRuntimeObjRect.pos.y;
        var maxY = mapItemRuntimeObjRect.rect.w + mapItemRuntimeObjRect.pos.y;
        if (clickPos.x >= minX && clickPos.x <= maxX && clickPos.y >= minY && clickPos.y <= maxY)
        {
            result[index] = true;
        }
        else
        {
            result[index] = false;
        }
    }
}

public class MapItemRuntimeObj
{
    public int instanceId;
    public int dataId;
    private SpriteRenderer[] spriteRenderers;
    private Color[] rendererColors;
    public Animator animator;
    public Transform transform;
    public PolygonCollider2D polygonCollider2D;
    private RuntimeObj runtimeObj;
    public int2 coordinate;
    public string key => runtimeObj.key;

    public MapItemRuntimeObj(RuntimeObj runtimeObj, int instanceId, int dataId, int2 coordinate)
    {
        this.dataId = dataId;
        this.instanceId = instanceId;
        this.runtimeObj = runtimeObj;
        this.coordinate = coordinate;
        transform = (runtimeObj.obj as Transform);
        animator = transform.GetComponentInChildren<Animator>();
        spriteRenderers = transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>(true);
        rendererColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            rendererColors[i] = spriteRenderers[i].color;
        }
        transform.TryGetComponent(out polygonCollider2D);
    }

    public void SetLayer(LayerMask layerMask)
    {
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].gameObject.layer = layerMask;
            }
        }
    }

    public void SetDefaultLayer()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].gameObject.layer = 0;
        }
    }

    public void SetCoordinate(int2 coordinate)
    {
        this.coordinate = coordinate;
        transform.position = GameCommon.GetMapPos(coordinate);
    }

    public void ResetColor()
    {
        if (rendererColors.Length != spriteRenderers.Length)
        {
            return;
        }
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = rendererColors[i];
        }
    }

    public void SetColor(Color color)
    {
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
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
        if (spriteRenderers != null)
        {
            ResetColor();
            SetDefaultLayer();
        }
        spriteRenderers = null;
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        runtimeObj = null;
    }

    public void Dispose()
    {
    }
}