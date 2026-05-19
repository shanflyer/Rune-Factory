 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public struct OperateDataList: IReferenceData
{
    public List<OperateDataReferenceData> OperateDatas;
    public List<EventReferenceData> eventReferenceDatas;
}
public struct OperateDataReferenceData : IReferenceData
{
    public int targetItem;
    public OperateData operateData;
}

[CreateAssetMenu(menuName ="Data/交互行为数据")]
public class OperateData : ScriptableObject, IGameData, IReferenceData
{
    public string operateName;
    public int id;
    public string iconName;
    public Sprite icon;
    public int checkId;
    public GameActionAsset checkActionData;
    public int actionId;
    public int linkItem; 
    public GameActionAsset gameActionData;
    public int eventId;
    public GameEventData gameEventData;
    public List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>();
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public string GetName()
    {
        return operateName;
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string actionDataPath = $"{DataPath.GetDataPath(typeof(GameActionAsset))}/{actionId}";
        gameActionData = Resources.Load<GameActionAsset>(actionDataPath);

        string eventDataPath = $"{DataPath.GetDataPath(typeof(GameEventData))}/{eventId}";
        gameEventData = Resources.Load<GameEventData>(eventDataPath);
    }
#endif

}