using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundItemClickAction : MonoBehaviour {
    public void ClickAction(GameObject obj)
    {
        GameComponentData.gameData.gameManager.ClickGroundItem(obj);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
