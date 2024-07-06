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
        CreatTempCharacter();
    }

    private TempCharacterCreatData NowTempCharacterCreatData;
    

    private void CreatTempCharacter()
    {
        if (NowTempCharacterCreatData == null)
        {
            return;
        }
        var nowCd = GameRandom.RandomInt(NowTempCharacterCreatData.cd.x, NowTempCharacterCreatData.cd.y);
        creatTempDelegate = CreatTempCharacter;
        if (totalCharacterCount >= NowTempCharacterCreatData.maxCharacterCount)
        { 
            GameTimerController.instance.DelayAction(nowCd, creatTempDelegate);
            return;
        }
        int characterId = 0; 
        int displayMap = WorldMapObjManager.instance.displayMap;
        int2 coordinate = MapCellController.instance.GetRandomBehavioCell(displayMap, BehaviorAreaType.创建);

        int randomIndex = GameRandom.RandomInt(0, NowTempCharacterCreatData.tempCharacters.Count);
        characterId = NowTempCharacterCreatData.tempCharacters[randomIndex];
        CreatTempCharacter creatTempCharacter = new CreatTempCharacter
        {
            characterId = characterId,
            mapInstance = displayMap,
            coordinateX = coordinate.x,
            coordinateY = coordinate.y
        };
        GameActionManager.instance.QueueAction(creatTempCharacter);

        totalCharacterCount++;

     
        GameTimerController.instance.DelayAction(nowCd, creatTempDelegate);
    }
}