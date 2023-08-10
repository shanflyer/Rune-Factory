using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManufacBuildClickAction : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
		
	}

    public void clickAction(int i)
    {
        GameComponentData.gameData.formulaAction.DisplayManufacturePanel(i);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
