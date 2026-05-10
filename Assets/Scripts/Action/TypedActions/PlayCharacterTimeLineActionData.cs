// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - PlayCharacterTimeLine
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/PlayCharacterTimeLine")]
public class PlayCharacterTimeLineActionData : GameActionBaseData
{
        public int characterId;
        public string playName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PlayCharacterTimeLine
        {
                characterId = this.characterId,
                playName = this.playName
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
