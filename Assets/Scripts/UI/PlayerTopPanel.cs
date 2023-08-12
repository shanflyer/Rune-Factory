using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTopPanel : GamePanel
{
    [SerializeField]
    Text goldValue, crystalValue;
    [SerializeField]
    Button goldAdd, crystalAdd;
    [SerializeField]
    Text date;
    [SerializeField]
    Button calendar;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        goldValue = FindChildGameObject<Text>("GoldValue");
        crystalValue = FindChildGameObject<Text>("CrystalValue");
        goldAdd = FindChildGameObject<Button>("GoldAdd");
        crystalAdd = FindChildGameObject<Button>("CrystalAdd");
        date=FindChildGameObject<Text>("Date");
        calendar = FindChildGameObject<Button>("CalendarButton");
    }
    protected override void Awake()
    {
        base.Awake();
        calendar.onClick.AddListener(() =>
        {

        });
        goldAdd.onClick.AddListener(GameController.instance.AddGold);
        crystalAdd.onClick.AddListener(GameController.instance.AddGold);
    }

   
    public override Task InitData(int dataId)
    {
        UpdateGameTime();
        return base.InitData(dataId);

    }
    public void UpdateMoney()
    {

    }
    public void UpdateGameTime()
    {
      var gameTime = GameTimeManager.nowGameTime;
        date.text = LanguageManage.instance.GameTimeToString(gameTime);
    }
    
}
