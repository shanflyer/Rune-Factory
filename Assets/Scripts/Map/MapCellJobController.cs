using System;
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

[BurstCompile]
public unsafe struct SparsePathfindingJob : IJobParallelFor
{
    [ReadOnly] public NativeHashMap<int, byte> barrierMap;
    [NativeDisableParallelForRestriction] public NativeHashMap<int, uint> cellMap;
    public NativeStream.Writer pathWriter;

    public NativeArray<int2> startPositions;
    public NativeArray<int2> endPositions;

    public int2 gridSize;
    public int stride;

    [NativeDisableUnsafePtrRestriction] public NativeArray<IntPtr> openListPtrs;
    [NativeDisableUnsafePtrRestriction] public NativeArray<IntPtr> costMapPtrs;

    public void Execute(int index)
    {
        int2 start = startPositions[index];
        int2 end = endPositions[index];
        int startFlat = Flat(start);
        int endFlat = Flat(end);
        int keyStart = index * stride + startFlat;

        cellMap[keyStart] = PathUtil.Pack(0, (uint)startFlat, 1); // open

        NativeList<int>* openKeysPtr = (NativeList<int>*)openListPtrs[index];
        ref NativeList<int> openKeys = ref UnsafeUtility.AsRef<NativeList<int>>(openKeysPtr);

        NativeHashMap<int, uint>* fCostMapPtr = (NativeHashMap<int, uint>*)costMapPtrs[index];
        ref NativeHashMap<int, uint> fCostMap = ref UnsafeUtility.AsRef<NativeHashMap<int, uint>>(fCostMapPtr);

        openKeys.Add(keyStart);
        fCostMap[keyStart] = 0;

        while (openKeys.Length > 0)
        {
            int minIndex = -1;
            uint minCost = uint.MaxValue;
            for (int i = 0; i < openKeys.Length; i++)
            {
                int key = openKeys[i];
                if (!fCostMap.TryGetValue(key, out uint cost)) continue;
                if (cost < minCost)
                {
                    minCost = cost;
                    minIndex = i;
                }
            }

            if (minIndex == -1) break;

            int minKey = openKeys[minIndex];
            openKeys.RemoveAtSwapBack(minIndex);
            fCostMap.Remove(minKey);

            uint currentPacked = cellMap[minKey];
            int2 current = Unflat(minKey % stride);

            if (current.Equals(end)) break;

            PathUtil.Unpack(currentPacked, out uint gCost, out _, out _);
            cellMap[minKey] = PathUtil.SetState(currentPacked, 2); // closed

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int2 neighbor = current + new int2(dx, dy);
                    if (!InBounds(neighbor)) continue;
                    int neighborFlat = Flat(neighbor);
                    int key = index * stride + neighborFlat;
                    if (barrierMap.ContainsKey(neighborFlat)) continue;

                    uint newG = gCost + ((dx == 0 || dy == 0) ? 2u : 3u);
                    uint h = (uint)(math.abs(neighbor.x - end.x) + math.abs(neighbor.y - end.y)) * 2;
                    uint fCost = newG + h;

                    if (cellMap.TryGetValue(key, out uint existing))
                    {
                        PathUtil.Unpack(existing, out uint oldF, out _, out uint state);
                        if (state == 2) continue;
                        if (fCost < oldF)
                        {
                            cellMap[key] = PathUtil.Pack(fCost, (uint)(minKey % stride), 1);
                            openKeys.Add(key);
                            fCostMap[key] = fCost;
                        }
                    }
                    else
                    {
                        cellMap[key] = PathUtil.Pack(fCost, (uint)(minKey % stride), 1);
                        openKeys.Add(key);
                        fCostMap[key] = fCost;
                    }
                }
        }

        // 回溯路径
        pathWriter.BeginForEachIndex(index);
        int currentFlat = endFlat;
        bool reachable = false;
        while (true)
        {
            if (!cellMap.TryGetValue(index * stride + currentFlat, out uint packed)) break;
            PathUtil.Unpack(packed, out _, out uint from, out _);
            pathWriter.Write(Unflat(currentFlat));
            if (currentFlat == startFlat) { reachable = true; break; }
            currentFlat = (int)from;
        }
        pathWriter.Write(start); // 包括起点
        pathWriter.EndForEachIndex();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Flat(int2 pos) => pos.x * gridSize.y + pos.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int2 Unflat(int index) => new int2(index / gridSize.y, index % gridSize.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InBounds(int2 pos) => pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
}

public class MapCellJobController : Singleton<MapCellJobController>
{
}