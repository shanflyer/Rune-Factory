using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

namespace OldName
{
    [System.Serializable]
    public class RedMoney
    {
        public int value;
    }
    [System.Serializable]
    public class SaveTime
    {
        public string value;
    }
    [System.Serializable]
    public class PlantSaveData
    {
        public int id;
        public int growedDays;
        public int turnCount;
        public PlantStatus plantStatus;
        public int dryDays;
        public int statusIndex;
        public int mapId, coordinateX, coordinateY;
        public PlantSaveData() { }

        public PlantSaveData(Plant plant)
        {
            id = plant.id;
            growedDays = plant.growedDays;
            turnCount = plant.turnCount;
            plantStatus = plant.plantStatus;
            dryDays = plant.dryDays;
            statusIndex = plant.statusIndex;
            mapId = plant.mapId;
            coordinateX = plant.field.coordinate.x;
            coordinateY = plant.field.coordinate.y;
        }
    }
    [System.Serializable]
    public class PlantFieldSeveData
    {
        public List<bool> isGrassClear;
        public List<FieldStr> fields;
        public List<PlantSaveData> plantSaveDatas;

        public PlantFieldSeveData()
        {
            isGrassClear = new List<bool>();
            fields = new List<FieldStr>();
            plantSaveDatas = new List<PlantSaveData>();
        }
        public void UpData()
        {
            FarmAction farmAction = GameComponentData.gameData.farmAction;
            isGrassClear = new List<bool>();
            foreach (var farmActionGrass in farmAction.Grasses)
            {
                isGrassClear.Add(farmActionGrass.isClear);
            }
            fields = new List<FieldStr>();
            foreach (var farmActionField in farmAction.Fields)
            {
                FieldStr fieldStr = new FieldStr(farmActionField);
                fields.Add(fieldStr);
            }
            plantSaveDatas = new List<PlantSaveData>();
            if (GameComponentData.gameData.plantAction.Plants != null)
            {
                foreach (var plantActionPlant in GameComponentData.gameData.plantAction.Plants)
                {
                    PlantSaveData plantSaveData = new PlantSaveData(plantActionPlant);
                    plantSaveDatas.Add(plantSaveData);
                }
            }

            DataSaveAndLoadTest.CreatPlantData();
        }
    }
    [System.Serializable]
    public class AnimalSaveData
    {
        public int id;
        public string name;
        public int mapId, coordinateX, coordinateY;
        public int nowAge;
        public int Hp;
        public int level, exp;
        public int hungerDays;
        public AnimalStatus animalStatus;
        public AgeStatus ageStatus;
        public int oldProduceDays;
        public int totalProduceCount;
        public AnimalSaveData() { }

        public AnimalSaveData(Animal animal)
        {
            id = animal.id;
            name = animal.Name;
            mapId = animal.mapId;
            coordinateX = animal.coordinate.x;
            coordinateY = animal.coordinate.y;
            nowAge = animal.nowAge;
            Hp = animal.property.HP;
            level = animal.level;
            exp = animal.property.EXP;
            hungerDays = animal.hungerDays;
            animalStatus = animal.animalStatus;
            oldProduceDays = animal.oldProduceDays;
            totalProduceCount = animal.totalProduceCount;
            ageStatus = animal.ageStatus;
        }
    }

    public class PastureSaveData
    {
        public int id;
        public string name;
        public List<AnimalSaveData> animalSaveDatas;
        public int grassCount;
        public int caseCount;
        public bool isOpen;
        public int packageId;
        public PastureSaveData() { }

        public PastureSaveData(Pasture pasture, bool _isOpen)
        {
            isOpen = _isOpen;
            id = pasture.id;
            name = pasture.name;
            grassCount = pasture.grassCount;
            caseCount = pasture.caseCount;
            packageId = pasture.itemPackage;

            animalSaveDatas = new List<AnimalSaveData>();
            if (pasture.Animals != null)
            {
                foreach (var pastureAnimal in pasture.Animals)
                {
                    AnimalSaveData animalSaveData = new AnimalSaveData(pastureAnimal);
                    animalSaveDatas.Add(animalSaveData);
                }
            }
        }
    }
    [System.Serializable]
    public class NpcSaveData
    {
        public int id;
        public int weapon;
        public int clothes;
        public int friendlyLevel;
        public int friendlyEXP;
        public int waitDays;
        public Season season;
        public int date;
        public int level, exp, hp;
        public bool isFriendlyExpAdd, istteamExpAdd, isGiftExpAdd0, isGiftExpAdd1, isGiftExpAdd2;
        public bool isFlower, isLove, isMarried, isYuehui;

        public NpcSaveData() { }
        public NpcSaveData(NPCX npcx)
        {
            id = npcx.id;
            weapon = npcx.weapon;
            clothes = npcx.clothes;
            friendlyLevel = npcx.npcData.friendlyLevel;
            friendlyEXP = npcx.npcData.frienflyExp;
            waitDays = npcx.WaitDays;
            season = npcx.npcData.brothSeason;
            date = npcx.npcData.brothDate;
            level = npcx.level;
            hp = npcx.property.HP;
            exp = npcx.property.EXP;
            istteamExpAdd = npcx.istteamExpAdd;
            isFriendlyExpAdd = npcx.isFriendlyExpAdd;
            isGiftExpAdd0 = npcx.isGiftExpAdd0;
            isGiftExpAdd1 = npcx.isGiftExpAdd1;
            isGiftExpAdd2 = npcx.isGiftExpAdd2;
            isFlower = npcx.isFlower;
            isLove = npcx.isLove;
            isMarried = npcx.isMarried;
            isYuehui = npcx.isYuehui;
        }
    }

    [System.Serializable]
    public class MarryData
    {
        public string babyBedBuyTime;
        public string pregnancyTime;
        public string haveChildrenTime;
        public string childName;
        public int bodyValue, mindValue;
        public int foodValue, moodValue, cleanValue;
        public GrowStatus BodyGrowStatus, MindGrowStatus;



        public void SaveBabyBedTime()
        {
            babyBedBuyTime = GameComponentData.gameData.gameTimeManager.GameTimeToString();
            //DataSaveAndLoadTest.CreatMarryData();
        }
        public void SavePregnancyTime()
        {
            pregnancyTime = GameComponentData.gameData.gameTimeManager.GameTimeToString();
            //DataSaveAndLoadTest.CreatMarryData();
        }
        public void SaveHaveChildrenTimeTime()
        {
            haveChildrenTime = GameComponentData.gameData.gameTimeManager.GameTimeToString();
            //DataSaveAndLoadTest.CreatMarryData();
        }

        public void SetChildData()
        {
            ChildData childData = GameComponentData.gameData.gameManager.childData;
            childName = childData.name;
            bodyValue = childData.bodyValue;
            mindValue = childData.mindValue;
            foodValue = childData.foodValue;
            moodValue = childData.moodValue;
            cleanValue = childData.cleanValue;
            BodyGrowStatus = childData.BodyGrowStatus;
            MindGrowStatus = childData.MindGrowStatus;


            //DataSaveAndLoadTest.CreatMarryData();
        }
    }
    [System.Serializable]
    public class PlayerSaveData
    {
        public string name;
        public Gender gender;
        public int level;
        public int exp;
        public int weapon;
        public int clothes;
        public Season season;
        public int date;
        public bool isMarried;
        public int teamPlayer0, teamPlayer1;
        public bool isMarriedFood, isAnMo;
        public AttributeType attributeType;
        public List<int> titles;
        public List<int> EquipMentIds;
        public List<int> formulas;

        public PlayerSaveData()
        {
            teamPlayer0 = 0;
            teamPlayer1 = 0;
        }

        public void UpPlayerData()
        {
            GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
            name = gamePlayer.name;
            attributeType = gamePlayer.attributeType;
            level = gamePlayer.level;
            exp = gamePlayer.property.EXP;
            weapon = gamePlayer.weapon.dataId;
            clothes = gamePlayer.clothes.dataId;

            isMarried = gamePlayer.isMarried;
            isMarriedFood = gamePlayer.isMarriedFood;
            isAnMo = gamePlayer.isAnMo;
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
            {
                teamPlayer0 = gamePlayer.TeamPlayer0.id;
            }
            else
            {
                teamPlayer0 = 0;
            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
            {
                teamPlayer1 = gamePlayer.TeamPlayer1.id;
            }
            else
            {
                teamPlayer1 = 0;
            }


            titles = new List<int>();
            foreach (var charactorTitle in GameComponentData.gameData.charactorTitleAction.CharactorTitles)
            {
                if (charactorTitle.isGet)
                {
                    titles.Add(charactorTitle.id);
                }
            }
            EquipMentIds = new List<int>();
            foreach (var equipmentManagerEuqipment in GameComponentData.gameData.equipmentManager.Euqipments)
            {
                if (equipmentManagerEuqipment.isBuy)
                {
                    EquipMentIds.Add(equipmentManagerEuqipment.id);
                }
            }

            formulas = new List<int>();
            foreach (var formulaActionFormula in GameComponentData.gameData.formulaAction.Formulas)
            {
                if (formulaActionFormula.isOpen)
                {
                    formulas.Add(formulaActionFormula.id);
                }
            }

        }
    }

    [System.Serializable]
    public class DeskData
    {
        public int deskCount;
        public List<int> deskItemIds, deskItenCounts;
        public List<int> itemIds, RandomValues;
        public DeskData()
        {
            deskCount = 1;
            deskItenCounts = new List<int>();
            deskItemIds = new List<int>();
            itemIds = new List<int>();
            RandomValues = new List<int>();
        }
        public void UpData()
        {
            deskCount = GameComponentData.gameData.shopGoldDeskAction.openCount;
            deskItemIds = new List<int>();
            deskItenCounts = new List<int>();

            if (GameComponentData.gameData.shopGoldDeskAction.GoodDeskes != null)
            {
                foreach (var desk in GameComponentData.gameData.shopGoldDeskAction.GoodDeskes)
                {
                    if (desk.item.instanceId != 0)
                    {
                        deskItemIds.Add(desk.item.dataId);
                        deskItenCounts.Add(desk.item.count);
                    }
                    else
                    {
                        deskItemIds.Add(0);
                        deskItenCounts.Add(0);
                    }

                }
            }


        }
    }
    [System.Serializable]
    public class DateData
    {
        public int year;
        public Season season;
        public int date;

        public void UpData()
        {
            year = GameTimeManager.nowGameTime.gameDate.year;
            season = GameTimeManager.nowGameTime.gameDate.season;
            date = GameTimeManager.nowGameTime.gameDate.date;
        }
    }

    [System.Serializable]
    public class PauseTime
    {
        public bool isAction;
        public int year;
        public int month;
        public int day;
        public int hour;
        public int min;
    }
    [System.Serializable]
    public class CharactorTitleValue
    {
        public int plantingExp, livestockExp, fishingExp, manufatureExp, CookingExp, busicessExp;
        public int fishSellMoney, equipSellMoney, foodSellMoney, farmProduceSellMoney;
        public int weaponCont, equipCount;
        public int riceCount, wineCont, fishCookCont, soupCount, vegetableCount, canCount;
        public int explorCount, killMonsterCount, failureCount;
        public int sleepDays;
        public List<int> fishes, Totals;
        public List<int> getItems;

        public CharactorTitleValue()
        {
            fishes = new List<int>();
            Totals = new List<int>();
            getItems = new List<int>();
        }
        public void UpData()
        {
            CharactorTitleAction charactorTitleAction = GameComponentData.gameData.charactorTitleAction;
            plantingExp = charactorTitleAction.plantingExp;
            livestockExp = charactorTitleAction.livestockExp;
            manufatureExp = charactorTitleAction.manufatureExp;
            CookingExp = charactorTitleAction.CookingExp;
            busicessExp = charactorTitleAction.busicessExp;
            fishSellMoney = charactorTitleAction.fishSellMoney;
            equipSellMoney = charactorTitleAction.equipSellMoney;
            foodSellMoney = charactorTitleAction.foodSellMoney;
            farmProduceSellMoney = charactorTitleAction.farmProduceSellMoney;
            weaponCont = charactorTitleAction.weaponCont;
            equipCount = charactorTitleAction.equipCount;
            riceCount = charactorTitleAction.riceCount;
            wineCont = charactorTitleAction.wineCont;
            fishCookCont = charactorTitleAction.fishCookCont;
            soupCount = charactorTitleAction.soupCount;
            vegetableCount = charactorTitleAction.vegetableCount;
            canCount = charactorTitleAction.canCount;
            explorCount = charactorTitleAction.explorCount;
            killMonsterCount = charactorTitleAction.killMonsterCount;
            failureCount = charactorTitleAction.failureCount;
            sleepDays = charactorTitleAction.sleepDays;
            fishes = new List<int>();
            Totals = new List<int>();
            foreach (var vector2Int in charactorTitleAction.fishTotals)
            {
                fishes.Add(vector2Int.x);
                Totals.Add(vector2Int.y);
            }
            getItems = new List<int>();
            foreach (var item in charactorTitleAction.getItems)
            {
                getItems.Add(item);
            }
        }
    }
    [System.Serializable]
    public class PlayerMoneyData
    {
        public int team1Hp, team2Hp, team1Level, team2Level, team1Exp, team2Exp;
        public int money0, money1, power, Hp;
        public int packageId, boxId, iceboxId;

        public List<int> battleAllValues;

        public int seedId, seedCount;
        public int waterValue;
        public int GroundMoney;
        public PlayerMoneyData() { }
        public void UpData()
        {
            GroundMoney = GameComponentData.gameData.coinAction.GroundMoney;
            GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
            money0 = gamePlayer.money;
            power = gamePlayer.property.Power;
            Hp = gamePlayer.property.HP;
            packageId = gamePlayer.package;
            boxId = gamePlayer.box;
            iceboxId = gamePlayer.icebox;

            battleAllValues = new List<int>();
            foreach (var batteleMap in GameComponentData.gameData.BattleMapAction.BatteleMaps)
            {
                battleAllValues.Add(batteleMap.allValue);
            }

            Item seedItem = GameComponentData.gameData.farmAction.farmTool;
            if (seedItem.dataId != 0)
            {
                seedId = seedItem.dataId;
                seedCount = seedItem.count;
            }
            else
            {
                seedId = 0;
                seedCount = 0;
            }
            waterValue = Mathf.RoundToInt(GameComponentData.gameData.farmAction.waterValue * 1000);

            if (gamePlayer.TeamPlayer0 == null || gamePlayer.TeamPlayer0.name == null)
            {
                team1Hp = -1;
                team1Level = -1;
                team1Exp = -1;
            }
            else
            {
                team1Hp = gamePlayer.TeamPlayer0.property.HP;
                team1Level = gamePlayer.TeamPlayer0.level;
                team1Exp = gamePlayer.TeamPlayer0.property.EXP;
            }
            if (gamePlayer.TeamPlayer1 == null || gamePlayer.TeamPlayer1.name == null)
            {
                team2Hp = -1;
                team2Level = -1;
                team2Exp = -1;
            }
            else
            {
                team2Hp = gamePlayer.TeamPlayer1.property.HP;
                team2Level = gamePlayer.TeamPlayer1.level;
                team2Exp = gamePlayer.TeamPlayer1.property.EXP;
            }
        }
    }

    [System.Serializable]
    public class GameSaveDataOld
    {
        public PauseTime pauseTime;
        public CharactorTitleValue charactorTitleValue;
        public PlayerMoneyData playerMoneyData;
        public DeskData deskData;
        public DateData dateData;
        public PlayerSaveData playerSaveData;
        public PlantFieldSeveData plantSeveDatas;
        public List<NpcSaveData> npcSaveDatas;
        public List<PastureSaveData> pastureSaveDatas;
        public SaveTime saveTime1;
        public string saveTime;

        public RedMoney reMoney;
        public MarryData marryData;
        public void SaveData()
        {
            UpDataPauseTime();
            charactorTitleValue.UpData();
            playerMoneyData.UpData();
            deskData.UpData();
            dateData.UpData();
            playerSaveData.UpPlayerData();
            plantSeveDatas.UpData();

            foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
            {
                NpcSaveData npcSaveData = new NpcSaveData(npcx);
                npcSaveDatas.Add(npcSaveData);
            }

            pastureSaveDatas = new List<PastureSaveData>();
            for (int i = 0; i < GameComponentData.gameData.pastureAction.Pastures.Count; i++)
            {
                PastureSaveData pastureSaveData = new PastureSaveData(GameComponentData.gameData.pastureAction.Pastures[i], GameComponentData.gameData.pastureAction.IsPastures[i]);
                pastureSaveDatas.Add(pastureSaveData);
            }
            saveTime1.value = System.DateTime.Now.ToString();
            UpRedMoney();

        }

        public void UpDataPauseTime()
        {
            DateTime x = System.DateTime.Now;
            pauseTime = new PauseTime();
            pauseTime.year = x.Year;
            pauseTime.month = x.Month;
            pauseTime.day = x.Day;
            pauseTime.hour = x.Hour;
            pauseTime.min = x.Minute;
            pauseTime.isAction = true;

            //DataSaveAndLoadTest.CreatPauseTimeData();
        }

        public void UpDataNpc()
        {
            npcSaveDatas = new List<NpcSaveData>();
            foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
            {
                NpcSaveData npcSaveData = new NpcSaveData(npcx);
                npcSaveDatas.Add(npcSaveData);
            }
            DataSaveAndLoadTest.CreatNPCData();
        }

        public void UpDate()
        {
            dateData.UpData();
            DataSaveAndLoadTest.CreatDateData();

        }
        public void UpPlayerMoneyData()
        {
            playerMoneyData.UpData();
            // DataSaveAndLoadTest.CreatPlayerMoneyData();

        }

        public void UpSaveTime()
        {
            saveTime1 = new SaveTime();
            saveTime1.value = System.DateTime.Now.ToString();
            //DataSaveAndLoadTest.CreatSaveTimedata();
        }
        public void UpRedMoney()
        {
            reMoney = new RedMoney();
            reMoney.value = GameComponentData.gameData.gameManager.gamePlayer.money1;
            DataSaveAndLoadTest.CreatRedMoneyData();
        }
        public void UpDataPlayer()
        {
            playerSaveData.UpPlayerData();
            DataSaveAndLoadTest.CreatPlayerData();

        }

        public void UpDeskData()
        {
            deskData.UpData();
            DataSaveAndLoadTest.CreatDeskData();

        }
        public void UpDataPastureData()
        {
            pastureSaveDatas = new List<PastureSaveData>();
            for (int i = 0; i < GameComponentData.gameData.pastureAction.Pastures.Count; i++)
            {
                PastureSaveData pastureSaveData = new PastureSaveData(GameComponentData.gameData.pastureAction.Pastures[i], GameComponentData.gameData.pastureAction.IsPastures[i]);
                pastureSaveDatas.Add(pastureSaveData);
            }
            DataSaveAndLoadTest.CreatPastureData();

        }
        public void UpPlantData()
        {
            plantSeveDatas.UpData();
            DataSaveAndLoadTest.CreatPlantData();
        }

        public void UpCharactorTitleValue()
        {
            charactorTitleValue.UpData();
            DataSaveAndLoadTest.CreatPlayerTitleData();
        }

        public void ZeroInitData()
        {
            dateData = new DateData
            {
                year = 1300,
                season = Season.夏,
                date = 20
            };
            playerSaveData = new PlayerSaveData
            {
                name = PlayerDate.playerName,
                gender = PlayerDate.gender,
                level = PlayerDate.level,
                date = PlayerDate.date,
                clothes = 0,
                weapon = 0,
                exp = 0
            };

            deskData = new DeskData();
            playerMoneyData = new PlayerMoneyData();
            plantSeveDatas = new PlantFieldSeveData();
            npcSaveDatas = new List<NpcSaveData>();
            pastureSaveDatas = new List<PastureSaveData>();
            playerSaveData.EquipMentIds = new List<int>();
            playerSaveData.formulas = new List<int>();
            playerSaveData.titles = new List<int>();
            charactorTitleValue = new CharactorTitleValue();
        }

        public void ZeroInitProperty(Property property)
        {
            if (!DataSaveAndLoadTest.LoadRedMoney())
            {
                reMoney = new RedMoney();
                reMoney.value = 0;
            }

            playerMoneyData = new PlayerMoneyData
            {
                GroundMoney = 0,
                Hp = property.HP,
                power = property.Power,
                money0 = 0,
                seedId = 0,
                seedCount = 0,
                waterValue = 0
            };
        }

        public void InitCharactorTitleValue()
        {
            CharactorTitleAction charactorTitleAction = GameComponentData.gameData.charactorTitleAction;
            charactorTitleAction.plantingExp = charactorTitleValue.plantingExp;
            charactorTitleAction.livestockExp = charactorTitleValue.livestockExp;
            charactorTitleAction.manufatureExp = charactorTitleValue.manufatureExp;
            charactorTitleAction.CookingExp = charactorTitleValue.CookingExp;
            charactorTitleAction.busicessExp = charactorTitleValue.busicessExp;
            charactorTitleAction.fishSellMoney = charactorTitleValue.fishSellMoney;
            charactorTitleAction.equipSellMoney = charactorTitleValue.equipSellMoney;
            charactorTitleAction.foodSellMoney = charactorTitleValue.foodSellMoney;
            charactorTitleAction.farmProduceSellMoney = charactorTitleValue.farmProduceSellMoney;
            charactorTitleAction.weaponCont = charactorTitleValue.weaponCont;
            charactorTitleAction.equipCount = charactorTitleValue.equipCount;
            charactorTitleAction.riceCount = charactorTitleValue.riceCount;
            charactorTitleAction.wineCont = charactorTitleValue.wineCont;
            charactorTitleAction.fishCookCont = charactorTitleValue.fishCookCont;
            charactorTitleAction.soupCount = charactorTitleValue.soupCount;
            charactorTitleAction.vegetableCount = charactorTitleValue.vegetableCount;
            charactorTitleAction.canCount = charactorTitleValue.canCount;
            charactorTitleAction.explorCount = charactorTitleValue.explorCount;
            charactorTitleAction.killMonsterCount = charactorTitleValue.killMonsterCount;
            charactorTitleAction.failureCount = charactorTitleValue.failureCount;
            charactorTitleAction.sleepDays = charactorTitleValue.sleepDays;
            charactorTitleAction.fishTotals = new List<Vector2Int>();
            for (int i = 0; i < charactorTitleValue.fishes.Count; i++)
            {
                charactorTitleAction.fishTotals.Add(new Vector2Int(charactorTitleValue.fishes[i], charactorTitleValue.Totals[i]));
            }
            charactorTitleAction.getItems = new List<int>();
            foreach (var item in charactorTitleValue.getItems)
            {
                charactorTitleAction.getItems.Add(item);
            }

        }
        public void InitNpcLoadData()
        {
            if (npcSaveDatas != null)
            {
                foreach (var npcSaveData in npcSaveDatas)
                {
                    NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == npcSaveData.id);
                    npcx.WaitDays = npcSaveData.waitDays;
                    npcx.clothes = npcSaveData.clothes;
                    npcx.weapon = npcSaveData.weapon;
                    npcx.npcData.brothDate = npcSaveData.date;
                    npcx.npcData.brothSeason = npcSaveData.season;
                    npcx.npcData.friendlyLevel = npcSaveData.friendlyLevel;
                    npcx.npcData.frienflyExp = npcSaveData.friendlyEXP;
                    npcx.level = npcSaveData.level;
                    npcx.property = npcx.professionData.ZeroProperty + npcx.professionData.GetPropertyFromLevel(npcx.level);
                    npcx.property.EXP = npcSaveData.exp;
                    npcx.property.HP = npcSaveData.hp;
                    npcx.isFriendlyExpAdd = npcSaveData.isFriendlyExpAdd;
                    npcx.isGiftExpAdd0 = npcSaveData.isGiftExpAdd0;
                    npcx.isGiftExpAdd1 = npcSaveData.isGiftExpAdd1;
                    npcx.isGiftExpAdd2 = npcSaveData.isGiftExpAdd2;
                    npcx.istteamExpAdd = npcSaveData.istteamExpAdd;
                    npcx.isFlower = npcSaveData.isFlower;
                    npcx.isLove = npcSaveData.isLove;
                    npcx.isMarried = npcSaveData.isMarried;
                    npcx.isYuehui = npcSaveData.isYuehui;
                    if (npcx.isMarried)
                    {
                        GameComponentData.gameData.NpcManager.SetMarriedNpcPos(npcx);
                    }
                }
            }

        }
        public void InitPlayerLoadData(GamePlayer gamePlayer)
        {
            GameTimeManager.nowGameTime = new GameTime(dateData.year, dateData.season, dateData.date, 0, 0);
            InitPastureData();
            InitPlantData();
            InitNpcLoadData();
            InitLoadData();
            InitCharactorTitleValue();
            GameComponentData.gameData.heritageAction.CheckZeroHeritagesData();
            if (playerMoneyData.battleAllValues.Count > 0)
            {
                if (playerMoneyData.battleAllValues.Count <= 10)
                {
                    var battleMaps =
                        GameComponentData.gameData.BattleMapAction.BatteleMaps.FindAll(b => b.id < 4010);
                    for (int i = 0; i < battleMaps.Count; i++)
                    {
                        battleMaps[i].allValue = playerMoneyData.battleAllValues[i];
                    }
                }
                else
                {
                    for (int i = 0; i < GameComponentData.gameData.BattleMapAction.BatteleMaps.Count; i++)
                    {
                        GameComponentData.gameData.BattleMapAction.BatteleMaps[i].allValue = playerMoneyData.battleAllValues[i];
                    }
                }

            }
            GameComponentData.gameData.BattleMapAction.InitBattleMaps();

            gamePlayer.InitGamePlayer();
            gamePlayer.property.EXP = playerSaveData.exp;
            gamePlayer.property.HP = playerMoneyData.Hp;
            gamePlayer.property.Power = playerMoneyData.power;
            gamePlayer.attributeType = playerSaveData.attributeType;
            gamePlayer.money = playerMoneyData.money0;

            gamePlayer.money1 = reMoney.value;
            if (playerSaveData.teamPlayer0 != 0)
            {
                if (playerSaveData.teamPlayer0 / 1000000 == 2)
                {
                    Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.Animals.Exists(a => a.id == playerSaveData.teamPlayer0));
                    if (pasture != null)
                    {
                        Animal animal = pasture.Animals.Find(a => a.id == playerSaveData.teamPlayer0);
                        Employer employer1 = new Employer(animal);
                        gamePlayer.TeamPlayer0 = new TeamPlayer(employer1);
                        gamePlayer.TeamPlayer0.property.HP = playerMoneyData.team1Hp;
                        animal.property.HP = playerMoneyData.team1Hp;
                        animal.level = playerMoneyData.team1Level;
                        animal.property.EXP = playerMoneyData.team1Exp;
                    }


                }
                else
                {
                    NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == playerSaveData.teamPlayer0);
                    if (npcx != null)
                    {
                        Employer employer = new Employer(npcx);
                        gamePlayer.TeamPlayer0 = new TeamPlayer(employer);
                        gamePlayer.TeamPlayer0.property.HP = playerMoneyData.team1Hp;
                        npcx.property.HP = playerMoneyData.team1Hp;
                        npcx.level = playerMoneyData.team1Level;
                        npcx.property.EXP = playerMoneyData.team1Exp;
                    }


                }

            }
            if (playerSaveData.teamPlayer1 != 0)
            {
                if (playerSaveData.teamPlayer1 / 1000000 == 2)
                {
                    Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.Animals.Exists(a => a.id == playerSaveData.teamPlayer1));

                    if (pasture != null)
                    {
                        Animal animal = pasture.Animals.Find(a => a.id == playerSaveData.teamPlayer1);
                        Employer employer1 = new Employer(animal);
                        gamePlayer.TeamPlayer1 = new TeamPlayer(employer1);
                        gamePlayer.TeamPlayer1.property.HP = playerMoneyData.team2Hp;
                        animal.property.HP = playerMoneyData.team2Hp;
                        animal.level = playerMoneyData.team2Level;
                        animal.property.EXP = playerMoneyData.team2Exp;
                    }

                }
                else
                {
                    NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == playerSaveData.teamPlayer1);
                    if (npcx != null)
                    {
                        Employer employer = new Employer(npcx);
                        gamePlayer.TeamPlayer1 = new TeamPlayer(employer);
                        gamePlayer.TeamPlayer1.property.HP = playerMoneyData.team2Hp;
                        npcx.property.HP = playerMoneyData.team2Hp;
                        npcx.level = playerMoneyData.team2Level;
                        npcx.property.EXP = playerMoneyData.team2Exp;
                    }

                }

            }
            FarmAction farmAction = GameComponentData.gameData.farmAction;
            if (playerMoneyData.seedId != 0)
            {
                farmAction.farmTool = ItemManager.instance.CreatItem(playerMoneyData.seedId, playerMoneyData.seedCount);
            }

            farmAction.waterValue = playerMoneyData.waterValue / 1000.0f;


            foreach (var _formula in playerSaveData.formulas)
            {
                Formula formula = GameComponentData.gameData.formulaAction.Formulas.Find(f => f.id == _formula);
                formula.isOpen = true;
            }

            foreach (var equipMentId in playerSaveData.EquipMentIds)
            {
                var x = GameComponentData.gameData.equipmentManager.Euqipments.Find(e => e.id == equipMentId);
                x.isBuy = true;
            }
            if (playerMoneyData.GroundMoney < 0)
            {
                playerMoneyData.GroundMoney = 0;
            }
            GameComponentData.gameData.coinAction.GroundMoney = playerMoneyData.GroundMoney;
            if (marryData != null)
            {
                GameComponentData.gameData.gameManager.childData = new ChildData
                {
                    BodyGrowStatus = marryData.BodyGrowStatus,
                    MindGrowStatus = marryData.MindGrowStatus,
                    bodyValue = marryData.bodyValue,
                    cleanValue = marryData.cleanValue,
                    foodValue = marryData.foodValue,
                    mindValue = marryData.mindValue,
                    moodValue = marryData.moodValue,
                    name = marryData.childName
                };
            }



        }

        public void InitPastureData()
        {
            if (pastureSaveDatas != null)
            {
                for (int i = 0; i < pastureSaveDatas.Count; i++)
                {
                    Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.id == pastureSaveDatas[i].id);
                    pasture.InitPasture(pastureSaveDatas[i]);
                    GameComponentData.gameData.pastureAction.IsPastures[i] = pastureSaveDatas[i].isOpen;
                }

                var x = pastureSaveDatas.FindAll(p => p.isOpen);
                GameComponentData.gameData.pastureAction.pastureNum = x.Count;
            }

        }
        public void InitDeskData()
        {
            GameComponentData.gameData.shopGoldDeskAction.LoadData(deskData);
        }
        public void InitPlantData()
        {

            FarmAction farmAction = GameComponentData.gameData.farmAction;
            if (plantSeveDatas != null)
            {
                for (int i = 0; i < plantSeveDatas.isGrassClear.Count; i++)
                {
                    farmAction.Grasses[i].isClear = plantSeveDatas.isGrassClear[i];
                }
                farmAction.Fields = new List<Field>();
                foreach (var plantSaveData in plantSeveDatas.fields)
                {
                    Field field = new Field(plantSaveData);
                    farmAction.Fields.Add(field);
                }
                PlantAction plantAction = GameComponentData.gameData.plantAction;
                plantAction.Plants = new List<Plant>();
                foreach (var plantSaveData in plantSeveDatas.plantSaveDatas)
                {
                    Plant plant = new Plant(plantSaveData.id, plantSaveData.mapId, new Vector2Int(plantSaveData.coordinateX, plantSaveData.coordinateY), plantSaveData.growedDays,
                        plantSaveData.turnCount, plantSaveData.plantStatus, plantSaveData.dryDays, plantSaveData.statusIndex);
                    //plant.SetPlantStatus();
                    plantAction.Plants.Add(plant);
                }
            }



        }
        public void InitLoadData()
        {


            PlayerDate.playerName = playerSaveData.name;
            PlayerDate.level = playerSaveData.level;
            PlayerDate.season = playerSaveData.season;
            PlayerDate.date = playerSaveData.date;
            PlayerDate.gender = playerSaveData.gender;
            PlayerDate.isMarried = playerSaveData.isMarried;
            PlayerDate.BoxPackage = 1;
            PlayerDate.IcePackage = 2;
            PlayerDate.package = 0;
            PlayerDate.isAnMo = playerSaveData.isAnMo;
            PlayerDate.isMarriedFood = playerSaveData.isMarriedFood;

            PlayerDate.weapon = playerSaveData.weapon;
            PlayerDate.clothes = playerSaveData.clothes;
            if (playerSaveData.titles != null)
            {
                foreach (var title in playerSaveData.titles)
                {
                    CharactorTitle charactorTitle =
                        GameComponentData.gameData.charactorTitleAction.CharactorTitles.Find(t => t.id == title);
                    charactorTitle.isGet = true;
                }
            }
            if (playerSaveData.EquipMentIds != null)
            {
                foreach (var equipMentId in playerSaveData.EquipMentIds)
                {
                    Euqipment euqipment =
                        GameComponentData.gameData.equipmentManager.Euqipments.Find(e => e.id == equipMentId);
                    euqipment.isBuy = true;
                }
            }

            if (playerSaveData.formulas != null)
            {
                foreach (var formula in playerSaveData.formulas)
                {
                    Formula _formula = GameComponentData.gameData.formulaAction.Formulas.Find(f => f.id == formula);
                    _formula.isOpen = true;
                }
            }
        }

    }

    [System.Serializable]
    public class GameSaveData
    {
        public PauseTime pauseTime;
        public CharactorTitleValue charactorTitleValue;
        public PlayerMoneyData playerMoneyData;
        public DeskData deskData;
        public DateData dateData;
        public PlayerSaveData playerSaveData;
        public PlantFieldSeveData plantSeveDatas;
        public List<NpcSaveData> npcSaveDatas;
        public List<PastureSaveData> pastureSaveDatas;
        public SaveTime saveTime1;
        public string saveTime;

        public RedMoney reMoney;
        public MarryData marryData;
        public void SaveData()
        {
            UpDataPauseTime();
            charactorTitleValue.UpData();
            playerMoneyData.UpData();
            deskData.UpData();
            dateData.UpData();
            playerSaveData.UpPlayerData();
            plantSeveDatas.UpData();

            foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
            {
                NpcSaveData npcSaveData = new NpcSaveData(npcx);
                npcSaveDatas.Add(npcSaveData);
            }

            pastureSaveDatas = new List<PastureSaveData>();
            for (int i = 0; i < GameComponentData.gameData.pastureAction.Pastures.Count; i++)
            {
                PastureSaveData pastureSaveData = new PastureSaveData(GameComponentData.gameData.pastureAction.Pastures[i], GameComponentData.gameData.pastureAction.IsPastures[i]);
                pastureSaveDatas.Add(pastureSaveData);
            }
            saveTime1.value = System.DateTime.Now.ToString();
            UpRedMoney();

        }

        public void UpDataPauseTime()
        {
            DateTime x = System.DateTime.Now;
            pauseTime = new PauseTime();
            pauseTime.year = x.Year;
            pauseTime.month = x.Month;
            pauseTime.day = x.Day;
            pauseTime.hour = x.Hour;
            pauseTime.min = x.Minute;
            pauseTime.isAction = true;

            //DataSaveAndLoadTest.CreatPauseTimeData();
        }

        public void UpDataNpc()
        {
            npcSaveDatas = new List<NpcSaveData>();
            foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
            {
                NpcSaveData npcSaveData = new NpcSaveData(npcx);
                npcSaveDatas.Add(npcSaveData);
            }
            DataSaveAndLoadTest.CreatNPCData();
        }

        public void UpDate()
        {
            dateData.UpData();
            DataSaveAndLoadTest.CreatDateData();

        }
        public void UpPlayerMoneyData()
        {
            playerMoneyData.UpData();
            // DataSaveAndLoadTest.CreatPlayerMoneyData();

        }

        public void UpSaveTime()
        {
            saveTime1 = new SaveTime();
            saveTime1.value = System.DateTime.Now.ToString();
            //DataSaveAndLoadTest.CreatSaveTimedata();
        }
        public void UpRedMoney()
        {
            reMoney = new RedMoney();
            reMoney.value = GameComponentData.gameData.gameManager.gamePlayer.money1;
            DataSaveAndLoadTest.CreatRedMoneyData();
        }
        public void UpDataPlayer()
        {
            playerSaveData.UpPlayerData();
            DataSaveAndLoadTest.CreatPlayerData();

        }

        public void UpDeskData()
        {
            deskData.UpData();
            DataSaveAndLoadTest.CreatDeskData();

        }
        public void UpDataPastureData()
        {
            pastureSaveDatas = new List<PastureSaveData>();
            for (int i = 0; i < GameComponentData.gameData.pastureAction.Pastures.Count; i++)
            {
                PastureSaveData pastureSaveData = new PastureSaveData(GameComponentData.gameData.pastureAction.Pastures[i], GameComponentData.gameData.pastureAction.IsPastures[i]);
                pastureSaveDatas.Add(pastureSaveData);
            }
            DataSaveAndLoadTest.CreatPastureData();

        }
        public void UpPlantData()
        {
            plantSeveDatas.UpData();
            DataSaveAndLoadTest.CreatPlantData();
        }

        public void UpCharactorTitleValue()
        {
            charactorTitleValue.UpData();
            DataSaveAndLoadTest.CreatPlayerTitleData();
        }

        public void ZeroInitData()
        {
            dateData = new DateData
            {
                year = 1300,
                season = Season.夏,
                date = 20
            };
            playerSaveData = new PlayerSaveData
            {
                name = PlayerDate.playerName,
                gender = PlayerDate.gender,
                level = PlayerDate.level,
                date = PlayerDate.date,
                clothes = 0,
                weapon = 0,
                exp = 0
            };

            deskData = new DeskData();
            playerMoneyData = new PlayerMoneyData();
            plantSeveDatas = new PlantFieldSeveData();
            npcSaveDatas = new List<NpcSaveData>();
            pastureSaveDatas = new List<PastureSaveData>();
            playerSaveData.EquipMentIds = new List<int>();
            playerSaveData.formulas = new List<int>();
            playerSaveData.titles = new List<int>();
            charactorTitleValue = new CharactorTitleValue();
        }

        public void ZeroInitProperty(Property property)
        {
            if (!DataSaveAndLoadTest.LoadRedMoney())
            {
                reMoney = new RedMoney();
                reMoney.value = 0;
            }

            playerMoneyData = new PlayerMoneyData
            {
                GroundMoney = 0,
                Hp = property.HP,
                power = property.Power,
                money0 = 0,

                seedId = 0,
                seedCount = 0,
                waterValue = 0
            };
        }

        public void InitCharactorTitleValue()
        {
            CharactorTitleAction charactorTitleAction = GameComponentData.gameData.charactorTitleAction;
            charactorTitleAction.plantingExp = charactorTitleValue.plantingExp;
            charactorTitleAction.livestockExp = charactorTitleValue.livestockExp;
            charactorTitleAction.manufatureExp = charactorTitleValue.manufatureExp;
            charactorTitleAction.CookingExp = charactorTitleValue.CookingExp;
            charactorTitleAction.busicessExp = charactorTitleValue.busicessExp;
            charactorTitleAction.fishSellMoney = charactorTitleValue.fishSellMoney;
            charactorTitleAction.equipSellMoney = charactorTitleValue.equipSellMoney;
            charactorTitleAction.foodSellMoney = charactorTitleValue.foodSellMoney;
            charactorTitleAction.farmProduceSellMoney = charactorTitleValue.farmProduceSellMoney;
            charactorTitleAction.weaponCont = charactorTitleValue.weaponCont;
            charactorTitleAction.equipCount = charactorTitleValue.equipCount;
            charactorTitleAction.riceCount = charactorTitleValue.riceCount;
            charactorTitleAction.wineCont = charactorTitleValue.wineCont;
            charactorTitleAction.fishCookCont = charactorTitleValue.fishCookCont;
            charactorTitleAction.soupCount = charactorTitleValue.soupCount;
            charactorTitleAction.vegetableCount = charactorTitleValue.vegetableCount;
            charactorTitleAction.canCount = charactorTitleValue.canCount;
            charactorTitleAction.explorCount = charactorTitleValue.explorCount;
            charactorTitleAction.killMonsterCount = charactorTitleValue.killMonsterCount;
            charactorTitleAction.failureCount = charactorTitleValue.failureCount;
            charactorTitleAction.sleepDays = charactorTitleValue.sleepDays;
            charactorTitleAction.fishTotals = new List<Vector2Int>();
            for (int i = 0; i < charactorTitleValue.fishes.Count; i++)
            {
                charactorTitleAction.fishTotals.Add(new Vector2Int(charactorTitleValue.fishes[i], charactorTitleValue.Totals[i]));
            }
            charactorTitleAction.getItems = new List<int>();
            foreach (var item in charactorTitleValue.getItems)
            {
                charactorTitleAction.getItems.Add(item);
            }

        }
        public void InitNpcLoadData()
        {
            if (npcSaveDatas != null)
            {
                foreach (var npcSaveData in npcSaveDatas)
                {
                    NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == npcSaveData.id);
                    npcx.WaitDays = npcSaveData.waitDays;
                    npcx.clothes = npcSaveData.clothes;
                    npcx.weapon = npcSaveData.weapon;
                    npcx.npcData.brothDate = npcSaveData.date;
                    npcx.npcData.brothSeason = npcSaveData.season;
                    npcx.npcData.friendlyLevel = npcSaveData.friendlyLevel;
                    npcx.npcData.frienflyExp = npcSaveData.friendlyEXP;
                    npcx.level = npcSaveData.level;
                    npcx.property = npcx.professionData.ZeroProperty + npcx.professionData.GetPropertyFromLevel(npcx.level);
                    npcx.property.EXP = npcSaveData.exp;
                    npcx.property.HP = npcSaveData.hp;
                    npcx.isFriendlyExpAdd = npcSaveData.isFriendlyExpAdd;
                    npcx.isGiftExpAdd0 = npcSaveData.isGiftExpAdd0;
                    npcx.isGiftExpAdd1 = npcSaveData.isGiftExpAdd1;
                    npcx.isGiftExpAdd2 = npcSaveData.isGiftExpAdd2;
                    npcx.istteamExpAdd = npcSaveData.istteamExpAdd;
                    npcx.isFlower = npcSaveData.isFlower;
                    npcx.isLove = npcSaveData.isLove;
                    npcx.isMarried = npcSaveData.isMarried;
                    npcx.isYuehui = npcSaveData.isYuehui;
                    if (npcx.isMarried)
                    {
                        GameComponentData.gameData.NpcManager.SetMarriedNpcPos(npcx);
                    }
                }
            }

        }
        public void InitPlayerLoadData(GamePlayer gamePlayer)
        {
            GameTimeManager.nowGameTime = new GameTime(dateData.year, dateData.season, dateData.date, 0, 0);
            InitPastureData();
            InitPlantData();
            InitNpcLoadData();
            InitLoadData();
            InitCharactorTitleValue();
            GameComponentData.gameData.heritageAction.CheckZeroHeritagesData();
            if (playerMoneyData.battleAllValues.Count > 0)
            {
                if (playerMoneyData.battleAllValues.Count <= 10)
                {
                    var battleMaps =
                        GameComponentData.gameData.BattleMapAction.BatteleMaps.FindAll(b => b.id < 4010);
                    for (int i = 0; i < battleMaps.Count; i++)
                    {
                        battleMaps[i].allValue = playerMoneyData.battleAllValues[i];
                    }
                }
                else
                {
                    for (int i = 0; i < GameComponentData.gameData.BattleMapAction.BatteleMaps.Count; i++)
                    {
                        GameComponentData.gameData.BattleMapAction.BatteleMaps[i].allValue = playerMoneyData.battleAllValues[i];
                    }
                }

            }
            GameComponentData.gameData.BattleMapAction.InitBattleMaps();

            gamePlayer.InitGamePlayer();
            gamePlayer.property.EXP = playerSaveData.exp;
            gamePlayer.property.HP = playerMoneyData.Hp;
            gamePlayer.property.Power = playerMoneyData.power;
            gamePlayer.attributeType = playerSaveData.attributeType;
            gamePlayer.money = playerMoneyData.money0;

            gamePlayer.money1 = reMoney.value;
            if (playerSaveData.teamPlayer0 != 0)
            {
                if (playerSaveData.teamPlayer0 / 1000000 == 2)
                {
                    Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.Animals.Exists(a => a.id == playerSaveData.teamPlayer0));
                    if (pasture != null)
                    {
                        Animal animal = pasture.Animals.Find(a => a.id == playerSaveData.teamPlayer0);
                        Employer employer1 = new Employer(animal);
                        gamePlayer.TeamPlayer0 = new TeamPlayer(employer1);
                        gamePlayer.TeamPlayer0.property.HP = playerMoneyData.team1Hp;
                        animal.property.HP = playerMoneyData.team1Hp;
                        animal.level = playerMoneyData.team1Level;
                        animal.property.EXP = playerMoneyData.team1Exp;
                    }


                }
                else
                {
                    NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == playerSaveData.teamPlayer0);
                    if (npcx != null)
                    {
                        Employer employer = new Employer(npcx);
                        gamePlayer.TeamPlayer0 = new TeamPlayer(employer);
                        gamePlayer.TeamPlayer0.property.HP = playerMoneyData.team1Hp;
                        npcx.property.HP = playerMoneyData.team1Hp;
                        npcx.level = playerMoneyData.team1Level;
                        npcx.property.EXP = playerMoneyData.team1Exp;
                    }


                }

            }
            if (playerSaveData.teamPlayer1 != 0)
            {
                if (playerSaveData.teamPlayer1 / 1000000 == 2)
                {
                    Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.Animals.Exists(a => a.id == playerSaveData.teamPlayer1));

                    if (pasture != null)
                    {
                        Animal animal = pasture.Animals.Find(a => a.id == playerSaveData.teamPlayer1);
                        Employer employer1 = new Employer(animal);
                        gamePlayer.TeamPlayer1 = new TeamPlayer(employer1);
                        gamePlayer.TeamPlayer1.property.HP = playerMoneyData.team2Hp;
                        animal.property.HP = playerMoneyData.team2Hp;
                        animal.level = playerMoneyData.team2Level;
                        animal.property.EXP = playerMoneyData.team2Exp;
                    }

                }
                else
                {
                    NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == playerSaveData.teamPlayer1);
                    if (npcx != null)
                    {
                        Employer employer = new Employer(npcx);
                        gamePlayer.TeamPlayer1 = new TeamPlayer(employer);
                        gamePlayer.TeamPlayer1.property.HP = playerMoneyData.team2Hp;
                        npcx.property.HP = playerMoneyData.team2Hp;
                        npcx.level = playerMoneyData.team2Level;
                        npcx.property.EXP = playerMoneyData.team2Exp;
                    }

                }

            }
            FarmAction farmAction = GameComponentData.gameData.farmAction;
            if (playerMoneyData.seedId != 0)
            {
                farmAction.farmTool = ItemManager.instance.CreatItem(playerMoneyData.seedId, playerMoneyData.seedCount);
            }

            farmAction.waterValue = playerMoneyData.waterValue / 1000.0f;


            foreach (var _formula in playerSaveData.formulas)
            {
                Formula formula = GameComponentData.gameData.formulaAction.Formulas.Find(f => f.id == _formula);
                formula.isOpen = true;
            }

            foreach (var equipMentId in playerSaveData.EquipMentIds)
            {
                var x = GameComponentData.gameData.equipmentManager.Euqipments.Find(e => e.id == equipMentId);
                x.isBuy = true;
            }
            if (playerMoneyData.GroundMoney < 0)
            {
                playerMoneyData.GroundMoney = 0;
            }
            GameComponentData.gameData.coinAction.GroundMoney = playerMoneyData.GroundMoney;
            if (marryData != null)
            {
                GameComponentData.gameData.gameManager.childData = new ChildData
                {
                    BodyGrowStatus = marryData.BodyGrowStatus,
                    MindGrowStatus = marryData.MindGrowStatus,
                    bodyValue = marryData.bodyValue,
                    cleanValue = marryData.cleanValue,
                    foodValue = marryData.foodValue,
                    mindValue = marryData.mindValue,
                    moodValue = marryData.moodValue,
                    name = marryData.childName
                };
            }



        }

        public void InitPastureData()
        {
            if (pastureSaveDatas != null)
            {
                for (int i = 0; i < pastureSaveDatas.Count; i++)
                {
                    Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.id == pastureSaveDatas[i].id);
                    pasture.InitPasture(pastureSaveDatas[i]);
                    GameComponentData.gameData.pastureAction.IsPastures[i] = pastureSaveDatas[i].isOpen;
                }

                var x = pastureSaveDatas.FindAll(p => p.isOpen);
                GameComponentData.gameData.pastureAction.pastureNum = x.Count;
            }

        }
        public void InitDeskData()
        {
            GameComponentData.gameData.shopGoldDeskAction.LoadData(deskData);
        }
        public void InitPlantData()
        {

            FarmAction farmAction = GameComponentData.gameData.farmAction;
            if (plantSeveDatas != null)
            {
                for (int i = 0; i < plantSeveDatas.isGrassClear.Count; i++)
                {
                    farmAction.Grasses[i].isClear = plantSeveDatas.isGrassClear[i];
                }
                farmAction.Fields = new List<Field>();
                foreach (var plantSaveData in plantSeveDatas.fields)
                {
                    Field field = new Field(plantSaveData);
                    farmAction.Fields.Add(field);
                }
                PlantAction plantAction = GameComponentData.gameData.plantAction;
                plantAction.Plants = new List<Plant>();
                foreach (var plantSaveData in plantSeveDatas.plantSaveDatas)
                {
                    Plant plant = new Plant(plantSaveData.id, plantSaveData.mapId, new Vector2Int(plantSaveData.coordinateX, plantSaveData.coordinateY), plantSaveData.growedDays,
                        plantSaveData.turnCount, plantSaveData.plantStatus, plantSaveData.dryDays, plantSaveData.statusIndex);
                    //plant.SetPlantStatus();
                    plantAction.Plants.Add(plant);
                }
            }



        }
        public void InitLoadData()
        {


            PlayerDate.playerName = playerSaveData.name;
            PlayerDate.level = playerSaveData.level;
            PlayerDate.season = playerSaveData.season;
            PlayerDate.date = playerSaveData.date;
            PlayerDate.gender = playerSaveData.gender;
            PlayerDate.isMarried = playerSaveData.isMarried;
            PlayerDate.BoxPackage = 1;
            PlayerDate.IcePackage = 2;
            PlayerDate.package = 0;
            PlayerDate.isAnMo = playerSaveData.isAnMo;
            PlayerDate.isMarriedFood = playerSaveData.isMarriedFood;

            PlayerDate.weapon = playerSaveData.weapon;
            PlayerDate.clothes = playerSaveData.clothes;
            if (playerSaveData.titles != null)
            {
                foreach (var title in playerSaveData.titles)
                {
                    CharactorTitle charactorTitle =
                        GameComponentData.gameData.charactorTitleAction.CharactorTitles.Find(t => t.id == title);
                    charactorTitle.isGet = true;
                }
            }
            if (playerSaveData.EquipMentIds != null)
            {
                foreach (var equipMentId in playerSaveData.EquipMentIds)
                {
                    Euqipment euqipment =
                        GameComponentData.gameData.equipmentManager.Euqipments.Find(e => e.id == equipMentId);
                    euqipment.isBuy = true;
                }
            }

            if (playerSaveData.formulas != null)
            {
                foreach (var formula in playerSaveData.formulas)
                {
                    Formula _formula = GameComponentData.gameData.formulaAction.Formulas.Find(f => f.id == formula);
                    _formula.isOpen = true;
                }
            }
        }

    }

}

