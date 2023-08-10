using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine .UI;

public class TipsAction : MonoBehaviour
{
    public Text TitleText, NotceText;
	// Use this for initialization
	void Start () {
		
	}

    public void ClickYes()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Return");
        gameObject.SetActive(false);
    }
    public void InitTipsData(string title, string Notice)
    {
        TitleText.text = title;
        NotceText.text = Notice;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
