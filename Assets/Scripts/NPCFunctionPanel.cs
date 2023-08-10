using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCFunctionPanel : MonoBehaviour
{
    public Button StoreButton;
    public Button YuehuiButton;
    public Text loveText;
    public Image itemImage;
    public Text itemText;
    public GameObject Npcfunction, NPCGift,marriedFunction;
    private NPCX npcx;

    private Shop npcShop;

    private ItemData giftData;

    private bool isCanMarried;
	// Use this for initialization
	void Start ()
	{
	    

	}

    public void ClickMarriedFood()
    {
        if (!GameComponentData.gameData.gameManager.gamePlayer.isMarriedFood)
        {
            if (GameComponentData.gameData.gameManager.gamePlayer.package.CaseCount <=
                GameComponentData.gameData.gameManager.gamePlayer.package.items.Count)
            {
                AudioManager.PlaySE(PlayType.ONCE, "Click");
                GameComponentData.gameData.talkTextsManager.TalkAction("1506", npcx.npcData.headName,npcx.Name,npcx,TalkActionType.普通);
                gameObject.SetActive(false);
            }
            else
            {
                GameComponentData.gameData.gameManager.gamePlayer.isMarriedFood = true;
                GameComponentData.gameData.gameManager.gamePlayer.package.SetItemInPackage(1400, 1);
                ItemData loveItemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(1400);
                NPCGift.SetActive(true);
                itemImage.sprite = GameComponent.ItemSprites.Find(icon => icon.name == loveItemData.Icon);
                itemText.text = LanguageManage.SwitchStr("获得1个") + loveItemData.Name;
                marriedFunction.SetActive(false);
            }
        }
        else
        {
            AudioManager.PlaySE(PlayType.ONCE, "Click");
            GameComponentData.gameData.talkTextsManager.TalkAction("1507", npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通);
            gameObject.SetActive(false);
        }
        
    }
    public void ClickAnMo()
    {
        if (!GameComponentData.gameData.gameManager.gamePlayer.isAnMo)
        {
            GameComponentData.gameData.gameManager.gamePlayer.isAnMo = true;
            GameComponentData.gameData.gameManager.playerCharactor.property.Power = GameComponentData.gameData
                .gameManager.playerCharactor.property.MaxPower;
            GameComponentData.gameData.gameManager.gamePlayer.property.Power =
                GameComponentData.gameData.gameManager.gamePlayer.property.MaxPower;
            AudioManager.PlaySE(PlayType.ONCE, "Heal");
            GameComponentData.gameData.talkTextsManager.TalkAction("1504",null);
            GameComponentData.gameData.gameManager.UpDataPlayer();
           
        }
        else
        {
            AudioManager.PlaySE(PlayType.ONCE, "Click");
            GameComponentData.gameData.talkTextsManager.TalkAction("1505", npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通) ;
        }
        gameObject.SetActive(false);
    }
    public void ClickOKButton()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        if (giftData != null)
        {
            Item item = new Item(giftData, 1);
            int count = GameComponentData.gameData.gameManager.gamePlayer.package.SetItemInPackage(item.ItemId, 1);
            if (count > 0)
            {
                GameComponentData.gameData.informationManager.AddInformation("*背包已满，无法获得物品！");
            }
            else
            {
                GameComponentData.gameData.informationManager.AddInformation("*获得1个" + giftData.Name);
            }
        }
       
        giftData = null;
    }
    public void InitNpcData(NPCX _npcx)
    {
        if (!_npcx.isMarried && GameComponentData.gameData.gameDebugAction.MarryTest)
        {
            _npcx.isPlayerBrothDay = false;
            NPCGift.SetActive(false);
            npcx = _npcx;
            Npcfunction.SetActive(true);
            marriedFunction.SetActive(false);
            npcShop = GameComponentData.gameData.shopManager.Shops.Find(s => s.Npcid == npcx.id);
            isCanMarried = true;
            YuehuiButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("我们结婚吧");
        }
        else
        {
            if (_npcx.isPlayerBrothDay && _npcx.npcData.friendlyLevel >= 3)
            {
                List<int> gifts = new List<int>();
                List<int> friendlyLevels = new List<int>();
                Npcfunction.SetActive(false);
                NPCGift.SetActive(true);
                var x = _npcx.npcData.brothDayGift.Split(';');
                foreach (var s in x)
                {
                    int itemId = int.Parse(s.Split(',')[0]);
                    int level = int.Parse(s.Split(',')[1]);
                    gifts.Add(itemId);
                    friendlyLevels.Add(level);
                }

                for (int i = friendlyLevels.Count - 1; i >= 0; i--)
                {
                    if (friendlyLevels[i] <= _npcx.npcData.friendlyLevel)
                    {
                        giftData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gifts[i]);
                        break;
                    }
                }
                itemImage.sprite = GameComponent.ItemSprites.Find(icon => icon.name == giftData.Name);
                itemText.text = LanguageManage.SwitchStr("获得1个") + giftData.Name;

                _npcx.isPlayerBrothDay = false;
            }
            else
            {
                _npcx.isPlayerBrothDay = false;
                NPCGift.SetActive(false);
                npcx = _npcx;
                if (_npcx.isMarried)
                {
                    Npcfunction.SetActive(false);
                    marriedFunction.SetActive(true);
                }
                else
                {
                    Npcfunction.SetActive(true);
                    marriedFunction.SetActive(false);
                    npcShop = GameComponentData.gameData.shopManager.Shops.Find(s => s.Npcid == npcx.id);
                    if (npcShop != null)
                    {
                        StoreButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        StoreButton.gameObject.SetActive(false);
                    }
                    if (npcx.isLove)
                    {
                        YuehuiButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        YuehuiButton.gameObject.SetActive(false);
                    }
                    if (npcx.isLove)
                    {
                        loveText.text = LanguageManage.SwitchStr("分手吧");
                    }
                    else
                    {
                        loveText.text = LanguageManage.SwitchStr("我喜欢你");
                    }
                    if (npcx.npcData.friendlyLevel >= 12)
                    {
                        isCanMarried = true;
                        YuehuiButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("我们结婚吧");
                    }
                    else
                    {
                        isCanMarried = false;
                        YuehuiButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("约会");
                    }
                }
            }
        }
    }

    public void ClickYuehuiButton()
    {
        if (isCanMarried)
        {
            if (!npcx.isMarried)
            {
                if (GameComponentData.gameData.gameDebugAction.MarryTest)
                {
                    GameComponentData.gameData.talkTextsManager.PleaseMarried(npcx);
                    gameObject.SetActive(false);
                }
                else
                {
                    var x = GameComponentData.gameData.equipmentManager.Euqipments.FindAll(e => e.id / 1000 == 2 && !e.isBuy);
                    if (x.Count > 0)
                    {
                        string equipName = "";
                        foreach (var euqipment in x)
                        {
                            equipName += " " + euqipment.name;
                        }
                        GameNotificationManager.instance.DisplayTips("提示", LanguageManage.SwitchStr("不能结婚！下列婚姻必需品未购买:") + equipName);
                    }
                    else
                    {

                        GameComponentData.gameData.talkTextsManager.PleaseMarried(npcx);
                        gameObject.SetActive(false);
                    }
                }
            }
            
            
        }
        else
        {
            if (npcx.isYuehui)
            {
                GameComponentData.gameData.talkTextsManager.TalkAction("1503", npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通);
            }
            else
            {
                GameComponentData.gameData.gameManager.lover = npcx;
                GameComponentData.gameData.gameManager.IsYueHui = true;
                GameComponentData.gameData.loveAction.lover = npcx;
                GameComponentData.gameData.talkTextsManager.TalkAction("1502", npcx.npcData.headName, npcx.Name, npcx,TalkActionType.约会);
                npcx.isYuehui = true;
            }
            gameObject.SetActive(false);
        }
        
        
    }

   
    public void ClickTalkButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        if (npcx.npcData.normalTalks != null&& npcx.npcData.normalTalks.Count>0)
        {
            int index = Random.Range(0, npcx.npcData.normalTalks.Count);
            int talkId = npcx.npcData.normalTalks[index];
            GameComponentData.gameData.talkTextsManager.TalkAction(talkId.ToString(),npcx.npcData.headName,npcx.Name,npcx,TalkActionType.普通);

            if (!npcx.isFriendlyExpAdd)
            {
                npcx.AddFriendlyexp(1);
                npcx.isFriendlyExpAdd = true;

            }

        }

        gameObject.SetActive(false);
    }

    public void ClickGiftButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        GameComponentData.gameData.warehouseObj.SetActive(true);
        List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
        wareDisplayTypes.Add(WareDisplayType.ALL);
        GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包,wareDisplayTypes,DisplayType.Gift);
        gameObject.SetActive(false);
    }
    public void LeaveLover()
    {
        npcx.isLove = false;
        npcx.isYuehui = false;
        GameComponentData.gameData.talkTextsManager.TalkAction(npcx.npcData.leaveTalk.ToString(), npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通);
        npcx.npcData.friendlyLevel /= 2;
        foreach (var npcx1 in GameComponentData.gameData.NpcManager.Npcxs)
        {
            npcx1.npcData.friendlyLevel -= 1;
            if (npcx1.npcData.friendlyLevel < 0)
            {
                npcx1.npcData.friendlyLevel = 0;
            }
            npcx1.npcData.frienflyExp = 0;
        }
        GameComponentData.gameData.informationManager.AddInformation("*" + LanguageManage.SwitchStr("解除情侣关系！"));
        GameComponentData.gameData.informationManager.AddInformation("*" + npcx.Name + LanguageManage.SwitchStr("友好度大幅下降！"));
        GameComponentData.gameData.informationManager.AddInformation("*" + LanguageManage.SwitchStr("全体NPC友好度下降！"));
    
    }
    public void ClickConfessionButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        if (npcx.isLove)
        {
            GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("分手"),LanguageManage.SwitchStr("确定要解除情侣关系吗？所有人的友好度都会下降。"),CareType.leaveLove);
        }
        else
        {
            if (npcx.npcData.friendlyLevel >= 8 && npcx.npcData.successfulTalk != 0&&npcx.npcData.sex!=GameComponentData.gameData.gameManager.gamePlayer.gender)
            {

                if (GameComponentData.gameData.NpcManager.Npcxs.Exists(n => n.isLove))
                {
                    GameComponentData.gameData.talkTextsManager.TalkAction("1500", npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通);
                }
                else
                {
                    npcx.isLove = true;
                    GameComponentData.gameData.talkTextsManager.TalkAction(npcx.npcData.successfulTalk.ToString(), npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通);
                }
                
            }
            else
            {
                GameComponentData.gameData.talkTextsManager.TalkAction(npcx.npcData.failureTalk.ToString(), npcx.npcData.headName, npcx.Name, npcx,TalkActionType.普通);
            }
        }
        
      

        
        gameObject.SetActive(false);
    }
    public void ClickTeamButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        if (GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0 != null&&GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id != 0 &&
            GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1 != null&&GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1.id != 0)
        {
            GameNotificationManager.instance.DisplayTips("组队失败","队伍中没有空位");
        }
        else
        {
            int talkId = 0;
            if (npcx.npcData.npcStatus == NpcStatus.正常 && npcx.npcData.friendlyLevel >= 5)
            {
                talkId = npcx.npcData.teamSuccessfulTalk;
            }
            else if (npcx.npcData.friendlyLevel < 5)
            {
                talkId = npcx.npcData.teamfailureTalk0;
            }
            else
            {
                talkId = npcx.npcData.teamfailureTalk1;
            }
            GameComponentData.gameData.talkTextsManager.TalkAction(talkId.ToString(), npcx.npcData.headName, npcx.Name);
            gameObject.SetActive(false);
        }
        
    }
    public void ClickStoreButton()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        GameComponentData.gameData.shopManager.InitShopData(npcShop.id);
        gameObject.SetActive(false);
    }

 
	// Update is called once per frame
	void Update () {
		
	}
}
