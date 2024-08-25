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
    private List<int3> talkForFriendShip;

    public Int2IntDictionary talkDatas=new Int2IntDictionary();

    public int GetTalk(int friendShipLevel,int mapInstance)
    {
        int talkId = -1;
        int2 key = new int2(friendShipLevel, mapInstance);

        for (int i = friendShipLevel; i >= 0; i--)
        {
            key = new int2(i, mapInstance);
            if (talkDatas.TryGetValue(key, out talkId))
            {
                var results = GameRandom.instance.GetRandomValue(talkId);
                if (results.Count > 0)
                {
                    talkId = results[0].x;
                }
                return talkId;
            }
        }
        for (int i = friendShipLevel; i >= 0; i--)
        {
            key = new int2(i, 0);
            if (talkDatas.TryGetValue(key, out talkId))
            {
                var results = GameRandom.instance.GetRandomValue(talkId);
                if (results.Count > 0)
                {
                    talkId = results[0].x;
                }
                return talkId;
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
        talkDatas.Clear();
        for(int i = 0; i < talkForFriendShip.Count; i++)
        {
            talkDatas[talkForFriendShip[i].xy] = talkForFriendShip[i].z;
        }
    }
}