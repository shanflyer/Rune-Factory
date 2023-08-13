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
    public int GoldPackage;
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
      
        AudioController.instance.PlayAudio(SE.click);
        if (item.instanceId==0)
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
    public async void SellItem()
    {
        if (item.instanceId!=0)
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncObjectData<ItemData>(item.dataId.ToString());
           // GameComponentData.gameData.gameManager.ChangePlayerMoney(itemData.SellPrice);
            AudioController.instance.PlayAudio(SE.Shop);
            int sellPrice =
                (int) (itemData.SellPrice * GameComponentData.gameData.shopGoldDeskAction.saleValue / 100.0f);
            string infomation = "*1 " + itemData.name+LanguageManage.SwitchStr("出售");
            InformationController.instance.AddInformation(infomation);
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

           // InformationController.instance.AddInformation(infomation);
            item.count--;

            ItemCountText.text = item.count.ToString();
            DeskItemAction deskItemAction = DeskItemInformationObj.GetComponent<DeskItemAction>();
            if (deskItemAction.deskAction == this)
            {
                deskItemAction.ChangeItemCount(item.count);
            }
            if (item.count <= 0)
            {
                InitDeskData(default(Item), int.MinValue);
            }

        }
        
        
    }
    public async void SellItem(int count)
    {
        if (item.instanceId != 0)
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncObjectData<ItemData>(item.dataId.ToString());
            // GameComponentData.gameData.gameManager.ChangePlayerMoney(itemData.SellPrice);
            AudioController.instance.PlayAudio(SE.Shop);
            int sellPrice =
                (int)(itemData.SellPrice * GameComponentData.gameData.shopGoldDeskAction.saleValue / 100.0f) * count;
            string infomation = "*"+count+ itemData.name + LanguageManage.SwitchStr("出售");
            InformationController.instance.AddInformation(infomation);
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

            // InformationController.instance.AddInformation(infomation);
            item.count-=count;
         
            ItemCountText.text = item.count.ToString();
            DeskItemAction deskItemAction = DeskItemInformationObj.GetComponent<DeskItemAction>();
            if (deskItemAction.deskAction == this)
            {
                deskItemAction.ChangeItemCount(item.count);
            }
            if (item.count <= 0)
            {
                InitDeskData(default(Item), int.MinValue);
            }

           
        }


    }

    public void InitDeskData(Item _item,int _package)
    {
        GoldPackage = _package;

        item = _item;
        if (item.instanceId != 0)
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
        if (item.instanceId != 0)
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
    
}
