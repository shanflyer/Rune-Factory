using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<TryVisitShop>(TryVisitShop);
    }
    async void TryVisitShop(TryVisitShop tryVisitShop)
    {
        string shopName = tryVisitShop.ShopName;
        if (string.IsNullOrEmpty(shopName))
        {
            Character character = CharacterManager.instance.GetCharacter(tryVisitShop.CharacterId);
            shopName = character.characterData.shopName;
        } 
        ShopGroup shopGroup = await GameDataManager.instance.GetAsyncData<ShopGroup>(shopName);
        if (shopGroup.shopDatas != null)
        {
            UIManager.instance.ShowGamePanel<ShopPanel, ShopGroup>(shopGroup);
        } 
    }
}