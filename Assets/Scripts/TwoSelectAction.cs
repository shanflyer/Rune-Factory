using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwoSelectAction : MonoBehaviour
{
    public Text TitleText,NoticeText;

	// Use this for initialization
	void Start () {
		
	}

    public void InitTwoSelectData(string title, string notice)
    {
        TitleText.text = title;
        NoticeText.text = notice;
    }

    public void YesClick()
    {
        GameComponentData.gameData.gameManager.YesButtonAction();
    }
    public void NoClick()
    {
        GameComponentData.gameData.gameManager.NoButtonAction();
    }
    // Update is called once per frame
    void Update () {
		
	}
}
