using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCountAction : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void DestoryObj()
    {
        GameComponentData.gameData.gameManager.ChangePlayerMoney(int.Parse(GetComponentInChildren<Text>().text));
        Destroy(gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
