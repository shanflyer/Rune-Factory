// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetCameraPixelValue
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetCameraPixelValue")]
public class SetCameraPixelValueActionData : GameActionBaseData
{
        public int pixelValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCameraPixelValue
        {
                pixelValue = this.pixelValue
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
