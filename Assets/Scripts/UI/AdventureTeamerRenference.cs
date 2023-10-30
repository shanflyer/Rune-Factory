using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
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
    Text Name;
    [SerializeField]
    Text Level;
     
    public override void SetPanelUISerializeObj()
    {
        Icon = FindChildGameObject<Image>("Icon");
        LevelUp = FindChildGameObject("LevelUp");
        SkillUp = FindChildGameObject("SkillUp");
        Name = FindChildGameObject<Text>("Name");
        Level = FindChildGameObject<Text>("Level");

        base.SetPanelUISerializeObj();
    }
    public  void InitData(FighterResult fighterResult, Action action = null)
    {
        this.LevelUp.localScale = fighterResult.levelUp ? Vector3.one : Vector3.zero;
        this.SkillUp.localScale = fighterResult.skillUp ? Vector3.one : Vector3.zero;

        Icon.sprite = fighterResult.Character.characterData.icon;
        Name.text = fighterResult.Character.name;
        Level.text = $"Lv.{fighterResult.Character.Level}";
       
    }
}
