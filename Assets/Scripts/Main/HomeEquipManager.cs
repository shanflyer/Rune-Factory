using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Collections;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

public class HomeEquipManager:Singleton<HomeEquipManager>
{
    MyNativeData<HomeEquip> homeEquips = new MyNativeData<HomeEquip>();

    NativeHashMap<int, int> itemEquips = new NativeHashMap<int, int>(4,Allocator.TempJob);

    Dictionary<int, List<int>> characterHomeEquips = new Dictionary<int, List<int>>();
    Dictionary<int, Dictionary<int,int>> characterHomeEquipCountData = new Dictionary<int, Dictionary<int, int>>();
    MyInstance myInstance;
    public override void Init()
    {
        base.Init();
        myInstance = new MyInstance();
        homeEquips.Init(8);
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
    }
    protected override void Clear()
    {
        base.Clear();
        homeEquips.Dispose();
        itemEquips.Dispose();
    }
    public HomeEquipList GetHomeEquipList(int characterId)
    {
        HomeEquipList homeEquipList=new HomeEquipList();
        homeEquipList.homeEquips = new List<HomeEquip>();
        if(characterHomeEquips.TryGetValue(characterId,out var ints))
        {
            for(int i = 0; i < ints.Count; i++)
            {
                int instanceId = ints[i];
                if(homeEquips.GetData(instanceId,out var homeEquip))
                {
                    homeEquipList.homeEquips.Add(homeEquip);
                }
            }
        } 
        return homeEquipList;
    }
    void CreatHomeEquip(CreatHomeEquip creatHomeEquip)
    {
        int instanceId = myInstance.CreatInstanceId();
        HomeEquip homeEquip = new HomeEquip
        {
            instanceId = instanceId,
            dataId = creatHomeEquip.equipDataId,
            characterId=creatHomeEquip.characterId,
        };
        homeEquips.SetData(homeEquip);
        if(!characterHomeEquips.TryGetValue(creatHomeEquip.characterId,out var ints))
        {
            ints = new List<int>();
            characterHomeEquips.Add(creatHomeEquip.characterId, ints);
        } 
        ints.Add(instanceId);
        
        int count =1;
        Dictionary<int, int> equipCountData = new Dictionary<int, int>();
        if (!characterHomeEquipCountData.TryGetValue(creatHomeEquip.characterId,out equipCountData))
        {
            characterHomeEquipCountData.Add(creatHomeEquip.characterId,equipCountData);
        }
        else
        {
            if(equipCountData.TryGetValue(creatHomeEquip.equipDataId, out var _count))
            {
                count = _count + 1;
            }
        }
        equipCountData[creatHomeEquip.equipDataId] = count;
        
    }
    void RemoveHomeEquip(RemoveHomeEquip removeHomeEquip)
    {
        if (homeEquips.GetData(removeHomeEquip.instanceId, out var homeEquip))
        {
            if (homeEquips.RemoveData(removeHomeEquip.instanceId))
            {
                if (characterHomeEquips.TryGetValue(removeHomeEquip.characterId, out var ints))
                {
                    ints.Remove(removeHomeEquip.instanceId);

                    if (characterHomeEquipCountData.TryGetValue(removeHomeEquip.characterId, out var HomeEquipCountData))
                    {
                        if (HomeEquipCountData.TryGetValue(homeEquip.dataId, out var count))
                        {
                            count--;
                            if (count <= 0)
                            {
                                HomeEquipCountData.Remove(homeEquip.dataId);
                            }
                            else
                            {
                                HomeEquipCountData[homeEquip.dataId] = count;
                            }
                           
                        }
                    }
                }
            }
        }

    }
    void ChangeHomeEquipCharacter(ChangeHomeEquipCharacter changeHomeEquipCharacter)
    {
        if(homeEquips.GetData(changeHomeEquipCharacter.equipInstanceId,out var homeEquip))
        {
            int oldCharacter = homeEquip.characterId;
            if (oldCharacter != changeHomeEquipCharacter.newPlayer)
            {
                if(characterHomeEquips.TryGetValue(oldCharacter,out var ints))
                {
                    ints.Remove(changeHomeEquipCharacter.equipInstanceId);

                    if (characterHomeEquipCountData.TryGetValue(oldCharacter, out var HomeEquipCountData))
                    {
                        if (HomeEquipCountData.TryGetValue(homeEquip.dataId, out var count))
                        {
                            count--;
                            if (count <= 0)
                            {
                                HomeEquipCountData.Remove(homeEquip.dataId);
                            }
                            else
                            {
                                HomeEquipCountData[homeEquip.dataId] = count;
                            } 
                        }
                    }
                }
                if(characterHomeEquips.TryGetValue(changeHomeEquipCharacter.newPlayer,out var ints1))
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
                    equipCountData[homeEquip.dataId] = count;
                } 
                homeEquip.characterId = changeHomeEquipCharacter.newPlayer; 
                homeEquips.SetData(homeEquip);
            }
        }
    }
    void TryLayInHomeEquip(TryLayInHomeEquip tryLayInHomeEquip)
    {
        if(characterHomeEquips.TryGetValue(tryLayInHomeEquip.characterId,out var ints))
        {
            if (ints.Contains(tryLayInHomeEquip.equipInstanceId))
            {
                if(homeEquips.GetData(tryLayInHomeEquip.equipInstanceId,out var homeEquip))
                {
                    homeEquip.mapInstance = 0;
                    homeEquip.coordinate = int2.zero;
                    homeEquips.SetData(homeEquip);
                    tryLayInHomeEquip.setResult(true);
                }
                return;
            }
        }
        tryLayInHomeEquip.setResult(false);
    }
    void SetHomeEquipCoordinate(SetHomeEquipCoordinate setHomeEquipCoordinate)
    {
        if(characterHomeEquips.TryGetValue(setHomeEquipCoordinate.characterId,out var ints))
        {
            if (ints.Contains(setHomeEquipCoordinate.equipInstanceId))
            {
                if(homeEquips.GetData(setHomeEquipCoordinate.equipInstanceId,out var homeEquip))
                {
                    homeEquip.mapInstance = setHomeEquipCoordinate.mapInstanceId;
                    homeEquip.coordinate = setHomeEquipCoordinate.coordinate;
                    homeEquips.SetData(homeEquip);
                    setHomeEquipCoordinate.setResult(true);
                }
            }
            return;
        }
        setHomeEquipCoordinate.setResult(false);
    }

    async void RefreshHomeEquip(RefreshHomeEquip refreshHomeEquip)
    {
        if(homeEquips.GetData(refreshHomeEquip.equipInstanceId,out var homeEquip))
        {
            if(homeEquip.mapItemId!=0)
            {
                if (homeEquip.mapInstance == 0)
                {
                    DeleteMapItem deleteMapItem = new DeleteMapItem
                    {
                        mapItemInstanceId = homeEquip.mapItemId,
                        triggerClear = true
                    };
                    GameActionManager.instance.QueueAction(deleteMapItem,true);
                }
                else
                {
                    MoveMapItem moveMapItem = new MoveMapItem
                    {
                        mapItemInstanceId = homeEquip.mapItemId,
                        mapInstance = homeEquip.mapInstance,
                        coordinate = homeEquip.coordinate
                    };
                    GameActionManager.instance.QueueAction(moveMapItem, true);
                }
            }
            else
            {
                if(homeEquip.mapInstance!=0) 
                {
                   // ItemData homeEquipData = await GameDataManager.instance.GetAsyncData<ItemData>(homeEquip.dataId);
                    //if (homeEquipData != null)
                    {
                        AddMapItem addMapItem = new AddMapItem
                        {
                            mapId = homeEquip.mapInstance,
                            coordinate = homeEquip.coordinate,
                            dataId = homeEquip.mapItemId,
                            setValue=SetMapItem
                        };
                        void SetMapItem(int itemInstance)
                        {
                            homeEquip.mapItemId = itemInstance;
                            homeEquips.SetData(homeEquip);
                        }
                        GameActionManager.instance.QueueAction(addMapItem, true);
                    }
                }
            }
        }
    }

    public async void BuyAction(ShopItemData selectShopItemData)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectShopItemData.item);
        if (itemData == null)
        {
            return;
        }
        if (characterHomeEquipCountData.TryGetValue(CharacterManager.instance.controllerCharacter.instanceId,out var equipCountData))
        {
            if(equipCountData.TryGetValue(selectShopItemData.item,out var count))
            {
                GameManager.instance.ShowTwoSelectAction("家具", $"已经拥有{count}个{itemData.itemName},是否确定购买", BuyHomeEquip,null);
            }
        }
        void BuyHomeEquip()
        {
            int trueCost = (int)(itemData.shopPrice * selectShopItemData.priceValue * 0.01f);
            PayManager.instance.PayAction("购买", $"购买{1}个+ {itemData.itemName} +", trueCost, selectShopItemData.payType, async (bool result) =>
            {
                if (!result)
                {
                    return;
                } 

                CreatHomeEquip CreatHomeEquip = new CreatHomeEquip
                {
                    characterId = CharacterManager.instance.controllerCharacter.instanceId,
                    equipDataId = selectShopItemData.item
                };
                GameActionManager.instance.QueueAction(CreatHomeEquip);

                InformationController.instance.AddInformation($"成功购买{1}个+ {itemData.itemName} +");
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
public struct HomeEquip : INativeData,IReferenceData
{
    public int instanceId;
    public int mapItemId;
    public int dataId;
    public int2 coordinate;
    public int mapInstance;
    public int characterId;
    public int Key => instanceId;

    public void Dispose()
    { 
    }
}