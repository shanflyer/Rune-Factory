using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntelligencePanelAction : MonoBehaviour
{
    public List<Text> Texts;
    public GameObject CharactortitleObj;
    public GameObject NpcListPanel;
    public GameObject employObj;
    public Text PlayerNameText, PlayerLevelText, SkilText;
    public Image playerImage;
    public Button playerWeaponButton, playerClothesButton;
    public Text PlayerHpText,PlayerRpText,PlayerAText, PlayerDFText;

    public Text PlayerNameText0, PlayerLevelText0, SkilText0;
    public Image playerImage0;
    public Button playerWeaponButton0, playerClothesButton0;
    public Text PlayerHpText0, PlayerAText0, PlayerDFText0;

    public Text PlayerNameText1, PlayerLevelText1, SkilText1;
    public Image playerImage1;
    public Button playerWeaponButton1, playerClothesButton1;
    public Text PlayerHpText1, PlayerAText1, PlayerDFText1;

    public GameObject teamPlayer0, teamPlayer1;

    public void InitIntelligenceData()
    {
        GameComponentData.gameData.pastureAction.MoveCameraButtonObj.SetActive(false);
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        PlayerNameText.text = gamePlayer.name;
        PlayerLevelText.text = "Lv." + gamePlayer.level;
        SkilText.text = LanguageManage.SwitchStr("属性:")+LanguageManage.SwitchStr(gamePlayer.attributeType.ToString());
        playerImage.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.playerImage);
        if (gamePlayer.weapon != null && gamePlayer.weapon.ItemId != 0)
        {
            playerWeaponButton.GetComponentInChildren<Text>().text = gamePlayer.weapon.name;
        }
        else
        {
            playerWeaponButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("武器:无");
        }
        if (gamePlayer.clothes != null && gamePlayer.clothes.ItemId != 0)
        {
            playerClothesButton.GetComponentInChildren<Text>().text = gamePlayer.clothes.name;
        }
        else
        {
            playerClothesButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("防具:无");
        }
        PlayerHpText.text = "HP:" + gamePlayer.property.HP + "/" + gamePlayer.property.MaxHP;
        PlayerRpText.text = "RP:" + gamePlayer.property.Power + "/" + gamePlayer.property.MaxPower;
        PlayerAText.text = "AT:" + gamePlayer.property.AT;
        PlayerDFText.text = "DF:" + gamePlayer.property.DF;

        if (gamePlayer.TeamPlayer0 == null|| gamePlayer.TeamPlayer0.id == 0)
        {
            teamPlayer0.SetActive(true);
        }
        else
        {
            teamPlayer0.SetActive(false);
            PlayerNameText0.text = gamePlayer.TeamPlayer0.name;
            PlayerLevelText0.text = "Lv." + gamePlayer.TeamPlayer0.level;

            SkilText0.text = LanguageManage.SwitchStr("属性:") + LanguageManage.SwitchStr(gamePlayer.TeamPlayer0.attributeType.ToString());
            playerImage0.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer0.charactorImage);
            if (gamePlayer.TeamPlayer0.Weapon!= 0)
            {
                ItemData weponData =
                    GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.Weapon);

                playerWeaponButton0.GetComponentInChildren<Text>().text = weponData.Name;
            }
            else
            {
                playerWeaponButton0.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("武器:无");
            }
            if (gamePlayer.TeamPlayer0.clothes != 0)
            {
                ItemData clothesData =
                    GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.clothes);
                playerClothesButton0.GetComponentInChildren<Text>().text = clothesData.Name;
            }
            else
            {
                playerClothesButton0.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("防具:无");
            }
            if (gamePlayer.TeamPlayer0.id / 1000000 == 2)
            {
                playerClothesButton0.interactable = false;
                playerWeaponButton0.interactable = false;
            }
            else
            {
                playerClothesButton0.interactable =true;
                playerWeaponButton0.interactable = true;
            }

            PlayerHpText0.text = "HP:" + gamePlayer.TeamPlayer0.property.HP + "/" + gamePlayer.TeamPlayer0.property.MaxHP;
            PlayerAText0.text = "AT:" + gamePlayer.TeamPlayer0.property.AT;
            PlayerDFText0.text = "DF:" + gamePlayer.TeamPlayer0.property.DF;
        }

        if (gamePlayer.TeamPlayer1 == null|| gamePlayer.TeamPlayer1.id == 0)
        {
            teamPlayer1.SetActive(true);
        }
        else
        {
            teamPlayer1.SetActive(false);
            PlayerNameText1.text = gamePlayer.TeamPlayer1.name;
            PlayerLevelText1.text = "Lv." + gamePlayer.TeamPlayer1.level;
            
            SkilText1.text = LanguageManage.SwitchStr("属性:") + LanguageManage.SwitchStr(gamePlayer.TeamPlayer1.attributeType.ToString());
            playerImage1.sprite = GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer1.charactorImage);
            if (gamePlayer.TeamPlayer1.Weapon != 0)
            {
                ItemData weponData =
                    GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.Weapon);

                playerWeaponButton1.GetComponentInChildren<Text>().text = weponData.Name;
            }
            else
            {
                playerWeaponButton1.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("武器:无");
            }
            if (gamePlayer.TeamPlayer1.clothes != 0)
            {
                ItemData clothesData =
                    GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.clothes);
                playerClothesButton1.GetComponentInChildren<Text>().text = clothesData.Name;
            }
            else
            {
                playerClothesButton1.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("防具:无");
            }
            if (gamePlayer.TeamPlayer1.id / 1000000 == 2)
            {
                playerClothesButton1.interactable = false;
                playerWeaponButton1.interactable = false;
            }
            else
            {
                playerClothesButton1.interactable = true;
                playerWeaponButton1.interactable = true;
            }

            PlayerHpText1.text = "HP:" + gamePlayer.TeamPlayer1.property.HP + "/" + gamePlayer.TeamPlayer1.property.MaxHP;
            PlayerAText1.text = "AT:" + gamePlayer.TeamPlayer1.property.AT;
            PlayerDFText1.text = "DF:" + gamePlayer.TeamPlayer1.property.DF;
        }
    }

    public void ClickReturn()
    {
        AudioController.instance.PlayAudio(SE.Return);
        if (GameComponentData.gameData.passDataManager.NowPassData.id == 1001&&
            !GameComponentData.gameData.adventurePanelAction.gameObject.activeSelf)
        {
            GameComponentData.gameData.gameManager.fieldTool.SetActive(true);
        }
        gameObject.SetActive(false);
    }
    public void ClickCharactorTitle()
    {
        AudioController.instance.PlayAudio(SE.click);
        CharactortitleObj.SetActive(true);
        GameComponentData.gameData.charactorTitleAction.InitData();
    }
    public void ClickNPCButton()
    {
        AudioController.instance.PlayAudio(SE.click);
        NpcListPanel.SetActive(true);
        NpcListPanel.GetComponent<NPCListDataPanelAction>().InitNpcListData();
    }
    public void ClickWeapon(int index)
    {
        AudioController.instance.PlayAudio(SE.click);
        if (index == 0)
        {
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.FarmTool);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Normal);
            GameComponentData.gameData.warehouseAction.playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().
                InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.id);
        }
        else if(index == 1)
        {
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.Weapon);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Normal);
            if (GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0 != null&&GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id != 0)
            {
                GameComponentData.gameData.warehouseAction.playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().
                    InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id);
            }
        }
        else if(index == 2)
        {
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.Weapon);
            GameComponentData.gameData.warehouseObj.SetActive(true);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Normal);
            if (GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1 != null&&GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1.id != 0)
            {
                GameComponentData.gameData.warehouseAction.playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().
                    InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1.id);
            }
            
        }
    }

    public void ClickTuijian(string id)
    {
        string url = "itms-apps://itunes.apple.com/cn/app/id" + id;
        Application.OpenURL(url);
    }
    public void ClickClothes(int index)
    {
        AudioController.instance.PlayAudio(SE.click);
        if (index == 0)
        {
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.Clothes);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Normal);
            GameComponentData.gameData.warehouseAction.playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().
                InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.id);
        }
        else if (index == 1)
        {
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.Clothes);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Normal);
            GameComponentData.gameData.warehouseAction.playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().
                InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0.id);
        }
        else if (index == 2)
        {
            GameComponentData.gameData.warehouseObj.SetActive(true);
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.Clothes);
            GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Normal);
            GameComponentData.gameData.warehouseAction.playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().
                InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1.id);
        }
    }
    public void ClickPackage()
    {
        AudioController.instance.PlayAudio(SE.click);
        GameComponentData.gameData.warehouseObj.SetActive(true);
        List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
        wareDisplayTypes.Add(WareDisplayType.ALL);
        GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType.背包,wareDisplayTypes, DisplayType.Normal);
    }
    public void ClickAddEmplor()
    {
        AudioController.instance.PlayAudio(SE.click);
        employObj.SetActive(true);
        employObj.GetComponent<CharactorShop>().NpcToggle.isOn = true;
       employObj.GetComponent<CharactorShop>().SwitchEmployType(1);
    }
    // Use this for initialization
    void Start () {
        foreach (var text in Texts)
        {
            LanguageManage.TextFanyi(text);
        }
	}

    public void FireTeamPlayer(int i)
    {
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        if (i == 1)
        {
            if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
            {
                GameComponentData.gameData.employerManger.Employers.Find(e => e.id == gamePlayer.TeamPlayer0.id)
                    .isHired = false;
                gamePlayer.TeamPlayer0 = null;
            }
        }
        if (i == 2)
        {
            if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
            {
                GameComponentData.gameData.employerManger.Employers.Find(e => e.id == gamePlayer.TeamPlayer1.id)
                    .isHired = false;
                gamePlayer.TeamPlayer1 = null;
            }
        }
        InitIntelligenceData();
    }
	// Update is called once per frame
	void Update () {
		
	}
}
