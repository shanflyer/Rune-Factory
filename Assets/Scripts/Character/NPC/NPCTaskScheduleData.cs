using BehaviorDesigner.Runtime;
using System;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

[CreateAssetMenu(menuName = "Data/Npc任务计划表数据")]
public class NPCTaskScheduleData : ScriptableObject, IGameData
{
    public int id;
    public string taskName;
    public NPCTaskScheduleType type;
    private string behaviorName;
    public ExternalBehaviorTree externalBehavior;
    public bool canBreak;
    public bool PauseWhenDisabled;
    public bool holdPos;
    public NPCBehaviorState behaviorState;
    public float maxPauseTime;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR

    public void SetReferenceData()
    {
        var path = $"{EditorDataPath.npcBehaviorPath}New/{behaviorName}{".asset"}";
        externalBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);
    }

#endif
}

public enum NPCTaskScheduleType
{
    时间, 事件,
}

[Serializable]
public struct GameTimeKey : IEquatable<int2>, IEquatable<GameTimeKey>
{
    public int2 minTime, maxTime;

    public static explicit operator GameTimeKey(string str)
    {
        var strs = str.Split(',');
        GameTimeKey gameTimeKey = new GameTimeKey
        {
            minTime =new int2(int.Parse(strs[0]), int.Parse(strs[1])),
            maxTime = new int2(int.Parse(strs[2]), int.Parse(strs[3])),
        };
        return gameTimeKey;
    }

    public static explicit operator GameTimeKey(int2 value)
    {
        GameTimeKey gameTimeKey = new GameTimeKey
        {
            minTime=value,
            maxTime=value
        };
        return gameTimeKey;
    }
    public static explicit operator GameTimeKey(int4 value)
    {
        GameTimeKey gameTimeKey = new GameTimeKey
        {
            minTime = value.xy,
            maxTime = value.zw
        };
        return gameTimeKey;
    }
    public override string ToString()
    {
        return $"{minTime.x},{minTime.y},{maxTime.x},{maxTime.y}";
    }

    public GameTimeKey(int[] timeArray)
    {
        minTime = new int2(timeArray[0], timeArray[1]);
        maxTime = new int2(timeArray[2], timeArray[3]);
    }

    public GameTimeKey(int minHour, int minMinute, int maxHour, int maxMinute)
    {
        minTime = new int2(minHour, minMinute);
        maxTime = new int2(maxHour, maxMinute);
    }

    public override bool Equals(object obj)
    {
        if (obj is GameTimeKey timeKey)
        {
            return Equals(timeKey);
        }
        else if (obj is int2 time)
        {
            return Contains(time);
        }
        return false;
    }

    public static bool operator ==(GameTimeKey gameTimeKey, int2 timeKey)
    {
        return gameTimeKey.Contains(timeKey);
    }

    public static bool operator !=(GameTimeKey gameTimeKey, int2 timeKey)
    {
        return !gameTimeKey.Contains(timeKey);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + minTime.x;
            hash = hash * 31 + minTime.y;
            hash = hash * 31 + maxTime.x;
            hash = hash * 31 + maxTime.y;
            return hash;
        }
    }

    public bool Equals(int2 time)
    {
        return Contains(time);
    }

    public bool Contains(int2 time)
    {
        int startMinute = minTime.x * 60 + minTime.y;
        int endMinute = maxTime.x * 60 + maxTime.y;
        int minute = time.x * 60 + time.y;

        if (startMinute == endMinute)
        {
            return minute == startMinute;
        }

        return minute >= startMinute && minute < endMinute;
    }

    public bool Equals(GameTimeKey timeKey)
    {
        return SameRange(timeKey);
    }

    public bool SameRange(GameTimeKey timeKey)
    {
        return timeKey.minTime.x == minTime.x && timeKey.minTime.y == minTime.y &&
               timeKey.maxTime.x == maxTime.x && timeKey.maxTime.y == maxTime.y;
    }

    public bool HasValidRange()
    {
        return minTime.x >= 0 && minTime.x <= 23 &&
               maxTime.x >= 0 && maxTime.x <= 24 &&
               minTime.y >= 0 && minTime.y <= 59 &&
               maxTime.y >= 0 && maxTime.y <= 59 &&
               (maxTime.x > minTime.x || maxTime.x == minTime.x && maxTime.y >= minTime.y);
    }
}
