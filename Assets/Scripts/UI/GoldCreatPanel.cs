using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldCreatPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Transform createrParent;
    [SerializeField]
    TextMeshProUGUI resultValue;
    [SerializeField]
    Button actionButton;
    [SerializeField]
    Button ReturnButton;
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
        ReturnButton.onClick.AddListener(Close);
        resultValue.text = "";
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        createrParent = FindChildGameObject("CreaterList");
        resultValue = FindChildGameObject<TextMeshProUGUI>("ResultValue");
        actionButton = FindChildGameObject<Button>("Action");
        createrReference = FindChildGameObject<CreaterReference>("CreaterReference");
        CreaterGroup = createrParent.GetComponent<ToggleGroup>();

        ReturnButton = FindChildGameObject<Button>("ReturnButton");
    } 

    MoneyCreatData selectMoneyCreatData;
    void SelectCreater(MoneyCreatData MoneyCreatData,bool selected)
    {
        if (selected)
        {
            selectMoneyCreatData = MoneyCreatData;
            resultValue.text = MoneyCreatData.getValue.ToString();
        }
        
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
    public override void Close()
    {
        base.Close();
        gameObject.SetActive(false);
    }
}
