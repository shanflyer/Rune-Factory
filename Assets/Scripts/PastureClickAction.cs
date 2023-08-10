using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastureClickAction : MonoBehaviour {
    public void Click(int index)
    {
        if (!GameComponentData.gameData.mapEditAction.isMapEdit)
        {
            GameComponentData.gameData.pastureAction.ClickAnimal(index);
        }
        
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
