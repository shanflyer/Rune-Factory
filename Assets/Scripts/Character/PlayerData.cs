using System.Collections;
using UnityEngine;
[System.Serializable]
public enum Gender
{
    male = 0,
    female = 1
}
public struct PlayerData
{
    public string name;
    public BrithDay brithDay;
}

public struct BrithDay
{
    public int year;
    public Season season;
    public int day;
}
 