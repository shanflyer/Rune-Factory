using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamerRenference : UIObjReference
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
    void InitData(Character character, bool LevelUp = false,bool SkillUp=false)
    {
        this.LevelUp.localScale = LevelUp ? Vector3.one : Vector3.zero;
        this.SkillUp.localScale = SkillUp ? Vector3.one : Vector3.zero;

        Icon.sprite = character.characterData.icon;
        Name.text = character.name;
        Level.text = $"Lv.{character.Level}";
    } 
}
