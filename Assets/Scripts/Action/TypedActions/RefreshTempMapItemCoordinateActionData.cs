// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshTempMapItemCoordinate
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshTempMapItemCoordinate")]
public class RefreshTempMapItemCoordinateActionData : GameActionBaseData
{
        public int instanceId;
        public int chatacterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshTempMapItemCoordinate
        {
                instanceId = this.instanceId,
                chatacterId = this.chatacterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
