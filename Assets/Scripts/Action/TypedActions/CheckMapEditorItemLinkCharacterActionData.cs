// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CheckMapEditorItemLinkCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CheckMapEditorItemLinkCharacter")]
public class CheckMapEditorItemLinkCharacterActionData : GameActionBaseData
{
        public int mapId;
        public int itemEditorId;
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckMapEditorItemLinkCharacter
        {
                mapId = this.mapId,
                itemEditorId = this.itemEditorId,
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.mapId = source;
            if (target != 0 && target != int.MinValue) action.itemEditorId = target;
            if (value != -1) action.characterId = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
