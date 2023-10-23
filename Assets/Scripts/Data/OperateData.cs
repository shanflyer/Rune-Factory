 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public int eventId;
    public GameEventData gameEventData;
    public string GetKey()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string actionDataPath = $"{DataPath.GetDataPath(typeof(GameActionData))}/{actionId}";
        gameActionData = Resources.Load<GameActionData>(actionDataPath);

        string eventDataPath = $"{DataPath.GetDataPath(typeof(GameEventData))}/{actionId}";
        gameEventData = Resources.Load<GameEventData>(eventDataPath);
    }
#endif

}