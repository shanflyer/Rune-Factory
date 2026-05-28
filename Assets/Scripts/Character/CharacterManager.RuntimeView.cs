using UnityEngine;

public partial class CharacterManager
{
    private bool ShouldDisplayCharacter(Character character, bool fixedDisplay = false)
    {
        return fixedDisplay || (character.mapInstance == WorldMapObjManager.instance.displayMap &&
                                !ExploreManager.instance.isExplore);
    }

    private bool TryGetRuntimeView(Character character, out CharacterRuntimeObj characterRuntimeObj)
    {
        return characterRuntionObjs.TryGetValue(character, out characterRuntimeObj);
    }

    private void QueueRefreshMapTempCharacter(int characterId, bool immediately = false)
    {
        var refreshMapTempCharacter = new RefreshMapTempCharacter
        {
            characterId = characterId,
        };
        GameActionManager.instance.QueueAction(refreshMapTempCharacter, immediately);
    }

    private void RefreshRuntimePosition(Character character, CharacterRuntimeObj characterRuntimeObj, Vector3 pos)
    {
        if (GameDataManager.instance.GlobalData.debug)
        {
            float dX = Unity.Mathematics.math.abs(characterRuntimeObj.transform.position.x - pos.x);
            if (dX >= 1.5)
            {
                Debug.Log($"Waring:{character.name}--oldPos{characterRuntimeObj.transform.position}--newPos{pos}");
            }
        }

        characterRuntimeObj.SetPosition(pos);
    }
}
