// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TraceCharacterResult
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TraceCharacterResult")]
public class TraceCharacterResultActionData : GameActionBaseData
{
        public int characterId;
        public int targetId;
        public bool successed;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TraceCharacterResult
        {
                characterId = this.characterId,
                targetId = this.targetId,
                successed = this.successed
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (source != 0 && source != int.MinValue) action.targetId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
