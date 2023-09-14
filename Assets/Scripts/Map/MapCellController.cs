using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.Analytics;

public class MapCellController :Singleton<MapCellController>
{

    public delegate void TriggerEvent(int eventId,int reference,bool enter);
    [BurstCompile]
    public struct RuntimeMapRoom
    {
        public readonly void Dispose()
        {
            roomCellData.Dispose();
            linkMaps.Dispose();
            neighbourMaps.Dispose();
            triggerEventCells.Dispose();
        }

        public int id;

        public int3 coordinate;
        public RoomCellData roomCellData;
        public NativeHashMap<int2, LinkMap> linkMaps;
        public NativeList<int> neighbourMaps;


        public NativeHashMap<int2, NativeList<int4>> triggerEventCells;

       // public NativeList<TriggerCell> triggerCells;
       // private NativeHashMap<int, int> triggerIndexs;
       // private NativeHashMap<int, int> triggerRefrenceDatas;

        public override int GetHashCode()
        {
            return id;
        }

        public void InitTriggerData()
        {
            triggerEventCells=new NativeHashMap<int2, NativeList<int4>>(16,Allocator.TempJob);


           // triggerCells = new NativeList<TriggerCell>(16, Allocator.TempJob);
            //triggerIndexs = new NativeHashMap<int, int>(16, Allocator.TempJob);
            //triggerRefrenceDatas = new NativeHashMap<int, int>(16, Allocator.TempJob);
        }
        /*
        public void AddTriggerCell(TriggerCell trigger,int linkId = 0)
        { 
            int id = GameCommon.CreateRandSeed();
            while (triggerIndexs.ContainsKey(id))
            {
                id = GameCommon.CreateRandSeed();
            }

            triggerCells.Add(trigger);
            triggerIndexs.Add(id, triggerCells.Length - 1);
            if (linkId !=0)
            {
                triggerRefrenceDatas.Add(linkId, id);
            }
        }
        */
        /*public void RemoveTriggerCell(int refrenceId)
        {
            if(triggerRefrenceDatas.TryGetValue(refrenceId,out int id))
            {
                if(triggerIndexs.TryGetValue(id,out int index))
                {
                    triggerCells.RemoveAt(index);
                    triggerIndexs.Remove(id);
                }
                triggerRefrenceDatas.Remove(refrenceId);
            }
        }*/
         

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
        bool CheckLinkMapDirection(Direction targetDirection, int2 offsetCoordinate)
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
            cellValue.Dispose(); 
        }
        public NativeArray<int> cellValue;
        public int2 startCoordinate, endCoordinate;

#if UNITY_EDITOR
        public List<Vector3Int> GetAllCellData()
        {
            List<Vector3Int> cellData = new List<Vector3Int>();
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            if (cellValue != null&&cellValue.Length>0)
            {
                for (int i = 0; i < cellValue.Length; i++)
                {
                    int x = i / perRowGridCount+startCoordinate.x;
                    int y = i % perRowGridCount+startCoordinate.y;
                    int z = cellValue[i];
                    cellData.Add(new Vector3Int(x, y, z));
                }
            }
          

            return cellData;
        }
#endif

        public bool CheckWalkable(int2 coordinate)
        {
            if (coordinate.x >= startCoordinate.x && coordinate.x <= endCoordinate.x &&
            coordinate.y >= startCoordinate.y && coordinate.y <= endCoordinate.y)
            {
                int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
                int index = (coordinate.y - startCoordinate.y) + (coordinate.x - startCoordinate.x) * perRowGridCount;
                return cellValue[index]==1;
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
                return cellValue[index] == 1;
            }
            return false;
        }
    }

    //触发格子事件
    /*public struct TriggerCell  
    {
        public int referenceId;
        public EntityType triggerType;
        public NativeHashSet<int2> cells;
        public int enterLinkEventId;
        public int exitLinkEventId;
    }*/


    private MyNativeData<RuntimeMapRoom> runtimeMapRooms;

    //检测触发事件
    public void CheckTriggerEvent(int entityId,EntityType entityType,int room,int2 oldCell,int2 nowCell,
        TriggerEvent triggerEvent)
    {
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
           var triggerEventCells = runtimeMapRoom.triggerEventCells ;
            if(triggerEventCells.TryGetValue(oldCell,out var enevntData))
            {
                for(int i = 0; i < enevntData.Length; i++)
                {
                    var data = enevntData[i]; 
                    int typeValue = data.x % (int)entityType;
                    if (typeValue > 0)
                    {
                        return;
                    } 
                    if (data.z != 0)
                    {
                        triggerEvent.Invoke(data.z,data.w, false);
                    } 
                }
            }

            if (triggerEventCells.TryGetValue(nowCell, out var eventData))
            {
                for (int i = 0; i < eventData.Length; i++)
                {
                    var data = eventData[i];
                    int typeValue = data.x % (int)entityType;
                    if (typeValue > 0)
                    {
                        return;
                    }
                    if (data.y != 0)
                    {
                        triggerEvent.Invoke(data.y, data.w, true);
                    }
                }
            }
        }


        /*
        TriggerCell triggerCell; 
        if (GetRuntimeMapRoom(room,out RuntimeMapRoom runtimeMapRoom))
        {
            NativeArray<TriggerCell> triggerCells= new  NativeArray<TriggerCell>(runtimeMapRoom.triggerCells.Length, Allocator.TempJob);
            triggerCells.CopyFrom(runtimeMapRoom.triggerCells.AsArray());

            NativeArray<int3> triggerEvents = new NativeArray<int3>(runtimeMapRoom.triggerCells.Length,Allocator.TempJob);

            TriggerJob triggerJob = new TriggerJob
            {
                triggerCells = triggerCells,
                oldCell = oldCell,
                nowCell = nowCell,
                triggerEvents = triggerEvents,
                triggerType=entityType,
            };
            //triggerJob.Run(runtimeMapRoom.triggerCells.Length);
           triggerJob.Schedule(runtimeMapRoom.triggerCells.Length, 8).Complete();

            int length = triggerJob.triggerEvents.Length;
            for(int i = 0; i < length; i++)
            {
                int eventId = triggerJob.triggerEvents[i].x;
                if (eventId != 0)
                {
                    triggerEvent(eventId, triggerJob.triggerEvents[i].y, triggerJob.triggerEvents[i].z==1);
                }
                
            }
            triggerEvents.Dispose();
            triggerCells.Dispose();
        }

       */
    }

    public void RemoveTriggerCell(int2[] cells,int room, int linkId)
    {
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for(int i = 0; i < cells.Length; i++)
            {
                if (runtimeMapRoom.triggerEventCells.TryGetValue(cells[i],out  var enevntData))
                {
                    for(int j = enevntData.Length-1; j >=0; j--)
                    {
                        var data = enevntData[j];
                        if (data.w == linkId)
                        {
                            enevntData.RemoveAt(j);
                        }
                    }
                }
            }
            //runtimeMapRoom.RemoveTriggerCell(linkId);
            runtimeMapRooms.SetData(runtimeMapRoom);
        }
    }
    /*
    public void RemoveTriggerCell(int room,int linkId)
    {
        if(runtimeMapRooms.GetData(room,out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom.RemoveTriggerCell(linkId);
            runtimeMapRooms.SetData(runtimeMapRoom); 
        } 
    }*/
    public void AddTriggerCell(int2[] cells,int room,int enterEventId,int exitEventId,EntityType triggerType,int linkId,int2 offset)
    {
        if (runtimeMapRooms.GetData(room, out RuntimeMapRoom runtimeMapRoom))
        { 
            int4 triggerEvent=new int4((int)triggerType,enterEventId,exitEventId,linkId);
            /*
            TriggerCell triggerCell = new TriggerCell
            {
                cells = new NativeHashSet<int2>(cells.Length, Allocator.TempJob),
                enterLinkEventId = enterEventId,
                exitLinkEventId = exitEventId,
                referenceId = linkId,
                triggerType = triggerType
            };
            */
            for (int i = 0; i < cells.Length; i++)
            {
                if (!runtimeMapRoom.triggerEventCells.TryGetValue(cells[i] + offset, out var items))
                {
                    items = new NativeList<int4>(4, Allocator.TempJob);
                    
                }
                items.Add(triggerEvent);
                runtimeMapRoom.triggerEventCells.Add(cells[i] + offset, items);

                //triggerCell.cells.Add(cells[i]+offset);
            }
            //runtimeMapRoom.AddTriggerCell(triggerCell, linkId); 
            runtimeMapRooms.SetData(runtimeMapRoom);
        }
         
    }

    public bool GetRuntimeMapRoom(int roomId,out RuntimeMapRoom runtimeMapRoom)
    { 
        return runtimeMapRooms.GetData(roomId,out runtimeMapRoom);
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
        if(runtimeMapRooms.GetData(roomId,out var runtimeMapRoom))
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
        int2 startCoordinate, int2 endCoordinate,int3 coordinate)
    {
       

        RoomCellData roomCellData = new RoomCellData
        {
            cellValue = new NativeArray<int>(mapCellDatas.Length, Allocator.Persistent),
            startCoordinate = startCoordinate,
            endCoordinate = endCoordinate, 
        }; 
        for(int i = 0; i < mapCellDatas.Length; i++)
        {
            roomCellData.cellValue[i] = mapCellDatas[i].isWalkable ? 1 : 0;
        }

        RuntimeMapRoom runtimeMapRoom = new RuntimeMapRoom
        {
            coordinate = coordinate,
            roomCellData = roomCellData,

            id=roomId,

            linkMaps = new NativeHashMap<int2, LinkMap>(16, Allocator.Persistent),
            neighbourMaps = new NativeList<int>(8, Allocator.Persistent), 
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
        if(GetRuntimeMapRoom(nowMap,out RuntimeMapRoom nowRuntimeMap))
        {
            return nowRuntimeMap.GetLinkMapInCoordinate(linkMap, ref inCoordinate);
        } 
        return false;
    }
    public bool ChangeMap(int2 nowCoordinate, int2 offsetCoordinate, int nowMap, out int3 newMap)
    {
        if(GetRuntimeMapRoom(nowMap,out RuntimeMapRoom runtimeMapRoom))
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

    public Stack<int2> FindPathNode(int2 startPos, int2 targetPos,int mapId)
    {
        Stack<int2> outData = new Stack<int2>();
        if (runtimeMapRooms.GetData(mapId,out RuntimeMapRoom runtimeMapRoom))
        {
            RoomCellData roomCellData = runtimeMapRoom.roomCellData;
            NativeList<int2> pathCells = new NativeList<int2>(16,Allocator.TempJob);
            FindPath findPath = new FindPath
            {
                roomCellData = roomCellData,
                startPos = new int2(startPos.x, startPos.y),
                targetPos = new int2(targetPos.x, targetPos.y),
                pathCells = pathCells
            };

            findPath.Schedule().Complete();

            for(int i = 0; i < findPath.pathCells.Length; i++)
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
            RoomCellData roomCellData= runtimeMapRoom.roomCellData;

            return roomCellData.CheckWalkable(coordinate);
        }
        return false;
    }
    public Queue<int> FindRoomList(int sourceId, int targetId, Queue<int> roomList, ref bool result)
    {
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
        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);

        int cost= MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
         
        return cost;
    }
    public struct FindPath:IJob
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

        void GetPath()
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

                while (openCellLength > 0)
                {
                    int2 nowCell = openCells[0];

                    openCells.RemoveAtSwapBack(0);
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


                        int cost = CalculateDistanceCost(cell, startPos)  +
                            CalculateDistanceCost(cell, targetPos) * 5;
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
                            if (cellCost.TryGetValue(openCells[j],out int _cost))
                            { 
                                if (_cost > cost)
                                { 
                                    openCells.InsertRangeWithBeginEnd(j, j+1);
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
                    if(parentCell.TryGetValue(checkCell,out int index))
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

    /*
    public struct TriggerJob:IJobParallelFor
    {
       [ReadOnly] public NativeArray<TriggerCell> triggerCells;
       [WriteOnly] public NativeArray<int3> triggerEvents;
       [ReadOnly] public int2 oldCell;
       [ReadOnly] public int2 nowCell;
        [ReadOnly] public EntityType triggerType;

        public void Execute(int index)
        {
            TriggerCell triggerCell = triggerCells[index];
            int typeValue = (int)triggerCell.triggerType % (int)triggerType;
            if (typeValue > 0)
            {
                return;
            }



            bool oldContanins = triggerCell.cells.Contains(oldCell);
            bool nowContanins = triggerCell.cells.Contains(nowCell);

            if (oldContanins&&!nowContanins)
            {
                triggerEvents[index]=new int3(triggerCell.exitLinkEventId,triggerCell.referenceId,1);
            }
            else if (!oldContanins && nowContanins)
            {
                triggerEvents[index] = new int3(triggerCell.enterLinkEventId, triggerCell.referenceId,0);
            }
        }
    }
    */
}
