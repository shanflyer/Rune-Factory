// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetTempMapItemCoordinate
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetTempMapItemCoordinate")]
public class SetTempMapItemCoordinateActionData : GameActionBaseData
{
        public int instanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetTempMapItemCoordinate
        {
                instanceId = this.instanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
