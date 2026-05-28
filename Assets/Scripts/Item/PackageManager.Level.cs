using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PackageManager
{
    public void CheckCharacterPackageFull(CheckCharacterPackageFull checkCharacterPackageFull)
    {
        Character character = CharacterManager.instance.GetCharacter(checkCharacterPackageFull.characterId);
        if (gamePackages.TryGetValue(character.characterPackage, out var gamePackage))
        {
            if (checkCharacterPackageFull.setResult != null)
            {
                checkCharacterPackageFull.setResult(gamePackage.itemCount < gamePackage.caseCount);
            }
        }
    }

    public int GetPackageLevelUpCost(int id)
    {
        if (gamePackages.TryGetValue(id, out GamePackage gamePackage))
        {
            if (gamePackage.packageSetData)
            {
                return (gamePackage.level + 1) * gamePackage.packageSetData.levelUpCost;
            }
        }
        return 0;
    }

    public void AddPackageUpLevel(int id)
    {
        if (gamePackages.TryGetValue(id, out GamePackage gamePackage))
        {
            if (gamePackage.packageSetData)
            {
                gamePackage.level += 1;
                gamePackage.caseCount += gamePackage.packageSetData.levelUpAddCount;
               // gamePackages[id] = gamePackage;
                GameActionManager.instance.QueueAction(default(RefreshPackage));

                SetItemAnimation setItemAnimation = new SetItemAnimation
                {
                    id = gamePackage.instanceId,
                    keyX = int.MinValue,
                    keyY = gamePackage.level
                };
                GameActionManager.instance.QueueAction(setItemAnimation);

                RefreshShortcut refreshShortcut = new RefreshShortcut
                {
                    packageId = gamePackage.instanceId
                };
                GameActionManager.instance.QueueAction(refreshShortcut);
            }
        }
    }

    public int AddPackageCaseCount(int packageId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            gamePackage.caseCount += count;
            //gamePackages[packageId] = gamePackage;
            return gamePackage.caseCount;
        }
        return 0;
    }

    public void SetPackageCaseCount(int packageId, int count)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            gamePackage.caseCount = count;
            //gamePackages[packageId] = gamePackage;
        }
    }

    public int GetPackageCaseCount(int packageId)
    {
        if (gamePackages.TryGetValue(packageId, out GamePackage gamePackage))
        {
            return gamePackage.caseCount;
        }
        return 0;
    }

}
