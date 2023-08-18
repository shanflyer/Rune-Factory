using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionDataList : ScriptableObject, IGameData,IDataArray<FunctionData>
{
    [SerializeField]
    List<FunctionData> functionDatas;

    public List<FunctionData> DataList => functionDatas;

    public string GetKey()
    {
        return name;
    }
}
[System.Serializable]
public struct FunctionData : IGameData
{
    public int id;
    public string buttonName;
    public List<FunctionData> secondFunctions;
    public GameActionData gameActionData;
    public string GetKey()
    {
        return id.ToString();
    }
}