using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class SellItem : GamePanel<Item>
{
    [SerializeField]
    private SpriteRenderer icon;

    [SerializeField]
    private TextMeshPro count;

    private Vector3 defaultOffset;
    public void SetDefaultOffset(Vector3 offset)
    {
        defaultOffset = offset;
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        icon = GetComponent<SpriteRenderer>();
        count = FindChildGameObject<TextMeshPro>("Count");
    }

    public override void Close()
    {
        //base.Close();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        icon.enabled = item.count > 0;
        count.enabled = item.count > 0;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        item = default(Item);
    }

    protected override void Awake()
    {
        //base.Awake();
        icon.enabled = false;
        count.enabled = false;
    }

    private Item item;

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

    public void RefreshDisplay()
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
            count.enabled = false;
        }
    }

    public override void InitReferenceData(Item v)
    {
        base.InitReferenceData(v);
        // 售卖展示入口保持同步，物品显示数据加载异常统一进入日志。
        AsyncTaskRunner.Run(InitReferenceDataAsync(v), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(Item v)
    {
        item = v;
        ShopItemDisplayData shopItemDisplayData = await GameDataManager.instance.GetAsyncData<ShopItemDisplayData>(item.dataId);
        if (shopItemDisplayData == null)
        {
            transform.localPosition = defaultOffset;
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            icon.size = new Vector2(0.32f, 0.32f);
            if (itemData != null)
            {
                icon.sprite = itemData.icon;
            }
        }
        else
        {
            Vector3 offsetPos = shopItemDisplayData.offset;
            transform.localPosition = offsetPos;
            if (shopItemDisplayData.itemCounts == null || shopItemDisplayData.itemCounts.Count == 0)
            {
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                if (itemData != null)
                {
                    icon.sprite = itemData.icon;
                }
            }
            else
            {
                icon.sprite = shopItemDisplayData.GetItemSprite(item.count);
            }
            float size = 0.32f * shopItemDisplayData.scale;
            icon.size = new Vector2(size, size);
        }
        count.text = item.count.ToString();

        RefreshDisplay();
    }
}
