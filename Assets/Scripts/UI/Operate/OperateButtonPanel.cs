using System.Collections.Generic;
using UnityEngine;

public class OperateButtonPanel : GamePanel<OperateDataList>
{
    public override bool changeInputModel => false;

    [SerializeField]
    private Transform OperateParent0, OperateParent1;

    [SerializeField]
    private OperateButtonReference operateButton;

    private DisplayList<OperateButtonReference, OperateDataReferenceData> OperateList0, OperateList1;

    protected override void Awake()
    {
        base.Awake();
        OperateList0 = new DisplayList<OperateButtonReference, OperateDataReferenceData>(operateButton, OperateParent0);
        OperateList1 = new DisplayList<OperateButtonReference, OperateDataReferenceData>(operateButton, OperateParent1);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        operateButton = FindChildGameObject<OperateButtonReference>("OperateButton");
        OperateParent0 = FindChildGameObject("OperateButtonList");
        OperateParent1 = FindChildGameObject("OperateButtonList2");
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared && GameActionManager.HasInstance)
            GameActionManager.instance.RemoveListener<SwitchOperateList>(SwitchOperateList);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<SwitchOperateList>(SwitchOperateList);
    }

    private OperateDataList operateDataList;
    private bool otherListShow = false;

    private void SwitchOperateList(SwitchOperateList switchOperateList)
    {
        otherListShow = !otherListShow;
        if (!otherListShow)
        {
            OperateList1.ClearAll();
        }
        else
        {
            if (operateDataList.OperateDatas.Count <= 4)
            {
                return;
            }
            List<OperateDataReferenceData> operateDatas = new List<OperateDataReferenceData>();

            var debugLog = "";
            for (int i = 3; i < operateDataList.OperateDatas.Count; i++)
            {
                var operateData = operateDataList.OperateDatas[i];
                if (operateData.operateData != null)
                {
                    if (GameDataManager.instance.GlobalData.debug)
                        debugLog = $"{debugLog};{operateData.operateData.name}:{operateData.operateData.id}";
                    operateDatas.Add(operateDataList.OperateDatas[i]);
                }
            }

            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log(debugLog);
            // 操作列表展开绑定当前面板生命周期，关闭后旧二级列表不再回写。
            RunLifecycleTask(token => OperateList1.InitListData(operateDatas, SelectAction, cancellationToken: token), "OperateList1");
        }
    }
    void SelectAction(OperateDataReferenceData operateData,int index,bool select)
    {
        PlayerOperateManager.instance.OperateAction(operateData, operateDataList.eventReferenceDatas);
    }

    public override void InitReferenceData(OperateDataList v)
    {
        base.InitReferenceData(v);
        // 操作列表绑定面板生命周期，重开后旧数据不再覆盖当前按钮。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(OperateDataList v, System.Threading.CancellationToken cancellationToken)
    {

        v.OperateDatas.RemoveAll(d => d.operateData == null);
        if (v.OperateDatas.Count == 0)
        {
            Close(); return;
        }

        operateDataList = v;
      
        
        OperateList1.ClearAll();
        otherListShow = false;
        if (v.OperateDatas.Count <= 4)
        {
            var debugLog = "";
            for (var i = 0; i < v.OperateDatas.Count; i++)
                if (GameDataManager.instance.GlobalData.debug)
                {
                    var operateData = v.OperateDatas[i];
                    debugLog = $"{debugLog};{operateData.operateData.name}:{operateData.operateData.id}";
                }

            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log(debugLog);
            await OperateList0.InitListData(v.OperateDatas, SelectAction, cancellationToken: cancellationToken);
        }
        else
        {
            List<OperateDataReferenceData> operateDatas = new List<OperateDataReferenceData>();

            for (int i = 0; i < 3; i++)
            {
                operateDatas.Add(v.OperateDatas[i]);
            }
            OperateData defaultData = await GameDataManager.instance.GetAsyncData<OperateData>(GameCommon.defaultOperateId);
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

            if (defaultData.checkActionData != null)
            {
                defaultData.checkActionData.Action(setResult: (bool result) =>
                {
                    if (ShouldStopLifecycleTask(cancellationToken))
                    {
                        return;
                    }

                    if (result)
                    {
                        operateDatas.Add(new OperateDataReferenceData
                        {
                            operateData = defaultData,
                            targetItem = 0
                        });
                    }
                },immediately: true);
            }
            else
            {
                operateDatas.Add(new OperateDataReferenceData
                {
                    operateData = defaultData,
                    targetItem = 0
                });
            }


            await OperateList0.InitListData(operateDatas, SelectAction, cancellationToken: cancellationToken);
        }
    }
}
