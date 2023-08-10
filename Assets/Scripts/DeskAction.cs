using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeskAction : MonoBehaviour
{
    [HideInInspector]
    public Item item;
    public SpriteRenderer ItemSpriteRenderer;
    public Text ItemCountText;
    public Package GoldPackage;
    public GameObject WarehouseObj;

    public GameObject DeskItemInformationObj;
    
	// Use this for initialization
	void Start ()
	{
        /*
	    item = null;
	    ItemSpriteRenderer.enabled = false;
        if(gameObject.name=="Desk")
	    ItemCountText.enabled = false;
        */
    }

    public void Click()
    {
      
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        if (item == null||item.ItemId==0)
        {
            WarehouseObj.SetActive(true);
            WarehouseObj.GetComponentInChildren<WarehouseAction>().deskAction = this;
            List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
            wareDisplayTypes.Add(WareDisplayType.Good);
            WarehouseObj.GetComponentInChildren<WarehouseAction>().InitWareHouseData(PackageType.背包, wareDisplayTypes,DisplayType.Sell);
        }
        else
        {
            DeskItemInformationObj.SetActive(true);
            DeskItemInformationObj.GetComponent<DeskItemAction>().InitDeskItemData(item,this);
        }
        
    }

    public void ClickAddNull()
    {
        GameComponentData.gameData.shopGoldDeskAction.ClickAddDesk();
    }
    public void SellItem()
    {
        if (item != null&&item.ItemId!=0)
        {
            ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(item.ItemId);
           // GameComponentData.gameData.gameManager.ChangePlayerMoney(itemData.SellPrice);
            AudioManager.PlaySE(PlayType.ONCE,"Shop");
            int sellPrice =
                (int) (itemData.SellPrice * GameComponentData.gameData.shopGoldDeskAction.saleValue / 100.0f);
            string infomation = "*1 " + itemData.Name+LanguageManage.SwitchStr("出售");
            GameComponentData.gameData.informationManager.AddInformation(infomation);
            if (GameComponentData.gameData.passDataManager.NowPassData.id == 1000)
            {
                GameComponentData.gameData.coinAction.CreatCoin(sellPrice, transform.position);
            }
            else
            {
                GameComponentData.gameData.coinAction.GroundMoney += sellPrice;
            }
            

            GameComponentData.gameData.charactorTitleAction.AddBusinessExp(sellPrice);
            if (itemData.Type == ItemType.食材 && itemData.typeValue != 0 && itemData.typeValue != 6 &&
                itemData.typeValue != 7 && itemData.typeValue != 8)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 3);
            }
            else if(itemData.Type == ItemType.食材 &&itemData.typeValue== 6)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 0);
            }
            else if (itemData.Type == ItemType.武器||itemData.Type==ItemType.防具)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 1);
            }
            else if (itemData.Type == ItemType.食物)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 2);
            }

           // GameComponentData.gameData.informationManager.AddInformation(infomation);
            item.count--;

            ItemCountText.text = item.count.ToString();
            DeskItemAction deskItemAction = DeskItemInformationObj.GetComponent<DeskItemAction>();
            if (deskItemAction.deskAction == this)
            {
                deskItemAction.ChangeItemCount(item.count);
            }
            if (item.count <= 0)
            {
                InitDeskData(null,null);
            }

        }
        
        
    }
    public void SellItem(int count)
    {
        if (item != null && item.ItemId != 0)
        {
            ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(item.ItemId);
            // GameComponentData.gameData.gameManager.ChangePlayerMoney(itemData.SellPrice);
            AudioManager.PlaySE(PlayType.ONCE, "Shop");
            int sellPrice =
                (int)(itemData.SellPrice * GameComponentData.gameData.shopGoldDeskAction.saleValue / 100.0f) * count;
            string infomation = "*"+count+ itemData.Name + LanguageManage.SwitchStr("出售");
            GameComponentData.gameData.informationManager.AddInformation(infomation);
            if (GameComponentData.gameData.passDataManager.NowPassData.id == 1000)
            {
                GameComponentData.gameData.coinAction.CreatCoin(sellPrice, transform.position);
            }
            else
            {
                GameComponentData.gameData.coinAction.GroundMoney += sellPrice;

                Debug.Log("sell:"+sellPrice+" Ground:"+ GameComponentData.gameData.coinAction.GroundMoney);
            }
            

            GameComponentData.gameData.charactorTitleAction.AddBusinessExp(sellPrice);
            if (itemData.Type == ItemType.食材 && itemData.typeValue != 0 && itemData.typeValue != 6 &&
                itemData.typeValue != 7 && itemData.typeValue != 8)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 3);
            }
            else if (itemData.Type == ItemType.食材 && itemData.typeValue == 6)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 0);
            }
            else if (itemData.Type == ItemType.武器 || itemData.Type == ItemType.防具)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 1);
            }
            else if (itemData.Type == ItemType.食物)
            {
                GameComponentData.gameData.charactorTitleAction.AddBusinessMoney(sellPrice, 2);
            }

            // GameComponentData.gameData.informationManager.AddInformation(infomation);
            item.count-=count;
         
            ItemCountText.text = item.count.ToString();
            DeskItemAction deskItemAction = DeskItemInformationObj.GetComponent<DeskItemAction>();
            if (deskItemAction.deskAction == this)
            {
                deskItemAction.ChangeItemCount(item.count);
            }
            if (item.count <= 0)
            {
                InitDeskData(null, null);
            }

           
        }


    }

    public void InitDeskData(Item _item,Package _package)
    {
        if (_package != null)
        {
            GoldPackage = _package;
        }
        
        item = _item;
        if (item != null)
        {
            ItemSpriteRenderer.enabled = true;
            ItemCountText.enabled = true;
            ItemSpriteRenderer.sprite = GameComponentData.gameData.itemsManager.GetItemIcon(item);
            ItemCountText.text = _item.count.ToString();
        }
        else
        {
            ItemSpriteRenderer.enabled = false;
            ItemCountText.enabled = false;
        }
        GameComponentData.gameData.shopGoldDeskAction.GoodDeskes.Find(d => d.deskAction == this).item = item;

    }
    public void InitDeskData(Item _item)
    {
     
        item = _item;
        if (item != null)
        {
            ItemSpriteRenderer.enabled = true;
            ItemCountText.enabled = true;
            ItemSpriteRenderer.sprite = GameComponentData.gameData.itemsManager.GetItemIcon(item);
            ItemCountText.text = _item.count.ToString();
        }
        else
        {
            ItemSpriteRenderer.enabled = false;
            ItemCountText.enabled = false;
        }

    }
    // Update is called once per frame
    void Update () {
		
	}
}
