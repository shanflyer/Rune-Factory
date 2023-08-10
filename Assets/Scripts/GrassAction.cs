using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassAction : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void ClickAction()
    {
        GameComponentData.gameData.farmAction.ClickGrass(gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
