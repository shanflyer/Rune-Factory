using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldCreatPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Transform createrParent;
    [SerializeField]
    Text resultValue;
    [SerializeField]
    Button actionButton;
    [SerializeField]
    ToggleGroup CreaterGroup;
    [SerializeField]
    CreaterReference createrReference;
    DisplayList<CreaterReference, MoneyCreatData> createrList;

    protected override void Awake()
    {
        base.Awake();
        createrList = new DisplayList<CreaterReference, MoneyCreatData>(createrReference, createrParent);
        actionButton.onClick.AddListener(() =>
        {
            if(selectMoneyCreatData!= null)
            {
                PayManager.instance.AddGold(selectMoneyCreatData);
            }
        });
        resultValue.text = "";
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        createrParent = FindChildGameObject("CreaterList");
        resultValue = FindChildGameObject<Text>("ResultValue");
        actionButton = FindChildGameObject<Button>("Action");
        createrReference = FindChildGameObject<CreaterReference>("CreaterReference");
        CreaterGroup = createrParent.GetComponent<ToggleGroup>();
    } 

    MoneyCreatData selectMoneyCreatData;
    void SelectCreater(MoneyCreatData MoneyCreatData,bool selected)
    {
        selectMoneyCreatData = MoneyCreatData;
    }
    public override async void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
        var datas =await GameDataManager.instance.GetAllAsyncData<MoneyCreatData>();
        List<MoneyCreatData> MoneyCreatDatas = new List<MoneyCreatData>();
        for(int i = 0; i < datas.Count; i++)
        {
            if (datas[i].getPayType == PayType.½ð±Ò)
            {
                MoneyCreatDatas.Add(datas[i]);
            }
        }
        createrList.InitListData(MoneyCreatDatas, SelectCreater,CreaterGroup);
    }
}
