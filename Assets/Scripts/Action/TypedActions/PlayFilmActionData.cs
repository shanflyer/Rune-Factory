// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - PlayFilm
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/PlayFilm")]
public class PlayFilmActionData : GameActionBaseData
{
        public string filmName;
        public string assetName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PlayFilm
        {
                filmName = this.filmName,
                assetName = this.assetName
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
