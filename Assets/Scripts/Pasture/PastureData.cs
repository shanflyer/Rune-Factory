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
    /// <summary>
    /// x:消耗;y:容量;z:表现
    /// </summary>
    public List<int3> levelDatas = new List<int3>();
    public int eventId;
    public int packageId;
    public int foodPackageId;
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