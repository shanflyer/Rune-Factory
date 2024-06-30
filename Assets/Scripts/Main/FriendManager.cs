using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public struct FriendShip
{
    public int characterId;
    public int nowValue;
    public int friendLevel;

    public async void AddValue(int value)
    {
        int totalVaue = value + nowValue;
        if (value < 0)
        {
            while (totalVaue < 0)
            {
                friendLevel--;
                FriendShipData friendShipData = await GameDataManager.instance.GetAsyncData<FriendShipData>(friendLevel);
                totalVaue += friendShipData.needValue;
            }
            nowValue = totalVaue;
        }
        else
        {
            while (totalVaue > 0)
            {
                FriendShipData friendShipData = await GameDataManager.instance.GetAsyncData<FriendShipData>(friendLevel);
                totalVaue -= friendShipData.needValue;
                if (totalVaue >= 0)
                {
                    friendLevel++;
                    nowValue = totalVaue;
                }
            }
        }
    }
}

public enum FriendAddType
{
    对话=1,礼物=2,邀请=3,其他=4
}
public class FriendManager : Singleton<FriendManager>
{
    
    public override async void Init()
    {
        base.Init();
        NPCFriendShips.Clear();
        var allNpc = await GameDataManager.instance.GetAllAsyncData<NPCData>();
        for (int i = 0; i < allNpc.Count; i++)
        {
            var npc = allNpc[i];
            FriendShip friendShip = new FriendShip
            {
                characterId = npc.id,
                friendLevel = npc.zeroFriendShipLevel,
            };
            NPCFriendShips[npc.id] = friendShip;
            friendAdd[npc.id] = GameCommon.friendAddCount;
        }

        GameActionManager.instance.AddListener<AddFriendShipValue>(AddFriendShipValue);
        GameActionManager.instance.AddListener<TryGiveGiftOpenPackage>(TryGiveGiftOpenPackage);
        GameActionManager.instance.AddListener<GiveGift>(GiveGift);
        GameActionManager.instance.AddListener<NewDay>(NewDay);
    }
    private void NewDay(NewDay newDay)
    {
        if (friendAdd.Count > 0)
        {
            var keys = friendAdd.Keys.ToList();
            foreach (var key in keys)
            {
                friendAdd[key] = GameCommon.friendAddCount;
            }
        }
       
    }
    private void GiveGift(GiveGift giveGift)
    {
        EventReferenceData eventReferenceData = new EventReferenceData
        {
            name = "目标人物",
            value = giveGift.receiveCharacter
        };
        EventReferenceData eventReferenceData1 = new EventReferenceData
        {
            name = "CharacterId",
            value = giveGift.giveCharacter
        };
        EventReferenceData eventReferenceData2 = new EventReferenceData
        {
            name = "礼物Id",
            value = giveGift.giftId
        };
        List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
        {
            eventReferenceData,eventReferenceData1,eventReferenceData2,
        };
        GameEventManager.instance.AddGameEvent(GameCommon.giftEventId, eventReferenceDatas);
    }

    private async void TryGiveGiftOpenPackage(TryGiveGiftOpenPackage tryGiveGiftOpenPackage)
    {
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>()
        };

        bool isAnimal = PastureManager.instance.GetAnimal(tryGiveGiftOpenPackage.toCharacterId, out var animal);
       

        Character character = CharacterManager.instance.GetCharacter(tryGiveGiftOpenPackage.fromCharacterId);
        if (character != null)
        {
            var packageData = PackageManager.instance.GetPackageData(character.characterPackage);
            if (isAnimal)
            {
                AnimalData animalData = animal.animalData;
                for(int i = 0; i < packageData.items.Count; i++)
                {
                    var item = packageData.items[i];
                    if (!animalData.foods.Contains(item.dataId))
                    {
                        item.locked = false;
                        packageData.items[i] = item;
                    }
                }
            } 
            packageList.packageDatas.Add(packageData);

            var warehousePanel = await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
            warehousePanel.SetSelectItemAction(SelectAction, isAnimal?"投喂": "赠送");

            void SelectAction(Item item,bool select)
            {
                warehousePanel.Close();
                int count = PackageManager.instance.GetPackageItemCount(item.packageId, item.dataId);
                if (count >= 1)
                {
                    UIManager.instance.CloseGamePanel<WarehousePanel>();
                    GiveGift giveGift = new GiveGift
                    {
                        giftId = item.dataId,
                        giveCharacter = tryGiveGiftOpenPackage.fromCharacterId,
                        receiveCharacter = tryGiveGiftOpenPackage.toCharacterId
                    };
                    GameActionManager.instance.QueueAction(giveGift, true);
                }
            }
        }
    }

    public void InitFriendSaveData(FriendSaveData friendSaveData)
    {
        foreach (var value in friendSaveData.friendShips)
        {
            FriendShip friendShip = new FriendShip
            {
                characterId = value.x,
                friendLevel = value.y,
                nowValue = value.z
            };
            NPCFriendShips[value.x] = friendShip;
        }
        foreach(var value in friendSaveData.friendAdds)
        {
            friendAdd[value.x] = value.yzw;
        }
    }
    public FriendSaveData GetFriendSaveData()
    {
        FriendSaveData friendSaveData = new FriendSaveData
        {
            friendAdds = new List<int4>(),
            friendShips = new List<int3>()
        };
        foreach(var friendShip in NPCFriendShips)
        {
            friendSaveData.friendShips.Add(new int3(friendShip.Value.characterId, friendShip.Value.friendLevel,
                friendShip.Value.nowValue)); 
        }
        foreach(var friend in friendAdd)
        {
            friendSaveData.friendAdds.Add(new int4(friend.Key, friend.Value));
        }
        return friendSaveData;
    }
    
    private Dictionary<int, FriendShip> NPCFriendShips = new Dictionary<int, FriendShip>();

    private Dictionary<int, int3> friendAdd = new Dictionary<int, int3>();
    public int GetFriendShipLevel(int characterId)
    {
        if (NPCFriendShips.TryGetValue(characterId, out var friendShip))
        {
            return friendShip.friendLevel;
        }
        return 0;
    }

    private void AddFriendShipValue(AddFriendShipValue addFriendShipValue)
    {
        bool canAddFriendShip = true;
        if (addFriendShipValue.value > 0)
        {
            canAddFriendShip = false;
            if (friendAdd.TryGetValue(addFriendShipValue.characterId, out var int3))
            {
                switch (addFriendShipValue.friendAddType)
                {
                    case FriendAddType.对话:
                        int3.x--;
                        canAddFriendShip = int3.x > 0;
                        break;
                    case FriendAddType.礼物:
                        int3.y--;
                        canAddFriendShip = int3.y > 0;
                        break;
                    case FriendAddType.邀请:
                        int3.z--;
                        canAddFriendShip = int3.z > 0;
                        break;
                    default:
                        break;
                }
                int3 = math.clamp(int3.zero, int3, int3);
                friendAdd[addFriendShipValue.characterId] = int3;
            }
        }
        if (canAddFriendShip)
        {
            if (NPCFriendShips.TryGetValue(addFriendShipValue.characterId, out var friendShip))
            {
                friendShip.AddValue(addFriendShipValue.value);
                NPCFriendShips[addFriendShipValue.characterId] = friendShip;
                RefreshFriendShip refreshFriendShip = new RefreshFriendShip
                {
                    characterId = addFriendShipValue.characterId
                };
                GameActionManager.instance.QueueAction(refreshFriendShip);
            }
        }
    }

    public void AddFriendShip(int characterId, int value)
    {
        if (NPCFriendShips.TryGetValue(characterId, out var friendShip))
        {
            friendShip.AddValue(value);
            NPCFriendShips[characterId] = friendShip;
            RefreshFriendShip refreshFriendShip = new RefreshFriendShip
            {
                characterId = characterId
            };
            GameActionManager.instance.QueueAction(refreshFriendShip);
        }
    }
}