using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct TempPosRange
{
    public int mapInstance;
    public int2 posMin;
    public int2 posMax;
}

[System.Serializable]
public struct TempCharacterStep
{
    public int2 startTime, endTime;
    public int2 cdRange;
    public int tempCharacter;
    public int2 startTempTime, endTempTime;
}
public class TempCharacterCreateData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string createName;
    public int maxCharacterCount;
    private TempCharacterStep[] tempCharacterSteps;
    public GameTimeKeyIntDataDictionary gameTimeKeyTempCharacterDic;
    public GameTimeKeyInt2DataDictionary gameTimeKeyIntDic;
    public string GetKey()
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
        gameTimeKeyIntDic = new GameTimeKeyInt2DataDictionary();
        gameTimeKeyTempCharacterDic = new GameTimeKeyIntDataDictionary();
        for (int i = 0; i < tempCharacterSteps.Length; i++)
        {
            GameTimeKey gameTimeKey = new GameTimeKey
            {
                minTime= tempCharacterSteps[i].startTime,
                maxTime = tempCharacterSteps[i].endTime, 
            };
            int2 cd = tempCharacterSteps[i].cdRange;
            gameTimeKeyIntDic.Add(gameTimeKey, cd);

            GameTimeKey gameTimeTempKey = new GameTimeKey
            {
                minTime = tempCharacterSteps[i].startTempTime,
                maxTime = tempCharacterSteps[i].endTempTime,
            };
            gameTimeKeyTempCharacterDic.Add(gameTimeTempKey, tempCharacterSteps[i].tempCharacter);
        }
      
    }
}