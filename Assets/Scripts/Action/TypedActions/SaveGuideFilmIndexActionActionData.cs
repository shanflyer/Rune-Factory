// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SaveGuideFilmIndexAction
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SaveGuideFilmIndexAction")]
public class SaveGuideFilmIndexActionActionData : GameActionBaseData
{
        public new int id;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SaveGuideFilmIndexAction
        {
                id = this.id
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.id = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
