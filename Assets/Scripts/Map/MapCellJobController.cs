using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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

    [NativeDisableParallelForRestriction] public NativeHashMap<uint, ulong> cellMap;
    public NativeStream.Writer pathWriter;

    public void Execute(int index)
    {
        var req = requests[index];
        var mapRange = mapRanges[index];
        int2 start = req.start;
        int2 end = req.end;

        int baseOffset = index * capacityPerMap;
        int count = 0;

        uint startFlat = GetCoordinateIndex(start.x, start.y, mapRange.xy, mapRange.zw);
        uint endFlat = GetCoordinateIndex(end.x, end.y, mapRange.xy, mapRange.zw);
        uint keyStart = (uint)index * 1_000_000 + startFlat;

        cellMap[keyStart] = PathUtil64.Pack(0, startFlat, 1);
        openCells[baseOffset + count] = keyStart;
        openCosts[baseOffset + count] = 0;
        count++;
        openCounts[index] = count;

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

            uint currentKey = openCells[baseOffset + minIndex];
            count--;
            openCounts[index] = count;
            openCells[baseOffset + minIndex] = openCells[baseOffset + count];
            openCosts[baseOffset + minIndex] = openCosts[baseOffset + count];

            ulong currentPacked = cellMap[currentKey];
            int2 current = GetCoordinate(currentKey, mapRange.xy, mapRange.zw);

            if (current.Equals(end)) break;

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
                    if (barrierMap.ContainsKey(mapCellIndex)) continue;

                    uint moveCost = (dx == 0 || dy == 0) ? 2u : 3u;
                    ulong newG = gCost + moveCost;

                    int dxCost = math.abs(neighbor.x - end.x);
                    int dyCost = math.abs(neighbor.y - end.y);
                    ulong h = (ulong)(math.min(dxCost, dyCost) * 3 + math.abs(dxCost - dyCost) * 2) * 3;
                    ulong fCost = newG + h;

                    if (cellMap.TryGetValue(baseKey, out ulong old))
                    {
                        PathUtil64.Unpack(old, out ulong oldF, out _, out ulong state);
                        if (state == 2) continue;
                        if (fCost < oldF)
                        {
                            cellMap[baseKey] = PathUtil64.Pack(fCost, currentKey, 1);
                            openCells[baseOffset + count] = baseKey;
                            openCosts[baseOffset + count] = (uint)fCost;
                            count++;
                            openCounts[index] = count;
                        }
                    }
                    else
                    {
                        cellMap[baseKey] = PathUtil64.Pack(fCost, currentKey, 1);
                        openCells[baseOffset + count] = baseKey;
                        openCosts[baseOffset + count] = (uint)fCost;
                        count++;
                        openCounts[index] = count;

                        if (neighbor.Equals(end))
                        {
                            foundEnd = true;
                            break;
                        }
                    }
                }
                if (foundEnd) break;
            }
             

            if (foundEnd) break;
        }

        pathWriter.BeginForEachIndex(index);
        uint currentFlat = endFlat;

        for (int iter = 0; iter < 512; iter++)
        {
            uint key = (uint)index * 1_000_000 + currentFlat;
            if (!cellMap.TryGetValue(key, out ulong packed)) break;

            pathWriter.Write(GetCoordinate(currentFlat, mapRange.xy, mapRange.zw));

            if (currentFlat == startFlat) break;

            PathUtil64.Unpack(packed, out _, out ulong from, out _);
            if (from == currentFlat) break;
            currentFlat = (uint)from;
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
    public override bool NeedUpdata => true;

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

    protected override void UpData()
    {
        base.UpData();
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
        NativeHashMap<uint, ulong> cellMap = new NativeHashMap<uint, ulong>(requestCount * 5000, Allocator.TempJob);
        NativeStream pathStream = new NativeStream(requestCount, Allocator.TempJob);

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
        job.Run(requestCount);
        // job.Schedule(requestCount, 1).Complete();

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