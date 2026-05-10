// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DisplayOrHideCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DisplayOrHideCharacter")]
public class DisplayOrHideCharacterActionData : GameActionBaseData
{
        public int characterId;
        public bool display;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayOrHideCharacter
        {
                characterId = this.characterId,
                display = this.display
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
