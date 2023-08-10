using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookLanguageaction : MonoBehaviour
{
    public List<Text> Texts;
	// Use this for initialization
	void Start () {
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}

    public void ClickReturn()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Return");
        gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
