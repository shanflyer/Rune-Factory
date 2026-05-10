// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryCreatPlant
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryCreatPlant")]
public class TryCreatPlantActionData : GameActionBaseData
{
        public int fieldId;
        public int plantId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryCreatPlant
        {
                fieldId = this.fieldId,
                plantId = this.plantId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
