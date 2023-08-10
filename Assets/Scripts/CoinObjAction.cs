using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinObjAction : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
		
	}

    public void ClickAction()
    {
        GameComponentData.gameData.coinAction.ClickCoin(gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
