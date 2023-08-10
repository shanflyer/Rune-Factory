using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SellSellectAction : MonoBehaviour
{
    public Button AddButton, ReduceButton, AddToMaxButton, ReduceMinButton;

    public InputField inputField;
    public Image itemImage;
    public Text itemName;
    private Item item;
    private int sellCount;
	// Use this for initialization
	void Start () {
        

    }

    public void InitSellSelectData(Item _item)
    {
        item = _item;
        ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(item.ItemId);

        itemImage.sprite = GameComponentData.gameData.itemsManager.GetItemIcon(itemData.Icon);
        itemName.text = itemData.Name;
        inputField.text = "1";
        sellCount = 1;
        List<Item> items =
            GameComponentData.gameData.gameManager.gamePlayer.package.items
                .FindAll(i => i.ItemId / 1000 == itemData.Id);
        int totalCount = 0;
        foreach (var item1 in items)
        {
            totalCount += item1.count;
        }

        if (totalCount == 1)
        {
            AddButton.interactable = false;
            AddToMaxButton.interactable = false;
        }
        else
        {
            AddButton.interactable = true;
            AddToMaxButton.interactable = true;
        }
        ReduceButton.interactable = false;
        ReduceMinButton.interactable = false;
    }

    public void AddAction()
    {
        sellCount++;

        List<Item> items =
            GameComponentData.gameData.gameManager.gamePlayer.package.items
                .FindAll(i => i.ItemId / 1000 == item.ItemId/1000);
        int totalCount = 0;
        foreach (var item1 in items)
        {
            totalCount += item1.count;
        }

        if (sellCount >= totalCount)
        {
            AddButton.interactable = false;
            AddToMaxButton.interactable = false;
        }
        ReduceButton.interactable = true;
        ReduceMinButton.interactable = true;
        inputField.text = sellCount.ToString();
    }

    public void AddMaxAction()
    {
        List<Item> items =
            GameComponentData.gameData.gameManager.gamePlayer.package.items
                .FindAll(i => i.ItemId / 1000 == item.ItemId / 1000);
        int totalCount = 0;
        foreach (var item1 in items)
        {
            totalCount += item1.count;
        }

        sellCount = totalCount;
        AddButton.interactable = false;
        AddToMaxButton.interactable = false;
        ReduceButton.interactable = true;
        ReduceMinButton.interactable = true;
        inputField.text = sellCount.ToString();
    }

    public void ReduceAction()
    {
        sellCount--;
        if (sellCount <= 1)
        {
            ReduceButton.interactable = false;
            ReduceMinButton.interactable = false;
        }
        AddButton.interactable = true;
        AddToMaxButton.interactable = true;
        inputField.text = sellCount.ToString();
    }

    public void ReduceToMin()
    {
        sellCount = 1;
        ReduceButton.interactable = false;
        ReduceMinButton.interactable = false;
        AddButton.interactable = true;
        AddToMaxButton.interactable = true;
        inputField.text = sellCount.ToString();
    }

    public void InputEndAction()
    {
        int inputValue = int.Parse(inputField.text);
        if(inputValue<=1)
        {
            inputValue = 1;
        }
        if (inputValue >= item.count)
        {
            inputValue = item.count;
        }
        sellCount = inputValue;
        inputField.text = inputValue.ToString();
    }

    public void SellItemAction()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click2");
        GameComponentData.gameData.warehouseAction.SellItem(sellCount);
     
    }
	// Update is called once per frame
	void Update () {
		
	}
}
