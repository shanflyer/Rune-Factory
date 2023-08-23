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
    private List<Item> packageItem;
    private string PriceTitle,CountTitle;
	// Use this for initialization
	void Start ()
	{
	    XiajiaText.text = LanguageManage.SwitchStr(XiajiaText.text);
	    GenghuanText.text = LanguageManage.SwitchStr(GenghuanText.text); 
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
            item = default(Item);
            gameObject.SetActive(false);
        }
    }
    public async void InitDeskItemData(Item _item,DeskAction _deskAction)
    {
        PriceTitle = LanguageManage.SwitchStr("单价:");
        CountTitle = LanguageManage.SwitchStr("上架数量:");
        item = _item;
        ItemData itemData =await GameDataManager.instance.GetAsyncData<ItemData>(_item.dataId.ToString());
        ItemImage.sprite = itemData.iconSprite;
        ItemName.text = itemData.name;
        ItemPrice.text = PriceTitle+itemData.SellPrice+"G";
        CountTitleText.text = CountTitle;
        ItemCountText.text =_item.count.ToString();
        deskAction = _deskAction; 
         
        if (PackageManager.instance.IsHaveItem(0, item.dataId))
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
        RemovePackageItem removePackageItem = new RemovePackageItem
        {
            itemCount = 1,
            itemDataId = item.dataId,
            packageId = 0
        };
        GameActionManager.instance.QueueAction(removePackageItem,true); 
        if (PackageManager.instance.IsHaveItem(0, item.dataId))
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
        int count = PackageManager.instance.GetPackageItemCount(0, item.dataId);
         
        item.count += count;
        ItemCountText.text = item.count.ToString();
        deskAction.ItemCountText.text= item.count.ToString();

        RemovePackageItem removePackageItem = new RemovePackageItem
        {
            itemDataId = item.dataId,
            itemCount = count,
            packageId = 0
        };
        GameActionManager.instance.QueueAction(removePackageItem, true); 
        AddButton.interactable = false;
        MaxButton.interactable = false;
     
    }

    public async void ReduceItem()
    {
        AudioController.instance.PlayAudio(SE.click);
        item.count--;

        Item _Item = ItemManager.instance.CreatItem(item.dataId,1);
        await  PackageManager.instance.SetItemInPackage(_Item, 0); 
        ItemCountText.text = item.count.ToString();
        deskAction.ItemCountText.text = item.count.ToString();
        if (item.count == 0)
        {
            item = default(Item);
            deskAction.InitDeskData(item, int.MinValue);
            gameObject.SetActive(false);
        }
        else
        {
            AddButton.interactable = true;
            MaxButton.interactable = true;
  
        }    
    }

    public async void GetItmeDown()
    {
        AudioController.instance.PlayAudio(SE.Return);
        await PackageManager.instance.SetItemInPackage(item, 0);
        item = default(Item);
        deskAction.InitDeskData(item, int.MinValue);
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
