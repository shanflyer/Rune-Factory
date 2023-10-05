using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class AdventurePanelAction : MonoBehaviour
{
    public List<Text> functionTexts;
    public GameObject itemIconPro;
    public Transform itemParent;
     
    public Sprite nullCharactorSprite;
    public Image TeamPlayerImage0, TeamPlayerImage1, TeamPlayerImage2;

    public GameObject PlacePanelPro;

    public Transform PlaceParent;
    private List<GameObject> PlaceObjs;
    public Text noticeText, explorerText;
    public Button explorbutton;
    //private BatteleMap selectMap;
	// Use this for initialization
	void Start ()
	{
	    
	}
    public void InitData()
    {
        GameComponentData.gameData.SaveButtonObj.SetActive(false);
        foreach (var functionText in functionTexts)
        {
            LanguageManage.TextFanyi(functionText);
        }


        GameComponentData.gameData.pastureAction.MoveCameraButtonObj.SetActive(false);
        AudioController.instance.PlayAudio(BGM.bgm002);
         
        if (PlaceObjs == null)
        {
            PlaceObjs = new List<GameObject>();
        }
        else
        {
            foreach(var x in PlaceObjs)
            {
                Destroy(x);
            }
            PlaceObjs=new List<GameObject>();
        }
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        TeamPlayerImage0.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.playerImage);
        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            
            TeamPlayerImage1.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer0.charactorImage);
        }
        else
        {
            TeamPlayerImage1.sprite = nullCharactorSprite;
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 )
        {
            TeamPlayerImage2.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer1.charactorImage);
        }
        else
        {
            TeamPlayerImage2.sprite = nullCharactorSprite;
        }
        /*
        List<BatteleMap> battleMaps = GameComponentData.gameData.BattleMapAction.BatteleMaps;
        foreach(var battleMap in battleMaps)
        {
            if (battleMap.id != 4100)
            {
                GameObject battleObj = Instantiate(PlacePanelPro);

                battleObj.GetComponent<PlacePanelAction>().InitData(battleMap);
                battleObj.transform.SetParent(PlaceParent, true);
                battleObj.GetComponentInChildren<Toggle>().group = PlaceParent.GetComponent<ToggleGroup>();
                PlaceObjs.Add(battleObj);
                battleObj.transform.localScale=Vector3.one;
            }
            
        }*/
        PlaceObjs[0].GetComponent<PlacePanelAction>().ClickToggleButton();
    }

   

    public void ZeroEploring()
    {

        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        gamePlayer.property.HP = gamePlayer.property.MaxHP;
        gamePlayer.property.Power = gamePlayer.property.MaxPower;
        gamePlayer.TeamPlayer0 = new TeamPlayer(2004, LanguageManage.SwitchStr("马"), 1, 1005, gamePlayer.property,0);
        gamePlayer.TeamPlayer0.ObjName = "house";
        gamePlayer.TeamPlayer1 = new TeamPlayer(3015, LanguageManage.SwitchStr("班尼迪克"), 1, 1002, gamePlayer.property,0);
        gamePlayer.TeamPlayer1.ObjName = "man1";
        //BatteleMap batteleMap = GameComponentData.gameData.BattleMapAction.BatteleMaps.Find(b => b.id == 4100);
  
        AudioController.instance.PlayAudio(BGM.Move);
        GameComponentData.gameData.mapParent.gameObject.SetActive(false); 
        GameComponentData.gameData.gameManager.StartFight(); 
        

        
        GameComponentData.gameData.informationObj.SetActive(false);
        gameObject.SetActive(false);
    }

   
      
   
    // Update is called once per frame
    void Update () {
		
	}
}
