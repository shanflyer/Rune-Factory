using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PackageManager
{
    private void CheckCharacterItemValue(CheckCharacterItemValue CheckCharacterItemValue)
    {
        Character character = CharacterManager.instance.GetCharacter(CheckCharacterItemValue.characterId);
        if (character != null)
        {
            Item item = GetItemFromInstanceId(character.characterPackage, CheckCharacterItemValue.itemId);
            if (item.instanceId != 0)
            {
                if (item.value  >= CheckCharacterItemValue.itemValue)
                {
                    if (CheckCharacterItemValue.setResult != null)
                    {
                        CheckCharacterItemValue.setResult(true);
                        return;
                    }
                }
            }
        }
        if (CheckCharacterItemValue.setResult != null)
        {
            CheckCharacterItemValue.setResult(false);
        }
    }

    private void CheckItemValue(CheckItemValue checkItemValue)
    {
        if (gamePackages.TryGetValue(checkItemValue.packageId, out var gamePackage))
        {
            Item item = gamePackage.GetItemFromInstanceId(checkItemValue.itemDataId);
            if (item.instanceId == 0)
            {
                var items = gamePackage.GetItemFromDataId(checkItemValue.itemDataId);
                if (items != null)
                {
                    int totalValue = 0;
                    for (int i = 0; i < items.Count; i++)
                    {
                        totalValue += items[i].value;
                    }
                    if (totalValue >= checkItemValue.itemValue)
                    {
                        checkItemValue.setResult(true);
                    }
                    else
                    {
                        checkItemValue.setResult(false);
                    }
                    return;
                }
                if (checkItemValue.setResult != null)
                {
                    checkItemValue.setResult(false);
                }
            }
            else
            {
                if (item.value >= checkItemValue.itemValue)
                {
                    if (checkItemValue.setResult != null)
                    {
                        checkItemValue.setResult(true);
                    }
                }
                else
                {
                    if (checkItemValue.setResult != null)
                    {
                        checkItemValue.setResult(false);
                    }
                }
            }
        }
    }

    private async Task GiveGiftAsync(GiveGift giveGift)
    {
        Character receiveCharacter = CharacterManager.instance.GetCharacter(giveGift.receiveCharacter);
        if (receiveCharacter != null)
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(giveGift.giftId);
            if (itemData.useEventId != 0)
            {
                List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
                {
                   new EventReferenceData
                   {
                       name="CharacterId",
                       valueType=ReferenceValueType.Int,
                       value=giveGift.receiveCharacter
                   },
                   new EventReferenceData
                   {
                       name="SelectItem",
                       valueType=ReferenceValueType.Int,
                       value=giveGift.giftId
                   },
                };
                await GameEventManager.instance.AddGameEvent(itemData.useEventId, eventReferenceDatas);
            }
            else
            {
                await SetItemInPackage(new Item(giveGift.giftId, 1), receiveCharacter.characterPackage);
            }
        }
        Character giveCharacter = CharacterManager.instance.GetCharacter(giveGift.giveCharacter);
        if (giveCharacter != null)
        {
            GetOutItenFromPackage(giveCharacter.characterPackage, giveGift.giftId, 1);
        }
    }

    private async Task UsetItemAsync(ItemUseAction itemUseEvent)
    {
        if (gamePackages.TryGetValue(itemUseEvent.packageId, out GamePackage gamePackage))
        {
            if (await UsetItemAction(itemUseEvent.itemId,itemUseEvent.itemInstance,itemUseEvent.targetCharacter))
            {
                gamePackage.GetItemOutPackage(itemUseEvent.itemId, itemUseEvent.itemCount);
                //gamePackages[itemUseEvent.packageId] = gamePackage;
                RefreshPackageChanged(gamePackage);
            }
            else
            {

            }
        }
    }

    private async Task<bool> UsetItemAction(int itemId,int itemInstance,int targetCharacter=0)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId.ToString());
        if (itemData != null )
        {
            if (itemData.useEventId == 0)
            {
                InformationController.instance.AddInformation("这件物品不能自由使用", true, true);
                return false;
            }
            else
            {
                if (ExploreManager.instance.isExplore && itemData.sceneType == SceneType.城镇)
                {
                    InformationController.instance.AddInformation("什么也没发生", true, true);
                    return true;
                }
                if (!ExploreManager.instance.isExplore && itemData.sceneType == SceneType.战斗)
                {
                    InformationController.instance.AddInformation("什么也没发生", true, true);
                    return true;
                }
                List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
                {
                   new EventReferenceData
                   {
                       name="CharacterId",
                       valueType=ReferenceValueType.Int,
                       value=targetCharacter==0?CharacterManager.instance.controllerCharacter.instanceId:targetCharacter
                   },
                   new EventReferenceData
                   {
                       name="SelectItem",
                       valueType=ReferenceValueType.Int,
                       value=itemId
                   },
                    new EventReferenceData
                   {
                       name="ItemInstance",
                       valueType=ReferenceValueType.Int,
                       value=itemInstance
                   },
                     new EventReferenceData
                   {
                       name="ItemTypeValue",
                       valueType=ReferenceValueType.Int,
                       value=itemData.typeValue
                   },
                };
                await GameEventManager.instance.AddGameEvent(itemData.useEventId, eventReferenceDatas);
                if (!string.IsNullOrEmpty(itemData.useInfo))
                {
                    InformationController.instance.AddInformation(itemData.useInfo, true, true);
                }

                return true;
            }

        }
        return false;
    }
}
