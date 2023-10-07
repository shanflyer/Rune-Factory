using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class MapCellController : Singleton<MapCellController>
{
    public delegate void TriggerEvent(int eventId, int reference, bool enter,bool controller);

    public struct MapTriggerAreas
    {
        public NativeList<TriggerArea> triggerAreas;
        private NativeHashMap<int, int> triggerIndexs;

        public void InitTriggerData()
        {
            triggerAreas = new NativeList<TriggerArea>(4, Allocator.TempJob);
            triggerIndexs = new NativeHashMap<int, int>(4, Allocator.TempJob);
        }

        public void AddTriggerCell(TriggerArea trigger)
        {
            triggerAreas.Add(trigger);
            triggerIndexs.Add(trigger.referenceId, triggerAreas.Length - 1);
        }

        public void RemoveTriggerCell(int refrenceId)
        {
            if (triggerIndexs.TryGetValue(refrenceId, out int index))
            {
                triggerAreas.RemoveAt(index);
                triggerIndexs.Remove(refrenceId);
            }
        }

        public readonly void Dispose()
        {
            for (int i = 0; i < triggerAreas.Length; i++)
            {
                triggerAreas[i].Dispose();
            }

            triggerAreas.Dispose();
            triggerIndexs.Dispose();
        }
    }

    [BurstCompile]
    public struct RuntimeMapRoom
    {
        public readonly void Dispose()
        {
            roomCellData.Dispose();
            linkMaps.Dispose();
            neighbourMaps.Dispose();
            commonTriggerAreas.Dispose();
            playerTriggerAreas.Dispose();
        }

        public int id;

        public int3 coordinate;
        public RoomCellData roomCellData;
        public NativeHashMap<int2, LinkMap> linkMaps;
        public NativeList<int> neighbourMaps;

        public MapTriggerAreas commonTriggerAreas;
        public MapTriggerAreas playerTriggerAreas;

        public override int GetHashCode()
        {
            return id;
        }

        public void InitTriggerData()
        {
            commonTriggerAreas.InitTriggerData();
            playerTriggerAreas.InitTriggerData();
        }

        public bool GetLinkMapInCoordinate(int linkMap, ref int2 inCoordinate)
        {
            foreach (var linkMapData in linkMaps)
            {
                if (linkMapData.Value.target.x == linkMap)
                {
                    inCoordinate = linkMapData.Value.startCell;
                    return true;
                }
            }
            return false;
        }

        public void AddLinkMap(int2 coordinate, Direction direction, int targetMap, int2 targetPoint)
        {
            LinkMap linkMap = new LinkMap
            {
                startCell = coordinate,
                direction = direction,
                target = new int3(targetMap, targetPoint.x, targetPoint.y)
            };
            linkMaps.Add(coordinate, linkMap);
            if (!neighbourMaps.Contains(targetMap))
            {
                neighbourMaps.Add(targetMap);
            }
        }

        public bool ChangeMap(int2 nowCoordinate, int2 offsetCoordinate, out int3 newMap)
        {
            newMap = int3.zero;
            if (linkMaps.TryGetValue(nowCoordinate, out LinkMap linkMap))
            {
                if (CheckLinkMapDirection(linkMap.direction, offsetCoordinate))
                {
                    newMap = linkMap.target;
                    return true;
                }
            }
            return false;
        }

        private bool CheckLinkMapDirection(Direction targetDirection, int2 offsetCoordinate)
        {
            switch (targetDirection)
            {
                case Direction.LEFT:
                    return offsetCoordinate.x < 0;

                case Direction.UP:
                    return offsetCoordinate.y > 0;

                case Direction.RIGHT:
                    return offsetCoordinate.x > 0;

                case Direction.DOWN:
                    return offsetCoordinate.y < 0;
            }
            return true;
        }
    }

    public struct LinkMap
    {
        public Direction direction;
        public int2 startCell;
        public int3 target;
    }

    public struct RoomCellData
    {
        public readonly void Dispose()
        {
            mapObjBarriers.Dispose();
            cellValue.Dispose();
        }

        public NativeArray<int> cellValue;
        public NativeHashMap<int, int> mapObjBarriers;
        public int2 startCoordinate, endCoordinate;

#if UNITY_EDITOR

        public List<Vector3Int> GetAllCellData()
        {
            List<Vector3Int> cellData = new List<Vector3Int>();
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            if (cellValue != null && cellValue.Length > 0)
            {
                for (int i = 0; i < cellValue.Length; i++)
                {
                    int x = i / perRowGridCount + startCoordinate.x;
                    int y = i % perRowGridCount + startCoordinate.y;
                    int z = cellValue[i];
                    if (z == 1 && mapObjBarriers.ContainsKey(i))
                    {
                        z = 0;
                    }
                    cellData.Add(new Vector3Int(x, y, z));
                }
            }

            return cellData;
        }

#endif

        public List<int2> GetCoordinates(int2 source, int minRange, int maxRange, bool isWalkable)
        {
            List<int2> results = new List<int2>();
            for (int x = -maxRange; x <= maxRange; x++)
            {
                for (int y = -maxRange; y <= maxRange - x; y++)
                {
                    if (math.abs(x) + math.abs(y) > maxRange || math.abs(x) + math.abs(y) < minRange)
                    {
                        continue;
                    }
                    int2 coordinate = source + new int2(x, y);
                    int index = GetCoordinateIndex(coordinate);
                    if (index >= 0 && index < cellValue.Length)
                    {
                        if (CheckWalkable(index) == isWalkable)
                        {
                            results.Add(coordinate);
                        }
                    }
                }
            }
            return results;
        }

        public int2 GetCoordinate(int index)
        {
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            return new int2(index / perRowGridCount, index % perRowGridCount);
        }

        public int GetCoordinateIndex(int x, int y)
        {
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            int index = (y - startCoordinate.y) + (x - startCoordinate.x) * perRowGridCount;
            return index;
        }

        public int GetCoordinateIndex(int2 coordinate)
        {
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            int index = (coordinate.y - startCoordinate.y) + (coordinate.x - startCoordinate.x) * perRowGridCount;
            return index;
        }

        public bool CheckWalkable(int index)
        {
            if (cellValue[index] == 1)
            {
                if (mapObjBarriers.ContainsKey(index))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool CheckWalkable(int2 coordinate)
        {
            if (coordinate.x >= startCoordinate.x && coordinate.x <= endCoordinate.x &&
            coordinate.y >= startCoordinate.y && coordinate.y <= endCoordinate.y)
            {
                int index = GetCoordinateIndex(coordinate);
                if (cellValue[index] == 1)
                {
                    if (mapObjBarriers.ContainsKey(index))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool CheckWalkable(Vector2Int coordinate)
        {
            if (coordinate.x >= startCoordinate.x && coordinate.x <= endCoordinate.x &&
            coordinate.y >= startCoordinate.y && coordinate.y <= endCoordinate.y)
            {
                int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
                int index = (coordinate.y - startCoordinate.y) + (coordinate.x - startCoordinate.x) * perRowGridCount;
                if (cellValue[index] == 1)
                {
                    if (mapObjBarriers.ContainsKey(index))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }

    private MyNativeData<RuntimeMapRoom> runtimeMapRooms;

    /// <summary>
    /// 添加障碍
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="itemPos"></param>
    /// <param name="room"></param>
    public void AddBarrierCell(int2[] cells, int2 itemPos, int room)
    {
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                int index = runtimeMapRoom.roomCellData.GetCoordinateIndex(cells[i] + itemPos);
                runtimeMapRoom.roomCellData.mapObjBarriers.TryGetValue(index, out int count);
                count++;
                runtimeMapRoom.roomCellData.mapObjBarriers[index] = count;
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
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                int index = runtimeMapRoom.roomCellData.GetCoordinateIndex(cells[i] + itemPos);
                runtimeMapRoom.roomCellData.mapObjBarriers.TryGetValue(index, out int count);
                count--;
                if (count <= 0)
                {
                    runtimeMapRoom.roomCellData.mapObjBarriers.Remove(index);
                }
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
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            int4 triggerEvent = new int4((int)triggerType, enterEventId, exitEventId, linkId);

            TriggerArea triggerArea = new TriggerArea
            {
                cells = new UnsafeHashSet<int2>(cells.Length, Allocator.TempJob),
                enterLinkEventId = enterEventId,
                exitLinkEventId = exitEventId,
                referenceId = linkId,
                triggerType = triggerType
            };

            runtimeMapRoom.commonTriggerAreas.AddTriggerCell(triggerArea);
            runtimeMapRooms.SetData(runtimeMapRoom);
        }
    }

    /// <summary>
    /// 移除通用触发格子
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="room"></param>
    /// <param name="linkId"></param>
    public void RemoveTriggerCell(int2[] cells, int room, int linkId)
    {
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom.commonTriggerAreas.RemoveTriggerCell(linkId);
            runtimeMapRooms.SetData(runtimeMapRoom);
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
    public void AddPlayerTriggerCell(int2[] cells, int room, int enterEventId, int linkId, int2 offset)
    {
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            TriggerArea triggerArea = new TriggerArea
            {
                cells = new UnsafeHashSet<int2>(4, Allocator.TempJob),
                enterLinkEventId = enterEventId,
                referenceId = linkId,
            };

            for (int i = 0; i < cells.Length; i++)
            {
                triggerArea.cells.Add(cells[i] + offset);
            }
            runtimeMapRoom.playerTriggerAreas.AddTriggerCell(triggerArea);
            runtimeMapRooms.SetData(runtimeMapRoom);
        }
    }

    /// <summary>
    /// 移除玩家交互格子
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="room"></param>
    /// <param name="linkId"></param>
    public void RemovePlayerTriggerCell(int2[] cells, int room, int linkId)
    {
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom.playerTriggerAreas.RemoveTriggerCell(linkId);
            runtimeMapRooms.SetData(runtimeMapRoom);
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
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        { 
            NativeArray<int3> triggerEvents = new NativeArray<int3>
                (runtimeMapRoom.commonTriggerAreas.triggerAreas.Length, Allocator.TempJob); 
            TriggerJob triggerJob = new TriggerJob
            {
                TriggerAreas= runtimeMapRoom.commonTriggerAreas.triggerAreas,
                oldCell = oldCell,
                nowCell = nowCell,
                triggerEvents = triggerEvents,
                triggerType = entityType,
            };
           
            triggerJob.Schedule(triggerEvents.Length, 8).Complete();

            int length = triggerJob.triggerEvents.Length;
            for (int i = 0; i < length; i++)
            {
                int eventId = triggerJob.triggerEvents[i].x;
                if (eventId != 0)
                {
                    triggerEvent(eventId, triggerEvents[i].y, triggerEvents[i].z == 1,false);
                }
            }
            triggerEvents.Dispose(); 
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
        TriggerEvent triggerEvent,int oldLink=0,bool trueMove=true)
    {
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            NativeArray<int3> triggerEvents = new NativeArray<int3>
                (runtimeMapRoom.playerTriggerAreas.triggerAreas.Length, Allocator.TempJob);

            TriggerPlayerJob triggerJob = new TriggerPlayerJob
            {
                TriggerAreas = runtimeMapRoom.playerTriggerAreas.triggerAreas,
                oldCell = oldCell,
                nowCell = nowCell,
                oldLinkId=oldLink,
                triggerType = EntityType.玩家,
                triggerEvents = triggerEvents
            };
            triggerJob.Run(triggerEvents.Length);
            //triggerJob.Schedule(triggerEvents.Length, 8).Complete();
            int length = triggerJob.triggerEvents.Length;
            List<int3> enterEventDatas = new List<int3>();
            for (int i = 0; i < length; i++)
            {
                int eventId = triggerJob.triggerEvents[i].x;
                if (eventId == 0&& triggerJob.triggerEvents[i].y==0)
                {
                    continue;
                }
                if (triggerEvents[i].z == 1)
                {
                    enterEventDatas.Add(triggerEvents[i]);
                }
                else if(trueMove)
                {
                    triggerEvent(eventId, triggerEvents[i].y, triggerEvents[i].z == 1,true);
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
                        float dis = CharacterManager.instance.GetDistanceController(objCoordinate.coordinate);
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
                triggerEvent(selectEventData.x, selectEventData.y, selectEventData.z == 1,true);
            }
            

            triggerEvents.Dispose();
        }
    }

    public bool GetRuntimeMapRoom(int roomId, out RuntimeMapRoom runtimeMapRoom)
    {
        return runtimeMapRooms.GetData(roomId, out runtimeMapRoom);
    }

    public bool ContainsRoom(int roomId)
    {
        return runtimeMapRooms.Contains(roomId);
    }

    public int3 GetRoomCoordinate(int roomId)
    {
        if (runtimeMapRooms.GetData(roomId, out var runtimeMapRoom))
        {
            return runtimeMapRoom.coordinate;
        }
        return int3.zero;
    }

    public RoomCellData GetRoomCellData(int roomId)
    {
        if (runtimeMapRooms.GetData(roomId, out var runtimeMapRoom))
        {
            return runtimeMapRoom.roomCellData;
        }
        return default(RoomCellData);
    }

    public void InitWorldRoomDatas(int roomCount)
    {
        runtimeMapRooms.Init(roomCount);
    }

    public void InitMapData(int roomId, MapCellData[] mapCellDatas,
        int2 startCoordinate, int2 endCoordinate, int3 coordinate)
    {
        RoomCellData roomCellData = new RoomCellData
        {
            cellValue = new NativeArray<int>(mapCellDatas.Length, Allocator.TempJob),
            mapObjBarriers = new NativeHashMap<int, int>(16, Allocator.TempJob),
            startCoordinate = startCoordinate,
            endCoordinate = endCoordinate,
        };
        for (int i = 0; i < mapCellDatas.Length; i++)
        {
            roomCellData.cellValue[i] = mapCellDatas[i].isWalkable ? 1 : 0;
        }

        RuntimeMapRoom runtimeMapRoom = new RuntimeMapRoom
        {
            coordinate = coordinate,
            roomCellData = roomCellData, 
            id = roomId, 
            linkMaps = new NativeHashMap<int2, LinkMap>(16, Allocator.TempJob),
            neighbourMaps = new NativeList<int>(8, Allocator.TempJob),
        };
        runtimeMapRoom.InitTriggerData();
        runtimeMapRooms.AddData(runtimeMapRoom);
    }

    private Direction GetMapDirection(int map0, int map1, int2 coordinate0, int2 coordinate1)
    {
        if (runtimeMapRooms.GetData(map0, out var runtimeMapRoom0) &&
            runtimeMapRooms.GetData(map1, out var runtimeMapRoom1))
        {
            coordinate0.x += runtimeMapRoom0.coordinate.x;
            coordinate0.y += runtimeMapRoom0.coordinate.y;

            coordinate1.x += runtimeMapRoom1.coordinate.x;
            coordinate1.y += runtimeMapRoom1.coordinate.y;

            return GameCommon.GetDirect(coordinate0, coordinate1);
        }
        return Direction.Default;
    }

    public bool GetLinkMapInCoordinate(int nowMap, int linkMap, ref int2 inCoordinate)
    {
        if (GetRuntimeMapRoom(nowMap, out RuntimeMapRoom nowRuntimeMap))
        {
            return nowRuntimeMap.GetLinkMapInCoordinate(linkMap, ref inCoordinate);
        }
        return false;
    }

    public bool ChangeMap(int2 nowCoordinate, int2 offsetCoordinate, int nowMap, out int3 newMap)
    {
        if (GetRuntimeMapRoom(nowMap, out RuntimeMapRoom runtimeMapRoom))
        {
            return runtimeMapRoom.ChangeMap(nowCoordinate, offsetCoordinate, out newMap);
        }
        newMap = int3.zero;
        return false;
    }

    public void InitLinkMap(List<MapLine> mapLines)
    {
        foreach (var mapLine in mapLines)
        {
            int2 cell0 = new int2(mapLine.cell0.x, mapLine.cell0.y);
            int2 cell1 = new int2(mapLine.cell1.x, mapLine.cell1.y);
            if (runtimeMapRooms.GetData(mapLine.map0, out RuntimeMapRoom runtimeMapRoom))
            {
                runtimeMapRoom.AddLinkMap(cell0, GetMapDirection(mapLine.map0, mapLine.map1, mapLine.cell0, mapLine.cell1),
                    mapLine.map1, cell1);
                runtimeMapRooms.SetData(runtimeMapRoom);
            }

            if (runtimeMapRooms.GetData(mapLine.map1, out RuntimeMapRoom _runtimeMapRoom))
            {
                _runtimeMapRoom.AddLinkMap(cell1, GetMapDirection(mapLine.map1, mapLine.map0, mapLine.cell1, mapLine.cell0),
                    mapLine.map0, cell0);
                runtimeMapRooms.SetData(_runtimeMapRoom);
            }
        }
    }

    public Stack<int2> FindPathNode(int2 startPos, int2 targetPos, int mapId)
    {
        Stack<int2> outData = new Stack<int2>();
        if (runtimeMapRooms.GetData(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            RoomCellData roomCellData = runtimeMapRoom.roomCellData;
            NativeList<int2> pathCells = new NativeList<int2>(16, Allocator.TempJob);
            FindPath findPath = new FindPath
            {
                roomCellData = roomCellData,
                startPos = new int2(startPos.x, startPos.y),
                targetPos = new int2(targetPos.x, targetPos.y),
                pathCells = pathCells
            };

            //findPath.Schedule().Complete();
            findPath.Run();

            for (int i = 0; i < findPath.pathCells.Length; i++)
            {
                outData.Push(findPath.pathCells[i]);
            }

            pathCells.Dispose();
        }
        return outData;
    }

    public bool CheckIsWalk(Vector2Int coordinate, int mapId)
    {
        if (runtimeMapRooms.GetData(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            RoomCellData roomCellData = runtimeMapRoom.roomCellData;

            return roomCellData.CheckWalkable(coordinate);
        }
        return false;
    }

    public Queue<int> FindRoomList(int sourceId, int targetId, Queue<int> roomList, ref bool result)
    {
        if (sourceId == targetId)
        {
            result = true;
            return new Queue<int>();
        }
        if (runtimeMapRooms.GetData(sourceId, out RuntimeMapRoom sourceRoom) &&
            runtimeMapRooms.Contains(targetId))
        {
            foreach (var id in sourceRoom.neighbourMaps)
            {
                roomList.Enqueue(id);
                if (id == targetId)
                {
                    result = true;
                    return roomList;
                }
                else
                {
                    var resultList = FindRoomList(id, targetId, roomList, ref result);
                    if (result)
                    {
                        return resultList;
                    }
                }
            }
        }
        return null;
    }

    public void DeleteRoom(int roomInstanceid)
    {
        runtimeMapRooms.RemoveData(roomInstanceid);
    }

    public override void Init()
    {
        base.Init();
    }

    protected override void Clear()
    {
        base.Clear();
        runtimeMapRooms.Dispose();
    }

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private static int CalculateDistanceCost(int2 aPosition, int2 bPosition)
    {
        // int2 d = bPosition - aPosition;
        //return d.x * d.x + d.y *d.y;

        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);

        int cost = MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;

        return cost;
    }

    public struct FindPath : IJob
    {
        //[ReadOnly] public int MOVE_STRAIGHT_COST;
        // [ReadOnly] public int MOVE_DIAGONAL_COST;
        [ReadOnly] public RoomCellData roomCellData;

        [ReadOnly] public int2 startPos, targetPos;

        [WriteOnly] public NativeList<int2> pathCells;

        public void Execute()
        {
            GetPath();
        }

        private void GetPath()
        {
            if (roomCellData.CheckWalkable(startPos) && roomCellData.CheckWalkable(targetPos))
            {
                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
                neighbourOffsetArray[0] = new int2(-1, 0); // Left
                neighbourOffsetArray[1] = new int2(+1, 0); // Right
                neighbourOffsetArray[2] = new int2(0, +1); // Up
                neighbourOffsetArray[3] = new int2(0, -1); // Down
                neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
                neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
                neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
                neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

                NativeList<int2> openCells = new NativeList<int2>(Allocator.Temp);
                NativeList<int2> closeCells = new NativeList<int2>(Allocator.Temp);
                NativeHashMap<int2, int> cellCost = new NativeHashMap<int2, int>(16, Allocator.Temp);
                NativeHashMap<int2, int> parentCell = new NativeHashMap<int2, int>(16, Allocator.Temp);

                int openCellLength = 0;
                int closeCellLength = 0;

                openCells.Add(startPos);
                openCellLength++;
                // int cost = CalculateDistanceCost(startPos, targetPos) * 5;
                cellCost[startPos] = 0;
                while (openCellLength > 0)
                {
                    int2 nowCell = openCells[0];

                    openCells.RemoveAt(0);
                    //openCells.RemoveAtSwapBack(0);
                    openCellLength--;
                    closeCells.Add(nowCell);
                    closeCellLength++;
                    if (nowCell.x == targetPos.x && nowCell.y == targetPos.y)
                    {
                        break;
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        int2 cell = neighbourOffsetArray[i] + nowCell;

                        if (roomCellData.CheckWalkable(cell))
                        {
                            if (closeCells.Contains(cell) || openCells.Contains(cell))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        int cost = CalculateDistanceCost(cell, startPos) +
                             CalculateDistanceCost(cell, targetPos) * 3;
                        cellCost[cell] = cost;
                        parentCell[cell] = closeCellLength - 1;

                        if (cell.x == targetPos.x && cell.y == targetPos.y)
                        {
                            openCells.Add(cell);
                            openCellLength++;
                            break;
                        }

                        bool insert = false;
                        for (int j = 0; j < openCells.Length; j++)
                        {
                            if (cellCost.TryGetValue(openCells[j], out int _cost))
                            {
                                if (_cost > cost)
                                {
                                    openCells.InsertRangeWithBeginEnd(j, j + 1);
                                    openCells[j] = cell;

                                    insert = true;
                                    openCellLength++;
                                    break;
                                }
                            }
                        }
                        if (!insert)
                        {
                            openCells.Add(cell);
                            openCellLength++;
                        }
                    }
                }

                var checkCell = closeCells[closeCellLength - 1];
                pathCells.Add(checkCell);
                while (checkCell.x != startPos.x || checkCell.y != startPos.y)
                {
                    if (parentCell.TryGetValue(checkCell, out int index))
                    {
                        if (closeCellLength <= index)
                        {
                            break;
                        }
                        checkCell = closeCells[index];
                        pathCells.Add(checkCell);
                    }
                    else
                    {
                        break;
                    }
                }

                neighbourOffsetArray.Dispose();
                openCells.Dispose();
                closeCells.Dispose();
                cellCost.Dispose();
                parentCell.Dispose();
            }
        }
    }

    public struct TriggerJob : IJobParallelFor
    {
        // [ReadOnly] public BlobAssetReference<TriggerAreaAsset> triggerAreaAssetRef;
        [ReadOnly] public NativeList<TriggerArea> TriggerAreas;

        [WriteOnly] public NativeArray<int3> triggerEvents;
        [ReadOnly] public int2 oldCell;
        [ReadOnly] public int2 nowCell;
        [ReadOnly] public EntityType triggerType;

        public void Execute(int index)
        {
            TriggerArea triggerArea = TriggerAreas[index];
            int typeValue = (int)triggerArea.triggerType % (int)triggerType;
            if (typeValue > 0)
            {
                return;
            }

            bool oldContanins = triggerArea.cells.Contains(oldCell);
            bool nowContanins = triggerArea.cells.Contains(nowCell);

            if (oldContanins && !nowContanins)
            {
                //离开事件
                triggerEvents[index] = new int3(triggerArea.exitLinkEventId, triggerArea.referenceId, 0);
            }
            else if (!oldContanins && nowContanins)
            {
                //进入事件
                triggerEvents[index] = new int3(triggerArea.enterLinkEventId, triggerArea.referenceId, 1);
            }
        }
    }
    public struct TriggerPlayerJob : IJobParallelFor
    {
        // [ReadOnly] public BlobAssetReference<TriggerAreaAsset> triggerAreaAssetRef;
        [ReadOnly] public NativeList<TriggerArea> TriggerAreas;

        [WriteOnly] public NativeArray<int3> triggerEvents;
        [ReadOnly] public int oldLinkId;
        [ReadOnly] public int2 oldCell;
        [ReadOnly] public int2 nowCell;
        [ReadOnly] public EntityType triggerType;

        public void Execute(int index)
        {
            TriggerArea triggerArea = TriggerAreas[index];
            int typeValue = (int)triggerArea.triggerType % (int)triggerType;
            if (typeValue > 0)
            {
                return;
            }
            bool oldContanins = triggerArea.cells.Contains(oldCell);
            bool nowContanins = triggerArea.cells.Contains(nowCell);
            if (triggerArea.referenceId == oldLinkId)
            {
                if (oldContanins && !nowContanins)
                {
                    //离开事件
                    triggerEvents[index] = new int3(triggerArea.exitLinkEventId, triggerArea.referenceId, 0);
                }
            }
            else
            {
                if (nowContanins)
                {
                    //进入
                    triggerEvents[index] = new int3(triggerArea.enterLinkEventId, triggerArea.referenceId, 1);
                } 
            }
        }
    }
}

public struct TriggerArea
{
    public UnsafeHashSet<int2> cells;
    public EntityType triggerType;
    public int referenceId;
    public int enterLinkEventId, exitLinkEventId;

    public void Dispose()
    {
        cells.Dispose();
    }
}