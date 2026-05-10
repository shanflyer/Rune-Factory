// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ChangeHomeEquipCharacter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ChangeHomeEquipCharacter")]
public class ChangeHomeEquipCharacterActionData : GameActionBaseData
{
        public int equipInstanceId;
        public int newPlayer;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeHomeEquipCharacter
        {
                equipInstanceId = this.equipInstanceId,
                newPlayer = this.newPlayer
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
