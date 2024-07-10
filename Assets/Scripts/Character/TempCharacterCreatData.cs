using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct TempPosRange
{
    public int mapInstance;
    public int2 posMin;
    public int2 posMax;
}

public class TempCharacterCreatData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string creatName;
    public int maxCharacterCount;
    private int[] gameTimeRanges;
    private int[] cds;  
    private int[] tempCharacters;
    private int[] tempTimeRanges;
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
        
        int count=gameTimeRanges.Length/4;
        for(int i=0;i<count;i++)
        {
            int index = i * 4;
            GameTimeKey gameTimeKey = new GameTimeKey
            {
                minHour = gameTimeRanges[index],
                minMinute = gameTimeRanges[index + 1],
                maxHour = gameTimeRanges[index + 2],
                maxMinute = gameTimeRanges[index + 3]
            };
            int2 cd = new int2(cds[i*2], cds[i * 2 + 1]);
            gameTimeKeyIntDic.Add(gameTimeKey, cd);
        }

        gameTimeKeyTempCharacterDic = new GameTimeKeyIntDataDictionary();
        int count1 = tempTimeRanges.Length / 4;
        for (int i = 0; i < count1; i++)
        {
            int index = i * 4;
            GameTimeKey gameTimeKey = new GameTimeKey
            {
                minHour = tempTimeRanges[index],
                minMinute = tempTimeRanges[index + 1],
                maxHour = tempTimeRanges[index + 2],
                maxMinute = tempTimeRanges[index + 3]
            };
            
            gameTimeKeyTempCharacterDic.Add(gameTimeKey, tempCharacters[i]);
        }
    }
}