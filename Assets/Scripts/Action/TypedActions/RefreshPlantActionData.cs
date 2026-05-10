// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshPlant
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshPlant")]
public class RefreshPlantActionData : GameActionBaseData
{
        public int mapId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshPlant
        {
                mapId = this.mapId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.mapId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
