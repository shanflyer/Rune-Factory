using System.Collections;
using TMPro;
using UnityEngine;

public class SellItem : GamePanel<Item>
{
    [SerializeField]
    SpriteRenderer icon;
    [SerializeField]
    TextMeshPro count;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        icon=GetComponent<SpriteRenderer>();
        count = FindChildGameObject<TextMeshPro>("Count");

    }
    protected override void Awake()
    {
        //base.Awake();
        icon.enabled = false;
        count.enabled = false;
    }

    Item item;
    public void AddItemCount(int count)
    {
        item.count += count;
        RefreshDisplay();
    }
    public void SetItemCount(int count)
    {
        item.count = count;
        RefreshDisplay();
    }
    void RefreshDisplay()
    {
        count.text = item.count.ToString();
        if (item.count > 0)
        {
            icon.enabled = true;
            count.enabled = true;
        }
        else
        {
            icon.enabled = false;
            count.enabled =false;
        }
    }

    public override async void InitReferenceData(Item v)
    {
        base.InitReferenceData(v);
        item = v;
        ItemData itemData=await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
        if (itemData != null)
        {
            icon.sprite = itemData.icon;
            count.text = item.count.ToString(); 
        }

        RefreshDisplay();
    }
}