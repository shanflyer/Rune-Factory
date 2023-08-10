using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PastureNameClick : MonoBehaviour
{
    public GameObject InputGameObject;
    public Text text0, text1, text2, text3;
    public void ClickTips(Text nameText)
    {
        InputGameObject = GameComponentData.gameData.InputObj;
        int index = int.Parse(nameText.name);
        if (GameComponentData.gameData.pastureAction.IsPastures[index])
        {
            InputGameObject.SetActive(true);
            InputGameObject.GetComponent<InputFieldAction>().NameText = nameText;
            
            InputGameObject.GetComponentInChildren<InputField>().text =
                GameComponentData.gameData.pastureAction.Pastures[index].name;
            InputGameObject.GetComponent<InputFieldAction>().inputStr = GameComponentData.gameData.pastureAction.Pastures[index].name;
        }

        
        
    }
	// Use this for initialization
	void Start () {
		
	}

    public void BuildClick(GameObject UnBuildObj)
    {
        GameComponentData.gameData.pastureAction.ClickBuild(UnBuildObj);
    }

    public void CLickHouse(GameObject obj)
    {
        GameComponentData.gameData.pastureAction.ClickPastureHouse(obj);
    }
    public void SetPastureName(int index, string name)
    {
        if (index == 0)
        {
            text0.text = name;
        }
        else if(index==1)
        {
            text1.text = name;
        }
        else if (index == 2)
        {
            text2.text = name;
        }
        else if (index == 3)
        {
            text3.text = name;
        }
        GameComponentData.gameData.pastureAction.Pastures[index].name = name;
    }
    public void InitPastureName()
    {
        text0.text = GameComponentData.gameData.pastureAction.Pastures[0].name;
        text1.text = GameComponentData.gameData.pastureAction.Pastures[1].name;
        text2.text = GameComponentData.gameData.pastureAction.Pastures[2].name;
        text3.text = GameComponentData.gameData.pastureAction.Pastures[3].name;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
