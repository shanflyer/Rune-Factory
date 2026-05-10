// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DisplayFilm
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DisplayFilm")]
public class DisplayFilmActionData : GameActionBaseData
{
        public string filmName;
        public string path;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayFilm
        {
                filmName = this.filmName,
                path = this.path
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
