using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
public enum TalkActionType
{
    普通 = 0,
    剧情 = 1,
    触发器 = 2,
    结婚 = 3,
    约会 = 4,
    约会中0 = 5,
    约会中2 = 6,
    指引 = 7
}

[System.Serializable]
public class TalkTextData
{
    public int talkId;
    public string charactorName;
    public string charactorIcon;
    public int nexTalkId;
    public string text;
    public TalkTextData() { }
}
public class TalkTextsManager : MonoBehaviour
{
    public GameObject NPCFunctionPanel;
    public List<TalkTextData> talkTextDatas;
    public GameObject TalkObj;
    public Text TalkName;
    public Image Face;
    public Text TalkText;
    public GameObject normal, love;
    [HideInInspector]
    public TalkTextData nowTalk;
    private string faceIcon;
    private string TalkTitleName;
    public bool isTalkEnd;
    private bool isPlayertalk;
    private NPCX talkNpc;
    private TalkActionType talkActionType;
    public void DataToJson()
    {
        string path = Application.dataPath + @"/Resources/Datas/talks.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(talkTextDatas);
        FileStream fileStream = new FileStream(path, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset fileText = Resources.Load<TextAsset>("Datas/talks");
        if (fileText == null)
        {

            talkTextDatas.Clear();
        }
        else
        {
            string jsonStr = fileText.text;
            talkTextDatas = JsonMapper.ToObject<List<TalkTextData>>(jsonStr);
            foreach (var talkTextData in talkTextDatas)
            {
                talkTextData.text = LanguageManage.SwitchStr(talkTextData.text);
                talkTextData.charactorName = LanguageManage.SwitchStr(talkTextData.charactorName);
            }
        }
    }

    void DisplayTalk()
    {
        if (talkNpc != null)
        {
            love.SetActive(talkNpc.isLove);
            normal.SetActive(!talkNpc.isLove);
            Face.sprite = GameComponent.headIcons.Find(icon => icon.name == talkNpc.npcData.headName);
            Face.enabled = true;
            TalkName.enabled = true;
            TalkName.text = talkNpc.npcData.name;
        }
        else
        {
            Face.sprite = GameComponent.headIcons.Find(icon => icon.name == nowTalk.charactorIcon);
            Face.enabled = true;
            TalkName.enabled = true;
            TalkName.text = nowTalk.charactorName;
            love.SetActive(false);
            normal.SetActive(true);
        }
       
        TalkObj.SetActive(true);
       
        
        TalkText.text = nowTalk.text;
       
    }


    
    public void PleaseMarried(NPCX npcx)
    {
        TalkAction("1482",npcx.npcData.headName,npcx.Name,npcx,TalkActionType.结婚);
        talkNpc = npcx;
    }
    // Use this for initialization
    public void TalkAction(string talkId, TalkActionType _talkActionType)
    {
        if (talkId != "")
        {

            if (talkTextDatas.Exists(t => t.talkId == int.Parse(talkId)))
            {
                talkActionType = _talkActionType;
                isTalkEnd = false;
                nowTalk = talkTextDatas.Find(t => t.talkId == int.Parse(talkId));

                // talkNpc = null;
                // isPlayertalk = true;
                if (nowTalk.charactorName == "player")
                {
                    nowTalk.charactorName = PlayerDate.playerName;
                }
                if (nowTalk.charactorIcon == "playerIcon")
                {
                    faceIcon = nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;

                }
                if (nowTalk.charactorName == "meal")
                {
                    if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.male)
                    {
                        nowTalk.charactorName = GameComponentData.gameData.gameManager.gamePlayer.name;
                        nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                    }
                    else
                    {
                        NPCX mealNpcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried);
                        nowTalk.charactorName = mealNpcx.npcData.name;
                        nowTalk.charactorIcon = mealNpcx.npcData.headName;
                    }
                }
                if (nowTalk.charactorName == "feMeal")
                {
                    if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.female)
                    {
                        nowTalk.charactorName = GameComponentData.gameData.gameManager.gamePlayer.name;
                        nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                    }
                    else
                    {
                        NPCX mealNpcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried);
                        nowTalk.charactorName = mealNpcx.npcData.name;
                        nowTalk.charactorIcon = mealNpcx.npcData.headName;
                    }
                }

                faceIcon = nowTalk.charactorIcon;
                TalkTitleName = nowTalk.charactorName;
                DisplayTalk();

            }
            else
            {
                isTalkEnd = true;
                //Debug.Log("没有对话" + talkId + "nowTalk:" + nowTalk.talkId);
            }
        }

    }
    public void TalkAction(string talkId,NPCX _npcx)
    {
        talkNpc = _npcx;
        if (talkId != "")
        {
            isPlayertalk = true;
            if (talkTextDatas.Exists(t => t.talkId == int.Parse(talkId)))
            {
               

                isTalkEnd = false;
                nowTalk = talkTextDatas.Find(t => t.talkId == int.Parse(talkId));
               


                if (nowTalk.charactorName == "")
                {
                    TalkTitleName = PlayerDate.playerName;
                }
                else
                {
                    TalkTitleName = nowTalk.charactorName;
                }
                if (nowTalk.charactorIcon == "")
                {
                    faceIcon = nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                }
                else
                {
                    faceIcon = nowTalk.charactorIcon;
                }
                

                if (nowTalk.charactorName == "player")
                {
                    nowTalk.charactorName = PlayerDate.playerName;
                }
                if (nowTalk.charactorIcon == "playerIcon")
                {
                    faceIcon = nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;

                }
                if (nowTalk.charactorName == "meal")
                {
                    if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.male)
                    {
                        nowTalk.charactorName = GameComponentData.gameData.gameManager.gamePlayer.name;
                        nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                    }
                    else
                    {
                        NPCX mealNpcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried);
                        nowTalk.charactorName = mealNpcx.npcData.name;
                        nowTalk.charactorIcon = mealNpcx.npcData.headName;
                    }
                }
                if (nowTalk.charactorName == "feMeal")
                {
                    if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.female)
                    {
                        nowTalk.charactorName = GameComponentData.gameData.gameManager.gamePlayer.name;
                        nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                    }
                    else
                    {
                        NPCX mealNpcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried);
                        nowTalk.charactorName = mealNpcx.npcData.name;
                        nowTalk.charactorIcon = mealNpcx.npcData.headName;
                    }
                }
                DisplayTalk();
            }
            else
            {
                isTalkEnd = true;
                //Debug.Log("没有对话" + talkId + "nowTalk:" + nowTalk.talkId);
            }
        }
        
    }
    public void NPCZeroTalkAction(NPCX npcx)
    {
        isPlayertalk = false;
        talkNpc = npcx;
        faceIcon = npcx.npcData.headName;
        TalkTitleName = npcx.Name;
        FestivalData playerFestivalData =
            GameComponentData.gameData.festivalManager.FestivalDatas.Find(f => f.id == 8);
        if (playerFestivalData.season==GameTimeManager.nowGameTime.gameDate.season&&playerFestivalData.date==
            GameTimeManager.nowGameTime.gameDate.date)
        {
            isTalkEnd = false;
            
            nowTalk = talkTextDatas.Find(t => t.talkId == npcx.npcData.brothDayTalk);
            talkActionType=TalkActionType.普通;
            ;
            npcx.isPlayerBrothDay = true;
            DisplayTalk();
        }
        else
        {
            if (talkTextDatas.Exists(t => t.talkId == npcx.npcData.zeroTalk))
            {
                isTalkEnd = false;
                if (npcx.isMarried)
                {
                    nowTalk = talkTextDatas.Find(t => t.talkId == 1508);
                }
                else if (npcx.isLove)
                {
                    nowTalk = talkTextDatas.Find(t => t.talkId == npcx.npcData.loveZeroTalk);
                }
                else
                {
                    nowTalk = talkTextDatas.Find(t => t.talkId == npcx.npcData.zeroTalk);
                }
                talkActionType = TalkActionType.普通;
                DisplayTalk();

            }
            else
            {
                isTalkEnd = true;
                //Debug.Log("没有对话" + talkId + "nowTalk:" + nowTalk.talkId);
            }
        }

        

    }
    public void TalkAction(string talkId, string iconName, string _talkTitle,NPCX _npcx,TalkActionType _talkActionType)
    {
        if (talkId != "")
        {
            talkNpc = _npcx;
            isPlayertalk = true;
            faceIcon = iconName;
            TalkTitleName = _talkTitle;
            talkActionType = _talkActionType;
            if (talkTextDatas.Exists(t => t.talkId == int.Parse(talkId)))
            {
                isTalkEnd = false;
                nowTalk = talkTextDatas.Find(t => t.talkId == int.Parse(talkId));

                if (nowTalk.charactorName == "player")
                {
                    nowTalk.charactorName = PlayerDate.playerName;
                }
                if (nowTalk.charactorIcon == "playerIcon")
                {
                    faceIcon = nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;

                }
                if (nowTalk.charactorName == "meal")
                {
                    if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.male)
                    {
                        nowTalk.charactorName = GameComponentData.gameData.gameManager.gamePlayer.name;
                        nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                    }
                    else
                    {
                        NPCX mealNpcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried);
                        nowTalk.charactorName = mealNpcx.npcData.name;
                        nowTalk.charactorIcon = mealNpcx.npcData.headName;
                    }
                }
                if (nowTalk.charactorName == "feMeal")
                {
                    if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.female)
                    {
                        nowTalk.charactorName = GameComponentData.gameData.gameManager.gamePlayer.name;
                        nowTalk.charactorIcon = GameComponentData.gameData.gameManager.gamePlayer.IconName;
                    }
                    else
                    {
                        NPCX mealNpcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried);
                        nowTalk.charactorName = mealNpcx.npcData.name;
                        nowTalk.charactorIcon = mealNpcx.npcData.headName;
                    }
                }
                DisplayTalk();

            }
            else
            {
                isTalkEnd = true;
                //Debug.Log("没有对话" + talkId + "nowTalk:" + nowTalk.talkId);
            }
        }

    }
    public void TalkAction(string talkId, string iconName,string _talkTitle)
    {
        if (talkId != "")
        {
            talkActionType=TalkActionType.普通;
            
            talkNpc = null;
            isPlayertalk = true;
            faceIcon = iconName;
            TalkTitleName = _talkTitle;
            if (talkTextDatas.Exists(t => t.talkId == int.Parse(talkId)))
            {
                isTalkEnd = false;
                nowTalk = talkTextDatas.Find(t => t.talkId == int.Parse(talkId));

                if (iconName != null)
                {
                    nowTalk.charactorIcon = iconName;
                }
                if (_talkTitle != null)
                {
                    nowTalk.charactorName = _talkTitle;
                }
                DisplayTalk();

            }
            else
            {
                isTalkEnd = true;
                //Debug.Log("没有对话" + talkId + "nowTalk:" + nowTalk.talkId);
            }
        }

    }
    public void NextTalk()
    {
        AudioController.instance.PlayAudio(SE.Return);
        if (nowTalk.nexTalkId != 0)
        {
            
            TalkAction(nowTalk.nexTalkId.ToString(),null);
        }
        else
        {

            switch (talkActionType)
            {
                case TalkActionType.剧情:
                    GameComponentData.gameData.filmManager.PlayNowFilm();
                    break;
                case TalkActionType.普通:
                    break;
               case TalkActionType.结婚:
                    GameComponentData.gameData.loveAction.MarriedAction(talkNpc);
                    break;
                case TalkActionType.约会:
                    GameComponentData.gameData.loveAction.YueHuiEndTalkAction();
                    break;
                case TalkActionType.约会中0:
                    GameComponentData.gameData.loveAction.YueTalk0End();
                    break;
                case TalkActionType.约会中2:
                    GameComponentData.gameData.loveAction.YueTalk1End();
                    break;
            }

            if (!isPlayertalk&&talkNpc!=null)
            {
                AudioController.instance.PlayAudio(SE.click);
                NPCFunctionPanel.SetActive(true);
                NPCFunctionPanel.GetComponent<NPCFunctionPanel>().InitNpcData(talkNpc);
            }
            TalkObj.SetActive(false);
            isTalkEnd = true;
            talkNpc = null;
            GameComponentData.gameData.guideController.CheckGuide();
            /*
      talkNpc != null

          
            else if (nowTalk.talkId == 1514)
            {
                GameComponentData.gameData.babyFilmAction.End();
            }
            else if (nowTalk.talkId == 1515)
            {
                GameComponentData.gameData.babyFilmAction.ChangeChild();
            }
            else if (nowTalk.talkId == 1517)
            {
                GameComponentData.gameData.babyFilmAction.SetChildName();
            }
            else if (nowTalk.talkId == 1519)
            {
                GameComponentData.gameData.babyFilmAction.HaveChildEnd();
            }
            */
        }
        
    }
	void Start () {
        isTalkEnd = true;

        JsonToData();

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
