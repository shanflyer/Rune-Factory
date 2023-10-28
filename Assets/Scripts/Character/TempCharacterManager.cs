using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class TempCharacterManager:Singleton<TempCharacterManager>
{
    public override bool NeedUpdata => true; 
    public override  void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<SetCreateTempCharacterLevel>(SetCreateTempCharacterLevel);
        GameActionManager.instance.AddListener<DestoryCharacter>(DestoryCharacter);
        GameActionManager.instance.AddListener<GetTempCharacterExit>(GetTempCharacterExit);
    }
    TempCharacterCreatData NowTempCharacterCreatData;
    List<int> tempCharacters;
    int totalCharacterCount;
    int nowCd;
    int waitTime;
    public int level { get; private set; }

    void GetTempCharacterExit(GetTempCharacterExit getTempCharacterExit)
    {
        var TempPosRanges = NowTempCharacterCreatData.tempExitDatas;
        int index = GameRandom.RandomInt(0, TempPosRanges.Count);
        var tempPosRange = TempPosRanges[index];
        int2 coordinate = GameRandom.RandomInt2(tempPosRange.posMin, tempPosRange.posMax);
        getTempCharacterExit.SetInt3Value(new int3(coordinate, tempPosRange.mapInstance));
    }
    

    void DestoryCharacter(DestoryCharacter destoryCharacter)
    {
        if (destoryCharacter.isTemp)
        {
            if (NowTempCharacterCreatData.tempCharacters.Contains(destoryCharacter.dataId) &&
            !tempCharacters.Contains(destoryCharacter.dataId))
            {
                tempCharacters.Add(destoryCharacter.dataId);
            }
            totalCharacterCount--;

        } 
    }
    async void SetCreateTempCharacterLevel(SetCreateTempCharacterLevel setCreateTempCharacterLevel)
    {
        level = setCreateTempCharacterLevel.level;
        NowTempCharacterCreatData = await GameDataManager.instance.GetAsyncData<TempCharacterCreatData>(level);
        tempCharacters = new List<int>();
        if (NowTempCharacterCreatData != null)
        {
            tempCharacters.AddRange(NowTempCharacterCreatData.tempCharacters);
        }
    }
    void CreatTempCharacter()
    {
        if (totalCharacterCount >= NowTempCharacterCreatData.maxCharacterCount)
        {
            return;
        }

        int characterId = 0;
        bool groupCreat=true;

        int BornIndex = GameRandom.RandomInt(0, NowTempCharacterCreatData.tempEnterDatas.Count);
        TempPosRange tempBornData = NowTempCharacterCreatData.tempEnterDatas[BornIndex];
        int2 coordinate = GameRandom.RandomInt2(tempBornData.posMin, tempBornData.posMax);

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
            int randomIndex = GameRandom.RandomInt(0,NowTempCharacterCreatData.tempGroupCharacters.Count);
            characterId = NowTempCharacterCreatData.tempGroupCharacters[randomIndex];
        } 
        

        CreatTempCharacter creatTempCharacter = new CreatTempCharacter
        {
            characterId = characterId,
            mapInstance=tempBornData.mapInstance,
            coordinateX=coordinate.x,
            coordinateY=coordinate.y
        };
        GameActionManager.instance.QueueAction(creatTempCharacter);

        totalCharacterCount++;
    }
    protected override void UpData()
    {
        base.UpData();
        if (NowTempCharacterCreatData == null)
        {
            return;
        }

        waitTime += (int)(Time.deltaTime * 1000);
        if (waitTime > nowCd)
        {
            waitTime = 0;
            nowCd = GameRandom.RandomInt(NowTempCharacterCreatData.cd.x, NowTempCharacterCreatData.cd.y);
            CreatTempCharacter();
        }
    }
}