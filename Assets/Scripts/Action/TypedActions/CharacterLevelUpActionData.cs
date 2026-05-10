// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CharacterLevelUp
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CharacterLevelUp")]
public class CharacterLevelUpActionData : GameActionBaseData
{
        public int characterId;
        public int level;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CharacterLevelUp
        {
                characterId = this.characterId,
                level = this.level
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
