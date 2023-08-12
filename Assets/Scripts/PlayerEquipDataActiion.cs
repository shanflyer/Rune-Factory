
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEquipDataActiion : MonoBehaviour
{
    public List<Text> Texts;
    public Image charactorImage;
    public Text attributeText;
    public Text RpText,HpText, AtText, DfText, TypeText;
    public Text WeapomText, ClothesText;
    public Image icon,hpUp,hpDown,atUp,atDown,dfUp,dfDown;
    public Dropdown NameDropdown;
    private int playerId;

    private ItemData weaponData, clotherData;
    private int selectItem;

    private Property oldProperty0,newProperty;
	// Use this for initialization
	void Start () {
		
	}

    public void ChangePlayerDrop()
    {
        NameDropdown.value = 0;
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        InitDataPlayerEquaipData(gamePlayer.id);
        GameComponentData.gameData.intelligencePanelAction.InitIntelligenceData();
    }
    public void ChangeDrop(Dropdown dropdown)
    {
        AudioController.instance.PlayAudio(SE.select);
        string playerName = dropdown.options[dropdown.value].text;
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        if (gamePlayer.name == playerName)
        {
            InitDataPlayerEquaipData(gamePlayer.id);
        }
        else if(gamePlayer.TeamPlayer0!=null&&gamePlayer.TeamPlayer0.name==playerName)
        {
            InitDataPlayerEquaipData(gamePlayer.TeamPlayer0.id);
        }
        else if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.name == playerName)
        {
            InitDataPlayerEquaipData(gamePlayer.TeamPlayer1.id);
        }
        if (selectItem != 0)
        {
            SelectItem(selectItem);
        }
    }

    public void EuqipMentAction()
    {
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        ItemData selectItemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(selectItem);
        Item oldItem = null;
        if (gamePlayer.id==playerId)
        {
            
            if (selectItemData.Type == ItemType.武器)
            {
                
                if (gamePlayer.weapon != null && gamePlayer.weapon.ItemId != 0)
                {
                    ItemData oldItemData =
                        GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.weapon.ItemId);
                    oldItem=new Item(oldItemData,1);
                    Property oldProperty= oldItemData.GetProperty();
                    gamePlayer.property -= oldProperty;
                    gamePlayer.property += selectItemData.GetProperty();
                }
                else
                {
                    gamePlayer.property += selectItemData.GetProperty();
                }
                gamePlayer.weapon=new Item(selectItemData,1);
            }
            else if (selectItemData.Type == ItemType.防具)
            {
                if (gamePlayer.clothes != null && gamePlayer.clothes.ItemId != 0)
                {
                    ItemData oldItemData =
                        GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.clothes.ItemId);
                    oldItem = new Item(oldItemData, 1);
                    Property oldProperty = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.clothes.ItemId).GetProperty();
                    gamePlayer.property -= oldProperty;
                    gamePlayer.property += selectItemData.GetProperty();
                }
                else
                {
                    gamePlayer.property += selectItemData.GetProperty();
                }
                gamePlayer.clothes = new Item(selectItemData, 1);
            }
        }
        else if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0&&gamePlayer.TeamPlayer0.id==playerId)
        {
            if (selectItemData.Type == ItemType.武器)
            {
                if (gamePlayer.TeamPlayer0.Weapon != 0)
                {
                    ItemData oldItemData =
                        GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.Weapon);
                    oldItem = new Item(oldItemData, 1);
                    Property oldProperty = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.Weapon).GetProperty();
                    gamePlayer.TeamPlayer0.property -= oldProperty;
                    gamePlayer.TeamPlayer0.property += selectItemData.GetProperty();
                }
                else
                {
                    gamePlayer.TeamPlayer0.property += selectItemData.GetProperty();
                }
                gamePlayer.TeamPlayer0.Weapon = selectItemData.Id;
                GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id).weapon =
                    selectItemData.Id;
            }
            else if (selectItemData.Type == ItemType.防具)
            {
                if (gamePlayer.TeamPlayer0.clothes != 0)
                {
                    ItemData oldItemData =
                        GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.clothes);
                    oldItem = new Item(oldItemData, 1);
                    Property oldProperty = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.clothes).GetProperty();
                    gamePlayer.TeamPlayer0.property -= oldProperty;
                    gamePlayer.TeamPlayer0.property += selectItemData.GetProperty();
                }
                else
                {
                    gamePlayer.TeamPlayer0.property += selectItemData.GetProperty();
                }
                gamePlayer.TeamPlayer0.clothes = selectItemData.Id;
                GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer0.id).clothes =
                    selectItemData.Id;
            }
        }
        else if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.id == playerId)
        {
            if (selectItemData.Type == ItemType.武器)
            {
                if (gamePlayer.TeamPlayer1.Weapon != 0)
                {
                    ItemData oldItemData =
                        GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.Weapon);
                    oldItem = new Item(oldItemData, 1);
                    Property oldProperty = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.Weapon).GetProperty();
                    gamePlayer.TeamPlayer1.property -= oldProperty;
                    gamePlayer.TeamPlayer1.property += selectItemData.GetProperty();
                }
                else
                {
                    gamePlayer.TeamPlayer1.property += selectItemData.GetProperty();
                }
                gamePlayer.TeamPlayer1.Weapon = selectItemData.Id;
                GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id).weapon =
                    selectItemData.Id;
            }
            else if (selectItemData.Type == ItemType.防具)
            {
                if (gamePlayer.TeamPlayer1.clothes != 0)
                {
                    ItemData oldItemData =
                        GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.clothes);
                    oldItem = new Item(oldItemData, 1);
                    Property oldProperty = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.clothes).GetProperty();
                    gamePlayer.TeamPlayer1.property -= oldProperty;
                    gamePlayer.TeamPlayer1.property += selectItemData.GetProperty();
                }
                else
                {
                    gamePlayer.TeamPlayer1.property += selectItemData.GetProperty();
                }
                gamePlayer.TeamPlayer1.clothes = selectItemData.Id;
                GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == gamePlayer.TeamPlayer1.id).clothes =
                    selectItemData.Id;
            }
        }

        
        gamePlayer.package.GetItemOutPackage(selectItem,1);
        gamePlayer.package.SetItemInPackage(oldItem);
        
        GameComponentData.gameData.warehouseAction.InitWareHouseData();
        InitDataPlayerEquaipData(playerId);
        GameComponentData.gameData.intelligencePanelAction.InitIntelligenceData();
    }
    public void InitDataPlayerEquaipData(int _playerId)
    {
        selectItem = 0;
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;

        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0 && gamePlayer.TeamPlayer1!=null)
        {
            NameDropdown.options=new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0=new Dropdown.OptionData(gamePlayer.name);
            Dropdown.OptionData optionData1 = new Dropdown.OptionData(gamePlayer.TeamPlayer0.name);
            Dropdown.OptionData optionData2 = new Dropdown.OptionData(gamePlayer.TeamPlayer1.name);
            NameDropdown.options.Add(optionData0);
            NameDropdown.options.Add(optionData1);
            NameDropdown.options.Add(optionData2);
        }
        else if(gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            Dropdown.OptionData optionData1 = new Dropdown.OptionData(gamePlayer.TeamPlayer0.name);
            NameDropdown.options.Add(optionData0);
            NameDropdown.options.Add(optionData1);
        }
        else if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            Dropdown.OptionData optionData1 = new Dropdown.OptionData(gamePlayer.TeamPlayer1.name);
            NameDropdown.options.Add(optionData0);
            NameDropdown.options.Add(optionData1);
        }
        else
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            NameDropdown.options.Add(optionData0);
        }


        hpUp.enabled = false;
        hpDown.enabled = false;
        atUp.enabled = false;
        atDown.enabled = false;
        dfDown.enabled = false;
        dfUp.enabled = false;
        playerId = _playerId;
        if (gamePlayer.id == playerId)
        {
            oldProperty0 = gamePlayer.property;
            NameDropdown.captionText.text = gamePlayer.name;
            RpText.text = gamePlayer.property.Power + "/" + gamePlayer.property.MaxPower;
            HpText.text = gamePlayer.property.HP + "/" + gamePlayer.property.MaxHP;
            AtText.text =  gamePlayer.property.AT.ToString();
            DfText.text =  gamePlayer.property.DF.ToString();
            TypeText.text = LanguageManage.SwitchStr("我");
            charactorImage.sprite =
                GameComponent.charactorIcon.Find(c => c.name == gamePlayer.playerImage);
            attributeText.text = LanguageManage.SwitchStr("属性:") +
                                 LanguageManage.SwitchStr(gamePlayer.attributeType.ToString());
            if (gamePlayer.weapon == null || gamePlayer.weapon.ItemId == 0)
            {
                WeapomText.text = LanguageManage.SwitchStr("武器:无");
                weaponData = null;
            }
            else
            {
                weaponData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.weapon.ItemId);
                WeapomText.text = weaponData.Name;
            }

            if (gamePlayer.clothes == null || gamePlayer.clothes.ItemId == 0)
            {
                ClothesText.text = LanguageManage.SwitchStr("防具:无");
                clotherData = null;
            }
            else
            {
                clotherData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.clothes.ItemId);
                ClothesText.text = clotherData.Name;
            }
        }
        if (gamePlayer.TeamPlayer0!=null&&gamePlayer.TeamPlayer0.id==playerId)
        {
            RpText.text = "--/--";
            oldProperty0 = gamePlayer.TeamPlayer0.property;
            NameDropdown.captionText.text = gamePlayer.TeamPlayer0.name;
            HpText.text =  gamePlayer.TeamPlayer0.property.HP + "/" + gamePlayer.TeamPlayer0.property.MaxHP;
            AtText.text =  gamePlayer.TeamPlayer0.property.AT.ToString();
            DfText.text =  gamePlayer.TeamPlayer0.property.DF.ToString();
            TypeText.text = LanguageManage.SwitchStr("队友");
            charactorImage.sprite =
                GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer0.charactorImage);
            attributeText.text = LanguageManage.SwitchStr("属性:") +
                                 LanguageManage.SwitchStr(gamePlayer.TeamPlayer0.attributeType.ToString());

            if (gamePlayer.TeamPlayer0.Weapon == 0)
            {
                WeapomText.text = LanguageManage.SwitchStr("武器:无");
                weaponData = null;
            }
            else
            {
                weaponData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.Weapon);
                WeapomText.text = weaponData.Name;
            }

            if (gamePlayer.TeamPlayer0.clothes == 0)
            {
                ClothesText.text = LanguageManage.SwitchStr("防具:无");
                clotherData = null;
            }
            else
            {
                clotherData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.clothes);
                ClothesText.text = clotherData.Name;
            }
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.id == playerId)
        {
            RpText.text = "--/--";
            oldProperty0 = gamePlayer.TeamPlayer1.property;
            NameDropdown.captionText.text = gamePlayer.TeamPlayer1.name;
            HpText.text = gamePlayer.TeamPlayer1.property.HP + "/" + gamePlayer.TeamPlayer1.property.MaxHP;
            AtText.text = gamePlayer.TeamPlayer1.property.AT.ToString();
            DfText.text =  gamePlayer.TeamPlayer1.property.DF.ToString();
            TypeText.text = LanguageManage.SwitchStr("队友");
            attributeText.text = LanguageManage.SwitchStr("属性:") +
                                 LanguageManage.SwitchStr(gamePlayer.TeamPlayer1.attributeType.ToString());
            charactorImage.sprite =
                GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer1.charactorImage);
            if (gamePlayer.TeamPlayer1.Weapon == 0)
            {
                WeapomText.text = LanguageManage.SwitchStr("武器:无");
                weaponData = null;
            }
            else
            {
                weaponData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.Weapon);
                WeapomText.text = weaponData.Name;
            }

            if (gamePlayer.TeamPlayer1.clothes == 0)
            {
                ClothesText.text = LanguageManage.SwitchStr("防具:无");
                clotherData = null;
            }
            else
            {
                clotherData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.clothes);
                ClothesText.text = clotherData.Name;
            }
        }

        HpText.color=Color.black;
        AtText.color=Color.black;
        DfText.color=Color.black;
    }
    public void InitDataPlayerEquaipData()
    {
        selectItem = 0;
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;

        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0 && gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            Dropdown.OptionData optionData1 = new Dropdown.OptionData(gamePlayer.TeamPlayer0.name);
            Dropdown.OptionData optionData2 = new Dropdown.OptionData(gamePlayer.TeamPlayer1.name);
            NameDropdown.options.Add(optionData0);
            NameDropdown.options.Add(optionData1);
            NameDropdown.options.Add(optionData2);
        }
        else if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0)
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            Dropdown.OptionData optionData1 = new Dropdown.OptionData(gamePlayer.TeamPlayer0.name);
            NameDropdown.options.Add(optionData0);
            NameDropdown.options.Add(optionData1);
        }
        else if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0)
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            Dropdown.OptionData optionData1 = new Dropdown.OptionData(gamePlayer.TeamPlayer1.name);
            NameDropdown.options.Add(optionData0);
            NameDropdown.options.Add(optionData1);
        }
        else
        {
            NameDropdown.options = new List<Dropdown.OptionData>();
            Dropdown.OptionData optionData0 = new Dropdown.OptionData(gamePlayer.name);
            NameDropdown.options.Add(optionData0);
        }


        hpUp.enabled = false;
        hpDown.enabled = false;
        atUp.enabled = false;
        atDown.enabled = false;
        dfDown.enabled = false;
        dfUp.enabled = false;
        if (gamePlayer.id == playerId)
        {
            oldProperty0 = gamePlayer.property;
            NameDropdown.captionText.text = gamePlayer.name;
            HpText.text = gamePlayer.property.HP + "/" + gamePlayer.property.MaxHP;
            AtText.text = gamePlayer.property.AT.ToString();
            DfText.text = gamePlayer.property.DF.ToString();
            TypeText.text = LanguageManage.SwitchStr("我");
            charactorImage.sprite =
                GameComponent.charactorIcon.Find(c => c.name == gamePlayer.playerImage);
            if (gamePlayer.weapon == null || gamePlayer.weapon.ItemId == 0)
            {
                WeapomText.text = LanguageManage.SwitchStr("武器:无");
                weaponData = null;
            }
            else
            {
                weaponData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.weapon.ItemId);
                WeapomText.text = weaponData.Name;
            }

            if (gamePlayer.clothes == null || gamePlayer.clothes.ItemId == 0)
            {
                ClothesText.text = LanguageManage.SwitchStr("防具:无");
                clotherData = null;
            }
            else
            {
                clotherData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.clothes.ItemId);
                ClothesText.text = clotherData.Name;
            }
        }
        if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0 && gamePlayer.TeamPlayer0.id == playerId)
        {
            oldProperty0 = gamePlayer.TeamPlayer0.property;
            NameDropdown.captionText.text = gamePlayer.TeamPlayer0.name;
            HpText.text = gamePlayer.TeamPlayer0.property.HP + "/" + gamePlayer.TeamPlayer0.property.MaxHP;
            AtText.text = gamePlayer.TeamPlayer0.property.AT.ToString();
            DfText.text = gamePlayer.TeamPlayer0.property.DF.ToString();
            TypeText.text = LanguageManage.SwitchStr("队友");
            charactorImage.sprite =
                GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer0.charactorImage);
            if (gamePlayer.TeamPlayer0.Weapon == 0)
            {
                WeapomText.text = LanguageManage.SwitchStr("武器:无");
                weaponData = null;
            }
            else
            {
                weaponData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.Weapon);
                WeapomText.text = weaponData.Name;
            }

            if (gamePlayer.TeamPlayer0.clothes == 0)
            {
                ClothesText.text = LanguageManage.SwitchStr("防具:无");
                clotherData = null;
            }
            else
            {
                clotherData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer0.clothes);
                ClothesText.text = clotherData.Name;
            }
        }
        if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.id == playerId)
        {
            oldProperty0 = gamePlayer.TeamPlayer1.property;
            NameDropdown.captionText.text = gamePlayer.TeamPlayer1.name;
            HpText.text = gamePlayer.TeamPlayer1.property.HP + "/" + gamePlayer.TeamPlayer1.property.MaxHP;
            AtText.text = gamePlayer.TeamPlayer1.property.AT.ToString();
            DfText.text = gamePlayer.TeamPlayer1.property.DF.ToString();
            TypeText.text = LanguageManage.SwitchStr("队友");
            charactorImage.sprite =
                GameComponent.charactorIcon.Find(c => c.name == gamePlayer.TeamPlayer1.charactorImage);
            if (gamePlayer.TeamPlayer1.Weapon == 0)
            {
                WeapomText.text = LanguageManage.SwitchStr("武器:无");
                weaponData = null;
            }
            else
            {
                weaponData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.Weapon);
                WeapomText.text = weaponData.Name;
            }

            if (gamePlayer.TeamPlayer1.clothes == 0)
            {
                ClothesText.text = LanguageManage.SwitchStr("防具:无");
                clotherData = null;
            }
            else
            {
                clotherData = GameComponentData.gameData.itemsManager.GetItemDataFromId(gamePlayer.TeamPlayer1.clothes);
                ClothesText.text = clotherData.Name;
            }
        }

        HpText.color = Color.black;
        AtText.color = Color.black;
        DfText.color = Color.black;
    }
    public void SelectItem(int itemId)
    {
        selectItem = itemId;
       // GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(itemId);
        if (itemData.Type == ItemType.武器)
        {
            if (weaponData == null)
            {
                Property zeroProperty=new Property(0);
                CompareProperty(zeroProperty,itemData.GetProperty());
            }
            else
            {
                CompareProperty(weaponData.GetProperty(), itemData.GetProperty());
            }
           
        }
        else if (itemData.Type == ItemType.防具)
        {
            if (clotherData == null)
            {
                Property zeroProperty = new Property(0);
                CompareProperty(zeroProperty, itemData.GetProperty());
            }
            else
            {
                CompareProperty(clotherData.GetProperty(), itemData.GetProperty());
            }
           
        }
        else
        {
            HpText.text = oldProperty0.HP + "/" + oldProperty0.MaxHP;
            AtText.text = oldProperty0.AT.ToString();
            DfText.text = oldProperty0.DF.ToString();
            HpText.color=Color.black;
            AtText.color=Color.black;
            DfText.color=Color.black;

            hpUp.enabled = false;
            hpDown.enabled = false;
            atUp.enabled = false;
            atDown.enabled = false;
            dfDown.enabled = false;
            dfUp.enabled = false;
        }
     
    }

    void CompareProperty(Property property0, Property property1)
    {
        newProperty = oldProperty0-property0+property1;
        HpText.text = newProperty.HP + "/" + newProperty.MaxHP;
        AtText.text = newProperty.AT.ToString();
        DfText.text = newProperty.DF.ToString();
        if (property0.MaxHP > property1.MaxHP)
        {
            HpText.color = Color.red;
            hpDown.enabled = true;
            hpUp.enabled = false;

        }
        if (property0.MaxHP == property1.MaxHP)
        {
            HpText.color = Color.black;
            hpDown.enabled = false;
            hpUp.enabled = false;
        }
        if (property0.MaxHP < property1.MaxHP)
        {
            HpText.color = Color.green;
            hpDown.enabled = false;
            hpUp.enabled = true;
        }
        if (property0.AT> property1.AT)
        {
            AtText.color=Color.red;
            atDown.enabled = true;
            atUp.enabled = false;
        }
        if (property0.AT == property1.AT)
        {
            AtText.color = Color.black;
            atDown.enabled = false;
            atUp.enabled = false;
        }
        if (property0.AT < property1.AT)
        {
            AtText.color=Color.green;
            
            atDown.enabled = false;
            atUp.enabled = true;
        }
        if (property0.DF > property1.DF)
        {
            DfText.color=Color.red;
            dfDown.enabled = true;
            dfUp.enabled = false;
        }
        if (property0.DF == property1.DF)
        {
            DfText.color=Color.black;
            dfDown.enabled = false;
            dfUp.enabled = false;
        }
        if (property0.DF < property1.DF)
        {
            DfText.color=Color.green;
            dfDown.enabled = false;
            dfUp.enabled = true;
        }

    }
	// Update is called once per frame
	void Update () {
		
	}
}
