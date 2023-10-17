
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 public class FriendManager:Singleton<FriendManager>
{
    public override void Init()
    {
        base.Init();
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
}