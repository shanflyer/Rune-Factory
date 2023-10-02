using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelAction : MonoBehaviour
{
    public Text TitleText, ReturnText, BuyText,GuishuText;
    
    public Image moneyIcon0, moneyIcon1;
    public Text NameText;
    public Text ItemInformationNameText,
        ItemInformationPriceText,
        ItemInformationGrowText,
        ItemInformationText,
        ItemSeasonText;
    public Text MyMoneyText, TotalMoneyText;
    public Text BuyItemCountText,MyItemText;
    public Transform ItemParent,PackageParent;
    public GameObject ShopItemPro,PackageSelectPro;
    public Image ItemInformationImage;
    public Button ReduceButton, AddButton,BuyButton;
    public GameObject pastureSelectObj;
    public Dropdown PastureDropdown;
    public Image NpcImage;
    private Shop shop;
    private int package;
    private ItemData selectItemData;
    private int SelectCount,totalPrice;
    private int playerPackage;
    private Pasture pasture;
    private AnimalData animalData;
    private List<int> pastureIds;
    public void InitShopPanelData(Shop _shop)
    {
        
        shop = _shop;

        if (shop.shopType == ShopType.工具店)
        {
            ShopPackage shopPackage = shop.ShopPackages[1];
           
            foreach (var shopItem in shopPackage.ShopItems)
            {
               //Formula formula=GameComponentData.gameData.formulaAction.Formulas.Find(f => f.Product == shopItem.ItemId);
               // if (formula != null&&formula.isOpen)
               // {
               //     shopItem.IsOpen = true;
                    
               //    // shop.packages[1].SetItemXInPackage(shopItem.ItemId);
               // }
               
            }
           
        }

        NPCData npcData = GameComponentData.gameData.NpcManager.NpcDatas.Find(n => n.id == _shop.Npcid);
        NpcImage.sprite = GameComponent.headIcons.Find(h => h.name == npcData.headName);

        MyMoneyText.text = GameComponentData.gameData.gameManager.gamePlayer.money.ToString();
        foreach (var shopShopPackage in shop.ShopPackages)
        {
            GameObject SelectObj = Instantiate(PackageSelectPro);
            SelectObj.transform.SetParent(PackageParent);
            SelectObj.transform.localScale = Vector3.one;
            SelectObj.GetComponent<PackageSelectAction>().shopPanelAction = this;
            SelectObj.GetComponentInChildren<Toggle>().group = PackageParent.GetComponent<ToggleGroup>();
            SelectObj.GetComponentInChildren<Text>().text = shopShopPackage.PackageName;
            
        }
        PackageParent.GetChild(0).GetComponentInChildren<Toggle>().isOn = true;
        ZeroSelectItem();

    }
    public async void Add()
    {
        AudioController.instance.PlayAudio(SE.click);
        SelectCount++;
        if (selectItemData.typeValue == 8)
        {
            int nullCase = pasture.caseCount-pasture.animalCaseCount;
            if (animalData.caseCount * SelectCount <= nullCase)
            {
                ReduceButton.interactable = true;
                BuyButton.interactable = true;
                BuyItemCountText.text = SelectCount.ToString();
                totalPrice = SelectCount * selectItemData.sellPrice;
                TotalMoneyText.text = LanguageManage.SwitchStr("总计:") + totalPrice + "G";
            }
            else
            {
                SelectCount--;
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("选择的牧场空间已达到上限。"));
            }
        }
        else
        {
            if (await PackageManager.instance.CheckPackageTryItemIn(package,selectItemData.id,SelectCount))
            {

                ReduceButton.interactable = true;
                BuyButton.interactable = true;
                BuyItemCountText.text = SelectCount.ToString();
                totalPrice = SelectCount * selectItemData.shopPrice;
                TotalMoneyText.text = LanguageManage.SwitchStr("总计:") + totalPrice + "G";
            }
            else
            {
                SelectCount--;
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("选择的道具数量已达到背包容量上限。"));
            }
        }
       

        
    }

    public void BuyAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        if (selectItemData.typeValue == 8)
        {
            GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("购买动物"), totalPrice, LanguageManage.SwitchStr("购买")
                + SelectCount + LanguageManage.SwitchStr("个 ") + selectItemData.name + " ?", CostType.购买道具
                ,selectItemData.shopMoneyType);
        }
        if (selectItemData.typeValue == 9)
        {
            GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("购买设施"), totalPrice, LanguageManage.SwitchStr("购买") 
                + SelectCount + LanguageManage.SwitchStr("个 ") + selectItemData.name + " ?", CostType.购买设施
                , selectItemData.shopMoneyType);
        }
        else
        {
            GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("购买道具"), totalPrice,LanguageManage.SwitchStr("购买")
                + SelectCount + LanguageManage.SwitchStr("个 ") + selectItemData.name + " ?", CostType.购买道具
                , selectItemData.shopMoneyType);
        }
       
    }
    public void BuySucecssful()
    {
        MyMoneyText.text = GameComponentData.gameData.gameManager.gamePlayer.money.ToString();
        if (selectItemData.shopMoneyType == ShopMoneyType.金币)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*消耗金币")
                + totalPrice + LanguageManage.SwitchStr(",购买了") + SelectCount + LanguageManage.SwitchStr("个 ") + selectItemData.name);
        }
        else
        {
            InformationController.instance.AddInformation(
                LanguageManage.SwitchStr("*消耗红晶") + totalPrice + LanguageManage.SwitchStr(",购买了") 
                + SelectCount + LanguageManage.SwitchStr("个 ") + selectItemData.name);
        }
        
        if (selectItemData.typeValue == 8)
        {
            GameComponentData.gameData.pastureAction.CreatAnimal(pasture,animalData,SelectCount);
            
        }
        else if (selectItemData.type==ItemType.其他物品&&selectItemData.typeValue==9)
        {
           
            Euqipment euqipment =
                GameComponentData.gameData.equipmentManager.Euqipments.Find(e => e.shopItem== selectItemData.id);
            if (euqipment.id == 1265)
            {
                DataSaveAndLoadTest.gameSaveData.marryData.SaveBabyBedTime();
            }

            euqipment.isBuy = true;
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("购买设施"),
                LanguageManage.SwitchStr("成功购买了")+euqipment.name+LanguageManage.SwitchStr(",已经送货到您家。"));
            
        }
        else
        {
            AddPackageItem addPackageItem = new AddPackageItem
            {
                packageId = package,
                itemDataId = selectItemData.id,
                itemCount = SelectCount
            };
            GameActionManager.instance.QueueAction(addPackageItem,true); 
        }
        
        foreach (Transform child in ItemParent)
        {
            child.GetComponentInChildren<Toggle>().isOn = false;
        }
        ZeroSelectItem();
    }
    public void Reduce()
    {
        AudioController.instance.PlayAudio(SE.click);
        SelectCount--;
        if (SelectCount <= 0)
        {
            SelectCount = 0;
            ReduceButton.interactable = false;
            BuyButton.interactable = false;
        }
        else
        {
            BuyButton.interactable = true;
        }
        BuyItemCountText.text = SelectCount.ToString();
        totalPrice = SelectCount * selectItemData.shopPrice;
        TotalMoneyText.text = LanguageManage.SwitchStr("总计:")+totalPrice+"G";
    }
    public void ReturnAction()
    {
        AudioController.instance.PlayAudio(SE.Return);
        foreach (Transform child in PackageParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in ItemParent)
        {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }

    public async void SelectPackage(int packageId)
    {
        foreach (Transform child in ItemParent)
        {
            Destroy(child.gameObject);
        }
        package = packageId;

        var items = PackageManager.instance.GetPackageItems(packageId);
        for(int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            ItemData itemData =await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());
            GameObject shopItemObj = Instantiate(ShopItemPro);
            shopItemObj.transform.SetParent(ItemParent);
            shopItemObj.GetComponent<ShopItemAction>().InitData(itemData, this);
            shopItemObj.GetComponentInChildren<Toggle>().group = ItemParent.GetComponent<ToggleGroup>();
            shopItemObj.transform.localScale = Vector3.one;
        }
         
    }

    public void DropValueChange(Dropdown dropdown)
    {
        AudioController.instance.PlayAudio(SE.select);
        string pastureName = dropdown.options[dropdown.value].text.Split('(')[0];
        pasture = GameComponentData.gameData.pastureAction.Pastures.Find(p => p.id == pastureIds[dropdown.value]);
        
        if (animalData != null)
        {
           
            int nullCase = pasture.caseCount - pasture.animalCaseCount;
            if (animalData.caseCount * SelectCount > nullCase)
            {
                for (int i = 1; i <= SelectCount; i++)
                {
                    SelectCount--;
                    if (animalData.caseCount * SelectCount <= nullCase)
                    {
                        break;
                    }
                }
            }
            BuyItemCountText.text = SelectCount.ToString();

        }
        
    }
    public async void SelectItem(ItemData _itemData)
    {
        AudioController.instance.PlayAudio(SE.select);
        selectItemData = _itemData;
        
        if (selectItemData.type==ItemType.消耗物品&&selectItemData.typeValue == 8)
        {
            pastureSelectObj.SetActive(true);
            animalData = GameComponentData.gameData.pastureAction.AnimalDatas.Find(a => a.shopItem == _itemData.id);
            ItemData fruit = await GameDataManager.instance.GetAsyncData<ItemData>(animalData.produceItem.ToString());
            ItemSeasonText.text = LanguageManage.SwitchStr("占用空间:")+animalData.caseCount;
            ItemInformationGrowText.text = LanguageManage.SwitchStr("产出物:") + fruit.name + LanguageManage.SwitchStr("天")+ LanguageManage.SwitchStr("产出间隔:")
                +animalData.produceCD;
            ItemInformationPriceText.text = LanguageManage.SwitchStr("动物单价:") + selectItemData.shopPrice + "G,"+ LanguageManage.SwitchStr("作物单价:") + fruit.sellPrice + "G";

            if (PastureDropdown.interactable)
            {
                AddButton.interactable = true;
                ReduceButton.interactable = true;

            }
            else
            {
                AddButton.interactable = false;
                ReduceButton.interactable = false;
            }

        }
        else
        {
            pastureSelectObj.SetActive(false);
            int haveCount = PackageManager.instance.GetPackageItemCount(0, selectItemData.id);
            if (_itemData.type==ItemType.种子)
            {
                PlantBaseData plant = GameComponentData.gameData.plantAction.PlantBaseDatas.Find(p => p.SeedId == _itemData.id);
                ItemData fruit = await GameDataManager.instance.GetAsyncData<ItemData>(plant.fruitId.ToString());
                ItemInformationGrowText.text = LanguageManage.SwitchStr("生长天数:") + plant.GrowthDays + LanguageManage.SwitchStr("天") + LanguageManage.SwitchStr("收获数:") + plant.fruitIdNum + LanguageManage.SwitchStr("个");
                ItemInformationPriceText.text = LanguageManage.SwitchStr("种子单价:") + selectItemData.shopPrice + "G"+ LanguageManage.SwitchStr("作物单价:") + fruit.sellPrice + "G";
                string seasonStr = LanguageManage.SwitchStr("适应季节:");
                foreach (var plantAdvantageSeason in plant.AdvantageSeasons)
                {
                    seasonStr += LanguageManage.SwitchStr(plantAdvantageSeason.ToString()) + ",";
                }
                ItemSeasonText.text = seasonStr;

                MyItemText.text = LanguageManage.SwitchStr($"(持有{haveCount}个)");
                BuyItemCountText.text = "0";
                TotalMoneyText.text = LanguageManage.SwitchStr("总计:")+"0G";
                ReduceButton.interactable = true;
                AddButton.interactable = true;
                SelectCount = 0;
            }
            else if(selectItemData.typeValue==9&&selectItemData.type==ItemType.其他物品)
            {
                Euqipment euqipment =
                    GameComponentData.gameData.equipmentManager.Euqipments.Find(
                        e => e.shopItem  == selectItemData.id);
                AddButton.interactable = false;
                ReduceButton.interactable = false;
                BuyItemCountText.text = "1";
                if (euqipment.isBuy)
                {
                    MyItemText.text = LanguageManage.SwitchStr("已购入");
                    BuyButton.interactable = false;
                }
                else
                {
                    MyItemText.text = LanguageManage.SwitchStr("未购入");
                    BuyButton.interactable = true;
                }
                ItemSeasonText.text = "";
                ItemInformationGrowText.text = "";
                ItemInformationPriceText.text = LanguageManage.SwitchStr("单价:") + selectItemData.shopPrice;
                
                TotalMoneyText.text = LanguageManage.SwitchStr("总计:")+selectItemData.shopPrice+"G";
                SelectCount = 1;
                totalPrice = selectItemData.shopPrice;

            }
            else
            {
                

                MyItemText.text = LanguageManage.SwitchStr($"(持有{haveCount}个)");
                BuyItemCountText.text = "0";
                TotalMoneyText.text = LanguageManage.SwitchStr("总计:0G");
                ReduceButton.interactable = true;
                AddButton.interactable = true;
                SelectCount = 0;
                ItemInformationPriceText.text = "";
                ItemInformationGrowText.text = selectItemData.text1;
                ItemSeasonText.text = "";

            }
        

            
            
            
        }
        ItemInformationNameText.text = selectItemData.name;
        ItemInformationText.text = selectItemData.text2;
        
        ItemInformationImage.enabled = true;
        ItemInformationImage.sprite = selectItemData.icon;


        if (selectItemData.shopMoneyType==ShopMoneyType.金币)
        {
            moneyIcon0.enabled = true;
            moneyIcon1.enabled = false;
            MyMoneyText.text= GameComponentData.gameData.gameManager.gamePlayer.money.ToString();
        }
        else
        {
            moneyIcon0.enabled = false;
            moneyIcon1.enabled = true;
            MyMoneyText.text = GameComponentData.gameData.gameManager.gamePlayer.money1.ToString();
        }
        //BuyButton.interactable = true;
    }
    public void ZeroSelectItem()
    {
        NameText.text = shop.Name;
        selectItemData = null;
        SelectCount = 0;
        moneyIcon0.enabled = false;
        moneyIcon1.enabled = false;
        MyMoneyText.text = "";
        ItemInformationGrowText.text = "";
        ItemInformationNameText.text = "";
        ItemInformationText.text = "";
        ItemInformationPriceText.text = LanguageManage.SwitchStr("要来点什么？");
        BuyItemCountText.text = "";
        MyItemText.text = "";
        ItemSeasonText.text = "";
        TotalMoneyText.text = "";
        ItemInformationImage.enabled = false;
        ReduceButton.interactable = false;
        AddButton.interactable = false;
        BuyButton.interactable = false;
        switch (shop.shopType)
        {
                case ShopType.动物店:
                    pastureSelectObj.SetActive(true);
                    PastureDropdown.options = new List<Dropdown.OptionData>();
                    PastureAction pastureAction = GameComponentData.gameData.pastureAction;
                    pastureIds=new List<int>();
                    for (int i = 0; i < pastureAction.IsPastures.Length; i++)
                    {
                        if (pastureAction.IsPastures[i])
                        {
                            string pastureStr = pastureAction.Pastures[i].name + "(" +
                                                pastureAction.Pastures[i].animalCaseCount+ "/" +
                                                pastureAction.Pastures[i].caseCount + ")";
                            Dropdown.OptionData optionData = new Dropdown.OptionData(pastureStr);
                        
                            PastureDropdown.options.Add(optionData);
                        pastureIds.Add(pastureAction.Pastures[i].id);
                        }
                    }
                    if (PastureDropdown.options.Count > 0)
                    {
                        PastureDropdown.value = 0;
                        PastureDropdown.captionText.text = PastureDropdown.options[0].text;
                        PastureDropdown.interactable = true;
                        
                    }
                    else
                    {
                        Dropdown.OptionData optionData = new Dropdown.OptionData(LanguageManage.SwitchStr("没有任何牧场"));
                        PastureDropdown.options.Add(optionData);
                        PastureDropdown.interactable = false;
                        
                    }
                break;
                case ShopType.家具店:
                break;
            default:
                pastureSelectObj.SetActive(false);
                break;
        }

    }
	// Use this for initialization
	void Start () {
	    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
	    TitleText.text = LanguageManage.SwitchStr(TitleText.text);
	    ReturnText.text = LanguageManage.SwitchStr(ReturnText.text);
	    GuishuText.text = LanguageManage.SwitchStr(GuishuText.text);
        LanguageManage.TextFanyi(BuyText);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
