using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class AdventureTeamerRenference: UIObjReference<FighterResult>
{
    [SerializeField]
    Image Icon;
    [SerializeField]
    Transform LevelUp;
    [SerializeField]
    Transform SkillUp;
    [SerializeField]
    TextMeshProUGUI Name;
    [SerializeField]
    TextMeshProUGUI Level;
     
    public override void SetPanelUISerializeObj()
    {
        Icon = FindChildGameObject<Image>("NPCImage");
        LevelUp = FindChildGameObject("LevelUp");
        SkillUp = FindChildGameObject("SkillUp");
        Name = FindChildGameObject<TextMeshProUGUI>("NPCName");
        Level = FindChildGameObject<TextMeshProUGUI>("NPCLevel");

        base.SetPanelUISerializeObj();
    }
    public override void InitData(FighterResult t, SelectAction<FighterResult> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);

        this.LevelUp.localScale = data.levelUp ? Vector3.one : Vector3.zero;
        this.SkillUp.localScale = data.skillUp ? Vector3.one : Vector3.zero;

        Icon.sprite = data.Character.characterData.icon;
        Name.text = data.Character.name;
        Level.text = $"Lv.{data.Character.Level}";
    }
    
}
