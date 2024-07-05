using System;
using System.Collections.Generic;
using Unity.Mathematics;

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

    private List<int> tempCharacters=new List<int>();
    private int totalCharacterCount;
    public int level { get; private set; }

    protected override void Clear()
    {
        base.Clear();
        creatTempDelegate = null;
    }

    private void ClearTempCharacter(ClearTempCharacter clearTempCharacter)
    {
        tempCharacters.Clear();
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
            if (tempCharacters.Contains(destoryCharacter.dataId))
            {
                tempCharacters.Add(destoryCharacter.dataId);
            }
            totalCharacterCount--;
        }
    }

    private Action creatTempDelegate;

    private async void StartCreatTempCharacter(StartCreatTempCharacter startCreatTempCharacter)
    {
        NowTempCharacterCreatData = await GameDataManager.instance.GetAsyncData<TempCharacterCreatData>(startCreatTempCharacter.creatDataId);
        if (NowTempCharacterCreatData == null)
        {
            return;
        }
        if (startCreatTempCharacter.clearAll)
        {
            ClearTempCharacter clearTempCharacter = new ClearTempCharacter();
            GameActionManager.instance.QueueAction(clearTempCharacter, true);
        }
        CreatTempCharacter();
    }

    private TempCharacterCreatData NowTempCharacterCreatData;

    private void CreatTempCharacter()
    {
        if (NowTempCharacterCreatData == null)
        {
            return;
        }
        if (totalCharacterCount >= NowTempCharacterCreatData.maxCharacterCount)
        {
            return;
        }
        int characterId = 0;
        bool groupCreat = true;
        int displayMap = WorldMapObjManager.instance.displayMap;
        int2 coordinate = MapCellController.instance.GetRandomBehavioCell(displayMap, BehaviorAreaType.创建);

        if (tempCharacters.Count > 0)
        {
            int randomValue = GameRandom.RandomInt(0, 100);
            groupCreat = randomValue > 50;
        }
        if (!groupCreat)
        {
            int randomIndex = GameRandom.RandomInt(0, tempCharacters.Count);
            characterId = tempCharacters[randomIndex];
            tempCharacters.RemoveAt(randomIndex);
        }
        else
        {
            int randomIndex = GameRandom.RandomInt(0, NowTempCharacterCreatData.tempGroupCharacters.Count);
            characterId = NowTempCharacterCreatData.tempGroupCharacters[randomIndex];
        }
        CreatTempCharacter creatTempCharacter = new CreatTempCharacter
        {
            characterId = characterId,
            mapInstance = displayMap,
            coordinateX = coordinate.x,
            coordinateY = coordinate.y
        };
        GameActionManager.instance.QueueAction(creatTempCharacter);

        totalCharacterCount++;

        var nowCd = GameRandom.RandomInt(NowTempCharacterCreatData.cd.x, NowTempCharacterCreatData.cd.y);
        creatTempDelegate = CreatTempCharacter;
        GameTimerController.instance.DelayAction(nowCd, creatTempDelegate);
    }
}