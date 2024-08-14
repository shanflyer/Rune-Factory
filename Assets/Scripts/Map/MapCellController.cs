using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CharacterGrid : INativeData
{
    public int characterId;
    public int4 Grid;
    public int Key => characterId;

    public void Dispose()
    {
    }
}

public class MapCharacterGrid
{
    public int mapInstance;
    public NativeList<int3> characters;
    public Dictionary<int, int> characterIndexs;

    public void RemoveCharacter(int characterInstance)
    {
        if(characterIndexs.TryGetValue(characterInstance,out var index))
        {
            if (index != characters.Length - 1)
            {
                int3 lastCharacter = characters[characters.Length - 1];
                characters[index] = lastCharacter;
                characterIndexs[lastCharacter.z] = index;
            }
            characterIndexs.Remove(characterInstance);
            characters.RemoveAt(characters.Length - 1);
        }
    }
    public void AddCharacter(int characterInstance, int2 coordinate)
    {
        if(!characterIndexs.ContainsKey(characterInstance))
        {
            characterIndexs.Add(characterInstance, characters.Length);
            characters.Add(new int3(coordinate.xy,characterInstance));
        }
    } 
    public void ChangeCharacter(int characterInstance, int2 coordinate)
    {
        if (!characterIndexs.TryGetValue(characterInstance,out int index))
        {
            characterIndexs.Add(characterInstance, characters.Length);
            characters.Add(new int3(coordinate.xy, characterInstance));
        }
        else
        {
            characters[index] = new int3(coordinate.xy, characterInstance);
        }
    }

    public MapCharacterGrid()
    {
        characters = new NativeList<int3>(16,Allocator.Persistent);
        characterIndexs = new Dictionary<int, int>();
    } 
    public void Dispose()
    {
        characters.Dispose();
    }
}

public class MapCellController : Singleton<MapCellController>
{
    public delegate void TriggerEvent(int eventId, int reference, bool enter, bool controller);

    //private Dictionary<int3, HashSet<int>> characterCells = new Dictionary<int3, HashSet<int>>();

    public const int characterRange = 5;

    private Dictionary<int, MapCharacterGrid> MapCharacterGrids = new Dictionary<int, MapCharacterGrid>();

    public void SetCharacterCoordinate(int3 oldCoordinate, int3 newCoordinate, int characterId)
    {
        if (newCoordinate.z != oldCoordinate.z)
        {
            if (MapCharacterGrids.TryGetValue(oldCoordinate.z, out var oldMapCharacterGrid))
            {
                oldMapCharacterGrid.RemoveCharacter(characterId);
            }
        }
        if (!MapCharacterGrids.TryGetValue(newCoordinate.z, out var mapCharacterGrid))
        {
            mapCharacterGrid = new MapCharacterGrid
            {
                mapInstance = newCoordinate.z,
            };
            MapCharacterGrids.Add(newCoordinate.z, mapCharacterGrid);
        }
        mapCharacterGrid.ChangeCharacter(characterId, newCoordinate.xy);
    }

    public void RemoveCharacterCoordinate(int3 coordinate, int characterId)
    {
        if (MapCharacterGrids.TryGetValue(coordinate.z, out var mapCharacterGrid))
        {
            mapCharacterGrid.RemoveCharacter(characterId);
        }
    }
    public int[] GetCharactersForRange(int3 coordinate, int Range)
    {
        if (MapCharacterGrids.TryGetValue(coordinate.z, out var mapCharacterGrid))
        {
            NativeList<int> results = new NativeList<int>(mapCharacterGrid.characters.Length, Allocator.TempJob);
            FindCharacterRangeInCell findCharacterRangeInCell = new FindCharacterRangeInCell
            {
                coordinate = coordinate.xy,
                girds = mapCharacterGrid.characters,
                result = results.AsParallelWriter(),
                range = Range
            };
            findCharacterRangeInCell.ScheduleParallel(mapCharacterGrid.characters.Length, 8, new JobHandle()).Complete();
            var data= results.ToArray();
            results.Dispose();
            return data;
        }
        return null;
    }
    public int GetClickCharacter(int3 coordinate,int Range=5)
    {
        if (MapCharacterGrids.TryGetValue(coordinate.z, out var mapCharacterGrid))
        {
            NativeList<int> results = new NativeList<int>(mapCharacterGrid.characters.Length, Allocator.TempJob);
            FindCharacterRangeInCell findCharacterRangeInCell = new FindCharacterRangeInCell
            {
                coordinate = coordinate.xy,
                girds = mapCharacterGrid.characters,
                result = results.AsParallelWriter(),
                range=Range
            };
            findCharacterRangeInCell.ScheduleParallel(mapCharacterGrid.characters.Length, 8, new JobHandle()).Complete();
            if (results.Length > 0)
            {
                int data = results[0];
                results.Dispose();
                return data;
            }
        }
        return -1;
    }

    public int2 GetRandomWalkable(int3 centerCoordinate, int range)
    {
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

    [BurstCompile]
    public struct MapTriggerAreas
    {
        public NativeList<TriggerArea> triggerAreas;
        private NativeHashMap<int, int> triggerIndexs;

        public NativeArray<int2> GetItemTriggerCells(int instanceId)
        {
            if (triggerIndexs.TryGetValue(instanceId, out var index))
            {
                var cells = triggerAreas[index].cells;
                return cells.ToNativeArray(Allocator.TempJob);
            }
            return new NativeArray<int2>(0, Allocator.TempJob);
        }

        public void InitTriggerData()
        {
            triggerAreas = new NativeList<TriggerArea>(4, Allocator.Persistent);
            triggerIndexs = new NativeHashMap<int, int>(4, Allocator.Persistent);
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

    //[BurstCompile]
    public class RuntimeMapRoom
    {
        private List<NpcBehaviorArea> NpcBehaviorAreas = new List<NpcBehaviorArea>();
        private Dictionary<BehaviorAreaType, List<int>> NpcBehaviorAreaTypeDic = new Dictionary<BehaviorAreaType, List<int>>();
        public int3 GetRandomBehaviorCell(int areaId)
        {
            var npcBehaviorArea = NpcBehaviorAreas.Find(n=>n.Name==areaId);

            if (npcBehaviorArea!=null)
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

        public void Dispose()
        {
            //roomCellData.Dispose();
            NpcBehaviorAreas = null;
            NpcBehaviorAreaTypeDic.Clear();

            linkMapIndexs.Dispose();
            linkMaps.Dispose();
            linkActions.Dispose();
            neighbourMaps.Dispose();
            commonTriggerAreas.Dispose();
            playerTriggerAreas.Dispose();
        }

        public int id;

        public int3 coordinate;
        public int roomCellDataIndex;
        // public RoomCellData roomCellData;

        public NativeHashMap<int2, int> linkMapIndexs;
        public NativeList<int4> linkMaps;

        /// <summary>
        /// 玩家转换地图前后的事件
        /// </summary>
        public NativeList<int3> linkActions;

        public NativeHashMap<int, int> neighbourMaps;

        public MapTriggerAreas commonTriggerAreas;
        public MapTriggerAreas playerTriggerAreas;

        public int Key => id;

        public void InitTriggerData()
        {
            commonTriggerAreas.InitTriggerData();
            playerTriggerAreas.InitTriggerData();
        }

        public bool GetLinkMapInCoordinate(int linkMap, ref int2 inCoordinate)
        {
            NativeList<int2> coordinates = new NativeList<int2>(4, Allocator.Temp);

            foreach (var linkIndex in linkMapIndexs)
            {
                if (linkMaps[linkIndex.Value].z == linkMap)
                {
                    coordinates.Add(linkIndex.Key);
                }
            }

            if (coordinates.Length > 0)
            {
                inCoordinate = coordinates[GameRandom.RandomInt(0, coordinates.Length)];
                return true;
            }
            return false;
        }

        public RuntimeMapRoom DeleteLinkMap(LinkMapCell linkMapCell)
        {
            int directionValue = 0;
            for (int i = 0; i < linkMapCell.directions.Count; i++)
            {
                directionValue += GameCommon.GetDirectionValue(linkMapCell.directions[i]);
            }

            int4 target = new int4(linkMapCell.targetCell.xyz, directionValue);
            for (int i = linkMaps.Length - 1; i >= 0; i--)
            {
                if (linkMaps[i].Equals(target))
                {
                    linkMaps.RemoveAt(i);
                    linkActions.RemoveAt(i);
                    break;
                }
            }
            var cells = GameCommon.GridToCells(linkMapCell.girds);

            for (int i = 0; i < cells.Count; i++)
            {
                linkMapIndexs.Remove(cells[i]);
            }
            if (neighbourMaps.TryGetValue(linkMapCell.targetCell.z, out var num))
            {
                num--;
                if (num == 0)
                {
                    neighbourMaps.Remove(linkMapCell.targetCell.z);
                }
            }
            return this;
        }

        public RuntimeMapRoom AddLinkMap(LinkMapCell linkMapCell)
        {
            int directionValue = 0;
            for (int i = 0; i < linkMapCell.directions.Count; i++)
            {
                directionValue += GameCommon.GetDirectionValue(linkMapCell.directions[i]);
            }

            int4 target = new int4(linkMapCell.targetCell.xyz, directionValue);
            linkMaps.Add(target);

            int3 linkAction = new int3(linkMapCell.beforAction, linkMapCell.afterAction, linkMapCell.checkAction);
            linkActions.Add(linkAction);

            var cells = GameCommon.GridToCells(linkMapCell.girds);
            for (int i = 0; i < cells.Count; i++)
            {
                linkMapIndexs.Add(cells[i], linkMaps.Length - 1);
            }
            neighbourMaps.TryGetValue(linkMapCell.targetCell.z, out var num);
            num++;
            neighbourMaps[linkMapCell.targetCell.z] = num;
            return this;
        }

        public bool ChangeMap(int2 nowCoordinate, Direction direction, out int3 newMap, out int3 changeAction)
        {
            newMap = int3.zero;
            changeAction = int3.zero;
            if (linkMapIndexs.TryGetValue(nowCoordinate, out int index))
            {
                int4 nextData = linkMaps[index];
                if (GameCommon.CheckDirectionValue(direction, nextData.w))
                {
                    newMap = nextData.xyz;
                    changeAction = linkActions[index];
                    return true;
                }
            }
            return false;
        }

        public bool ChangeMap(int2 nowCoordinate, Direction direction, out int3 newMap)
        {
            newMap = int3.zero;
            if (linkMapIndexs.TryGetValue(nowCoordinate, out int index))
            {
                int4 nextData = linkMaps[index];
                if (GameCommon.CheckDirectionValue(direction, nextData.w))
                {
                    newMap = nextData.xyz;
                    return true;
                }
            }
            return false;
        }
    }

    [BurstCompile]
    public struct RoomCellData
    {
        public readonly void Dispose()
        {
            mapObjBarriers.Dispose();
        }

        public NativeHashMap<int, int> mapObjBarriers;
        public int2 startCoordinate, endCoordinate;

        public void AddBarrier(int x, int y)
        {
            int index = GetCoordinateIndex(x, y);
            mapObjBarriers.TryGetValue(index, out int count);
            count++;
            mapObjBarriers[index] = count;
        }

        public void AddBarrier(int2 coordinate)
        {
            int index = GetCoordinateIndex(coordinate);
            mapObjBarriers.TryGetValue(index, out int count);
            count++;
            mapObjBarriers[index] = count;
        }

        public void RemmoveBarrier(int2 coordinate)
        {
            int index = GetCoordinateIndex(coordinate);
            mapObjBarriers.TryGetValue(index, out int count);
            count--;
            if (count == 0)
            {
                mapObjBarriers.Remove(index);
            }
            else
            {
                mapObjBarriers[index] = count;
            }
        }

        public int2 GetRandomCanWalkCell()
        {
            List<int> walkCells = new List<int>();
            int startIndex = GetCoordinateIndex(startCoordinate);
            int endIndex = GetCoordinateIndex(endCoordinate);
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!mapObjBarriers.ContainsKey(i))
                {
                    walkCells.Add(i);
                }
            }
            int index = GameRandom.RandomInt(0, walkCells.Count);
            int cell = walkCells[index];
            return GetCoordinate(cell);
        }

#if UNITY_EDITOR

        public List<Vector3Int> GetAllCellData()
        {
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            List<Vector3Int> cellData = new List<Vector3Int>();
            int startIndex = GetCoordinateIndex(startCoordinate);
            int endIndex = GetCoordinateIndex(endCoordinate);
            for (int i = startIndex; i <= endIndex; i++)
            {
                int x = i / perRowGridCount + startCoordinate.x;
                int y = i % perRowGridCount + startCoordinate.y;
                int z = 1;
                if (!mapObjBarriers.ContainsKey(i))
                {
                    z = 0;
                }
                cellData.Add(new Vector3Int(x, y, z));
            }
            return cellData;
        }

#endif

        public List<int2> GetCoordinates(int2 source, int minRange, int maxRange, bool isWalkable)
        {
            List<int2> results = new List<int2>();
            int cellCount = GetCoordinateIndex(endCoordinate);
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
                    if (index >= 0 && index < cellCount)
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
            return new int2(index / perRowGridCount + startCoordinate.x, index % perRowGridCount + startCoordinate.y);
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
            int cellCount = GetCoordinateIndex(endCoordinate);
            if (cellCount > index)
            {
                if (mapObjBarriers.ContainsKey(index))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool CheckWalkable(int x, int y)
        {
            if (x >= startCoordinate.x && x <= endCoordinate.x &&
           y >= startCoordinate.y && y <= endCoordinate.y)
            {
                int index = GetCoordinateIndex(x, y);
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
                if (mapObjBarriers.ContainsKey(index))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public int CheckBarrierCount(int2 coordinate)
        {
            if (coordinate.x >= startCoordinate.x && coordinate.x <= endCoordinate.x &&
            coordinate.y >= startCoordinate.y && coordinate.y <= endCoordinate.y)
            {
                int index = GetCoordinateIndex(coordinate);
                if (mapObjBarriers.TryGetValue(index, out var count))
                {
                    return count;
                }
                return 0;
            }
            return 0;
        }

        public bool CheckWalkable(Vector2Int coordinate)
        {
            if (coordinate.x >= startCoordinate.x && coordinate.x <= endCoordinate.x &&
            coordinate.y >= startCoordinate.y && coordinate.y <= endCoordinate.y)
            {
                int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
                int index = (coordinate.y - startCoordinate.y) + (coordinate.x - startCoordinate.x) * perRowGridCount;
                if (mapObjBarriers.ContainsKey(index))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }

    private Dictionary<int, RuntimeMapRoom> runtimeMapRooms = new Dictionary<int, RuntimeMapRoom>();
    private NativeList<RoomCellData> roomCellDatas;
    public int3 GetRandomBehavioCell(int mapInstance, int areaId)
    {
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            return runtimeMapRoom.GetRandomBehaviorCell(areaId);
        }
        return int3.zero;
    }
    public int3 GetRandomBehavioCell(int mapInstance, BehaviorAreaType behaviorAreaType)
    {
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            return runtimeMapRoom.GetRandomBehaviorCell(behaviorAreaType);
        }
        return int3.zero;
    }
    public int2 GetRandomItemPlayerTriggerCell(int roomId,int itemInstanceId)
    {
        var cells = GetItemPlayerTriggerCells(itemInstanceId, roomId);
        int index = GameRandom.RandomInt(0, cells.Length);
        return cells[index];
    }
    public int2 GetNearestItemPlayerTriggerCell(int roomId, int itemInstanceId, int2 cell)
    {
        var cells = GetItemPlayerTriggerCells(itemInstanceId, roomId);
        if (cells != null && cells.Length > 0)
        {
            int index = 0;
            int distance = int.MaxValue;
            for (int i = 0; i < cells.Length; i++)
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
        var cells = GetItemTriggerCells(itemInstanceId, roomId);
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        for (int i = 0; i < cells.Length; i++)
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
        var cells = GetItemTriggerCells(itemInstanceId, roomId);
        if (cells != null && cells.Length > 0)
        {
            int index = 0;
            int distance = int.MaxValue;
            for (int i = 0; i < cells.Length; i++)
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
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        {
            return roomCellDatas[runtimeMapRoom.roomCellDataIndex].GetRandomCanWalkCell();
        }

        return new int2(int.MinValue, int.MinValue);
    }

    public int2[] GetItemTriggerCells(int instanceId, int room)
    {
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            var cells = runtimeMapRoom.commonTriggerAreas.GetItemTriggerCells(instanceId);
            int2[] cellArray = cells.ToArray();
            cells.Dispose();
            return cellArray;
        }
        return null;
    }

    public int2[] GetItemPlayerTriggerCells(int instanceId, int room)
    {
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            var cells = runtimeMapRoom.playerTriggerAreas.GetItemTriggerCells(instanceId);
            int2[] cellArray = cells.ToArray();
            cells.Dispose();
            return cellArray;
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
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                roomCellDatas[runtimeMapRoom.roomCellDataIndex].AddBarrier(cells[i] + itemPos);

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
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                roomCellDatas[runtimeMapRoom.roomCellDataIndex].RemmoveBarrier(cells[i] + itemPos);
                /*
                int index = roomCellDatas[runtimeMapRoom.roomCellDataIndex].GetCoordinateIndex(cells[i] + itemPos);
                runtimeMapRoom.roomCellData.mapObjBarriers.TryGetValue(index, out int count);
                count--;
                if (count <= 0)
                {
                    runtimeMapRoom.roomCellData.mapObjBarriers.Remove(index);
                }*/
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
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            int4 triggerEvent = new int4((int)triggerType, enterEventId, exitEventId, linkId);

            TriggerArea triggerArea = new TriggerArea
            {
                cells = new UnsafeHashSet<int2>(cells.Length, Allocator.Persistent),
                enterLinkEventId = enterEventId,
                exitLinkEventId = exitEventId,
                referenceId = linkId,
                triggerType = triggerType
            };
            for (int i = 0; i < cells.Length; i++)
            {
                triggerArea.cells.Add(cells[i] + offset);
            }

            runtimeMapRoom.commonTriggerAreas.AddTriggerCell(triggerArea);
            // runtimeMapRooms.SetData(runtimeMapRoom);
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
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom.commonTriggerAreas.RemoveTriggerCell(linkId);
            //runtimeMapRooms.SetData(runtimeMapRoom);
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
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            TriggerArea triggerArea = new TriggerArea
            {
                cells = new UnsafeHashSet<int2>(4, Allocator.Persistent),
                enterLinkEventId = enterEventId,
                referenceId = linkId,
            };

            for (int i = 0; i < cells.Length; i++)
            {
                triggerArea.cells.Add(cells[i] + offset);
            }
            runtimeMapRoom.playerTriggerAreas.AddTriggerCell(triggerArea);
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
        if (runtimeMapRooms.TryGetValue(room, out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom.playerTriggerAreas.RemoveTriggerCell(linkId);
            // runtimeMapRooms.SetData(runtimeMapRoom);
        }
    }

    public void CheckTriggerEvent(int entityId, EntityType entityType, int room, int2 cell, bool exit, TriggerEvent triggerEvent)
    {
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            NativeArray<int3> triggerEvents = new NativeArray<int3>
                (runtimeMapRoom.commonTriggerAreas.triggerAreas.Length, Allocator.Persistent);
            SingleTriggerJob triggerJob = new SingleTriggerJob
            {
                TriggerAreas = runtimeMapRoom.commonTriggerAreas.triggerAreas,
                cell = cell,
                exit = exit,
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
                    triggerEvent(eventId, triggerEvents[i].y, triggerEvents[i].z == 1, false);
                }
            }
            triggerEvents.Dispose();
        }
    }

    public void CheckPlayerTriggerEvent(int room, int2 cell, bool exit,
        TriggerEvent triggerEvent, int oldLink = 0, bool trueMove = true)
    {
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            NativeArray<int3> triggerEvents = new NativeArray<int3>
                (runtimeMapRoom.playerTriggerAreas.triggerAreas.Length, Allocator.Persistent);

            SingleTriggerPlayerJob triggerJob = new SingleTriggerPlayerJob
            {
                TriggerAreas = runtimeMapRoom.playerTriggerAreas.triggerAreas,
                cell = cell,
                exit = exit,
                oldLinkId = oldLink,
                triggerType = EntityType.玩家,
                triggerEvents = triggerEvents
            };
            //triggerJob.Run(triggerEvents.Length);
            triggerJob.Schedule(triggerEvents.Length, 8).Complete();
            int length = triggerJob.triggerEvents.Length;
            List<int3> enterEventDatas = new List<int3>();
            for (int i = 0; i < length; i++)
            {
                int eventId = triggerJob.triggerEvents[i].x;
                if (eventId == 0 && triggerJob.triggerEvents[i].y == 0)
                {
                    continue;
                }
                if (triggerEvents[i].z == 1)
                {
                    enterEventDatas.Add(triggerEvents[i]);
                }
                else if (trueMove)
                {
                    triggerEvent(eventId, triggerEvents[i].y, triggerEvents[i].z == 1, true);
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
        if (GetRuntimeMapRoom(room, out RuntimeMapRoom runtimeMapRoom))
        {
            NativeArray<int3> triggerEvents = new NativeArray<int3>
                (runtimeMapRoom.commonTriggerAreas.triggerAreas.Length, Allocator.TempJob);
            TriggerJob triggerJob = new TriggerJob
            {
                TriggerAreas = runtimeMapRoom.commonTriggerAreas.triggerAreas,
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
                    triggerEvent(eventId, triggerEvents[i].y, triggerEvents[i].z == 1, false);
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
        TriggerEvent triggerEvent, int oldLink = 0, bool trueMove = true)
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
                oldLinkId = oldLink,
                triggerType = EntityType.玩家,
                triggerEvents = triggerEvents
            };
            // triggerJob.Run(triggerEvents.Length);
            triggerJob.Schedule(triggerEvents.Length, 8).Complete();
            int length = triggerJob.triggerEvents.Length;
            List<int3> enterEventDatas = new List<int3>();
            for (int i = 0; i < length; i++)
            {
                int eventId = triggerJob.triggerEvents[i].x;
                if (eventId == 0 && triggerJob.triggerEvents[i].y == 0)
                {
                    continue;
                }
                if (triggerEvents[i].z == 1)
                {
                    enterEventDatas.Add(triggerEvents[i]);
                }
                else //if(trueMove)
                {
                    triggerEvent(eventId, triggerEvents[i].y, triggerEvents[i].z == 1, true);
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
        if (GetRuntimeMapRoom(mapInstance, out var runtimeMapRoom))
        {
            result = roomCellDatas[runtimeMapRoom.roomCellDataIndex].GetCoordinates(source, minRange, maxRange, isWalkable);
            return true;
        }
        result = null;
        return false;
    }

    public bool GetRuntimeMapRoom(int roomId, out RuntimeMapRoom runtimeMapRoom)
    {
        return runtimeMapRooms.TryGetValue(roomId, out runtimeMapRoom);
    }

    public bool ContainsRoom(int roomId)
    {
        return runtimeMapRooms.ContainsKey(roomId);
    }

    public int3 GetRoomCoordinate(int roomId)
    {
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        {
            return runtimeMapRoom.coordinate;
        }
        return int3.zero;
    }

    public List<Vector3Int> GetAllCellData(int mapInstance)
    {
        if (runtimeMapRooms.TryGetValue(mapInstance, out var runtimeMapRoom))
        {
            return roomCellDatas[runtimeMapRoom.roomCellDataIndex].GetAllCellData();
        }
        return null;
    }

    public MapTriggerAreas GetPlayerTrigger(int roomId)
    {
        if (runtimeMapRooms.TryGetValue(roomId, out var runtimeMapRoom))
        {
            return runtimeMapRoom.playerTriggerAreas;
        }
        return default(MapTriggerAreas);
    }

    public void InitMapData(int roomId, MapRoomData mapRoomData, int3 coordinate)
    {
        RoomCellData roomCellData = new RoomCellData
        {
            mapObjBarriers = new NativeHashMap<int, int>(16, Allocator.Persistent),
            startCoordinate = mapRoomData.startCoordinate,
            endCoordinate = mapRoomData.endCoordinate,
        };

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
                    roomCellData.AddBarrier(x, y);
                }
            }
        }
        roomCellDatas.Add(roomCellData);
        RuntimeMapRoom runtimeMapRoom = new RuntimeMapRoom
        {
            coordinate = coordinate,
            roomCellDataIndex = roomCellDatas.Length - 1,
            id = roomId,
            linkMapIndexs = new NativeHashMap<int2, int>(16, Allocator.Persistent),
            linkMaps = new NativeList<int4>(16, Allocator.Persistent),
            linkActions = new NativeList<int3>(16, Allocator.Persistent),
            neighbourMaps = new NativeHashMap<int, int>(8, Allocator.Persistent),
        };
        runtimeMapRoom.SetNpcBehaviorAreas(mapRoomData.npcBehaviorAreas);
        runtimeMapRoom.InitTriggerData();
        runtimeMapRooms.Add(runtimeMapRoom.Key, runtimeMapRoom);
    }

    private Direction GetMapDirection(int map0, int map1, int2 coordinate0, int2 coordinate1)
    {
        if (runtimeMapRooms.TryGetValue(map0, out var runtimeMapRoom0) &&
            runtimeMapRooms.TryGetValue(map1, out var runtimeMapRoom1))
        {
            coordinate0.x += runtimeMapRoom0.coordinate.x;
            coordinate0.y += runtimeMapRoom0.coordinate.y;

            coordinate1.x += runtimeMapRoom1.coordinate.x;
            coordinate1.y += runtimeMapRoom1.coordinate.y;

            return GameCommon.GetDirect(coordinate0, coordinate1);
        }
        return Direction.LEFT;
    }

    public bool GetLinkMapInCoordinate(int nowMap, int linkMap, ref int2 inCoordinate)
    {
        if (GetRuntimeMapRoom(nowMap, out RuntimeMapRoom nowRuntimeMap))
        {
            return nowRuntimeMap.GetLinkMapInCoordinate(linkMap, ref inCoordinate);
        }
        return false;
    }

    public bool ChangeMap(int2 nowCoordinate, Direction direction, int nowMap, out int3 newMap)
    {
        if (GetRuntimeMapRoom(nowMap, out RuntimeMapRoom runtimeMapRoom))
        {
            return runtimeMapRoom.ChangeMap(nowCoordinate, direction, out newMap);
        }
        newMap = int3.zero;
        return false;
    }

    public void ChangeMapAction(int2 nowCoordinate, Direction direction, int nowMap, Int3Action action)
    {
        int3 newMap = int3.zero;
        if (GetRuntimeMapRoom(nowMap, out RuntimeMapRoom runtimeMapRoom))
        {
            if (runtimeMapRoom.ChangeMap(nowCoordinate, direction, out newMap, out var changeAction))
            {
                if (changeAction.z != 0)
                {
                    GameActionDataManager.instance.Action(changeAction.z, (bool value) =>
                    {
                        if (value)
                        {
                            ChangeMap();
                        }
                    }, true);
                }
                else
                {
                    ChangeMap();
                }

                async void ChangeMap()
                {
                    if (changeAction.x != 0)
                    {
                        var actionData = await GameDataManager.instance.GetAsyncData<GameActionData>(changeAction.x);
                        actionData.Action(setResult: (bool value) =>
                        {
                            action.Invoke(newMap, changeAction.y);
                        });
                    }
                    else
                    {
                        action.Invoke(newMap);
                    }
                }
            }
        }
    }

    public void InitLinkMap(List<MapLine> mapLines)
    {
        foreach (var mapLine in mapLines)
        {
            bool isInit = true;
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
            if (!isInit)
            {
                continue;
            }
            InitLinkMap(mapLine);
        }
    }

    public void DeleteMapLink(MapLine mapLine)
    {
        if (runtimeMapRooms.TryGetValue(mapLine.map0, out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom = runtimeMapRoom.DeleteLinkMap(mapLine.cells0);
            // runtimeMapRooms.SetData(runtimeMapRoom);
        }

        if (runtimeMapRooms.TryGetValue(mapLine.map1, out RuntimeMapRoom _runtimeMapRoom))
        {
            _runtimeMapRoom = _runtimeMapRoom.DeleteLinkMap(mapLine.cells1);
            // runtimeMapRooms.SetData(_runtimeMapRoom);
        }
    }

    public void InitLinkMap(MapLine mapLine)
    {
        if (runtimeMapRooms.TryGetValue(mapLine.map0, out RuntimeMapRoom runtimeMapRoom))
        {
            runtimeMapRoom.AddLinkMap(mapLine.cells0);
            // runtimeMapRooms.SetData(runtimeMapRoom);
        }

        if (runtimeMapRooms.TryGetValue(mapLine.map1, out RuntimeMapRoom _runtimeMapRoom))
        {
            _runtimeMapRoom.AddLinkMap(mapLine.cells1);
            // runtimeMapRooms.SetData(_runtimeMapRoom);
        }
    }

    public Stack<int2> FindPathNode(int2 startPos, int2 targetPos, int mapId, bool Nearest = false)
    {
        Stack<int2> outData = FindPathNode(startPos, targetPos, mapId);
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
                    outData = FindPathNode(startPos, _targetPos, mapId);
                    if (outData.Count != 0)
                    {
                        return outData;
                    }
                }
                index++;
            }
        }
        return outData;
    }

    public Stack<int2> FindSamplePathNode(int2 startPos, int2 targetPos, int mapId, int2[] cells)
    {
        Stack<int2> outData = new Stack<int2>();
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            // RoomCellData roomCellData = roomCellDatas[runtimeMapRoom.roomCellDataIndex];
            NativeList<int2> pathCells = new NativeList<int2>(16, Allocator.TempJob);
            NativeHashSet<int2> cellSets = new NativeHashSet<int2>(16, Allocator.TempJob);
            for (int i = 0; i < cells.Length; i++)
            {
                cellSets.Add(cells[i]);
            }

            SampleFindPath findPath = new SampleFindPath
            {
                //roomCellData = &roomCellDatas[runtimeMapRoom.roomCellDataIndex],
                startPos = new int2(startPos.x, startPos.y),
                targetPos = new int2(targetPos.x, targetPos.y),
                pathCells = pathCells,
                cells = cellSets
            };

            findPath.Schedule().Complete();
            //findPath.Run();

            for (int i = 0; i < findPath.pathCells.Length; i++)
            {
                outData.Push(findPath.pathCells[i]);
            }
            cellSets.Dispose();
            pathCells.Dispose();
        }
        return outData;
    }

    public Stack<int2> FindPathNode(int2 startPos, int2 targetPos, int mapId)
    {
        Stack<int2> outData = new Stack<int2>();
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            // RoomCellData roomCellData = roomCellDatas[runtimeMapRoom.roomCellDataIndex];
            NativeList<int2> pathCells = new NativeList<int2>(16, Allocator.TempJob);
            FindPath findPath = new FindPath
            {
                startCoordinate = roomCellDatas[runtimeMapRoom.roomCellDataIndex].startCoordinate,
                endCoordinate = roomCellDatas[runtimeMapRoom.roomCellDataIndex].endCoordinate,
                mapObjBarriers = roomCellDatas[runtimeMapRoom.roomCellDataIndex].mapObjBarriers,
                startPos = new int2(startPos.x, startPos.y),
                targetPos = new int2(targetPos.x, targetPos.y),
                pathCells = pathCells
            };

            // findPath.Schedule().Complete();
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
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            return roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(coordinate);
        }
        return false;
    }

    public bool CheckIsWalk(int3 coordinate)
    {
        if (runtimeMapRooms.TryGetValue(coordinate.z, out RuntimeMapRoom runtimeMapRoom))
        {
            return roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(coordinate.xy);
        }
        return false;
    }

    public bool CheckFutureIsWalk(int2[] cells, int mapId, HashSet<int2> specialCells)
    {
        if (runtimeMapRooms.TryGetValue(mapId, out var runtimeMapRoom))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (!roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(cells[i]))
                {
                    if (specialCells != null)
                    {
                        if (specialCells.Contains(cells[i]) && roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckBarrierCount(cells[i]) <= 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    public bool CheckIsWalk(int2 coordinate, int mapId)
    {
        if (runtimeMapRooms.TryGetValue(mapId, out RuntimeMapRoom runtimeMapRoom))
        {
            return roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(coordinate);
        }
        return false;
    }

    private NativeHashMap<int, int> GetRoomNeighbours(int roomId)
    {
        if (runtimeMapRooms.TryGetValue(roomId, out RuntimeMapRoom sourceRoom))
        {
            return sourceRoom.neighbourMaps;
        }
        return default(NativeHashMap<int, int>);
    }

    public Queue<int> FindRoomList(int sourceId, int targetId, ref bool result)
    {
        if (sourceId == targetId)
        {
            result = true;
            return new Queue<int>();
        }

        Queue<int> roomList = new Queue<int>();
        Dictionary<int, int> links = new Dictionary<int, int>();
        List<int> nowList = new List<int>();
        HashSet<int> checkRoom = new HashSet<int>();
        checkRoom.Add(targetId);
        nowList.Add(targetId);

        for (int i = 0; i < nowList.Count; i++)
        {
            int checkId = nowList[i];
            var Neighbours = GetRoomNeighbours(checkId);
            if (Neighbours.IsEmpty)
            {
                result = false;
                return new Queue<int>();
            }
            foreach (var neighbour in Neighbours)
            {
                if (!checkRoom.Contains(neighbour.Key))
                {
                    links[neighbour.Key] = targetId;
                    checkRoom.Add(neighbour.Key);
                    nowList.Add(neighbour.Key);
                    if (sourceId == neighbour.Key)
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
                    roomList.Enqueue(_roomId);
                }
                if (_roomId == targetId)
                {
                    return roomList;
                }
            }
        }
        return null;
    }

    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<RemoveCellCharacter>(RemoveCellCharacter);
        roomCellDatas = new NativeList<RoomCellData>(16, Allocator.Persistent);
    }

    private void RemoveCellCharacter(RemoveCellCharacter removeCellCharacter)
    {
        if (removeCellCharacter.cell.Equals(int3.zero))
        {
            Character character = CharacterManager.instance.GetCharacter(removeCellCharacter.characterId);
            RemoveCharacterCoordinate(character.ObjCoordinate, removeCellCharacter.characterId);
        }
        else
        {
            RemoveCharacterCoordinate(removeCellCharacter.cell, removeCellCharacter.characterId);
        }
    }

    public bool CheckTryMoveTarget(int2 startCoordinate, int2 targetCoordinate, int mapInstace)
    {
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
                return roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(nowX, nowY);
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
                return roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(nowX, nowY);
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
                return roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(nowX, nowY);
            }
        }
        return false;
    }

    public int2 GetTrueFreedomTarget(int2 startCoordinate, int2 targetCoordinate, int mapInstace)
    {
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
                    if (!roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(nowX, nowY))
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
                    if (!roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(nowX, nowY))
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
                    if (!roomCellDatas[runtimeMapRoom.roomCellDataIndex].CheckWalkable(nowX, nowY))
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
        foreach (var data in MapCharacterGrids)
        {
            data.Value.Dispose();
        }
        MapCharacterGrids.Clear();
        for (int i = 0; i < roomCellDatas.Length; i++)
        {
            roomCellDatas[i].Dispose();
        }
        roomCellDatas.Dispose();
        foreach (var runtimeMapRoom in runtimeMapRooms)
        {
            runtimeMapRoom.Value.Dispose();
        }
        runtimeMapRooms.Clear();
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

    public struct MinCellDataList
    {
        private NativeList<int3> datas;
        private NativeHashMap<int, int> nextDatas;
        private NativeHashMap<int, int> forwardDatas;
        public int length;
        public int minIndex;

        public MinCellDataList(int capacity, Allocator allocator)
        {
            datas = new NativeList<int3>(capacity, allocator);
            length = 0;
            nextDatas = new NativeHashMap<int, int>(capacity, allocator);
            minIndex = 0;
            forwardDatas = new NativeHashMap<int, int>(capacity, allocator);
        }

        public void Add(int3 t)
        {
            if (length < datas.Length)
            {
                datas[length] = t;
            }
            else
            {
                datas.Add(t);
            }

            length++;

            if (length > 1)
            {
                if (t.z < datas[minIndex].z)
                {
                    nextDatas.Add(length - 1, minIndex);
                    forwardDatas.Add(minIndex, length - 1);
                    minIndex = length - 1;
                }
                else
                {
                    SetSortLinkIndex(minIndex, length - 1, t.z);
                }
            }
        }

        private void SetSortLinkIndex(int nowIndex, int targetIndex, int targetValue)
        {
            if (nextDatas.TryGetValue(nowIndex, out var nextIndex))
            {
                if (targetValue < datas[nextIndex].z)
                {
                    forwardDatas[nextIndex] = targetIndex;
                    forwardDatas[targetIndex] = nowIndex;

                    nextDatas[nowIndex] = targetIndex;
                    nextDatas[targetIndex] = nextIndex;
                }
                else
                {
                    SetSortLinkIndex(nextIndex, targetIndex, targetValue);
                }
            }
            else
            {
                nextDatas.Add(nowIndex, targetIndex);
                forwardDatas.Add(targetIndex, nowIndex);
            }
        }

        public int2 GetData()
        {
            int2 result = datas[minIndex].xy;
            RemoveMin();
            return result;
        }

        public void RemoveMin()
        {
            if (length > 0)
            {
                if (length > 1)
                {
                    var deathData = datas[minIndex];
                    int oldMinIndex = minIndex;
                    int lastIndex = length - 1;

                    int nextIndex = nextDatas[minIndex];
                    forwardDatas.Remove(nextIndex);
                    nextDatas.Remove(minIndex);

                    if (nextIndex != lastIndex)
                    {
                        minIndex = nextIndex;
                    }
                    datas[oldMinIndex] = datas[lastIndex];
                    datas[lastIndex] = deathData;
                    if (forwardDatas.TryGetValue(lastIndex, out var _forwardIndex))
                    {
                        nextDatas[_forwardIndex] = oldMinIndex;
                        forwardDatas.Remove(lastIndex);
                        forwardDatas[oldMinIndex] = _forwardIndex;
                    }
                    if (nextDatas.TryGetValue(lastIndex, out var _nextIndex))
                    {
                        forwardDatas[_nextIndex] = oldMinIndex;
                        nextDatas.Remove(lastIndex);
                        nextDatas[oldMinIndex] = _nextIndex;
                    }
                }
                length--;
            }
        }

        public void Dispose()
        {
            datas.Dispose();
            nextDatas.Dispose();
        }
    }

    [BurstCompile]
    public struct FindCharacterRangeInCell : IJobFor
    {
        [ReadOnly]
        public NativeList<int3> girds;
        [ReadOnly]
        public int range;
        [ReadOnly]
        public int2 coordinate;

        [WriteOnly]
        public NativeList<int>.ParallelWriter result;

        public void Execute(int index)
        {
            int3 target = girds[index];
            int minX = target.x - range;
            int minY = target.y - range;
            int maxX = target.x + range;
            int maxY = target.y + range;
            if (minX <= coordinate.x && minY <= coordinate.y && maxX> coordinate.x && maxY> coordinate.y)
            {
                result.AddNoResize(target.z);
            }
        }
    }

    [BurstCompile]
    public struct SampleFindPath : IJob
    {
        //[ReadOnly] public int MOVE_STRAIGHT_COST;
        // [ReadOnly] public int MOVE_DIAGONAL_COST;
        // [NativeDisableUnsafePtrRestriction]
        // [ReadOnly] public unsafe RoomCellData* roomCellData;

        [ReadOnly] public NativeHashSet<int2> cells;
        [ReadOnly] public int2 startPos, targetPos;

        [WriteOnly] public NativeList<int2> pathCells;

        public void Execute()
        {
            GetPath();
        }

        private void GetPath()
        {
            if (cells.Contains(startPos) && cells.Contains(targetPos))
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

                NativeHashMap<int2, int2> parentCell = new NativeHashMap<int2, int2>(16, Allocator.Temp);
                MinCellDataList openCellList = new MinCellDataList(16, Allocator.Temp);
                NativeHashSet<int2> checkedCell = new NativeHashSet<int2>(16, Allocator.Temp);

                openCellList.Add(new int3(startPos, 0));
                int2 nowCell = startPos;
                while (openCellList.length > 0)
                {
                    nowCell = openCellList.GetData();
                    checkedCell.Add(nowCell);
                    //openCells.RemoveAtSwapBack(0);

                    if (nowCell.x == targetPos.x && nowCell.y == targetPos.y)
                    {
                        break;
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        int2 cell = neighbourOffsetArray[i] + nowCell;
                        if (checkedCell.Contains(cell))
                        {
                            continue;
                        }
                        if (!cells.Contains(cell))
                        {
                            continue;
                        }

                        int cost = CalculateDistanceCost(cell, startPos) +
                             CalculateDistanceCost(cell, targetPos);
                        parentCell[cell] = nowCell;
                        checkedCell.Add(cell);
                        openCellList.Add(new int3(cell, cost));

                        if (cell.x == targetPos.x && cell.y == targetPos.y)
                        {
                            break;
                        }
                    }
                }

                while (nowCell.Equals(targetPos))
                {
                    pathCells.Add(nowCell);
                    if (!parentCell.TryGetValue(nowCell, out nowCell))
                    {
                        break;
                    }
                }
                neighbourOffsetArray.Dispose();
                parentCell.Dispose();
                checkedCell.Dispose();
            }
        }
    }

    [BurstCompile]
    public struct FindPath : IJob
    {
        //[ReadOnly] public int MOVE_STRAIGHT_COST;
        // [ReadOnly] public int MOVE_DIAGONAL_COST;

        [ReadOnly] public NativeHashMap<int, int> mapObjBarriers;
        [ReadOnly] public int2 startCoordinate, endCoordinate;

        [ReadOnly] public int2 startPos, targetPos;

        [WriteOnly] public NativeList<int2> pathCells;

        public void Execute()
        {
            GetPath();
        }

        public bool CheckWalkable(int2 coordinate)
        {
            if (coordinate.x >= startCoordinate.x && coordinate.x <= endCoordinate.x &&
            coordinate.y >= startCoordinate.y && coordinate.y <= endCoordinate.y)
            {
                int index = GetCoordinateIndex(coordinate);
                if (mapObjBarriers.ContainsKey(index))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public int GetCoordinateIndex(int2 coordinate)
        {
            int perRowGridCount = endCoordinate.y - startCoordinate.y + 1;
            int index = (coordinate.y - startCoordinate.y) + (coordinate.x - startCoordinate.x) * perRowGridCount;
            return index;
        }

        private void GetPath()
        {
            if (CheckWalkable(startPos) && CheckWalkable(targetPos))
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

                //NativeList<int2> closeCells = new NativeList<int2>(Allocator.Temp);
                NativeHashMap<int2, int2> parentCell = new NativeHashMap<int2, int2>(16, Allocator.Temp);

                MinCellDataList openCellList = new MinCellDataList(16, Allocator.Temp);
                NativeHashSet<int2> checkedCell = new NativeHashSet<int2>(16, Allocator.Temp);
                //int closeCellLength = 0;

                openCellList.Add(new int3(startPos, 0));

                // openCells.Add(startPos);
                // openCellLength++;
                // int cost = CalculateDistanceCost(startPos, targetPos) * 5;
                int2 nowCell = startPos;
                while (openCellList.length > 0)
                {
                    nowCell = openCellList.GetData();
                    checkedCell.Add(nowCell);
                    //openCells.RemoveAt(0);
                    //openCells.RemoveAtSwapBack(0);
                    //openCellLength--;
                    //closeCells.Add(nowCell);
                    //closeCellLength++;
                    if (nowCell.x == targetPos.x && nowCell.y == targetPos.y)
                    {
                        break;
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        int2 cell = neighbourOffsetArray[i] + nowCell;

                        if (!checkedCell.Contains(cell))
                        {
                            if (!CheckWalkable(cell))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        int cost = CalculateDistanceCost(cell, startPos) +
                             CalculateDistanceCost(cell, targetPos) * 1;
                        parentCell[cell] = nowCell;
                        checkedCell.Add(cell);
                        openCellList.Add(new int3(cell, cost));
                        if (cell.x == targetPos.x && cell.y == targetPos.y)
                        {
                            break;
                        }
                    }
                }

                // var checkCell = nowCell;

                while (!nowCell.Equals(startPos))
                {
                    pathCells.Add(nowCell);
                    if (!parentCell.TryGetValue(nowCell, out nowCell))
                    {
                        break;
                    }
                }

                neighbourOffsetArray.Dispose();
                parentCell.Dispose();
                checkedCell.Dispose();
            }
        }
    }

    [BurstCompile]
    public struct SingleTriggerJob : IJobParallelFor
    {
        // [ReadOnly] public BlobAssetReference<TriggerAreaAsset> triggerAreaAssetRef;
        [ReadOnly] public NativeList<TriggerArea> TriggerAreas;

        [WriteOnly] public NativeArray<int3> triggerEvents;
        [ReadOnly] public int2 cell;
        [ReadOnly] public bool exit;
        [ReadOnly] public EntityType triggerType;

        public void Execute(int index)
        {
            TriggerArea triggerArea = TriggerAreas[index];
            int typeValue = (int)triggerArea.triggerType % (int)triggerType;
            if (typeValue > 0)
            {
                return;
            }

            if (triggerArea.cells.Contains(cell))
            {
                if (exit)
                {
                    //离开事件
                    triggerEvents[index] = new int3(triggerArea.exitLinkEventId, triggerArea.referenceId, 0);
                }
                else
                {
                    //进入事件
                    triggerEvents[index] = new int3(triggerArea.enterLinkEventId, triggerArea.referenceId, 1);
                }
            }
        }
    }

    [BurstCompile]
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

    [BurstCompile]
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

    [BurstCompile]
    public struct SingleTriggerPlayerJob : IJobParallelFor
    {
        // [ReadOnly] public BlobAssetReference<TriggerAreaAsset> triggerAreaAssetRef;
        [ReadOnly] public NativeList<TriggerArea> TriggerAreas;

        [WriteOnly] public NativeArray<int3> triggerEvents;
        [ReadOnly] public int oldLinkId;
        [ReadOnly] public int2 cell;
        [ReadOnly] public bool exit;
        [ReadOnly] public EntityType triggerType;

        public void Execute(int index)
        {
            TriggerArea triggerArea = TriggerAreas[index];
            int typeValue = (int)triggerArea.triggerType % (int)triggerType;
            if (typeValue > 0)
            {
                return;
            }
            bool contanins = triggerArea.cells.Contains(cell);
            if (contanins)
            {
                if (triggerArea.referenceId == oldLinkId && exit)
                {
                    //离开事件
                    triggerEvents[index] = new int3(triggerArea.exitLinkEventId, triggerArea.referenceId, 0);
                }
                if (!exit && triggerArea.referenceId != oldLinkId)
                {
                    //进入
                    triggerEvents[index] = new int3(triggerArea.enterLinkEventId, triggerArea.referenceId, 1);
                }
            }
        }
    }

    /*
    public struct CellsDistanceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int2> cells;
        [ReadOnly] public int2 source;
        [WriteOnly] public NativeArray<int2> distances;
    }*/
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