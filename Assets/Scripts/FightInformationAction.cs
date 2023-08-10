using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum InformationType
{
    使用道具=1,
    逃跑成功=2,
    逃跑失败=3,
    战斗失败=4,
    信息=5,
    战斗胜利=6
}
public class FightInformationAction : MonoBehaviour
{
    public Text valueText;

    public InformationType informationType;
	// Use this for initialization
	void Start () {
		
	}
    
    public void InitData(string valueStr,InformationType _informationType)
    {
        informationType = _informationType;
        valueText.text = valueStr;
    }

    public void InformationDisplayEnd()
    {
        GameComponentData.gameData.BattleMapAction.InformationEnd(informationType);
        gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
