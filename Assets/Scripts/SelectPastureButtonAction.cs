using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPastureButtonAction : MonoBehaviour
{
    public Pasture pasture;
	// Use this for initialization
	void Start () {
		
	}

    public void ClickAction()
    {
        GameComponentData.gameData.animalSetPanelAction.MoveAnimalPasture(pasture);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
