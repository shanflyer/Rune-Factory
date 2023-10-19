using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState
{
    修养中=0,正常=1
}
public struct NPCDataList : IReferenceData
{
    public List<NPCData> NPCDatas;
}
public struct NPCData:IReferenceData
{
    public int npcInstanceId;
    public int characterInstanceId;
    public int characterDataId;
    public int friendValue;
    public int friendLevel;
    public NPCState npcState;
}
public class NPCManager : Singleton<NPCManager>
{ 
    public override void Init()
    {
        base.Init();
    }
    Dictionary<int, NPCData> npcs = new Dictionary<int, NPCData>();
    public NPCData GetNPCData(int instanceId)
    {
        if(npcs.TryGetValue(instanceId,out var nPCData))
        {
            return nPCData;
        }
        return default(NPCData);
    }
}
