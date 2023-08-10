using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageSelectAction : MonoBehaviour
{
    [HideInInspector]
    public ShopPanelAction shopPanelAction;

    public void CickAction(Toggle toggle)
    {
        if (toggle.isOn)
        {
            shopPanelAction.SelectPackage(GetComponentInChildren<Text>().text);
        }
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
