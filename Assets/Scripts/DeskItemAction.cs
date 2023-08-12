using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeskItemAction : MonoBehaviour
{
    public Text  XiajiaText, GenghuanText;

    public Image ItemImage;
    public Text ItemName;
    public Text ItemPrice;
    public Text CountTitleText;
    public Text ItemCountText;
    public Button ReButton, AddButton, MaxButton;

    public GameObject WarehouseObj;
    [HideInInspector]
    public Item item;
    [HideInInspector] public DeskAction deskAction;
    private Package playerPackage;
    private List<Item> packageItem;
    private string PriceTitle,CountTitle;
	// Use this for initialization
	void Start ()
	{
	    XiajiaText.text = LanguageManage.SwitchStr(XiajiaText.text);
	    GenghuanText.text = LanguageManage.SwitchStr(GenghuanText.text);
	    playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
    }

    public void ClickReturnButton()
    {
        AudioController.instance.PlayAudio(SE.Return);
        gameObject.SetActive(false);
    }
    public void ChangeItemCount(int count)
    {
       // AudioController.instance.PlayAudio(SE.select);
        ItemCountText.text = count.ToString();
        if (count <= 1)
        {
            ReButton.enabled = false;
            
        }
        if (count <= 0)
        {
            item = null;
            gameObject.SetActive(false);
        }
    }
    public void InitDeskItemData(Item _item,DeskAction _deskAction)
    {
        PriceTitle = LanguageManage.SwitchStr("单价:");
        CountTitle = LanguageManage.SwitchStr("上架数量:");
        item = _item;
        ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(_item.ItemId);
        ItemImage.sprite = GameComponentData.gameData.itemsManager.GetItemIcon(itemData.Icon);
        ItemName.text = itemData.Name;
        ItemPrice.text = PriceTitle+itemData.SellPrice+"G";
        CountTitleText.text = CountTitle;
        ItemCountText.text =_item.count.ToString();
        deskAction = _deskAction;
        playerPackage = GameComponentData.gameData.gameManager.gamePlayer.package;


        packageItem = playerPackage.items.FindAll(i => i.ItemId/1000 == item.ItemId/1000);
        if (packageItem.Count>0)
        {
            AddButton.interactable = true;
            MaxButton.interactable = true;
        }
        else
        {
            AddButton.interactable = false;
            MaxButton.interactable = false;
        }
        
    }

    public void AddItemCount()
    {
        AudioController.instance.PlayAudio(SE.click);
        playerPackage.GetItemOutPackage(item.ItemId,1);
        packageItem = playerPackage.items.FindAll(i => i.ItemId / 1000 == item.ItemId / 1000);
        if (packageItem.Count > 0)
        {
            AddButton.interactable = false;
            MaxButton.interactable = false;
        }
        item.count++;
        ItemCountText.text = item.count.ToString();
        deskAction.ItemCountText.text = item.count.ToString();
       
    }

    public void MaxAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        packageItem = playerPackage.items.FindAll(i => i.ItemId / 1000 == item.ItemId / 1000);
        int count = 0;
        foreach (var item1 in packageItem)
        {
            count += item1.count;
        }
        item.count += count;
        ItemCountText.text = item.count.ToString();
        deskAction.ItemCountText.text= item.count.ToString();
        playerPackage.GetItemOutPackage(item.ItemId,count);
        AddButton.interactable = false;
        MaxButton.interactable = false;
     
    }

    public void ReduceItem()
    {
        AudioController.instance.PlayAudio(SE.click);
        item.count--;

        Item _Item = new Item(item) {count = 1};
        playerPackage.SetItemInPackage(_Item);
        ItemCountText.text = item.count.ToString();
        deskAction.ItemCountText.text = item.count.ToString();
        if (item.count == 0)
        {
            item = null;
            deskAction.InitDeskData(null,null);
            gameObject.SetActive(false);
        }
        else
        {
            AddButton.interactable = true;
            MaxButton.interactable = true;
  
        }    
    }

    public void GetItmeDown()
    {
        AudioController.instance.PlayAudio(SE.Return);
        playerPackage.SetItemInPackage(item);
        item = null;
        deskAction.InitDeskData(null,null);
        gameObject.SetActive(false);

    }

    public void ChangeItem()
    {
        AudioController.instance.PlayAudio(SE.click);
        WarehouseObj.SetActive(true);
        List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
        wareDisplayTypes.Add(WareDisplayType.Good);
        WarehouseObj.GetComponentInChildren<WarehouseAction>().InitWareHouseData(PackageType.背包,wareDisplayTypes,DisplayType.Sell);
        gameObject.SetActive(false);

    }
	// Update is called once per frame
	void Update () {
		
	}
}
