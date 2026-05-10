// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetWeather
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetWeather")]
public class SetWeatherActionData : GameActionBaseData
{
        public bool noLerp;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetWeather
        {
                noLerp = this.noLerp
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
