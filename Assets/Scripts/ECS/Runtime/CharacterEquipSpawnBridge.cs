using NativeCollections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 全局共享的子弹生成队列（ECS 外部调用，支持并发）
/// </summary>
public class CharacterEquipSpawnBridge : Singleton<CharacterEquipSpawnBridge>
{
    public NativeFixedQueue<SetCharacterEquipment> Queue;
    public NativeFixedQueue<SetCharacterEquipment>.ParallelWriter ParallelWriter => Queue.AsParallelWriter();
    public override void Init()
    {
        base.Init();
        if (!Queue.IsCreated)
            Queue = new NativeFixedQueue<SetCharacterEquipment>(256, Allocator.Persistent);
    }
    protected override void Clear()
    {
        if (Queue.IsCreated)
            Queue.Dispose();
        base.Clear();
    }


   
}
public struct SetCharacterEquipment
{
    public Entity entity;
    public ItemType itemType;
    public int itemId;
    public int itemValue;
}