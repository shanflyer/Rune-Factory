using System.Threading.Tasks;

public partial class PackageManager
{
    private void RemoveRuntimePackage(RemoveRuntimePackage removeRuntimePackage)
    {
        if (runtimePackageRuntimes.TryGetValue(removeRuntimePackage.key, out int instanceId))
        {
            if (gamePackages.ContainsKey(instanceId))
            {
                gamePackages.Remove(instanceId);
            }
            runtimePackageRuntimes.Remove(removeRuntimePackage.key);
        }
    }

    private async Task CreatRuntimePackageAsync(CreatRuntimePackage creatRuntimePackage)
    {
        int instanceId = creatRuntimePackage.instanceId;
        if (instanceId < 0)
        {
            instanceId = MyInstance.instance.Uid;
        }
        GamePackage gamePackage = new GamePackage
        {
            instanceId = instanceId,
            name = creatRuntimePackage.name,
            caseCount = creatRuntimePackage.caseCount,
            itemPackage = creatRuntimePackage.itemPackage
        };
        for (int i = 0; i < creatRuntimePackage.Items.Count; i++)
        {
            await gamePackage.SetItemInPackage(creatRuntimePackage.Items[i]);
        }
        gamePackages.Add(creatRuntimePackage.instanceId, gamePackage);
        runtimePackageRuntimes.Add(creatRuntimePackage.key, creatRuntimePackage.instanceId);
    }
}
