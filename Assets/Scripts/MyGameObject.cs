using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameObject
{
    public int id;
    public GameObject Obj;
    public string Name;
    public int mapId;
    public Vector2Int coordinate;

    public MyGameObject(int _id,GameObject _Obj, string _name,int _mappId,Vector2Int _coordinate)
    {
        id = _id;
        Obj = _Obj;
        Name = _name;
        coordinate = _coordinate;
        mapId=_mappId;
    }
    public MyGameObject(int _id, string _name, int _mappId, Vector2Int _coordinate)
    {
        id = _id;
        Name = _name.Split('.')[0];
        coordinate = _coordinate;
        mapId = _mappId;
    }
    public MyGameObject() { }
}
