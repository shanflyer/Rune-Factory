// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshOperateCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshOperateCharacter")]
public class RefreshOperateCharacterActionData : GameActionBaseData
{
        public int characterId;
        public bool join;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshOperateCharacter
        {
                characterId = this.characterId,
                join = this.join
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
