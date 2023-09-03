using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using LitJson;
using OldName;
public class SceneData
{
    public static int PassId;
    public static bool isTest=true;
    public static bool isImmediate;
    public static bool isFightEnd;
}
public class UserData
{
    public string name;
    public int passId;

}

[System.Serializable]
public enum MoneyCostType
{
    AddSoliderCost=1,
    BuyItemCost=2,
    ReburnCost=3
}

[System.Serializable]
public class Boundary
{
    public float MinX, MinY, MaxX, MaxY;
}
[System.Serializable]
public class ShopData
{
    public int passId;
    public int shopPackage;
}

public enum CareType
{
    SLEEP=0,
    Prodece=1,
    OutBattle=2,
    ClearPlant=3,
    GoldExchange=4,
    leaveLove=5,
    属性提醒=6
}

public enum CostType
{
    增加柜台=0,
    增加田地=1,
    增加牧场=2,
    增加牧场容量=3,
    增加牧场产出格子=4,
    购买道具=5,
    购买设施=6,
    购买佣兵=7,
    复活=8,
    增加背包格子=9,
    增加杂物箱格子=10,
    增加冰箱格子=11,
    炼金=12
}
public class GroundItem
{
    public Item item;
    public GameObject Obj;
    public int MapId;
    public Vector2Int coordinate;
    public GroundItem(Item _item,GameObject _obj,int _MapId,Vector2Int _coordinate)
    {
        item = _item;
        Obj = _obj;
        SetData();
        MapId = _MapId;
        coordinate = _coordinate;
        Vector3 objPos = Obj.transform.position;
        Obj.transform.position=new Vector3(objPos.x,objPos.y,-1);
    }

    public async void SetData()
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
        Obj.GetComponentInChildren<SpriteRenderer>().sprite = itemData.icon;
        Obj.GetComponentInChildren<Text>().text = item.count.ToString();
    }

    public void ChangeCount(int _count)
    {
        item.count = _count;
        Obj.GetComponentInChildren<Text>().text = item.count.ToString();
    }
    public void SetObjPos()
    {
        Vector3 pos = AStarTest.CoordinateToPos(coordinate);
        Obj.transform.position = pos;
    }

    public void CreatObj()
    {
        if (GameComponentData.gameData.passDataManager.nowPass == MapId)
        {
            Obj = MonoBehaviour.Instantiate(GameComponentData.gameData.GroundItemPro);
            SetObjPos();
            SetData();
        }
    }

    public void LostAction()
    {
        item.count--;

    }
}
[System.Serializable]
public struct Money1
{
    public int count;
}

[System.Serializable]
public struct PlayerEnglishName
{
    public int id;
    public string name;
}
[System.Serializable]
public struct PackageCaseAdd
{
    public int caseAdd;
    public int zeroCost;
    
}
public enum PlayerMoveType
{
    Start=0,
    End=1,
    Fishing=2
}
public class GameManager : MonoBehaviour
{
    public Texture2D t1;
    public GameObject ChildFunctionObj;
    public GameObject SaveDataPanel;
    public GameObject functionButtn;
    public List<Text> FunctionTexts;
    [HideInInspector]
    public ChildData childData;
    public PackageCaseAdd pakageAddData, boxAddData, iceBoxAddData;

    public GameObject RedMoneyBuyPanel;
    public GameObject LianJinPanel;
    public GameObject IntelligencePanelObj;
    public GameObject adventure;
    public Image headIcon;
    public Slider HpSlider,RpSlider;
    public Vector2 LostCd;
    private List<GroundItem> GroundItems;
    public Text MoneyText,Money1Text;
    public GameObject tilemask;
    public Transform TileMaskPerent;
    public GameObject TwoSelectPanel, TwoSelectPanel1;

    
    [HideInInspector]
    public Boundary nowBoundary;
    public GameComponent GameData;
    private Transform mapParent;
    private Transform PlantParent;
    private Transform NpcParent;
    public GameObject tile;
    private AStarTest astar;
    private Vector2 cellNum;
    private int cameraSelectNum;
    private CamreaMoveTest cameraMove;
    public bool  isDisplayCoordinate;
    private List<GroundItem> groundItems;
    private Vector2 mapStartPos, mapEndPos;
 
    [HideInInspector]
    public float result0, result1;
    public int passId;
    private PassDataManager passDataManager;
    private MapEditAction mapEditAction;
    public static List<Sprite> buffIcons;
    public static List<GameObject> effectPro;
 
    private CareType careType;
    
    public Vector2 mapSize;
   
    public GameObject fieldTool;
    public bool IsAttackDisplay;
    [HideInInspector]
    public string pastureName0, pastureName1, pastureName2, pastureName3;
    public GamePlayer gamePlayer;
    public GameObject goldCostSelectObj;
    public Text PlayerMoneyText;
    private GameObject mapX;
    [HideInInspector] public List<Charactor> Charactors;
    private int goldCostValue;
    private CostType costType;
    private ShopMoneyType shopMoneyType;
    [HideInInspector] public Vector2Int euqipmentCoordinate;
    [HideInInspector]
    public Charactor playerCharactor;
    [HideInInspector]
    public PlayerMoveType playerMoveType;

    private int nextPassid, oldPassid, oldPass0;
    private int nowMapid;
    [HideInInspector]
    public List<GameObject> mapPros, battleMapPros;
    [HideInInspector]
    public int packageCount, boxCount, iceBoxCount;
    [HideInInspector] public int packageLevel, boxLevel, iceBoxLevel;
    public NPCX lover;
    public bool IsYueHui;
    [HideInInspector]
    public NPCData hurtNpc;


   

    public bool ChangePlayerMoney(int value)
    {
        if (value < 0 && gamePlayer.money >= value)
        {
            gamePlayer.money += value;
            if (gamePlayer.money <= 0)
            {
                gamePlayer.money = 0;
            }
            MoneyText.text = gamePlayer.money.ToString();

            if (shopMoneyType == ShopMoneyType.金币)
            {
                goldCostSelectObj.GetComponent<GoldCostSelect>().TotalText.text = gamePlayer.money.ToString();
            }
            return true;
        }
        else if (value >= 0)
        {
           
            gamePlayer.money += value;
            if (gamePlayer.money <= 0)
            {
                gamePlayer.money = 0;
            }
            MoneyText.text = gamePlayer.money .ToString();
            GameData.charactorTitleAction.CheckMoney();

            if (shopMoneyType == ShopMoneyType.金币)
            {
                goldCostSelectObj.GetComponent<GoldCostSelect>().TotalText.text = gamePlayer.money.ToString();
            }
            return true;
        }
        else
        {
            return false;
        }
        
    }
    public bool ChangePlayerMoney1(int value)
    {
        if (value < 0 && gamePlayer.money1 >= value)
        {
            gamePlayer.money1 += value;
            if (gamePlayer.money1 <= 0)
            {
                gamePlayer.money1 = 0;
            }
            Money1Text.text = gamePlayer.money1.ToString();

            if (shopMoneyType == ShopMoneyType.红晶)
            {
                goldCostSelectObj.GetComponent<GoldCostSelect>().TotalText.text = gamePlayer.money1.ToString();
            }
            return true;
        }
        else if (value >= 0)
        {
            gamePlayer.money1+= value;
            if (gamePlayer.money1<= 0)
            {
                gamePlayer.money1= 0;
            }
            Money1Text.text = gamePlayer.money1 .ToString();

            if (shopMoneyType == ShopMoneyType.红晶)
            {
                goldCostSelectObj.GetComponent<GoldCostSelect>().TotalText.text = gamePlayer.money1.ToString();
            }
            return true;
        }
        else
        {
            return false;
        }

    }
    public void AddMoney1(int value)
    {
        GetComponent<payAction>().waitApple.SetActive(false);

        if (value > 0)
        {
            ChangePlayerMoney1(value);
           GameData.DisplayPrompt(LanguageManage.SwitchStr("获得红晶:"+value));
        }
    }
    private void OnNativeShareSuccess(string result)
    {
        // Debug.Log("success: " + result);
        
    }
    private void OnNativeShareCancel(string result)
    {
        // Debug.Log("cancel: " + result);

    }
    public void ClickIntelligenceButton()
    {
        AudioController.instance.PlayAudio(SE.click);
        fieldTool.SetActive(false);
        IntelligencePanelObj.SetActive(true);
        IntelligencePanelObj.GetComponent<IntelligencePanelAction>().InitIntelligenceData();
    }
    public void ClickCell(Cell _cell)
    {
        if (_cell.isArable)
        {
            GameData.farmAction.FieldAction(_cell);
        }
    }

    void Start()
    {
       
    }

    void Awake ()
    {
        hurtNpc = null;
        IsYueHui = false;
	    packageLevel = 0;
	    boxLevel = 0;
        iceBoxLevel = 0;
	    oldPassid = -1;
	    GroundItems=new List<GroundItem>();
        Charactors=new List<Charactor>();
        PlayerMoneyText.text = gamePlayer.money.ToString();
	    pastureName0 = "";
	    pastureName1 = "";
	    pastureName2 = "";
	    pastureName3 = "";
        IsAttackDisplay = true;
        GetComponent<GameComponent>().InitData();


	    mapPros = Resources.LoadAll<GameObject>("Map").ToList();
	    battleMapPros = Resources.LoadAll<GameObject>("FightMap").ToList();

        groundItems =new List<GroundItem>();
	    
	    GameData.InitData();
	   
	    buffIcons = Resources.LoadAll<Sprite>("buff/").ToList();
	    effectPro = Resources.LoadAll<GameObject>("effect/").ToList();
        //GameData.gameTimeManager.InitData();
	    
	    
        mapParent = GameData.mapParent;
	    PlantParent = GameData.PlantParent;
	    NpcParent = GameData.NpcParent;
        
	    cameraSelectNum = 1;
	    cameraMove = GameData.cameraMove;

	    passDataManager = GameData.passDataManager;
	    mapEditAction = GameData.mapEditAction;
	    result0 = 0;
	    result1 = 0;
        GameData.shopManager.InitData();

        GameData.shopGoldDeskAction.saleValue = 100;
        if (mapEditAction.isMapEdit)
	    {
	        SceneData.PassId = passDataManager.nowPass;
	        passId = SceneData.PassId;
	        //加载地图
	        passDataManager.nowPass = passId;
	        passDataManager.InitData();
	        Pass nowPassData = passDataManager.NowPassData;
	        GameObject mapPro = Resources.Load<GameObject>("Map/" + nowPassData.background);
	        if (mapPro != null)
	        {
	            mapX = Instantiate(mapPro) as GameObject;
	            mapX.transform.SetParent(mapParent.transform, false);
	            mapX.transform.position = mapParent.transform.position;
	        }
	        else
	        {

	            mapX = Instantiate(Resources.Load<GameObject>("Map/DefalutMap")) as GameObject;
	            mapX.transform.SetParent(mapParent.transform);
	            mapX.transform.position = mapParent.transform.position;
	            mapX.name = nowPassData.background;
	        }



	        cellNum = new Vector2(nowPassData.mapSizeX, nowPassData.mapSizeY);
	        nowBoundary = CreatBoundary();
	        mapSize = new Vector2(nowPassData.mapSizeX * 48, nowPassData.mapSizeY * 48);

	        mapParent.GetComponent<BoxCollider2D>().size = new Vector2(nowPassData.mapSizeX * 48 / 100.0f, nowPassData.mapSizeY * 48 / 100.0f);
	        mapStartPos = new Vector2(-mapSize.x / 200, -mapSize.y / 200);
	        mapEndPos = new Vector2(mapSize.x / 200, mapSize.y / 200);
	        astar = new AStarTest(cellNum, mapStartPos, mapEndPos);
	        mapEditAction.InitData();
	        CreatTile();

	        mapEditAction.InitData();
	        cameraMove.InitData();

        }
	    else
	    { 
	        GameData.charactorDataAction.InitData();
	        GameData.influenceAction.InitInfluenceAction();
	        GameComponentData.gameData.NpcManager.InitData(); 
	        GameData.charactorTitleAction.Initdata0();
            if (DataSaveAndLoadTest.isJsonData)
	        {
	            
	            //DataSaveAndLoadTest.LoadUserData();
                DataSaveAndLoadTest.gameSaveData.InitPlayerLoadData(gamePlayer);
	            packageLevel = (PackageManager.instance.GetPackageCaseCount(0) - 5) / 5;
                boxLevel= (PackageManager.instance.GetPackageCaseCount(1) - 10) / 5;
                iceBoxLevel= (PackageManager.instance.GetPackageCaseCount(2) - 10) / 5;
                
                GameData.gameTimeManager.CreatData();
                
	            
	            if (DataSaveAndLoadTest.gameSaveData.marryData == null)
	            {
	                DataSaveAndLoadTest.gameSaveData.marryData=new MarryData();

                }
	            InitRoom();

            }
	        else
	        { 
                gamePlayer.InitGamePlayer();
                GameData.plantAction.ZeroInitData();
                InitZeroSceneData();
	            gamePlayer.TeamPlayer0 = null;
	            gamePlayer.TeamPlayer1 = null;

                PackageManager.instance.SetPackageCaseCount(0,gamePlayer.packageZeroCout);
                PackageManager.instance.SetPackageCaseCount(1, gamePlayer.boxZeroCount);
                PackageManager.instance.SetPackageCaseCount(2, gamePlayer.iceboxZeroCount); 
	           
	            GameData.gameTimeManager.InitData();

	     


            }
	        if (GameComponentData.gameData.gameDebugAction.GameStartTest)
	        {
	            GameComponentData.gameData.gameManager.InitRoom();
            }
	        else
	        {
	            GameData.eventManager.CheckEvents();
            }
	       
	        ChangePlayerMoney(0);
	        ChangePlayerMoney1(0);
            GameData.employerManger.InitEmployers();



            //加载地图
	        //InitGate();
	       // GameData.adventurePanelAction.gameObject.SetActive(true);
           // GameData.adventurePanelAction.ZeroEploring();
           

	        if (DataSaveAndLoadTest.isJsonData)
	        {
	            DataSaveAndLoadTest.gameSaveData.InitDeskData();
                CheckPauseTime();
            }
	        foreach (var functionText in FunctionTexts)
	        {
	            functionText.text = LanguageManage.SwitchStr(functionText.text);
	        }
	        if (GameTimeManager.nowGameTime.gameDate.year == 1300 && GameTimeManager.nowGameTime.gameDate.season == Season.春
	            && GameTimeManager.nowGameTime.gameDate.date == 1)
	        {
	            functionButtn.SetActive(false);
	        }
	        else
	        {
	            functionButtn.SetActive(true);
	        }
        }
        UpDataPlayer();
        headIcon.sprite = GameComponent.headIcons.Find(h => h.name == gamePlayer.IconName);
        //GameData.heritageAction.ClickheritageObjbutton();
        // GameData.NpcManager.InitData();
        float waitTime = UnityEngine.Random.Range(LostCd.x, LostCd.y);
        GameTimerController.instance.DelayAction((int)(waitTime * 1000), Losting);
    }
    public static void OutputRt(Texture2D rt)
    {
        string str = Application.persistentDataPath + "/001.png";

        byte[] byt = rt.EncodeToPNG();
        File.WriteAllBytes(str, byt);
    }
    void OnApplicationQuit()
    {
        DataSaveAndLoadTest.gameSaveData.SaveData();
        DataSaveAndLoadTest.CreatSaveData(-1);
    }

    public void ChildDateCost()
    {
        if (DataSaveAndLoadTest.gameSaveData.marryData.haveChildrenTime != null)
        {
            int foodcost = 2;
            int moodcost = 2;
            int cleancost = 2;
            switch (childData.BodyGrowStatus)
            {
                case GrowStatus.慢速成长:
                    foodcost = 1;
                    break;
                case GrowStatus.中速成长:
                    foodcost = 2;
                    break;
                case GrowStatus.生长停滞:
                    foodcost = 0;
                    break;
                case GrowStatus.高速成长:
                    foodcost = 3;
                    break;
            }
            switch (childData.MindGrowStatus)
            {
                case GrowStatus.慢速成长:
                    moodcost = 1;
                    break;
                case GrowStatus.中速成长:
                    moodcost = 2;
                    break;
                case GrowStatus.生长停滞:
                    moodcost = 0;
                    break;
                case GrowStatus.高速成长:
                    moodcost = 3;
                    break;
            }
            cleancost = Mathf.RoundToInt((foodcost + moodcost) / 2.0f);
            childData.foodValue -= foodcost;
            childData.moodValue -= moodcost;
            childData.cleanValue -= cleancost;
            if (childData.foodValue < 0)
            {
                childData.foodValue = 0;
            }
            if (childData.moodValue < 0)
            {
                childData.moodValue = 0;
            }
            if (childData.cleanValue < 0)
            {
                childData.cleanValue = 0;
            }
            int bodyGrowValue = Mathf.RoundToInt((childData.foodValue + childData.moodValue) / 2.0f);
            if (childData.foodValue == 0)
            {
                bodyGrowValue = 0;
            }
            int mindGrowValue = Mathf.RoundToInt((childData.cleanValue + childData.moodValue) / 2.0f);
            if (childData.moodValue == 0)
            {
                mindGrowValue = 0;
            }
            if (bodyGrowValue >= 8)
            {
                childData.BodyGrowStatus = GrowStatus.高速成长;
                childData.bodyValue += 3;
            }
            else if (bodyGrowValue >= 4)
            {
                childData.BodyGrowStatus = GrowStatus.中速成长;
                childData.bodyValue += 2;
            }
            else if (bodyGrowValue > 0)
            {
                childData.BodyGrowStatus = GrowStatus.慢速成长;
                childData.bodyValue += 1;
            }
            else
            {
                childData.BodyGrowStatus = GrowStatus.生长停滞;
            }
            if (mindGrowValue >= 8)
            {
                childData.MindGrowStatus = GrowStatus.高速成长;
                childData.mindValue += 3;
            }
            else if (mindGrowValue >= 4)
            {
                childData.MindGrowStatus = GrowStatus.中速成长;
                childData.mindValue += 2;
            }
            else if (mindGrowValue > 0)
            {
                childData.MindGrowStatus = GrowStatus.慢速成长;
                childData.mindValue += 1;
            }
            else
            {
                childData.MindGrowStatus = GrowStatus.生长停滞;
            }
        }
       


    }
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            DataSaveAndLoadTest.gameSaveData.SaveData();
            DataSaveAndLoadTest.CreatSaveData(-1);
        }
        else
        {
            CheckPauseTime();

        }
    }

    public void ClickChildAction()
    {
        ChildFunctionObj.SetActive(true);
        ChildFunctionObj.GetComponent<ChildFunctionAction>().DisplayChildData();
    }
    public void CheckPauseTime()
    {
        if (DataSaveAndLoadTest.gameSaveData.pauseTime!=null&&DataSaveAndLoadTest.gameSaveData.pauseTime.isAction)
        {
            DataSaveAndLoadTest.gameSaveData.pauseTime.isAction = false;
            PauseTime pauseTime = DataSaveAndLoadTest.gameSaveData.pauseTime;
            DateTime pause=new DateTime(pauseTime.year,pauseTime.month,pauseTime.day,pauseTime.hour,pauseTime.min,0);
            DateTime nowDateTime=DateTime.Now;
            var x = nowDateTime - pause;
            int total = (int)x.TotalMinutes;
            GameData.shopGoldDeskAction.SellInBack(total);

        }
    }
    public void ZeroFight()
    {
        //InitGate();
        GameData.adventurePanelAction.gameObject.SetActive(true);
         GameData.adventurePanelAction.ZeroEploring();
    }
  
    public void BuyGold()
    {
        LianJinPanel.SetActive(true);
        LianJinPanel.GetComponent<LianjinAction>().InitData();
    }
    public void BuyRedMoney()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            AudioController.instance.PlayAudio(SE.click);
            RedMoneyBuyPanel.SetActive(true);
        }
       
    }
    void InitZeroSceneData()
    {
        OutputRt(t1);
        DataSaveAndLoadTest.IniteZerodata();
        //DataSaveAndLoadTest.CreatNewData();
        DataSaveAndLoadTest.gameSaveData.ZeroInitProperty(gamePlayer.property);
        DataSaveAndLoadTest.gameSaveData.UpPlayerMoneyData();
        DataSaveAndLoadTest.gameSaveData.UpSaveTime();


    }
    public bool CostRp(int costValue)
    {
        if (gamePlayer.property.Power >= costValue)
        {
            gamePlayer.property.Power -= costValue;
            RpSlider.value = gamePlayer.property.Power / (float)gamePlayer.property.MaxPower;

            return true;
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不足"),LanguageManage.SwitchStr("体力不足，不能进行此项工作，请到 内宅 床上 休息以恢复体力。"));
            return false;
        }
    }
    public void ReturnFromAdventure()
    {
        adventure.SetActive(false);
    }
    public void UpDataPlayer()
    {
        HpSlider.value = (float)gamePlayer.property.HP /(float) gamePlayer.property.MaxHP;
        RpSlider.value = (float)gamePlayer.property.Power / (float)gamePlayer.property.MaxPower;
    }
    public void GreatGroundItem(Item item)
    {

        Vector2Int _coordinate=new Vector2Int(euqipmentCoordinate.x,euqipmentCoordinate.y+1);
        Vector3 pos = AStarTest.CoordinateToPos(_coordinate);
        GameObject obj = Instantiate(GameData.GroundItemPro,pos,Quaternion.identity);
        GroundItem groundItem=new GroundItem(item,obj,GameData.passDataManager.nowPass,_coordinate);
        GroundItems.Add(groundItem); 
    }
    public void GreatGroundItem(Item item,Vector2Int _coordinate)
    { 
        Vector3 pos = AStarTest.CoordinateToPos(_coordinate);
        GameObject obj = Instantiate(GameData.GroundItemPro, pos, Quaternion.identity);
        GroundItem groundItem = new GroundItem(item, obj, GameData.passDataManager.nowPass, _coordinate);
        GroundItems.Add(groundItem); 
    }
    public void InitCostData(string Title, int _costValue, string notice,CostType _costType,ShopMoneyType _shopMoneyType)
    {
        costType = _costType;
        goldCostValue = _costValue;
        shopMoneyType = _shopMoneyType;
        goldCostSelectObj.SetActive(true);
        goldCostSelectObj.GetComponent<GoldCostSelect>().InitGoldCostData(Title,_costValue,notice,shopMoneyType);
    }
    public bool InitCostData(int _costValue,ShopMoneyType _shopMoneyType,CostType _costType)
    {
        shopMoneyType = _shopMoneyType;
        costType = _costType;
        goldCostValue = _costValue;
       return CostAction();
    }
    public void GoldCostYes()
    {
        if (CostAction())
        {
            AudioController.instance.PlayAudio(SE.Click2);
            switch (costType)
            {
                case CostType.增加柜台:
                    InformationController.instance.AddInformation("*消耗金币" + goldCostValue + ",新增加一个柜台。");
                    GameComponentData.gameData.shopGoldDeskAction.AddNewDesk();
                    break;
                case CostType.增加牧场:
                    GameComponentData.gameData.pastureAction.BuildSucessful();
                    break;
                case CostType.增加牧场产出格子:
                    GameComponentData.gameData.pastureItemPanelAction.AddCase();
                    break;
                case CostType.增加牧场容量:
                    GameComponentData.gameData.pasturePanelAction.AddPastureCaseCount();
                    break;
                case CostType.增加田地:
                    InformationController.instance.AddInformation("*消耗金币" + goldCostValue + ",新开发一块田地。");
                    GameComponentData.gameData.farmAction.GrassClearAction();
                    break;
                case CostType.购买道具:
                    
                    GameComponentData.gameData.shopPanelAction.BuySucecssful();
                    break;
                case CostType.购买设施:
                    GameComponentData.gameData.shopPanelAction.BuySucecssful();
                    break; 
                case CostType.增加背包格子:
                    AddPackageAction(PackageType.背包);
                    break;
                case CostType.增加杂物箱格子:
                    AddPackageAction(PackageType.杂物箱);
                    break;
                case CostType.增加冰箱格子:
                    AddPackageAction(PackageType.冰箱);
                    break;
                case CostType.炼金:
                    GameComponentData.gameData.lianjinAction.Lianjin();
                    break;
            }
            goldCostSelectObj.SetActive(false);
        }
        else
        {
            AudioController.instance.PlayAudio(SE.Return);
            if (shopMoneyType == ShopMoneyType.金币)
            {
                InitCareSelectData(LanguageManage.SwitchStr("金币不足"), LanguageManage.SwitchStr("需消耗金币:") + goldCostValue
                    +LanguageManage.SwitchStr("现有金币:")+gamePlayer.money+LanguageManage.SwitchStr(",是否使用炼金术提炼金币？"),CareType.GoldExchange);
            }
            else
            {
                BuyRedMoney();
            }
            
            
        }
    }
    public bool CostAction()
    {
        if (shopMoneyType == ShopMoneyType.红晶)
        {
            if (gamePlayer.money1 >= goldCostValue)
            {
                gamePlayer.money1 -= goldCostValue;
                Money1Text.text = gamePlayer.money1.ToString();
                return true;
            }
            return false;
        }
        else
        {
            if (gamePlayer.money >= goldCostValue)
            {
                gamePlayer.money -= goldCostValue;
                PlayerMoneyText.text = gamePlayer.money.ToString();
                
                return true;
            }
            return false;
        }
       
    }

  
    public static int HurtValue(Property goldProperty,Property attackProperty)
    {
        int hurtValue = 1;
        if (attackProperty.AT - goldProperty.DF > 0)
        {
            hurtValue = attackProperty.AT - goldProperty.DF;
        }
        return hurtValue;
    }
    public void FightAction()
    {
        fieldTool.SetActive(false);
        AudioController.instance.PlayAudio(SE.click);
        adventure.SetActive(true);
        adventure.GetComponent<AdventurePanelAction>().InitData();
    }

    public void StartFight()
    {
        /*
        GameComponentData.gameData.pastureAction.HideAnimal();
        playerCharactor.Obj.SetActive(false);
        foreach (var groundItem in GroundItems)
        {
            groundItem.Obj.SetActive(false);
        }
        
        foreach (Transform mapObj in mapParent)
        {
            Destroy(mapObj.gameObject);
        }
        foreach (Transform child in NpcParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in PlantParent)
        {
            Destroy(child.gameObject);
        }
        GameData.shopGoldDeskAction.ShopingInBack();
        */

        GameData.mapEditAction.isCamreaMove = false;
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }
    public async void ClickGroundItem(GameObject Obj)
    {
        GroundItem groundItem = GroundItems.Find(g => g.Obj == Obj);
        ItemData itemData =await GameDataManager.instance.GetAsyncData<ItemData>(groundItem.item.dataId.ToString()); 
        int xcount =await PackageManager.instance.SetItemInPackage(groundItem.item,0);
       
        if (xcount > 0)
        {
            groundItem.item.count = xcount;
            groundItem.ChangeCount(xcount);

            int clickCount = groundItem.item.count - xcount;
            if (clickCount > 0)
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("捡起物品"), LanguageManage.SwitchStr("捡起了")
                    + (groundItem.item.count - xcount) + LanguageManage.SwitchStr("个") + itemData.name +","+ LanguageManage.SwitchStr("地上还有")
                    + xcount + LanguageManage.SwitchStr("个"));
                
                InformationController.instance.AddInformation("*"+LanguageManage.SwitchStr("捡起了") + (groundItem.item.count - xcount) + LanguageManage.SwitchStr("个") +":" + itemData.name);
            }
            else
            {
                InformationController.instance.AddInformation("*"+ LanguageManage.SwitchStr("背包已满，无法捡起物品"));
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("捡起物品"), LanguageManage.SwitchStr("背包已满，无法捡起物品"));
            }
           
        }
        else
        {
            InformationController.instance.AddInformation("*" + LanguageManage.SwitchStr("捡起了") + (groundItem.item.count - xcount) + LanguageManage.SwitchStr("个") + itemData.name);
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("捡起物品"), LanguageManage.SwitchStr("捡起了") + (groundItem.item.count - xcount) + LanguageManage.SwitchStr("个") + itemData.name);
            Destroy(groundItem.Obj);
            GroundItems.Remove(groundItem);
        }
    }
    async void Losting()
    {
        
        if (GroundItems.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, groundItems.Count - 1);
            GroundItem groundItem = GroundItems[index];
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(groundItem.item.dataId.ToString());  
            int lostcount = UnityEngine.Random.Range(1, groundItem.item.count);
            int count = groundItem.item.count - lostcount;
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*地底哥布林偷走了掉在地上的") + lostcount + LanguageManage.SwitchStr("个") + itemData.name);
            if (count > 0)
            {
                groundItem.ChangeCount(count);

            }
            else
            {
                Destroy(groundItem.Obj);
                GroundItems.Remove(groundItem);

            }
        }
        float waitTime = UnityEngine.Random.Range(LostCd.x, LostCd.y);
        GameTimerController.instance.DelayAction((int)(waitTime * 1000), Losting);
    }
     
    public void ClickSaveButton()
    {
        SaveDataPanel.SetActive(true);
        SaveDataPanel.GetComponent<MuSaveAction>().Initdata();
    }
    public void NextPass()
    {

        
        StartCoroutine("LoadScene");
    }

    public void GeOutToMainMenu()
    {
        StartCoroutine("LoadMainMenu");
    }

    public void InitCareSelectData(string Title, string Notice,CareType _careType)
    {
        careType = _careType;
        if (GameData.formulaAction.MamufacturePanel.activeSelf)
        {
            TwoSelectPanel1.SetActive(true);
            TwoSelectPanel1.GetComponent<TwoSelectAction>().InitTwoSelectData(Title, Notice);
        }
        else
        {
            TwoSelectPanel.SetActive(true);
            TwoSelectPanel.GetComponent<TwoSelectAction>().InitTwoSelectData(Title, Notice);
        }
       
    }

    public void YesButtonAction()
    {
        TwoSelectPanel1.SetActive(false);
        TwoSelectPanel.SetActive(false);
        switch (careType)
        {
            case CareType.属性提醒: 
                break;
                case CareType.SLEEP:
                    AudioController.instance.PlayAudio(SE.click);
                GameComponentData.gameData.DisplayWaitPanelData(WaitType.SLEEP, LanguageManage.SwitchStr("夜深了，风声伴人入眠..."));
                break;
                case CareType.Prodece:
                AudioController.instance.PlayAudio(SE.Click2);
                GameComponentData.gameData.manufacturingAction.ProduceItem();
                break;
                case  CareType.OutBattle:
                    AudioController.instance.PlayAudio(SE.click); 
                break;
                case CareType.ClearPlant:
                AudioController.instance.PlayAudio(SE.Return);
                GameData.farmAction.ClearSelectPlant();
                break;
                case CareType.GoldExchange:
                LianJinPanel.SetActive(true);
                LianJinPanel.GetComponent<LianjinAction>().InitData();
                break;
                case CareType.leaveLove:
                    AudioController.instance.PlayAudio(SE.Return);
                GameData.npcFunctionPanel.LeaveLover();
                break;
               
        }
    }

    public void LianjinAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        LianJinPanel.SetActive(true);
        LianJinPanel.GetComponent<LianjinAction>().InitData();
    }
    public void NoButtonAction()
    {
        AudioController.instance.PlayAudio(SE.Return);
        TwoSelectPanel.SetActive(false);
        TwoSelectPanel1.SetActive(false);
        switch (careType)
        {
                case CareType.SLEEP:
                
                break;
                case CareType.OutBattle:
                   
                break;
        }
    }
    public void PlayReturnAudio()
    {
        AudioController.instance.PlayAudio(SE.Return);
    }
    public void PlayClickAudio()
    {
        AudioController.instance.PlayAudio(SE.click);
    }
    public static void SetIconSize(Image icon, float sizeY)
    {
        icon.SetNativeSize();
        Vector2 size = icon.gameObject.GetComponent<RectTransform>().sizeDelta;
        if (size.y > sizeY)
        {
            float scaleValue = sizeY / size.y;
            icon.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * scaleValue, size.y * scaleValue);
        }
    }
    public void StartLoadScene()
    {
        StartCoroutine("LoadScene");
    }
    public void InitImmediateSceneData(int passId)
    { 
        /*
        if (SceneData.startZeroData.zeroPlayers!=null)
        {
            ZeroData = SceneData.startZeroData;
        }*/
        if (passId == 0)
        {
            passId = passDataManager.nowPass;
            SceneData.PassId = passDataManager.nowPass;
        }
        SceneData.isFightEnd = true;
        
        //加载地图
        
        passDataManager.nowPass = passId;
        passDataManager.InitData();
        Pass nowPassData = passDataManager.NowPassData;
        GameObject mapPro = Resources.Load<GameObject>("Map/" + nowPassData.background);
        GameObject mapX=Instantiate(mapPro)as GameObject;
        mapX.transform.SetParent(mapParent.transform,false);
        mapX.transform.position = mapParent.transform.position;
        cellNum = new Vector2(nowPassData.mapSizeX, nowPassData.mapSizeY);
        nowBoundary = CreatBoundary();
        mapSize = new Vector2(nowPassData.mapSizeX * 48, nowPassData.mapSizeY * 48);

        mapParent.GetComponent<BoxCollider2D>().size = new Vector2(nowPassData.mapSizeX * 48 / 100.0f, nowPassData.mapSizeY * 38 / 100.0f);
        mapStartPos = new Vector2(-mapSize.x / 200, -mapSize.y / 200);
        mapEndPos = new Vector2(mapSize.x / 200, mapSize.y / 200);
        astar = new AStarTest(cellNum, mapStartPos, mapEndPos);
        mapEditAction.InitData();
        CreatTile();
        cameraMove.InitData();

    }
  
    public void InitEditSceneData()
    {
       
        


    }

    public void MoveToOldMap()
    {
        MoveToMap(oldPass0);
    }
 
   
     void InitMapData(int _passId)
    {
       
        nowMapid = _passId;
        GameComponentData.gameData.pastureAction.HideAnimal();

        foreach (var groundItem in GroundItems)
        {
            if (groundItem.MapId == _passId)
            {
                groundItem.Obj.SetActive(true);
            }
            else
            {
                groundItem.Obj.SetActive(false);
            }
        }

        foreach (Transform mapObj in mapParent)
        {
            Destroy(mapObj.gameObject);
        }
        foreach (Transform child in NpcParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in PlantParent)
        {
            Destroy(child.gameObject);
        }
        passId = _passId;
        passDataManager.nowPass = passId;
        SceneData.PassId = passDataManager.nowPass;
        passDataManager.InitData();
        Pass nowPassData = passDataManager.NowPassData;
        passDataManager.PlayerMapBGM();
        GameObject mapPro = mapPros.Find(m=>m.name==nowPassData.background);
        mapX = Instantiate(mapPro) as GameObject;
        mapX.transform.SetParent(mapParent.transform, false);
        mapX.transform.position = mapParent.transform.position;
        SeasonSelect seasonSelect = mapX.GetComponent<SeasonSelect>();
        if (seasonSelect != null)
        {
            seasonSelect.SeasonSetData(GameTimeManager.nowGameTime.gameDate.season);
        }
        


        cellNum = new Vector2(nowPassData.mapSizeX, nowPassData.mapSizeY);
        nowBoundary = CreatBoundary();
        mapSize = new Vector2(nowPassData.mapSizeX * 48, nowPassData.mapSizeY * 48);
        mapParent.GetComponent<BoxCollider2D>().size = new Vector2(nowPassData.mapSizeX * 48 / 100.0f, nowPassData.mapSizeY * 48 / 100.0f);
        mapStartPos = new Vector2(-mapSize.x / 200, -mapSize.y / 200);
        mapEndPos = new Vector2(mapSize.x / 200, mapSize.y / 200);
        astar = new AStarTest(cellNum, mapStartPos, mapEndPos);
        if (passId == 1001)
        {
            GameData.farmAction.InitGrassObj(mapX.transform.GetChild(3));
        }
        
        mapEditAction.InitData();
        CreatTile();
        //cameraMove.InitData();
        GameComponentData.gameData.NpcManager.SetMapNpc(nowMapid);
       // MoveMapZero();

    }

    public void NpcMoveMapZero(int oldPass,NPCX npcx)
    {
        MapStartCoordinate mapStartCoordinate =
            passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == oldPass);
        Vector2Int zeroCoordinate = new Vector2Int();
        if (mapStartCoordinate != null)
        {
            zeroCoordinate = mapStartCoordinate.coordiante;
        }
        else
        {
            MapStartCoordinate mapStartCoordinate1 =
                passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == 0);
            zeroCoordinate = mapStartCoordinate1.coordiante;
        }
        if (npcx != null)
        {
            playerMoveType = PlayerMoveType.Start;
            Vector3 pos = AStarTest.CoordinateToPos(zeroCoordinate);
            if (npcx.Obj == null)
            {
                GameObject playerPro = Resources.Load<GameObject>("charactor/" + npcx.npcData.ObjName);
               
                npcx.Obj = Instantiate(playerPro);
            }
            npcx.Obj.transform.position = pos;
            npcx.npcAnimationAction = npcx.Obj.GetComponent<NPCAnimationAction>();
            npcx.npcAnimationAction.animator = npcx.Obj.GetComponent<Animator>();
            npcx.npcAnimationAction.costTime= GameComponentData.gameData.peopleAction.costTime;
            npcx.npcAnimationAction.charactor = npcx;
             npcx.coordinate = zeroCoordinate;
            npcx.npcAnimationAction.InitPlayerData(npcx, passDataManager.NowPassData.zeroGoldCoordinate);
        }
    }
    void CreatPlayer(Pass nowPassData)
    {
        MapStartCoordinate mapStartCoordinate =
            passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == nextPassid);
        Vector2Int zeroCoordinate = new Vector2Int();
        if (mapStartCoordinate != null)
        {
            zeroCoordinate = mapStartCoordinate.coordiante;
        }
        else
        {
            MapStartCoordinate mapStartCoordinate1 =
                passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == 0);
            zeroCoordinate = mapStartCoordinate1.coordiante;
        }

        GameObject playerPro = Resources.Load<GameObject>("charactor/"+gamePlayer.ObjName);
        GameObject playerObj = Instantiate(playerPro);
        playerObj.transform.position = AStarTest.CoordinateToPos(zeroCoordinate);
        OldName.ProfessionData playerProfessionData = GameData.charactorDataAction.professionDatas0.Find(p => p.id == 8);

        playerCharactor =new Charactor(80,gamePlayer.name,playerObj,nowPassData.id, zeroCoordinate, playerProfessionData,1);
        
    }


    public void MoveEndAction()
    {
        
        switch (playerMoveType)
        {
                case PlayerMoveType.End:
                   // GameComponentData.gameData.filmAction.MoveEndAction();
                    if (nextPassid == 1000)
                    {
                        InitMyShop();
                    }
                    else if (nextPassid == 1001)
                    {
                        InitMyFarm();
                    }
                    else if (nextPassid == 1002)
                    {
                        InitPasture();
                    }
                    else if (nextPassid == 1003)
                    {
                        InitRoom();
                    }
                    else if (nextPassid == 2000)
                    {
                        InitStreet();
                    }
                    else if (nextPassid == 2001)
                    {
                        InitSquare();
                    }
                    else if (nextPassid == 2002)
                    {
                        InitGate();
                    }
                    else
                    {
                        SwitchMapData(nextPassid);
                    }
                break;
                case PlayerMoveType.Start:
                    //GameComponentData.gameData.filmAction.MoveEndAction();
                break;
                case PlayerMoveType.Fishing:
                playerCharactor.Obj.GetComponent<NPCAnimationAction>().SetDirection(Direction.UP);
                GameComponentData.gameData.fishManager.StartFish();
                break;
        }
        
    }
    void MoveMapZero()
    {
        if (oldPassid != nextPassid)
        {
            MapStartCoordinate mapStartCoordinate =
                passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == oldPassid);
            Vector2Int zeroCoordinate=new Vector2Int();
            if (mapStartCoordinate != null)
            {
                zeroCoordinate = mapStartCoordinate.coordiante;
            }
            else
            {
                MapStartCoordinate mapStartCoordinate1 =
                    passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == 0);
                zeroCoordinate = mapStartCoordinate1.coordiante;
            }
            if (playerCharactor != null)
            {
                playerMoveType = PlayerMoveType.Start;
                Vector3 pos = AStarTest.CoordinateToPos(zeroCoordinate);
                playerCharactor.Obj.transform.position = pos;
                playerCharactor.coordinate = zeroCoordinate;
                playerCharactor.npcAnimationAction.InitPlayerData(playerCharactor, passDataManager.NowPassData.zeroGoldCoordinate);
            }
            else
            {
                CreatPlayer(passDataManager.NowPassData);
                playerMoveType = PlayerMoveType.Start;
                Vector3 pos = AStarTest.CoordinateToPos(zeroCoordinate);
                playerCharactor.Obj.transform.position = pos;
                playerCharactor.coordinate = zeroCoordinate;
                playerCharactor.npcAnimationAction.InitPlayerData(playerCharactor, passDataManager.NowPassData.zeroGoldCoordinate);
            }
            if (!GameData.gameDebugAction.GameStartTest)
            {
                GameData.eventManager.CheckEvents();
            }
            
            GameData.guideController.CheckGuide();
        }
        oldPassid = passDataManager.NowPassData.id;
        SeasonSelect seasonSelect = GameData.mapParent.GetComponentInChildren<SeasonSelect>();
        if (seasonSelect != null)
        {
            Transform loverParent = seasonSelect.LoverTransform;
            if (lover != null && loverParent != null)
            {
                GameObject loverPro = GameComponent.models.Find(m => m.name == lover.npcData.ObjName);
                GameObject loverObj = Instantiate(loverPro, loverParent.position, Quaternion.identity);
                loverObj.SetActive(true);
                loverObj.transform.SetParent(loverParent);
                loverObj.GetComponentInChildren<NPCAnimationAction>().SetDirection(Direction.DOWN);
            }
        }
       
        
        lover = null;
    }

    void MoveMapEnd()
    {
        oldPass0 = oldPassid;
        MapStartCoordinate mapStartCoordinate =
            passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == nextPassid);
        Vector2Int endCoordinate = new Vector2Int();
        if (mapStartCoordinate != null)
        {
            endCoordinate = mapStartCoordinate.coordiante;
        }
        else
        {
            MapStartCoordinate mapStartCoordinate1 =
                passDataManager.NowPassData.ZeroMapStartCoordinates.Find(z => z.mapId == 0);
            endCoordinate = mapStartCoordinate1.coordiante;
        }

        if (nextPassid != oldPassid)
        {
            playerMoveType = PlayerMoveType.End;
            playerCharactor.npcAnimationAction.InitPlayerData(playerCharactor, endCoordinate);
        }
      
        
    }
    public void MoveToShopMap(int _shop)
    {
        AudioController.instance.PlayAudio(SE.click);
        //selecteShop = _shop;
        Shop shop = GameComponentData.gameData.shopManager.Shops.Find(s => s.id == _shop);
        //selecteShop = _shop;
        ReturnFromAdventure();
        oldPassid = passDataManager.NowPassData.id;
        if (oldPassid == 1001)
        {
            GameData.pastureAction.MoveToCenter();
        }
        nextPassid = shop.mapid;
        MoveMapEnd();
    }
    public void MoveToMap(int _nextMapid)
    {
        GameData.pastureAction.MoveCameraButtonObj.SetActive(false);
        AudioController.instance.PlayAudio(SE.click);
        ReturnFromAdventure();

        oldPassid = passDataManager.NowPassData.id;
        nextPassid = _nextMapid;
        if (oldPassid == 1001)
        {
            GameData.pastureAction.MoveToCenter();
        }
        MoveMapEnd();
    }
    public void UpdataMapSeason()
    {
        if (mapX != null)
        {
            SeasonSelect seasonSelect = mapX.GetComponent<SeasonSelect>();
            if (seasonSelect != null)
            {
                seasonSelect.SeasonSetData(GameTimeManager.nowGameTime.gameDate.season);
            }
        }
        
       
    }
    public void InitMyShop()
    {
        if (nowMapid != 1000)
        {
            fieldTool.SetActive(false);
            InitMapData(1000);
            StartCoroutine("InitMyShoping");
        }
        
    }
    public void InitSquare()
    {
        if (nowMapid != 2001)
        {
            fieldTool.SetActive(false);
            if (oldPass0 == 1000)
            {
                GameData.shopGoldDeskAction.ShopingInBack();
            }
            InitMapData(2001);
            StartCoroutine("InitSquaring");
        }
        
    }
    public void InitStreet()
    {
        if (nowMapid != 2000)
        {
            fieldTool.SetActive(false);
            if (oldPass0 == 1000)
            {
                GameData.shopGoldDeskAction.ShopingInBack();
            }
            
            InitMapData(2000);
            StartCoroutine("InitStreeting");
        }
           
    }

    public void SwitchMapData(int _mapid)
    {
        if (nowMapid != _mapid)
        {
            fieldTool.SetActive(false);
            GameData.shopGoldDeskAction.ShopingInBack();
            InitMapData(_mapid);
            StartCoroutine("InitStreeting");
        }
    }
    public void InitGate()
    {
        GameData.shopGoldDeskAction.ShopingInBack();
        if (nowMapid != 2002)
        {
            fieldTool.SetActive(false);
            InitMapData(2002);
            StartCoroutine("InitGating");
        }
            
    }


    public void InitMyFarm()
    {
        if (nowMapid != 1001)
        {
            fieldTool.SetActive(true);
            if (oldPass0 == 1000)
            {
                GameData.shopGoldDeskAction.ShopingInBack();
            }
            InitMapData(1001);
            StartCoroutine("InitFarm");
        }
            
    }
    public void InitPasture()
    {
        if (nowMapid != 1002)
        {
            fieldTool.SetActive(false);
            if (oldPass0 == 1000)
            {
                GameData.shopGoldDeskAction.ShopingInBack();
            }
            InitMapData(1002);


            StartCoroutine("InitPastureMap");
        }

            
        
    }
    public void InitRoom()
    {
        if (nowMapid != 1003)
        {
            fieldTool.SetActive(false);

            
            if (oldPass0 == 1000)
            {
                GameData.shopGoldDeskAction.ShopingInBack();
            }
            InitMapData(1003);
            StartCoroutine("InitRoomMap");
        }
            
        
    }

    
    IEnumerator InitMyShoping()
    {
        yield return new WaitForSeconds(0.1f);
        GameData.shopGoldDeskAction.CreatDesk();
        GameData.shopGoldDeskAction.StartNpcShoping();
        Camera.main.transform.position = new Vector3(0, 0, -10);
        GameData.mapEditAction.isCamreaMove = false;
        mapParent.GetComponentInChildren<SaleAction>().SaleValueText.text =
            GameComponentData.gameData.shopGoldDeskAction.saleValue + "%";
        MoveMapZero();
    }
    IEnumerator InitSquaring()
    {
        yield return new WaitForSeconds(0.1f);
        GameData.mapEditAction.isCamreaMove = false;
        Camera.main.transform.position = new Vector3(0, 0, -10);
        MoveMapZero();
    }
    IEnumerator InitStreeting()
    {
        yield return new WaitForSeconds(0.1f);
        GameData.mapEditAction.isCamreaMove = false;
        Camera.main.transform.position = new Vector3(0, 0, -10);
        MoveMapZero();
    }
    IEnumerator InitFarm()
    {

        yield return new WaitForSeconds(0.1f);
        GameData.farmAction.InitFarm();
        GameData.mapEditAction.isCamreaMove = false;
        Camera.main.transform.position= new Vector3(0, 0, -10);
        MoveMapZero();
    }
    
    IEnumerator InitPastureMap()
    {
        yield return new WaitForSeconds(0.1f);
        //GameData.mapEditAction.isCamreaMove = true;
        Camera.main.transform.position = new Vector3(0, 0, -10);
        PastureNameClick pastureNameClick = mapParent.GetComponentInChildren<PastureNameClick>();
       
        pastureNameClick.InitPastureName();
        GameData.pastureAction.InitPastureData(mapParent.GetChild(0).GetChild(0).GetChild(3));
        GameData.pastureAction.DisplayAnimal();
        MoveMapZero();
    }

    IEnumerator InitRoomMap()
    {
        yield return new WaitForSeconds(0.1f);
        GameData.mapEditAction.isCamreaMove = false;
        Camera.main.transform.position = new Vector3(0, 0, -10);
        GameComponentData.gameData.equipmentManager.DisplayEuqiqment();
        CheckChild();
        MoveMapZero();
    }

    public void CheckChild()
    {
        mapParent.GetComponentInChildren<GameBoxClickAction>().CheckChild();
    }
    public void CheakPlayerMoveEnd()
    {
        if (playerMoveType == PlayerMoveType.End)
        {
            

        }
        if (playerMoveType == PlayerMoveType.Start)
        {

           
            if (IsYueHui && GameData.gameManager.passDataManager.NowPassData.id == 2010)
            {
                GameData.loveAction.YueAction();
                IsYueHui = false;
            }
        }

        
    }
    public void CameraSelect(GameObject selectButton)
    {
        AudioController.instance.PlayAudio(SE.select);
        cameraSelectNum++;
        if (cameraSelectNum > 3)
        {
            cameraSelectNum = 1;
        }
        switch (cameraSelectNum)
        {
            case 1:
                selectButton.GetComponentInChildren<Text>().text = "视角：近";
                Camera.main.orthographicSize = 3.6f;


               

                break;
            case 2:
                selectButton.GetComponentInChildren<Text>().text = "视角：中";
                Camera.main.orthographicSize = 4.3f;

                break;
            case 3:
                selectButton.GetComponentInChildren<Text>().text = "视角：远";
                Camera.main.orthographicSize = 5.0f;

                break;
            default:
                break;
        }
        cameraMove.SetBoundary();
    }
    public Boundary CreatBoundary()
    {
        float screenWith = Screen.width;
        float screenHight = Screen.height;
        float mapWith = mapSize.x;
        float mapHight = mapSize.y;
        mapStartPos = new Vector2(-mapWith/200.0f, -mapHight/200.0f);
        mapEndPos = new Vector2(mapWith / 200.0f, mapHight / 200.0f);
        
        float cameraSize = Camera.main.orthographicSize;
        float dy = mapHight / 200.0f - cameraSize;
        float dx = mapWith / 200.0f - screenWith * cameraSize / screenHight;
        
        Boundary boundary = new Boundary
        {
            MaxX = dx,
            MaxY = dy,
            MinX = -dx,
            MinY = -dy - 0.8f
        };
        nowBoundary = boundary;
        return boundary;
    }
    void CreatTile()
    {
        foreach (Transform child in TileMaskPerent)
        {
            Destroy(child.gameObject);
        }
        AStarTest.CreatTile(tile,mapX.transform);
        AStarTest.CreatMaskTile(tilemask,TileMaskPerent);
    }
    //手动移动结束前
    public void StopdDirectDisplay()
    {
        StopCoroutine("DirectorCellColorChanging");
        Cell[] cells = AStarTest.cellList;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].SetTileColor(new Color(0, 0, 0, 0));
        }
    }
    public void CellsWhite()
    {
        StopCoroutine("DirectorCellColorChanging");
        Cell[] cells = AStarTest.cellList;
        for (int i=0;i<cells.Length;i++)
        {
            cells[i].InitCell(); 
        }
        
    }
    
    public void AddPackageCase(PackageType packageType)
    {
        switch (packageType)
        {
            case PackageType.背包:
                int costValue = (pakageAddData.zeroCost/2) * (1 + packageLevel)*(1+packageLevel)+ (pakageAddData.zeroCost / 2) * (1 + packageLevel);
                InitCostData(LanguageManage.SwitchStr("增加格子"),costValue,LanguageManage.SwitchStr("增加5格背包格子"),CostType.增加背包格子, ShopMoneyType.金币);
                break;
            case PackageType.杂物箱:
                int costValue1 = (boxAddData.zeroCost/2) * (1 + boxLevel) * (1 + boxLevel) + (boxAddData.zeroCost / 2) * (1 + boxLevel);
                InitCostData(LanguageManage.SwitchStr("增加格子"), costValue1, LanguageManage.SwitchStr("增加5格杂物箱格子"), CostType.增加杂物箱格子, ShopMoneyType.金币);
                break;
            case PackageType.冰箱:
                int costValue2 = (iceBoxAddData.zeroCost/2) * (1 + iceBoxLevel) * (1 + iceBoxLevel)+ (iceBoxAddData.zeroCost / 2) * (1 + iceBoxLevel);
                InitCostData(LanguageManage.SwitchStr("增加格子"), costValue2, LanguageManage.SwitchStr("增加5格冰箱格子"), CostType.增加冰箱格子, ShopMoneyType.金币);
                break;
        }
    }

    public void AddPackageAction(PackageType packageType)
    {
        switch (packageType)
        {
            case PackageType.背包:
                packageCount = gamePlayer.packageZeroCout + packageLevel * pakageAddData.caseAdd;
                PackageManager.instance.SetPackageCaseCount(0, packageCount);

                packageLevel++;
                GameComponentData.gameData.warehouseAction.AddNullCase(packageType);
                break;
            case PackageType.杂物箱:
                boxCount = gamePlayer.boxZeroCount + boxLevel * boxAddData.caseAdd;
                PackageManager.instance.SetPackageCaseCount(1, boxCount);
                boxLevel++;
                GameComponentData.gameData.warehouseAction.AddNullCase(packageType);
                break;
            case PackageType.冰箱:
                iceBoxCount = gamePlayer.iceboxZeroCount + iceBoxLevel * iceBoxAddData.caseAdd;
                PackageManager.instance.SetPackageCaseCount(2, iceBoxCount);
                iceBoxLevel++;
                GameComponentData.gameData.warehouseAction.AddNullCase(packageType);
                break;
        }
    }
   
    // Update is called once per frame
    void Update()
    {
        
	}
}
