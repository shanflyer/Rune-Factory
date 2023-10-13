using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class OperateButtonPanel : GamePanel<OperateDataList>
{
    public override bool changeInputModel => false;

    [SerializeField]
    Transform OperateParent0, OperateParent1;
    [SerializeField]
    OperateButtonReference operateButton;

    DisplayList<OperateButtonReference, OperateData> OperateList0, OperateList1;
    protected override void Awake()
    {
        base.Awake();
        OperateList0 = new DisplayList<OperateButtonReference, OperateData>(operateButton, OperateParent0);
        OperateList1 = new DisplayList<OperateButtonReference, OperateData>(operateButton, OperateParent1);
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
        GameActionManager.instance.RemoveListener<SwitchOperateList>(SwitchOperateList);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<SwitchOperateList>(SwitchOperateList);
    }
  
    OperateDataList operateDataList;
    bool otherListShow = false;
    void SwitchOperateList(SwitchOperateList switchOperateList)
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
            List<OperateData> operateDatas = new List<OperateData>();

            for (int i = 3; i < operateDataList.OperateDatas.Count; i++)
            {
                operateDatas.Add(operateDataList.OperateDatas[i]);
            }  
            OperateList1.InitListData(operateDatas, PlayerOperateManager.instance.OperateAction);
        } 
    }

    public override async void InitReferenceData(OperateDataList v)
    {
        base.InitReferenceData(v);
        operateDataList = v;
        OperateList1.ClearAll();
        otherListShow = false;
        if (v.OperateDatas.Count <= 4)
        {
            OperateList0.InitListData(v.OperateDatas, PlayerOperateManager.instance.OperateAction); 
        }
        else
        {
            List<OperateData> operateDatas = new List<OperateData>(); 

            for(int i = 0; i < 3; i++)
            {
                operateDatas.Add(v.OperateDatas[i]);
            }
            OperateData defaultData = await GameDataManager.instance.GetAsyncData<OperateData>(GameCommon.defaultOperateId);
            operateDatas.Add(defaultData);
            OperateList0.InitListData(operateDatas, PlayerOperateManager.instance.OperateAction);
        }
    }
}
