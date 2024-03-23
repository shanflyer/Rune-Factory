using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private void Awake()
    {
        selectToggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, value);
            }
        });
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        selectToggle = GetComponent<Toggle>();
        ValueText = FindChildGameObject<TextMeshProUGUI>("Value");
        festivalTips = FindChildGameObject<Image>("festivalTips");
        backGround = FindChildGameObject<Image>("backGround");
    } 

    public override async Task InitData(GameDate t, SelectAction<GameDate> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        selectToggle.group = toggleGroup;
        ValueText.text = data.date.ToString();
        festivalTips.enabled = gameDate.FestivaList.Length > 0;
        backGround.color = (gameDate.date - 1) % 6 == 0 ? new Color(1, 0.76f, 0.64f) : Color.white; 

    }

}
