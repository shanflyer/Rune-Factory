using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCaseObjAction : MonoBehaviour {
    public PackageType packageType;
	// Use this for initialization
	void Start () {
		
	}
	public void ClickAddAction()
    {
        GameComponentData.gameData.gameManager.AddPackageCase(packageType);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
