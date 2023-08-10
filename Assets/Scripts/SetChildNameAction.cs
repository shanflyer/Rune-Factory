
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetChildNameAction : MonoBehaviour
{
    private string childName;
    public InputField inputField;

	// Use this for initialization
	void Start ()
	{
	    childName = LanguageManage.SwitchStr("小宝宝");
	    inputField.text = childName;


	}

    public void SetInputFieldValue()
    {
        childName = InputFieldAction.Ctr(inputField.text, 15);
        inputField.text = childName;
    }

    public void OKButtonAction()
    {
        GameComponentData.gameData.gameManager.childData=new ChildData(childName);
        
        GameComponentData.gameData.filmManager.PlayNowFilm();
        gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
