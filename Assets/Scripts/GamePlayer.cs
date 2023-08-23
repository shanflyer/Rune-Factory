using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OldName
{
    public static class PlayerDate
    {
        public static string playerName;
        public static int level;
        public static Gender gender;
        public static Season season;
        public static int date;
        public static int package, BoxPackage, IcePackage;
        public static int weapon, clothes;
        public static bool isMarried;
        public static bool isMarriedFood, isAnMo;

    }

    [System.Serializable]
    public enum PowerCostType
    {
        锄地 = 0,
        栽种 = 1,
        浇水 = 2,
        收获 = 3,
        喂草 = 4,
        行进 = 5
    }

    [System.Serializable]
    public class TeamPlayer
    {
        public int id;
        public string name;
        public int level;
        public int profession;
        public int Weapon, clothes;
        public string charactorImage, ObjName;
        public int skillId;
        public AttributeType attributeType;
        [HideInInspector] public Property property;

        public TeamPlayer(int _id, string _name, int _level, int _profession, Property _property, int _skill)
        {
            id = _id;
            name = _name;
            level = _level;
            property = _property;
            profession = _profession;
            skillId = _skill;
        }


        public void AddExpValue(int value)
        {
            level = property.AddExp(value, profession, level);


            if (id / 1000000 == 2)
            {
                Employer employer = GameComponentData.gameData.employerManger.Employers.Find(e => e.id == id);
                Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.Animals.Exists(a => a.id == id));
                Animal animal = pasture.Animals.Find(a => a.id == id);
                animal.property = property;
                animal.level = level;
            }
            else
            {
                NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == id);
                if (npcx != null)
                {
                    npcx.property = property;
                    npcx.level = level;
                }

            }
        }
        public void AddHpValue(int value)
        {
            property.HP += value;
            if (property.HP > property.MaxHP)
            {
                property.HP = property.MaxHP;
            }
            if (property.HP < 0)
            {
                property.HP = 0;
            }
            if (id / 1000000 == 2)
            {
                Employer employer = GameComponentData.gameData.employerManger.Employers.Find(e => e.id == id);
                Pasture pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.Animals.Exists(a => a.id == id));
                Animal animal = pasture.Animals.Find(a => a.id == id);
                animal.property.HP = property.HP;
                employer.property.HP = property.HP;
            }
            else
            {
                NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == id);
                if (npcx != null)
                {
                    npcx.property.HP = property.HP;
                    Employer employer = GameComponentData.gameData.employerManger.Employers.Find(e => e.id == id);
                    employer.property.HP = property.HP;
                }

            }
        }
        public TeamPlayer(Employer _employer)
        {
            _employer.isHired = true;
            id = _employer.id;
            name = _employer.name;
            level = _employer.level;
            profession = _employer.profession;
            skillId = _employer.skillId;
            property = _employer.property;
            charactorImage = _employer.charactorImage;
            ObjName = _employer.ObjName;
            Weapon = _employer.weapon;
            clothes = _employer.clothes;
            attributeType = _employer.attributeType;
        }
    }
    [System.Serializable]
    public class GameTeam
    {

    }

    [System.Serializable]
    public class ChildData
    {
        public string name;
        public int bodyValue, mindValue;
        public int foodValue, moodValue, cleanValue;
        public GrowStatus BodyGrowStatus, MindGrowStatus;
        public ChildData() { }

        public ChildData(string _name)
        {
            name = _name;
            bodyValue = 0;
            mindValue = 0;
            foodValue = 5;
            moodValue = 5;
            cleanValue = 5;
            BodyGrowStatus = GrowStatus.中速成长;
            MindGrowStatus = GrowStatus.中速成长;

        }

    }
    [System.Serializable]
    public class GamePlayer
    {
        public int id;
        public string name;
        public int level;
        public int packageZeroCout, boxZeroCount, iceboxZeroCount;
        public int package = 0, box = 1, icebox = 2;
        public int money, money1;
        [HideInInspector]
        public string ObjName;

        public bool isMarried;
        public string marriedObjName;
        public int SkillId;
        [HideInInspector] public string IconName, playerImage;
        public Gender gender;
        public Season season;
        public int date;
        public Property property;
        public Item weapon, clothes;

        public AttributeType attributeType;
        [HideInInspector]
        public TeamPlayer TeamPlayer0, TeamPlayer1;
        public bool isMarriedFood, isAnMo;
        public int oldLevel0, oldLevel1, oldLevel2;

        // Use this for initialization
        void Start()
        {

        }

        public void InitOldLevel()
        {
            oldLevel0 = level;
            if (TeamPlayer0 != null)
            {
                oldLevel1 = TeamPlayer0.level;
            }
            if (TeamPlayer1 != null)
            {
                oldLevel2 = TeamPlayer1.level;
            }
        }
        public async void InitGamePlayer()
        {
            id = 8 * 1000;
            if (PlayerDate.level != 0)
            {
                name = PlayerDate.playerName;
                level = PlayerDate.level;
                gender = PlayerDate.gender;
                season = PlayerDate.season;
                date = PlayerDate.date;
                isMarried = PlayerDate.isMarried;
                isAnMo = PlayerDate.isAnMo;
                isMarriedFood = PlayerDate.isMarriedFood;

            }

            TeamPlayer0 = null;
            TeamPlayer1 = null;
            ProfessionData playerProfessionData =
                GameComponentData.gameData.charactorDataAction.professionDatas0.Find(p => p.id == 8);
            property = playerProfessionData.ZeroProperty + playerProfessionData.GetPropertyFromLevel(level);
            attributeType = AttributeType.无;
            if (PlayerDate.weapon == 0)
            {
                weapon = default(Item);

            }
            else
            {
                weapon = ItemManager.instance.CreatItem(PlayerDate.weapon, 1);
                var data = await GameDataManager.instance.GetAsyncData<ItemData>(weapon.dataId.ToString());
                property += data.property;
            }
            if (PlayerDate.clothes == 0)
            {
                clothes = default(Item);
            }
            else
            {
                clothes = ItemManager.instance.CreatItem(PlayerDate.clothes, 1);
                var data = await GameDataManager.instance.GetAsyncData<ItemData>(clothes.dataId.ToString());
                property += data.property;
            }
            if (gender == Gender.female)
            {
                ObjName = "Femeal";
                marriedObjName = "xin1";
                IconName = "7";
                playerImage = "1_47";
            }
            else
            {
                ObjName = "Meal";
                marriedObjName = "xinlang5";
                IconName = "0";
                playerImage = "1_1";
            }
            package = PlayerDate.package;

            box = PlayerDate.BoxPackage;
            icebox = PlayerDate.IcePackage;

            if (PlayerDate.weapon != 0)
            {
                weapon = ItemManager.instance.CreatItem(PlayerDate.weapon, 1);
            }
            if (PlayerDate.clothes != 0)
            {
                clothes = ItemManager.instance.CreatItem(PlayerDate.clothes, 1);
            }


        }


        // Update is called once per frame
        void Update()
        {

        }
    }

}
