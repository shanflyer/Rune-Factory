using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTopPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    TextMeshProUGUI goldValue, crystalValue;
    [SerializeField]
    Button goldAdd, crystalAdd;
    [SerializeField]
    TextMeshProUGUI date;
    [SerializeField]
    Button calendar, SetButton;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        goldValue = FindChildGameObject<TextMeshProUGUI>("GoldValue");
        crystalValue = FindChildGameObject<TextMeshProUGUI>("CrystalValue");
        goldAdd = FindChildGameObject<Button>("Gold");
        crystalAdd = FindChildGameObject<Button>("Crystal");
        date=FindChildGameObject<TextMeshProUGUI>("Date");
        calendar = FindChildGameObject<Button>("TimeObj");
        SetButton = FindChildGameObject<Button>("SetButton");
       
    }
    protected override void Awake()
    {
        base.Awake();
        calendar.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<CalendarPanel>(layer: 3);
        });
        goldAdd.onClick.AddListener(PayManager.instance.TryCreatGold);
        crystalAdd.onClick.AddListener(PayManager.instance.TryCreatMoney);

        SetButton.onClick.AddListener(() =>
        {
           // AudioController.instance.PlayAudio(SE.click);
            UIManager.instance.ShowGamePanel<SetPanel>();
        });

        GameActionManager.instance.AddListener<RefreshPlayerGold>(RefreshPlayerGold);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
    }

   
    public override Task InitData(string dataKay)
    {
        goldValue.text = PayManager.instance.NowGold.ToString();
        crystalValue.text = PayManager.instance.NowDiamond.ToString();

       
        var gameTime = GameTimeManager.instance.nowGameTime;
        if (gameTime != null)
        {
            date.text = LanguageManage.instance.GameTimeToString(gameTime);
        }

        return base.InitData(dataKay);

    }
    public override void Show(int layer = -1)
    {
        base.Show(layer);
    }
    public override void InitReferenceData(IReferenceData v)
    {
        RefreshPlayerGold(default(RefreshPlayerGold));
        UpdateGameTime(default(UpdateGameTime)); 
        base.InitReferenceData(v);
    }
    void RefreshPlayerGold(RefreshPlayerGold updateMoney)
    {
        goldValue.text = PayManager.instance.NowGold.ToString();
        crystalValue.text = PayManager.instance.NowDiamond.ToString();
    }
    void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        var gameTime = GameTimeManager.instance.nowGameTime;
        if (gameTime != null)
        {
            date.text = LanguageManage.instance.GameTimeToString(gameTime);
        }
      
    }

}
