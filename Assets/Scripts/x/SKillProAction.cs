using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SKillProAction : MonoBehaviour
{
    private SkillData skill;
    private int index;

    public void InitSkillData(SkillData _skill,int _index)
    {
        skill = _skill;
        index = _index;
        if (skill != null)
        {
            GetComponentInChildren<Image>().color=new Color(0.86f,0.98f,0.54f);
            GetComponentInChildren<Text>().text = skill.name;
        }
        else
        {
            GetComponentInChildren<Image>().color = new Color(0.97f,0.35f,0.24f);
            GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("返回");
        }
        
    }

    public void ClickButton()
    {
        GameComponentData.gameData.fightPanelAction.SelectSkill(index);
    }
}
