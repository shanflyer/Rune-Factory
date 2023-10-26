using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
public class PastureData : ScriptableObject, IGameData
{
    public string pastureName;
    public int id;
    public int mapId;
    public int linkItem;
    public int linkRoom;
    public int zeroLevel;
    public bool open = false;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
    }
}