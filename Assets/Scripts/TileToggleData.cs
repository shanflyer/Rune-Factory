using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileToggleData : MonoBehaviour {
    private MapEditAction mapEditAction;
	// Use this for initialization
	void Start () {
        mapEditAction = GameComponentData.gameData.mapEditAction;
	}
    public void Clicked()
    {
        mapEditAction.EditCell(gameObject.name);

    }
	// Update is called once per frame
	void Update () {
		
	}
}
