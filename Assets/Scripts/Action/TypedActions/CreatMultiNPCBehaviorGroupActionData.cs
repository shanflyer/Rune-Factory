// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CreatMultiNPCBehaviorGroup
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CreatMultiNPCBehaviorGroup")]
public class CreatMultiNPCBehaviorGroupActionData : GameActionBaseData
{
        public int dataId;
        public bool faceCenter;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatMultiNPCBehaviorGroup
        {
                dataId = this.dataId,
                faceCenter = this.faceCenter
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
