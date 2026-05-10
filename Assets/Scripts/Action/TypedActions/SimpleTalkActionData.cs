// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SimpleTalk
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SimpleTalk")]
public class SimpleTalkActionData : GameActionBaseData
{
        public int talkId;
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SimpleTalk
        {
                talkId = this.talkId,
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.talkId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
