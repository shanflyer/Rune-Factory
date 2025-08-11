using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static UnityEngine.RuleTile.TilingRuleOutput;

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

public delegate void MoveWithPath(Stack<int2> path, int map);

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

    [NativeDisableParallelForRestriction]
    public NativeHashMap<uint, ulong> cellMap;

    public NativeStream.Writer pathWriter;

    public void Execute(int index)
    {
        // Debug.Log($"execute index:{index}");
        var req = requests[index];
        var mapRange = mapRanges[index];
        int2 start = req.start;
        int2 end = req.end;

        int baseOffset = index * capacityPerMap;
        int count = 0;

        uint startFlat = GetCoordinateIndex(start.x, start.y, mapRange.xy, mapRange.zw);
        uint endFlat = GetCoordinateIndex(end.x, end.y, mapRange.xy, mapRange.zw);
        uint keyStart = (uint)index * 1_000_000 + startFlat;
        var startPacked = PathUtil64.Pack(0, startFlat, 1);
        var addResult = cellMap.TryAdd(keyStart, startPacked);
        //Debug.Log($"startFlat:{startFlat}--endFlat:{endFlat}-新增keyStart{keyStart}--{index}--addResult:{addResult}");
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
            int i = 0;
            for (; i <= count - 4; i += 4)
            {
                uint4 costs = new uint4(openCosts[baseOffset + i], openCosts[baseOffset + i + 1], openCosts[baseOffset + i + 2], openCosts[baseOffset+i + 3]);
                bool4 mask = costs < minCost;
                if (math.any(mask))
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (costs[j] < minCost)
                        {
                            minCost = costs[j];
                            minIndex = i + j;
                        }
                    }
                }
            }

            for (; i < count; i++)
            {
                if (openCosts[baseOffset + i] < minCost)
                {
                    minCost = openCosts[baseOffset + i];
                    minIndex = i;
                }
            }
            if (minIndex == -1) break;

            currentFlat = openCells[baseOffset + minIndex];
            uint currentKey = (uint)index * 1_000_000 + currentFlat;
            // Debug.Log($"index={index}, count={count}, currentKey={currentKey}");

            int2 current = end;
            ulong currentPacked = 0;
            if (currentKey == keyStart)
            {
                currentPacked = startPacked;
            }
            else
            {
                var result = cellMap.TryGetValue(currentKey, out currentPacked);
                if (!result)
                {
#if UNITY_EDITOR
                    //  Debug.LogError($"startFlat:{startFlat}--endFlat:{endFlat}--获取错误--{currentKey}");
#endif
                }
            }

            count--;
            openCounts[index] = count;
            openCells[baseOffset + minIndex] = openCells[baseOffset + count];
            openCosts[baseOffset + minIndex] = openCosts[baseOffset + count];

            //Debug.Log($"访问:{currentKey}");
            current = GetCoordinate(currentFlat, mapRange.xy, mapRange.zw);

            if (math.all(current == end))
            {
#if UNITY_EDITOR
                // Debug.Log($"发现路径{end}---{index}");
#endif

                break;
            }

            PathUtil64.Unpack(currentPacked, out ulong gCost, out _, out _);
            cellMap[currentKey] = PathUtil64.SetState(currentPacked, 2);

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

                    var result = cellMap.TryGetValue(baseKey, out ulong old);

                    if (result)
                    {
                        PathUtil64.Unpack(old, out ulong oldF, out ulong cameFrom, out ulong state);

                        if (state == 2) continue; // 已关闭
                        if (cameFrom == currentFlat) continue; // 防止互指
                        if (currentFlat == flat) continue;    // 防止自指
                        if (fCost < oldF)
                        {
                            //  Debug.Log($"保存:{baseKey}");
                            cellMap[baseKey] = PathUtil64.Pack(fCost, currentFlat, 1);
                            openCells[baseOffset + count] = flat;
                            openCosts[baseOffset + count] = (uint)fCost;
                            count++;
                            openCounts[index] = count;
                        }
                    }
                    else
                    {
                        if (currentFlat != flat)
                        {
                            triedAddCount++;
                            if (!cellMap.TryAdd(baseKey, PathUtil64.Pack(fCost, currentFlat, 1)))
                            {
#if UNITY_EDITOR
                                // Debug.Log($"保存:{baseKey} 失败");
#endif
                            }

                            openCells[baseOffset + count] = flat;
                            openCosts[baseOffset + count] = (uint)fCost;
                            count++;
                            openCounts[index] = count;
                            if (math.all(neighbor == end)) 
                            {
                                currentFlat = flat;
                                foundEnd = true;
                                break;
                            }
                        }
                    }
                }
                if (foundEnd) break;
            }

            if (foundEnd) break;
        }
        //Debug.Log($"index={index} 尝试新增数量: {triedAddCount}");
        pathWriter.BeginForEachIndex(index);
        if (currentFlat != endFlat)
        {
#if UNITY_EDITOR
            //   Debug.Log($"未发现路径");
#endif
        }
        else
        {  
            // 新增循环检测
            var visited = new NativeHashSet<uint>(64, Allocator.Temp);

            for (int iter = 0; iter < 512; iter++)
            {
                // 检查循环
                if (!visited.Add(currentFlat))
                {
#if UNITY_EDITOR
                    // Debug.LogWarning($"检测到循环路径，提前终止 index:{index}");
#endif
                    break;
                }

                uint key = (uint)index * 1_000_000 + currentFlat;
                if (!cellMap.TryGetValue(key, out ulong packed))
                    break;

                pathWriter.Write(GetCoordinate(currentFlat, mapRange.xy, mapRange.zw));

                if (currentFlat == startFlat) break;

                PathUtil64.Unpack(packed, out _, out ulong from, out _);
                if (from == currentFlat) break;

                currentFlat = (uint)from;
            }

            visited.Dispose(); 
        }
        pathWriter.EndForEachIndex();
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

    private JobHandle pathJobHandle;
    private bool jobRunning;

    // 保存 Job 运行所需的临时数据
    private NativeArray<int4> mapRanges;
    private NativeArray<uint> openCells;
    private NativeArray<uint> openCosts;
    private NativeArray<int> openCounts;
    private NativeHashMap<uint, ulong> cellMap;
    private NativeStream pathStream;

    public override void Init()
    {
        base.Init();
        pathRequests = new NativeList<PathRequest>(Allocator.Persistent);
    }

    protected override void Clear()
    {
        base.Clear();
        if (jobRunning) pathJobHandle.Complete();
        pathRequests.Dispose();
    }

    protected override void Update()
    {
        base.Update();

        if (jobRunning) return; // 上一次任务还没完成，不要再调度
        int requestCount = pathRequests.Length;
        if (requestCount == 0) return;

        const int capacityPerMap = 2048;
        int totalCapacity = requestCount * capacityPerMap;

        mapRanges = new NativeArray<int4>(requestCount, Allocator.TempJob);
        for (int i = 0; i < requestCount; i++)
            mapRanges[i] = MapCellController.instance.GetRoomRange(pathRequests[i].roomId);

        openCells = new NativeArray<uint>(totalCapacity, Allocator.TempJob);
        openCosts = new NativeArray<uint>(totalCapacity, Allocator.TempJob);
        openCounts = new NativeArray<int>(requestCount, Allocator.TempJob);

        int estimatedCapacity = requestCount * 4096;
        int actualCapacity = math.ceilpow2(estimatedCapacity);
        cellMap = new NativeHashMap<uint, ulong>(actualCapacity, Allocator.TempJob);
        pathStream = new NativeStream(requestCount, Allocator.TempJob);

        var job = new SparsePathfindingSIMDJob
        {
            requests = pathRequests.AsArray(),
            mapRanges = mapRanges,
            openCells = openCells,
            openCosts = openCosts,
            openCounts = openCounts,
            capacityPerMap = capacityPerMap,
            cellMap = cellMap,
            barrierMap = MapCellController.instance.MapObjBarriers,
            pathWriter = pathStream.AsWriter()
        };

        pathJobHandle = job.Schedule(requestCount, 2);
        jobRunning = true;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (!jobRunning) return;

        if (pathJobHandle.IsCompleted)
        {
            pathJobHandle.Complete();

            var reader = pathStream.AsReader();
            for (int i = 0; i < pathStream.ForEachCount; i++)
            {
                reader.BeginForEachIndex(i);
                Stack<int2> path = new Stack<int2>();
                while (reader.RemainingItemCount > 0)
                    path.Push(reader.Read<int2>());
                reader.EndForEachIndex();

                MoveWithPath[i].Invoke(path, pathRequests[i].roomId);
            }

            // 释放
            openCells.Dispose();
            openCosts.Dispose();
            openCounts.Dispose();
            cellMap.Dispose();
            mapRanges.Dispose();
            pathStream.Dispose();

            // 清理
            pathRequests.Clear();
            MoveWithPath.Clear();
            jobRunning = false;
        }
    }

    public void AddPathRequest(int2 start, int2 end, int roomId, MoveWithPath moveWithPath)
    {
        pathRequests.Add(new PathRequest { start = start, end = end, roomId = roomId });
        MoveWithPath.Add(moveWithPath);
    }
}
