using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FunctionInfoData : ScriptableObject, IGameData, IReferenceData,IDataArray<InfoData>
{
    public int id;
    [SerializeField]
    private InfoData[] InfoData;

    public InfoData[] DataList => InfoData;

    public string GetKey()
    {
        return id.ToString();
    }
    public void SetKey(string key)
    {
        id=int.Parse(key);
    }
    public void SetObjList(List<object> list)
    {
        InfoData = new InfoData[list.Count];
        for(int i = 0; i < list.Count; i++)
        {
            InfoData[i] = (InfoData)list[i];
        }
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    {
    }

    public bool isSingleGroup()
    {
        return true;
    }
}
[Serializable]
public struct InfoData : IGameData, IReferenceData
{
    private int groupId;
    public string text;
    public string GetKey()
    {
        return groupId.ToString();
    }
    public override string ToString()
    {
        return groupId.ToString();
    }
    public void SetReferenceData()
    {
    }
}
