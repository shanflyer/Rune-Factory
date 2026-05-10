// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CharacterDeath
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CharacterDeath")]
public class CharacterDeathActionData : GameActionBaseData
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CharacterDeath
        {
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
