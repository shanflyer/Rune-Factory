using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureResultAction : MonoBehaviour
{
    public List<Text> Texts;
    public GameObject successfulTitle0, failureTitle1;
    public GameObject itemPro;
    public Transform itemParent;
    public GameObject charactorObj0, charactorObj1, charactorObj2;
    public Image icon0, icon1, icon2;
    public Text name0, name1, name2,level0,level1,level2;
    public GameObject levelup0, levelup1, levelup2;
    public GameObject skillup0, skillup1, skillup2;

    private bool isSuccessful;
    // Use this for initialization
    void Start () {
        
	}

    public void ClickOkButton()
    {
        GameComponentData.gameData.SaveButtonObj.SetActive(true);
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        if (isSuccessful)
        {
            GameComponentData.gameData.BattleMapAction.InformationEnd(InformationType.战斗胜利);
        }
        else
        {
            if(GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0!=null&& GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id!=0&&
               GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id / 1000000 != 2)
            {
                NPCX npcx = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id);
                GameComponentData.gameData.gameManager.hurtNpc = npcx.npcData;
            }
            else
            {
                GameComponentData.gameData.gameManager.hurtNpc = null;
            }

                GameComponentData.gameData.BattleMapAction.InformationEnd(InformationType.战斗失败);


            GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0 = null;
            GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1 = null;
        }
        AudioManager.PlayBGM(PlayType.CYCLE,"002");
        Camera.main.transform.position = new Vector3(0, 0, -20);
        GameComponentData.gameData.gameManager.gamePlayer.attributeType = AttributeType.无;
        GameComponentData.gameData.gameManager.InitGate();
        gameObject.SetActive(false);

    }

    public void InitData(bool _isSuccessful,List<Item> items,GamePlayer gamePlayer)
    {
        GameComponentData.gameData.BattleMapAction.StopAllCoroutines();
        foreach (var text in Texts)
        {
            LanguageManage.TextFanyi(text);
        }
        if (_isSuccessful)
        {
            AudioManager.PlayBGM(PlayType.ONCE, "Victory");
        }
        else
        {
            AudioManager.PlayBGM(PlayType.ONCE, "Defeat");
        }
        skillup0.SetActive(false);
        skillup1.SetActive(false);
        skillup2.SetActive(false);
        isSuccessful = _isSuccessful;
        if (isSuccessful)
        {
            successfulTitle0.SetActive(true);
            failureTitle1.SetActive(false);
        }
        else
        {
            successfulTitle0.SetActive(false);
            failureTitle1.SetActive(true);
        }
        foreach (Transform child in itemParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var item in items)
        {
            GameObject itemObj = Instantiate(itemPro);
            itemObj.transform.SetParent(itemParent);
            itemObj.transform.localScale=Vector3.one;
            itemObj.GetComponent<ItemBoxAction>().InitItemData(item);
            itemObj.GetComponentInChildren<Button>().interactable = false;
        }
        icon0.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.playerImage);
        name0.text = gamePlayer.name;
        level0.text = "Lv."+gamePlayer.level;
        if (gamePlayer.oldLevel0 < gamePlayer.level)
        {
            levelup0.SetActive(true);
        }
        else
        {
            levelup0.SetActive(false);
        }
        
        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            charactorObj1.SetActive(true);
            icon1.sprite= GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer0.charactorImage);
            name1.text = gamePlayer.TeamPlayer0.name;
            level1.text = "Lv."+gamePlayer.TeamPlayer0.level;
            if (gamePlayer.oldLevel1 < gamePlayer.TeamPlayer0.level)
            {
                levelup1.SetActive(true);
            }
            else
            {
                levelup1.SetActive(false);
            }
        }
        else
        {
            charactorObj1.SetActive(false);
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
        {
            charactorObj2.SetActive(true);
            icon2.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer1.charactorImage);
            name2.text = gamePlayer.TeamPlayer1.name;
            level2.text = "Lv." + gamePlayer.TeamPlayer1.level;
            if (gamePlayer.oldLevel1 < gamePlayer.TeamPlayer1.level)
            {
                levelup2.SetActive(true);
            }
            else
            {
                levelup2.SetActive(false);
            }
        }
        else
        {
            charactorObj2.SetActive(false);
        }

    }
	// Update is called once per frame
	void Update () {
		
	}
}
