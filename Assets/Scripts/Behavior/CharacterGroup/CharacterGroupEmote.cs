using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using System.Collections.Generic;

[TaskCategory("Game/CharacterGroup")]
[TaskName("团体角色表情展示")]
public class CharacterGroupEmote : Action
{
    public SharedIntList characterIds;
    [SerializeField]
    private SharedInt randomEmote;
    [SerializeField]
    private float waitDuration = 1.0f;
    [SerializeField]
    private float2 showTime = new float2(1, 1);

    private float startTime;
    private TaskStatus taskStatus;
    private int index = 0;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
    }
    void ShowCharacterEmote()
    {
        if (index >= characterIds.Value.Count)
        {
            taskStatus = TaskStatus.Success;
            return;
        }
        var characterId =characterIds.Value[index];
        var randomResults = GameRandom.instance.GetRandomValue(randomEmote.Value);
        if (randomResults.Count > 0)
        {
            int emote = randomResults[0].x;
            float emoteShowTime = GameRandom.RandomFloat(showTime);
            ShowEmote showEmote = new ShowEmote
            {
                emoteId = emote,
                entityType = EntityType.角色,
                id = characterId,
                showTime = (int)(emoteShowTime * 1000)
            };
            GameActionManager.instance.QueueAction(showEmote);
        }
        index++;
        GameTimerController.instance.DelayAction((int)(waitDuration*1000), ShowCharacterEmote);
    }
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
