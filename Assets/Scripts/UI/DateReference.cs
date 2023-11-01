using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DateReference : UIObjReference<GameDate>
{
    [SerializeField]
    private TextMeshProUGUI ValueText;
    [SerializeField]
    private Image festivalTips;
    [SerializeField]
    private Image backGround;
    [SerializeField]
    private Toggle selectToggle;

    private GameDate gameDate;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        selectToggle = GetComponent<Toggle>();
        ValueText = FindChildGameObject<TextMeshProUGUI>("Value");
        festivalTips = FindChildGameObject<Image>("festivalTips");
        backGround = FindChildGameObject<Image>("backGround");
    }
    public void SetToggleAction(ToggleGroup toggleGroup, UnityAction<bool> toggleAction)
    {
        selectToggle.group = toggleGroup;
        selectToggle.onValueChanged.AddListener(toggleAction);
    }
 
    public override void InitData(GameDate _gameDate, SelectAction<GameDate> SelectAction = null,ToggleGroup toggleGroup=null)
    {
        gameDate = _gameDate;
        ValueText.text = _gameDate.date.ToString();
        festivalTips.enabled = gameDate.FestivaList.Length > 0;
        backGround.color= (gameDate.date - 1) % 6 == 0? new Color(1, 0.76f, 0.64f):Color.white;
    } 
	 
}
