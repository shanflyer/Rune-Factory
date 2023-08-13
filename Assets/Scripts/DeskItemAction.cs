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
        ItemData itemData =await GameDataManager.instance.GetAsyncObjectData<ItemData>(_item.dataId.ToString());
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
