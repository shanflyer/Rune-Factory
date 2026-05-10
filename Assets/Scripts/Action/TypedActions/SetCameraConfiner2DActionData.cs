// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetCameraConfiner2D
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetCameraConfiner2D")]
public class SetCameraConfiner2DActionData : GameActionBaseData
{
        public bool enable;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCameraConfiner2D
        {
                enable = this.enable
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
