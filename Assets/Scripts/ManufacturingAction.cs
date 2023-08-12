using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public enum PackageType
{
    背包=0,
    杂物箱=1,
    冰箱=2
}

public struct PackageItem
{
    public int itemId;
    public List<int> packageCount;

    public PackageItem(int _itemid)
    {
        itemId = _itemid;
        packageCount=new List<int>();
    }
}
public class ManufacturingAction : MonoBehaviour
{
    public GameObject formualSelectObj;
    public Toggle WeaponToggle, EuqipToggle;
    public List<Text> FunctionTexts;


    public Text ReturnButtonText,
        SucaiText,
        PeifangText,
        ChangchuText,
        ZidongText,
        ZhizhuoText,XiaohaoText,TianjiabuttonText,YichuButtonText,BaibaoText, BaibaoText0,
        ChuwuxiangText,BingxiangText;


    public Text RPcostValueText, RPTotalText;

    public Text TitleText;
    public List<ItemBoxAction> stuffs;
    public GameObject product;
    public Text ProductCountText;
    public Dropdown FormulaDropdown;
    public Button UpButton, DownButton, AutoSelectButton, ProduceButton;
    public Text ItemNameText, ItemPriceText, ItemTypeText, ItemNoticeText, ItemNoticeText1;
    public Button GetOutButton, GetInButton;
    public int DefaultItemId;
    public GameObject ItemPro;
    public Sprite DefaultSprite;
    public Transform ItemParent;
    public int produceCount;
    private FormulaType formulaType;
    private List<Formula> formulas;
    private Formula formula;
    private Package package;
    private ItemBoxAction SelectItem;
    private bool isMatch;
    private List<PackageItem> PackageItemCounts;
    private Item produceItem;
    private int RpCostValue;
    public void InitMarufacturingData(FormulaType _formulaType)
    {
        if (_formulaType == FormulaType.装备)
        {
            formualSelectObj.SetActive(true);
            WeaponToggle.isOn = true;
            EuqipToggle.isOn = true;
        }
        else
        {
            formualSelectObj.SetActive(false);
        }
        foreach (var functionText in FunctionTexts)
        {
            LanguageManage.TextFanyi(functionText);
        }
        RpCostValue = 0;
        isMatch = false;
        formulaType = _formulaType;
        PackageItemCounts=new List<PackageItem>();
        foreach (var stuff in stuffs)
        {
            stuff.GetComponent<ItemBoxAction>().Hide();
            stuff.GetComponent<ItemBoxAction>().item = null;
            stuff.GetComponent<ItemBoxAction>().isFull = false;
        }
        product.GetComponent<ItemBoxAction>().Hide();
        switch (formulaType)
        {
            case FormulaType.药剂:
                TitleText.text = "药剂配置";
                break;
            case FormulaType.装备:
                TitleText.text = LanguageManage.SwitchStr("装备制造");
                break;
            case FormulaType.酒水:
                TitleText.text = LanguageManage.SwitchStr("酒水酿造");
                break;
            case FormulaType.冷食:
                TitleText.text = LanguageManage.SwitchStr("食物制作");
                break;
            case FormulaType.热食:
                TitleText.text = LanguageManage.SwitchStr("食物烹饪");
                break;
            default:
                break;
        }
        CreatDropDown();
        FormulaDropdown.value = 0;
        FormulaDropdown.captionText.text = FormulaDropdown.options[0].text;
        produceCount = 1;
        ProductCountText.text = produceCount.ToString();
        RPcostValueText.text = "0";
        RPTotalText.text = "("+GameComponentData.gameData.gameManager.gamePlayer.property.Power+")";
        DisplayMyBox(PackageType.背包);
        ProduceButton.gameObject.SetActive(false);
        DownButton.gameObject.SetActive(false);
        UpButton.gameObject.SetActive(false);
        HideItemInformation();
        ZeroStuff();
        AutoSelectButton.gameObject.SetActive(false);
  
    }
    
    public void DisplayFormualList()
    {
        CreatDropDown();
    }
    public void ZeroStuff()
    {
        foreach (var stuff in stuffs)
        {
           stuff.GetComponent<ItemBoxAction>().ZeroData();
        }
    }

    void CreatDropDown()
    {
        if (formulaType == FormulaType.装备)
        {
            if (!WeaponToggle.isOn&&!EuqipToggle)
            {
                formulas=new List<Formula>();
            }
            else if (!WeaponToggle.isOn)
            {
                formulas = GameComponentData.gameData.formulaAction.Formulas.FindAll(f => f.formulaType == formulaType && f.isOpen&&
                GameComponentData.gameData.itemsManager.ItemDataList.Find(i=>i.Id==f.Product).Type==ItemType.防具);
            }
            else if(!EuqipToggle.isOn)
            {
                formulas = GameComponentData.gameData.formulaAction.Formulas.FindAll(f => f.formulaType == formulaType && f.isOpen &&
                                                                                          GameComponentData.gameData.itemsManager.ItemDataList.Find(i => i.Id == f.Product).Type == ItemType.武器);
            }else
            {
                formulas = GameComponentData.gameData.formulaAction.Formulas.FindAll(f => f.formulaType == formulaType && f.isOpen);
            }
        }
        else
        {
            formulas = GameComponentData.gameData.formulaAction.Formulas.FindAll(f => f.formulaType == formulaType && f.isOpen);
        }
        


        FormulaDropdown.options = new List<Dropdown.OptionData> {new Dropdown.OptionData(LanguageManage.SwitchStr("不使用配方"))};
        foreach (var _formula in formulas)
        {
            if (_formula.isOpen)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData(_formula.name);
                FormulaDropdown.options.Add(optionData);
            }
        
        }
    }
    void HideStuffAfterCreat()
    {
        if (formula != null && formula.isOpen)
        {
            foreach (var stuff in stuffs)
            {
                stuff.GetComponent<ItemBoxAction>().DisPlayFormulaItem();
            }
            product.GetComponent<ItemBoxAction>().DisPlayFormulaItem();
        }
        else
        {
            foreach (var stuff in stuffs)
            {
                stuff.GetComponent<ItemBoxAction>().ZeroData();
            }
            product.GetComponent<ItemBoxAction>().ZeroData();
        }
        ProductCountText.text = "1";
       AutoSelectButton.gameObject.SetActive(false);
       ProduceButton.gameObject.SetActive(false);
        UpButton.gameObject.SetActive(false);
        DownButton.gameObject.SetActive(false);
        HideItemInformation();
        int index = FormulaDropdown.value;
        CreatDropDown();
        FormulaDropdown.value = index;
        FormulaDropdown.captionText.text = FormulaDropdown.options[index].text;
    }
    public void DisplayBoxItem(Package _package)
    {
        foreach (Transform child in ItemParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var packageItem in package.items)
        {
            GameObject ItemObj = Instantiate(ItemPro);
            
            ItemObj.transform.SetParent(ItemParent,true);
            ItemObj.transform.localScale = Vector3.one;
            ItemObj.GetComponentInChildren<Toggle>().group =GetComponentInChildren<ToggleGroup>();
            ItemBoxAction itemBoxAction = ItemObj.GetComponent<ItemBoxAction>();
            itemBoxAction.InitItemData(packageItem);
            itemBoxAction.mask.enabled = false;
            itemBoxAction.isBox = true;
            if (formulaType == FormulaType.冷食|| formulaType == FormulaType.热食 || formulaType == FormulaType.酒水)
            {
                ItemData itemData =
                    GameComponentData.gameData.itemsManager.GetItemDataFromId(packageItem.ItemId);
                if (!itemData.IsFresh)
                {
                    itemBoxAction.mask.enabled = true;
                }
            }
        }
    }
    public void DisplayMyBox(PackageType packageType)
    {
        foreach (Transform child in ItemParent)
        {
            Destroy(child.gameObject);
        }
        switch (packageType)
        {
            case PackageType.背包:
                package = GameComponentData.gameData.gameManager.gamePlayer.package;
                break;
            case PackageType.杂物箱:
                package = GameComponentData.gameData.gameManager.gamePlayer.box;
                break;
            case PackageType.冰箱:
                package = GameComponentData.gameData.gameManager.gamePlayer.icebox;
                break;
        }
        foreach (var packageItem in package.items)
        {
            GameObject ItemObj = Instantiate(ItemPro);
            
            ItemObj.transform.SetParent(ItemParent,true);
            ItemObj.transform.localScale = Vector3.one;
            ItemObj.GetComponentInChildren<Toggle>().group = gameObject.GetComponentInChildren<ToggleGroup>();
            ItemBoxAction itemBoxAction = ItemObj.GetComponent<ItemBoxAction>();
            itemBoxAction.InitItemData(packageItem);
            itemBoxAction.mask.enabled = false;
            itemBoxAction.isBox = true;
            itemBoxAction.package = package;
            if (formulaType == FormulaType.热食||formulaType==FormulaType.冷食 || formulaType == FormulaType.酒水)
            {
                ItemData itemData =
                    GameComponentData.gameData.itemsManager.GetItemDataFromId(packageItem.ItemId);
                if (!itemData.IsFresh)
                {
                    itemBoxAction.mask.enabled = true;
                }
            }
        }
    }

    public bool  CheckFormula()
    {
      
        var x = stuffs.FindAll(s => s.GetComponent<ItemBoxAction>().isFull);
        if (x.Count == formula.Stuffs.Count)
        {
            foreach (var obj in x)
            {
                Item item = obj.GetComponent<ItemBoxAction>().item;
                if (item != null && formula.Stuffs.Exists(s => s  == item.ItemId / 1000))
                {
                    
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public int CheckItemCount()
    {
        AudioController.instance.PlayAudio(SE.select);
        
        List<int> playerItemCounts=new List<int>();
        List<Item> stuffItems=new List<Item>();

        foreach (var stuff in stuffs)
        {
            ItemBoxAction itemBoxAction = stuff.GetComponent<ItemBoxAction>();
            if (itemBoxAction.isFull)
            {
                if (stuffItems.Exists(i => i.ItemId / 1000 == itemBoxAction.item.ItemId / 1000))
                {
                    stuffItems.Find(i => i.ItemId / 1000 == itemBoxAction.item.ItemId / 1000).count++;
                }
                else
                {
                    Item _item=new Item(itemBoxAction.item){count = 1};
                    stuffItems.Add(_item);
                }
            }
        }
        foreach (var stuffItem in stuffItems)
        {
            var px = GameComponentData.gameData.gameManager.gamePlayer.package.items.FindAll(
                i => i.ItemId / 1000 == stuffItem.ItemId / 1000);
            int pxCount = 0;
            foreach (var item in px)
            {
                pxCount += item.count;
            }
            var bx = GameComponentData.gameData.gameManager.gamePlayer.box.items.FindAll(
                i => i.ItemId / 1000 == stuffItem.ItemId / 1000);
            foreach (var item in bx)
            {
                pxCount += item.count;
            }
            var ix = GameComponentData.gameData.gameManager.gamePlayer.box.items.FindAll(
                i => i.ItemId / 1000 == stuffItem.ItemId / 1000);
            foreach (var item in ix)
            {
                pxCount += item.count;
            }
            pxCount /= stuffItem.count;
            playerItemCounts.Add(pxCount);
        }


       

        playerItemCounts.Sort();
        if (playerItemCounts.Count > 0)
        {
            return playerItemCounts[0];
        }
        else
        {
            return -1;
        }
        
    }

    public void DisplayFormula(Dropdown dropdown)
    {
        AudioController.instance.PlayAudio(SE.click);
        int index = dropdown.value;
        dropdown.captionText.text = dropdown.options[index].text;
        foreach (var stuff in stuffs)
        {
            stuff.GetComponent<ItemBoxAction>().isFull = false;
            stuff.GetComponent<ItemBoxAction>().Hide();
        }
        if (index == 0)
        {
           
            for (int i = 0; i < formula.Stuffs.Count; i++)
            {
                stuffs[i].GetComponent<ItemBoxAction>().Hide();
            }
            product.GetComponent<ItemBoxAction>().Hide();
            formula = null;
            AutoSelectButton.gameObject.SetActive(false);
        }
        else
        {
            formula = formulas[index - 1];
            if (formula.isOpen)
            {
                for (int i = 0; i < formula.Stuffs.Count; i++)
                {
                    stuffs[i].GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Stuffs[i]);

                    stuffs[i].GetComponent<ItemBoxAction>().mask.enabled = true;
                }
                product.GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Product);
                product.GetComponent<ItemBoxAction>().mask.enabled = true;
                AutoSelectButton.gameObject.SetActive(true);
            }
            else
            {
                for (int i = 0; i < formula.Stuffs.Count; i++)
                {
                    stuffs[i].GetComponent<ItemBoxAction>().Hide();
                }
                product.GetComponent<ItemBoxAction>().Hide();
                AutoSelectButton.gameObject.SetActive(false);
            }
           
        }
       InitItemCostRP();
       ProduceButton.gameObject.SetActive(false);
    }
    public void DisplayFormula()
    {
        if (formula == null)
        {
            InitMarufacturingData(formulaType);
        }
        else
        {
            int index = FormulaDropdown.value;

            foreach (var stuff in stuffs)
            {
                stuff.GetComponent<ItemBoxAction>().isFull = false;
                stuff.GetComponent<ItemBoxAction>().Hide();
            }
            if (index == 0)
            {

                for (int i = 0; i < formula.Stuffs.Count; i++)
                {
                    stuffs[i].GetComponent<ItemBoxAction>().Hide();
                }
                product.GetComponent<ItemBoxAction>().Hide();
                formula = null;
                AutoSelectButton.gameObject.SetActive(false);
            }
            else
            {
                formula = formulas[index - 1];
                if (formula.isOpen)
                {
                    for (int i = 0; i < formula.Stuffs.Count; i++)
                    {
                        stuffs[i].GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Stuffs[i]);

                        stuffs[i].GetComponent<ItemBoxAction>().mask.enabled = true;
                    }
                    product.GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Product);
                    product.GetComponent<ItemBoxAction>().mask.enabled = true;
                    AutoSelectButton.gameObject.SetActive(true);
                }
                else
                {
                    for (int i = 0; i < formula.Stuffs.Count; i++)
                    {
                        stuffs[i].GetComponent<ItemBoxAction>().Hide();
                    }
                    product.GetComponent<ItemBoxAction>().Hide();
                    AutoSelectButton.gameObject.SetActive(false);
                }

            }
            InitItemCostRP();
            ProduceButton.gameObject.SetActive(false);
        }
        
    }
    void InitItemCostRP()
    {
        if (formula != null && formula.isOpen)
        {
            RpCostValue = formula.PowerCost * produceCount;
            RPcostValueText.text = RpCostValue.ToString();
        }
        else
        {
            RpCostValue = 0;
            foreach (var stuff in stuffs)
            {
                if (stuff.GetComponent<ItemBoxAction>().isFull)
                {
                    RpCostValue += 5;
                }
                
            }
            RpCostValue *= produceCount;
            RPcostValueText.text = RpCostValue.ToString();
        }
        
    }
    public void DisplaySelectItemInformation(ItemBoxAction itemBoxAction)
    {
        Item _item = itemBoxAction.item;
        if (_item==null||_item.ItemId == 0)
        {
            ItemNameText.text = "";
            ItemPriceText.text = "";
            ItemTypeText.text = "";
            ItemNoticeText.text = "";
            ItemNoticeText1.text = "";
        }
        else
        {
            SelectItem = itemBoxAction;
            ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(_item.ItemId);
            ItemNameText.text = itemData.Name;
            ItemPriceText.text = itemData.SellPrice + "G";
            ItemTypeText.text = LanguageManage.SwitchStr(itemData.Type.ToString());
            ItemNoticeText.text = itemData.Text2;
            ItemNoticeText1.text = itemData.Text1;
            if (itemBoxAction.mask.enabled)
            {
                GetOutButton.gameObject.SetActive(false);
                GetInButton.gameObject.SetActive(false);
                if (itemBoxAction.isBox)
                {
                    GetInButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (itemBoxAction.isBox)
                {
                    int _count = _item.count;
                    List<ItemBoxAction> itemBoxActions =
                        stuffs.FindAll(s => s.item != null);
                    if (itemBoxActions.Count>0)
                    {
                        var x = itemBoxActions.FindAll(s => s.item.ItemId / 1000 == _item.ItemId / 1000);
                        _count = _item.count / (x.Count + 1);
                    }
                   
                   
                    if (_count >= 1&&stuffs.Exists(s=>!s.isFull))
                    {
                        GetOutButton.gameObject.SetActive(false);
                        GetInButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        GetOutButton.gameObject.SetActive(false);
                        GetInButton.gameObject.SetActive(false);
                    }

                    
                }
                else
                {
                    GetOutButton.gameObject.SetActive(true);
                    GetInButton.gameObject.SetActive(false);
                }
            }
        }
       
        
    }

    public void HideItemInformation()
    {
        ItemNameText.text = "";
        ItemPriceText.text = "";
        ItemTypeText.text = "";
        ItemNoticeText.text = "";
        ItemNoticeText1.text = "";
        GetInButton.gameObject.SetActive(false);
        GetOutButton.gameObject.SetActive(false);
    }
    public void GetInButtonAction()
    {


        ItemData itemData =
            GameComponentData.gameData.itemsManager.GetItemDataFromId(SelectItem.item.ItemId);
        if ((formulaType == FormulaType.酒水 || formulaType == FormulaType.热食 || formulaType == FormulaType.冷食) && (
            !itemData.IsFresh))
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr("非生鲜物品不能添加！"));
        }
        else
        {
            ItemBoxAction stuff = null;
            if (formula != null)
            {
                if (formula.Stuffs.Exists(s => s == SelectItem.item.ItemId / 1000))
                {
                    List<int> items = formula.Stuffs.FindAll(s => s == SelectItem.item.ItemId / 1000);
                    foreach (var item in items)
                    {
                        int index = formula.Stuffs.FindIndex(s => s == item);
                        if (!stuffs[index].isFull)
                        {
                            stuff = stuffs[index];
                            stuff.SetAbleColor();

                            stuff.isFull = true;
                            break;
                        }
                    }
                    if (stuff == null)
                    {
                        stuff = stuffs.Find(s => !s.GetComponent<ItemBoxAction>().isFull);
                        if (stuff != null)
                        {
                            stuff.GetComponent<ItemBoxAction>().SetUnenableColor();

                            stuff.GetComponent<ItemBoxAction>().isFull = true;
                        }

                    }
                }
                else
                {
                    stuff = stuffs.Find(s => !s.GetComponent<ItemBoxAction>().isFull);
                    if (stuff != null)
                    {
                        stuff.GetComponent<ItemBoxAction>().SetUnenableColor();

                        stuff.GetComponent<ItemBoxAction>().isFull = true;
                    }
                }
            }
            else
            {
                stuff = stuffs.Find(s => !s.GetComponent<ItemBoxAction>().isFull);
                if (stuff != null)
                {
                    stuff.GetComponent<ItemBoxAction>().SetAbleColor();

                    stuff.GetComponent<ItemBoxAction>().isFull = true;
                }
            }
            if (stuff != null)
            {
                stuff.GetComponent<ItemBoxAction>().InitItemData(SelectItem.item);
                stuff.GetComponent<ItemBoxAction>().count.enabled = false;
                stuff.GetComponent<ItemBoxAction>().package = package;
                GetInButton.gameObject.SetActive(false);
                DisplayBoxItem(package);
                ProduceButton.gameObject.SetActive(true);
            }

            SetProductDisplay();
        }

        
    }

    public void AutoSelect()
    {
       
        foreach (var formulaStuff in formula.Stuffs)
        {
            GamePlayer player= GameComponentData.gameData.gameManager.gamePlayer;
            if (player.package.IsHaveItem(formulaStuff) || player.box.IsHaveItem(formulaStuff) ||
                player.icebox.IsHaveItem(formulaStuff))
            {
                int index = formula.Stuffs.FindIndex(f=>f==formulaStuff);
                stuffs[index].GetComponent<ItemBoxAction>().SetAbleColor();
            }
            else
            {
                ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(formulaStuff);
                GameComponentData.gameData.informationManager.AddInformation(LanguageManage.SwitchStr("*缺少素材:")+itemData.Name);
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),TitleText.text+LanguageManage.SwitchStr("*缺少素材:") + itemData.Name+" ...");
            }
        }
        SetProductDisplay();
    }
    void SetProductDisplay()
    {
        int x = CheckItemCount();
        if (x >= 0)
        {
            if (x <= 1)
            {
                produceCount = 1;
                UpButton.gameObject.SetActive(false);
                DownButton.gameObject.SetActive(false);
            }
            else if (x <= produceCount)
            {
                produceCount = x;
                UpButton.gameObject.SetActive(false);
                DownButton.gameObject.SetActive(true);
            }
            else
            {
                UpButton.gameObject.SetActive(true);
                DownButton.gameObject.SetActive(true);
               
            }
        }
        else
        {
            UpButton.gameObject.SetActive(true);
            DownButton.gameObject.SetActive(true);
        }
        if (produceCount == 1)
        {
            DownButton.gameObject.SetActive(false);
        }
        ProductCountText.text = produceCount.ToString();


       
        var fullStuffs = stuffs.FindAll(s => s.GetComponent<ItemBoxAction>().isFull);
        if (fullStuffs.Count == 0)
        {
            product.GetComponent<ItemBoxAction>().Hide();
            ProduceButton.gameObject.SetActive(false);
        }
        else
        {
            ProduceButton.gameObject.SetActive(true);
            if (formula == null || !formula.isOpen)
            {
                product.GetComponent<ItemBoxAction>().icon.sprite = DefaultSprite;
                product.GetComponent<ItemBoxAction>().icon.enabled = true;
            }
            else
            {
                isMatch = CheckFormula();

                if (isMatch)
                {
                    product.GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Product);
                    product.GetComponent<ItemBoxAction>().SetAbleColor();
                }
                else
                {

                    if (fullStuffs.Exists(f => !f.GetComponent<ItemBoxAction>().isMatch))
                    {
                        product.GetComponent<ItemBoxAction>().icon.sprite = DefaultSprite;
                        product.GetComponent<ItemBoxAction>().icon.enabled = true;
                    }
                    else
                    {
                        product.GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Product);
                    }

                }
            }
        }
        InitItemCostRP();
    }
    public void AddCount()
    {
        AudioController.instance.PlayAudio(SE.click);
        produceCount++;
        DownButton.gameObject.SetActive(true);
        int x = CheckItemCount();
        if (produceCount >= x)
        {
            produceCount = x;
            UpButton.gameObject.SetActive(false);
            
        }
        if (produceCount == 1)
        {
            DownButton.gameObject.SetActive(false);
        }
        if (produceCount == -1)
        {
            produceCount = 0;
            DownButton.gameObject.SetActive(false);
        }
        ProductCountText.text = produceCount.ToString();
        InitItemCostRP();
    }
    public void ReduceCount()
    {
        AudioController.instance.PlayAudio(SE.click);
        produceCount--;
        UpButton.gameObject.SetActive(true);
        if (produceCount <= 1)
        {
            produceCount = 1;
            DownButton.gameObject.SetActive(false);
           
        }
       
        ProductCountText.text = produceCount.ToString();
        InitItemCostRP();
    }
    public void Return()
    {
        AudioController.instance.PlayAudio(SE.Return);
        gameObject.SetActive(false);
    }
    public void GetOutButtonAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        int index = stuffs.FindIndex(s => s.GetComponent<ItemBoxAction>() == SelectItem);
        SelectItem.isFull = false;
        SelectItem.gameObject.GetComponentInChildren<Toggle>().isOn = false;
        if (formula != null)
        {
            if (index > formula.Stuffs.Count - 1)
            {
                SelectItem.ZeroData();
            }
            else
            {
                SelectItem.DisPlayFormulaItem(formula.Stuffs[index]);
                product.GetComponent<ItemBoxAction>().DisPlayFormulaItem(formula.Product);
            }
            
        }
        else
        {
            SelectItem.ZeroData();
        }
        if (stuffs.Exists(s => s.GetComponent<ItemBoxAction>().isFull))
        {
            
        }
        else
        {
            ProduceButton.gameObject.SetActive(false);
        }
        HideItemInformation();

        SetProductDisplay();
    }

    public void ClickProduceButtonAction()
    {
        
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        if (RpCostValue >= gamePlayer.property.Power)
        {
            AudioController.instance.PlayAudio(SE.Return);
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("RP消耗过大"), LanguageManage.SwitchStr("需消耗RP:")
                +RpCostValue+LanguageManage.SwitchStr(",超过拥有RP;")+gamePlayer.property.Power
                +LanguageManage.SwitchStr("无法制作！"));
        }
        else
        {
            string noticeStr = "";
            AudioController.instance.PlayAudio(SE.Return);
           
            if (isMatch)
            {
                noticeStr += LanguageManage.SwitchStr("是否确定按配方开始制作？");
            }
            else
            {
                noticeStr += LanguageManage.SwitchStr("无法确定产出物，是否开始制作？");
            }

            GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("制造"), noticeStr, CareType.Prodece);
        }

       
    }

    public void ProduceItem()
    {
        if (GameComponentData.gameData.gameManager.CostRp(RpCostValue))
        {
            PackageItemCounts = new List<PackageItem>();
            foreach (var stuff in stuffs)
            {
                PackageItem packageItem=new PackageItem();
                
                Item sitem = stuff.GetComponent<ItemBoxAction>().item;
                if (sitem != null)
                {
                    sitem.count = produceCount;

                    packageItem.itemId = sitem.ItemId;
                    packageItem.packageCount = new List<int>();

                    int packageCount = GameComponentData.gameData.gameManager.gamePlayer.package.IsHaveItem(sitem);
                    packageItem.packageCount.Add(packageCount);
                    sitem.count -= packageCount;
                    if (sitem.count > 0)
                    {
                        int boxCount = GameComponentData.gameData.gameManager.gamePlayer.box.IsHaveItem(sitem);
                        packageItem.packageCount.Add(boxCount);
                        sitem.count -= boxCount;
                    }
                    else
                    {
                        packageItem.packageCount.Add(0);
                    }
                    if (sitem.count > 0)
                    {
                        int iceCount = GameComponentData.gameData.gameManager.gamePlayer.icebox.IsHaveItem(sitem);
                        packageItem.packageCount.Add(iceCount);
                        sitem.count -= iceCount;
                    }
                    else
                    {
                        packageItem.packageCount.Add(0);
                    }

                    PackageItemCounts.Add(packageItem);
                }
               

            }




            if (isMatch)
            {
                ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(formula.Product);
                produceItem = new Item(itemData, produceCount);
            }
            else
            {
                List<int> itemIds = new List<int>();
                foreach (var stuff in stuffs)
                {
                    ItemBoxAction stuffBoxAction = stuff.GetComponent<ItemBoxAction>();
                    if (stuffBoxAction.isFull)
                    {
                        itemIds.Add(stuffBoxAction.item.ItemId);
                    }
                    stuff.GetComponent<ItemBoxAction>().isFull=false;


                }
                Formula _formula = null;
                foreach (var formula1 in formulas)
                {
                    bool isMatchF = true;
                    List<int> _itemIds = new List<int>();
                    _itemIds.AddRange(itemIds);
                    foreach (var formula1Stuff in formula1.Stuffs)
                    {
                       
                        if (_itemIds.Exists(i => i / 1000 == formula1Stuff))
                        {
                            int itemid = _itemIds.Find(i => i / 1000 == formula1Stuff);
                            _itemIds.Remove(itemid);
                        }
                        else
                        {
                            isMatchF = false;
                            break;
                        }
                    }
                    if (isMatchF)
                    {
                        _formula = formula1;
                        break;
                    }
                }
                if (_formula != null)
                {
                    if (_formula.formulaType == FormulaType.装备)
                    {
                        GameComponentData.gameData.charactorTitleAction.AddManufatureExp(produceCount);
                        
                    }
                    else if(_formula.formulaType == FormulaType.热食 || _formula.formulaType == FormulaType.冷食)
                    {
                        GameComponentData.gameData.charactorTitleAction.AddCookExp(produceCount);

                    }


                    _formula.isOpen = true;
                    ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(_formula.Product);
                    produceItem = new Item(itemData, produceCount);
                    if (itemData.Type == ItemType.武器)
                    {
                        GameComponentData.gameData.charactorTitleAction.AddManufatureCount(produceCount,1);
                    }
                    if (itemData.Type == ItemType.防具)
                    {
                        GameComponentData.gameData.charactorTitleAction.AddManufatureCount(produceCount, 2);
                    }
                }
                else
                {
                    int defaultId = 0;
                    switch (formulaType)
                    {
                        case FormulaType.装备:
                            defaultId = 1170;
                            break;
                        case FormulaType.热食:
                            defaultId = 1168;
                            break;
                        case FormulaType.冷食:
                            defaultId = 1168;
                            break;
                        case FormulaType.酒水:
                            defaultId = 1169;
                            break;
                    }

                    ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(defaultId);
                    produceItem = new Item(itemData, produceCount);
                }
            }
            CreatProduct();


            DisplayFormula();
        }
        
        
    }

    public void CreatProduct()
    {
        GamePlayer player = GameComponentData.gameData.gameManager.gamePlayer;
        foreach (var packageItemCount in PackageItemCounts)
        {
            
          player.package.GetItemOutPackage(packageItemCount.itemId,packageItemCount.packageCount[0]);
            player.box.GetItemOutPackage(packageItemCount.itemId, packageItemCount.packageCount[1]);
            player.icebox.GetItemOutPackage(packageItemCount.itemId, packageItemCount.packageCount[2]);
        }
       int groundItemCount=player.package.SetItemInPackage(produceItem);
        if (groundItemCount > 0)
        {
            Item _item = new Item(produceItem) {count = groundItemCount};
            GameComponentData.gameData.gameManager.GreatGroundItem(_item);

            ItemData produceItemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(produceItem.ItemId);
            GameComponentData.gameData.informationManager.AddInformation(LanguageManage.SwitchStr("*获得") + produceItem.count + LanguageManage.SwitchStr("个:") + produceItemData.Name+LanguageManage.SwitchStr("，其中")+groundItemCount+LanguageManage.SwitchStr("个落在地上"));
            GameComponentData.gameData.informationManager.AddInformation(LanguageManage.SwitchStr("*在地上的道具随时会被地底哥布林偷走，请及时回收。"));

            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("制作完成"), LanguageManage.SwitchStr("获得") + produceItem.count + LanguageManage.SwitchStr("个:") +
                produceItemData.Name+LanguageManage.SwitchStr(",因为背包已满，其中")+groundItemCount+ LanguageManage.SwitchStr("个落在地上，在地上的道具随时会被地底哥布林偷走，请及时回收。"));
        }
        else
        {
           
            ItemData produceItemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(produceItem.ItemId);
            GameComponentData.gameData.informationManager.AddInformation(LanguageManage.SwitchStr("*获得") + produceItem.count + LanguageManage.SwitchStr("个 ") + produceItemData.Name);
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("制作完成"), LanguageManage.SwitchStr("获得") + produceItem.count + LanguageManage.SwitchStr("个 ") + produceItemData.Name);
        }
        if (player.property.Power > RpCostValue)
        {
            player.property.Power -= RpCostValue;
        }
        else
        {
            int Rpvalue = RpCostValue- player.property.Power;
            player.property.Power = 0;
            player.property.HP -=Rpvalue;
        }
        GameComponentData.gameData.gameManager.UpDataPlayer();
        DisplayBoxItem(package);
        HideStuffAfterCreat();
        InitItemCostRP();
    }
	// Use this for initialization
	void Start ()
	{
	    ReturnButtonText.text = LanguageManage.SwitchStr(ReturnButtonText.text);
	    SucaiText.text = LanguageManage.SwitchStr(SucaiText.text);
	    PeifangText.text = LanguageManage.SwitchStr(PeifangText.text);
	    ChangchuText.text = LanguageManage.SwitchStr(ChangchuText.text);
	    ZidongText.text = LanguageManage.SwitchStr(ZidongText.text);
	    ZhizhuoText.text = LanguageManage.SwitchStr(ZhizhuoText.text);
	    XiaohaoText.text = LanguageManage.SwitchStr(XiaohaoText.text);
	    TianjiabuttonText.text = LanguageManage.SwitchStr(TianjiabuttonText.text);
	    YichuButtonText.text = LanguageManage.SwitchStr(YichuButtonText.text);
	    BaibaoText.text = LanguageManage.SwitchStr(BaibaoText.text);
	    ChuwuxiangText.text = LanguageManage.SwitchStr(ChuwuxiangText.text);
	    BingxiangText.text = LanguageManage.SwitchStr(BingxiangText.text);
        LanguageManage.TextFanyi(BaibaoText0);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
