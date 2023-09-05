using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DateReference : UIObjReference<GameDate>
{
    [SerializeField]
    private Text ValueText;
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
        ValueText = FindChildGameObject<Text>("Value");
        festivalTips = FindChildGameObject<Image>("festivalTips");
        backGround = FindChildGameObject<Image>("backGround");
    }
    public void SetToggleAction(ToggleGroup toggleGroup, UnityAction<bool> toggleAction)
    {
        selectToggle.group = toggleGroup;
        selectToggle.onValueChanged.AddListener(toggleAction);
    }
    public override void InitData(GameDate _gameDate, Action action = null)
    {
        gameDate = _gameDate;
        ValueText.text = _gameDate.date.ToString();
        festivalTips.enabled = gameDate.FestivaList.Count > 0;
        backGround.color= (gameDate.date - 1) % 6 == 0? new Color(1, 0.76f, 0.64f):Color.white;
    } 
	 
}
