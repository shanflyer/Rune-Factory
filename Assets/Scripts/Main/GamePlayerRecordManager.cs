using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GamePlayerRecordManager:Singleton<GamePlayerRecordManager>
{
    public List<Permission> Permissions => permissions;
    private List<Permission> permissions = new List<Permission>();

    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }

}
public struct Permission:IReferenceData
{
    public bool isGet;
    public PermissionData permissionData;
}