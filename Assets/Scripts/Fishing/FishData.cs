using BehaviorDesigner.Runtime; 
using UnityEngine;

public class FishData:ScriptableObject,IGameData,IReferenceData
{
    public int id;
    public int itemId;
    public string fishName;
    public string showObjName;
    public GameObject showObj;
    public string behaviorName;
    public ExternalBehaviorTree externalBehavior;
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
        return fishName;
    }
    public void SetReferenceData()
    {
        string fishObjPath = "Prefabs/Fish/";
        showObj = Resources.Load<GameObject>($"{fishObjPath}{showObjName}");
        externalBehavior = Resources.Load<ExternalBehaviorTree>($"Behavior/NPC/{behaviorName}");
    }
}