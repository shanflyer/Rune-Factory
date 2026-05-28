public partial class PackageManager
{
    private void RefreshPackageChanged(GamePackage gamePackage, bool shortcutImmediately = false)
    {
        RefreshPackageMapDisplay(gamePackage.caseCount, gamePackage.itemCount, gamePackage.instanceId);
        GameActionManager.instance.QueueAction(new RefreshShortcut
        {
            packageId = gamePackage.instanceId
        }, shortcutImmediately);
    }
}
