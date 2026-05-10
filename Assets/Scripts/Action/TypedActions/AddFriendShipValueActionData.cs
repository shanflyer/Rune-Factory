// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - AddFriendShipValue
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/AddFriendShipValue")]
public class AddFriendShipValueActionData : GameActionBaseData
{
        public int characterId;
        public int value;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AddFriendShipValue
        {
                characterId = this.characterId,
                value = this.value
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (value != -1) action.value = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
