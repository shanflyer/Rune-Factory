using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

internal partial struct EmoteShowSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EmoteComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}

public partial struct EmoteShowJob : IJobEntity
{
    public float deltaTime;

    [ReadOnly]
    public NativeParallelHashSet<int>.ReadOnly removeEmotes;

    public void Execute(EnabledRefRW<EmoteComponent> isEnable, ref EmoteComponent emoteComponent)
    {
        if (!isEnable.ValueRO)
        {
            return;
        }
        if (removeEmotes.Contains(emoteComponent.entityId))
        {
            isEnable.ValueRW = false;
            return;
        }
        emoteComponent.currentTime += deltaTime;
        if (emoteComponent.currentTime > -emoteComponent.showTime)
        {
            isEnable.ValueRW = false;
        }
    }
}