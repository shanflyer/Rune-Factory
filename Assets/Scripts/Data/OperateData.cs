 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OperateDataList: IReferenceData
{
    public List<OperateData> OperateDatas;
}

[CreateAssetMenu(menuName ="Data/交互行为数据")]
public class OperateData : ScriptableObject, IGameData, IReferenceData
{
    public string operateName;
    public int id;
    public int actionId;
    public int linkItem; 
    public GameActionData gameActionData;
    public string GetKey()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string actionDataPath = $"{DataPath.GetDataPath(typeof(GameActionData))}/{actionId}";
        gameActionData = Resources.Load<GameActionData>(actionDataPath);
    }
#endif

}