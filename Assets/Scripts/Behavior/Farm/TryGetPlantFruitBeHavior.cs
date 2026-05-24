using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/农场")]
[TaskName("尝试采摘")]
public class TryGetPlantFruitBeHavior : Action
{
    public SharedInt fieldId;
    public SharedInt characterId;
    public override void OnStart()
    {

        taskStatus = TaskStatus.Running;
        TryGetPlantFruit tryGetPlantFruit = new TryGetPlantFruit
        {
            fieldId = fieldId.Value,
            setResult = CheckState,
            setValue = GetPlantFruitId
        };
        GameActionManager.instance.QueueAction(tryGetPlantFruit);
    }

    private void GetPlantFruitId(int FruitId)
    {
        if (FruitId != 0 && characterId.Value != 0)
        {
            var field = FarmManager.instance.GetFieldCoordinate(fieldId.Value);
            var pos = GameCommon.GetMapPos(field.xy);
            var showItemAction = new ShowItemAction
            {
                mapInstanceId = field.z,
                position = pos,
                itemId = FruitId
            };
            GameActionManager.instance.QueueAction(showItemAction);
        }
    }
    void CheckState(bool value)
    {
        if (value)
        {
            taskStatus = TaskStatus.Success;
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
