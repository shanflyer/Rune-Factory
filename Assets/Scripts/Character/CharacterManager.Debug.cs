using System.Collections.Generic;

public partial class CharacterManager
{
    public bool TryGetDebugSnapshot(int characterId, out CharacterDebugSnapshot snapshot)
    {
        var character = GetCharacter(characterId);
        if (character == null)
        {
            snapshot = default;
            return false;
        }

        snapshot = character.GetDebugSnapshot();
        return true;
    }

    public List<CharacterDebugSnapshot> GetAllDebugSnapshots()
    {
        var snapshots = new List<CharacterDebugSnapshot>(characters.length);
        for (int i = 0; i < characters.length; i++)
        {
            snapshots.Add(characters[i].GetDebugSnapshot());
        }

        return snapshots;
    }
}
