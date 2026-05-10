// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - NPCFishingResult
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/NPCFishingResult")]
public class NPCFishingResultActionData : GameActionBaseData
{
        public int characterId;
        public bool success;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new NPCFishingResult
        {
                characterId = this.characterId,
                success = this.success
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
