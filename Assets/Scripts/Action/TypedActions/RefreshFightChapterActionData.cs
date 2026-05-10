// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshFightChapter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshFightChapter")]
public class RefreshFightChapterActionData : GameActionBaseData
{
        public new int id;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshFightChapter
        {
                id = this.id
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
