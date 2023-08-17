using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PastureItemPanelAction : MonoBehaviour
{
    public List<Text> Texts;
    public Text pastureName,ProduceCaseText,AddCostText;

    public GameObject itemBox, NullBox;

    public Transform BoxParent;
    private Pasture pasture;
    private int costValue;
    public void InitPastureItemPanelData(Pasture _pasture)
    {
        foreach (Transform box in BoxParent)
        {
            Destroy(box.gameObject);
        }
        pasture = _pasture;
        pastureName.text = _pasture.name;

        int caseCount = PackageManager.instance.GetPackageCaseCount(pasture.itemPackage);
        var items = PackageManager.instance.GetPackageItems(pasture.itemPackage);

        ProduceCaseText.text = "(" + items.Count + "/" + caseCount + ")";
        foreach (var itemPackageItem in items)
        {
            GameObject itemObj = Instantiate(itemBox);
            
            itemObj.transform.SetParent(BoxParent);
            itemObj.transform.localScale = Vector3.one;
            itemObj.GetComponent<ItemBoxAction>().InitItemData(itemPackageItem);
            Destroy(itemObj.GetComponentInChildren<Toggle>().gameObject);
        }
        int count = caseCount - items.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject itemObj = Instantiate(NullBox);
            
            itemObj.transform.SetParent(BoxParent);
            itemObj.transform.localScale = Vector3.one;
            NullBox.GetComponent<ItemBoxAction>().icon.enabled = false;
            NullBox.GetComponent<ItemBoxAction>().count.enabled = false;
        }
        costValue = GameComponentData.gameData.pastureAction.zeroAddCase + (caseCount - 2) *
                        GameComponentData.gameData.pastureAction.addcasePlus;
        AddCostText.text = costValue.ToString();
    }

    public void AddCost()
    {
        AudioController.instance.PlayAudio(SE.click);
        GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("增加格位"),costValue, LanguageManage.SwitchStr("为牧场:")+pasture.name+
            LanguageManage.SwitchStr(" 新增一个产出箱格位？"),CostType.增加牧场产出格子
            ,ShopMoneyType.金币);
    }
    public void AddCase()
    { 
        var items = PackageManager.instance.GetPackageItems(pasture.itemPackage);
        int caseCount= PackageManager.instance.AddPackageCaseCount(pasture.itemPackage, 1);
         
        GameObject itemObj = Instantiate(NullBox);
        itemObj.transform.localScale = Vector3.one;
        itemObj.transform.SetParent(BoxParent);
        costValue = GameComponentData.gameData.pastureAction.zeroAddCase + (caseCount- 2) *
                        GameComponentData.gameData.pastureAction.addcasePlus;
        AddCostText.text = costValue.ToString();
        ProduceCaseText.text = "(" + items.Count + "/" + caseCount + ")";
        InformationController.instance.AddInformation(pasture.name+LanguageManage.SwitchStr("增加一个产出格,消耗金币")+costValue);
       
    }

    public async void GetItemToPlayer()
    {
        AudioController.instance.PlayAudio(SE.click);
        List<Item> outItems=new List<Item>();
        var items = PackageManager.instance.GetPackageItems(pasture.itemPackage);
        foreach (var itemPackageItem in items)
        {   
            if (await PackageManager.instance.CheckPackageTryItemIn(0, itemPackageItem.dataId, itemPackageItem.count))
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr("背包已满！"));
                break;
            }
            else
            {
                /*
                Item outItem = new Item
                {
                    ItemId = itemPackageItem.ItemId,
                    groupNum = itemPackageItem.groupNum,
                    count = itemPackageItem.count - laveCount
                };*/
                outItems.Add(itemPackageItem);
            }
        }
        foreach (var outItem in outItems)
        {
            RemovePackageItem removePackageItem = new RemovePackageItem
            {
                itemDataId = outItem.dataId,
                itemCount = outItem.count,
                packageId = pasture.itemPackage
            };
            GameActionManager.instance.QueueAction(removePackageItem, true);
             
        }
        InitPastureItemPanelData(pasture);
    }
	// Use this for initialization
	void Start () {
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
