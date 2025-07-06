using BehaviorDesigner.Runtime.Tasks.Unity.UnityPlayerPrefs;
using NativeCollections;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class PathUtil64
{
    public static ulong Pack(ulong fCost, ulong cameFrom, ulong state)
    {
        return (fCost << 40) | (cameFrom << 4) | (state & 0xFuL);
    }

    public static void Unpack(ulong packed, out ulong fCost, out ulong cameFrom, out ulong state)
    {
        state = packed & 0xFuL;
        cameFrom = (packed >> 4) & 0xFFFFFFFFFL;
        fCost = packed >> 40;
    }

    public static ulong SetState(ulong packed, ulong state)
    {
        return (packed & ~0xFuL) | (state & 0xFuL);
    }
}

public delegate void MoveWithPath(Stack<int2> path);

[BurstCompile]
public struct SparsePathfindingSIMDJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PathRequest> requests;
    [ReadOnly] public NativeArray<int4> mapRanges;
    [ReadOnly] public NativeParallelHashMap<uint, ushort>.ReadOnly barrierMap;
    [NativeDisableParallelForRestriction] public NativeArray<uint> openCells;
    [NativeDisableParallelForRestriction] public NativeArray<uint> openCosts;
    public NativeArray<int> openCounts;
    public int capacityPerMap;
     
    public NativeConcurrentMap<uint, ulong>.ParallelWriter cellMap;
    public NativeConcurrentMap<uint, ulong>.ParallelReader readCellMap;
    public NativeStream.Writer pathWriter;

    public void Execute(int index)
    {
        Debug.Log($"execute index:{index}");
        var req = requests[index];
        var mapRange = mapRanges[index];
        int2 start = req.start;
        int2 end = req.end;

        int baseOffset = index * capacityPerMap;
        int count = 0;

        uint startFlat = GetCoordinateIndex(start.x, start.y, mapRange.xy, mapRange.zw);
        uint endFlat = GetCoordinateIndex(end.x, end.y, mapRange.xy, mapRange.zw);
        uint keyStart = (uint)index * 1_000_000 + startFlat;
        cellMap.TryAdd(keyStart, PathUtil64.Pack(0, startFlat, 1));
       // Debug.Log($"新增keyStart{keyStart}--{index}");
        openCells[baseOffset + count] = startFlat;
        openCosts[baseOffset + count] = 0;
        count++;
        openCounts[index] = count;

        int triedAddCount = 0;
        uint currentFlat = 0;
        while (count > 0)
        {
            uint minCost = uint.MaxValue;
            int minIndex = -1;
            for (int i = 0; i < count; i++)
            {
                uint c = openCosts[baseOffset + i];
                if (c < minCost)
                {
                    minCost = c;
                    minIndex = i;
                }
            }
            if (minIndex == -1) break;

            currentFlat = openCells[baseOffset + minIndex];
            uint currentKey= (uint)index * 1_000_000 + currentFlat;
            // Debug.Log($"index={index}, count={count}, currentKey={currentKey}");
            count--;
            openCounts[index] = count;
            openCells[baseOffset + minIndex] = openCells[baseOffset + count];
            openCosts[baseOffset + minIndex] = openCosts[baseOffset + count];

            int2 current = end;
            ulong currentPacked=0;
      
            var result = readCellMap.TryGetValue(currentKey, out currentPacked);
            if (result != TryGetResult.Found)
            {
                Debug.LogError($"获取错误;{Enum.GetName(typeof(TryGetResult), result) ?? result.ToString()}");
            }
            //Debug.Log($"访问:{currentKey}");
            current = GetCoordinate(currentFlat, mapRange.xy, mapRange.zw);

            if (current.Equals(end))
            {
                Debug.Log($"发现路径{end}---{index}"); 
                break;
            } 

            PathUtil64.Unpack(currentPacked, out ulong gCost, out _, out _);
            if(!cellMap.TrySet(currentKey, PathUtil64.SetState(currentPacked, 2)))
            {
                Debug.LogError($"修改原值错误;{currentKey}");
            } 

            bool foundEnd = false;
         
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                { 
                    if (dx == 0 && dy == 0) continue;
                    int2 neighbor = current + new int2(dx, dy);


                    if (!InBounds(neighbor, mapRange.xy, mapRange.zw)) continue;

                    uint flat = GetCoordinateIndex(neighbor.x, neighbor.y, mapRange.xy, mapRange.zw);
                    uint baseKey = (uint)index * 1_000_000 + flat;
                    uint mapCellIndex = (uint)req.roomId * 1_000_000 + flat;

                   // Debug.Log($"index={index}, current={current}, neighbor={neighbor}, flat={flat}, baseKey={baseKey}");

                    if (barrierMap.ContainsKey(mapCellIndex))
                    {
                      //  Debug.Log($"index={index}, blocked by barrier: {mapCellIndex}");
                        continue;
                    }
                        

                    uint moveCost = (dx == 0 || dy == 0) ? 2u : 3u;
                    ulong newG = gCost + moveCost;

                    int dxCost = math.abs(neighbor.x - end.x);
                    int dyCost = math.abs(neighbor.y - end.y);
                    ulong h = (ulong)(math.min(dxCost, dyCost) * 3 + math.abs(dxCost - dyCost) * 2) * 3;
                    ulong fCost = newG + h;

                    result = readCellMap.TryGetValue(baseKey, out ulong old);

                    if (result == TryGetResult.Found)
                    {
                        PathUtil64.Unpack(old, out ulong oldF, out _, out ulong state);
                        if (state == 2) continue;
                        if (fCost < oldF)
                        {
                            //  Debug.Log($"保存:{baseKey}");
                            if (cellMap.TrySet(baseKey, PathUtil64.Pack(fCost, currentFlat, 1)))
                            {
                                openCells[baseOffset + count] = flat;
                                openCosts[baseOffset + count] = (uint)fCost;
                                count++;
                                openCounts[index] = count;
                            }
                            else
                            {
                                Debug.LogError($"修改原值错误111;{baseKey}");
                            }
                        }
                    }
                    else
                    {
                         //Debug.Log($"新增{baseKey}--{index}");
                        triedAddCount++;

                        if (!cellMap.TryAdd(baseKey, PathUtil64.Pack(fCost, currentFlat, 1)))
                        {
                            Debug.Log($"保存:{baseKey} 失败--{Enum.GetName(typeof(TryGetResult), result) ?? result.ToString()}");
                        }

                        openCells[baseOffset + count] = flat;
                        openCosts[baseOffset + count] = (uint)fCost;
                        count++;
                        openCounts[index] = count;

                        if (neighbor.Equals(end))
                        {
                            currentFlat = flat;
                            Debug.Log($"发现路径{end}---{index}");
                            foundEnd = true;
                            break;
                        }
                    }
                }
                if (foundEnd) break;
            }
           

            if (foundEnd) break;
        }
        //Debug.Log($"index={index} 尝试新增数量: {triedAddCount}");

        if (currentFlat != endFlat)
        {
            Debug.Log($"未发现路径");
        }
        else
        {
            pathWriter.BeginForEachIndex(index);
            
            for (int iter = 0; iter < 512; iter++)
            {
                uint key = (uint)index * 1_000_000 + currentFlat;
                var result = readCellMap.TryGetValue(key, out ulong packed);
                if (result != TryGetResult.Found)
                {
                    Debug.Log($"index:{index}---获取失败key:{key}--{Enum.GetName(typeof(TryGetResult), result) ?? result.ToString()}");
                    break;
                }
                // uint flat = currentFlat - (uint)index * 1_000_000;
                pathWriter.Write(GetCoordinate(currentFlat, mapRange.xy, mapRange.zw));

                if (currentFlat == startFlat) break;

                PathUtil64.Unpack(packed, out _, out ulong from, out _);
                if (from == currentFlat) break;
                currentFlat = (uint)from;
            }
            pathWriter.EndForEachIndex();
        }
       
    }

    public int2 GetCoordinate(uint index, int2 start, int2 end)
    {
        int w = end.y - start.y + 1;
        return new int2((int)index / w + start.x, (int)index % w + start.y);
    }

    public uint GetCoordinateIndex(int x, int y, int2 start, int2 end)
    {
        int w = end.y - start.y + 1;
        return (uint)((y - start.y) + (x - start.x) * w);
    }

    private bool InBounds(int2 p, int2 start, int2 end)
    {
        return p.x >= start.x && p.x <= end.x && p.y >= start.y && p.y <= end.y;
    }
}

public struct PathRequest
{
    public int2 start;
    public int2 end;
    public int roomId;
}

public class MapCellJobController : Singleton<MapCellJobController>
{
    public NativeList<PathRequest> pathRequests;
    public List<MoveWithPath> MoveWithPath = new List<MoveWithPath>();
    public override bool NeedUpdate => true;

    public override void Init()
    {
        base.Init();
        pathRequests = new NativeList<PathRequest>(Allocator.Persistent);
    }

    protected override void Clear()
    {
        base.Clear();
        pathRequests.Dispose();
    }

    protected override void Update()
    {
        base.Update();
        int requestCount = pathRequests.Length;
        if (requestCount == 0) return;

        const int capacityPerMap = 2048;
        int totalCapacity = requestCount * capacityPerMap;

        NativeArray<int4> mapRanges = new NativeArray<int4>(requestCount, Allocator.TempJob);
        for (int i = 0; i < requestCount; i++)
            mapRanges[i] = MapCellController.instance.GetRoomRange(pathRequests[i].roomId);

        NativeArray<uint> openCells = new NativeArray<uint>(totalCapacity, Allocator.TempJob);
        NativeArray<uint> openCosts = new NativeArray<uint>(totalCapacity, Allocator.TempJob);
        NativeArray<int> openCounts = new NativeArray<int>(requestCount, Allocator.TempJob);

        int estimatedCapacity = requestCount * 2048;
        int actualCapacity = math.ceilpow2(estimatedCapacity);
        NativeConcurrentMap<uint, ulong> cellMap = new NativeConcurrentMap<uint, ulong>(actualCapacity, Allocator.TempJob);
        NativeStream pathStream = new NativeStream(requestCount, Allocator.TempJob);

        var job = new SparsePathfindingSIMDJob
        {
            requests = pathRequests.AsArray(),
            mapRanges = mapRanges,
            openCells = openCells,
            openCosts = openCosts,
            openCounts = openCounts,
            capacityPerMap = capacityPerMap,
            cellMap = cellMap.AsParallelWriter(),
            readCellMap=cellMap.AsParallelReader(),
            barrierMap = MapCellController.instance.MapObjBarriers,
            pathWriter = pathStream.AsWriter()
        };
       // job.Run(requestCount);
        job.Schedule(requestCount, 2).Complete();

        var reader = pathStream.AsReader();
        for (int i = 0; i < pathStream.ForEachCount; i++)
        {
            reader.BeginForEachIndex(i);
            Stack<int2> path = new Stack<int2>();
            while (reader.RemainingItemCount > 0)
                path.Push(reader.Read<int2>());
            reader.EndForEachIndex();
            MoveWithPath[i].Invoke(path);
        }

        openCells.Dispose();
        openCosts.Dispose();
        openCounts.Dispose();
        cellMap.Dispose();
        mapRanges.Dispose();
        pathStream.Dispose();
        pathRequests.Clear();
        MoveWithPath.Clear();
    }

    public void AddPathRequest(int2 start, int2 end, int roomId, MoveWithPath moveWithPath)
    {
        pathRequests.Add(new PathRequest { start = start, end = end, roomId = roomId });
        MoveWithPath.Add(moveWithPath);
    }
}