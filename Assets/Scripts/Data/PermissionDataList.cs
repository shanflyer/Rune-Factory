using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PermissionDataList : ScriptableObject, IGameData, IDataArray<PermissionData>
{
    public PermissionData[] permissionDatas;
    public PermissionData[] DataList => permissionDatas;

    public override string ToString()
    {
        return "PermissionDataList";
    }
    public string GetKey()
    {
        return "PermissionDataList";
    }
    public void SetReferenceData()
    {
    }
}
public struct PermissionData :IGameData, IReferenceData
{
    public int id;
    public string permissionName;
    public string conditionStr;
    public int reward;
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
