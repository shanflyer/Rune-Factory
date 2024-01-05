using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/FieldArea")]
public class FieldArea : ScriptableObject, IGameData
{
    public string FieldName;
    public int id;
    public int mapId;
    public List<int> fields = new List<int>();
    public bool open = false;

    public override string ToString()
    {
        return id.ToString();
    }
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}
