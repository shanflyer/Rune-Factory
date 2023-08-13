using NUnit.Framework.Interfaces;
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

    public async void InitSellSelectData(Item _item)
    {
        item = _item;
        ItemData itemData =await GameDataManager.instance.GetAsyncObjectData<ItemData>(item.dataId.ToString());

        itemImage.sprite = itemData.iconSprite;
        itemName.text = itemData.name;
        inputField.text = "1";
        sellCount = 1; 
        int totalCount = PackageManager.instance.GetPackageItemCount(0,itemData.id); 

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

        int totalCount = PackageManager.instance.GetPackageItemCount(0, item.dataId); 

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
        int totalCount = PackageManager.instance.GetPackageItemCount(0, item.dataId); 
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
        AudioController.instance.PlayAudio(SE.Click2);
        GameComponentData.gameData.warehouseAction.SellItem(sellCount);
     
    }
	// Update is called once per frame
	void Update () {
		
	}
}
