using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public partial class PackageManager : Singleton<PackageManager>
{
    public List<int> playerPackages = new List<int>();

    public void AddPlayerPackage(int id)
    {
        if (!playerPackages.Contains(id))
        {
            playerPackages.Add(id);
        }
    }

    private Dictionary<int, ObjPackageAnimationData> objPackageAnimationDatas = new Dictionary<int, ObjPackageAnimationData>();
    private Dictionary<int, GamePackage> gamePackages = new Dictionary<int, GamePackage>();
    private Dictionary<Vector2Int, int> runtimePackageRuntimes = new Dictionary<Vector2Int, int>();




}

public struct PackageList : IReferenceData
{
    public List<PackageData> packageDatas;
    public ItemMatchData itemMatchData;
    public bool canSetShortcut;
}

public enum ItemMatchType
{
    Null = 0,
    ItemType = 1,
    IsFresh = 2,
    Manufacture = 3
}

public struct ItemMatchData
{
    public ItemMatchType itemMatchType;
    public HashSet<int> matchValues;

    public async Task<bool> MatchAction(Item item)
    {
        if (matchValues == null || matchValues.Count == 0 || item.dataId == 0)
        {
            return true;
        }
        switch (itemMatchType)
        {
            case ItemMatchType.ItemType:
                return matchValues.Contains((int)item.itemType);

            case ItemMatchType.IsFresh:
                return matchValues.Contains(item.isFresh ? 1 : 0);
            case ItemMatchType.Manufacture:
                var match = false;
                var itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                foreach (var matchValue in matchValues)
                    if (itemData.manufacture.Contains(matchValue))
                    {
                        match = true;
                        break;
                    }

                return match;
        }
        return true;
    }

    public bool MatchAction(ItemData itemData)
    {
        switch (itemMatchType)
        {
            case ItemMatchType.ItemType:
                return matchValues.Contains((int)itemData.type);

            case ItemMatchType.IsFresh:
                return matchValues.Contains(itemData.isFresh ? 1 : 0);
        }
        return true;
    }
}

public struct PackageData : IReferenceData
{
    public string name;
    public int instanceId;
    public int dataId;
    public int caseCount;
    public int level;

    public PackageType packageType;
    public List<Item> items;
}
