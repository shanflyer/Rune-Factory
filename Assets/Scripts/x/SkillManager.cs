using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using OldName;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public enum SkillType
{
    固定攻击=1,
    资质攻击=2
}
[System.Serializable]
public class SkillData
{
    public string name;
    public int id;
    public AttributeType attributeType;
    public int value;
    public SkillType skillType;
    public string effect;
}

public class Skill
{
    public SkillData skillData;
    public GameObject SkillEffet;
    public int attackValue;
    public Skill() { }

    public Skill(SkillData _skillData,int AT)
    {
        skillData = _skillData;
        attackValue = AT;
    }
    public Skill(int skillId,int AT)
    {
        skillData = GameComponentData.gameData.skillManager.skillDatas.Find(s=>s.id==skillId);
        attackValue = AT;
    }
}
public class SkillManager : MonoBehaviour
{
    public Transform playerSkillParent,monsterSkillParent;
    public GameObject selectSkillPanel;
    public List<SkillData> skillDatas;
    // Use this for initialization
    [HideInInspector]
    public int level;
    [HideInInspector]
    public int value;
    private float waitTime;
    private Skill actionSkill;
    private bool isPlayer;
    public void DataToJson()
    {
        skillDatas = new List<SkillData>();

        string path = Application.dataPath + "/Resources/Datas/Skills.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(skillDatas);
        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter stream = new StreamWriter(fileStream);
        stream.Write(jsonStr);
        stream.Close();
    }

    public void JsonToData()
    {
        string path = "Datas/Skills";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            string jsonStr = textAsset.text;
            skillDatas= JsonMapper.ToObject<List<SkillData>>(jsonStr);
        }
    }

    void Start () {
        foreach (var skillData in skillDatas)
        {
            skillData.name = LanguageManage.SwitchStr(skillData.name);
        }
	}

    public void SkillAction(Skill skill,bool _isPlayer,int AT,Vector3 pos)
    {
        isPlayer = _isPlayer;
        
        actionSkill = skill;
        if (actionSkill.skillData.skillType == SkillType.资质攻击)
        {
            actionSkill.attackValue =(int)(AT * 1.5f);
        }
        
       GameComponentData.gameData.battleDisplayAction.InitSkill(pos,skill.skillData.attributeType);
    }

    public void StartWaitSKillAction()
    {
        GameObject skillEffectPro = GameComponent.Effects.Find(e => e.name == actionSkill.skillData.effect);
        actionSkill.SkillEffet = Instantiate(skillEffectPro);
        if (isPlayer)
        {
            actionSkill.SkillEffet.transform.SetParent(playerSkillParent);
            actionSkill.SkillEffet.transform.localPosition = Vector3.zero;

        }
        else
        {
            actionSkill.SkillEffet.transform.SetParent(monsterSkillParent);
            actionSkill.SkillEffet.transform.localPosition = Vector3.zero;

        }
        waitTime = actionSkill.SkillEffet.GetComponent<DestoryAfterTime>().timeValue;
        StartCoroutine("WaitSKillAction");
    }
    IEnumerator WaitSKillAction()
    {
        float timeValue = 0;
        while (true)
        {
            
            yield return new WaitForFixedUpdate();
            timeValue += Time.deltaTime;
            if (timeValue > waitTime)
            {
                GameComponentData.gameData.BattleMapAction.AfterSkillEffect(actionSkill,isPlayer);
                GameComponentData.gameData.battleDisplayAction.HideMapMask();
                StopAllCoroutines();
            }
        }
       
    }
	// Update is called once per frame
	void Update () {
		
	}
}
#if UNITY_EDITOR
[CustomEditor(typeof(SkillManager))]
public class SkillManagerEditor : Editor
{
    private SkillManager skillManager { get { return (target as SkillManager); } }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            skillManager.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            skillManager
                .JsonToData();
        }
        base.OnInspectorGUI();
    }
}
#endif
