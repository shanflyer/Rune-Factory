// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemoveCellCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemoveCellCharacter")]
public class RemoveCellCharacterActionData : GameActionBaseData
{
        public int characterId;
        public bool isTemp;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveCellCharacter
        {
                characterId = this.characterId,
                isTemp = this.isTemp
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
