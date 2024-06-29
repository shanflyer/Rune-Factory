using System.Collections.Generic;
using Unity.Mathematics;

public class HomeEquipManager : Singleton<HomeEquipManager>
{
    private Dictionary<int,HomeEquip> homeEquips = new Dictionary<int, HomeEquip>();

    private Dictionary<int, List<int>> characterHomeEquips = new Dictionary<int, List<int>>();
    private Dictionary<int, Dictionary<int, int>> characterHomeEquipCountData = new Dictionary<int, Dictionary<int, int>>();

    public override void Init()
    {
        base.Init();
        homeEquips.Clear();
        GameActionManager.instance.AddListener<CreatHomeEquip>(CreatHomeEquip);
        GameActionManager.instance.AddListener<RemoveHomeEquip>(RemoveHomeEquip);
        GameActionManager.instance.AddListener<ChangeHomeEquipCharacter>(ChangeHomeEquipCharacter);
        GameActionManager.instance.AddListener<TryLayInHomeEquip>(TryLayInHomeEquip);
        GameActionManager.instance.AddListener<SetHomeEquipCoordinate>(SetHomeEquipCoordinate);
        GameActionManager.instance.AddListener<RefreshHomeEquip>(RefreshHomeEquip);
        GameActionManager.instance.AddListener<DisplayHomeEquipPanel>((DisplayHomeEquipPanel DisplayHomeEquipPanel) =>
        {
            UIManager.instance.ShowGamePanel<PlayerHomeEquipPanel, HomeEquipList>(GetHomeEquipList(DisplayHomeEquipPanel.characterId));
        });
        GameActionManager.instance.AddListener<UnSetHomeEquip>(UnSetHomeEquip);
    }

    protected override void Clear()
    {
        base.Clear();
        homeEquips.Clear();
    }

    public HomeEquip GetHomeEquip(int instanceId)
    {
        if (homeEquips.TryGetValue(instanceId, out var homeEquip))
        {
            return homeEquip;
        }
        return null;
    }

    public HomeEquipList GetHomeEquipList(int characterId)
    {
        HomeEquipList homeEquipList = new HomeEquipList();
        homeEquipList.homeEquips = new List<HomeEquip>();
        if (characterHomeEquips.TryGetValue(characterId, out var ints))
        {
            for (int i = 0; i < ints.Count; i++)
            {
                int instanceId = ints[i];
                if (homeEquips.TryGetValue(instanceId, out var homeEquip))
                {
                    if (homeEquip.homeEquipmentData.hide)
                    {
                        continue;
                    }
                    homeEquipList.homeEquips.Add(homeEquip);
                }
            }
        }
        return homeEquipList;
    }

    private void UnSetHomeEquip(UnSetHomeEquip unSetHomeEquip)
    {
        if (homeEquips.TryGetValue(unSetHomeEquip.instanceId, out var homeEquip))
        {
            homeEquip.mapInstance = -1;
            homeEquip.coordinate = int2.zero; 
            RefreshHomeEquip(homeEquip);
            unSetHomeEquip.setResult(true);
        }
    }

    private async void CreatHomeEquip(CreatHomeEquip creatHomeEquip)
    {
        HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(creatHomeEquip.equipDataId);
        HomeEquip homeEquip = new HomeEquip(creatHomeEquip.instanceId == 0 ? WorldMapManager.instance.GetInstanceFromItem() : creatHomeEquip.instanceId,
           creatHomeEquip.instanceId, creatHomeEquip.characterId, homeEquipmentData); 

        if (!characterHomeEquips.TryGetValue(creatHomeEquip.characterId, out var ints))
        {
            ints = new List<int>();
            characterHomeEquips.Add(creatHomeEquip.characterId, ints);
        }
        ints.Add(homeEquip.instanceId);

        int count = 1;
        if (!characterHomeEquipCountData.TryGetValue(creatHomeEquip.characterId, out var equipCountData))
        {
            equipCountData = new Dictionary<int, int>();
            characterHomeEquipCountData.Add(creatHomeEquip.characterId, equipCountData);
        }
        else
        {
            if (equipCountData.TryGetValue(creatHomeEquip.equipDataId, out var _count))
            {
                count = _count + 1;
            }
        }
        equipCountData[creatHomeEquip.equipDataId] = count; 

        switch (homeEquipmentData.homeEquipFunc)
        {
            case HomeEquipFunc.床:
                break;
            case HomeEquipFunc.箱子:
                if (homeEquipmentData.homeEquipFuncValue != 0)
                {
                    CreatPackage creatPackage = new CreatPackage
                    {
                        packageDataId = homeEquipmentData.homeEquipFuncValue,
                        level = 0,
                        instanceId=homeEquip.instanceId
                    };
                    GameActionManager.instance.QueueAction(creatPackage);
                }
                break;
            case HomeEquipFunc.柜台:
                if (homeEquipmentData.homeEquipFuncValue != 0)
                {
                    CreatStoreCounter creatStoreCounter = new CreatStoreCounter
                    {
                        itemInstanceId = homeEquip.instanceId,
                        storeDataId = homeEquipmentData.homeEquipFuncValue
                    };
                    GameActionManager.instance.QueueAction(creatStoreCounter);
                }
                 break;
            case HomeEquipFunc.生产:
                if (homeEquipmentData.homeEquipFuncValue != 0)
                {
                    CreatManufature creatManufature = new CreatManufature
                    {
                        instanceId = homeEquip.instanceId,
                        manufatureId = homeEquipmentData.homeEquipFuncValue
                    };
                    GameActionManager.instance.QueueAction(creatManufature);
                } 
                break;
            case HomeEquipFunc.装饰:
                break;
        }
         
        if (creatHomeEquip.setResult != null)
        {
            creatHomeEquip.setResult(true);
        }
    }

    private void RemoveHomeEquip(RemoveHomeEquip removeHomeEquip)
    {
        if (homeEquips.TryGetValue(removeHomeEquip.instanceId, out var homeEquip))
        {
            if (homeEquips.Remove(removeHomeEquip.instanceId))
            {
                if (characterHomeEquips.TryGetValue(removeHomeEquip.characterId, out var ints))
                {
                    ints.Remove(removeHomeEquip.instanceId);

                    if (characterHomeEquipCountData.TryGetValue(removeHomeEquip.characterId, out var HomeEquipCountData))
                    {
                        if (HomeEquipCountData.TryGetValue(homeEquip.homeEquipmentData.id, out var count))
                        {
                            count--;
                            if (count <= 0)
                            {
                                HomeEquipCountData.Remove(homeEquip.homeEquipmentData.id);
                            }
                            else
                            {
                                HomeEquipCountData[homeEquip.homeEquipmentData.id] = count;
                            }
                        }
                    }
                }
            }
        }
    }

    private void ChangeHomeEquipCharacter(ChangeHomeEquipCharacter changeHomeEquipCharacter)
    {
        if (homeEquips.TryGetValue(changeHomeEquipCharacter.equipInstanceId, out var homeEquip))
        {
            int oldCharacter = homeEquip.characterId;
            if (oldCharacter != changeHomeEquipCharacter.newPlayer)
            {
                if (characterHomeEquips.TryGetValue(oldCharacter, out var ints))
                {
                    ints.Remove(changeHomeEquipCharacter.equipInstanceId);

                    if (characterHomeEquipCountData.TryGetValue(oldCharacter, out var HomeEquipCountData))
                    {
                        if (HomeEquipCountData.TryGetValue(homeEquip.equipDataId, out var count))
                        {
                            count--;
                            if (count <= 0)
                            {
                                HomeEquipCountData.Remove(homeEquip.equipDataId);
                            }
                            else
                            {
                                HomeEquipCountData[homeEquip.equipDataId] = count;
                            }
                        }
                    }
                }
                if (characterHomeEquips.TryGetValue(changeHomeEquipCharacter.newPlayer, out var ints1))
                {
                    ints1.Add(changeHomeEquipCharacter.equipInstanceId);

                    int count = 1;
                    Dictionary<int, int> equipCountData = new Dictionary<int, int>();
                    if (!characterHomeEquipCountData.TryGetValue(changeHomeEquipCharacter.newPlayer, out equipCountData))
                    {
                        characterHomeEquipCountData.Add(changeHomeEquipCharacter.newPlayer, equipCountData);
                    }
                    else
                    {
                        if (equipCountData.TryGetValue(changeHomeEquipCharacter.newPlayer, out var _count))
                        {
                            count = _count + 1;
                        }
                    }
                    equipCountData[homeEquip.equipDataId] = count;
                }
                homeEquip.characterId = changeHomeEquipCharacter.newPlayer; 
            }
        }
    }

    private void TryLayInHomeEquip(TryLayInHomeEquip tryLayInHomeEquip)
    {
        if (characterHomeEquips.TryGetValue(tryLayInHomeEquip.characterId, out var ints))
        {
            if (ints.Contains(tryLayInHomeEquip.equipInstanceId))
            {
                if (homeEquips.TryGetValue(tryLayInHomeEquip.equipInstanceId, out var homeEquip))
                {
                    homeEquip.mapInstance = 0;
                    homeEquip.coordinate = int2.zero; 
                    tryLayInHomeEquip.setResult(true);
                }
                return;
            }
        }
        tryLayInHomeEquip.setResult(false);
    }

    private void SetHomeEquipCoordinate(SetHomeEquipCoordinate setHomeEquipCoordinate)
    {
        if (characterHomeEquips.TryGetValue(setHomeEquipCoordinate.characterId, out var ints))
        {
            if (ints.Contains(setHomeEquipCoordinate.equipInstanceId))
            {
                if (homeEquips.TryGetValue(setHomeEquipCoordinate.equipInstanceId, out var homeEquip))
                {
                    homeEquip.mapInstance = setHomeEquipCoordinate.mapInstanceId;
                    homeEquip.coordinate = setHomeEquipCoordinate.coordinate; 
                    if (setHomeEquipCoordinate.setResult != null)
                    {
                        setHomeEquipCoordinate.setResult(true);
                    }
                }
            }
            return;
        }
        if (setHomeEquipCoordinate.setResult != null)
            setHomeEquipCoordinate.setResult(false);
    }

    private void RefreshHomeEquip(HomeEquip homeEquip)
    {
        if (homeEquip.mapItemInstance != 0)
        {
            MoveMapItem moveMapItem = new MoveMapItem
            {
                mapItemInstanceId = homeEquip.mapItemInstance,
                mapInstance = homeEquip.mapInstance,
                coordinate = homeEquip.coordinate
            };
            GameActionManager.instance.QueueAction(moveMapItem, true);
        }
        else
        {
            AddMapItem addMapItem = new AddMapItem
            {
                mapId = homeEquip.mapInstance,
                coordinate = homeEquip.coordinate,
                dataId = homeEquip.mapItemInstance,
                setValue = SetMapItem
            };
            void SetMapItem(int itemInstance)
            {
                homeEquip.mapItemInstance = itemInstance; 
            }
            GameActionManager.instance.QueueAction(addMapItem, true);
        }
    }

    private void RefreshHomeEquip(RefreshHomeEquip refreshHomeEquip)
    {
        if (homeEquips.TryGetValue(refreshHomeEquip.equipInstanceId, out var homeEquip))
        {
            RefreshHomeEquip(homeEquip);
        }
    }

    public async void BuyAction(ShopItemData selectShopItemData)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectShopItemData.item);
        if (itemData == null)
        {
            return;
        }
        if (characterHomeEquipCountData.TryGetValue(CharacterManager.instance.controllerCharacter.instanceId, out var equipCountData) &&
            equipCountData.TryGetValue(selectShopItemData.item, out var count))
        {
            GameManager.instance.ShowTwoSelectAction("家具", $"已经拥有{count}个{itemData.itemName},是否确定购买", BuyHomeEquip, null);
        }
        else
        {
            BuyHomeEquip();
        }
        void BuyHomeEquip()
        {
            //四舍五入取整
            int trueCost = (int)math.ceil(itemData.shopPrice * selectShopItemData.priceValue * 0.01f);
            PayManager.instance.PayAction("购买", $"购买1个+ {itemData.itemName} +", trueCost, selectShopItemData.payType, async (bool result) =>
            {
                if (!result)
                {
                    return;
                }

                CreatHomeEquip CreatHomeEquip = new CreatHomeEquip
                {
                    characterId = CharacterManager.instance.controllerCharacter.instanceId, 
                    equipDataId = itemData.typeValue
                };
                GameActionManager.instance.QueueAction(CreatHomeEquip);

                InformationController.instance.AddInformation($"成功购买1个+ {itemData.itemName} +");
                if (selectShopItemData.buyAction != 0)
                {
                    var GameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(selectShopItemData.buyAction);
                    GameActionData.Action();
                }
                ShopBuySuccess shopBuySuccess = new ShopBuySuccess
                {
                    buyCount = 1
                };
                GameActionManager.instance.QueueAction(shopBuySuccess);
            });
        }
    }
}

public struct HomeEquipList : IReferenceData
{
    public List<HomeEquip> homeEquips;
}

public class HomeEquip : INativeData, IReferenceData
{
    public int instanceId;
    public int mapItemInstance; 
    public HomeEquipmentData homeEquipmentData; 
    public int2 coordinate;
    public int mapInstance;
    public int characterId; 
    public int Key => instanceId;

    public int equipDataId=> homeEquipmentData.id;

    public HomeEquip(int instanceId, int mapItemInstance, int characterId,HomeEquipmentData homeEquipmentData)
    {
        this.instanceId = instanceId;
        this.mapItemInstance = mapItemInstance;
        this.characterId = characterId;
        this.homeEquipmentData=homeEquipmentData;
    }
    public bool Equals(IReferenceData other)
    {
        if (other is HomeEquip homeEquip)
        {
            return homeEquip.instanceId == instanceId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return instanceId;
    }

    public void Dispose()
    {
    }
}