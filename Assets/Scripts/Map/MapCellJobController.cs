using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public delegate void MoveWithPath(Stack<int2> path, int map, int2 startCoordinate, int2 targetCoordinate);

[BurstCompile]
public struct SparsePathfindingSIMDJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<PathRequest> requests;
    [ReadOnly] public NativeArray<int4> mapRanges;
    [ReadOnly] public NativeParallelHashMap<uint, short>.ReadOnly barrierMap;

    // —— 方案B：每个请求的面积 & 该请求段的起始偏移（由控制器计算传入）——
    [ReadOnly] public NativeArray<int> areas;           // area[i] = (w*h) of request i

    [ReadOnly] public NativeArray<int> baseOffsets;     // prefix sum offsets per request

    // —— 每格数据（总数组，长度= Σ areas[i]）——
    // 代价：只保留 g；f 用 (newG+h) 现场算、只进 open 用
    [NativeDisableParallelForRestriction] public NativeArray<ushort> bestG;        // 初始=ushort.MaxValue，起点=0

    [NativeDisableParallelForRestriction] public NativeArray<ushort> parentFlat;
    [NativeDisableParallelForRestriction] public NativeArray<byte> nodeState;    // 0=未见,1=open,2=closed

    // —— open 集（总数组，长度= Σ areas[i]，每请求一段）——
    [NativeDisableParallelForRestriction] public NativeArray<ushort> openCells;    // flat（<= area-1）

    [NativeDisableParallelForRestriction] public NativeArray<ushort> openCosts;    // f= g+h（存为 ushort）
    public NativeArray<int> openCounts;                                            // 每请求当前 open 数量

    public NativeStream.Writer pathWriter;

    public void Execute(int index)
    {
        var req = requests[index];
        var range = mapRanges[index];

        int area = areas[index];
        int baseOff = baseOffsets[index];

        int2 start = req.start;
        int2 end = req.end;

        uint startFlat = GetCoordinateIndex(start.x, start.y, range.xy, range.zw);
        uint endFlat = GetCoordinateIndex(end.x, end.y, range.xy, range.zw);

        // 边界防御
        if (startFlat >= (uint)area || endFlat >= (uint)area)
        {
            pathWriter.BeginForEachIndex(index);
            pathWriter.EndForEachIndex();
            return;
        }

        // —— 起点入队 —— //
        int sSlot = baseOff + (int)startFlat;
        nodeState[sSlot] = 1;         // open
        bestG[sSlot] = 0;

        int count = 0;
        openCells[baseOff + count] = (ushort)startFlat;
        openCosts[baseOff + count] = 0;           // 也可写启发式 h(start)
        count++;
        openCounts[index] = count;

        uint currentFlat = 0;

        while (count > 0)
        {
            // —— 取 open 最小 f（向量化线扫）——
            uint minCost = uint.MaxValue; int minIdx = -1;

            int i = 0;
            for (; i <= count - 4; i += 4)
            {
                uint4 c = new uint4(
                    openCosts[baseOff + i],
                    openCosts[baseOff + i + 1],
                    openCosts[baseOff + i + 2],
                    openCosts[baseOff + i + 3]); // 注意 i+3
                if (c.x < minCost) { minCost = c.x; minIdx = i; }
                if (c.y < minCost) { minCost = c.y; minIdx = i + 1; }
                if (c.z < minCost) { minCost = c.z; minIdx = i + 2; }
                if (c.w < minCost) { minCost = c.w; minIdx = i + 3; }
            }
            for (; i < count; i++)
            {
                uint c = openCosts[baseOff + i];
                if (c < minCost) { minCost = c; minIdx = i; }
            }
            if (minIdx < 0) break;

            // pop
            currentFlat = openCells[baseOff + minIdx];
            count--;
            openCounts[index] = count;
            openCells[baseOff + minIdx] = openCells[baseOff + count];
            openCosts[baseOff + minIdx] = openCosts[baseOff + count];

            int2 current = GetCoordinate(currentFlat, range.xy, range.zw);
            if (math.all(current == end))
                break;

            uint currG = bestG[baseOff + (int)currentFlat];
            if (currG == ushort.MaxValue) currG = 0;

            nodeState[baseOff + (int)currentFlat] = 2; // closed

            bool foundEnd = false;

            // 8 邻域
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int2 nb = current + new int2(dx, dy);
                    if (!InBounds(nb, range.xy, range.zw)) continue;

                    uint flat = GetCoordinateIndex(nb.x, nb.y, range.xy, range.zw);
                    if (flat >= (uint)area) continue;

                    uint mapCellIndex = (uint)req.roomId * 1_000_000u + flat;
                    if (barrierMap.ContainsKey(mapCellIndex)) continue;

                    uint moveCost = (dx == 0 || dy == 0) ? 2u : 3u;
                    uint newG = currG + moveCost;
                    if (newG > ushort.MaxValue) newG = ushort.MaxValue; // 限幅，防溢出

                    int dxCost = math.abs(nb.x - end.x);
                    int dyCost = math.abs(nb.y - end.y);
                    uint h = (uint)((math.min(dxCost, dyCost) * 3 + math.abs(dxCost - dyCost) * 2) * 3);

                    uint fCost = newG + h;
                    if (fCost > ushort.MaxValue) fCost = ushort.MaxValue; // 限幅，便于存 ushort

                    int slot = baseOff + (int)flat;
                    byte st = nodeState[slot];

                    if (st == 0) // 未见：首次发现
                    {
                        nodeState[slot] = 1; // open
                        parentFlat[slot] = (ushort)currentFlat;
                        bestG[slot] = (ushort)newG;

                        // 入队
                        if (count < area)
                        {
                            openCells[baseOff + count] = (ushort)flat;
                            openCosts[baseOff + count] = (ushort)fCost;
                            count++;
                            openCounts[index] = count;
                        }

                        if (flat == endFlat)
                        {
                            currentFlat = flat;
                            foundEnd = true;
                            break;
                        }
                    }
                    else if (st != 2) // open：尝试改优（只看 g，更优 => f 也更优）
                    {
                        if (newG < bestG[slot])
                        {
                            parentFlat[slot] = (ushort)currentFlat;  // 更新父
                            bestG[slot] = (ushort)newG;

                            if (count < area)
                            {
                                openCells[baseOff + count] = (ushort)flat;
                                openCosts[baseOff + count] = (ushort)fCost;
                                count++;
                                openCounts[index] = count;
                            }
                        }
                    }
                    // closed 直接跳过
                }

            if (foundEnd) break;
        }

        // —— 回溯（只读 cameFromDir）——
        pathWriter.BeginForEachIndex(index);
        if (currentFlat == endFlat)
        {
            int w = range.z - range.x + 1;
            int h = range.w - range.y + 1;
            int maxSteps = math.min(w * h, areas[index]);

            for (int t = 0; t < maxSteps; t++)
            {
                pathWriter.Write(GetCoordinate(currentFlat, range.xy, range.zw));
                if (currentFlat == startFlat) break;

                int slot = baseOff + (int)currentFlat;
                ushort p = parentFlat[slot];
                if (p == 0xFFFF || p == (ushort)currentFlat) break; // 无父/自指保护
                currentFlat = p;
            }
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
    public override bool NeedLateUpdate => true;

    private JobHandle pathJobHandle;
    private bool jobRunning;

    // —— 本批资源 —— //
    private NativeArray<int4> mapRanges;

    private NativeArray<int> areas;
    private NativeArray<int> baseOffsets;

    private NativeArray<ushort> openCells;
    private NativeArray<ushort> openCosts;
    private NativeArray<int> openCounts;

    private NativeArray<ushort> bestG;
    private NativeArray<ushort> parentFlat;
    private NativeArray<byte> nodeState;

    private NativeStream pathStream;

    // —— 快照（这批要跑的请求/回调）—— //
    private NativeArray<PathRequest> requestsSnap;

    private List<MoveWithPath> callbacksSnap;

    // —— 等待下一批 —— //
    private NativeList<PathRequest> pendingRequests;

    private List<MoveWithPath> pendingCallbacks = new List<MoveWithPath>();

    public override void Init()
    {
        base.Init();
        pathRequests = new NativeList<PathRequest>(Allocator.Persistent);
        pendingRequests = new NativeList<PathRequest>(Allocator.Persistent);
    }

    protected override void Clear()
    {
        base.Clear();
        if (jobRunning) pathJobHandle.Complete();
        if (pathRequests.IsCreated) pathRequests.Dispose();
        if (pendingRequests.IsCreated) pendingRequests.Dispose();
    }

    protected override void Update()
    {
        base.Update();
        if (jobRunning) return;

        int requestCount = pathRequests.Length;
        if (requestCount == 0) return;

        // —— 做快照（防止运行中被改动）——
        requestsSnap = new NativeArray<PathRequest>(requestCount, Allocator.TempJob);
        for (int i = 0; i < requestCount; i++) requestsSnap[i] = pathRequests[i];

        callbacksSnap = new List<MoveWithPath>(requestCount);
        for (int i = 0; i < requestCount; i++) callbacksSnap.Add(MoveWithPath[i]);

        // 本批清空，新的请求都进 pending
        pathRequests.Clear();
        MoveWithPath.Clear();

        // —— 房间范围 & 面积/偏移 —— //
        mapRanges = new NativeArray<int4>(requestCount, Allocator.TempJob);
        for (int i = 0; i < requestCount; i++)
            mapRanges[i] = MapCellController.instance.GetRoomRange(requestsSnap[i].roomId);

        areas = new NativeArray<int>(requestCount, Allocator.TempJob);
        baseOffsets = new NativeArray<int>(requestCount, Allocator.TempJob);
        int totalArea = 0;
        for (int i = 0; i < requestCount; i++)
        {
            int4 r = mapRanges[i];
            int w = r.z - r.x + 1;
            int h = r.w - r.y + 1;
            int area = w * h;
            areas[i] = area;
            baseOffsets[i] = totalArea;
            totalArea += area;
        }

        // —— 分配按 totalArea 的总数组（更省内存）——
        openCells = new NativeArray<ushort>(totalArea, Allocator.TempJob);
        openCosts = new NativeArray<ushort>(totalArea, Allocator.TempJob);
        openCounts = new NativeArray<int>(requestCount, Allocator.TempJob);

        bestG = new NativeArray<ushort>(totalArea, Allocator.TempJob);
        nodeState = new NativeArray<byte>(totalArea, Allocator.TempJob);
        parentFlat = new NativeArray<ushort>(totalArea, Allocator.TempJob);
        for (int i = 0; i < totalArea; i++)
        {
            bestG[i] = ushort.MaxValue;
            parentFlat[i] = 0xFFFF;  // 无父标记
            nodeState[i] = 0;
        }

        // —— Stream 按请求数分段 —— //
        pathStream = new NativeStream(requestCount, Allocator.TempJob);

        var job = new SparsePathfindingSIMDJob
        {
            requests = requestsSnap,
            mapRanges = mapRanges,
            barrierMap = MapCellController.instance.MapObjBarriers,

            areas = areas,
            baseOffsets = baseOffsets,

            bestG = bestG,
            parentFlat = parentFlat,
            nodeState = nodeState,

            openCells = openCells,
            openCosts = openCosts,
            openCounts = openCounts,

            pathWriter = pathStream.AsWriter()
        };

        pathJobHandle = job.Schedule(requestCount, 2);
        jobRunning = true;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (!jobRunning) return;
        if (!pathJobHandle.IsCompleted) return;

        pathJobHandle.Complete();

        // —— 读取 —— //
        var reader = pathStream.AsReader();
        int n = reader.ForEachCount; // == requestsSnap.Length
        for (int i = 0; i < n; i++)
        {
            reader.BeginForEachIndex(i);
            Stack<int2> path = new Stack<int2>();
            while (reader.RemainingItemCount > 0)
                path.Push(reader.Read<int2>());
            reader.EndForEachIndex();
            // 用快照，避免索引错位
            var req = requestsSnap[i];

            callbacksSnap[i].Invoke(path, req.roomId, req.start, req.end);
        }

        // —— 释放（只释放一次）—— //
        pathStream.Dispose();

        openCells.Dispose();
        openCosts.Dispose();
        openCounts.Dispose();

        bestG.Dispose();
        parentFlat.Dispose();
        nodeState.Dispose();

        areas.Dispose();
        baseOffsets.Dispose();
        mapRanges.Dispose();
        requestsSnap.Dispose();

        // 把 pending 并回下一批
        for (int i = 0; i < pendingRequests.Length; i++)
            pathRequests.Add(pendingRequests[i]);
        pendingRequests.Clear();

        foreach (var cb in pendingCallbacks)
            MoveWithPath.Add(cb);
        pendingCallbacks.Clear();

        jobRunning = false; // ★★ 关键：避免下帧再次读已释放的 stream
    }

    public void AddPathRequest(int2 start, int2 end, int roomId, MoveWithPath moveWithPath)
    {
        if (jobRunning)
        {
            // Job 期间的请求先排队，防止改动本批快照/NativeList 触发重分配
            pendingRequests.Add(new PathRequest { start = start, end = end, roomId = roomId });
            pendingCallbacks.Add(moveWithPath);
        }
        else
        {
            pathRequests.Add(new PathRequest { start = start, end = end, roomId = roomId });
            MoveWithPath.Add(moveWithPath);
        }
    }
}