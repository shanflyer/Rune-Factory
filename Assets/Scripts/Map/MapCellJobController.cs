using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

public static class PathUtil
{
    public static uint Pack(uint fCost, uint cameFrom, uint state)
    {
        return (fCost << 17) | (cameFrom << 2) | (state & 0b11);
    }

    public static void Unpack(uint packed, out uint fCost, out uint cameFrom, out uint state)
    {
        state = packed & 0b11;
        cameFrom = (packed >> 2) & 0x7FFF;
        fCost = packed >> 17;
    }

    public static uint GetState(uint packed) => packed & 0b11;

    public static uint SetState(uint packed, uint state) => (packed & ~0b11u) | (state & 0b11u);
}

public delegate void MoveWithPath(Stack<int2> path);

[BurstCompile]
public unsafe struct SparsePathfindingSIMDJob : IJobParallelFor
{
    [ReadOnly] public NativeList<PathRequest> requests;
    [ReadOnly] public NativeArray<int4> mapRanges;
    [ReadOnly] public NativeArray<IntPtr> barrierMapPtrs;
    [NativeDisableParallelForRestriction] public NativeHashMap<int, uint> cellMap;
    public NativeStream.Writer pathWriter;

    [NativeDisableUnsafePtrRestriction] public NativeArray<IntPtr> openPtrs;
    [NativeDisableUnsafePtrRestriction] public NativeArray<IntPtr> costPtrs;

    public void Execute(int index)
    {
        var req = requests[index];
        var mapRange = mapRanges[index];
        var gridSize = mapRange.zw - mapRange.xy;
        int stride = gridSize.y;
        int2 start = req.start;
        int2 end = req.end;

        NativeHashMap<int, byte>* barrierPtr = (NativeHashMap<int, byte>*)barrierMapPtrs[req.roomId];
        ref NativeHashMap<int, ushort> barrierMap = ref UnsafeUtility.AsRef<NativeHashMap<int, ushort>>(barrierPtr);
        if (!barrierMap.IsCreated)
        {
            return;
        }

        NativeList<int>* openPtr = (NativeList<int>*)openPtrs[index];
        NativeList<uint>* costPtr = (NativeList<uint>*)costPtrs[index];
        ref NativeList<int> open = ref UnsafeUtility.AsRef<NativeList<int>>(openPtr);
        ref NativeList<uint> costList = ref UnsafeUtility.AsRef<NativeList<uint>>(costPtr);

        int startFlat = Flat(start, gridSize, mapRange.xy);
        int endFlat = Flat(end, gridSize, mapRange.xy);
        int keyStart = req.roomId * 1_000_000 + index * stride + startFlat;

        cellMap[keyStart] = PathUtil.Pack(0, (uint)startFlat, 1);
        open.Add(keyStart);
        costList.Add(0);
        while (open.Length > 0)
        {
            // SIMD 寻找最小 fCost
            uint minCost = uint.MaxValue;
            int minIndex = -1;

            int i = 0;
            for (; i <= open.Length - 4; i += 4)
            {
                uint4 costs = new uint4(costList[i], costList[i + 1], costList[i + 2], costList[i + 3]);
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

            for (; i < costList.Length; i++)
            {
                if (costList[i] < minCost)
                {
                    minCost = costList[i];
                    minIndex = i;
                }
            }

            if (minIndex == -1) break;

            int currentKey = open[minIndex];
            uint currentCost = costList[minIndex];
            open.RemoveAtSwapBack(minIndex);
            costList.RemoveAtSwapBack(minIndex);

            uint currentPacked = cellMap[currentKey];
            int2 current = Unflat(currentKey % stride, gridSize, mapRange.xy);

            if (current.Equals(end)) break;

            PathUtil.Unpack(currentPacked, out uint gCost, out _, out _);
            cellMap[currentKey] = PathUtil.SetState(currentPacked, 2); // closed

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int2 neighbor = current + new int2(dx, dy);
                    if (!InBounds(neighbor, mapRange.xy, mapRange.zw)) continue;

                    int flat = Flat(neighbor, gridSize, mapRange.xy);
                    int key = index * stride + flat;
                    if (barrierMap.ContainsKey(flat)) continue;

                    uint moveCost = (dx == 0 || dy == 0) ? 2u : 3u;
                    uint newG = gCost + moveCost;

                    int dxCost = math.abs(neighbor.x - end.x);
                    int dyCost = math.abs(neighbor.y - end.y);
                    uint h = (uint)(math.min(dxCost, dyCost) * 3 + math.abs(dxCost - dyCost) * 2);
                    uint fCost = newG + h;

                    if (cellMap.TryGetValue(key, out uint old))
                    {
                        PathUtil.Unpack(old, out uint oldF, out _, out uint state);
                        if (state == 2) continue;
                        if (fCost < oldF)
                        {
                            cellMap[key] = PathUtil.Pack(fCost, (uint)(currentKey % stride), 1);
                            open.Add(key);
                            costList.Add(fCost);
                        }
                    }
                    else
                    {
                        cellMap[key] = PathUtil.Pack(fCost, (uint)(currentKey % stride), 1);
                        open.Add(key);
                        costList.Add(fCost);
                    }
                }
        }

        // 路径回溯
        pathWriter.BeginForEachIndex(index);
        int currentFlat = endFlat;
        while (true)
        {
            int key = index * stride + currentFlat;
            if (!cellMap.TryGetValue(key, out uint packed)) break;
            PathUtil.Unpack(packed, out _, out uint from, out _);
            pathWriter.Write(Unflat(currentFlat, gridSize, mapRange.xy));
            if (currentFlat == startFlat) break;
            currentFlat = (int)from;
        }
        pathWriter.Write(start);
        pathWriter.EndForEachIndex();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Flat(int2 p, int2 size, int2 startCoordinate)
    {
        p = p - startCoordinate;
        return p.x * size.y + p.y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int2 Unflat(int i, int2 size, int2 startCoordinate)
    {
        var p = new int2(i / size.y, i % size.y);
        p = p + startCoordinate;
        return p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InBounds(int2 p, int2 startCoordinate, int2 endCoordinate)
    {
        return p.x >= startCoordinate.x && p.x < endCoordinate.x && p.y >= startCoordinate.y && p.y < endCoordinate.y;
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
    public List<MoveWithPath> MoveWithPath;
    public override bool NeedUpdata => true;

    public void AddPathRequest(int2 start, int2 end,int roomId, MoveWithPath moveWithPath)
    {
        pathRequests.Add(new PathRequest
        {
            start = start,
            end = end,
            roomId = roomId
        });
        MoveWithPath.Add(moveWithPath);
    }

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

    protected override unsafe void UpData()
    {
        base.UpData();
        int requestCount = pathRequests.Length;
        if (requestCount > 0)
        {
            NativeStream pathStream = new NativeStream(requestCount, Allocator.TempJob);
            NativeArray<int4> mapRanges = new NativeArray<int4>(requestCount, Allocator.TempJob);
            NativeArray<NativeList<int>> openLists = new NativeArray<NativeList<int>>(requestCount, Allocator.TempJob);
            NativeArray<NativeList<uint>> openCosts = new NativeArray<NativeList<uint>>(requestCount, Allocator.TempJob);
            NativeHashMap<int, ushort>[] barrierMaps = new NativeHashMap<int, ushort>[requestCount];

            NativeArray<IntPtr> openPtrs = new NativeArray<IntPtr>(requestCount, Allocator.TempJob);
            NativeArray<IntPtr> costPtrs = new NativeArray<IntPtr>(requestCount, Allocator.TempJob);
            NativeArray<IntPtr> barrierMapPtrs = new NativeArray<IntPtr>(requestCount, Allocator.TempJob);

            for (int i = 0; i < requestCount; i++)
            {
                int mapId = pathRequests[i].roomId;

                var mapBarrier = MapCellController.instance.GetRoomObjBarriers(mapId);
                barrierMaps[i] = mapBarrier;

                mapRanges[i] = MapCellController.instance.GetRoomRange(mapId);

                openLists[i] = new NativeList<int>(Allocator.TempJob);
                openCosts[i] = new NativeList<uint>(Allocator.TempJob);
                openPtrs[i] = (IntPtr)openLists[i].GetUnsafePtr();
                costPtrs[i] = (IntPtr)openCosts[i].GetUnsafePtr();

                fixed (NativeHashMap<int, ushort>* ptr = &barrierMaps[i]) // 必须 fixed 托管数组
                {
                    barrierMapPtrs[i] = (IntPtr)ptr;
                }
            }
            NativeHashMap<int, uint> cellMap = new NativeHashMap<int, uint>(requestCount * 5000, Allocator.TempJob); // 稀疏使用
            var job = new SparsePathfindingSIMDJob
            {
                requests = pathRequests,
                mapRanges = mapRanges,
                barrierMapPtrs = barrierMapPtrs,
                cellMap = cellMap,
                openPtrs = openPtrs,
                costPtrs = costPtrs,
                pathWriter = pathStream.AsWriter()
            };

            var handle = job.Schedule(requestCount, 1);
            handle.Complete();

            mapRanges.Dispose();
            for (int i = 0; i < requestCount; i++)
            {
                openLists[i].Dispose();
                openCosts[i].Dispose();
            }
            openLists.Dispose();
            openCosts.Dispose();

            var reader = pathStream.AsReader();
            for (int i = 0; i < pathStream.ForEachCount; i++)
            {
                reader.BeginForEachIndex(i);
                Stack<int2> path = new Stack<int2>();
                while (reader.RemainingItemCount > 0)
                {
                    int2 coord = reader.Read<int2>();
                    path.Push(coord);
                }
                MoveWithPath[i].Invoke(path);
            }
            pathStream.Dispose();
            pathRequests.Clear();
            MoveWithPath.Clear();
        }
    }
}