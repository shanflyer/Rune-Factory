using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
#if UNITY_EDITOR

using UnityEditor;

#endif


public class NPCBehaviorData : ScriptableObject, IGameData
{
    public string npcName; public int id;
    public int home;
    public List<int> homeAreas = new List<int>();
    public int workMap;
    public List<int> workMapAreas = new List<int>();
    public List<int2> visitShops = new List<int2>();
    public int likeItem;
    public int unLikeItem;
    public int likeTalk, unlikeTalk, likeEmote, unlikeEmote, defaultTalk, defaultEmote;
    private string behaviorName;

    public List<int> dailyTasks = new List<int>(); 
    public List<int2> beds = new List<int2>();
    public List<int2> workItems = new List<int2>();

    private int[] gameTimeRanges;
    private int[] visitMaps; 
    public ExternalBehaviorTree externalBehavior; 
    public GameTimeKeyIntDataDictionary gameTimeKeyVisitMapDic; 
    public List<int2> npcFriends=new List<int2>();
   
    public override string ToString()
    {
        return id.ToString();
    }

    public string GetKey()
    {
        return id.ToString();
    }
     
#if UNITY_EDITOR

    public void SetReferenceData()
    {
        externalBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>($"{EditorDataPath.npcBehaviorPath}{behaviorName}.asset");

        gameTimeKeyVisitMapDic = new GameTimeKeyIntDataDictionary();
        if (gameTimeRanges != null && gameTimeRanges.Length > 0)
        {
            int count = gameTimeRanges.Length / 4;
            for (int i = 0; i < count; i++)
            {
                int index = i * 4;
                GameTimeKey gameTimeKey = new GameTimeKey
                {
                    minTime=new int2(gameTimeRanges[index], gameTimeRanges[index + 1]),
                    maxTime = new int2(gameTimeRanges[index+2], gameTimeRanges[index + 3]), 
                };
                gameTimeKeyVisitMapDic.Add(gameTimeKey, visitMaps[i]);
            }
        }
       
    }

#endif
}