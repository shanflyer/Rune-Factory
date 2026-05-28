using System.Threading.Tasks;

public partial class CharacterManager
{
    private async Task<Character> CreateCharacterInstanceAsync(int characterDataId, int instanceId,
        bool needCreatePackage = true)
    {
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
        var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
        return new Character(characterData, professionData, instanceId, needCreatePackage);
    }

    private async Task<TempCharacter> CreateTempCharacterInstanceAsync(int tempCharacterDataId, int instanceId)
    {
        var tempCharacterData = await GameDataManager.instance.GetAsyncData<TempCharacterData>(tempCharacterDataId);
        var characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(tempCharacterData.linkCharacterId);
        var professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(characterData.profession);
        return new TempCharacter(characterData, professionData, instanceId, tempCharacterData);
    }
}
