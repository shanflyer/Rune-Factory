using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;
using System.Collections.Generic; 

[TaskCategory("Game/PlayerStore")]
[TaskName("选择NPC喜欢的物品")]
public class SelectNPCLikeCounter : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt selectStoreCounterId;

    public override TaskStatus OnUpdate()
    {
        var RuntimeStoreCounters = PlayerStoreManager.instance.RuntimeStoreCounters;
        Dictionary<int, List<RuntimeStoreCounter>> runtimeStoreCounters = new Dictionary<int, List<RuntimeStoreCounter>>();
        foreach (RuntimeStoreCounter RuntimeStoreCounter in RuntimeStoreCounters.Values)
        {
            if (RuntimeStoreCounter.count > 0)
            {
                if (!runtimeStoreCounters.TryGetValue(RuntimeStoreCounter.itemData.id, out var list))
                {
                    list = new List<RuntimeStoreCounter>();
                    runtimeStoreCounters.Add(RuntimeStoreCounter.itemData.id, list);
                }
                list.Add(RuntimeStoreCounter);
            }

        }
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value, out var npc))
        {
            var likeItems = npc.likeItems;
            for (int i = 0; i < likeItems.Count; i++)
            {
                int itemId = likeItems[i];
                if (runtimeStoreCounters.TryGetValue(itemId, out var list))
                {
                    int index = GameRandom.RandomInt(0, list.Count);
                    selectStoreCounterId.Value = list[index].instanceId;
                    return TaskStatus.Success;
                }
            }
        } 
        return TaskStatus.Failure;
    }
}