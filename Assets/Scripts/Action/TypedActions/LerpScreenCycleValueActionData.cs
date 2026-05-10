// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - LerpScreenCycleValue
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/LerpScreenCycleValue")]
public class LerpScreenCycleValueActionData : GameActionBaseData
{
        public float minCycleValue;
        public float maxCycleValue;
        public float lerpTime;
        public Vector2 cyclePos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new LerpScreenCycleValue
        {
                minCycleValue = this.minCycleValue,
                maxCycleValue = this.maxCycleValue,
                lerpTime = this.lerpTime,
                cyclePos = this.cyclePos
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
