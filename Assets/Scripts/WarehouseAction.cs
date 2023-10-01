using OldName;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public enum DisplayType
{
    Out=0,
    In=1,
    Sell=2,
    Equip=3,
    AfterBattle=4,
    Battling= 5,
    Normal=6,
    Gift=7

}
public enum WareDisplayType
{
    ALL=0,
    Good=1,
    FarmTool=2,
    Weapon=3,
    Clothes=4,
    food=5,
    药剂=6,
    卷轴=7
}
public class WarehouseAction : MonoBehaviour
{
    public List<Text> Texts;
    public GameObject playerEquipDataObj;
    public GameObject functionButtons;
    public GameObject SellSelectObj;
    public GameObject ItemBoxPro,NULLBoxPro,AddBoxPro;
    public Transform ItemBoxParent;
    private List<GameObject> ItemBoxObjs;
    private Item selectedItem;
    public GameObject ItemInfomationObj;
    public Text TitleText;
    public PackageType PackageType;
    public Text SellText;
    [HideInInspector] public DeskAction deskAction;
    public Image ItemIconImage;
    public Text CaseCounText;
    public Text ItemNameText,
        ItemTypeText,
        ItemPriceText,
        ItemPropertyText,
        ItemNoticeText;

    public Button TitleButton;
    public GameObject UpText, DownText;
    public Button sellButton;
    public Text BoxNameText, BoxCountText;
    private int outPackage, playerPackage;
    private DisplayType displayType;
    [HideInInspector] public List<WareDisplayType> wareDisplayTypes;

    private List<GameObject> addCaseObjs;
	// Use this for initialization
	void Start () {
		ItemBoxObjs=new List<GameObject>();
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}

    public void DisplayBattleItem()
    {
        
    }
    public void InitWareHouseData(PackageType _PackageType, List<WareDisplayType> _wareDisplayType,DisplayType _diaDisplayType)
    {
        playerEquipDataObj.SetActive(false);
        PackageType = _PackageType;
        displayType = _diaDisplayType;
        switch (_PackageType)
        {
            case PackageType.冰箱:
                if (displayType == DisplayType.In)
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.icebox;
                    
                }
                else
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.icebox;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                }
                break;
            case PackageType.背包:
                playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                outPackage = int.MinValue;
                BoxNameText.text = "";
                BoxCountText.text = "";
                break;
            case PackageType.杂物箱:
                if (displayType == DisplayType.In)
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.box;
                }
                else
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.box;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                }
                break;
        }
        DisplayWarehouseItems(_PackageType,_wareDisplayType);
    }

    public void InitWareHouseData()
    {
        playerEquipDataObj.SetActive(false);
        switch (PackageType)
        {
            case PackageType.冰箱:
                if (displayType == DisplayType.In)
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.icebox;

                }
                else
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.icebox;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                }
                break;
            case PackageType.背包:
                playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                outPackage = int.MinValue;
                BoxNameText.text = "";
                BoxCountText.text = "";
                break;
            case PackageType.杂物箱:
                if (displayType == DisplayType.In)
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.box;
                }
                else
                {
                    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.box;
                    outPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                }
                break;
        }
        DisplayWarehouseItems(PackageType, wareDisplayTypes);
    }
    async void DisplayWarehouseItems(PackageType _PackageType, List<WareDisplayType> _wareDisplayTypes)
    { 
        wareDisplayTypes = _wareDisplayTypes;

        int outCaseCount = PackageManager.instance.GetPackageCaseCount(outPackage);
        List<Item> outItems = PackageManager.instance.GetPackageItems(outPackage);
        int outItemCount = outItems.Count;

        int playerCaseCount = PackageManager.instance.GetPackageCaseCount(playerPackage);
        List<Item> playerItems = PackageManager.instance.GetPackageItems(playerPackage);
       int playerItemCount = playerItems.Count;

        List<Item> dispayItems = new List<Item>();
        switch (displayType)
        {
            case DisplayType.In:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr("背包");
                BoxNameText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxCountText.text = $"({outItemCount}/{outCaseCount})";
                SellText.text = LanguageManage.SwitchStr("放入");
                break;
            case DisplayType.Sell:
                BoxNameText.text = "";
                BoxCountText.text = "";
                SellText.text = LanguageManage.SwitchStr("出售");
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(true);
                break;
            case DisplayType.Out:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxNameText.text = LanguageManage.SwitchStr("背包");
                BoxCountText.text = $"({playerItemCount}/{playerCaseCount})";
                SellText.text =  LanguageManage.SwitchStr("取出");
                break;
            case DisplayType.Equip:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxNameText.text = "";
                BoxCountText.text = "";
                SellText.text = LanguageManage.SwitchStr("选择");
                break;
            case DisplayType.AfterBattle:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxNameText.text = "";
                BoxCountText.text = "";
                SellText.text = LanguageManage.SwitchStr("使用");
                break;
            case DisplayType.Battling:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxNameText.text = "";
                BoxCountText.text = "";
                SellText.text = LanguageManage.SwitchStr("使用");
                break;
            case DisplayType.Normal:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxNameText.text = "";
                BoxCountText.text = "";
                SellText.text = LanguageManage.SwitchStr("使用");
                playerEquipDataObj.SetActive(true);
                playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().InitDataPlayerEquaipData(GameComponentData.gameData.gameManager.gamePlayer.id);
                break;
            case DisplayType.Gift:
                TitleButton.interactable = false;
                UpText.SetActive(false);
                DownText.SetActive(false);
                TitleText.text = LanguageManage.SwitchStr(PackageType.ToString());
                BoxNameText.text = "";
                BoxCountText.text = "";
                SellText.text = LanguageManage.SwitchStr("赠送");
                break;
        }
        switch (_PackageType)
        {
            case PackageType.背包:
                foreach (WareDisplayType wareDisplayType in wareDisplayTypes)
                {
                    switch (wareDisplayType)
                    {
                        case WareDisplayType.FarmTool:
                            foreach (var item in playerItems)
                            {
                                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
                                if (itemData.type == ItemType.种子)
                                {
                                    dispayItems.Add(item);
                                }
                            }
                            break;
                        case WareDisplayType.ALL:
                            dispayItems = playerItems;
                            break;
                        case WareDisplayType.Good:
                            dispayItems = playerItems;
                            break;
                        case WareDisplayType.Weapon:
                            foreach (var item in playerItems)
                            {
                                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
                                if (itemData.type == ItemType.武器)
                                {
                                    dispayItems.Add(item);
                                }
                            }
                            break;
                        case WareDisplayType.Clothes:
                            foreach (var item in playerItems)
                            {
                                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
                                if (itemData.type == ItemType.防具)
                                {
                                    dispayItems.Add(item);
                                }
                            }
                            break;
                        case WareDisplayType.food:
                            foreach (var item in playerItems)
                            {
                                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
                                if (itemData.type == ItemType.食物)
                                {
                                    dispayItems.Add(item);
                                }
                            }
                            break;
                        case WareDisplayType.药剂:
                            foreach (var item in playerItems)
                            {
                                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
                                if (itemData.type == ItemType.药剂)
                                {
                                    dispayItems.Add(item);
                                }
                            }
                            break;
                        case WareDisplayType.卷轴:
                            foreach (var item in playerItems)
                            {
                                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
                                if (itemData.type == ItemType.卷轴)
                                {
                                    dispayItems.Add(item);
                                }
                            }
                            break;
                    }
                }
               
                break;
            case PackageType.杂物箱:
                dispayItems = playerItems;
                break;
            case PackageType.冰箱:
                dispayItems = playerItems;
                break;
        }
        if (ItemBoxObjs == null)
        {
            ItemBoxObjs=new List<GameObject>();
        }
        foreach (Transform child in ItemBoxParent)
        {
            Destroy(child.gameObject);
        }
        ItemBoxObjs.Clear();

        if (addCaseObjs == null)
        {
            addCaseObjs=new List<GameObject>();
        }
        foreach (var addCaseObj in addCaseObjs)
        {
            Destroy(addCaseObj);
        }
        addCaseObjs.Clear();

        foreach (var packageItem in dispayItems)
        {
            GameObject itemBoxObj = Instantiate(ItemBoxPro);
            itemBoxObj.transform.localScale=Vector3.one;
            //itemBoxObj.GetComponent<ItemBoxAction>().InitItemData(packageItem,deskAction);
           // itemBoxObj.GetComponent<ItemBoxAction>().isWareDisplay = true;
            itemBoxObj.transform.SetParent(ItemBoxParent,false);
            itemBoxObj.GetComponentInChildren<Toggle>().group = ItemBoxParent.GetComponent<ToggleGroup>();
            ItemBoxObjs.Add(itemBoxObj);
        }

        ItemInfomationObj.SetActive(false);

        for (int i = 0; i < playerCaseCount - playerItemCount; i++)
        {
            GameObject NullBoxObj = Instantiate(NULLBoxPro) as GameObject;
            NullBoxObj.transform.SetParent(ItemBoxParent, false);
            NullBoxObj.GetComponent<ItemBoxReference>().Hide();
            NullBoxObj.GetComponentInChildren<Toggle>().group = ItemBoxParent.GetComponentInChildren<ToggleGroup>();
        }


        for (int i = 0; i < 5; i++)
        {
            GameObject addCaseObj = Instantiate(AddBoxPro);
            addCaseObj.transform.SetParent(ItemBoxParent,false);
            addCaseObjs.Add(addCaseObj);
            addCaseObj.GetComponent<AddCaseObjAction>().packageType = _PackageType;

        }

        CaseCounText.text = $"({playerItemCount}/{playerCaseCount})";
        
    }

    public void AddNullCase(PackageType packageType)
    {
        int playerCaseCount = PackageManager.instance.GetPackageCaseCount(playerPackage);
        List<Item> playerItems = PackageManager.instance.GetPackageItems(playerPackage);
        int playerItemCount = playerItems.Count;
        foreach (var addCaseObj in addCaseObjs)
        {
            Destroy(addCaseObj);
        }
        addCaseObjs.Clear();
        for (int i = 0; i < 5; i++)
        {
            GameObject NullBoxObj = Instantiate(NULLBoxPro) as GameObject;
            NullBoxObj.transform.SetParent(ItemBoxParent, false);
            NullBoxObj.GetComponent<ItemBoxReference>().Hide();
            NullBoxObj.GetComponentInChildren<Toggle>().group = ItemBoxParent.GetComponentInChildren<ToggleGroup>();
        }
        for (int i = 0; i < 5; i++)
        {
            GameObject addCaseObj = Instantiate(AddBoxPro);
            addCaseObj.transform.SetParent(ItemBoxParent, false);
            addCaseObjs.Add(addCaseObj);
            addCaseObj.GetComponent<AddCaseObjAction>().packageType = PackageType;
        }
        PackageManager.instance.AddPackageCaseCount(playerPackage, 5); 
        CaseCounText.text = $"({playerItemCount}/{playerCaseCount+5})";
        AudioController.instance.PlayAudio(SE.Click2);
    }
    public async void ClickItem(Item item)
    {
        AudioController.instance.PlayAudio(SE.select);

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());

        if (itemData!=null)
        {
            ItemInfomationObj.SetActive(true);
            selectedItem = item;
            ItemIconImage.sprite = itemData.icon;
            ItemNameText.text = itemData.name;
            ItemTypeText.text = LanguageManage.SwitchStr(itemData.type.ToString());
            ItemPriceText.text = itemData.sellPrice + "G";
            ItemPropertyText.text = itemData.text1;
            ItemNoticeText.text = itemData.text2;
            sellButton.interactable = true;
            sellButton.gameObject.SetActive(true);
            
            if (displayType == DisplayType.Normal)
            {
                if (itemData.type == ItemType.武器 ||itemData.type == ItemType.防具)
                {
                    sellButton.gameObject.SetActive(true);
                    SellText.text = LanguageManage.SwitchStr("选择");
                    playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().SelectItem(selectedItem.dataId);
                }
                else if (itemData.type == ItemType.食物||itemData.type==ItemType.其他物品)
                {
                    sellButton.gameObject.SetActive(true);
                    SellText.text = LanguageManage.SwitchStr("使用");
                }
                else if(itemData.type==ItemType.药剂&&itemData.typeValue!=-1)
                {
                    sellButton.gameObject.SetActive(true);
                    SellText.text = LanguageManage.SwitchStr("使用");
                }
                else
                {
                    sellButton.gameObject.SetActive(false);
                }
            }
           

        }
        else
        {
            ItemInfomationObj.SetActive(false);
        }

    }

    public async void ClickSellButton()
    {
        int outCaseCount = PackageManager.instance.GetPackageCaseCount(outPackage);
        List<Item> outItems = PackageManager.instance.GetPackageItems(outPackage);
        int outItemCount = outItems.Count;
        ItemData selectItemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectedItem.dataId.ToString()); 
        switch (displayType)
        {
            case DisplayType.Sell:
                AudioController.instance.PlayAudio(SE.click);
                SellSelectObj.SetActive(true);
               // SellSelectObj.GetComponent<SellSellectAction>().InitSellSelectData(selectedItem);

                break;
            case DisplayType.In:

                if (selectItemData.isFresh && outPackage != GameComponentData.gameData.gameManager.gamePlayer.icebox)
                {
                    AudioController.instance.PlayAudio(SE.Return);
                    GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("生鲜物品可放入冰箱，无法放入杂物箱!"));
                }
                else if (!selectItemData.isFresh && outPackage == GameComponentData.gameData.gameManager.gamePlayer.icebox)
                {
                    AudioController.instance.PlayAudio(SE.Return);
                    GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("非生鲜物品不能放入冰箱!"));
                }
                else
                {
                    AudioController.instance.PlayAudio(SE.click);

                    int outCount = await PackageManager.instance.SetItemInPackage(selectedItem, outPackage);
                    RemovePackageItem removePackageItem = new RemovePackageItem
                    {
                        packageId = playerPackage,
                        itemDataId = selectedItem.dataId,
                        itemCount = selectedItem.count - outCount
                    };
                    GameActionManager.instance.QueueAction(removePackageItem, true);
                    DisplayWarehouseItems(PackageType, wareDisplayTypes);
                    outItemCount = PackageManager.instance.GetPackageItems(outPackage).Count;
                    BoxCountText.text = $"({outItemCount}/{outCaseCount})";
                    if (outCount > 0)
                    {
                        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),
                             LanguageManage.SwitchStr("空间不足！"));
                    }
                     
                }

                break;
            case DisplayType.Out:
                {
                    AudioController.instance.PlayAudio(SE.click);
                    int outCount = await PackageManager.instance.SetItemInPackage(selectedItem, outPackage);
                    if (outCount > 0)
                    {
                        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),
                            LanguageManage.SwitchStr("背包") + LanguageManage.SwitchStr("空间不足！"));
                    }
                    RemovePackageItem removePackageItem = new RemovePackageItem
                    {
                        packageId = playerPackage,
                        itemDataId = selectedItem.dataId,
                        itemCount = selectedItem.count - outCount
                    };

                    GameActionManager.instance.QueueAction(removePackageItem,true);
                    DisplayWarehouseItems(PackageType, wareDisplayTypes);
                    outItemCount = PackageManager.instance.GetPackageItems(outPackage).Count;
                    BoxCountText.text = $"({outItemCount}/{outCaseCount})";
                }
               
                break;
            case DisplayType.Equip:
                {
                    AudioController.instance.PlayAudio(SE.click);
                    Item farmTool = GameComponentData.gameData.farmAction.farmTool;
                    if (farmTool.instanceId != 0)
                    {
                        PackageManager.instance.SetItemInPackage(farmTool, 0);
                    }
                    functionButtons.SetActive(true);
                    GameComponentData.gameData.farmAction.InitFarmTool(selectedItem);
                    transform.parent.gameObject.SetActive(false);

                    RemovePackageItem removePackageItem = new RemovePackageItem
                    {
                        packageId = playerPackage,
                        itemDataId = selectedItem.dataId,
                        itemCount = selectedItem.count
                    };
                    GameActionManager.instance.QueueAction(removePackageItem, true);
                }
                
                break;
            case DisplayType.Normal:
                AudioController.instance.PlayAudio(SE.click);
                if (selectItemData.type == ItemType.武器 || selectItemData.type == ItemType.防具)
                {
                    playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().EuqipMentAction();
                    ItemInfomationObj.SetActive(false);
                }
                else if(selectItemData.type==ItemType.食物|| (selectItemData.type == ItemType.药剂&&selectItemData.typeValue!=-1))
                {
                    UseItem();
                }
                else if (selectItemData.type == ItemType.其他物品)
                {
                    GameComponentData.gameData.formulaAction.OpenFormula(selectItemData.typeValue,selectItemData.id);
                    RemovePackageItem removePackageItem = new RemovePackageItem
                    {
                        packageId = playerPackage,
                        itemDataId = selectedItem.dataId,
                        itemCount = 1
                    };
                    GameActionManager.instance.QueueAction(removePackageItem, true); 
                    InitWareHouseData();
                }
                break;
            case DisplayType.AfterBattle:
                AudioController.instance.PlayAudio(SE.Click2);
                UseItem(); 
                //GameComponentData.gameData.BattleMapAction.UseItem(selectItemData,DisplayType.AfterBattle);
                break;
            case DisplayType.Battling:
                AudioController.instance.PlayAudio(SE.Click2);
                UseItem(); 
                //GameComponentData.gameData.BattleMapAction.UseItem(selectItemData,DisplayType.Battling);
                break;
            case DisplayType.Gift:
                AudioController.instance.PlayAudio(SE.Click2);
                //GameComponentData.gameData.gameManager.gamePlayer.package.GetItemOutPackage(selectedItem.ItemId,1);
                GameComponentData.gameData.NpcManager.selectNpcx.AddGiftFriendllyExp(selectedItem.dataId);

                GameComponentData.gameData.warehouseObj.SetActive(false);
                break;
        }
        
    }

    public void ClickReturnButton()
    {
       
        AudioController.instance.PlayAudio(SE.Return);
        if (GameComponentData.gameData.passDataManager.NowPassData.id == 1001)
        {
            GameComponentData.gameData.gameManager.fieldTool.SetActive(true);
        }
        transform.parent.gameObject.SetActive(false);
       
    }
    async void UseItem()
    {
        ItemData selectItemData1 = await GameDataManager.instance.GetAsyncData<ItemData>(selectedItem.dataId.ToString()); 
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        if (selectItemData1.type == ItemType.食物 )
        {
            string noticeStr = LanguageManage.SwitchStr("使用了1个:") + selectItemData1.name;
            string propertyStr = "";
            if (selectItemData1.property.AT > 0)
            {
                propertyStr += LanguageManage.SwitchStr(",AT增加了") + selectItemData1.property.AT;
            }
            if (selectItemData1.property.DF > 0)
            {
                propertyStr += LanguageManage.SwitchStr(",DF增加了")+ selectItemData1.property.DF;
            }
            if (selectItemData1.property.HP > 0)
            {
                propertyStr += LanguageManage.SwitchStr(",HP增加了")+ selectItemData1.property.HP;
            }
            if (selectItemData1.property.MaxHP > 0)
            {
                propertyStr += LanguageManage.SwitchStr(",HP最大值增加了") + selectItemData1.property.MaxHP;
            }
            if (selectItemData1.property.Power > 0)
            {
                propertyStr += LanguageManage.SwitchStr(",体力值增加了") + selectItemData1.property.Power;
            }
            if (selectItemData1.property.HP < 0)
            {
                propertyStr += LanguageManage.SwitchStr("HP全满") + selectItemData1.property.Power;
            }

            InformationController.instance.AddInformation("*" + noticeStr + propertyStr);
          
            if (gamePlayer.property.HP > 0)
            {
                gamePlayer.property += selectItemData1.property;
            }
            if (selectItemData1.property.HP < 0)
            {
                gamePlayer.property.HP = gamePlayer.property.MaxHP;
            }
            if (gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0&&gamePlayer.TeamPlayer0.property.HP>0)
            {
                gamePlayer.TeamPlayer0.property += selectItemData1.property;
                gamePlayer.TeamPlayer0.AddHpValue(0);
            }
            if (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.property.HP > 0)
            {
                gamePlayer.TeamPlayer1.property += selectItemData1.property;
                gamePlayer.TeamPlayer1.AddHpValue(0);
            }
            AudioController.instance.PlayAudio(SE.Heal);
            GameComponentData.gameData.gameManager.UpDataPlayer();
            playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().InitDataPlayerEquaipData();
            GameComponentData.gameData.intelligencePanelAction.InitIntelligenceData();
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("道具使用"), noticeStr + propertyStr);

            RemovePackageItem removePackageItem = new RemovePackageItem
            {
                packageId = gamePlayer.package,
                itemCount = 1,
                itemDataId = selectedItem.dataId
            };
            GameActionManager.instance.QueueAction(removePackageItem, true);
             
            InitWareHouseData();
        }
        else if (selectItemData1.type == ItemType.药剂)
        {
            AudioController.instance.PlayAudio(SE.Heal);
            GameComponentData.gameData.gameManager.gamePlayer.attributeType = (AttributeType) selectItemData1.typeValue;
            playerEquipDataObj.GetComponent<PlayerEquipDataActiion>().ChangePlayerDrop();
            RemovePackageItem removePackageItem = new RemovePackageItem
            {
                packageId = gamePlayer.package,
                itemCount = 1,
                itemDataId = selectedItem.dataId
            };
            GameActionManager.instance.QueueAction(removePackageItem, true);
            InitWareHouseData();
        }
        else if(selectItemData1.type==ItemType.卷轴)
        {
            RemovePackageItem removePackageItem = new RemovePackageItem
            {
                packageId = gamePlayer.package,
                itemCount = 1,
                itemDataId = selectedItem.dataId
            };
            GameActionManager.instance.QueueAction(removePackageItem, true);
            InitWareHouseData();
        }
    }
    public void SellItem(int sellCount)
    {

        Item sellItem = ItemManager.instance.CreatItem(selectedItem.dataId, sellCount);
        Item deskItem = deskAction.item;
        if (deskItem.dataId!= 0)
        {
            PackageManager.instance.SetItemInPackage(deskAction.item, deskAction.GoldPackage);
        }

        deskAction.InitDeskData(sellItem,playerPackage);

        RemovePackageItem removePackageItem = new RemovePackageItem
        {
            itemCount = sellCount,
            itemDataId = selectedItem.dataId,
            packageId = playerPackage
        };
        GameActionManager.instance.QueueAction(removePackageItem, true);
         
        functionButtons.SetActive(true);
        SellSelectObj.SetActive(false);
        gameObject.transform.parent.gameObject.SetActive(false);
      

    }
	// Update is called once per frame
	void Update () {
		
	}
}
