using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxAction : MonoBehaviour
{
    public Toggle toggle;
    public Image icon;
    public Image mask;
    public Image backGround;
    public Text count;
    [HideInInspector]
    public Item item;
    [HideInInspector]
    public int package;

    public bool isEnough;
    [HideInInspector] public bool isFull,isMatch;
    [HideInInspector] public bool isBox, isWareDisplay;
    [HideInInspector] public DeskAction deskAction;
	// Use this for initialization
	void Start () {
		
	}

    public void ClickButton()
    {
        toggle.isOn = !toggle.isOn;
    }
    public void ZeroDisplay()
    {
        mask.enabled = true;
        count.enabled = false;
        GetComponentInChildren<Toggle>().enabled = false;
        mask.enabled = true;
    }
    public void ZeroData()
    {
        Hide();
        GetComponentInChildren<Toggle>().enabled = false;
        mask.enabled = true;
    }

    public void SetUnenableColor()
    {
        isFull = false;
        mask.enabled = false;
        isMatch = false;
        backGround.color=new Color(1,0.506f,0.506f);
        
    }
    public void DisPlayFormulaItem(int _itemid)
    {
        InitItemData(_itemid);
        count.enabled = false;
        GetComponentInChildren<Toggle>().enabled = true;
        mask.enabled = true;
        icon.color = new Color(0.624f, 0.624f, 0.624f, 0.5f);
    }
    public void DisPlayFormulaItem()
    {
        if (item.instanceId != 0)
        {
            InitItemData(item.dataId);
            count.enabled = false;
            GetComponentInChildren<Toggle>().enabled = true;
            mask.enabled = true;
            icon.color = new Color(0.624f, 0.624f, 0.624f, 0.5f);
        }
       
    }
    public void SetAbleColor()
    {
        isFull = true;
        mask.enabled = false;
        isMatch = true;
        backGround.color = new Color(0.682f, 1.0f, 0.914f);
        icon.color=Color.white;
       
    }
    public async void InitItemData(Item _item,DeskAction _deskAction)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(_item.dataId.ToString());
        mask.gameObject.SetActive(false);
        item = _item;
        icon.sprite =itemData.iconSprite;
        icon.color=Color.white;
        icon.enabled = true;
        //icon.SetNativeSize();
        count.text = item.count.ToString();
        count.enabled = true;
        deskAction = _deskAction;
    }
    public async void InitItemData(Item _item)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(_item.dataId.ToString());
        item = _item;
       
        
        GetComponentInChildren<Toggle>().enabled = true;
        icon.sprite = itemData.iconSprite;
        //icon.SetNativeSize();
        count.text = item.count.ToString();
        icon.enabled = true;
        count.enabled = true;
        icon.color = new Color(1, 1, 1, 1);
    }
    public async void InitItemData(int  _itemid)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(_itemid.ToString()); ;
        GetComponentInChildren<Toggle>().enabled = true;
        item = ItemManager.instance.CreatItem(_itemid, 1);
        icon.sprite = itemData.iconSprite;
        //icon.SetNativeSize();
        count.enabled=false;
        icon.enabled = true;
        count.enabled = true;
        icon.color = new Color(1, 1, 1, 1);
    }
    public void Hide()
    {
        mask.enabled = true;
        icon.enabled = false;
        count.enabled = false;
    }

    public void ClickItem1(Toggle toggle)
    {
        if (toggle.isOn)
        {
            if (isWareDisplay)
            {
                GameComponentData.gameData.warehouseAction.ClickItem(item);
            }
            else
            {
                GameComponentData.gameData.manufacturingAction.DisplaySelectItemInformation(this);
            }
           
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
