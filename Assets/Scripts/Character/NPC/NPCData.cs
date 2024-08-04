using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

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
    public string text;
    public List<int> functionIds;
    public string shopName;
    public int playerOperateEventId;
    public int nextTalkEventId;
    public List<int2> talkForFriendShip;

    public int GetTalk(int friendShipLevel)
    {
        int talkId = -1;
        for (int i = 0; i < talkForFriendShip.Count; i++)
        {
            if (talkForFriendShip[i].x<= friendShipLevel)
            {
                var results = GameRandom.instance.GetRandomValue(talkForFriendShip[i].y);
                if (results.Count > 0)
                {
                    talkId=results[0].x;
                }
            }
            else
            {
                break;
            }
        }

        return talkId;
    }
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