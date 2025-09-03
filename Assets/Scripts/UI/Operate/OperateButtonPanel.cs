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
        if (!SingletonType.Cleared)
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
            OperateList1.InitListData(operateDatas, SelectAction);
        }
    }
    void SelectAction(OperateDataReferenceData operateData,int index,bool select)
    {
        PlayerOperateManager.instance.OperateAction(operateData, operateDataList.eventReferenceDatas);
    }

    public override async void InitReferenceData(OperateDataList v)
    {
        base.InitReferenceData(v);

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
            OperateList0.InitListData(v.OperateDatas, SelectAction);
        }
        else
        {
            List<OperateDataReferenceData> operateDatas = new List<OperateDataReferenceData>();

            for (int i = 0; i < 3; i++)
            {
                operateDatas.Add(v.OperateDatas[i]);
            }
            OperateData defaultData = await GameDataManager.instance.GetAsyncData<OperateData>(GameCommon.defaultOperateId);
            if (defaultData.checkActionData != null)
            {
                defaultData.checkActionData.Action(setResult: (bool result) =>
                {
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

            
            OperateList0.InitListData(operateDatas, SelectAction);
        }
    }
}