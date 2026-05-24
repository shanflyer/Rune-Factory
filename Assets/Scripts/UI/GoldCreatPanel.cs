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
    void SelectCreater(MoneyCreatData MoneyCreatData, int index, bool selected)
    {
        if (selected)
        {
            selectMoneyCreatData = MoneyCreatData;
            resultValue.text = MoneyCreatData.getValue.ToString();
        }
        
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
        // 金币生成列表绑定面板生命周期，关闭后旧加载不再写入列表。
        RunLifecycleTask(InitReferenceDataAsync, nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(System.Threading.CancellationToken cancellationToken)
    {
        var datas =await GameDataManager.instance.GetAllAsyncData<MoneyCreatData>();
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        List<MoneyCreatData> MoneyCreatDatas = new List<MoneyCreatData>();
        for(int i = 0; i < datas.Count; i++)
        {
            if (datas[i].getPayType == PayType.金币)
            {
                MoneyCreatDatas.Add(datas[i]);
            }
        }
        await createrList.InitListData(MoneyCreatDatas, SelectCreater, CreaterGroup, cancellationToken: cancellationToken);
    }
    public override void Close()
    {
        base.Close();
        gameObject.SetActive(false);
    }
}
