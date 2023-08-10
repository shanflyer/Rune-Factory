using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PastureBuildPanel : MonoBehaviour
{
    public Text TitleText, ReturnText, YesText,MuchangMingText,JianzaoHuafeiText;
	// Use this for initialization
	void Start ()
	{
	    TitleText.text = LanguageManage.SwitchStr(TitleText.text);
	    ReturnText.text = LanguageManage.SwitchStr(ReturnText.text);
	    YesText.text = LanguageManage.SwitchStr(YesText.text);
	    MuchangMingText.text = LanguageManage.SwitchStr(MuchangMingText.text);
	    JianzaoHuafeiText.text = LanguageManage.SwitchStr(JianzaoHuafeiText.text);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
