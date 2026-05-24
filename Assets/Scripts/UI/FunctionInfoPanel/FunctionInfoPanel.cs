using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FunctionInfoPanel : GamePanel<FunctionInfoData>
{
    [SerializeField]
    private Transform infoParent;
    [SerializeField]
    private FunctionInfoReference FunctionInfoReference;
    [SerializeField]
    private Button close;
    private DisplayList<FunctionInfoReference, InfoData> functionInfoList;
    protected override void Awake()
    {
        functionInfoList = new DisplayList<FunctionInfoReference, InfoData>(FunctionInfoReference, infoParent);
        close.onClick.AddListener(Close);
        base.Awake();
    }
    public override void SetPanelUISerializeObj()
    {
        infoParent = FindChildGameObject("List");
        close = FindChildGameObject<Button>("Close");
        FunctionInfoReference = FindChildGameObject<FunctionInfoReference>("Info");
        base.SetPanelUISerializeObj();
    }
    public override void InitReferenceData(FunctionInfoData v)
    {
        base.InitReferenceData(v);
        // 功能说明列表绑定面板生命周期，关闭后旧列表不再回写。
        RunLifecycleTask(token => functionInfoList.InitListData(v.DataList.ToList(), cancellationToken: token), nameof(InitReferenceData));
    }

    public override Task InitData(string dataKey)
    {
        return base.InitData(dataKey);
    }
}
