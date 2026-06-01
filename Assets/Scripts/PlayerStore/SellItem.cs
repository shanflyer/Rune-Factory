using TMPro;
using UnityEngine;

public class SellItem : GamePanel<Item>
{
    [SerializeField]
    private SpriteRenderer icon;

    [SerializeField]
    private TextMeshPro count;

    private Vector3 defaultOffset;
    private Item item;

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
        // The pooled store display is closed by its owner.
    }

    public override void OnEnable()
    {
        base.OnEnable();
        RefreshDisplay();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        item = default;
    }

    protected override void Awake()
    {
        if (icon != null)
        {
            icon.enabled = false;
        }
        if (count != null)
        {
            count.enabled = false;
        }
    }

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
        if (icon == null || count == null)
        {
            return;
        }

        count.text = item.count.ToString();
        bool hasItem = item.count > 0 && item.dataId > 0;
        icon.enabled = hasItem;
        count.enabled = hasItem;
    }

    public override void InitReferenceData(Item v)
    {
        base.InitReferenceData(v);
        AsyncTaskRunner.Run(InitReferenceDataAsync(v), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(Item v)
    {
        item = v;
        if (item.count <= 0 || item.dataId <= 0)
        {
            transform.localPosition = defaultOffset;
            RefreshDisplay();
            return;
        }

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
            transform.localPosition = shopItemDisplayData.offset;
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

        RefreshDisplay();
    }
}
