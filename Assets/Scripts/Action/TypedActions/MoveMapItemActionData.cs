// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - MoveMapItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/MoveMapItem")]
public class MoveMapItemActionData : GameActionBaseData
{
        public int mapItemInstanceId;
        public int mapInstance;
        public bool noneTryAdd;
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new MoveMapItem
        {
                mapItemInstanceId = this.mapItemInstanceId,
                mapInstance = this.mapInstance,
                noneTryAdd = this.noneTryAdd,
                dataId = this.dataId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
