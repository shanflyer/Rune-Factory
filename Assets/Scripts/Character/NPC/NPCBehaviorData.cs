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
    public List<int> dailyTasks = new List<int>();
    public List<int> eventTasks = new List<int>();

    public List<int2> beds = new List<int2>();
    public List<int2> workItems = new List<int2>();

    public List<int> likeItems = new List<int>();
    public List<int> unLikeItems = new List<int>();
    public int likeTalk, unlikeTalk, likeEmote, unlikeEmote, defaultTalk, defaultEmote; 
    private string behaviorName;
    public ExternalBehaviorTree externalBehavior;

    private int[] gameTimeRanges;
    private int[] visitMaps;
    public GameTimeKeyIntDataDictionary gameTimeKeyVisitMapDic;
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
                    minHour = gameTimeRanges[index],
                    minMinute = gameTimeRanges[index + 1],
                    maxHour = gameTimeRanges[index + 2],
                    maxMinute = gameTimeRanges[index + 3]
                };
                gameTimeKeyVisitMapDic.Add(gameTimeKey, visitMaps[index]);
            }
        }
       
    }

#endif
}