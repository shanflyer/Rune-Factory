using UnityEngine;

public class NPCData : ScriptableObject, IReferenceData, IGameData
{
    public int id;
    public string npcName;
    public int linkCharacterId;
    public bool zeroCreate;
    public int zeroFriendShipLevel;
    public int overridePackage;
    public NPCState zeroState;
    public bool hide;

    public override string ToString()
    {
        return id.ToString();
    }

    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
    }
}