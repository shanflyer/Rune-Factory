
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
public class FriendManager:Singleton<FriendManager>
{
    public override async void  Init()
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
        }

        GameActionManager.instance.AddListener<AddFriendShipValue>(AddFriendShipValue);
        GameActionManager.instance.AddListener<TryGiveGiftOpenPackage>(TryGiveGiftOpenPackage);
    }
    async void TryGiveGiftOpenPackage(TryGiveGiftOpenPackage tryGiveGiftOpenPackage)
    {
        PackageList packageList = new PackageList
        {
            packageDatas = new List<PackageData>()
        };
        Character character = CharacterManager.instance.GetCharacter(tryGiveGiftOpenPackage.fromCharacterId);
        if(character!=null )
        {
            var packageData = PackageManager.instance.GetPackageData(character.characterPackage);
            packageList.packageDatas.Add(packageData);

            var warehousePanel=await UIManager.instance.ShowGamePanel<WarehousePanel, PackageList>(packageList);
            warehousePanel.SetSelectItemAction(SelectAction, "赠送");

            void SelectAction(Item item, int packageId)
            {
                warehousePanel.Close();
                int count = PackageManager.instance.GetPackageItemCount(packageId, item.dataId);
                if (count >= 1)
                {
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


    Dictionary<int, FriendShip> NPCFriendShips = new Dictionary<int, FriendShip>();
 
    public int GetFriendShipLevel(int characterId)
    {
        if (NPCFriendShips.TryGetValue(characterId, out var friendShip))
        {
            return friendShip.friendLevel;
        }
        return -1;
    }
    void AddFriendShipValue(AddFriendShipValue addFriendShipValue)
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