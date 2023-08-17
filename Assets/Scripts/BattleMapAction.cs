using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;
using System.IO;
namespace OldName
{
    [System.Serializable]
    public struct RewardItem
    {
        public int itemID;
        public int count0, count1;
        public int Weights;
    }

    [System.Serializable]
    public class MonsterData
    {
        public string name;
        public int id;
        public int level;
        public int profession;
        public string objName;
        public string hitName;
        public string hitSe;
        public int rewardMoneyMin, rewardMoneyMax;
        public List<RewardItem> RewardItems;
        public int skill;
        public MonsterData() { }
        public MonsterData(MonsterStrData monsterStrData)
        {
            name = monsterStrData.name;
            id = monsterStrData.id;
            level = monsterStrData.level;
            profession = monsterStrData.profession;
            objName = monsterStrData.objName;
            hitName = monsterStrData.hitName;
            hitSe = monsterStrData.hitSe;
            rewardMoneyMax = monsterStrData.rewardMoneyMax;
            rewardMoneyMin = monsterStrData.rewardMoneyMin;
            RewardItems = new List<RewardItem>();

            if (monsterStrData.rewardItemStr != "")
            {
                var x = monsterStrData.rewardItemStr.Split(';');
                foreach (var s in x)
                {
                    var ss = s.Split(',');
                    RewardItem rewardItem = new RewardItem
                    {
                        itemID = int.Parse(ss[0]),
                        count0 = int.Parse(ss[1]),
                        count1 = int.Parse(ss[2]),
                        Weights = int.Parse(ss[3])
                    };
                    RewardItems.Add(rewardItem);
                }
            }
            skill = monsterStrData.skill;

        }
    }
    [System.Serializable]
    public class MonsterStrData
    {
        public string name;
        public int id;
        public int level;
        public int profession;
        public string objName;
        public string hitName;
        public string hitSe;
        public int rewardMoneyMin, rewardMoneyMax;
        public string rewardItemStr;
        public int skill;

        public MonsterStrData()
        {

        }
        public MonsterStrData(MonsterData monsterData)
        {
            name = monsterData.name;
            id = monsterData.id;
            level = monsterData.level;
            profession = monsterData.profession;
            objName = monsterData.objName;
            hitName = monsterData.hitName;
            hitSe = monsterData.hitSe;
            rewardMoneyMin = monsterData.rewardMoneyMin;
            rewardMoneyMax = monsterData.rewardMoneyMax;
            rewardItemStr = "";
            foreach (var monsterDataRewardItem in monsterData.RewardItems)
            {
                rewardItemStr += monsterDataRewardItem.itemID + "," + monsterDataRewardItem.count0 + "," + monsterDataRewardItem.count1 + "," + monsterDataRewardItem.Weights + ";";
            }
            skill = monsterData.skill;
        }
    }

    [System.Serializable]
    public class BattleMnonster
    {
        public int weight;
        public int monsterId0, monsterId1, monsterId2;
        public AttributeType attributeType0, attributeType1, attributeType2;
        public int probability0, probability1, probability2;
        public BattleMnonster() { }

        public BattleMnonster(MonsterGroup monsterGroup)
        {
            weight = monsterGroup.weight;
            monsterId0 = monsterGroup.monsterId0;
            monsterId1 = monsterGroup.monsterId1;
            monsterId2 = monsterGroup.monsterId2;
            attributeType0 = (AttributeType)monsterGroup.attributeType0;
            attributeType1 = (AttributeType)monsterGroup.attributeType1;
            attributeType2 = (AttributeType)monsterGroup.attributeType2;
            probability0 = monsterGroup.probability0;
            probability1 = monsterGroup.probability1;
            probability2 = monsterGroup.probability2;
        }
    }
    [System.Serializable]
    public class MapMonster
    {
        public int timeValue;
        public List<BattleMnonster> BattleMnonsters;

        public MapMonster()
        {
            timeValue = 0;
            BattleMnonsters = new List<BattleMnonster>();
        }
    }

 

    public class Monster : MyGameObject
    {
        public int level;
        public Property property;
        public Skill skill;
        public int skillValue;
        public MonsterData monsterData;
        public AttributeType attributeType;
        public GameObject attributeMask;
        public int maskHp;
        public bool isDead;
        public Monster()
        {
        }
        public Monster(MonsterData _monsterData)
        {
            monsterData = _monsterData;
            base.Name = _monsterData.name;
            base.id = _monsterData.id;
            level = _monsterData.level;
            isDead = false;
            attributeType = AttributeType.无;
            ProfessionData professionData =
                GameComponentData.gameData.charactorDataAction.professionDatas0.Find(p => p.id == _monsterData.profession);
            property = professionData.ZeroProperty + professionData.GetPropertyFromLevel(level);
            if (monsterData.skill != 0)
            {
                skill = new Skill(monsterData.skill, property.AT);
            }
            skillValue = 0;
        }

        public void SetSpriteLayerOrder(int layerOrder)
        {
            Obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = layerOrder;
            if (attributeMask != null)
            {
                attributeMask.GetComponentInChildren<AttributeMaskAction>().SetSortLayer(layerOrder + 1);
            }
        }
        public Monster(MonsterData _monsterData, Vector3 pos, AttributeType _attributeType)
        {
            monsterData = _monsterData;
            base.Name = _monsterData.name;
            base.id = _monsterData.id;
            level = _monsterData.level;
            isDead = false;
            ProfessionData professionData =
                GameComponentData.gameData.charactorDataAction.professionDatas0.Find(p => p.id == _monsterData.profession);
            property = professionData.ZeroProperty + professionData.GetPropertyFromLevel(level);
            property.HP = property.MaxHP;
            GameObject pro = GameComponent.monsterobjs.Find(o => o.name == monsterData.objName);
            Obj = MonoBehaviour.Instantiate(pro);
            Obj.transform.SetParent(GameComponentData.gameData.BattleMapAction.mapParent, false);
            Obj.transform.position = pos;
            attributeType = _attributeType;
            if (monsterData.skill != 0)
            {

                skill = new Skill(monsterData.skill, property.AT);
            }

            switch (attributeType)
            {
                case AttributeType.无:

                    break;
                case AttributeType.光:
                    attributeMask = MonoBehaviour.Instantiate(GameComponentData.gameData.BattleMapAction.lightMask);
                    attributeMask.transform.SetParent(Obj.transform, false);
                    attributeMask.transform.position = Vector3.zero;
                    maskHp = property.MaxHP / 2;
                    break;
                case AttributeType.冰:
                    attributeMask = MonoBehaviour.Instantiate(GameComponentData.gameData.BattleMapAction.iceMask);
                    attributeMask.transform.SetParent(Obj.transform, false);
                    attributeMask.transform.localPosition = Vector3.zero;
                    maskHp = property.MaxHP / 2;
                    break;
                case AttributeType.暗:
                    attributeMask = MonoBehaviour.Instantiate(GameComponentData.gameData.BattleMapAction.darkMask);
                    attributeMask.transform.SetParent(Obj.transform, false);
                    attributeMask.transform.localPosition = Vector3.zero;
                    maskHp = property.MaxHP / 2;
                    break;
                case AttributeType.火:
                    attributeMask = MonoBehaviour.Instantiate(GameComponentData.gameData.BattleMapAction.fireMask);
                    attributeMask.transform.SetParent(Obj.transform, false);
                    attributeMask.transform.localPosition = Vector3.zero;
                    maskHp = property.MaxHP / 2;
                    break;
                case AttributeType.风:
                    attributeMask = MonoBehaviour.Instantiate(GameComponentData.gameData.BattleMapAction.windMask);
                    attributeMask.transform.SetParent(Obj.transform, false);
                    attributeMask.transform.localPosition = Vector3.zero;
                    maskHp = property.MaxHP / 2;
                    break;
            }


        }

        public List<Item> GetRewardItem()
        {
            List<Item> items = new List<Item>();
            foreach (var monsterDataRewardItem in monsterData.RewardItems)
            {
                int value = Random.Range(0, 10000);
                if (value <= monsterDataRewardItem.Weights)
                {
                    int count = Random.Range(monsterDataRewardItem.count0, monsterDataRewardItem.count1);

                    Item item = ItemManager.instance.CreatItem(monsterDataRewardItem.itemID, count);
                    items.Add(item);
                }
            }
            return items;
        }
    }

    [System.Serializable]
    public enum BattleMapType
    {
        初阶 = 0, 中阶 = 1, 高阶 = 2
    }
    [System.Serializable]
    public class BatteleMap
    {
        public int playSkillValue;
        public string mapName;
        public int id;
        public string ObjName;
        public int monterCd;
        public int completeValue;
        public int allValue;
        public bool isOpen;
        public string battleNotice;
        public BattleMapType battleMapType;
        public AttributeType attributeType;
        public List<MapMonster> mapMonsters;
        public List<int> nextOpen;
        public BatteleMap()
        {
            id = 0;
            mapName = null;
            ObjName = null;
            monterCd = 0;
            completeValue = 0;
            allValue = 0;
            isOpen = false;
            mapMonsters = new List<MapMonster>();
            nextOpen = new List<int>();
            battleMapType = BattleMapType.初阶;
            attributeType = AttributeType.无;
            battleNotice = "";
        }

        public BatteleMap(BatteleMapStr batteleMapStr)
        {
            id = batteleMapStr.id;
            mapName = batteleMapStr.mapName;
            ObjName = batteleMapStr.ObjName;
            monterCd = batteleMapStr.monterCd;
            completeValue = 0;
            allValue = 0;
            isOpen = false;

            mapMonsters = new List<MapMonster>();
            GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
            var _monsterGroup = gameController.GetComponent<BattleMapAction>().MonsterGroups.FindAll(m => m.id == batteleMapStr.monsterGrop);
            MapMonster mapMonster = new MapMonster();

            mapMonsters.Add(mapMonster);
            foreach (var monsterGroup in _monsterGroup)
            {
                if (monsterGroup.time == mapMonster.timeValue)
                {

                    mapMonster.BattleMnonsters.Add(new BattleMnonster(monsterGroup));
                }
                else
                {
                    mapMonster = new MapMonster();
                    mapMonster.timeValue = monsterGroup.time;
                    mapMonsters.Add(mapMonster);
                    mapMonster.BattleMnonsters.Add(new BattleMnonster(monsterGroup));
                }
            }
            nextOpen = new List<int>();
            var y = batteleMapStr.nextOpen.Split(',');
            foreach (var s in y)
            {
                nextOpen.Add(int.Parse(s));
            }
            battleNotice = batteleMapStr.battleNotice;
            battleMapType = batteleMapStr.battleMapType;
            attributeType = batteleMapStr.attributeType;
        }
    }
    [System.Serializable]
    public class BatteleMapStr
    {
        public int id;
        public string mapName;
        public string ObjName;
        public int monterCd;
        public int monsterGrop;
        public string nextOpen;
        public string battleNotice;
        public BattleMapType battleMapType;
        public AttributeType attributeType;
        public BatteleMapStr() { }

    }
    public class BattleMapAction : MonoBehaviour
    {

        public GameObject charactorCanvas;
        public GameObject attributeEffect;
        public Color FireColor, IceColor, DarkColor, LightColor, WideColor;
        public GameObject fireMask, iceMask, darkMask, lightMask, windMask;
        public Transform fightCameraPos;
        public Transform flyItemparent;
        public string hitDefalutSe, missSe;
        public GameObject Boxpro;
        public GameObject ResultObj;
        public GameObject flyItemPro;
        public GameObject FightInformationObj;
        public GameObject hurtHpPro;
        public float playerfightCd;
        public Image SkillValueImage;
        public Text SkillvalueText;
        public Vector2 ZeroPos0, ZeroPos1, ZeroPos2, startPos0, startPos1, startPos2;
        public Transform MonsterPos0, MonsterPos1, MonsterPos2;
        public Transform ItemArea0, ItemArea1;
        public float itemflySpeed, itemCd;
        public List<BatteleMap> BatteleMaps;
        public List<MonsterGroup> MonsterGroups;
        public int firstBattleMap;
        [HideInInspector]
        public List<BatteleMapStr> BatteleMapStrs;
        public float moveTime;
        public Transform mapParent;
        private GameObject mapPro;
        private GameObject charactorPro;
        private GameObject map0, map1;
        [HideInInspector]
        public GameObject charactorObj, TeamPlayer0, TeamPlayer1;
        private GameObject teamPlayerObj0, teamPlayerObj1;
        [HideInInspector]
        public GamePlayer gamePlayer;
        private bool isDead0, isDead1, isDead2;
        private float timeValue;
        private float monsterTime;
        private BatteleMap batteleMap;
        private Monster monster0, monster1, monster2;
        public bool isMoving;
        private int SkillCount;
        private float SkillValue;
        private ProfessionData monsterProfessionData0, monsterProfessionData1, monsterProfessionData2;
        private ProfessionData playerProfessionData, teamPlayerProfessionData0, teamPlayerProfessionData1;
        public bool isAuto;
        private bool isPlayerBattle;
        private bool fightEnd0, fightEnd1, fightEnd2, monsterEnd0, monsterEnd1, monsterEnd2;
        private bool isMosterHurtEnd0, isMosterHurtEnd1, isMosterHurtEnd2;
        private bool isPlayerHurtEnd0, isPlayerHurtEnd1, isPlayerHurtEnd2;
        private int moneyValue, rewardEXP;
        private bool isFightContineu;
        private List<Item> flyItems;
        private List<Vector2> ItemPoses;
        private List<Item> rewardItems;
        private List<Item> Resultitems;
        private bool isEndMonster;
        private List<GameObject> itemObjects;

        private GameObject boxObj;
        private float damageValue0, damageValue1, damageValue2;
        private int hp;
        private int switchValue;
        private float timeScaleValue;
        public Button switchButton;

        public bool isZero;

        public bool isDisplayResult;


        private GameObject addObj;

        private int addHp;
        // Use this for initialization
        void Start()
        {

        }

        public void InitData()
        {
            gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
            isMoving = false;
            SkillCount = 0;
            SkillValue = 0;
            SkillvalueText.text = SkillCount.ToString();
            SkillValueImage.fillAmount = SkillValue;
            teamPlayerObj0 = null;
            teamPlayerObj1 = null;
            fightEnd0 = fightEnd1 = fightEnd2 = false;
            //JsonToData();
            BatteleMaps[0].isOpen = true;
        }
        public void CheckBattleMapOpen(BatteleMap batteleMap)
        {
            foreach (var i in batteleMap.nextOpen)
            {
                if (i != 0)
                {
                    BatteleMap next = BatteleMaps.Find(b => b.id == i);
                    next.isOpen = true;
                }

            }
        }
        public void ClickSwitchButton()
        {

            switchButton.interactable = false;
            StartCoroutine("SwitchFlying");
        }
        IEnumerator SwitchFlying()
        {
            Vector3 pz0 = startPos0;
            Vector3 ps0 = startPos1;
            Vector3 px0 = new Vector3(pz0.x + (ps0.x - pz0.x) / 1, pz0.y + (ps0.y - pz0.y) / 1, 0);
            Vector3 Pos0 = pz0;

            Vector3 pz1 = startPos1;
            Vector3 ps1 = startPos2;
            Vector3 px1 = new Vector3(pz1.x + (ps1.x - pz1.x) / 1, pz1.y + (ps1.y - pz1.y) / 1, 0);
            Vector3 Pos1 = pz1;

            Vector3 pz2 = startPos2;
            Vector3 ps2 = startPos0;
            Vector3 px2 = new Vector3(pz2.x + (ps2.x - pz2.x) / 1, pz2.y + (ps2.y - pz2.y) / 1, 0);
            Vector3 Pos2 = pz2;
            float t = 0;
            while (true)
            {
                Pos0 = (1 - t) * (1 - t) * pz0 + 2 * t * (1 - t) * px0 + t * t * ps0;
                Pos1 = (1 - t) * (1 - t) * pz1 + 2 * t * (1 - t) * px1 + t * t * ps1;
                Pos2 = (1 - t) * (1 - t) * pz2 + 2 * t * (1 - t) * px2 + t * t * ps2;
                float speed = 0.6f / moveTime;
                t += speed;

                if (switchValue == 1)
                {
                    if (charactorObj != null)
                    {
                        charactorObj.transform.localPosition = Pos0;
                    }
                    if (teamPlayerObj0 != null)
                    {
                        teamPlayerObj0.transform.localPosition = Pos1;
                    }
                    if (teamPlayerObj1 != null)
                    {
                        teamPlayerObj1.transform.localPosition = Pos2;
                    }
                }
                else if (switchValue == 2)
                {
                    if (charactorObj != null)
                    {
                        charactorObj.transform.localPosition = Pos1;
                    }
                    if (teamPlayerObj0 != null)
                    {
                        teamPlayerObj0.transform.localPosition = Pos2;
                    }
                    if (teamPlayerObj1 != null)
                    {
                        teamPlayerObj1.transform.localPosition = Pos0;
                    }
                }
                else if (switchValue == 3)
                {
                    if (charactorObj != null)
                    {
                        charactorObj.transform.localPosition = Pos2;
                    }
                    if (teamPlayerObj0 != null)
                    {
                        teamPlayerObj0.transform.localPosition = Pos0;
                    }
                    if (teamPlayerObj1 != null)
                    {
                        teamPlayerObj1.transform.localPosition = Pos1;
                    }
                }

                if (t >= 1)
                {
                    SwtichAttackOrder();
                    switchButton.interactable = true;
                    StopCoroutine("SwitchFlying");
                }
                yield return new WaitForSeconds(0.01f);
            }
        }
        public void SwtichAttackOrder()
        {
            switchValue++;
            if (switchValue > 3)
            {
                switchValue = 1;
            }
            InitAttackOrder();
        }

        void InitAttackOrder()
        {
            if (switchValue > 3)
            {
                switchValue = 1;
            }
            if (switchValue == 1)
            {
                if (charactorObj != null && !isDead0)
                {
                    if (teamPlayerObj0 != null && !isDead1)
                    {
                        if (teamPlayerObj1 != null && !isDead2)
                        {
                            damageValue0 = 0.5f;
                            damageValue1 = 0.3f;
                            damageValue2 = 0.2f;
                        }
                        else
                        {
                            damageValue0 = 0.6f;
                            damageValue1 = 0.3f;
                            damageValue2 = 0f;
                        }
                    }
                    else
                    {
                        if (teamPlayerObj1 != null && !isDead2)
                        {
                            damageValue0 = 0.7f;
                            damageValue1 = 0f;
                            damageValue2 = 0.3f;
                        }
                        else
                        {
                            damageValue0 = 1f;
                            damageValue1 = 0f;
                            damageValue2 = 0f;
                        }
                    }
                }
                else
                {
                    if (teamPlayerObj0 != null && !isDead1)
                    {
                        if (teamPlayerObj1 != null && !isDead2)
                        {
                            damageValue0 = 0f;
                            damageValue1 = 0.7f;
                            damageValue2 = 0.3f;
                        }
                        else
                        {
                            damageValue0 = 0f;
                            damageValue1 = 1f;
                            damageValue2 = 0f;
                        }
                    }
                    else
                    {
                        if (teamPlayerObj1 != null && !isDead2)
                        {
                            damageValue0 = 0f;
                            damageValue1 = 0f;
                            damageValue2 = 1f;
                        }
                    }
                }
            }
            else if (switchValue == 2)
            {
                if (teamPlayerObj1 != null && !isDead2)
                {
                    if (charactorObj != null && !isDead0)
                    {
                        if (teamPlayerObj0 != null && !isDead1)
                        {
                            damageValue2 = 0.5f;
                            damageValue0 = 0.3f;
                            damageValue1 = 0.2f;
                        }
                        else
                        {
                            damageValue2 = 0.6f;
                            damageValue0 = 0.3f;
                            damageValue1 = 0f;
                        }
                    }
                    else
                    {
                        if (teamPlayerObj0 != null && !isDead1)
                        {
                            damageValue2 = 0.7f;
                            damageValue0 = 0f;
                            damageValue1 = 0.3f;
                        }
                        else
                        {
                            damageValue2 = 1f;
                            damageValue0 = 0f;
                            damageValue1 = 0f;
                        }
                    }
                }
                else
                {
                    if (charactorObj != null && !isDead0)
                    {
                        if (teamPlayerObj0 != null && !isDead1)
                        {
                            damageValue2 = 0f;
                            damageValue0 = 0.7f;
                            damageValue1 = 0.3f;
                        }
                        else
                        {
                            damageValue2 = 0f;
                            damageValue0 = 1f;
                            damageValue1 = 0f;
                        }
                    }
                    else
                    {
                        if (teamPlayerObj0 != null && !isDead1)
                        {
                            damageValue2 = 0f;
                            damageValue0 = 0f;
                            damageValue1 = 1f;
                        }
                    }
                }
            }
            else
            {
                if (teamPlayerObj0 != null && !isDead1)
                {
                    if (teamPlayerObj1 != null && !isDead2)
                    {
                        if (charactorObj != null && !isDead0)
                        {
                            damageValue1 = 0.5f;
                            damageValue2 = 0.3f;
                            damageValue0 = 0.2f;
                        }
                        else
                        {
                            damageValue1 = 0.6f;
                            damageValue2 = 0.3f;
                            damageValue0 = 0f;
                        }
                    }
                    else
                    {
                        if (charactorObj != null && !isDead0)
                        {
                            damageValue1 = 0.7f;
                            damageValue2 = 0f;
                            damageValue0 = 0.3f;
                        }
                        else
                        {
                            damageValue1 = 1f;
                            damageValue2 = 0f;
                            damageValue0 = 0f;
                        }
                    }
                }
                else
                {
                    if (teamPlayerObj1 != null && !isDead2)
                    {
                        if (charactorObj != null && !isDead0)
                        {
                            damageValue1 = 0f;
                            damageValue2 = 0.7f;
                            damageValue0 = 0.3f;
                        }
                        else
                        {
                            damageValue1 = 0f;
                            damageValue2 = 1f;
                            damageValue0 = 0f;
                        }
                    }
                    else
                    {
                        if (charactorObj != null && !isDead0)
                        {
                            damageValue1 = 0f;
                            damageValue2 = 0f;
                            damageValue0 = 1f;
                        }
                    }
                }
            }
        }

        public void JsonToData()
        {
            TextAsset file = Resources.Load<TextAsset>("Datas/BatteleMap");
            if (file != null)
            {
                BatteleMapStrs = JsonMapper.ToObject<List<BatteleMapStr>>(file.text);

                BatteleMaps = new List<BatteleMap>();
                foreach (var batteleMapStr in BatteleMapStrs)
                {
                    BatteleMap batteleMap1 = new BatteleMap(batteleMapStr);
                    batteleMap1.mapName = LanguageManage.SwitchStr(batteleMap1.mapName);
                    batteleMap1.battleNotice = LanguageManage.SwitchStr(batteleMap1.battleNotice);
                    BatteleMaps.Add(batteleMap1);
                }
            }
            else
            {
                Debug.Log(file.name + "不存在");
            }


        }
        public void JsonToMonsterGropData()
        {
            TextAsset file = Resources.Load<TextAsset>("Datas/MonsterGrop");
            if (file != null)
            {
                MonsterGroups = JsonMapper.ToObject<List<MonsterGroup>>(file.text);
            }
            else
            {
                Debug.Log(file.name + "不存在");
            }


        }
        public void InitBattleData(BatteleMap _batteleMap, bool _isZero)
        {

            foreach (Transform child in mapParent)
            {
                Destroy(child.gameObject);
            }
            isMoving = false;
            isDisplayResult = false;
            isZero = _isZero;
            switchButton.interactable = false;
            switchValue = 1;
            timeScaleValue = 1;
            isEndMonster = false;
            monsterTime = 0;
            batteleMap = _batteleMap;
            mapPro = GameComponentData.gameData.gameManager.battleMapPros.Find(m => m.name == batteleMap.ObjName);
            _batteleMap.completeValue = 0;
            timeValue = 0;

            Camera.main.transform.position = fightCameraPos.position;

            map0 = Instantiate(mapPro);
            map0.transform.SetParent(mapParent, true);
            map0.transform.localPosition = Vector3.zero;
            Vector3 pos1 = new Vector3(9.6f, 0, 0);
            map1 = Instantiate(mapPro);
            map1.transform.SetParent(mapParent, true);
            map1.transform.localPosition = pos1;

            isDead0 = false;
            isDead1 = isDead2 = true;
            var x = mapParent.GetComponentsInChildren<Transform>();

            GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;

            playerProfessionData =
                CharactorDataAction.professionDatas.Find(p => p.id == gamePlayer.id / 1000);
            charactorPro = Resources.Load<GameObject>("charactor/" + gamePlayer.ObjName);

            charactorObj = Instantiate(charactorPro);
            charactorObj.transform.SetParent(mapParent, false);
            charactorObj.transform.localPosition = startPos0;


            charactorObj.GetComponentInChildren<Animator>().SetBool("IsWalk", true);
            charactorObj.GetComponentInChildren<Animator>().SetFloat("X", 1.0f);
            charactorObj.GetComponentInChildren<Animator>().SetFloat("Y", 0);

            foreach (Transform child in charactorObj.transform.GetChild(0))
            {
                child.gameObject.layer = LayerMask.NameToLayer("Fight");
                child.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
            charactorObj.transform.GetChild(0).GetChild(charactorObj.transform.GetChild(0).childCount - 1).GetComponent<SpriteRenderer>().sortingOrder = 0;


            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
            {
                teamPlayerProfessionData0 =
                    CharactorDataAction.professionDatas.Find(p => p.id == gamePlayer.TeamPlayer0.profession);
                GameObject TeamPlayerPro = Resources.Load<GameObject>("charactor/" + gamePlayer.TeamPlayer0.ObjName);

                teamPlayerObj0 = Instantiate(TeamPlayerPro);
                teamPlayerObj0.transform.SetParent(mapParent, false);
                teamPlayerObj0.transform.localPosition = startPos1;

                teamPlayerObj0.GetComponentInChildren<Animator>().SetBool("IsWalk", true);
                teamPlayerObj0.GetComponentInChildren<Animator>().SetFloat("X", 1.0f);
                teamPlayerObj0.GetComponentInChildren<Animator>().SetFloat("Y", 0);

                foreach (Transform child in teamPlayerObj0.transform.GetChild(0))
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Fight");
                    child.GetComponent<SpriteRenderer>().sortingOrder = 1;
                }
                teamPlayerObj0.transform.GetChild(0).GetChild(teamPlayerObj0.transform.GetChild(0).childCount - 1).GetComponent<SpriteRenderer>().sortingOrder = 0;
                isDead1 = false;
            }

            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
            {
                teamPlayerProfessionData1 =
                    CharactorDataAction.professionDatas.Find(p => p.id == gamePlayer.TeamPlayer1.profession);
                GameObject TeamPlayerPro = Resources.Load<GameObject>("charactor/" + gamePlayer.TeamPlayer1.ObjName);

                teamPlayerObj1 = Instantiate(TeamPlayerPro);
                teamPlayerObj1.transform.SetParent(mapParent, false);
                teamPlayerObj1.transform.localPosition = startPos2;

                teamPlayerObj1.GetComponentInChildren<Animator>().SetBool("IsWalk", true);
                teamPlayerObj1.GetComponentInChildren<Animator>().SetFloat("X", 1.0f);
                teamPlayerObj1.GetComponentInChildren<Animator>().SetFloat("Y", 0);
                teamPlayerObj1.transform.SetParent(mapParent, true);
                foreach (Transform child in teamPlayerObj1.transform.GetChild(0))
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Fight");
                    child.GetComponent<SpriteRenderer>().sortingOrder = 3;
                }
                teamPlayerObj1.transform.GetChild(0).GetChild(teamPlayerObj1.transform.GetChild(0).childCount - 1).GetComponent<SpriteRenderer>().sortingOrder = 0;
                isDead2 = false;
            }
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0 && !isDead1)
            {
                if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0 && !isDead2)
                {
                    damageValue0 = 0.5f;
                    damageValue1 = 0.3f;
                    damageValue2 = 0.2f;
                }
                else
                {
                    damageValue0 = 0.7f;
                    damageValue1 = 0.3f;
                    damageValue2 = 0f;
                }
            }
            else
            {
                if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0 && !isDead2)
                {
                    damageValue0 = 0.7f;
                    damageValue1 = 0;
                    damageValue2 = 0.3f;
                }
                else
                {
                    damageValue0 = 1f;
                    damageValue1 = 0;
                    damageValue2 = 0f;
                }
            }

            Resultitems = new List<Item>();
            gamePlayer.InitOldLevel();
            if (!isZero)
            {
                //Debug.Log("battle1!");
                StopAllCoroutines();
                StartCoroutine("MovingZero");
            }
            else
            {
                // Debug.Log("battle0!");
                charactorObj.transform.localPosition = startPos0;
                if (teamPlayerObj0 != null)
                {
                    teamPlayerObj0.transform.localPosition = startPos1;
                }
                if (teamPlayerObj1 != null)
                {
                    teamPlayerObj1.transform.localPosition = startPos2;
                }
                batteleMap.completeValue = 90;
                monsterTime = 90;
                CreatMonster();
            }

        }
        IEnumerator MovingZero()
        {

            Vector3 pz0 = ZeroPos0;
            Vector3 ps0 = startPos0;
            pz0 += mapParent.position;
            ps0 += mapParent.position;
            Vector3 Pos0 = pz0;

            Vector3 pz1 = ZeroPos1;
            Vector3 ps1 = startPos1;
            pz1 += mapParent.position;
            ps1 += mapParent.position;
            Vector3 Pos1 = pz1;

            Vector3 pz2 = ZeroPos2;
            Vector3 ps2 = startPos2;
            pz2 += mapParent.position;
            ps2 += mapParent.position;
            Vector3 Pos2 = pz2;




            float zeroTimeValue = 0;
            while (true)
            {
                Pos0 = pz0 + (ps0 - pz0) * zeroTimeValue;
                Pos1 = pz1 + (ps1 - pz1) * zeroTimeValue;
                Pos2 = pz2 + (ps2 - pz2) * zeroTimeValue;
                if (charactorObj != null)
                {
                    charactorObj.transform.position = Pos0;
                }
                if (teamPlayerObj0 != null)
                {
                    teamPlayerObj0.transform.position = Pos1;
                }
                if (teamPlayerObj1 != null)
                {
                    teamPlayerObj1.transform.position = Pos2;
                }

                float speed = 0.12f / moveTime;
                zeroTimeValue += speed;
                if (zeroTimeValue >= 1)
                {
                    if (charactorObj != null)
                    {
                        charactorObj.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                    }
                    if (teamPlayerObj0 != null)
                    {
                        teamPlayerObj0.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                    }
                    if (teamPlayerObj1 != null)
                    {
                        teamPlayerObj1.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                    }

                    switchButton.interactable = true;
                    StopCoroutine("MovingZero");
                    StartCoroutine("Moving");


                }
                yield return new WaitForSeconds(0.02f);
            }

        }

        public void ClickAutoButton(bool _isAuto)
        {
            AudioController.instance.PlayAudio(SE.click);
            if (gamePlayer.property.Power <= 0)
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不支"), LanguageManage.SwitchStr("体力已降为0，无法继续前进！"));
            }
            else
            {
                isAuto = _isAuto;
                isMoving = _isAuto;


                if (charactorObj != null)
                {
                    charactorObj.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                }
                if (teamPlayerObj0 != null)
                {
                    teamPlayerObj0.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                }
                if (teamPlayerObj1 != null)
                {
                    teamPlayerObj1.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                }
            }


        }
        public void ClickStartButton(Button StartButton)
        {
            if (gamePlayer.property.Power <= 0)
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不支"), LanguageManage.SwitchStr("体力已降为0，无法继续前进！"));
            }
            else
            {
                isAuto = false;
                isMoving = !isMoving;
                if (charactorObj != null)
                {
                    charactorObj.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                }
                if (teamPlayerObj0 != null)
                {
                    teamPlayerObj0.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                }
                if (teamPlayerObj1 != null)
                {
                    teamPlayerObj1.GetComponent<Animator>().SetBool("IsWalk", isMoving);
                }
                if (isMoving)
                {
                    StartButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("暂停");
                }
                else
                {
                    StartButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("前进");
                }
            }

        }
        IEnumerator Moving()
        {
            Vector3 p0 = Vector3.zero;
            Vector3 p1 = new Vector3(-9.6f, 0, 0);
            Vector3 p3 = new Vector3(9.6f, 0, 0);
            Vector3 p4 = new Vector3(0, 0, 0);
            Vector3 Pos0 = p0;
            Vector3 Pos1 = p3;
            float cd = 0;
            while (true)
            {
                if (isMoving)
                {
                    Pos0 = p0 + (p1 - p0) * timeValue;
                    Pos1 = p3 + (p4 - p3) * timeValue;
                    map0.transform.localPosition = Pos0;
                    map1.transform.localPosition = Pos1;


                    float speed = 0.02f / moveTime;
                    timeValue += speed;

                    if (timeValue >= 1)
                    {
                        GameObject obj = map0;
                        obj.transform.position = new Vector3(9.6f, 0, 0);
                        map0 = map1;
                        map1 = obj;
                        timeValue = 0;
                    }

                    cd += 0.02f;

                    if (cd >= batteleMap.monterCd)
                    {
                        if (gamePlayer.property.Power > 0)
                        {
                            gamePlayer.property.Power -= 5;
                            if (gamePlayer.property.Power <= 0)
                            {
                                gamePlayer.property.Power = 0;
                            }
                            GameComponentData.gameData.gameManager.UpDataPlayer();
                            cd = 0;
                            int randomValue = Random.Range(0, 10000);
                            if (randomValue >= batteleMap.completeValue)
                            {
                                CreatMonster();
                            }
                        }
                        else
                        {
                            isAuto = false;
                            gamePlayer.property.Power = 0;
                            isMoving = false;
                            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不支"), LanguageManage.SwitchStr("体力已降为0，无法继续前进！"));
                        }

                    }
                }

                yield return new WaitForSeconds(0.02f);
            }

        }

        public void SetMoving(bool _ismoving)
        {
            isMoving = _ismoving;
            if (charactorObj != null)
            {
                charactorObj.GetComponent<Animator>().SetBool("IsWalk", isMoving);
            }
            if (teamPlayerObj0 != null)
            {
                teamPlayerObj0.GetComponent<Animator>().SetBool("IsWalk", isMoving);
            }
            if (teamPlayerObj1 != null)
            {
                teamPlayerObj1.GetComponent<Animator>().SetBool("IsWalk", isMoving);
            }
        }

        public float GetAttritubeEffectValue(AttributeType attributeType0, AttributeType attributeType1)
        {
            switch (attributeType0)
            {
                case AttributeType.无:
                    return 0;
                case AttributeType.光:
                    if (attributeType1 == AttributeType.暗)
                    {
                        return 0.5f;
                    }
                    else if (attributeType1 == AttributeType.无)
                    {
                        return 0.2f;
                    }
                    else
                    {
                        return 0;
                    }
                case AttributeType.暗:
                    if (attributeType1 == AttributeType.风)
                    {
                        return 0.5f;
                    }
                    else if (attributeType1 == AttributeType.无)
                    {
                        return 0.2f;
                    }
                    else
                    {
                        return 0;
                    }
                case AttributeType.风:
                    if (attributeType1 == AttributeType.冰)
                    {
                        return 0.5f;
                    }
                    else if (attributeType1 == AttributeType.无)
                    {
                        return 0.2f;
                    }
                    else
                    {
                        return 0;
                    }
                case AttributeType.冰:
                    if (attributeType1 == AttributeType.火)
                    {
                        return 0.5f;
                    }
                    else if (attributeType1 == AttributeType.无)
                    {
                        return 0.2f;
                    }
                    else
                    {
                        return 0;
                    }
                case AttributeType.火:
                    if (attributeType1 == AttributeType.光)
                    {
                        return 0.5f;
                    }
                    else if (attributeType1 == AttributeType.无)
                    {
                        return 0.2f;
                    }
                    else
                    {
                        return 0;
                    }
            }
            return 0;
        }

        public void CreatMonster()
        {
            isMosterHurtEnd0 = true;
            isMosterHurtEnd1 = true;
            isMosterHurtEnd2 = true;
            GameComponentData.gameData.fightPanelAction.SetPlayerSkillValue(SkillValue / 100.0f);
            timeScaleValue = 1;
            AudioController.instance.PlayAudio(BGM.Battle);
            isMoving = false;
            isFightContineu = false;
            isPlayerBattle = false;
            rewardEXP = 0;
            rewardItems = new List<Item>();
            flyItems = new List<Item>();
            if (charactorObj != null)
            {
                charactorObj.GetComponent<Animator>().SetBool("IsWalk", isMoving);
            }
            if (teamPlayerObj0 != null)
            {
                teamPlayerObj0.GetComponent<Animator>().SetBool("IsWalk", isMoving);
            }
            if (teamPlayerObj1 != null)
            {
                teamPlayerObj1.GetComponent<Animator>().SetBool("IsWalk", isMoving);
            }
            if (monsterTime >= 80)
            {
                isEndMonster = true;
            }
            var Mapmonsters = batteleMap.mapMonsters.FindAll(m => m.timeValue <= monsterTime);

            if (Mapmonsters.Count > 0)
            {
                MapMonster mapMonster = Mapmonsters[Mapmonsters.Count - 1];
                int all = 0;
                int index = 0;
                List<int> randomValues = new List<int>();
                int randomValue = 0;
                foreach (var battleMnonster in mapMonster.BattleMnonsters)
                {
                    all += battleMnonster.weight;
                    randomValues.Add(all);
                }
                int x = Random.Range(0, all);
                for (int i = 0; i < randomValues.Count; i++)
                {
                    if (randomValues[i] >= x)
                    {
                        index = i;
                        break;
                    }
                }

                BattleMnonster _battleMnonster = mapMonster.BattleMnonsters[index];
                if (_battleMnonster.monsterId0 != 0)
                {
                    MonsterData selectMonsterData = GameComponentData.gameData.monsterManager.MonsterDatas.Find(m => m.id == _battleMnonster.monsterId0);
                    int value = Random.Range(0, 10000);
                    if (value < _battleMnonster.probability0)
                    {
                        monster0 = new Monster(selectMonsterData, MonsterPos0.position, _battleMnonster.attributeType0);
                    }
                    else
                    {
                        monster0 = new Monster(selectMonsterData, MonsterPos0.position, AttributeType.无);
                    }

                    monsterProfessionData0 =
                        CharactorDataAction.professionDatas.Find(p => p.id == selectMonsterData.profession);
                    isMosterHurtEnd0 = false;
                    monster0.SetSpriteLayerOrder(0);
                    GameObject monsterCanvas = Instantiate(charactorCanvas, monster0.Obj.transform.GetChild(1));
                    monsterCanvas.transform.SetParent(monster0.Obj.transform.GetChild(1), false);
                    monsterCanvas.GetComponent<BattelcharactorCavasAction>().Inite(monster0.skill != null);
                    if (monster0.attributeMask != null)
                    {
                        Transform slider = monster0.Obj.GetComponentInChildren<Slider>().transform;
                        Vector3 pos0 = monster0.attributeMask.transform.position;
                        monster0.attributeMask.transform.position = new Vector3(pos0.x, slider.position.y - 0.63f, pos0.z);
                    }

                }
                if (_battleMnonster.monsterId1 != 0)
                {
                    MonsterData selectMonsterData = GameComponentData.gameData.monsterManager.MonsterDatas.Find(m => m.id == _battleMnonster.monsterId1);
                    int value = Random.Range(0, 10000);
                    if (value < _battleMnonster.probability1)
                    {
                        monster1 = new Monster(selectMonsterData, MonsterPos1.position, _battleMnonster.attributeType1);
                    }
                    else
                    {
                        monster1 = new Monster(selectMonsterData, MonsterPos1.position, AttributeType.无);
                    }
                    monsterProfessionData1 =
                        CharactorDataAction.professionDatas.Find(p => p.id == selectMonsterData.profession);
                    isMosterHurtEnd1 = false;
                    monster1.SetSpriteLayerOrder(4);
                    GameObject monsterCanvas = Instantiate(charactorCanvas, monster1.Obj.transform.GetChild(1));
                    monsterCanvas.transform.SetParent(monster1.Obj.transform.GetChild(1), false);
                    monsterCanvas.GetComponent<BattelcharactorCavasAction>().Inite(monster1.skill != null);

                    if (monster1.attributeMask != null)
                    {
                        Transform slider = monster1.Obj.GetComponentInChildren<Slider>().transform;
                        Vector3 pos0 = monster1.attributeMask.transform.position;
                        monster1.attributeMask.transform.position = new Vector3(pos0.x, slider.position.y - 0.63f, pos0.z);
                    }
                }
                if (_battleMnonster.monsterId2 != 0)
                {
                    MonsterData selectMonsterData = GameComponentData.gameData.monsterManager.MonsterDatas.Find(m => m.id == _battleMnonster.monsterId2);
                    int value = Random.Range(0, 10000);
                    if (value < _battleMnonster.probability2)
                    {
                        monster2 = new Monster(selectMonsterData, MonsterPos2.position, _battleMnonster.attributeType2);
                    }
                    else
                    {
                        monster2 = new Monster(selectMonsterData, MonsterPos2.position, AttributeType.无);
                    }
                    monsterProfessionData2 =
                        CharactorDataAction.professionDatas.Find(p => p.id == selectMonsterData.profession);
                    isMosterHurtEnd2 = false;
                    monster2.SetSpriteLayerOrder(2);
                    GameObject monsterCanvas = Instantiate(charactorCanvas, monster2.Obj.transform.GetChild(1));
                    monsterCanvas.transform.SetParent(monster2.Obj.transform.GetChild(1), false);
                    monsterCanvas.GetComponent<BattelcharactorCavasAction>().Inite(monster2.skill != null);
                    if (monster2.attributeMask != null)
                    {
                        Transform slider = monster2.Obj.GetComponentInChildren<Slider>().transform;
                        Vector3 pos0 = monster2.attributeMask.transform.position;
                        monster2.attributeMask.transform.position = new Vector3(pos0.x, slider.position.y - 0.63f, pos0.z);
                    }
                }
                GameComponentData.gameData.fightPanelAction.BattleFunctionButton();
                monsterTime += 17;
                if (isAuto)
                {

                    if (SkillValue >= 100)
                    {
                        if (!isDead0 && gamePlayer.SkillId != 0)
                        {
                            SkillValue = 0;
                            GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.SkillId, gamePlayer.property.AT), true, gamePlayer.property.AT, charactorObj.transform.position);
                        }
                        else if (!isDead1 && gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.skillId != 0)
                        {
                            SkillValue = 0;
                            GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.TeamPlayer0.skillId, gamePlayer.TeamPlayer0.property.AT), true,
                                gamePlayer.TeamPlayer0.property.AT, teamPlayerObj0.transform.position);
                        }
                        else if (!isDead2 && gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer1.skillId != 0)
                        {
                            SkillValue = 0;
                            GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.TeamPlayer1.skillId, gamePlayer.TeamPlayer1.property.AT), true,
                                gamePlayer.TeamPlayer1.property.AT, teamPlayerObj1.transform.position);
                        }
                        else
                        {
                            PlayerBattle();
                        }


                    }
                    else
                    {
                        PlayerBattle();
                    }
                }
                else
                {
                    GameComponentData.gameData.fightPanelAction.FightDisplayFunctions();
                }
            }

        }
        public void GetOutBattled()
        {
            StopAllCoroutines();
            if (charactorObj != null)
            {
                Destroy(charactorObj);
            }
            if (teamPlayerObj0 != null)
            {
                Destroy(teamPlayerObj0);
            }
            if (teamPlayerObj1 != null)
            {
                Destroy(teamPlayerObj1);
            }
            if (monster0 != null)
            {
                Destroy(monster0.Obj);
                monster0 = null;
            }
            if (monster1 != null)
            {
                Destroy(monster1.Obj);
                monster1 = null;
            }
            if (monster2 != null)
            {
                Destroy(monster2.Obj);
                monster2 = null;
            }


            if (isDead0)
            {
                gamePlayer.property.HP = 1;
                //gamePlayer.property.Power = 1;
            }

            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
            {
                NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id);
                if (npcx != null)
                {
                    if (isDead1)
                    {
                        GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                        npcx.npcData.npcStatus = NpcStatus.修养中;
                        npcx.WaitDays = 2;
                        InformationController.instance.AddInformation("*NPC" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                        gamePlayer.TeamPlayer0 = null;
                        GameComponentData.gameData.gameManager.hurtNpc = npcx.npcData;
                    }

                }
                else if (gamePlayer.TeamPlayer0.id / 1000000 == 2)
                {
                    if (isDead1)
                    {
                        Pasture _pasture = null;
                        foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                        {
                            if (pasture.Animals != null)
                            {
                                if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer0.id))
                                {
                                    _pasture = pasture;
                                    break;
                                }
                            }
                        }
                        Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer0.id);
                        if (animal != null)
                        {
                            Destroy(animal.Obj);
                            GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                            _pasture.RemoveAnimal(animal.id);
                            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));
                        }
                        gamePlayer.TeamPlayer0 = null;
                    }
                }
            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
            {
                NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id);
                if (npcx != null)
                {
                    if (isDead2)
                    {
                        GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                        npcx.npcData.npcStatus = NpcStatus.修养中;
                        npcx.WaitDays = 2;
                        InformationController.instance.AddInformation("*" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                        gamePlayer.TeamPlayer1 = null;
                    }
                }
                else if (gamePlayer.TeamPlayer1.id / 1000000 == 2)
                {
                    if (isDead2)
                    {
                        Pasture _pasture = null;
                        foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                        {
                            if (pasture.Animals != null)
                            {
                                if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer1.id))
                                {
                                    _pasture = pasture;
                                    break;
                                }
                            }
                        }
                        Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer1.id);
                        if (animal != null)
                        {
                            Destroy(animal.Obj);
                            GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                            _pasture.RemoveAnimal(animal.id);
                            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));
                        }
                        gamePlayer.TeamPlayer1 = null;
                    }


                }

            }
            Destroy(map0);
            Destroy(map1);
            GameComponentData.gameData.mapParent.gameObject.SetActive(true);
            batteleMap = null;
            GameComponentData.gameData.gameManager.UpDataPlayer();
            Camera.main.transform.position = new Vector3(0, 0, -20);
            gamePlayer.attributeType = AttributeType.无;
            gamePlayer.SkillId = 0;
            GameComponentData.gameData.gameManager.InitGate();
            //AudioManager.PlayBGM(PlayType.CYCLE,GameComponentData.gameData.passDataManager.NowPassData.FriendBGM);
        }

        public void FightEnd(GameObject obj)
        {
            if (charactorObj != null)
            {
                if (obj == charactorObj)
                {
                    fightEnd0 = true;
                }
            }

            if (teamPlayerObj0 != null)
            {
                if (obj == teamPlayerObj0)
                {
                    fightEnd1 = true;
                }
            }

            if (teamPlayerObj1 != null)
            {
                if (obj == teamPlayerObj1)
                {
                    fightEnd2 = true;
                }
            }

            if (monster0 != null && !monster0.isDead)
            {
                if (obj == monster0.Obj)
                {
                    monsterEnd0 = true;
                }
            }
            else
            {
                monsterEnd0 = true;
            }
            if (monster1 != null && !monster1.isDead)
            {
                if (obj == monster1.Obj)
                {
                    monsterEnd1 = true;
                }

            }
            else
            {
                monsterEnd1 = true;
            }
            if (monster2 != null && !monster2.isDead)
            {
                if (obj == monster2.Obj)
                {
                    monsterEnd2 = true;
                }
            }
            else
            {
                monsterEnd2 = true;
            }

        }



        public void Hurt(GameObject obj)
        {
            if ((monster0 != null && !monster0.isDead) || (monster1 != null && !monster1.isDead) || (monster2 != null && !monster2.isDead))
            {
                Monster monster = null;
                if (monster0 != null && obj == monster0.Obj)
                {
                    monster = monster0;
                    monsterEnd0 = true;
                }
                if (monster1 != null && obj == monster1.Obj)
                {
                    monster = monster1;
                    monsterEnd1 = true;
                }
                if (monster2 != null && obj == monster2.Obj)
                {
                    monster = monster2;
                    monsterEnd2 = true;
                }
                if (monster != null)
                {
                    if (monster0 == null || monster0.isDead)
                    {
                        monsterEnd0 = true;
                    }
                    if (monster1 == null || monster1.isDead)
                    {
                        monsterEnd1 = true;
                    }
                    if (monster2 == null || monster2.isDead)
                    {
                        monsterEnd2 = true;
                    }

                    GameObject Obj = GameComponent.Effects.Find(e => e.name == monster.monsterData.hitName);

                    if (!isDead0 && charactorObj != null)
                    {
                        bool isDodge = false;
                        float dodgeValue = (gamePlayer.property.Dodge - monster.property.Dodge) / 100.0f;
                        float radomValue = Random.Range(0, 1.0f);
                        if (dodgeValue < 0)
                        {
                            dodgeValue = 0;
                        }
                        else if (dodgeValue > 1)
                        {
                            dodgeValue = 1;
                        }
                        if (radomValue <= dodgeValue)
                        {
                            isDodge = true;
                        }
                        if (charactorObj.GetComponentInChildren<Animator>().GetBool("IsHurt"))
                        {
                            if (monster == monster0)
                            {
                                monsterEnd0 = true;
                            }
                            else if (monster == monster1)
                            {
                                monsterEnd1 = true;
                            }
                            else
                            {
                                monsterEnd2 = true;
                            }
                        }
                        else
                        {
                            charactorObj.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                        }
                        if (isDodge)
                        {

                            DisPlayHurt(charactorObj, 0, false, missSe);
                        }
                        else
                        {
                            bool isCrit = false;
                            float attackValue = 1;
                            float critValue = (monster.property.Crit - gamePlayer.property.Crit) / 100.0f;
                            float radomValue1 = Random.Range(0, 1.0f);
                            if (critValue < 0)
                            {
                                critValue = 0;
                            }
                            else if (critValue > 1)
                            {
                                critValue = 1;
                            }
                            if (radomValue1 <= critValue)
                            {
                                isCrit = true;
                                attackValue = Random.Range(1.4f, 1.8f);
                            }


                            int hurtValue = Mathf.RoundToInt(GameManager.HurtValue(gamePlayer.property, monster.property) * attackValue * timeScaleValue);
                            hurtValue = Mathf.RoundToInt(hurtValue * damageValue0);

                            GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(0, -hurtValue);

                            Instantiate(Obj, charactorObj.transform.position, Quaternion.identity);
                            DisPlayHurt(charactorObj, -hurtValue, isCrit, monster.monsterData.hitSe);
                        }

                    }
                    if (!isDead1 && teamPlayerObj0 != null)
                    {
                        bool isDodge = false;
                        float dodgeValue = (gamePlayer.TeamPlayer0.property.Dodge - monster.property.Dodge) / 100.0f;
                        float radomValue = Random.Range(0, 1.0f);
                        if (dodgeValue < 0)
                        {
                            dodgeValue = 0;
                        }
                        else if (dodgeValue > 1)
                        {
                            dodgeValue = 1;
                        }
                        if (radomValue <= dodgeValue)
                        {
                            isDodge = true;
                        }
                        if (teamPlayerObj0.GetComponentInChildren<Animator>().GetBool("IsHurt"))
                        {
                            if (monster == monster0)
                            {
                                monsterEnd0 = true;
                            }
                            else if (monster == monster1)
                            {
                                monsterEnd1 = true;
                            }
                            else
                            {
                                monsterEnd2 = true;
                            }
                        }
                        else
                        {
                            teamPlayerObj0.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                        }
                        if (isDodge)
                        {
                            DisPlayHurt(TeamPlayer0, 0, false, missSe);
                        }
                        else
                        {
                            bool isCrit = false;
                            float attackValue = 1;
                            float critValue = (monster.property.Crit - gamePlayer.TeamPlayer0.property.Crit) / 100.0f;
                            float radomValue1 = Random.Range(0, 1.0f);
                            if (critValue < 0)
                            {
                                critValue = 0;
                            }
                            else if (critValue > 1)
                            {
                                critValue = 1;
                            }
                            if (radomValue1 <= critValue)
                            {
                                isCrit = true;
                                attackValue = Random.Range(1.4f, 1.8f);
                            }

                            int hurtValue = Mathf.RoundToInt(GameManager.HurtValue(gamePlayer.TeamPlayer0.property, monster.property) * attackValue * timeScaleValue);
                            hurtValue = Mathf.RoundToInt(hurtValue * damageValue1);

                            GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(1, -hurtValue);

                            Instantiate(Obj, teamPlayerObj0.transform.position, Quaternion.identity);
                            DisPlayHurt(teamPlayerObj0, -hurtValue, isCrit, monster.monsterData.hitSe);
                        }

                    }
                    if ((!isDead2 && teamPlayerObj1 != null))
                    {
                        bool isDodge = false;
                        float dodgeValue = (gamePlayer.TeamPlayer1.property.Dodge - monster.property.Dodge) / 100.0f;
                        float radomValue = Random.Range(0, 1.0f);
                        if (dodgeValue < 0)
                        {
                            dodgeValue = 0;
                        }
                        else if (dodgeValue > 1)
                        {
                            dodgeValue = 1;
                        }
                        if (radomValue <= dodgeValue)
                        {
                            isDodge = true;
                        }
                        if (teamPlayerObj1.GetComponentInChildren<Animator>().GetBool("IsHurt"))
                        {
                            if (monster == monster0)
                            {
                                monsterEnd0 = true;
                            }
                            else if (monster == monster1)
                            {
                                monsterEnd1 = true;
                            }
                            else
                            {
                                monsterEnd2 = true;
                            }
                        }
                        else
                        {
                            teamPlayerObj1.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                        }
                        if (isDodge)
                        {
                            DisPlayHurt(TeamPlayer1, 0, false, missSe);
                        }
                        else
                        {
                            bool isCrit = false;
                            float attackValue = 1;
                            float critValue = (monster.property.Crit - gamePlayer.TeamPlayer1.property.Crit) / 100.0f;
                            float radomValue1 = Random.Range(0, 1.0f);
                            if (critValue < 0)
                            {
                                critValue = 0;
                            }
                            else if (critValue > 1)
                            {
                                critValue = 1;
                            }
                            if (radomValue1 <= critValue)
                            {
                                isCrit = true;
                                attackValue = Random.Range(1.4f, 1.8f);
                            }

                            int hurtValue = Mathf.RoundToInt(GameManager.HurtValue(gamePlayer.TeamPlayer1.property, monster.property) * attackValue * timeScaleValue);
                            hurtValue = Mathf.RoundToInt(hurtValue * damageValue2);
                            GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(2, -hurtValue);
                            Instantiate(Obj, teamPlayerObj1.transform.position, Quaternion.identity);
                            DisPlayHurt(teamPlayerObj1, -hurtValue, isCrit, monster.monsterData.hitSe);
                        }
                    }
                    if (charactorObj != null && gamePlayer.property.HP <= 0)
                    {
                        isDead0 = true;
                        isPlayerHurtEnd0 = true;
                        fightEnd0 = true;
                        GameComponentData.gameData.fightPanelAction.playerType0.gameObject.SetActive(true);
                        GameComponentData.gameData.fightPanelAction.attribute0.gameObject.SetActive(false);
                        charactorObj.GetComponentInChildren<Animator>().SetBool("IsDead", true);
                    }
                    if (teamPlayerObj0 != null && gamePlayer.TeamPlayer0.property.HP <= 0)
                    {
                        isDead1 = true;
                        isPlayerHurtEnd1 = true;
                        fightEnd1 = true;
                        GameComponentData.gameData.fightPanelAction.playerType1.gameObject.SetActive(true);
                        GameComponentData.gameData.fightPanelAction.attribute1.gameObject.SetActive(false);
                        teamPlayerObj0.GetComponentInChildren<Animator>().SetBool("IsDead", true);
                    }
                    if (teamPlayerObj1 != null && gamePlayer.TeamPlayer1.property.HP <= 0)
                    {
                        isDead2 = true;
                        isPlayerHurtEnd2 = true;
                        fightEnd2 = true;
                        GameComponentData.gameData.fightPanelAction.playerType2.gameObject.SetActive(true);
                        GameComponentData.gameData.fightPanelAction.attribute2.gameObject.SetActive(false);
                        teamPlayerObj1.GetComponentInChildren<Animator>().SetBool("IsDead", true);
                    }
                }
                else
                {
                    float trueHurt0 = 1;
                    float trueHurt1 = 1;
                    float trueHurt2 = 1;
                    if (monster0 != null && !monster0.isDead)
                    {

                        if (monster1 != null && !monster1.isDead)
                        {
                            if (monster2 != null && !monster2.isDead)
                            {
                                trueHurt0 = 0.4f;
                            }
                            else
                            {
                                trueHurt0 = 0.5f;
                            }
                        }
                        else
                        {
                            if (monster2 != null && !monster2.isDead)
                            {
                                trueHurt0 = 0.6f;
                            }
                        }
                        MonsterHurtAction(monster0, obj, trueHurt0);
                    }
                    if (monster1 != null && !monster1.isDead)
                    {
                        if (monster0 != null && !monster0.isDead)
                        {
                            if (monster2 != null && !monster2.isDead)
                            {
                                trueHurt1 = 0.4f;
                            }
                            else
                            {
                                trueHurt1 = 0.5f;
                            }
                        }
                        else
                        {
                            if (monster2 != null && !monster2.isDead)
                            {
                                trueHurt1 = 0.6f;
                            }
                        }
                        MonsterHurtAction(monster1, obj, trueHurt1);
                    }
                    if (monster2 != null && !monster2.isDead)
                    {
                        if (monster0 != null && !monster0.isDead)
                        {
                            if (monster1 != null && !monster1.isDead)
                            {
                                trueHurt2 = 0.2f;
                            }
                            else
                            {
                                trueHurt2 = 0.4f;
                            }
                        }
                        else
                        {
                            if (monster1 != null && !monster1.isDead)
                            {
                                trueHurt2 = 0.4f;
                            }
                        }
                        MonsterHurtAction(monster2, obj, trueHurt2);
                    }

                }
            }

        }

        IEnumerator DisplaySkillAddHp()
        {
            yield return new WaitForSeconds(0.5f);
            DisPlayHurt(addObj, addHp, false, hitDefalutSe);
        }
        private void MonsterSkillHurt(Monster monster, Skill _skill, float hurtX)
        {

            float x = 1;
            switch (_skill.skillData.attributeType)
            {
                case AttributeType.冰:
                    if (monster.attributeType == AttributeType.火)
                    {
                        x = 1.5f;
                    }
                    else if (monster.attributeType == AttributeType.风)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.光:
                    if (monster.attributeType == AttributeType.暗)
                    {
                        x = 1.5f;
                    }
                    else if (monster.attributeType == AttributeType.火)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.暗:
                    if (monster.attributeType == AttributeType.风)
                    {
                        x = 1.5f;
                    }
                    else if (monster.attributeType == AttributeType.光)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.火:
                    if (monster.attributeType == AttributeType.光)
                    {
                        x = 1.5f;
                    }
                    else if (monster.attributeType == AttributeType.冰)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.风:
                    if (monster.attributeType == AttributeType.冰)
                    {
                        x = 1.5f;
                    }
                    else if (monster.attributeType == AttributeType.暗)
                    {
                        x = 0.5f;
                    }
                    break;
            }
            int hurtValue = _skill.attackValue - monster.property.DF;
            if (hurtValue <= 0)
            {
                hurtValue = 1;
            }
            hurtValue = (int)(hurtValue * x * timeScaleValue * hurtX);


            monster.Obj.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
            monster.property.HP -= hurtValue;
            if (monster.property.HP < 0)
            {
                monster.property.HP = 0;
            }
            monster.Obj.GetComponentInChildren<Slider>().value = monster.property.HP / (float)monster.property.MaxHP;

            if (monster.attributeType == _skill.skillData.attributeType)
            {
                monster.property.HP += (int)(hurtValue / 2.0f);
                if (monster.property.HP > monster.property.MaxHP)
                {
                    monster.property.HP = monster.property.MaxHP;
                }
                addObj = monster.Obj;
                addHp = (int)(hurtValue / 2.0f);
                StartCoroutine("DisplaySkillAddHp");
            }
            DisPlayHurt(monster.Obj, -hurtValue, false, hitDefalutSe);
            monster.skillValue += 10;
            if (monster.property.HP <= 0)
            {
                monster.isDead = true;
                if (monster == monster0)
                {
                    monsterEnd0 = true;
                    isMosterHurtEnd0 = true;

                }
                else if (monster == monster1)
                {
                    monsterEnd1 = true;
                    isMosterHurtEnd1 = true;
                }
                else
                {
                    monsterEnd2 = true;
                    isMosterHurtEnd2 = true;
                }
                moneyValue = Random.Range(monster.monsterData.rewardMoneyMin, monster.monsterData.rewardMoneyMax);
                rewardEXP += monster.property.rewardEXP;
                rewardItems.AddRange(monster.GetRewardItem());
                if (rewardItems.Count > 0)
                {
                    foreach (var rewardItem in rewardItems)
                    {
                        for (int i = 0; i < rewardItem.count; i++)
                        {
                            Item item = ItemManager.instance.CreatItem(rewardItem.dataId, 1);
                            flyItems.Add(item);
                        }
                    }

                }
                monster.property.HP = 0;
                monster.Obj.GetComponent<Animator>().SetBool("IsDead", true);
                if (!isDead0 && charactorObj != null && !isZero)
                {
                    GameComponentData.gameData.fightPanelAction.UpDataPlayerEXP(0, monster.property.rewardEXP);
                }
                if (!isDead1 && teamPlayerObj0 != null && !isZero)
                {
                    GameComponentData.gameData.fightPanelAction.UpDataPlayerEXP(1, monster.property.rewardEXP);
                }
                if (!isDead2 && teamPlayerObj1 != null && !isZero)
                {
                    GameComponentData.gameData.fightPanelAction.UpDataPlayerEXP(2, monster.property.rewardEXP);
                }
            }


        }
        private void PlayerSkillHurt(int playerIndex, Skill _skill)
        {

            isPlayerHurtEnd0 = true;
            isPlayerHurtEnd1 = true;
            isPlayerHurtEnd2 = true;

            float x = 1;
            AttributeType playerAttributeType = AttributeType.无;
            if (playerIndex == 0)
            {
                playerAttributeType = gamePlayer.attributeType;
            }
            else if (playerIndex == 1)
            {
                playerAttributeType = gamePlayer.TeamPlayer0.attributeType;
            }
            else
            {
                playerAttributeType = gamePlayer.TeamPlayer1.attributeType;
            }

            switch (_skill.skillData.attributeType)
            {
                case AttributeType.冰:
                    if (playerAttributeType == AttributeType.火)
                    {
                        x = 1.5f;
                    }
                    else if (playerAttributeType == AttributeType.风)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.光:
                    if (playerAttributeType == AttributeType.暗)
                    {
                        x = 1.5f;
                    }
                    else if (playerAttributeType == AttributeType.火)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.暗:
                    if (playerAttributeType == AttributeType.风)
                    {
                        x = 1.5f;
                    }
                    else if (playerAttributeType == AttributeType.光)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.火:
                    if (playerAttributeType == AttributeType.光)
                    {
                        x = 1.5f;
                    }
                    else if (playerAttributeType == AttributeType.冰)
                    {
                        x = 0.5f;
                    }
                    break;
                case AttributeType.风:
                    if (playerAttributeType == AttributeType.冰)
                    {
                        x = 1.5f;
                    }
                    else if (playerAttributeType == AttributeType.暗)
                    {
                        x = 0.5f;
                    }
                    break;
            }
            InitAttackOrder();
            if (playerIndex == 0)
            {
                if (!isDead0 && charactorObj != null)
                {
                    int hurtValue = _skill.attackValue - gamePlayer.property.DF;
                    if (hurtValue <= 0)
                    {
                        hurtValue = 1;
                    }
                    hurtValue = (int)(hurtValue * timeScaleValue * damageValue0);
                    charactorObj.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                    GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(0, -hurtValue);
                    DisPlayHurt(charactorObj, -hurtValue, false, hitDefalutSe);
                    if (gamePlayer.attributeType == _skill.skillData.attributeType)
                    {
                        GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(0, hurtValue / 2);
                        addObj = charactorObj;
                        addHp = hurtValue / 2;
                        StartCoroutine("DisplaySkillAddHp");
                    }

                }
                if (charactorObj != null && gamePlayer.property.HP <= 0)
                {
                    isDead0 = true;
                    isPlayerHurtEnd0 = true;
                    fightEnd0 = true;
                    GameComponentData.gameData.fightPanelAction.playerType0.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.attribute0.gameObject.SetActive(false);
                    charactorObj.GetComponentInChildren<Animator>().SetBool("IsDead", true);
                }
            }
            else if (playerIndex == 1)
            {
                if (!isDead1 && teamPlayerObj0 != null)
                {
                    int hurtValue = _skill.attackValue - gamePlayer.TeamPlayer0.property.DF;
                    if (hurtValue <= 0)
                    {
                        hurtValue = 1;
                    }
                    hurtValue = (int)(hurtValue * timeScaleValue * damageValue1);
                    teamPlayerObj0.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                    GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(1, -hurtValue);
                    DisPlayHurt(teamPlayerObj0, -hurtValue, false, hitDefalutSe);
                    if (gamePlayer.TeamPlayer0.attributeType == _skill.skillData.attributeType)
                    {
                        GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(1, hurtValue / 2);
                        addObj = teamPlayerObj0;
                        addHp = hurtValue / 2;
                        StartCoroutine("DisplaySkillAddHp");
                    }

                }
                if (teamPlayerObj0 != null && gamePlayer.TeamPlayer0.property.HP <= 0)
                {
                    isDead1 = true;
                    isPlayerHurtEnd1 = true;
                    fightEnd1 = true;
                    GameComponentData.gameData.fightPanelAction.playerType1.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.attribute1.gameObject.SetActive(false);
                    teamPlayerObj0.GetComponentInChildren<Animator>().SetBool("IsDead", true);
                }
            }
            else
            {
                if (!isDead2 && teamPlayerObj1 != null)
                {
                    int hurtValue = _skill.attackValue - gamePlayer.TeamPlayer1.property.DF;
                    if (hurtValue <= 0)
                    {
                        hurtValue = 1;
                    }
                    hurtValue = (int)(hurtValue * timeScaleValue * damageValue2);
                    teamPlayerObj1.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                    GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(2, -hurtValue);
                    DisPlayHurt(teamPlayerObj1, -hurtValue, false, hitDefalutSe);
                    if (gamePlayer.TeamPlayer1.attributeType == _skill.skillData.attributeType)
                    {
                        GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(2, hurtValue / 2);
                        addObj = teamPlayerObj1;
                        addHp = hurtValue / 2;
                        StartCoroutine("DisplaySkillAddHp");
                    }

                }
                if (teamPlayerObj1 != null && gamePlayer.TeamPlayer1.property.HP <= 0)
                {
                    isDead2 = true;
                    isPlayerHurtEnd2 = true;
                    fightEnd2 = true;
                    GameComponentData.gameData.fightPanelAction.playerType2.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.attribute2.gameObject.SetActive(false);
                    teamPlayerObj1.GetComponentInChildren<Animator>().SetBool("IsDead", true);
                }
            }





        }

        //怪物受伤逻辑
        private void MonsterHurtAction(Monster monster, GameObject attackObj, float trueHurt)
        {
            AttributeType attackAttributeType = AttributeType.无;
            Property property = new Property();
            ProfessionData professionData = playerProfessionData;

            if (monster.Obj != null)
            {
                if (attackObj == charactorObj)
                {
                    property = gamePlayer.property;
                    attackAttributeType = gamePlayer.attributeType;
                }
                if (attackObj == teamPlayerObj0)
                {
                    property = gamePlayer.TeamPlayer0.property;
                    professionData = teamPlayerProfessionData0;
                    attackAttributeType = gamePlayer.TeamPlayer0.attributeType;
                }
                if (attackObj == teamPlayerObj1)
                {
                    property = gamePlayer.TeamPlayer1.property;
                    professionData = teamPlayerProfessionData1;
                    attackAttributeType = gamePlayer.TeamPlayer1.attributeType;
                }
                if (monster.Obj.GetComponentInChildren<Animator>().GetBool("IsHurt"))
                {
                    if (attackObj == charactorObj)
                    {
                        fightEnd0 = true;
                    }
                    if (attackObj == teamPlayerObj0)
                    {
                        fightEnd1 = true;
                    }
                    if (attackObj == teamPlayerObj1)
                    {
                        fightEnd2 = true;
                    }
                }
                else
                {
                    monster.Obj.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                }


                bool isDodge = false;
                float dodgeValue = (monster.property.Dodge - property.Dodge) / 100.0f;
                float radomValue = Random.Range(0, 1.0f);
                if (dodgeValue < 0)
                {
                    dodgeValue = 0;
                }
                else if (dodgeValue > 1)
                {
                    dodgeValue = 1;
                }
                if (radomValue <= dodgeValue)
                {
                    isDodge = true;
                }
                if (isDodge)
                {
                    if (monster.Obj.GetComponentInChildren<Animator>().GetBool("IsHurt"))
                    {
                        HurtEnd(monster.Obj);
                    }
                    else
                    {
                        monster.Obj.GetComponentInChildren<Animator>().SetBool("IsHurt", true);
                    }
                    DisPlayHurt(monster.Obj, 0, false, missSe);
                }
                else
                {
                    bool isCrit = false;
                    float attackValue = 1;
                    float critValue = (property.Crit - monster.property.Crit) / 100.0f;
                    float radomValue1 = Random.Range(0, 1.0f);
                    if (critValue < 0)
                    {
                        critValue = 0;
                    }
                    else if (critValue > 1)
                    {
                        critValue = 1;
                    }
                    if (radomValue1 <= critValue)
                    {
                        isCrit = true;
                        attackValue = Random.Range(1.4f, 1.8f);
                    }

                    int hurtValue = Mathf.RoundToInt(trueHurt * GameManager.HurtValue(monster.property, property) * attackValue * timeScaleValue);

                    hurtValue = (int)(hurtValue * (1.0f + GetAttritubeEffectValue(attackAttributeType, monster.attributeType)));

                    monster.skillValue += 10;
                    monster.Obj.GetComponentInChildren<BattelcharactorCavasAction>().SetNuDisplay(monster.skillValue);
                    if (monster.maskHp > 0)
                    {
                        monster.maskHp -= hurtValue / 2;
                        monster.property.HP -= hurtValue / 2;
                        GameObject hurtPro0 = GameComponent.Effects.Find(e => e.name == professionData.hitEffectName);
                        Instantiate(hurtPro0, monster.attributeMask.transform.GetChild(1).position, Quaternion.identity);
                        monster.attributeMask.GetComponentInChildren<AttributeMaskAction>().SetMaskDisplay(monster.maskHp / (float)monster.property.MaxHP);
                    }
                    else
                    {
                        monster.property.HP -= hurtValue;
                    }

                    if (monster.property.HP < 0)
                    {
                        monster.property.HP = 0;
                    }
                    monster.Obj.GetComponentInChildren<Slider>().value = monster.property.HP / (float)monster.property.MaxHP;

                    GameObject hurtPro = GameComponent.Effects.Find(e => e.name == professionData.hitEffectName);
                    Instantiate(hurtPro, monster.Obj.transform.position, Quaternion.identity);
                    DisPlayHurt(monster.Obj, -hurtValue, isCrit, hitDefalutSe);
                }

                if (monster.property.HP <= 0)
                {
                    monster.isDead = true;

                    if (monster == monster0)
                    {
                        monsterEnd0 = true;
                        isMosterHurtEnd0 = true;

                    }
                    else if (monster == monster1)
                    {
                        monsterEnd1 = true;
                        isMosterHurtEnd1 = true;
                    }
                    else
                    {
                        monsterEnd2 = true;
                        isMosterHurtEnd2 = true;
                    }




                    moneyValue = Random.Range(monster.monsterData.rewardMoneyMin, monster.monsterData.rewardMoneyMax);
                    rewardEXP += monster.property.rewardEXP;
                    rewardItems.AddRange(monster.GetRewardItem());
                    if (rewardItems.Count > 0)
                    {
                        foreach (var rewardItem in rewardItems)
                        {
                            for (int i = 0; i < rewardItem.count; i++)
                            {
                                Item item = ItemManager.instance.CreatItem(rewardItem.dataId, 1);
                                flyItems.Add(item);
                            }
                        }

                    }

                    monster.property.HP = 0;
                    monster.Obj.GetComponent<Animator>().SetBool("IsDead", true);
                    if (!isDead0 && charactorObj != null && !isZero)
                    {
                        GameComponentData.gameData.fightPanelAction.UpDataPlayerEXP(0, monster.property.rewardEXP);
                    }
                    if (!isDead1 && teamPlayerObj0 != null && !isZero)
                    {
                        GameComponentData.gameData.fightPanelAction.UpDataPlayerEXP(1, monster.property.rewardEXP);
                    }
                    if (!isDead2 && teamPlayerObj1 != null && !isZero)
                    {
                        GameComponentData.gameData.fightPanelAction.UpDataPlayerEXP(2, monster.property.rewardEXP);
                    }
                }

            }
        }

        public List<int> GetPlayerSkillList()
        {
            List<int> skills = new List<int>();
            if (!isDead0 && gamePlayer.SkillId != 0)
            {
                skills.Add(gamePlayer.SkillId);
            }
            else
            {
                skills.Add(0);
            }
            if (!isDead1 && gamePlayer.TeamPlayer0.skillId != 0)
            {
                skills.Add(gamePlayer.TeamPlayer0.skillId);
            }
            else
            {
                skills.Add(0);
            }
            if (!isDead1 && gamePlayer.TeamPlayer1.skillId != 0)
            {
                skills.Add(gamePlayer.TeamPlayer1.skillId);
            }
            else
            {
                skills.Add(0);
            }
            return skills;
        }

        public void PlayerSkillAction(int index)
        {
            Skill skill = new Skill();
            if (index == 0)
            {
                GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.SkillId, gamePlayer.property.AT), true, gamePlayer.property.AT, charactorObj.transform.position);
            }
            else if (index == 1)
            {
                GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.TeamPlayer0.skillId, gamePlayer.TeamPlayer0.property.AT), true,
                    gamePlayer.TeamPlayer0.property.AT, teamPlayerObj0.transform.position);
            }
            else
            {
                GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.TeamPlayer1.skillId, gamePlayer.TeamPlayer1.property.AT), true,
                    gamePlayer.TeamPlayer1.property.AT, teamPlayerObj1.transform.position);
            }
            SkillValue = 0;
            isFightContineu = true;

        }
        public void PlayerDeadAction()
        {
            gamePlayer.property.Power /= 2;
            ResultObj.SetActive(true);
            GameComponentData.gameData.charactorTitleAction.AddfailureCount();
            ResultObj.GetComponentInChildren<AdventureResultAction>().InitData(false, Resultitems, gamePlayer);


            // FightInformationObj.SetActive(true);
            //FightInformationObj.GetComponent<FightInformationAction>().InitData("队伍全灭",InformationType.战斗失败);
            StopAllCoroutines();

        }

        public void InitBattleMaps()
        {
            foreach (var map in BatteleMaps)
            {
                if (map.allValue > 100)
                {
                    CheckBattleMapOpen(map);
                }
            }
        }
        IEnumerator Boxitem()
        {
            Vector3 pos = new Vector3((MonsterPos0.position.x + MonsterPos2.position.x) / 2, (MonsterPos0.position.y + MonsterPos1.position.y) / 2, MonsterPos0.position.z);
            boxObj = Instantiate(Boxpro, pos, Quaternion.identity);
            yield return new WaitForSeconds(1.2f);
            Startinitflyitem();

        }
        public void PlayerDead()
        {
            if (isDead0 && (teamPlayerObj0 == null || isDead1) && (teamPlayerObj1 == null || isDead2))
            {
                if (isZero)
                {
                    gamePlayer.TeamPlayer0 = null;
                    gamePlayer.TeamPlayer1 = null;

                    GameComponentData.gameData.mapParent.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.informationObj.transform.SetParent(GameComponentData.gameData.fightPanelAction.informaParent1, true);
                    GameComponentData.gameData.fightPanelAction.informationObj.transform.localPosition = Vector3.zero;
                    //GameComponentData.gameData.fightPanelAction.functionButtons.SetActive(true);


                    GameComponentData.gameData.fightPanelAction.gameObject.SetActive(false);
                    AudioController.instance.StopBgm();
                    Debug.Log("film");
                    //GameComponentData.gameData.zeroFilmController.FightEndAction();
                }
                else
                {
                    GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("全队死亡"), 100, LanguageManage.SwitchStr("是否复活全队？"), CostType.复活, ShopMoneyType.红晶);
                }

                // PlayerDeadAction();

            }
            else
            {
                //MonsterFight();
            }
        }

        private void UseItemSkill(int skillId)
        {
            GameObject obj = null;
            if (charactorObj != null)
            {
                GameComponentData.gameData.skillManager.SkillAction(new Skill(skillId, gamePlayer.property.AT), true, gamePlayer.property.AT, charactorObj.transform.position);
            }
            else if (teamPlayerObj0 != null)
            {
                GameComponentData.gameData.skillManager.SkillAction(new Skill(skillId, gamePlayer.TeamPlayer0.property.AT),
                    true, gamePlayer.TeamPlayer0.property.AT, teamPlayerObj0.transform.position);
            }
            else if (teamPlayerObj1 != null)
            {
                GameComponentData.gameData.skillManager.SkillAction(new Skill(skillId, gamePlayer.TeamPlayer1.property.AT),
                    true, gamePlayer.TeamPlayer1.property.AT, teamPlayerObj1.transform.position);
            }

        }

        IEnumerator AfterSkillMonsterAttack()
        {
            yield return new WaitForSeconds(1.5f);

        }
        //怪物死亡
        public async void MonsterDeadAction()
        {

            GameComponentData.gameData.charactorTitleAction.AddKillCount();
            if ((monster0 == null || monster0.isDead) && (monster1 == null || monster1.isDead) && (monster2 == null || monster2.isDead))
            {
                batteleMap.completeValue += 17;
                if (batteleMap.allValue < batteleMap.completeValue)
                {
                    batteleMap.allValue = batteleMap.completeValue;
                }
                if (isEndMonster)
                {
                    CheckBattleMapOpen(batteleMap);
                    StartCoroutine("Boxitem");
                }
                else
                {
                    Startinitflyitem();
                }

                if (rewardItems.Count > 0)
                {
                    gamePlayer.money += moneyValue;
                    bool isFull = false;
                    List<Item> rewardItem1 = new List<Item>();
                    foreach (var rewardItem in rewardItems)
                    {
                        int spare = await PackageManager.instance.SetItemInPackage(rewardItem, 0);
                        Item itemX = ItemManager.instance.CreatItem(rewardItem.dataId, rewardItem.count - spare);

                        rewardItem1.Add(itemX);
                        if (spare > 0)
                        {
                            isFull = true;
                            break;
                        }
                    }
                    foreach (var rewardItem in rewardItem1)
                    {
                        int index = Resultitems.FindIndex(i => i.dataId == rewardItem.dataId);

                        if (index >= 0)
                        {
                            var item = Resultitems[index];
                            item.count += rewardItem.count;
                            Resultitems[index] = item;
                        }
                        else
                        {
                            Resultitems.Add(rewardItem);
                        }
                    }
                    string rewardStr = LanguageManage.SwitchStr("*获得经验x") + rewardEXP;
                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*获得经验x")
                                                                                 + rewardEXP + LanguageManage.SwitchStr("*获得金币x") + moneyValue);
                    rewardStr += LanguageManage.SwitchStr(",获得金币x") + moneyValue;
                    foreach (var rewardItem in rewardItems)
                    {
                        ItemData itemData = await GameDataManager.instance.GetAsyncObjectData<ItemData>(rewardItem.dataId.ToString());
                        rewardStr += "," + itemData.name + "x" + rewardItem.count;

                        InformationController.instance.AddInformation(LanguageManage.SwitchStr("*获得")
                                                                                     + itemData.name + "x" + rewardItem.count);
                    }
                    if (isFull)
                    {
                        rewardStr += LanguageManage.SwitchStr(",由于背包已满，部分道具未获得！");
                        InformationController.instance.AddInformation(LanguageManage.SwitchStr("*由于背包已满，部分道具未获得！"));
                    }

                }
                else
                {
                    gamePlayer.money += moneyValue;
                    string rewardStr = LanguageManage.SwitchStr("*获得经验x") + rewardEXP;
                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*获得经验x") + rewardEXP);
                    rewardStr += LanguageManage.SwitchStr(",获得金币x") + moneyValue;
                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*获得金币x") + moneyValue);
                    if (!isAuto)
                    {
                        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("战斗胜利"), rewardStr);
                    }
                }
                GameComponentData.gameData.gameManager.PlayerMoneyText.text = gamePlayer.money.ToString();
            }
            GameComponentData.gameData.fightPanelAction.ExplorValueText.text = monsterTime.ToString();
        }
        public void ClickItemAfterBattle()
        {
            SetMoving(false);
            isFightContineu = false;
            GameComponentData.gameData.fightPanelAction.startButton.GetComponentInChildren<Text>().text =
                LanguageManage.SwitchStr("前进");
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.food);
            wareDisplayTypes.Add(WareDisplayType.药剂);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.AfterBattle);
        }
        public void ClickItemInBattling()
        {
            isFightContineu = true;
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.food);
            wareDisplayTypes.Add(WareDisplayType.药剂);
            wareDisplayTypes.Add(WareDisplayType.卷轴);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Battling);
        }

        public void DisPlayHurt(GameObject obj, int value, bool isCrit, string hitSe)
        {

            GameObject HpObj = Instantiate(hurtHpPro, obj.transform.position, Quaternion.identity);

            //AudioManager.PlaySE(PlayType.ONCE,hitSe);

            HpObj.GetComponentInChildren<Text>().text = value.ToString();
            if (value > 0)
            {
                HpObj.GetComponentInChildren<Text>().color = Color.green;
            }
            else
            {
                if (value == 0)
                {
                    HpObj.GetComponentInChildren<Text>().text = "miss";
                }
                if (isCrit)
                {
                    HpObj.GetComponentInChildren<Text>().color = Color.red;
                }
                else
                {
                    HpObj.GetComponentInChildren<Text>().color = Color.white;
                }
            }


        }
        public void HurtEnd(GameObject obj)
        {
            bool playerFight = false;
            bool monsterFight = false;
            if (charactorObj != null && obj == charactorObj)
            {
                isPlayerHurtEnd0 = true;
            }
            else if (teamPlayerObj0 != null && obj == teamPlayerObj0)
            {
                isPlayerHurtEnd1 = true;
            }
            else if (teamPlayerObj1 != null && obj == teamPlayerObj1)
            {
                isPlayerHurtEnd2 = true;
            }



            if (!isDead2)
            {
                playerFight = fightEnd2;
            }
            else if (!isDead1)
            {
                playerFight = fightEnd1;
            }
            else
            {
                playerFight = fightEnd0;
            }


            if (monster0 != null && obj == monster0.Obj)
            {
                isMosterHurtEnd0 = true;
            }
            if (monster1 != null && obj == monster1.Obj)
            {
                isMosterHurtEnd1 = true;
            }
            if (monster2 != null && obj == monster2.Obj)
            {
                isMosterHurtEnd2 = true;
            }

            if (monster2 != null && !monster2.isDead)
            {
                monsterFight = monsterEnd2;
            }
            else if (monster1 != null && !monster1.isDead)
            {
                monsterFight = monsterEnd1;
            }
            else if (monster0 != null && !monster0.isDead)
            {
                monsterFight = monsterEnd0;
            }
            if (playerFight)
            {
                fightEnd0 = fightEnd1 = fightEnd2 = false;
                if (isMosterHurtEnd0 && isMosterHurtEnd1 && isMosterHurtEnd2)
                {
                    if (monster0 != null)
                    {
                        isMosterHurtEnd0 = false;
                    }
                    if (monster1 != null)
                    {
                        isMosterHurtEnd1 = false;
                    }
                    if (monster2 != null)
                    {
                        isMosterHurtEnd2 = false;
                    }


                    if (!isDead0 || !isDead1 || !isDead2)
                    {

                        if ((monster0 != null && !monster0.isDead) || (monster1 != null && !monster1.isDead) ||
                            (monster2 != null && !monster2.isDead))
                        {
                            MonsterFight();
                        }

                    }
                    else
                    {
                        PlayerDead();
                    }

                }
            }
            if (monsterFight)
            {
                if (isPlayerHurtEnd0 && isPlayerHurtEnd1 && isPlayerHurtEnd2)
                {
                    monsterEnd0 = false;
                    monsterEnd1 = false;
                    monsterEnd2 = false;
                    timeScaleValue *= 1.35f;
                    SkillValue += 10;
                    if (SkillValue >= 100)
                    {
                        SkillValue = 100;

                    }
                    GameComponentData.gameData.fightPanelAction.SetPlayerSkillValue(SkillValue / 100.0f);
                    isPlayerBattle = false;
                    if (isAuto)
                    {

                        if (SkillValue >= 100)
                        {
                            if (!isDead0 && gamePlayer.SkillId != 0)
                            {
                                SkillValue = 0;
                                GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.SkillId, gamePlayer.property.AT), true, gamePlayer.property.AT, charactorObj.transform.position);
                            }
                            else if (!isDead1 && gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.skillId != 0)
                            {
                                SkillValue = 0;
                                GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.TeamPlayer0.skillId, gamePlayer.TeamPlayer0.property.AT), true,
                                    gamePlayer.TeamPlayer0.property.AT, teamPlayerObj0.transform.position);
                            }
                            else if (!isDead2 && gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer1.skillId != 0)
                            {
                                SkillValue = 0;
                                GameComponentData.gameData.skillManager.SkillAction(new Skill(gamePlayer.TeamPlayer1.skillId, gamePlayer.TeamPlayer1.property.AT), true,
                                    gamePlayer.TeamPlayer1.property.AT, teamPlayerObj1.transform.position);
                            }
                            else
                            {
                                PlayerBattle();
                            }


                        }
                        else
                        {
                            PlayerBattle();
                        }

                    }
                    else
                    {
                        GameComponentData.gameData.fightPanelAction.FightDisplayFunctions();
                    }
                }
            }


        }

        public void AfterSkillEffect(Skill _skill, bool isPlayer)
        {
            if (isPlayer)
            {
                isPlayerBattle = true;
                fightEnd0 = true;
                fightEnd1 = true;
                fightEnd2 = true;

                GameComponentData.gameData.fightPanelAction.SetPlayerSkillValue(SkillValue / 100.0f);
                float trueHurt0 = 1;
                float trueHurt1 = 1;
                float trueHurt2 = 1;
                if (monster0 != null && !monster0.isDead)
                {

                    if (monster1 != null && !monster1.isDead)
                    {
                        if (monster2 != null && !monster2.isDead)
                        {
                            trueHurt0 = 0.4f;
                        }
                        else
                        {
                            trueHurt0 = 0.5f;
                        }
                    }
                    else
                    {
                        if (monster2 != null && !monster2.isDead)
                        {
                            trueHurt0 = 0.6f;
                        }
                    }
                    MonsterSkillHurt(monster0, _skill, trueHurt0);
                }
                if (monster1 != null && !monster1.isDead)
                {
                    if (monster0 != null && !monster0.isDead)
                    {
                        if (monster2 != null && !monster2.isDead)
                        {
                            trueHurt1 = 0.4f;
                        }
                        else
                        {
                            trueHurt1 = 0.5f;
                        }
                    }
                    else
                    {
                        if (monster2 != null && !monster2.isDead)
                        {
                            trueHurt1 = 0.6f;
                        }
                    }
                    MonsterSkillHurt(monster1, _skill, trueHurt1);
                }
                if (monster2 != null && !monster2.isDead)
                {
                    if (monster0 != null && !monster0.isDead)
                    {
                        if (monster1 != null && !monster1.isDead)
                        {
                            trueHurt2 = 0.2f;
                        }
                        else
                        {
                            trueHurt2 = 0.4f;
                        }
                    }
                    else
                    {
                        if (monster1 != null && !monster1.isDead)
                        {
                            trueHurt2 = 0.4f;
                        }
                    }
                    MonsterSkillHurt(monster2, _skill, trueHurt2);
                }
                timeScaleValue *= 1.35f;
            }
            else
            {
                fightEnd0 = fightEnd1 = fightEnd2 = false;
                monsterEnd0 = true;
                monsterEnd1 = true;
                monsterEnd2 = true;
                if (!isDead0)
                {
                    PlayerSkillHurt(0, _skill);
                }
                if (!isDead1)
                {
                    PlayerSkillHurt(1, _skill);
                }
                if (!isDead2)
                {
                    PlayerSkillHurt(2, _skill);
                }
                timeScaleValue *= 1.35f;
            }
        }
        public void AutoFightClick(bool _isAuto)
        {

            if (!isPlayerBattle)
            {
                if (!isAuto)
                {
                    isAuto = _isAuto;
                    PlayerBattle();
                }
            }
            isAuto = _isAuto;
        }
        public void PlayerBattle()
        {
            AudioController.instance.PlayAudio(SE.Click2);
            isPlayerBattle = true;
            GameComponentData.gameData.fightPanelAction.FightHideFunctions();

            StartCoroutine("PlayerFighting");

        }

        public void AfterBronEffect()
        {
            if (isDead0)
            {
                isDead0 = false;

                gamePlayer.property.HP = gamePlayer.property.MaxHP;
                charactorObj.GetComponent<Animator>().SetBool("IsDead", false);
                InformationController.instance.AddInformation("*" + gamePlayer.name + LanguageManage.SwitchStr("复活"));
            }
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0 && isDead1)
            {
                isDead1 = false;
                gamePlayer.TeamPlayer0.property.HP = gamePlayer.TeamPlayer0.property.MaxHP;
                teamPlayerObj0.GetComponent<Animator>().SetBool("IsDead", false);
                InformationController.instance.AddInformation("*" + gamePlayer.TeamPlayer0.name + LanguageManage.SwitchStr("复活"));
            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0 && isDead2)
            {
                isDead2 = false;
                gamePlayer.TeamPlayer1.property.HP = gamePlayer.TeamPlayer1.property.MaxHP;
                teamPlayerObj1.GetComponent<Animator>().SetBool("IsDead", false);
                InformationController.instance.AddInformation("*" + gamePlayer.TeamPlayer1.name + LanguageManage.SwitchStr("复活"));
            }
            if (isFightContineu)
            {
                isFightContineu = false;
                MonsterFight();
            }
            else
            {
                if (isAuto)
                {
                    PlayerBattle();
                }
                else
                {
                    GameComponentData.gameData.fightPanelAction.FightDisplayFunctions();
                }
            }

        }

        public void CreatRebronEffect()
        {
            GameObject RebronEffectPro = GameComponent.Effects.Find(e => e.name == "ReBron");
            if (isDead0)
            {
                Instantiate(RebronEffectPro, charactorObj.transform.position, Quaternion.identity);
            }
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0 && isDead1)
            {
                Instantiate(RebronEffectPro, teamPlayerObj0.transform.position, Quaternion.identity);
            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0 && isDead2)
            {
                Instantiate(RebronEffectPro, teamPlayerObj1.transform.position, Quaternion.identity);
            }
        }

        IEnumerator AfterAttributeEffect()
        {
            yield return new WaitForSeconds(1.5f);
            GameComponentData.gameData.fightPanelAction.SetPlayerAttribute(gamePlayer.attributeType);
            if (isFightContineu)
            {
                isFightContineu = false;
                MonsterFight();
            }
        }

        public void AfterEffectAction()
        {
            GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(0, 0);
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
            {
                GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(1, 0);
            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
            {
                GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(2, 0);
            }
            if (isFightContineu)
            {
                isFightContineu = false;


                MonsterFight();
            }

        }

        public void EscapeAction()
        {
            int level = 0;
            Monster monster = null;
            if (monster0 != null)
            {
                level = monster0.level;
            }
            if (monster1 != null && monster1.level > level)
            {
                level = monster1.level;
            }
            if (monster2 != null && monster2.level > level)
            {
                level = monster2.level;
            }


            if (level < gamePlayer.level)
            {
                EscapeSucessfull();
                Camera.main.transform.position = new Vector3(0, 0, -20);
                //AudioManager.PlayBGM(PlayType.CYCLE,GameComponentData.gameData.passDataManager.NowPassData.FriendBGM);
            }
            else
            {
                int levelValue = level - gamePlayer.level;
                int count = levelValue / 5 + 1;
                float value = Mathf.Pow(0.5f, count);
                float randomVaue = Random.Range(0, 1);
                if (value >= randomVaue)
                {
                    EscapeSucessfull();
                }
                else
                {
                    EscapeDefate();
                }
            }
        }

        public void InformationEnd(InformationType informationType)
        {
            switch (informationType)
            {
                case InformationType.使用道具:
                    break;
                case InformationType.逃跑失败:
                    fightEnd0 = fightEnd1 = fightEnd2 = true;
                    MonsterFight();
                    break;
                case InformationType.逃跑成功:

                    if (charactorObj != null)
                    {
                        Destroy(charactorObj);
                    }
                    if (teamPlayerObj0 != null)
                    {
                        Destroy(teamPlayerObj0);
                    }
                    if (teamPlayerObj1 != null)
                    {
                        Destroy(teamPlayerObj1);
                    }
                    if (monster0 != null)
                    {
                        Destroy(monster0.Obj);
                        monster0 = null;
                    }
                    if (monster1 != null)
                    {
                        Destroy(monster1.Obj);
                        monster1 = null;
                    }
                    if (monster2 != null)
                    {
                        Destroy(monster2.Obj);
                        monster2 = null;
                    }


                    if (isDead0)
                    {
                        gamePlayer.property.HP = 1;
                        //gamePlayer.property.Power = 1;
                    }


                    //GameComponentData.gameData.gameManager.UpDataPlayer();
                    if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
                    {
                        NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id);
                        if (npcx != null)
                        {
                            if (isDead1)
                            {
                                GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                                npcx.npcData.npcStatus = NpcStatus.修养中;
                                npcx.WaitDays = 2;
                                InformationController.instance.AddInformation("*NPC" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                            }

                        }
                        else if (gamePlayer.TeamPlayer0.id / 1000000 == 2)
                        {
                            if (isDead1)
                            {
                                Pasture _pasture = null;
                                foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                                {
                                    if (pasture.Animals != null)
                                    {
                                        if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer0.id))
                                        {
                                            _pasture = pasture;
                                            break;
                                        }
                                    }
                                }
                                Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer0.id);
                                if (animal != null)
                                {
                                    Destroy(animal.Obj);
                                    GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                                    _pasture.RemoveAnimal(animal.id);

                                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));
                                }

                            }


                        }
                    }
                    if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
                    {
                        NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id);
                        if (npcx != null)
                        {
                            if (isDead2)
                            {
                                GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                                npcx.npcData.npcStatus = NpcStatus.修养中;
                                npcx.WaitDays = 2;
                                InformationController.instance.AddInformation("*" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                            }

                        }
                        else if (gamePlayer.TeamPlayer1.id / 1000000 == 2)
                        {
                            if (isDead2)
                            {
                                Pasture _pasture = null;
                                foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                                {
                                    if (pasture.Animals != null)
                                    {
                                        if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer1.id))
                                        {
                                            _pasture = pasture;
                                            break;
                                        }
                                    }
                                }
                                Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer1.id);
                                if (animal != null)
                                {
                                    Destroy(animal.Obj);
                                    GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                                    _pasture.RemoveAnimal(animal.id);
                                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物")
                                                                                                 + animal.Name + LanguageManage.SwitchStr("战死"));
                                }

                            }


                        }
                    }
                    gamePlayer.TeamPlayer0 = null;
                    gamePlayer.TeamPlayer1 = null;

                    GameComponentData.gameData.mapParent.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.FightEnd();
                    batteleMap = null;
                    fightEnd0 = fightEnd1 = fightEnd2 = true;
                    GameComponentData.gameData.fightPanelAction.MovingFunctionButton();
                    break;
                case InformationType.战斗胜利:
                    if (charactorObj != null)
                    {
                        Destroy(charactorObj);
                    }
                    if (teamPlayerObj0 != null)
                    {
                        Destroy(teamPlayerObj0);
                    }
                    if (teamPlayerObj1 != null)
                    {
                        Destroy(teamPlayerObj1);
                    }
                    if (monster0 != null)
                    {
                        Destroy(monster0.Obj);
                        monster0 = null;
                    }
                    if (monster1 != null)
                    {
                        Destroy(monster1.Obj);
                        monster1 = null;
                    }
                    if (monster2 != null)
                    {
                        Destroy(monster2.Obj);
                        monster0 = null;
                    }
                    if (isDead0)
                    {
                        gamePlayer.property.HP = 1;
                        //gamePlayer.property.Power = 1;
                    }


                    //GameComponentData.gameData.gameManager.UpDataPlayer();
                    if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
                    {
                        NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id);
                        if (npcx != null)
                        {
                            if (isDead1)
                            {
                                GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                                npcx.npcData.npcStatus = NpcStatus.修养中;
                                npcx.WaitDays = 2;
                                InformationController.instance.AddInformation("*NPC" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                                gamePlayer.TeamPlayer0 = null;

                                GameComponentData.gameData.gameManager.hurtNpc = npcx.npcData;
                            }

                        }
                        else if (gamePlayer.TeamPlayer0.id / 1000000 == 2)
                        {
                            if (isDead1)
                            {
                                Pasture _pasture = null;
                                foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                                {
                                    if (pasture.Animals != null)
                                    {
                                        if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer0.id))
                                        {
                                            _pasture = pasture;
                                            break;
                                        }
                                    }
                                }
                                Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer0.id);
                                if (animal != null)
                                {
                                    Destroy(animal.Obj);
                                    GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                                    _pasture.RemoveAnimal(animal.id);
                                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));
                                }
                                gamePlayer.TeamPlayer0 = null;
                            }


                        }

                    }
                    if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
                    {
                        NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id);
                        if (npcx != null)
                        {
                            if (isDead2)
                            {
                                GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                                npcx.npcData.npcStatus = NpcStatus.修养中;
                                npcx.WaitDays = 2;
                                InformationController.instance.AddInformation("*" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                                gamePlayer.TeamPlayer1 = null;
                            }

                        }
                        else if (gamePlayer.TeamPlayer1.id / 1000000 == 2)
                        {
                            if (isDead2)
                            {
                                Pasture _pasture = null;
                                foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                                {
                                    if (pasture.Animals != null)
                                    {
                                        if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer1.id))
                                        {
                                            _pasture = pasture;
                                            break;
                                        }
                                    }
                                }
                                Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer1.id);
                                if (animal != null)
                                {
                                    Destroy(animal.Obj);
                                    GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                                    _pasture.RemoveAnimal(animal.id);
                                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));
                                }
                                gamePlayer.TeamPlayer1 = null;
                            }


                        }

                    }

                    Destroy(map0);
                    Destroy(map1);
                    GameComponentData.gameData.mapParent.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.FightEnd();
                    batteleMap = null;
                    break;
                case InformationType.战斗失败:
                    Destroy(charactorObj);
                    Destroy(teamPlayerObj0);
                    Destroy(teamPlayerObj1);
                    if (monster0 != null)
                    {
                        Destroy(monster0.Obj);
                        monster0 = null;
                    }
                    if (monster1 != null)
                    {
                        Destroy(monster1.Obj);
                        monster1 = null;
                    }
                    if (monster2 != null)
                    {
                        Destroy(monster0.Obj);
                        monster0 = null;
                    }
                    if (isDead0)
                    {
                        gamePlayer.property.HP = 1;
                        //gamePlayer.property.Power = 1;
                        GameComponentData.gameData.gameManager.UpDataPlayer();
                    }
                    if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
                    {
                        NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id);
                        if (npcx != null)
                        {
                            GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                            npcx.npcData.npcStatus = NpcStatus.修养中;
                            npcx.WaitDays = 2;
                            InformationController.instance.AddInformation("*NPC" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                        }
                        else if (gamePlayer.TeamPlayer0.id / 1000000 == 2)
                        {


                            Pasture _pasture = null;
                            foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                            {
                                if (pasture.Animals != null)
                                {
                                    if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer0.id))
                                    {
                                        _pasture = pasture;
                                        break;
                                    }
                                }
                            }

                            Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer0.id);
                            if (animal != null)
                            {
                                Destroy(animal.Obj);
                                GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                                _pasture.RemoveAnimal(animal.id);
                                InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));

                            }

                        }
                        else
                        {
                            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*佣兵") + gamePlayer.TeamPlayer0.name + "伤残，已离开");
                        }

                    }
                    if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
                    {
                        NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id);
                        if (npcx != null)
                        {
                            GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id).isHired = false;
                            npcx.npcData.npcStatus = NpcStatus.修养中;
                            npcx.WaitDays = 2;
                            InformationController.instance.AddInformation("*" + npcx.Name + LanguageManage.SwitchStr("受到重伤，进入修养中"));
                        }
                        else if (gamePlayer.TeamPlayer1.id / 1000000 == 2)
                        {
                            Pasture _pasture = null;
                            foreach (var pasture in GameComponentData.gameData.pastureAction.Pastures)
                            {
                                if (pasture.Animals != null)
                                {
                                    if (pasture.Animals.Exists(a => a.id == gamePlayer.TeamPlayer1.id))
                                    {
                                        _pasture = pasture;
                                        break;
                                    }
                                }
                            }
                            Animal animal = _pasture.Animals.Find(a => a.id == gamePlayer.TeamPlayer1.id);

                            if (animal != null)
                            {
                                Destroy(animal.Obj);
                                GameComponentData.gameData.employerManger.AnimalDead(animal.id);
                                _pasture.RemoveAnimal(animal.id);
                                InformationController.instance.AddInformation(LanguageManage.SwitchStr("*动物") + animal.Name + LanguageManage.SwitchStr("战死"));

                            }

                        }
                        else
                        {
                            InformationController.instance.AddInformation("*佣兵" + gamePlayer.TeamPlayer0.name + " 伤残，已离开");
                        }

                    }


                    if (isDead1)
                    {
                        gamePlayer.TeamPlayer0 = null;
                    }
                    if (isDead2)
                    {
                        gamePlayer.TeamPlayer1 = null;
                    }
                    Destroy(map0);
                    Destroy(map1);
                    GameComponentData.gameData.mapParent.gameObject.SetActive(true);
                    GameComponentData.gameData.fightPanelAction.FightEnd();
                    batteleMap = null;
                    break;
                default:
                    break;
            }

        }
        void EscapeSucessfull()
        {

            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*逃跑成功"));
            FightInformationObj.SetActive(true);
            FightInformationObj.GetComponent<FightInformationAction>().InitData(LanguageManage.SwitchStr("逃跑成功！"), InformationType.逃跑成功);
        }

        void EscapeDefate()
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*逃跑失败"));
            FightInformationObj.SetActive(true);
            FightInformationObj.GetComponent<FightInformationAction>().InitData(LanguageManage.SwitchStr("逃跑失败！"), InformationType.逃跑失败);
        }

        public void ReBron()
        {
            FightPanelAction fightPanelAction = GameComponentData.gameData.fightPanelAction;
            fightPanelAction.UpDataPlayerHp(0, gamePlayer.property.MaxHP);
            fightPanelAction.playerType0.gameObject.SetActive(false);
            fightPanelAction.SetAttributeIcon(gamePlayer.attributeType, fightPanelAction.attribute0);
            if (gamePlayer.TeamPlayer0 != null && gamePlayer.TeamPlayer0.id != 0)
            {
                GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(1, gamePlayer.TeamPlayer0.property.MaxHP);
                GameComponentData.gameData.fightPanelAction.playerType1.gameObject.SetActive(false);
                fightPanelAction.SetAttributeIcon(gamePlayer.TeamPlayer0.attributeType, fightPanelAction.attribute1);
            }
            if (gamePlayer.TeamPlayer1 != null && gamePlayer.TeamPlayer1.id != 0)
            {
                GameComponentData.gameData.fightPanelAction.UpDataPlayerHp(2, gamePlayer.TeamPlayer1.property.MaxHP);
                GameComponentData.gameData.fightPanelAction.playerType2.gameObject.SetActive(false);
                fightPanelAction.SetAttributeIcon(gamePlayer.TeamPlayer1.attributeType, fightPanelAction.attribute2);
            }

            CreatRebronEffect();
            AudioController.instance.PlayAudio(SE.Magic);
        }
        public void UseItem(ItemData selectItem, DisplayType _displayType)
        {
            GameComponentData.gameData.warehouseObj.SetActive(false);
            if (_displayType == DisplayType.Battling)
            {
                //GameComponentData.gameData.fightPanelAction.Fightfunction1.SetActive(false);
                if (selectItem.id == 1501)
                {

                    ReBron();
                }
                else if (selectItem.Type == ItemType.药剂 && selectItem.typeValue != -1)
                {
                    AttributeType attributeType = (AttributeType)selectItem.typeValue;

                    if (charactorObj != null)
                    {
                        GameObject attributeOj = Instantiate(attributeEffect, charactorObj.transform.position, Quaternion.identity);
                        switch (attributeType)
                        {
                            case AttributeType.光:
                                gamePlayer.SkillId = 10400;
                                attributeOj.GetComponent<SpriteRenderer>().color = LightColor;
                                break;
                            case AttributeType.风:
                                gamePlayer.SkillId = 10300;
                                attributeOj.GetComponent<SpriteRenderer>().color = WideColor;
                                break;
                            case AttributeType.冰:
                                gamePlayer.SkillId = 10200;
                                attributeOj.GetComponent<SpriteRenderer>().color = IceColor;
                                break;
                            case AttributeType.暗:
                                gamePlayer.SkillId = 10500;
                                attributeOj.GetComponent<SpriteRenderer>().color = DarkColor;
                                break;
                            case AttributeType.火:
                                gamePlayer.SkillId = 10100;
                                attributeOj.GetComponent<SpriteRenderer>().color = FireColor;
                                break;
                        }
                    }
                    gamePlayer.attributeType = attributeType;
                    StartCoroutine("AfterAttributeEffect");
                }
                else if (selectItem.Type == ItemType.卷轴)
                {

                    UseItemSkill(selectItem.typeValue);

                }
                else
                {
                    GameComponentData.gameData.warehouseObj.SetActive(false);
                    if (selectItem.property.HP == -1)
                    {
                        hp = 100000;
                    }
                    else
                    {
                        hp = selectItem.property.HP;
                    }
                    AddHp();
                }

                isFightContineu = true;
            }
            else if (_displayType == DisplayType.AfterBattle)
            {
                if (selectItem.id == 1501)
                {

                    ReBron();
                }
                else if (selectItem.Type == ItemType.药剂 && selectItem.typeValue != -1)
                {
                    AttributeType attributeType = (AttributeType)selectItem.typeValue;
                    if (charactorObj != null)
                    {
                        GameObject attributeOj = Instantiate(attributeEffect, charactorObj.transform.position, Quaternion.identity);
                        switch (attributeType)
                        {
                            case AttributeType.光:
                                attributeOj.GetComponent<SpriteRenderer>().color = LightColor;
                                break;
                            case AttributeType.风:
                                attributeOj.GetComponent<SpriteRenderer>().color = WideColor;
                                break;
                            case AttributeType.冰:
                                attributeOj.GetComponent<SpriteRenderer>().color = IceColor;
                                break;
                            case AttributeType.暗:
                                attributeOj.GetComponent<SpriteRenderer>().color = DarkColor;
                                break;
                            case AttributeType.火:
                                attributeOj.GetComponent<SpriteRenderer>().color = FireColor;
                                break;
                        }
                    }
                    gamePlayer.attributeType = attributeType;
                }
                else
                {
                    if (selectItem.property.HP == -1)
                    {
                        hp = 100000;
                    }
                    else
                    {
                        hp = selectItem.property.HP;
                    }
                    AddHp();
                }

            }
        }
        public void AddHp()
        {
            GameObject Obj = GameComponent.Effects.Find(e => e.name == "Hp");
            if (!isDead0 && charactorObj != null)
            {
                gamePlayer.property.HP += hp;
                Instantiate(Obj, charactorObj.transform.position, Quaternion.identity);
            }
            if (!isDead1 && teamPlayerObj0 != null)
            {
                gamePlayer.TeamPlayer0.property.HP += hp;
                Instantiate(Obj, teamPlayerObj0.transform.position, Quaternion.identity);
            }
            if (!isDead2 && teamPlayerObj1 != null)
            {
                gamePlayer.TeamPlayer1.property.HP += hp;
                Instantiate(Obj, teamPlayerObj1.transform.position, Quaternion.identity);
            }
            hp = 0;
            AudioController.instance.PlayAudio(SE.Heal);
        }

        private void MonsterFight()
        {
            if (monster0 != null && monster0.Obj != null && monster0.property.HP > 0 && monster0.skill != null && monster0.skillValue >= 100)
            {
                monster0.skillValue = 0;
                GameComponentData.gameData.skillManager.SkillAction(monster0.skill, false, monster0.property.AT, monster0.Obj.transform.position);
            }
            else if (monster1 != null && monster1.Obj != null && monster1.property.HP > 0 && monster1.skill != null && monster1.skillValue >= 100)
            {
                monster1.skillValue = 0;
                GameComponentData.gameData.skillManager.SkillAction(monster1.skill, false, monster1.property.AT, monster1.Obj.transform.position);
            }
            else if (monster2 != null && monster2.Obj != null && monster2.property.HP > 0 && monster2.skill != null && monster2.skillValue >= 100)
            {
                monster2.skillValue = 0;
                GameComponentData.gameData.skillManager.SkillAction(monster2.skill, false, monster2.property.AT, monster2.Obj.transform.position);
            }
            else
            {
                StartCoroutine("MonsterFighting");
            }
        }
        IEnumerator MonsterFighting()
        {
            fightEnd0 = fightEnd1 = fightEnd2 = false;
            InitAttackOrder();
            if (monster0 != null && monster0.Obj != null && monster0.property.HP > 0)
            {
                monster0.Obj.GetComponentInChildren<Animator>().SetBool("IsFight", true);
            }
            yield return new WaitForSeconds(playerfightCd);
            InitAttackOrder();
            if (monster1 != null && monster1.Obj != null && monster1.property.HP > 0)
            {
                monster1.Obj.GetComponentInChildren<Animator>().SetBool("IsFight", true);
            }
            yield return new WaitForSeconds(playerfightCd);
            InitAttackOrder();
            if (monster2 != null && monster2.Obj != null && monster2.property.HP > 0)
            {
                monster2.Obj.GetComponentInChildren<Animator>().SetBool("IsFight", true);
            }

        }

        void Startinitflyitem()
        {
            AudioController.instance.PlayAudio(SE.Get);
            ItemPoses = new List<Vector2>();
            foreach (var flyItem in flyItems)
            {
                Vector2 pos = new Vector2(Random.Range(ItemArea0.position.x, ItemArea1.position.x), Random.Range(ItemArea0.position.y, ItemArea1.position.y));
                ItemPoses.Add(pos);
            }
            StartCoroutine("ItemFlying");
        }

        public void CheckFlyItem()
        {
            if (!itemObjects.Exists(i => !i.GetComponent<ItemFlyAction>().IsFlyEnd))
            {
                foreach (var itemObject in itemObjects)
                {
                    itemObject.GetComponent<ItemFlyAction>().StartResward();
                }
            }
        }

        public void CheckRewardItem()
        {
            if (!itemObjects.Exists(i => !i.GetComponent<ItemFlyAction>().IsReward) && !isDisplayResult)
            {

                isDisplayResult = true;
                if (boxObj != null)
                {
                    Destroy(boxObj);
                }
                foreach (Transform child in flyItemparent)
                {
                    Destroy(child.gameObject);
                }
                itemObjects.Clear();
                if (monsterTime >= 100)
                {
                    if (isZero)
                    {
                        gamePlayer.TeamPlayer0 = null;
                        gamePlayer.TeamPlayer1 = null;
                        Destroy(charactorObj);
                        Destroy(teamPlayerObj0);
                        Destroy(teamPlayerObj1);
                        foreach (Transform child in mapParent)
                        {
                            Destroy(child.gameObject);
                        }

                        GameComponentData.gameData.mapParent.gameObject.SetActive(true);
                        GameComponentData.gameData.fightPanelAction.informationObj.transform.SetParent(GameComponentData.gameData.fightPanelAction.informaParent1, true);
                        GameComponentData.gameData.fightPanelAction.informationObj.transform.localPosition = Vector3.zero;
                        //GameComponentData.gameData.fightPanelAction.functionButtons.SetActive(true);


                        GameComponentData.gameData.fightPanelAction.gameObject.SetActive(false);
                        AudioController.instance.StopBgm();
                        GameComponentData.gameData.eventManager.CheckEvents();
                        // GameComponentData.gameData.zeroFilmController.FightEndAction();
                    }
                    else
                    {
                        StopAllCoroutines();
                        GameComponentData.gameData.charactorTitleAction.AddExplorCount();

                        ResultObj.SetActive(true);
                        ResultObj.GetComponent<AdventureResultAction>().InitData(true, Resultitems, gamePlayer);
                    }


                }
                else
                {
                    isDisplayResult = false;
                    AudioController.instance.PlayAudio(BGM.Move);
                    GameComponentData.gameData.fightPanelAction.MovingFunctionButton();
                    if (isAuto)
                    {
                        ClickAutoButton(true);
                    }
                }

            }
        }
        IEnumerator ItemFlying()
        {
            itemObjects = new List<GameObject>();
            Vector3 pos = new Vector3((MonsterPos0.position.x + MonsterPos2.position.x) / 2, (MonsterPos0.position.y + MonsterPos1.position.y) / 2, MonsterPos0.position.z);
            while (true)
            {
                if (flyItems.Count > 0)
                {

                    Vector2 pos1 = new Vector2(Random.Range(ItemArea0.position.x, ItemArea1.position.x), Random.Range(ItemArea0.position.y, ItemArea1.position.y));
                    GameObject itemObj = Instantiate(flyItemPro);
                    itemObj.transform.SetParent(flyItemparent, false);
                    itemObj.transform.position = pos;
                    itemObj.GetComponent<ItemFlyAction>().InitItemFlyData(pos1, itemflySpeed, flyItems[0]);

                    // itemObj.transform.localScale=Vector3.one;
                    itemObjects.Add(itemObj);
                    flyItems.RemoveAt(0);
                }
                else
                {
                    StopCoroutine("ItemFlying");
                }
                yield return new WaitForSeconds(itemCd);
            }

        }
        IEnumerator TeamFight()
        {
            if (teamPlayerObj0 != null && !isDead1)
            {
                fightEnd1 = false;
                isPlayerHurtEnd1 = false;
                teamPlayerObj0.GetComponentInChildren<Animator>().SetBool("IsFight", true);
            }
            else
            {
                isPlayerHurtEnd1 = true;
            }
            yield return new WaitForSeconds(playerfightCd);
            if (teamPlayerObj1 != null && !isDead2)
            {
                fightEnd2 = false;
                isPlayerHurtEnd2 = false;
                teamPlayerObj1.GetComponentInChildren<Animator>().SetBool("IsFight", true);
            }
            else
            {
                isPlayerHurtEnd2 = true;
            }

        }
        IEnumerator PlayerFighting()
        {
            if (charactorObj != null && !isDead0)
            {
                fightEnd0 = false;
                isPlayerHurtEnd0 = false;
                charactorObj.GetComponentInChildren<Animator>().SetBool("IsFight", true);
            }
            else
            {
                isPlayerHurtEnd0 = true;
            }

            yield return new WaitForSeconds(playerfightCd);
            StartCoroutine("TeamFight");

        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}

