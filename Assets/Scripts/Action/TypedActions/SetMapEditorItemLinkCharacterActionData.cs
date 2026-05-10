// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetMapEditorItemLinkCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetMapEditorItemLinkCharacter")]
public class SetMapEditorItemLinkCharacterActionData : GameActionBaseData
{
        public int mapId;
        public int mapItemEditorId;
        public int linkInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetMapEditorItemLinkCharacter
        {
                mapId = this.mapId,
                mapItemEditorId = this.mapItemEditorId,
                linkInstanceId = this.linkInstanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.mapId = source;
            if (target != 0 && target != int.MinValue) action.mapItemEditorId = target;
            if (value != -1) action.linkInstanceId = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
