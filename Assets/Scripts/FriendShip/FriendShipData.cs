using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FriendShipData : ScriptableObject, IGameData
{
    public string friendName;
    public int level;
    public int needValue;

    public override string ToString()
    {
        return level.ToString();
    }
    public string GetKey()
    {
        return level.ToString();
    }

    public void SetReferenceData()
    { 
    }
}