// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetHomeEquipCoordinate
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetHomeEquipCoordinate")]
public class SetHomeEquipCoordinateActionData : GameActionBaseData
{
        public int characterId;
        public int equipInstanceId;
        public int mapInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetHomeEquipCoordinate
        {
                characterId = this.characterId,
                equipInstanceId = this.equipInstanceId,
                mapInstanceId = this.mapInstanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
