// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - StopFilm
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/StopFilm")]
public class StopFilmActionData : GameActionBaseData
{
        public string filmName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StopFilm
        {
                filmName = this.filmName
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
