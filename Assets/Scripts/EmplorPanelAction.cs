using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OldName;
public class EmplorPanelAction : MonoBehaviour
{
    public List<Text> Texts;
    public Image emplorImage;
    public Text LevelText,HpText,AtText,DfText,FriendText;
    public Text emplorNameText;
    public Text skillText;
    public Text FrientLevelText;
    public GameObject friendObj;
    public GameObject teamTips;
    [HideInInspector]
    public Employer employer;
    
    // Use this for initialization
    void Start () {
        foreach (var text in Texts)
        {
            LanguageManage.TextFanyi(text);
        }
        LanguageManage.TextFanyi(skillText);
	}

   
    public void Clicked()
    {
        GameComponentData.gameData.charactorShop.Selected(employer);
    }
    public void InitEmplorData(Employer _employer)
    {
        employer = _employer;
        if (employer.employType == EmployType.NPC)
        {
           // NPCData npcData = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.id == employer.id).npcData;
            friendObj.SetActive(true);
           // FriendText.text = npcData.friendlyLevel.ToString();
            float modulus = 1.0f;
            //if (npcData.friendlyLevel >= 10)
            {
                FrientLevelText.text = LanguageManage.SwitchStr("亲密");
                modulus = 0.0f;
            }
           // else if(npcData.friendlyLevel>=7)
            {
                FrientLevelText.text = LanguageManage.SwitchStr("友爱");
                modulus = 0.3f;
            }
            //else if(npcData.friendlyLevel>=4)
            {
                FrientLevelText.text = LanguageManage.SwitchStr("要好");
                modulus = 0.6f;
            }
           // else if(npcData.friendlyLevel>=2)
            {
                FrientLevelText.text = LanguageManage.SwitchStr("熟悉");
            }
            //else
            {
                FrientLevelText.text = LanguageManage.SwitchStr("陌生");
            }

            
            int emplorCost = Mathf.RoundToInt(200 + 150 * (employer.level / 5)*modulus);
            _employer.cost = emplorCost;
            if (employer.employType == EmployType.NPC)
            {
              //  if (GameComponentData.gameData.NpcManager.NpcDatas.Find(n => n.id == employer.id).professionId==10)
                {
                    _employer.cost = 0;
                }
            }
        }
        else
        {
            friendObj.SetActive(false);
        }
       // OldName. ProfessionData professionData = CharactorDataAction.professionDatas.Find(p => p.id == employer.id / 1000);
        //emplorImage.sprite = GameComponent.charactorIcon.Find(c => c.name == employer.charactorImage);
        emplorNameText.text = employer.name;
        LevelText.text = "Lv." + employer.level;
        AtText.text = "AT." + employer.property.AT;
        DfText.text = "DF." + employer.property.DF;
        HpText.text = "HP." + employer.property.HP;
        if (employer.skillId == 0)
        {
            skillText.text = LanguageManage.SwitchStr("属性:")+LanguageManage.SwitchStr(employer.attributeType.ToString());
        }
        else
        {
            
        }
        if (employer.isHired)
        {
            teamTips.SetActive(true);
        }
        else
        {
            teamTips.SetActive(false);
        }

    }
	// Update is called once per frame
	void Update () {
		
	}
}
