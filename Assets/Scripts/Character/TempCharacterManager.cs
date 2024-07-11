using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine; 

public class TempCharacterManager : Singleton<TempCharacterManager>
{
    //public override bool NeedUpdata => true;
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<DestoryCharacter>(DestoryCharacter);
        GameActionManager.instance.AddListener<ClearTempCharacter>(ClearTempCharacter);
        GameActionManager.instance.AddListener<StartCreatTempCharacter>(StartCreatTempCharacter);
    }

    private int totalCharacterCount; 
    public int level { get; private set; }

    protected override void Clear()
    {
        base.Clear();
        creatTempDelegate = null;
    }

    private void ClearTempCharacter(ClearTempCharacter clearTempCharacter)
    { 
        totalCharacterCount = 0;
        if (creatTempDelegate != null)
        {
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
    private List<int> tempList=new List<int>();
    private int tempRandomId;
    private async void StartCreatTempCharacter(StartCreatTempCharacter startCreatTempCharacter)
    {
        if (startCreatTempCharacter.clearAll)
        {
            ClearTempCharacter clearTempCharacter = new ClearTempCharacter();
            GameActionManager.instance.QueueAction(clearTempCharacter, true);
        }
        NowTempCharacterCreatData = await GameDataManager.instance.GetAsyncData<TempCharacterCreatData>(startCreatTempCharacter.creatDataId);
        if (NowTempCharacterCreatData == null)
        {
            return;
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
            int zeroCount = NowTempCharacterCreatData.maxCharacterCount / 2;
            int2 nowTimeKey = GameTimeManager.instance.nowHourMinute;
            for (int i = 0; i < zeroCount; i++)
            {
                CreatCharacter(nowTimeKey, BehaviorAreaType.聚集);
            }
        } 
        CreatTempCharacter();
    }

    private TempCharacterCreatData NowTempCharacterCreatData;
    private MyList<int> tempCharacters;
     
    void CreatCharacter(int2 nowTimeKey, BehaviorAreaType behaviorAreaType)
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
        int2 coordinate = MapCellController.instance.GetRandomBehavioCell(displayMap, BehaviorAreaType.创建).xy;
        CreatTempCharacter creatTempCharacter = new CreatTempCharacter
        {
            characterId = characterId,
            mapInstance = displayMap,
            coordinateX = coordinate.x,
            coordinateY = coordinate.y
        };
        GameActionManager.instance.QueueAction(creatTempCharacter);

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
            nowCd = GameRandom.RandomInt(cdRange)*1000;
        }
        Debug.Log($"creatCD:{nowCd}");
        creatTempDelegate = CreatTempCharacter;
        if (totalCharacterCount >= NowTempCharacterCreatData.maxCharacterCount)
        { 
            GameTimerController.instance.DelayAction(nowCd, creatTempDelegate);
            return;
        } 
        CreatCharacter(nowTimeKey,BehaviorAreaType.创建);
        GameTimerController.instance.DelayAction(nowCd, creatTempDelegate);
    }
}