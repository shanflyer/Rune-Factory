// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshCharacter")]
public class RefreshCharacterActionData : GameActionBaseData
{
        public new int id;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshCharacter
        {
                id = this.id
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
