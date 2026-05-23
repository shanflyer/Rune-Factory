using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

//[BurstCompile]
public class RuntimeMapRoom
{
    public int2 startCoordinate, endCoordinate;
    private List<NpcBehaviorArea> NpcBehaviorAreas = new List<NpcBehaviorArea>();
    private Dictionary<BehaviorAreaType, List<int>> NpcBehaviorAreaTypeDic = new Dictionary<BehaviorAreaType, List<int>>();
    
    public int2 GetStartIndex(int2 coordinate)
    {
        return coordinate - startCoordinate;
    }
    public int3 GetRandomBehaviorCell(int areaId)
    {
        var npcBehaviorArea = NpcBehaviorAreas.Find(n => n.Name == areaId);

        if (npcBehaviorArea != null)
        {
            int gridIndex = GameRandom.RandomInt(0, npcBehaviorArea.grids.Count / 4);
            int x = GameRandom.RandomInt(npcBehaviorArea.grids[gridIndex * 4], npcBehaviorArea.grids[gridIndex * 4 + 2] + 1);
            int y = GameRandom.RandomInt(npcBehaviorArea.grids[gridIndex * 4 + 1], npcBehaviorArea.grids[gridIndex * 4 + 3] + 1);

            return new int3(x + npcBehaviorArea.pos.x, y + npcBehaviorArea.pos.y, npcBehaviorArea.Name);
        }
        return int3.zero;
    }

    public int3 GetRandomBehaviorCell(BehaviorAreaType behaviorAreaType)
    {
        if (NpcBehaviorAreaTypeDic.TryGetValue(behaviorAreaType, out var ints))
        {
            if (ints.Count > 0)
            {
                int index = GameRandom.RandomInt(0, ints.Count);
                NpcBehaviorArea npcBehaviorArea = NpcBehaviorAreas[ints[index]];

                int gridIndex = GameRandom.RandomInt(0, npcBehaviorArea.grids.Count / 4);
                int x = GameRandom.RandomInt(npcBehaviorArea.grids[gridIndex * 4], npcBehaviorArea.grids[gridIndex * 4 + 2] + 1);
                int y = GameRandom.RandomInt(npcBehaviorArea.grids[gridIndex * 4 + 1], npcBehaviorArea.grids[gridIndex * 4 + 3] + 1);

                return new int3(x + npcBehaviorArea.pos.x, y + npcBehaviorArea.pos.y, npcBehaviorArea.Name);
            }
        }
        return int3.zero;
    }

    public void SetNpcBehaviorAreas(List<NpcBehaviorArea> NpcBehaviorAreas)
    {
        this.NpcBehaviorAreas.Clear();
        NpcBehaviorAreaTypeDic.Clear();
        this.NpcBehaviorAreas = NpcBehaviorAreas;
        for (int i = 0; i < NpcBehaviorAreas.Count; i++)
        {
            NpcBehaviorArea npcBehaviorArea = NpcBehaviorAreas[i];
            if (!NpcBehaviorAreaTypeDic.TryGetValue(npcBehaviorArea.behaviorAreaType, out var ints))
            {
                ints = new List<int>();
                NpcBehaviorAreaTypeDic.Add(npcBehaviorArea.behaviorAreaType, ints);
            }
            ints.Add(i);
        }
    }
    public void AddGroundIndexData(uint index,int groundIndex)
    { 
        mapGroundIndexDatas[index] = groundIndex;
    }
    public int GetGroundIndex(uint index)
    {
        if (mapGroundIndexDatas.TryGetValue(index,out var item))
        {
            return item;
        }
        return -1;
    }
    public void Dispose()
    {
        //roomCellData.Dispose();
        NpcBehaviorAreas = null;
        NpcBehaviorAreaTypeDic?.Clear();

        // 地图房间可能在初始化失败或重复 Clear 时释放，Native 容器必须先确认已创建。
        if (linkMapIndexes.IsCreated) linkMapIndexes.Dispose();

        if (commonTriggerDatas.IsCreated) commonTriggerDatas.Dispose();
        if (commonTriggerCellIndexes.IsCreated) commonTriggerCellIndexes.Dispose();
        if (commonTriggerCells.IsCreated) commonTriggerCells.Dispose();

        if (playerTriggerAreaDatas.IsCreated) playerTriggerAreaDatas.Dispose();
        if (playerTriggerCells.IsCreated) playerTriggerCells.Dispose();
        if (playerTriggerCellIndexes.IsCreated) playerTriggerCellIndexes.Dispose();

        if (playerForwardTriggerAreaDatas.IsCreated) playerForwardTriggerAreaDatas.Dispose();
        if (playerForwardTriggerCells.IsCreated) playerForwardTriggerCells.Dispose();
        if (playerForwardTriggerIndexes.IsCreated) playerForwardTriggerIndexes.Dispose();

        if (mapGroundIndexDatas.IsCreated) mapGroundIndexDatas.Dispose();
    }

    public int id;

    public int3 coordinate; 
    // public RoomCellData roomCellData;
    public NativeHashMap<uint, int> mapGroundIndexDatas;
    public NativeHashMap<int2, int> linkMapIndexes; 
     

    /// <summary>
    /// 地图触发区域
    /// </summary>
    public NativeHashMap<int, int> commonTriggerCellIndexes;

    public NativeList<TriggerAreaData> commonTriggerDatas;
    public NativeParallelMultiHashMap<int, int2> commonTriggerCells;

    public NativeHashMap<int, int> playerTriggerCellIndexes;
    public NativeList<TriggerAreaData> playerTriggerAreaDatas;
    public NativeParallelMultiHashMap<int, int2> playerTriggerCells;

    public NativeHashMap<int, int> playerForwardTriggerIndexes;
    public NativeList<TriggerAreaData> playerForwardTriggerAreaDatas;
    public NativeParallelMultiHashMap<int, int2> playerForwardTriggerCells;

    public List<int2> GetTriggerAreaCells(int instanceId)
    {
        List<int2> cells = new List<int2>();
        if (commonTriggerCells.TryGetFirstValue(instanceId, out var cell, out var iterator))
        {
            do
            {
                if (MapCellController.instance.CheckIsWalk(cell, id)) cells.Add(cell);
            } while (commonTriggerCells.TryGetNextValue(out cell, ref iterator));
        }
        return cells;
    }

    public List<int2> GetPlayerTriggerAreaCells(int instanceId)
    {
        List<int2> cells = new List<int2>();
        if (playerTriggerCells.TryGetFirstValue(instanceId, out var cell, out var iterator))
        {
            do
            {
                if (MapCellController.instance.CheckIsWalk(cell, id)) cells.Add(cell);
            } while (playerTriggerCells.TryGetNextValue(out cell, ref iterator));
        }
        return cells;
    }

    public int Key => id;

    public void InitTriggerData()
    {
        commonTriggerCells = new NativeParallelMultiHashMap<int, int2>(1024, Allocator.Persistent);
        commonTriggerCellIndexes = new NativeHashMap<int, int>(1024, Allocator.Persistent);
        commonTriggerDatas = new NativeList<TriggerAreaData>(512, Allocator.Persistent);

        playerTriggerCells = new NativeParallelMultiHashMap<int, int2>(1024, Allocator.Persistent);
        playerTriggerCellIndexes = new NativeHashMap<int, int>(1024, Allocator.Persistent);
        playerTriggerAreaDatas = new NativeList<TriggerAreaData>(512, Allocator.Persistent);

        playerForwardTriggerCells = new NativeParallelMultiHashMap<int, int2>(1024, Allocator.Persistent);
        playerForwardTriggerIndexes = new NativeHashMap<int, int>(1024, Allocator.Persistent);
        playerForwardTriggerAreaDatas = new NativeList<TriggerAreaData>(512, Allocator.Persistent);
    }
      
}

public struct MapLinkCell
{
    public int2 coordinate;
    public int directionValue;
    public int afterAction;
    public int3 targetCell;
}

public struct SpecialLinkCell
{
    public int lineInstance;
    public int map0, map1;
    public MapLinkCell MapLinkCell0, MapLinkCell1;
}

public partial class MapCellController : Singleton<MapCellController>
{
    private const int MaxRandomBehaviorCellAttempts = 128;
    private System.Threading.Tasks.Task initializationTask = System.Threading.Tasks.Task.CompletedTask;
    public override System.Threading.Tasks.Task InitializationTask => initializationTask;

    private readonly Dictionary<int, RuntimeMapRoom> runtimeMapRooms = new();
    private NativeParallelHashMap<uint, short> mapObjBarriers;
    public NativeParallelHashMap<uint, short>.ReadOnly MapObjBarriers => mapObjBarriers.AsReadOnly();
    private Dictionary<int, int> tempMaps; 
    
    private readonly Dictionary<int, MapLine> initMapLineDic = new();
    NativeParallelMultiHashMap<int2, MapLinkCell> mapLinkCellSet;
    NativeHashMap<int3, MapLinkCell> mapLinkSet;
    private readonly Dictionary<int, HashSet<int>> mapNeighbors = new(); 
    private readonly List<SpecialLinkCell> SpecialLinkCells = new();

    public delegate void TriggerEvent(int eventId, int reference, bool enter, bool controller);

    //private Dictionary<int3, HashSet<int>> characterCells = new Dictionary<int3, HashSet<int>>();

    public const int characterRange = 5;
    public NativeList<ChangeBarrier> changeBarriers;
    public void TransTempMap(ref int mapId)
    {
        if (tempMaps.TryGetValue(mapId, out var tempMap)) mapId = tempMap;
    }
  
    public int2 GetRandomWalkable(int3 centerCoordinate, int range)
    {
        if (tempMaps.TryGetValue(centerCoordinate.z, out var trueMap)) centerCoordinate.z = trueMap;
        var cells = GetWalkableCells(centerCoordinate, range);
        if (cells.Count == 0)
        {
            return centerCoordinate.xy;
        }
        int index = GameRandom.RandomInt(0, cells.Count);
        return cells[index];
    }

    public List<int2> GetWalkableCells(int3 centerCoordinate, int range)
    {
        if (tempMaps.TryGetValue(centerCoordinate.z, out var trueMap)) centerCoordinate.z = trueMap;
        List<int2> cells = new List<int2>();

        for (int x = -range; x < range; x++)
        {
            for (int y = -range; y < range; y++)
            {
                int2 coordinate = centerCoordinate.xy + new int2(x, y);
                if (CheckIsWalk(coordinate, centerCoordinate.z))
                {
                    cells.Add(coordinate);
                }
            }
        }

        return cells;
    }


    public int2 GetStartIndex(int mapInstance,int2 coordinate)
    {
        if (tempMaps.TryGetValue(mapInstance, out var trueMap)) mapInstance = trueMap;
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            return runtimeMapRoom.GetStartIndex(coordinate);
        }
        return int2.zero;
    }
    public int3 GetRandomBehaviorCell(int mapInstance, int areaId)
    {
        var soureMap = mapInstance;
        if (tempMaps.TryGetValue(mapInstance, out var trueMap)) mapInstance = trueMap;
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            var result = runtimeMapRoom.GetRandomBehaviorCell(areaId);
            for (int i = 0; i < MaxRandomBehaviorCellAttempts && !CheckIsWalk(result.x, result.y, soureMap); i++)
            {
                result = runtimeMapRoom.GetRandomBehaviorCell(areaId);
            }
            if (!CheckIsWalk(result.x, result.y, soureMap))
            {
                Debug.LogError($"GetRandomBehaviorCell failed. map:{soureMap}, trueMap:{mapInstance}, areaId:{areaId}");
                return int3.zero;
            }
            result.z = soureMap;
            return result;
        }
        return int3.zero;
    }

    public int3 GetRandomBehaviorCell(int mapInstance, BehaviorAreaType behaviorAreaType)
    {
        var soureMap = mapInstance;
        if (tempMaps.TryGetValue(mapInstance, out var trueMap)) mapInstance = trueMap;
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            var result = runtimeMapRoom.GetRandomBehaviorCell(behaviorAreaType);
            for (int i = 0; i < MaxRandomBehaviorCellAttempts && !CheckIsWalk(result.x, result.y, soureMap); i++)
            {
                result = runtimeMapRoom.GetRandomBehaviorCell(behaviorAreaType);
            }
            if (!CheckIsWalk(result.x, result.y, soureMap))
            {
                Debug.LogError($"GetRandomBehaviorCell failed. map:{soureMap}, trueMap:{mapInstance}, areaType:{behaviorAreaType}");
                return int3.zero;
            }
            result.z = soureMap;
            return result;
        }
        return int3.zero;
    }
     
    public bool GetRandomItemTriggerCell(int roomId, int itemInstanceId, out int2 cell)
    {
        cell = int2.zero;
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        var cells = GetItemTriggerCells(itemInstanceId, roomId);
        if (cells != null)
        {
            try
            {
                var index = GameRandom.RandomInt(0, cells.Count);
                cell = cells[index];
            }
            catch (Exception exception)
            {
                Debug.LogError($"room:{roomId}-itemInstanceId{itemInstanceId}--{cells.Count}--{exception}");
            }

            return true;
        }


        return false;
    }

    public bool GetRandomItemPlayerTriggerCell(int roomId, int itemInstanceId, out int2 cell)
    {
        cell = int2.zero;
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        var cells = GetItemPlayerTriggerCells(itemInstanceId, roomId);
        if (cells != null)
        {
            try
            {
                var index = GameRandom.RandomInt(0, cells.Count);
                cell = cells[index];
            }
            catch (Exception exception)
            {
                Debug.LogError($"room:{roomId}-itemInstanceId{itemInstanceId}--{cells.Count}--{exception}");
            }
           
            return true;
        }

        
        return false;
    }

    public int2 GetNearestItemPlayerTriggerCell(int roomId, int itemInstanceId, int2 cell)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        var cells = GetItemPlayerTriggerCells(itemInstanceId, roomId);
        if (cells != null && cells.Count > 0)
        {
            int index = 0;
            int distance = int.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                int dx = cell.x - cells[i].x;
                int dy = cell.y - cells[i].y;
                int _distance = dx * dx + dy * dy;
                if (_distance < distance)
                {
                    distance = _distance;
                    index = i;
                }
            }
            return cells[index];
        }
        return cell;
    }

    public int2 GetItemCommonCenterTriggerCell(int roomId, int itemInstanceId)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        var cells = GetItemTriggerCells(itemInstanceId, roomId);
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].x < minX)
            {
                minX = cells[i].x;
            }
            if (cells[i].x > maxX)
            {
                maxX = cells[i].x;
            }
            if (cells[i].y < minY)
            {
                minY = cells[i].y;
            }
            if (cells[i].y > maxY)
            {
                maxY = cells[i].y;
            }
        }
        return new int2((minX + maxX) / 2, (minY + maxY) / 2);
    }

    public int2 GetNearestItemCommonTriggerCell(int roomId, int itemInstanceId, int2 cell)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        var cells = GetItemTriggerCells(itemInstanceId, roomId);
        if (cells != null && cells.Count > 0)
        {
            int index = 0;
            int distance = int.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                int dx = cell.x - cells[i].x;
                int dy = cell.y - cells[i].y;
                int _distance = dx * dx + dy * dy;
                if (_distance < distance)
                {
                    distance = _distance;
                    index = i;
                }
            }
            return cells[index];
        }
        return cell;
    }

    public int2 GetRandomRoomCell(int roomId)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        {
            int2 startCoordinate = runtimeMapRoom.startCoordinate;
            int2 endCoordinate = runtimeMapRoom.endCoordinate;

            while(true)
            {
                var cell = GameRandom.RandomInt2(startCoordinate, endCoordinate); 
                if (CheckIsWalk(cell, roomId))
                {
                    return cell;
                }
            } 
        }

        return new int2(int.MinValue, int.MinValue);
    }

    public int4 GetRoomRange(int roomId)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        { 
            return   new int4(runtimeMapRoom.startCoordinate, runtimeMapRoom.endCoordinate);
        }
        return int4.zero;
    }
    
   
    public List<int2> GetItemTriggerCells(int instanceId, int room)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            return runtimeMapRoom.GetTriggerAreaCells(instanceId);
        }
        return null;
    }

    public List<int2> GetItemPlayerTriggerCells(int instanceId, int room)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            var cells = runtimeMapRoom.GetPlayerTriggerAreaCells(instanceId);
            return cells;
        }
        return null;
    }

    /// <summary>
    /// 添加障碍
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="itemPos"></param>
    /// <param name="room"></param>
    public void AddBarrierCell(int2[] cells, int2 itemPos, int room)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i] + itemPos;
                if (cell.x >= runtimeMapRoom.startCoordinate.x && cell.y >= runtimeMapRoom.startCoordinate.y &&
                    cell.x <= runtimeMapRoom.endCoordinate.x && cell.y <= runtimeMapRoom.endCoordinate.y)
                    AddBarrier((uint)room, cell.x, cell.y, runtimeMapRoom.startCoordinate,
                        runtimeMapRoom.endCoordinate);


                /*
                int index = roomCellDatas[runtimeMapRoom.roomCellDataIndex].GetCoordinateIndex(cells[i] + itemPos);
                roomCellDatas[runtimeMapRoom.roomCellDataIndex].mapObjBarriers.TryGetValue(index, out int count);
                count++;
                roomCellDatas[runtimeMapRoom.roomCellDataIndex].mapObjBarriers[index] = count;*/
            }
        }
    }

    /// <summary>
    /// 移除障碍
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="itemPos"></param>
    /// <param name="room"></param>
    public void RemoveBarrierCell(int2[] cells, int2 itemPos, int room)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i] + itemPos;
                if (cell.x >= runtimeMapRoom.startCoordinate.x && cell.y >= runtimeMapRoom.startCoordinate.y &&
                    cell.x <= runtimeMapRoom.endCoordinate.x && cell.y <= runtimeMapRoom.endCoordinate.y)
                    RemoveBarrier((uint)room, cell, runtimeMapRoom.startCoordinate, runtimeMapRoom.endCoordinate);
            }
        }
    }

    /// <summary>
    /// 添加通用触发格子
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="room"></param>
    /// <param name="enterEventId"></param>
    /// <param name="exitEventId"></param>
    /// <param name="triggerType"></param>
    /// <param name="linkId"></param>
    /// <param name="offset"></param>
    public void AddTriggerCell(int2[] cells, int room, int enterEventId, int exitEventId, EntityType triggerType, int linkId, int2 offset)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            int4 triggerEvent = new int4((int)triggerType, enterEventId, exitEventId, linkId);
            TriggerAreaData triggerAreaData = new TriggerAreaData
            {
                enterLinkEventId = enterEventId,
                exitLinkEventId = exitEventId,
                referenceId = linkId,
                triggerType = triggerType
            };

            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i] + offset;
                if (cell.x >= runtimeMapRoom.startCoordinate.x && cell.y >= runtimeMapRoom.startCoordinate.y &&
                    cell.x <= runtimeMapRoom.endCoordinate.x && cell.y <= runtimeMapRoom.endCoordinate.y)
                    runtimeMapRoom.commonTriggerCells.Add(linkId, cell);
            }
            runtimeMapRoom.commonTriggerDatas.Add(triggerAreaData);
            runtimeMapRoom.commonTriggerCellIndexes.Add(linkId, runtimeMapRoom.commonTriggerDatas.Length - 1);
        }
    }

    /// <summary>
    /// 移除通用触发格子
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="room"></param>
    /// <param name="linkId"></param>
    public void RemoveTriggerCell(int room, int linkId)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            if (runtimeMapRoom.commonTriggerCellIndexes.TryGetValue(linkId, out var index))
            {
                runtimeMapRoom.commonTriggerDatas.RemoveAt(index);
                runtimeMapRoom.commonTriggerCellIndexes.Remove(linkId);
            }
            runtimeMapRoom.commonTriggerCells.Remove(linkId);
        }
    }

    /// <summary>
    /// 添加玩家交互格子
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="room"></param>
    /// <param name="enterEventId"></param>
    /// <param name="linkId"></param>
    /// <param name="offset"></param>
    public void AddPlayerTriggerCell(int2[] cells, int room, int enterEventId, int linkId, int2 offset, bool isForward)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            TriggerAreaData triggerArea = new TriggerAreaData
            {
                enterLinkEventId = enterEventId,
                referenceId = linkId,
            };

            if (isForward)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i] + offset;
                    if (cell.x >= runtimeMapRoom.startCoordinate.x && cell.y >= runtimeMapRoom.startCoordinate.y &&
                        cell.x <= runtimeMapRoom.endCoordinate.x && cell.y <= runtimeMapRoom.endCoordinate.y)
                        runtimeMapRoom.playerForwardTriggerCells.Add(linkId, cell);
                }
                runtimeMapRoom.playerForwardTriggerAreaDatas.Add(triggerArea);
                runtimeMapRoom.playerForwardTriggerIndexes.Add(linkId, runtimeMapRoom.playerTriggerAreaDatas.Length - 1);
            }
            else
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i] + offset;
                    if (cell.x >= runtimeMapRoom.startCoordinate.x && cell.y >= runtimeMapRoom.startCoordinate.y &&
                        cell.x <= runtimeMapRoom.endCoordinate.x && cell.y <= runtimeMapRoom.endCoordinate.y)
                        runtimeMapRoom.playerTriggerCells.Add(linkId, cell);
                }
                runtimeMapRoom.playerTriggerAreaDatas.Add(triggerArea);
                runtimeMapRoom.playerTriggerCellIndexes.Add(linkId, runtimeMapRoom.playerTriggerAreaDatas.Length - 1);
            }

            //runtimeMapRooms.SetData(runtimeMapRoom);
        }
    }

    /// <summary>
    /// 移除玩家交互格子
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="room"></param>
    /// <param name="linkId"></param>
    public void RemovePlayerTriggerCell(int room, int linkId)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            if (runtimeMapRoom.playerTriggerCellIndexes.TryGetValue(linkId, out var index))
            {
                runtimeMapRoom.playerTriggerAreaDatas.RemoveAt(index);
                runtimeMapRoom.playerTriggerCellIndexes.Remove(linkId);
            }

            runtimeMapRoom.playerTriggerCells.Remove(linkId);
        }
    }

    public void CheckTriggerEvent(int entityId, EntityType entityType, int room, int2 cell, bool exit, TriggerEvent triggerEvent)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            var areaCellMap = runtimeMapRoom.commonTriggerCells.AsReadOnly();
            for (var index = 0; index < runtimeMapRoom.commonTriggerDatas.Length; index++)
            {
                var triggerArea = runtimeMapRoom.commonTriggerDatas[index];
                var typeValue = (int)triggerArea.triggerType % (int)entityType;
                if (typeValue > 0) return;

                if (areaCellMap.TryGetFirstValue(triggerArea.referenceId, out var areaCell, out var it))
                    do
                    {
                        if (areaCell.Equals(cell))
                        {
                            if (exit)
                                //离开事件
                                triggerEvent(triggerArea.exitLinkEventId, triggerArea.referenceId, false, false);
                            else
                                //进入事件
                                triggerEvent(triggerArea.exitLinkEventId, triggerArea.referenceId, true, false);
                            break;
                        }
                    } while (areaCellMap.TryGetNextValue(out cell, ref it));
            } 
        }
    }

    public void CheckPlayerTriggerEvent(int room, int2 cell, bool exit,
        TriggerEvent triggerEvent, bool isForward, int oldLink = 0)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;

        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        { 
            NativeArray<int3> triggerEvents = new NativeArray<int3>
                (isForward ? runtimeMapRoom.playerForwardTriggerAreaDatas.Length : runtimeMapRoom.playerTriggerAreaDatas.Length, Allocator.Persistent);

            var TriggerAreas = isForward
                ? runtimeMapRoom.playerForwardTriggerAreaDatas
                : runtimeMapRoom.playerTriggerAreaDatas;
            var areaCellMap = isForward
                ? runtimeMapRoom.playerForwardTriggerCells.AsReadOnly()
                : runtimeMapRoom.playerTriggerCells.AsReadOnly();
            for (var index = 0; index < TriggerAreas.Length; index++)
            {
                var triggerArea = TriggerAreas[index];
                var typeValue = (int)triggerArea.triggerType % (int)EntityType.玩家;
                if (typeValue > 0) return;
                if (areaCellMap.TryGetFirstValue(triggerArea.referenceId, out var AreaCell, out var it))
                    do
                    {
                        if (AreaCell.Equals(cell))
                        {
                            if (triggerArea.referenceId == oldLink && exit)
                                //离开事件
                                triggerEvents[index] =
                                    new int3(triggerArea.exitLinkEventId, triggerArea.referenceId, 0);
                            if (!exit && triggerArea.referenceId != oldLink)
                                //进入
                                triggerEvents[index] = new int3(triggerArea.enterLinkEventId, triggerArea.referenceId,
                                    1);
                            break;
                        }
                    } while (areaCellMap.TryGetNextValue(out AreaCell, ref it));
            }
             
            List<int3> enterEventDatas = new List<int3>();
            for (var i = 0; i < triggerEvents.Length; i++)
            {
                var eventId = triggerEvents[i].x;
                if (eventId == 0 && triggerEvents[i].y == 0)
                {
                    continue;
                }
                if (triggerEvents[i].z == 1)
                {
                    enterEventDatas.Add(triggerEvents[i]);
                }
                else
                {
                    triggerEvent(eventId, triggerEvents[i].y, false, true);
                }
            }
            if (enterEventDatas.Count != 0)
            {
                //判断最近距离
                float distance = 1000;
                int3 selectEventData = enterEventDatas[0];
                for (int i = 0; i < enterEventDatas.Count; i++)
                {
                    if (WorldMapManager.instance.GetMapItemPos(enterEventDatas[i].y, out var objCoordinate))
                    {
                        float dis = CharacterManager.instance.GetDistanceController(objCoordinate.xy);
                        if (dis < distance)
                        {
                            distance = dis;
                            selectEventData = enterEventDatas[i];
                        }
                    }
                    else
                    {
                        selectEventData = enterEventDatas[i];
                        break;
                    }
                }
                triggerEvent(selectEventData.x, selectEventData.y, selectEventData.z == 1, true);
            }

            triggerEvents.Dispose();
        }
    }

    /// <summary>
    /// 检测触发事件
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="entityType"></param>
    /// <param name="room"></param>
    /// <param name="oldCell"></param>
    /// <param name="nowCell"></param>
    /// <param name="triggerEvent"></param>
    public void CheckTriggerEvent(int entityId, EntityType entityType, int room, int2 oldCell, int2 nowCell,
        TriggerEvent triggerEvent)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            var areaCellMap = runtimeMapRoom.commonTriggerCells.AsReadOnly();
            for (var index = 0; index < runtimeMapRoom.commonTriggerDatas.Length; index++)
            {
                var triggerArea = runtimeMapRoom.commonTriggerDatas[index];
                var typeValue = (int)triggerArea.triggerType % (int)entityType;
                if (typeValue > 0) return;
                var oldContanins = false;
                var nowContanins = false;
                if (!areaCellMap.ContainsKey(triggerArea.referenceId)) return;
                if (areaCellMap.TryGetFirstValue(triggerArea.referenceId, out var cell, out var it))
                    do
                    {
                        if (!oldContanins && cell.Equals(oldCell)) oldContanins = true;
                        if (!nowContanins && cell.Equals(nowCell)) nowContanins = true;
                    } while (areaCellMap.TryGetNextValue(out cell, ref it));

                if (oldContanins && !nowContanins)
                {
                    //离开事件
                    if (triggerArea.exitLinkEventId != 0)
                        triggerEvent(triggerArea.exitLinkEventId, triggerArea.referenceId, false, false);
                }
                else if (!oldContanins && nowContanins)
                {
                    //进入事件
                    if (triggerArea.exitLinkEventId != 0)
                        triggerEvent(triggerArea.exitLinkEventId, triggerArea.referenceId, true, false);  
                }
            } 
        }
    }

    /// <summary>
    /// 检测是否进入玩家交互区域
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="entityType"></param>
    /// <param name="room"></param>
    /// <param name="oldCell"></param>
    /// <param name="nowCell"></param>
    /// <param name="triggerEvent"></param>
    public void CheckPlayerTriggerEvent(int room, int2 oldCell, int2 nowCell,
        TriggerEvent triggerEvent, bool isFroward, int oldLink = 0)
    {
        if (tempMaps.TryGetValue(room, out var trueMap)) room = trueMap;
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            NativeArray<int3> triggerEvents = new NativeArray<int3>
            (isFroward ? runtimeMapRoom.playerForwardTriggerAreaDatas.Length : runtimeMapRoom.playerTriggerAreaDatas.Length,
                Allocator.Persistent);

            var TriggerAreas = isFroward
                ? runtimeMapRoom.playerForwardTriggerAreaDatas
                : runtimeMapRoom.playerTriggerAreaDatas;
            var areaCellMap = isFroward
                ? runtimeMapRoom.playerForwardTriggerCells.AsReadOnly()
                : runtimeMapRoom.playerTriggerCells.AsReadOnly();

            for (var index = 0; index < TriggerAreas.Length; index++)
            {
                var triggerArea = TriggerAreas[index];
                var typeValue = (int)triggerArea.triggerType % (int)EntityType.玩家;
                if (typeValue > 0) return;
                var oldContanins = false;
                var nowContanins = false;
                if (areaCellMap.TryGetFirstValue(triggerArea.referenceId, out var cell, out var it))
                    do
                    {
                        if (!oldContanins && cell.Equals(oldCell)) oldContanins = true;
                        if (!nowContanins && cell.Equals(nowCell)) nowContanins = true;
                    } while (areaCellMap.TryGetNextValue(out cell, ref it));

                if (triggerArea.referenceId == oldLink)
                {
                    if (oldContanins && !nowContanins)
                        //离开事件
                        triggerEvents[index] = new int3(triggerArea.exitLinkEventId, triggerArea.referenceId, 0);
                }
                else
                {
                    if (nowContanins)
                        //进入
                        triggerEvents[index] = new int3(triggerArea.enterLinkEventId, triggerArea.referenceId, 1);
                }
            }

            var length = triggerEvents.Length;
            List<int3> enterEventDatas = new List<int3>();
            for (int i = 0; i < length; i++)
            {
                var eventId = triggerEvents[i].x;
                if (eventId == 0 && triggerEvents[i].y == 0)
                {
                    continue;
                }
                if (triggerEvents[i].z == 1)
                {
                    enterEventDatas.Add(triggerEvents[i]);
                }
                else  
                {
                    triggerEvent(eventId, triggerEvents[i].y, false, true);
                }
            }
            if (enterEventDatas.Count != 0)
            {
                //判断最近距离
                float distance = 1000;
                int3 selectEventData = enterEventDatas[0];
                for (int i = 0; i < enterEventDatas.Count; i++)
                {
                    if (WorldMapManager.instance.GetMapItemPos(enterEventDatas[i].y, out var objCoordinate))
                    {
                        float dis = CharacterManager.instance.GetDistanceController(objCoordinate.xy);
                        if (dis < distance)
                        {
                            distance = dis;
                            selectEventData = enterEventDatas[i];
                        }
                    }
                    else
                    {
                        selectEventData = enterEventDatas[i];
                        break;
                    }
                }
                triggerEvent(selectEventData.x, selectEventData.y, selectEventData.z == 1, true);
            }

            triggerEvents.Dispose();
        }
    }

    public bool GetCoordinates(int mapInstance, int2 source, int minRange, int maxRange, bool isWalkable, out List<int2> result)
    {
        if (tempMaps.TryGetValue(mapInstance, out var trueMap)) mapInstance = trueMap;
        if (GetRuntimeMapRoom(mapInstance, out var runtimeMapRoom))
        {
            List<int2> result1 = new List<int2>();
            uint cellCount = GetCoordinateIndex(runtimeMapRoom.endCoordinate.x, runtimeMapRoom.endCoordinate.y, runtimeMapRoom.startCoordinate,
                runtimeMapRoom.endCoordinate);
            for (int x = -maxRange; x <= maxRange; x++)
            {
                for (int y = -maxRange; y <= maxRange - x; y++)
                {
                    if (math.abs(x) + math.abs(y) > maxRange || math.abs(x) + math.abs(y) < minRange)
                    {
                        continue;
                    }
                    int2 coordinate = source + new int2(x, y);
                    uint index = GetCoordinateIndex(coordinate.x,coordinate.y, runtimeMapRoom.startCoordinate,runtimeMapRoom.endCoordinate);
                    if (index >= 0 && index < cellCount)
                    {
                        if (CheckIsWalk(coordinate,mapInstance) == isWalkable)
                        {
                            result1.Add(coordinate);
                        }
                    }
                }
            }
             result=result1;
             
            return true;
        }
        result = null;
        return false;
    }

    public bool GetRuntimeMapRoom(int roomId, out RuntimeMapRoom runtimeMapRoom)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        return runtimeMapRooms.TryGetValue(roomId, out runtimeMapRoom);
    }

    public bool ContainsRoom(int roomId)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        return runtimeMapRooms.ContainsKey(roomId);
    }

    public int3 GetRoomCoordinate(int roomId)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        {
            return runtimeMapRoom.coordinate;
        }
        return int3.zero;
    }
 
#if UNITY_EDITOR

    public List<Vector3Int> GetAllCellData(int mapInstance)
    {
        var sourceId = mapInstance;
        if (tempMaps.TryGetValue(mapInstance, out var trueMap)) mapInstance = trueMap;
        List<Vector3Int> cellData = new List<Vector3Int>();
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            for(int x = runtimeMapRoom.startCoordinate.x;x<= runtimeMapRoom.endCoordinate.x; x++)
            {
                for (int y = runtimeMapRoom.startCoordinate.y; y <= runtimeMapRoom.endCoordinate.y; y++)
                {
                    int z = 1;
                    if (CheckIsWalk(x, y, mapInstance))
                    {
                        z = 0;
                    }
                    cellData.Add(new Vector3Int(x, y, z));
                }
            } 
        }
        return cellData;
    }

#endif

    public int2[] GetPlayerTrigger(int roomId)
    {
        if (tempMaps.TryGetValue(roomId, out var trueMap)) roomId = trueMap;
        int2[] result = null;
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        {
            var cells = runtimeMapRoom.playerTriggerCells.GetValueArray(Allocator.Temp);
            result = cells.ToArray();
            cells.Dispose();
        }
        return result;
    }
    public uint GetCoordinateIndex(int x, int y,int2 startCoordinate, int2 endCoordinate)
    {
        int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
        int index = (y - startCoordinate.y) + (x - startCoordinate.x) * perRowGridCount;
        return (uint)index;
    }
    void AddBarrier(uint mapId, int x, int y, int2 startCoordinate, int2 endCoordinate)
    {
        if (tempMaps.TryGetValue((int)mapId, out var trueMap)) mapId = (uint)trueMap;
        uint index = GetCoordinateIndex(x, y,startCoordinate,endCoordinate);
        index = index +mapId* 1000_000;
        changeBarriers.Add(new ChangeBarrier { index = index, add = true });

        /*
        mapObjBarriers.TryGetValue(index, out var count);
        count++;
        mapObjBarriers[index] = count; */
    }
    
  

    public void RemoveBarrier(uint mapId, int2 coordinate, int2 startCoordinate, int2 endCoordinate)
    {
        if (tempMaps.TryGetValue((int)mapId, out var trueMap)) mapId = (uint)trueMap;
        uint index = GetCoordinateIndex(coordinate.x,coordinate.y,startCoordinate,endCoordinate);
        index = index + mapId * 1000_000;
        changeBarriers.Add(new ChangeBarrier { index = index, add = false });
        /*
        mapObjBarriers.TryGetValue(index, out var count);
        count--;
        if (count <= 0)
            mapObjBarriers.Remove(index);
        else
            mapObjBarriers[index] = count;*/
    }

    public void ChangeMapBarrierAction()
    {
        for (var i = 0; i < changeBarriers.Length; i++)
        {
            var changeBarrier = changeBarriers[i];
            mapObjBarriers.TryGetValue(changeBarrier.index, out var count);
            if (changeBarrier.add)
                count++;
            else
                count--;

            if (count <= 0)
                mapObjBarriers.Remove(changeBarrier.index);
            else
                mapObjBarriers[changeBarrier.index] = count;
        }

        changeBarriers.Clear();
    }

    public void InitMapData(int roomId, MapRoomData mapRoomData, int3 coordinate)
    { 
        var barrierGridCount = mapRoomData.barrierGrids.Count / 4;
        for (int j = 0; j < barrierGridCount; j++)
        {
            int minX = mapRoomData.barrierGrids[j * 4];
            int minY = mapRoomData.barrierGrids[j * 4 + 1];
            int maxX = mapRoomData.barrierGrids[j * 4 + 2];
            int maxY = mapRoomData.barrierGrids[j * 4 + 3];

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    AddBarrier((uint)roomId, x, y, mapRoomData.startCoordinate, mapRoomData.endCoordinate); 
                }
            }
        }

         
        RuntimeMapRoom runtimeMapRoom = new RuntimeMapRoom
        {
            startCoordinate= mapRoomData.startCoordinate,
            endCoordinate=mapRoomData.endCoordinate,
            coordinate = coordinate, 
            id = roomId,
            mapGroundIndexDatas = new NativeHashMap<uint, int>(16, Allocator.Persistent),
            linkMapIndexes = new NativeHashMap<int2, int>(16, Allocator.Persistent), 
        };

        var groundGridCount = mapRoomData.groundGrids.Count / 4;
        for (int j = 0; j < groundGridCount; j++)
        {
            int minX = mapRoomData.groundGrids[j * 4];
            int minY = mapRoomData.groundGrids[j * 4 + 1];
            int maxX = mapRoomData.groundGrids[j * 4 + 2];
            int maxY = mapRoomData.groundGrids[j * 4 + 3];

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var index = GetCoordinateIndex(x, y, mapRoomData.startCoordinate, mapRoomData.endCoordinate);
                    runtimeMapRoom.AddGroundIndexData(index, mapRoomData.groundIndexes[j]);
                }
            }
        }

        runtimeMapRoom.SetNpcBehaviorAreas(mapRoomData.npcBehaviorAreas);
        runtimeMapRoom.InitTriggerData();
        runtimeMapRooms.Add(runtimeMapRoom.Key, runtimeMapRoom);
    }
 
    public bool GetLinkMapInCoordinate(int startMap, int endMap, int nowMap, int linkMap, int2 nowCoordinate,
        int2 endCoordinate,
        out int4 changeCoordinate)
    {
        if (tempMaps.TryGetValue(startMap, out var trueMap)) startMap = trueMap;
        if (tempMaps.TryGetValue(endMap, out trueMap)) endMap = trueMap;
        if (tempMaps.TryGetValue(nowMap, out trueMap)) nowMap = trueMap;
        if (tempMaps.TryGetValue(linkMap, out trueMap)) linkMap = trueMap;

        if (GetSpecialLink(startMap, endMap, nowMap, nowCoordinate, linkMap, endCoordinate,
                out changeCoordinate)) return true;

        var key = new int2(nowMap, linkMap);
        changeCoordinate = int4.zero;
        var linkCells = new List<MapLinkCell>();
        if (mapLinkCellSet.TryGetFirstValue(key, out var mapLinkCell, out var it))
            do
            {
                if (CheckIsWalk(new int3(mapLinkCell.coordinate, nowMap))) linkCells.Add(mapLinkCell);
            } while (mapLinkCellSet.TryGetNextValue(out mapLinkCell, ref it));

        if (linkCells.Count > 0)
        {
            var cell = linkCells[GameRandom.RandomInt(0, linkCells.Count)];
            var inCoordinate = cell.coordinate;
            var targetCoordinate = cell.targetCell.xy;
            changeCoordinate = new int4(inCoordinate, targetCoordinate);
            return true;
        }

        return false;
    }
 
    public bool ChangeMapAction(int2 nowCoordinate, Direction direction, int nowMap, Int3Action action,bool isPlayer)
    {
        if (tempMaps.TryGetValue(nowMap, out var _trueMap)) nowMap = _trueMap;
        int3 key = new int3(nowCoordinate.xy, nowMap);
        if (mapLinkSet.TryGetValue(key, out var mapLinkCell))
        {
            if (direction == Direction.Default|| DirectionMask.Has(mapLinkCell.directionValue, direction))
            {
                ChangeMap();
                async void ChangeMap()
                {
                    action.Invoke(mapLinkCell.targetCell, mapLinkCell.afterAction);
                    /*
                    if (mapLinkCell.afterAction != 0&& isPlayer)
                    {
                        var actionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(mapLinkCell.afterAction);
                        actionData.Action(setResult: (bool value) =>
                        {
                            action.Invoke(mapLinkCell.targetCell, mapLinkCell.afterAction);
                        });
                    }
                    else
                    {
                        action.Invoke(mapLinkCell.targetCell, mapLinkCell.afterAction);
                    }*/
                }
                return true;
            }
        } 
        return false;
    }

    public void InitLinkMap(List<MapLine> mapLines)
    {
        foreach (var mapLine in mapLines)
        {
            bool isInit = mapLine.zeroInit;
            if (!GameController.instance.startPlay)
            {
                int saveValue = GameDataSaveManager.instance.CheckMapLine(mapLine.instanceId);
                switch (saveValue)
                {
                    case -1:
                        isInit = mapLine.zeroInit;

                        break;

                    case 0:
                        isInit = false;
                        break;

                    case 1:
                        isInit = true;
                        break;
                }
            }
            else
            {
                if ( mapLine.testInit)
                {
                    isInit = true;
                }
            }
           
            if (!isInit)
            {
                continue;
            }
           // Debug.Log($"mapLine:{mapLine.map0}--{mapLine.map1}");
            InitLinkMap(mapLine);
        }
    }

    public void DeleteMapLink(MapLine mapLine)
    {
        var key0 = new int2(mapLine.Map0, mapLine.Map1);
        var key1 = new int2(mapLine.Map1, mapLine.Map0);
        HashSet<int> removeIt = new HashSet<int>();
        if(mapLinkCellSet.TryGetFirstValue(key0,out var item,out var it))
        {
            do
            {
                var key = new int3(item.coordinate.xy, mapLine.Map0); 
                mapLinkSet.Remove(key);

            } while (mapLinkCellSet.TryGetNextValue(out item, ref it));
        }
        if (mapLinkCellSet.TryGetFirstValue(key1, out  item, out  it))
        {
            do
            {
                var key = new int3(item.coordinate.xy, mapLine.Map1);
                mapLinkSet.Remove(key);

            } while (mapLinkCellSet.TryGetNextValue(out item, ref it));
        }
        mapLinkCellSet.Remove(key0);
        mapLinkCellSet.Remove(key1);

        if (mapNeighbors.TryGetValue(mapLine.Map0, out var ints))
        {
            ints.Remove(mapLine.Map1);
            mapNeighbors.Remove(mapLine.Map0);
        }

        if (mapNeighbors.TryGetValue(mapLine.Map1, out ints))
        {
            ints.Remove(mapLine.Map0);
            mapNeighbors.Remove(mapLine.Map1);
        } 
    }

    public void InitLinkMap(MapLine mapLine)
    {
        initMapLineDic[mapLine.instanceId] = mapLine;
        var isSpecial = IsSpecialLink(mapLine.instanceId, out var specialMapLink);
        var key0 = new int2(mapLine.Map0, mapLine.Map1);
        var key1 = new int2(mapLine.Map1, mapLine.Map0);

        int dir0=0;
        int dir1 = 0;
        for(int i = 0; i < mapLine.cells0.directions.Count; i++)
        {
            dir0= DirectionMask.Add(dir0,mapLine.cells0.directions[i]);
        }

        if (isSpecial)
        {
            var mapLinkCell0 = new MapLinkCell
            {
                coordinate = GameCommon.GridCenter(mapLine.cells0.girds),
                directionValue = dir0,
                afterAction = mapLine.cells0.afterAction,
                targetCell = mapLine.cells0.targetCell
            };
            var mapLinkCell1 = new MapLinkCell
            {
                coordinate = GameCommon.GridCenter(mapLine.cells1.girds),
                directionValue = dir0,
                afterAction = mapLine.cells0.afterAction,
                targetCell = mapLine.cells0.targetCell
            };
            var specialLinkCell = new SpecialLinkCell
            {
                lineInstance = mapLine.instanceId,
                map0 = mapLine.Map0, map1 = mapLine.Map1,
                MapLinkCell0 = mapLinkCell0,
                MapLinkCell1 = mapLinkCell1
            };
            SpecialLinkCells.Add(specialLinkCell);
        }
        
        var cells0 = GameCommon.GridToCells(mapLine.cells0.girds);
        for (int i = 0; i < cells0.Count; i++)
        {
            MapLinkCell mapLinkCell = new MapLinkCell
            {
                coordinate = cells0[i],
                directionValue = dir0,
                afterAction = mapLine.cells0.afterAction,
                targetCell = mapLine.cells0.targetCell,
            };
            if (!isSpecial) mapLinkCellSet.Add(key0, mapLinkCell);

            mapLinkSet.Add(new int3(cells0[i].xy, mapLine.Map0), mapLinkCell); 
           
        }

        if (!mapNeighbors.TryGetValue(mapLine.Map0, out var ints))
        {
            ints = new HashSet<int>();
            mapNeighbors.Add(mapLine.Map0, ints);
        }

        ints.Add(mapLine.Map1);

        if (!mapNeighbors.TryGetValue(mapLine.Map1, out var ints1))
        {
            ints1 = new HashSet<int>();
            mapNeighbors.Add(mapLine.Map1, ints1); 
        }

        ints1.Add(mapLine.Map0);

        var cells1 = GameCommon.GridToCells(mapLine.cells1.girds);
        for (int i = 0; i < mapLine.cells1.directions.Count; i++)
        {
            dir1 = DirectionMask.Add(dir1, mapLine.cells1.directions[i]);
        }
        for (int i = 0; i < cells1.Count; i++)
        {
            MapLinkCell mapLinkCell = new MapLinkCell
            {
                coordinate = cells1[i],
                directionValue = dir1,
                afterAction = mapLine.cells1.afterAction,
                targetCell = mapLine.cells1.targetCell,
            };
            if (!isSpecial) mapLinkCellSet.Add(key1, mapLinkCell);

            mapLinkSet.Add(new int3(cells1[i].xy, mapLine.Map1), mapLinkCell);
        }
         
    }

    public void FindPathNodeNearest(int2 startPos, int2 targetPos, int mapId,MoveWithPath moveWithPath)
    {
        if (tempMaps.TryGetValue(mapId, out var _trueMap)) mapId = _trueMap;
        MapCellJobController.instance.AddPathRequest(startPos, targetPos, mapId,
            (Stack<int2> outData, int map, int2 start, int2 end) =>
        {
            if (outData.Count == 0)
            {
                float2 direct = math.normalize(startPos - targetPos);
                int index = 1;
                int2 offsetCoordinate = int2.zero;
                int2 _targetPos = targetPos;
                while (!_targetPos.Equals(startPos))
                {
                    float2 offsetPos = direct * GameCommon.cellWidth * index;
                    int2 _offsetCoordinate = GameCommon.GetMapCoordinateInt(offsetPos);
                    if (!_offsetCoordinate.Equals(offsetCoordinate))
                    {
                        offsetCoordinate = _offsetCoordinate;
                        _targetPos = targetPos + offsetCoordinate;
                        MapCellJobController.instance.AddPathRequest(startPos, _targetPos, mapId,
                            (Stack<int2> outData1, int map, int2 start, int2 end) =>
                        {
                            if (outData1.Count != 0)
                            {
                                moveWithPath.Invoke(outData1, map, start, end);
                            }
                        }); 
                    }
                    index++;
                }
            }

            moveWithPath.Invoke(outData, map, start, end);
        }); 
    }
 
    public int GetGroundIndex(int2 coordinate, int mapId)
    {
        if (tempMaps.TryGetValue(mapId, out var _trueMap)) mapId = _trueMap;
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            uint index = GetCoordinateIndex(coordinate.x, coordinate.y, runtimeMapRoom.startCoordinate, runtimeMapRoom.endCoordinate);
            return runtimeMapRoom.GetGroundIndex(index);
        }
        return -1;
    }

 
    public bool CheckIsWalk(int3 coordinate)
    {
        if (tempMaps.TryGetValue(coordinate.z, out var _trueMap)) coordinate.z = _trueMap;
        if (runtimeMapRooms.TryGetValue(coordinate.z, out RuntimeMapRoom runtimeMapRoom))
        {
            if (runtimeMapRoom.startCoordinate.x <= coordinate.x && runtimeMapRoom.startCoordinate.y <= coordinate.y &&
                runtimeMapRoom.endCoordinate.x >= coordinate.x && runtimeMapRoom.endCoordinate.y >= coordinate.y)
            {
                var index = GetCoordinateIndex(coordinate.x, coordinate.y, runtimeMapRoom.startCoordinate,
                    runtimeMapRoom.endCoordinate);
                index = index + (uint)coordinate.z * 1000_000;
                return !mapObjBarriers.ContainsKey(index);
            }
          
        }
        return false;
    }
     
    public bool CheckIsWalk(int2 coordinate,int mapId)
    {
        if (tempMaps.TryGetValue(mapId, out var _trueMap)) mapId = _trueMap;
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            if (runtimeMapRoom.startCoordinate.x <= coordinate.x && runtimeMapRoom.startCoordinate.y <= coordinate.y &&
                runtimeMapRoom.endCoordinate.x >= coordinate.x && runtimeMapRoom.endCoordinate.y >= coordinate.y)
            {
                var index = GetCoordinateIndex(coordinate.x, coordinate.y, runtimeMapRoom.startCoordinate,
                    runtimeMapRoom.endCoordinate);
                index = index + (uint)mapId * 1000_000;
                return !mapObjBarriers.ContainsKey(index);
            }
           
        }
        return false;
    }
    public bool CheckIsWalk(int x,int y, int mapId)
    {
        if (tempMaps.TryGetValue(mapId, out var _trueMap)) mapId = _trueMap;
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            if (runtimeMapRoom.startCoordinate.x <= x && runtimeMapRoom.startCoordinate.y <= y &&
                runtimeMapRoom.endCoordinate.x >= x && runtimeMapRoom.endCoordinate.y >= y)
            {
                var index = GetCoordinateIndex(x, y, runtimeMapRoom.startCoordinate, runtimeMapRoom.endCoordinate);
                index = index + (uint)mapId * 1000_000;
                return !mapObjBarriers.ContainsKey(index);
            }
           
        }
        return false;
    }

    public bool CheckFutureIsWalk(HashSet<int2> cells, int mapId)
    {
        if (tempMaps.TryGetValue(mapId, out var _trueMap)) mapId = _trueMap;
        if (runtimeMapRooms.TryGetValue(mapId, out var runtimeMapRoom))
        {
            using (var e = cells.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var cell = e.Current;
                    if (!CheckIsWalk(cell,mapId))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        return false;
    }
     

    private HashSet<int> GetRoomNeighbors(int roomId)
    {
        if (tempMaps.TryGetValue(roomId, out var _trueMap)) roomId = _trueMap;
        if(mapNeighbors.TryGetValue(roomId,out var ints))
        {
            return ints;
        }

        return null;
    }

    
    public bool FindRoomList(int sourceId, int targetId, out List<int> roomList)
    { 
         roomList = new List<int>();
        if (sourceId == targetId)
        {
            return true;
        }
         
        Dictionary<int, int> links = new Dictionary<int, int>();
        List<int> nowList = new List<int>();
        HashSet<int> checkRoom = new HashSet<int>();
        checkRoom.Add(targetId);
        nowList.Add(targetId);

        bool result = false;
        for (int i = 0; i < nowList.Count; i++)
        {
            int checkId = nowList[i];
            var Neighbours = GetRoomNeighbors(checkId);
            if (Neighbours == null)
            {
                // Debug.Log($"Neighbours.IsEmpty:{checkId}");
                return false; 
            }

            foreach (var neighbour in Neighbours)
            {
                // Debug.Log($"checkId:{checkId}  - Neighbours:{neighbour.Key}");
                if (!checkRoom.Contains(neighbour))
                {
                    links[neighbour] = checkId;
                    checkRoom.Add(neighbour);
                    nowList.Add(neighbour);
                    if (sourceId == neighbour)
                    {
                        result = true;
                        break;
                    }
                }
            }

            if (result)
            {
                break;
            }
        }

        if (result)
        {
            int _roomId = sourceId;
            //roomList.Enqueue(_roomId);
            while (true)
            {
                if (links.TryGetValue(_roomId, out _roomId))
                {
                    roomList.Add(_roomId);
                }
                if (_roomId == targetId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<SpecialMapLink> allSpecialLink;

    private bool IsSpecialLink(int lineId, out SpecialMapLink SpecialMapLink)
    {
        for (var i = 0; i < allSpecialLink.Count; i++)
            if (allSpecialLink[i].specialId == lineId)
            {
                SpecialMapLink = allSpecialLink[i];
                return true;
            }

        SpecialMapLink = null;
        return false;
    }

    public bool GetSpecialLink(int startMap, int endMap, int nowMap, int2 nowCoordinate, int nextMap,
        int2 nextCoordinate,
        out int4 changeCoordinate)
    {
        if (tempMaps.TryGetValue(startMap, out var trueMap)) startMap = trueMap;
        if (tempMaps.TryGetValue(endMap, out trueMap)) endMap = trueMap;
        if (tempMaps.TryGetValue(nowMap, out trueMap)) nowMap = trueMap;
        if (tempMaps.TryGetValue(nextMap, out trueMap)) nextMap = trueMap;
        for (var i = 0; i < allSpecialLink.Count; i++)
        {
            var specialLink = allSpecialLink[i];
            if (specialLink.IsMatchTarget(startMap) && specialLink.map0.specialMap == nextMap)
                if (initMapLineDic.TryGetValue(specialLink.specialId, out var mapLine))
                {
                    if (mapLine.Map0 == nowMap)
                        changeCoordinate = new int4(mapLine.start0, mapLine.center1);
                    else
                        changeCoordinate = new int4(mapLine.start1, mapLine.center0);

                    return true;
                }

            if (specialLink.IsMatchTarget(endMap) && specialLink.map0.specialMap == nowMap)
                if (initMapLineDic.TryGetValue(specialLink.specialId, out var mapLine))
                {
                    if (mapLine.Map0 == nowMap)
                        changeCoordinate = new int4(mapLine.start0, mapLine.center1);
                    else
                        changeCoordinate = new int4(mapLine.start1, mapLine.center0);
                    return true;
                }

            if (specialLink.map0.specialMap == nowMap)
                if (specialLink.map0.IsInArea(nowCoordinate) &&
                    specialLink.IsMatchTargetMap(nextMap, nextCoordinate, out var map))
                    if (initMapLineDic.TryGetValue(specialLink.specialId, out var mapLine))
                    {
                        if (mapLine.Map0 == nowMap)
                            changeCoordinate = new int4(mapLine.start0, mapLine.center1);
                        else
                            changeCoordinate = new int4(mapLine.start1, mapLine.center0);
                        return true;
                    }


            if (specialLink.map0.specialMap == nextMap)
                if (specialLink.map0.IsInArea(nextCoordinate) &&
                    specialLink.IsMatchTargetMap(nowMap, nowCoordinate, out var map))
                    if (initMapLineDic.TryGetValue(specialLink.specialId, out var mapLine))
                    {
                        if (mapLine.Map0 == nowMap)
                            changeCoordinate = new int4(mapLine.start0, mapLine.center1);
                        else
                            changeCoordinate = new int4(mapLine.start1, mapLine.center0);
                        return true;
                    }
        }

        changeCoordinate = int4.zero;
        return false;
    }

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async System.Threading.Tasks.Task InitAsync()
    {
        tempMaps = new Dictionary<int, int>();
        var allTempMap = await GameDataManager.instance.GetAllAsyncData<TempMapData>();
        for (var i = 0; i < allTempMap.Count; i++)
        {
            var tempMap = allTempMap[i];
            tempMaps.Add(tempMap.tempMapId, tempMap.tempMapId);
        }
        
        allSpecialLink = await GameDataManager.instance.GetAllAsyncData<SpecialMapLink>();
        
        GameActionManager.instance.AddListener<RemoveCellCharacter>(RemoveCellCharacter);
        mapObjBarriers = new NativeParallelHashMap<uint, short>(204800, Allocator.Persistent);
         
        mapLinkCellSet = new NativeParallelMultiHashMap<int2, MapLinkCell>(2048, Allocator.Persistent);
        mapLinkSet = new NativeHashMap<int3, MapLinkCell>(2048, Allocator.Persistent);
        changeBarriers = new NativeList<ChangeBarrier>(512, Allocator.Persistent);
    }
 
    public bool CheckTryMoveTarget(int2 startCoordinate, int2 targetCoordinate, int mapInstace)
    {
        if (tempMaps.TryGetValue(mapInstace, out var trueMap)) mapInstace = trueMap;
        if (runtimeMapRooms.TryGetValue(mapInstace, out var runtimeMapRoom))
        {
            int dx = targetCoordinate.x - startCoordinate.x;
            int dy = targetCoordinate.y - startCoordinate.y;
            int absDx = math.abs(dx);
            int absDy = math.abs(dy);
            if (absDx == absDy && absDx != 0)
            {
                int addX = targetCoordinate.x < startCoordinate.x ? -1 : 1;
                int addY = targetCoordinate.y < startCoordinate.y ? -1 : 1;

                int nowX = startCoordinate.x + addX;
                int nowY = startCoordinate.y + addY;
                return CheckIsWalk(nowX, nowY,mapInstace);
            }
            else
            if (absDx > absDy)
            {
                float perAddy = absDy / (float)absDx;
                int nowX = startCoordinate.x;
                int nowY = startCoordinate.y;
                float trueNowY = startCoordinate.y;
                int addX = targetCoordinate.x < startCoordinate.x ? -1 : 1;
                perAddy *= targetCoordinate.y < startCoordinate.y ? -1 : 1;

                nowX = startCoordinate.x + addX;
                trueNowY = trueNowY + perAddy;
                nowY = perAddy < 0 ? (int)math.ceil(trueNowY) : (int)math.floor(trueNowY);
                return CheckIsWalk(nowX, nowY, mapInstace);
            }
            else if (absDx < absDy)
            {
                float perAddx = absDx / (float)absDy;
                int nowX = startCoordinate.x;
                int nowY = startCoordinate.y;
                float trueNowX = startCoordinate.x;
                int addY = targetCoordinate.y < startCoordinate.y ? -1 : 1;
                perAddx *= targetCoordinate.x < startCoordinate.x ? -1 : 1;

                nowY = startCoordinate.y + addY;
                trueNowX = trueNowX + perAddx;
                nowX = perAddx < 0 ? (int)math.ceil(trueNowX) : (int)math.floor(trueNowX);
                return CheckIsWalk(nowX, nowY, mapInstace);
            }
        }
        return false;
    }

    public int2 GetTrueFreedomTarget(int2 startCoordinate, int2 targetCoordinate, int mapInstace)
    {
        if (tempMaps.TryGetValue(mapInstace, out var trueMap)) mapInstace = trueMap;
        int2 result = startCoordinate;
        if (runtimeMapRooms.TryGetValue(mapInstace, out var runtimeMapRoom))
        {
            int dx = targetCoordinate.x - startCoordinate.x;
            int dy = targetCoordinate.y - startCoordinate.y;
            int absDx = math.abs(dx);
            int absDy = math.abs(dy);
            if (absDx == absDy && absDx != 0)
            {
                int nowX = startCoordinate.x;
                int nowY = startCoordinate.y;
                int addX = targetCoordinate.x < startCoordinate.x ? -1 : 1;
                int addY = targetCoordinate.y < startCoordinate.y ? -1 : 1;
                for (int i = 0; i <= absDx; i++)
                {
                    nowX = startCoordinate.x + addX;
                    nowY = startCoordinate.y + addY;
                    if (!CheckIsWalk(nowX, nowY,mapInstace))
                    {
                        break;
                    }
                    else
                    {
                        result.x = nowX;
                        result.y = nowY;
                    }
                }
            }
            else
            if (absDx > absDy)
            {
                float perAddy = absDy / (float)absDx;
                int nowX = startCoordinate.x;
                int nowY = startCoordinate.y;
                float trueNowY = startCoordinate.y;
                int addX = targetCoordinate.x < startCoordinate.x ? -1 : 1;
                perAddy *= targetCoordinate.y < startCoordinate.y ? -1 : 1;
                for (int x = 0; x <= absDx; x++)
                {
                    nowX = startCoordinate.x + addX;
                    trueNowY = trueNowY + perAddy;
                    if (x == absDx)
                    {
                        nowY = targetCoordinate.y;
                    }
                    else
                    {
                        nowY = perAddy < 0 ? (int)math.ceil(trueNowY) : (int)math.floor(trueNowY);
                    }
                    if (!CheckIsWalk(nowX, nowY,mapInstace))
                    {
                        break;
                    }
                    else
                    {
                        result.x = nowX;
                        result.y = nowY;
                    }
                }
            }
            else if (absDx < absDy)
            {
                float perAddx = absDx / (float)absDy;
                int nowX = startCoordinate.x;
                int nowY = startCoordinate.y;
                float trueNowX = startCoordinate.x;
                int addY = targetCoordinate.y < startCoordinate.y ? -1 : 1;
                perAddx *= targetCoordinate.x < startCoordinate.x ? -1 : 1;
                for (int y = 0; y <= absDy; y++)
                {
                    nowY = startCoordinate.y + addY;
                    trueNowX = trueNowX + perAddx;
                    if (y == absDy)
                    {
                        nowX = targetCoordinate.x;
                    }
                    else
                    {
                        nowX = perAddx < 0 ? (int)math.ceil(trueNowX) : (int)math.floor(trueNowX);
                    }
                    if (!CheckIsWalk(nowX, nowY, mapInstace))
                    {
                        break;
                    }
                    else
                    {
                        result.x = nowX;
                        result.y = nowY;
                    }
                }
            }
        }
        return result;
    }

    protected override void Clear()
    {
        base.Clear();
        initializationTask = System.Threading.Tasks.Task.CompletedTask;
        foreach (var data in MapCharacterGrids)
        {
            data.Value.Dispose();
        }
        MapCharacterGrids.Clear();  
        foreach (var runtimeMapRoom in runtimeMapRooms)
        {
            runtimeMapRoom.Value.Dispose();
        }
        runtimeMapRooms.Clear();
        // Clear 可能早于异步初始化完成或被重复调用，释放前统一检查 Native 容器状态。
        if (mapObjBarriers.IsCreated) mapObjBarriers.Dispose();
        if (mapLinkCellSet.IsCreated) mapLinkCellSet.Dispose();
        if (mapLinkSet.IsCreated) mapLinkSet.Dispose();
        if (changeBarriers.IsCreated) changeBarriers.Dispose();
    }
}

public struct TriggerAreaData
{
    public EntityType triggerType;
    public int referenceId;
    public int enterLinkEventId, exitLinkEventId;
}
