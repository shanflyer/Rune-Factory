using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PermissionPanel :GamePanel<IReferenceData>
{
    [SerializeField]
    Button ReturnButton;
    [SerializeField]
    Transform PermissionParent;
    [SerializeField]
    PermissionReference permissionReference;
    DisplayList<PermissionReference, Permission> permissionList;
    protected override void Awake()
    {
        base.Awake();
        permissionList = new DisplayList<PermissionReference, Permission>(permissionReference, PermissionParent);
        ReturnButton.onClick.AddListener(Close);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        ReturnButton = FindChildGameObject<Button>("ReturnButton");
        permissionReference = FindChildGameObject<PermissionReference>("PermissionReference");
        PermissionParent = FindChildGameObject("PermissionParent");
    }
    public override void InitData(string dataKey)
    {
        permissionList.InitListData(GamePlayerRecordManager.instance.Permissions);
        base.InitData(dataKey);
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }
}
