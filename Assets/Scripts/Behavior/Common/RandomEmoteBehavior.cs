using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("NewGame/Common")]
[TaskName("展示随机表情")]
public class RandomEmoteBehavior : Action
{
    [SerializeField]
    private SharedInt entityId;
    [SerializeField]
    private EntityType entityType;
    [SerializeField]
    private SharedInt emoteRandomId;
    [SerializeField]
    private SharedInt2 showTime;
    [SerializeField]
    private bool needWait;

    public override void OnStart()
    {
        var randomResult = GameRandom.instance.GetRandomValue(emoteRandomId.Value);
        if (randomResult.Count > 0)
        {
            int emoteId = randomResult[0].x;
            int showTimeValue = GameRandom.RandomInt(showTime.Value);
            ShowEmote showEmote = new ShowEmote
            {
                emoteId = emoteId,
                entityType = entityType,
                id = entityId.Value,
                showTime = showTimeValue,
            };
            GameActionManager.instance.QueueAction(showEmote);
            if (needWait)
            {
                taskStatus = TaskStatus.Running;
                GameTimerController.instance.DelayAction(showTimeValue, () =>
                {
                    taskStatus = TaskStatus.Success;
                });
            }
            else
            {
                taskStatus = TaskStatus.Success;
            }
            return;
        }
        taskStatus = TaskStatus.Failure;
    }

    TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
