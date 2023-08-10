using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemAction : MonoBehaviour
{
    public Image icon;
    public Image MoneyImage0, MoneyImage1;
    public Text price;
    private ShopPanelAction shopPanelAction;
    private ItemData itemData;
	// Use this for initialization
	void Start () {
		
	}

    public void ClickButton()
    {
        Toggle toggle = GetComponentInChildren<Toggle>();
        toggle.isOn = !toggle.isOn;
    }
    public void InitData(ItemData _itemData,ShopPanelAction _shopPanelAction)
    {
        if (_itemData.shopMoneyType == ShopMoneyType.金币)
        {
            MoneyImage0.enabled = true;
            MoneyImage1.enabled = false;
        }
        else
        {
            MoneyImage0.enabled = false;
            MoneyImage1.enabled = true;
        }
        shopPanelAction = _shopPanelAction;
        itemData = _itemData;
        icon.sprite = GameComponent.ItemSprites.Find(i => i.name == itemData.Icon);
        price.text = itemData.ShopPrice.ToString();
    }

    public void Click(Toggle toggle)
    {
        if (toggle)
        {
            shopPanelAction.SelectItem(itemData);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
