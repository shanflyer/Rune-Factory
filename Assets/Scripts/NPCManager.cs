using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

[System.Serializable]
public enum BeahaveType
{
    静待=0,
    巡逻=1
}
[System.Serializable]
public  class BeahaveData
{
    public BeahaveType beahaveType;
    public int MapId;
    public Vector2Int RangePos0;
    public Vector2Int RangePos1;
}

[System.Serializable]
public enum NpcStatus
{
    正常=1,
    修养中=2
}


[System.Serializable]
public class NPCStrData
{

    public string name;
    public int id;
    public int professionId;
    public Gender sex;
    public int level;
    public int exp;
    public int hp;
    public NpcStatus npcStatus;
    public string ObjName,marriedObj;
    public string ImageName;
    public string headName;
    public Season brothSeason;
    public int brothDate;
    public int nowAge, deathAge;
    public string beahaveData;
    public int friendlyLevel;
    public int frienflyExp;
    public string NoticeText;
    public int zeroTalk,loveZeroTalk;
    public int failureTalk, successfulTalk;
    public int teamSuccessfulTalk, teamfailureTalk0, teamfailureTalk1;
    public string LoveHobby, LikeHobby;
    public string brothDayGift;
    public int loveGiftTalk, likeGiftTalk, normalGiftTalk,flowerTalk,noFlowerTalk, leaveTalk;
    public string normalTalks;
    public int brothDayTalk;
    public AttributeType attributeType;
    public int skill;
    public NPCStrData() { }

    public NPCStrData(NPCData npcData)
    {
        skill = npcData.skill;
        name = npcData.name;
        id = npcData.id;
        attributeType = npcData.attributeType;
        professionId = npcData.professionId;
        sex = npcData.sex;
        level = npcData.level;
        exp = npcData.exp;
        hp = npcData.hp;
        npcStatus = npcData.npcStatus;
        ObjName = npcData.ObjName;
        marriedObj = npcData.marriedObj;
        ImageName = npcData.ImageName;
        headName = npcData.headName;
        brothSeason = npcData.brothSeason;
        brothDate = npcData.brothDate;
        nowAge = npcData.nowAge;
        deathAge = npcData.deathAge;
        beahaveData = "";
        beahaveData += npcData.beahaveData.beahaveType + ";" + npcData.beahaveData.MapId + ";" +
                       npcData.beahaveData.RangePos0.x + "," + npcData.beahaveData.RangePos0.y +
                       ";" + npcData.beahaveData.RangePos1.x + "," + npcData.beahaveData.RangePos1.y;
        friendlyLevel = npcData.friendlyLevel;
        frienflyExp = npcData.frienflyExp;
        NoticeText = npcData.NoticeText;
        zeroTalk = npcData.zeroTalk;
        loveZeroTalk = npcData.loveZeroTalk;
        failureTalk = npcData.failureTalk;
        successfulTalk = npcData.successfulTalk;
        teamSuccessfulTalk = npcData.teamSuccessfulTalk;
        teamfailureTalk0 = npcData.teamfailureTalk0;
        teamfailureTalk1 = npcData.teamfailureTalk1;
        loveGiftTalk = npcData.loveGiftTalk;
        likeGiftTalk = npcData.likeGiftTalk;
        leaveTalk = npcData.leaveTalk;
        normalGiftTalk = npcData.normalGiftTalk;
        flowerTalk = npcData.flowerTalk;
        noFlowerTalk = npcData.noFlowerTalk;
        LoveHobby = "";
        foreach (var i in npcData.LoveHobby)
        {
            LoveHobby += i + ",";
        }
        LikeHobby = "";
        foreach (var i in npcData.LikeHobby)
        {
            LikeHobby += i + ",";
        }

        normalTalks = "";
        foreach (var npcDataNormalTalk in npcData.normalTalks)
        {
            normalTalks += npcDataNormalTalk+ ",";
        }
        brothDayGift = npcData.brothDayGift;
        brothDayTalk = npcData.brothDayTalk;
    }
}
[System.Serializable]
public class NPCData
{
    public string name;
    public int id;
    public int professionId;
    public int level;
    public int exp;
    public int hp;
    public Gender sex;
    public string ObjName, marriedObj;
    public string ImageName;
    public string headName;
    public Season brothSeason;
    public int brothDate;
    public int nowAge, deathAge;
    public BeahaveData beahaveData;
    public int friendlyLevel;
    public NpcStatus npcStatus;
    public int frienflyExp;
    public string NoticeText;
    public int zeroTalk,loveZeroTalk;
    public int failureTalk, successfulTalk;
    public int teamSuccessfulTalk, teamfailureTalk0, teamfailureTalk1;
    public List<int> LoveHobby, LikeHobby;
    public string brothDayGift;
    public int loveGiftTalk, likeGiftTalk, normalGiftTalk, leaveTalk;
    public int flowerTalk, noFlowerTalk;
    public List<int> normalTalks;
    public int brothDayTalk;
    public AttributeType attributeType;
    public int skill;
    public NPCData() { }

    public NPCData(NPCStrData npcStrData)
    {
        skill = npcStrData.skill;
        attributeType = npcStrData.attributeType;
        npcStatus=npcStrData.npcStatus;
        name = npcStrData.name;
        id = npcStrData.id;
        sex = npcStrData.sex;
        professionId = npcStrData.professionId;
        level = npcStrData.level;
        exp = npcStrData.exp;
        hp = npcStrData.hp;
        ObjName = npcStrData.ObjName;
        marriedObj = npcStrData.marriedObj;
        ImageName = npcStrData.ImageName;
        headName = npcStrData.headName;
        brothSeason = npcStrData.brothSeason;
        brothDate = npcStrData.brothDate;
        nowAge = npcStrData.nowAge;
        deathAge = npcStrData.deathAge;
        beahaveData=new BeahaveData();
        var x = npcStrData.beahaveData.Split(';');
        beahaveData.beahaveType = (BeahaveType) int.Parse(x[0]);
        beahaveData.MapId = int.Parse(x[1]);
        Vector2Int range0=Vector2Int.zero;
        Vector2Int range1 = Vector2Int.zero;
        range0.x = int.Parse(x[2].Split(',')[0]);
        range0.y = int.Parse(x[2].Split(',')[1]);
        range1.x = int.Parse(x[3].Split(',')[0]);
        range1.y = int.Parse(x[3].Split(',')[1]);
        beahaveData.RangePos0 = range0;
        beahaveData.RangePos1 = range1;
        friendlyLevel = npcStrData.friendlyLevel;
        frienflyExp = npcStrData.frienflyExp;
        NoticeText = npcStrData.NoticeText;
        zeroTalk = npcStrData.zeroTalk;
        loveZeroTalk = npcStrData.loveZeroTalk;
        failureTalk = npcStrData.failureTalk;
        successfulTalk = npcStrData.successfulTalk;
        teamSuccessfulTalk = npcStrData.teamSuccessfulTalk;
        teamfailureTalk0 = npcStrData.teamfailureTalk0;
        teamfailureTalk1 = npcStrData.teamfailureTalk1;
        loveGiftTalk = npcStrData.loveGiftTalk;
        likeGiftTalk = npcStrData.likeGiftTalk;
        normalGiftTalk = npcStrData.normalGiftTalk;
        flowerTalk = npcStrData.flowerTalk;
        noFlowerTalk = npcStrData.noFlowerTalk;
        leaveTalk = npcStrData.leaveTalk;
        LoveHobby =new List<int>();
        foreach (var s in npcStrData.LoveHobby.Split(','))
        {
            if (s != "")
            {
                LoveHobby.Add(int.Parse(s));
            }
            
        }
        LikeHobby = new List<int>();
        foreach (var s in npcStrData.LikeHobby.Split(','))
        {
            if (s != "")
                LikeHobby.Add(int.Parse(s));
        }

        var str = npcStrData.normalTalks.Split(',');
        normalTalks=new List<int>();
        foreach (var s in str)
        {
           
            normalTalks.Add(int.Parse(s));
        }
        brothDayTalk = npcStrData.brothDayTalk;
        brothDayGift = npcStrData.brothDayGift;
    }

}
public class NPCX : Charactor
{
    public NPCData npcData;
    public int WaitDays;
    public int weapon, clothes;
    public bool isLove, isMarried,isYuehui;
    public bool isPlayerBrothDay,isNpcBrothDay;
    [HideInInspector]
    public bool isFriendlyExpAdd;
    [HideInInspector]
    public bool istteamExpAdd,isGiftExpAdd0, isGiftExpAdd1, isGiftExpAdd2,isFlower;
    public NPCX(NPCData _npcData):base(_npcData.id,_npcData.name,null,_npcData.beahaveData.MapId,
        new Vector2Int(Random.Range(_npcData.beahaveData.RangePos0.x, _npcData.beahaveData.RangePos1.x), Random.Range(_npcData.beahaveData.RangePos0.y, _npcData.beahaveData.RangePos1.y)),
        CharactorDataAction.professionDatas.Find(p=>p.id==_npcData.professionId),_npcData.level)
    {
        npcData = _npcData;
       
    }
    public void AddGiftFriendllyExp(int itemid)
    {
        ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(itemid);
        if (itemData.Id == 1502&&WaitDays > 0)
        {
            WaitDays = 0;
            GameComponentData.gameData.talkTextsManager.TalkAction("1600", npcData.headName, Name, this, TalkActionType.普通);
        }
        else
        {
            if (itemData.Type == ItemType.花)
            {
                if (isFlower)
                {
                    GameComponentData.gameData.talkTextsManager.TalkAction(npcData.noFlowerTalk.ToString(), npcData.headName, Name, this, TalkActionType.普通);
                }
                else
                {
                    isFlower = true;
                    GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(itemid, 1);
                    int expValue = itemData.typeValue;
                    AddFriendlyexp(expValue);
                    GameComponentData.gameData.talkTextsManager.TalkAction(npcData.flowerTalk.ToString(), npcData.headName, Name, this, TalkActionType.普通);
                }
            }
            else
            {
                GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(itemid, 1);
                int expValue = 2;
                FestivalData festivalData = GameComponentData.gameData.festivalManager.FestivalDatas.Find(f => f.id == npcData.id);
                if (festivalData.season == GameTimeManager.nowGameTime.gameDate.season &&
                    festivalData.date == GameTimeManager.nowGameTime.gameDate.date)
                {
                    expValue = 4;
                }
                if (npcData.LoveHobby.Contains(itemData.Id))
                {
                    if (!isGiftExpAdd0)
                    {
                        AddFriendlyexp(6 * expValue);
                        isGiftExpAdd0 = true;
                    }
                    else
                    {
                        AddFriendlyexp(2 * expValue);
                    }
                    GameComponentData.gameData.talkTextsManager.TalkAction(npcData.loveGiftTalk.ToString(), npcData.headName, Name);
                }
                else if (npcData.LikeHobby.Contains(itemData.Id))
                {
                    if (!isGiftExpAdd1)
                    {
                        AddFriendlyexp(3 * expValue);
                        isGiftExpAdd1 = true;
                    }
                    else
                    {
                        AddFriendlyexp(1 * expValue);
                    }
                    GameComponentData.gameData.talkTextsManager.TalkAction(npcData.likeGiftTalk.ToString(), npcData.headName, Name);
                }
                else
                {
                    if (!isGiftExpAdd2)
                    {
                        AddFriendlyexp(1 * expValue);
                        isGiftExpAdd2 = true;
                    }
                    else
                    {
                    }
                    GameComponentData.gameData.talkTextsManager.TalkAction(npcData.normalGiftTalk.ToString(), npcData.headName, Name);
                }

            }

        }


    }
    public void AddFriendlyexp(int Friendlyexp)
    {
        npcData.frienflyExp += Friendlyexp;
        int maxExp = GameComponentData.gameData.NpcManager.zeroFriendUpExp +
                     npcData.friendlyLevel * GameComponentData.gameData.NpcManager.AddUpExp;
        while (npcData.frienflyExp>=maxExp)
        {
           
            npcData.friendlyLevel++;
           
            npcData.frienflyExp -= maxExp;
            maxExp = GameComponentData.gameData.NpcManager.zeroFriendUpExp +
                     npcData.friendlyLevel * GameComponentData.gameData.NpcManager.AddUpExp;
        }
        GameComponentData.gameData.charactorTitleAction.CheckFriendly();
        InformationController.instance.AddInformation("*" + Name + LanguageManage.SwitchStr(" 友好度等级为") + npcData.friendlyLevel);
        InformationController.instance.AddInformation("*"+Name+ LanguageManage.SwitchStr(" 友好度增加") +Friendlyexp);
       
    }

   
}
public class NPCManager : MonoBehaviour
{
    public int marriedMapId;
    public Vector2Int marriedCoordinated;
    public Vector2 waitTime;
    public List<NPCData> NpcDatas;
    private List<NPCStrData> NpcStrDatas;
    public List<NPCX> Npcxs;
    [HideInInspector]
    public NPCX selectNpcx;
    public int zeroFriendUpExp, AddUpExp;
	// Use this for initialization
    public bool isVisit;
	void Start ()
	{
	    

	}
    public void DataToJson()
    {
        NpcStrDatas=new List<NPCStrData>();
        foreach (var npcData in NpcDatas)
        {
            NPCStrData npcStrData=new NPCStrData(npcData);
            NpcStrDatas.Add(npcStrData);
        }

        string filePath = Application.dataPath + @"/Resources/Datas/NpcDatas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(NpcStrDatas);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset fileText = Resources.Load<TextAsset>("Datas/" + "NpcDatas");
        if (fileText == null)
        {
            // Debug.LogError("No" + "ArmData");
        }
        else
        {
            string jsonStr = fileText.text;
            NpcStrDatas=new List<NPCStrData>();
            NpcStrDatas = JsonMapper.ToObject<List<NPCStrData>>(jsonStr);
            NpcDatas=new List<NPCData>();
            foreach (var npcStrData in NpcStrDatas)
            {
               NPCData npcData=new NPCData(npcStrData);
                npcData.name = LanguageManage.SwitchStr(npcData.name);
                npcData.NoticeText = LanguageManage.SwitchStr(npcData.NoticeText);
               NpcDatas.Add(npcData);
            }

        }
    }
    public void UpdataDate()
    {
        foreach (var npcx in Npcxs)
        {
            npcx.isFriendlyExpAdd = false;
            npcx.isGiftExpAdd0 = false;
            npcx.isGiftExpAdd1 = false;
            npcx.isGiftExpAdd2 = false;
            npcx.istteamExpAdd = false;
            npcx.isFlower = false;
            npcx.isYuehui = false;
            if (npcx.npcData.npcStatus == NpcStatus.修养中)
            {
                npcx.WaitDays--;
                if (npcx.WaitDays <= 0)
                {
                    npcx.npcData.npcStatus = NpcStatus.正常;

                }
            }
            npcx.property.HP = npcx.property.MaxHP;

            if (GameComponentData.gameData.employerManger.Employers != null)
            {
                Employer employer = GameComponentData.gameData.employerManger.Employers.Find(e => e.id == npcx.id);
                if (employer != null)
                {
                    employer.property = npcx.property;
                }
            }
           
          
        }
    }
    public void InitData()
    {
        JsonToData();
        Npcxs=new List<NPCX>();
        foreach (var npcData in NpcDatas)
        {
            NPCX npcx = new NPCX(npcData);
            Npcxs.Add(npcx);
            Property property = npcx.professionData.ZeroProperty + npcx.professionData.GetPropertyFromLevel(npcx.level);
            npcx.property = property;
        }
        
    }
    public void VisitNPC(NPCX _selectNpcx)
    {
        GameComponentData.gameData.NpcListDataPanelAction.gameObject.SetActive(false);
        GameComponentData.gameData.intelligencePanelAction.gameObject.SetActive(false);
        GameComponentData.gameData.gameManager.MoveToMap(_selectNpcx.npcData.beahaveData.MapId);
        selectNpcx = _selectNpcx;
        isVisit = true;
    }
    public void VisitNPC(int npcId)
    {
        NPCData npcData = NpcDatas.Find(n => n.id == npcId);
        GameComponentData.gameData.NpcListDataPanelAction.gameObject.SetActive(false);
        GameComponentData.gameData.intelligencePanelAction.gameObject.SetActive(false);
        GameComponentData.gameData.gameManager.MoveToMap(npcData.beahaveData.MapId);
        ZeroTalkAction(Npcxs.Find(n=>n.id==npcData.id));
    }
    public void SetMapNpc(int mapid)
    {
        foreach (var npcx in Npcxs)
        {
            if (npcx.npcData.beahaveData.MapId == mapid)
            {
                if (npcx.Obj == null)
                {
                    GameObject objpro = Resources.Load<GameObject>("charactor/" + npcx.npcData.ObjName);
                    Vector3 pos = AStarTest.CoordinateToPos(npcx.coordinate);
                    Cell cell = AStarTest.GetCellWithCoordinate(npcx.coordinate);
                    cell.myGameObjects.Add(npcx);
                    npcx.Obj = Instantiate(objpro,pos,Quaternion.identity);
                    npcx.Obj.transform.SetParent(GameComponentData.gameData.NpcParent);
                }
                else
                {
                    npcx.Obj.SetActive(true);
                }
                if (npcx.npcData.beahaveData.beahaveType == BeahaveType.静待)
                {
                  npcx.Obj.GetComponent<NPCAnimationAction>().SetDirection(Direction.DOWN);
                }
                else
                {
                    List<Vector2Int> goldVector2Ints=new List<Vector2Int>();
                    for (int x = npcx.npcData.beahaveData.RangePos0.x; x <= npcx.npcData.beahaveData.RangePos1.x; x++)
                    {
                        for (int y = npcx.npcData.beahaveData.RangePos0.y; y <= npcx.npcData.beahaveData.RangePos1.y; y++)
                        {
                            Vector2Int vector2Int = new Vector2Int(x, y);
                            goldVector2Ints.Add(vector2Int);
                        }
                    }
                    if (npcx.Obj != null)
                        npcx.Obj.GetComponent<NPCAnimationAction>().StartNPCMoving(goldVector2Ints,npcx.coordinate,npcx);
                }
            }
            else
            {
                if (npcx.Obj != null)
                {
                    npcx.Obj.SetActive(false);
                }
                
            }
        }
    }

    public void SetMarriedNpcPos(NPCX npcx)
    {
        var shops = GameComponentData.gameData.shopManager.Shops;
        Shop shop = shops.Find(s => s.Npcid == npcx.npcData.id);
        if (shop != null)
        {
            NPCX npcx0 = Npcxs.Find(n => n.id == 3012);
            shop.Npcid = 3012;
            npcx0.npcData.beahaveData.MapId = npcx.npcData.beahaveData.MapId;
            npcx0.npcData.beahaveData.beahaveType = npcx.npcData.beahaveData.beahaveType;
            npcx0.npcData.beahaveData.RangePos0 = npcx.npcData.beahaveData.RangePos0;
            npcx0.npcData.beahaveData.RangePos1 = npcx.npcData.beahaveData.RangePos1;
            npcx0.mapId = npcx.mapId;
            npcx0.coordinate = npcx.coordinate;
        }

        npcx.npcData.beahaveData.MapId = marriedMapId;
        npcx.npcData.beahaveData.beahaveType=BeahaveType.静待;
        npcx.npcData.beahaveData.RangePos0 = marriedCoordinated;
        npcx.npcData.beahaveData.RangePos1 = marriedCoordinated;
        npcx.mapId = marriedMapId;
        npcx.coordinate = marriedCoordinated;

        
        
    }
    public void MoveEndTalk()
    {
        if (isVisit)
        {
            ZeroTalkAction(selectNpcx);
        }
        isVisit = false;
    }
    public void ZeroTalkAction(int mapId,Vector2 coordinate)
    {
        selectNpcx = Npcxs.Find(n =>n.mapId==mapId&&n.coordinate == coordinate);

        if (selectNpcx != null)
        {
            AudioController.instance.PlayAudio(SE.click);
            GameComponentData.gameData.talkTextsManager.NPCZeroTalkAction(selectNpcx);
        }
        
    }
    public void ZeroTalkAction(NPCX selectNpcx)
    {
        if (selectNpcx != null)
        {
            GameComponentData.gameData.talkTextsManager.NPCZeroTalkAction(selectNpcx);
        }

    }

    // Update is called once per frame
    void Update () {
		
	}
}
