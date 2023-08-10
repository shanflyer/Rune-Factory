using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeritageProAction : MonoBehaviour
{
    public List<Text> Texts;

    public Text TitleText, NoticeText, ValueText;

    public GameObject tipe;
	// Use this for initialization
	void Start () {
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}

    public void InitData(Heritage heritage)
    {
        TitleText.text = heritage.title;
        NoticeText.text = heritage.tiaojian;
        ValueText.text = heritage.rewardValue.ToString();
        if (heritage.isGet)
        {
            tipe.SetActive(true);
        }
        else
        {
            tipe.SetActive(false);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
