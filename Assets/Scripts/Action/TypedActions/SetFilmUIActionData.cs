// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetFilmUI
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetFilmUI")]
public class SetFilmUIActionData : GameActionBaseData
{
        public bool display;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetFilmUI
        {
                display = this.display
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
