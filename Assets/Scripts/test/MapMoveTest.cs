using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class MapMoveTest : MonoBehaviour
{
    public Tilemap tileMap;

    public RuleTile x;
	// Use this for initialization
	void Start ()
	{
	    tileMap.SetTile(Vector3Int.one, x);
	}

    
	// Update is called once per frame
	void Update () {
		
	}
}
