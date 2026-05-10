// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - Talk
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/Talk")]
public class TalkActionData : GameActionBaseData
{
        public int talkId;
        public int characterId;
        public bool displayFunction;
        public int nextTalkEventId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new Talk
        {
                talkId = this.talkId,
                characterId = this.characterId,
                displayFunction = this.displayFunction,
                nextTalkEventId = this.nextTalkEventId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.talkId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
