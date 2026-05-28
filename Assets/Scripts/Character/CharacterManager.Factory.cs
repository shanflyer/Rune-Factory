using System.Collections.Generic;
using System.Threading.Tasks;

public partial class CharacterManager
{
    private readonly Dictionary<int, CharacterData> characterDataCache = new Dictionary<int, CharacterData>();
    private readonly Dictionary<int, ProfessionData> professionDataCache = new Dictionary<int, ProfessionData>();
    private readonly Dictionary<int, TempCharacterData> tempCharacterDataCache = new Dictionary<int, TempCharacterData>();

    private async Task<Character> CreateCharacterInstanceAsync(int characterDataId, int instanceId,
        bool needCreatePackage = true)
    {
        var characterData = await GetCharacterDataCachedAsync(characterDataId);
        var professionData = await GetProfessionDataCachedAsync(characterData.profession);
        return new Character(characterData, professionData, instanceId, needCreatePackage);
    }

    private async Task<TempCharacter> CreateTempCharacterInstanceAsync(int tempCharacterDataId, int instanceId)
    {
        var tempCharacterData = await GetTempCharacterDataCachedAsync(tempCharacterDataId);
        var characterData = await GetCharacterDataCachedAsync(tempCharacterData.linkCharacterId);
        var professionData = await GetProfessionDataCachedAsync(characterData.profession);
        return new TempCharacter(characterData, professionData, instanceId, tempCharacterData);
    }

    private async Task<CharacterData> GetCharacterDataCachedAsync(int characterDataId)
    {
        if (!characterDataCache.TryGetValue(characterDataId, out var characterData))
        {
            characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(characterDataId);
            if (characterData != null)
            {
                characterDataCache[characterDataId] = characterData;
            }
        }

        return characterData;
    }

    private async Task<ProfessionData> GetProfessionDataCachedAsync(int professionDataId)
    {
        if (!professionDataCache.TryGetValue(professionDataId, out var professionData))
        {
            professionData = await GameDataManager.instance.GetAsyncData<ProfessionData>(professionDataId);
            if (professionData != null)
            {
                professionDataCache[professionDataId] = professionData;
            }
        }

        return professionData;
    }

    private async Task<TempCharacterData> GetTempCharacterDataCachedAsync(int tempCharacterDataId)
    {
        if (!tempCharacterDataCache.TryGetValue(tempCharacterDataId, out var tempCharacterData))
        {
            tempCharacterData = await GameDataManager.instance.GetAsyncData<TempCharacterData>(tempCharacterDataId);
            if (tempCharacterData != null)
            {
                tempCharacterDataCache[tempCharacterDataId] = tempCharacterData;
            }
        }

        return tempCharacterData;
    }

    public void InvalidateFactoryCache()
    {
        characterDataCache.Clear();
        professionDataCache.Clear();
        tempCharacterDataCache.Clear();
    }

    private void ClearCharacterFactoryCache()
    {
        InvalidateFactoryCache();
    }
}
