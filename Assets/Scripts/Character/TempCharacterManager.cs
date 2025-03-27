using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct SpecialAreaTempCharacterCreatData
{
    public int2 areaKey;
    public int4 areaRange;
    public TempCharacterCreatData tempCharacterCreatData;
    public Action creatSpecialTempDelegate;
    public int specialTempRandomId;

    public List<int> oldSpecialCharacters;
}
public class TempCharacterManager : Singleton<TempCharacterManager>
{
    //public override bool NeedUpdata => true;
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<DestoryCharacter>(DestoryCharacter);
        GameActionManager.instance.AddListener<ClearTempCharacter>(ClearTempCharacter);
        GameActionManager.instance.AddListener<StartCreatTempCharacter>(StartCreatTempCharacter);
        GameActionManager.instance.AddListener<StartCreatSpecialTempCharacter>(StartCreatSpecialTempCharacter);
        GameActionManager.instance.AddListener<StopTempCharacterCreat>(StopTempCharacterCreat);
        level = 1;
    }

    public int totalCharacterCount { get; private set; }
    public int level { get; private set; }

    protected override void Clear()
    {
        base.Clear();
        creatTempDelegate = null;
    }

    void StopTempCharacterCreat(StopTempCharacterCreat stopTempCharacterCreat)
    {
        totalCharacterCount = 0;
        if (creatTempDelegate != null)
        {
            GameTimerController.instance.RemoveWaiter(creatTempDelegate);
        }
        NowTempCharacterCreatData = null;
    }
    private void ClearTempCharacter(ClearTempCharacter clearTempCharacter)
    {
        totalCharacterCount = 0;
        if (creatTempDelegate != null)
        {
           // Debug.Log("ClearTempCharacter!!");
            GameTimerController.instance.RemoveWaiter(creatTempDelegate);
        }
        NowTempCharacterCreatData = null;
    }

    private void DestoryCharacter(DestoryCharacter destoryCharacter)
    {
        if (destoryCharacter.isTemp)
        {
            totalCharacterCount--;
        }
    }

    private Action creatTempDelegate;
    private List<int> tempList = new List<int>();
    private int tempRandomId;
#if UNITY_EDITOR
    public List<int> TempList => tempList;
#endif
     
    Dictionary<int2, SpecialAreaTempCharacterCreatData> specialTempCharacterCreatDataDic = new Dictionary<int2, SpecialAreaTempCharacterCreatData>();
    private async void StartCreatSpecialTempCharacter(StartCreatSpecialTempCharacter startCreatSpecialTempCharacter)
    {
        if (!specialTempCharacterCreatDataDic.ContainsKey(startCreatSpecialTempCharacter.areaKey))
        {
           var  SpecialTempCharacterCreatData = await GameDataManager.instance.GetAsyncData<TempCharacterCreatData>(startCreatSpecialTempCharacter.creatDataId);
            if (SpecialTempCharacterCreatData == null)
                return;

            SpecialAreaTempCharacterCreatData specialAreaTempCharacterCreatData = new SpecialAreaTempCharacterCreatData
            {
                areaKey = startCreatSpecialTempCharacter.areaKey,
                areaRange = startCreatSpecialTempCharacter.gridRange,
                tempCharacterCreatData = SpecialTempCharacterCreatData,
                oldSpecialCharacters=new List<int>()
            }; 
            specialTempCharacterCreatDataDic.Add(startCreatSpecialTempCharacter.areaKey, specialAreaTempCharacterCreatData);
            UpDataTryCreatSpecialTempCharacter();

            void UpDataTryCreatSpecialTempCharacter()
            {
                if (!specialTempCharacterCreatDataDic.TryGetValue(startCreatSpecialTempCharacter.areaKey, out var specialAreaTempCharacterCreatData))
                {
                    return;
                } 
                int nowCd = 1;
                int2 nowTimeKey = GameTimeManager.instance.nowHourMinute;
                if (SpecialTempCharacterCreatData.gameTimeKeyIntDic.TryGetValue(nowTimeKey, out var cdRange))
                {
                    nowCd = GameRandom.RandomInt(cdRange) * 1000;
                }

                if (SpecialTempCharacterCreatData.gameTimeKeyTempCharacterDic.TryGetValue(nowTimeKey, out var tempId))
                {
                    if (specialAreaTempCharacterCreatData.specialTempRandomId != tempId)
                    {
                        specialAreaTempCharacterCreatData.specialTempRandomId = tempId;
                        var randomResult = GameRandom.instance.GetRandomValue(tempId);
                        CreatSpecialCharacter(randomResult, startCreatSpecialTempCharacter.gridRange, startCreatSpecialTempCharacter.areaKey.y,
                            specialAreaTempCharacterCreatData.creatSpecialTempDelegate==null);
                    }
                }

                Action creatSpecialTempDelegate = UpDataTryCreatSpecialTempCharacter; 
                GameTimerController.instance.DelayAction(nowCd, creatSpecialTempDelegate);
                specialAreaTempCharacterCreatData.creatSpecialTempDelegate = creatSpecialTempDelegate;
                specialTempCharacterCreatDataDic[startCreatSpecialTempCharacter.areaKey] = specialAreaTempCharacterCreatData;
            }
        } 
    } 

 
    
    void ClearSpecialNPC()
    { 
        using(var e = specialTempCharacterCreatDataDic.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var data = e.Current.Value;
                if (data.creatSpecialTempDelegate != null)
                {
                    GameTimerController.instance.RemoveWaiter(data.creatSpecialTempDelegate);
                }
            }
        }
        specialTempCharacterCreatDataDic.Clear();
    }
    private void CreatSpecialCharacter(List<int2> creatNpcs, int4 gridRange,int2 areaKey,bool prewarm)
    {
        if(specialTempCharacterCreatDataDic.TryGetValue(areaKey,out var specialAreaTempCharacterCreatData))
        {
            if(specialAreaTempCharacterCreatData.oldSpecialCharacters.Count>0)
            {
                for (int i = 0; i < specialAreaTempCharacterCreatData.oldSpecialCharacters.Count; i++)
                {
                    int characterId = specialAreaTempCharacterCreatData.oldSpecialCharacters[i];
                    Character character = CharacterManager.instance.GetCharacter(characterId);
                    if(character is TempCharacter tempCharacter)
                    {
                        tempCharacter.templevel = 2;
                    }
                }
                specialAreaTempCharacterCreatData.oldSpecialCharacters.Clear();
            }
        }
       
        for (int i = 0; i < creatNpcs.Count; i++)
        {
            int npcId = creatNpcs[i].x;
            int count = creatNpcs[i].y;
            for (int j = 0; j < count; j++)
            {
                int displayMap = WorldMapObjManager.instance.displayMap;
                int x = GameRandom.RandomInt(gridRange.x, gridRange.z);
                int y = GameRandom.RandomInt(gridRange.y, gridRange.z);

                if (!prewarm)
                { 
                    int2 coordinate = MapCellController.instance.GetRandomBehavioCell(displayMap, BehaviorAreaType.创建).xy;

                    CreatTempCharacter creatTempCharacter = new CreatTempCharacter
                    {
                        characterId = npcId,
                        mapInstance = displayMap,
                        coordinateX = coordinate.x,
                        coordinateY = coordinate.y,
                        setValue = CreatTempCharacterSuccess
                    };
                    GameActionManager.instance.QueueAction(creatTempCharacter);
                }
                else
                {
                    CreatTempCharacter creatTempCharacter = new CreatTempCharacter
                    {
                        characterId = npcId,
                        mapInstance = displayMap,
                        coordinateX = x,
                        coordinateY = y,
                        setValue = CreatTempCharacterSuccess
                    };
                    GameActionManager.instance.QueueAction(creatTempCharacter);
                } 

                void CreatTempCharacterSuccess(int characterId)
                {
                    SetTempCharacterTarget setTempCharacterTarget = new SetTempCharacterTarget
                    {
                        characterId = characterId,
                        area = areaKey.y,
                        targetCoordinate = new int2(x, y)
                    };
                    GameActionManager.instance.QueueAction(setTempCharacterTarget);

                    specialAreaTempCharacterCreatData.oldSpecialCharacters.Add(characterId);
                }
            }
        }
    }

     
    private async void StartCreatTempCharacter(StartCreatTempCharacter startCreatTempCharacter)
    {
       // return;
        if (startCreatTempCharacter.clearAll)
        {
            ClearSpecialNPC();

            ClearTempCharacter clearTempCharacter = new ClearTempCharacter();
            GameActionManager.instance.QueueAction(clearTempCharacter, true);
        }
        NowTempCharacterCreatData = await GameDataManager.instance.GetAsyncData<TempCharacterCreatData>(startCreatTempCharacter.creatDataId);
        if (NowTempCharacterCreatData == null)
        {
            return;
        }
        if (startCreatTempCharacter.overrideMaxCount > 0)
        {
            maxTempCount = startCreatTempCharacter.overrideMaxCount;
        }
        else
        {
            maxTempCount = NowTempCharacterCreatData.maxCharacterCount;
        }

        if (NowTempCharacterCreatData.gameTimeKeyTempCharacterDic.TryGetValue(GameTimeManager.instance.nowHourMinute, out var tempId))
        {
            if (tempRandomId != tempId)
            {
                tempRandomId = tempId;
                tempList = GameRandom.instance.GetRandomItemList(tempId);
            }
        }
        tempCharacters = new MyList<int>(tempList);
        if (startCreatTempCharacter.prewarm)
        {
            int zeroCount = maxTempCount / 2;
            int2 nowTimeKey = GameTimeManager.instance.nowHourMinute;
            for (int i = 0; i < zeroCount; i++)
            {
               CreatCharacter(nowTimeKey, BehaviorAreaType.聚集);
            }
        }
        CreatTempCharacter();
       // Debug.Log("CreatTempCharacter!!");
    }

    private int maxTempCount;
    private TempCharacterCreatData NowTempCharacterCreatData;
    private MyList<int> tempCharacters;

    private void CreatCharacter(int2 nowTimeKey, BehaviorAreaType behaviorAreaType)
    {
        int characterId = 0;
        if (NowTempCharacterCreatData.gameTimeKeyTempCharacterDic.TryGetValue(nowTimeKey, out var tempId))
        {
            if (tempRandomId != tempId)
            {
                tempRandomId = tempId;
                tempList = GameRandom.instance.GetRandomItemList(tempId);
                tempCharacters.SetList(tempList);
            }
        }
        if (tempCharacters.length == 0)
        {
            tempCharacters.SetList(tempList);
        }
        int randomIndex = GameRandom.RandomInt(0, tempCharacters.length);
        characterId = tempCharacters[randomIndex];
        tempCharacters.RemoveAt(randomIndex);

        int displayMap = WorldMapObjManager.instance.displayMap;
        int2 coordinate = MapCellController.instance.GetRandomBehavioCell(displayMap, behaviorAreaType).xy;
        CreatTempCharacter creatTempCharacter = new CreatTempCharacter
        {
            characterId = characterId,
            mapInstance = displayMap,
            coordinateX = coordinate.x,
            coordinateY = coordinate.y
        };
        GameActionManager.instance.QueueAction(creatTempCharacter,true);

        totalCharacterCount++;
    }

    private void CreatTempCharacter()
    {
        if (NowTempCharacterCreatData == null)
        {
            return;
        }
        int nowCd = 1;
        int2 nowTimeKey = GameTimeManager.instance.nowHourMinute;
        if (NowTempCharacterCreatData.gameTimeKeyIntDic.TryGetValue(nowTimeKey, out var cdRange))
        {
            nowCd = GameRandom.RandomInt(cdRange) * 1000; 
        }
        nowCd += (int)(PlayerStoreManager.instance.GetCustomerCD()*1000);
        // Debug.Log($"creatCD:{nowCd}");
        creatTempDelegate = CreatTempCharacter;
         
        GameTimerController.instance.DelayAction(nowCd, creatTempDelegate);
        if (totalCharacterCount < maxTempCount)
        {
            CreatCharacter(nowTimeKey, BehaviorAreaType.创建);
        } 
    }
}