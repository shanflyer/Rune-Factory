using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk:MyGameObject {
    public DeskAction deskAction;
    public Item item;
    public Desk(int _id,string _name, GameObject _obj,int _mapId,Vector2Int _coordiante,DeskAction _deskAction):base(_id,_obj,_name,_mapId,_coordiante)
    {
        deskAction = _deskAction;
       
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
