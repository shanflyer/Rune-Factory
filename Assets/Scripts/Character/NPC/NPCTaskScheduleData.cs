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
            if (timeKey.maxTime.x== timeKey.minTime.x && timeKey.maxTime.y== timeKey.minTime.y)
            {
                if (timeKey.maxTime.x > minTime.x)
                {
                    if (timeKey.maxTime.x < maxTime.x)
                    {
                        return true;
                    }
                    else if (timeKey.maxTime.y < maxTime.y)
                    {
                        return true;
                    }
                }
                else if (timeKey.maxTime.x == minTime.x && timeKey.maxTime.y >= minTime.y)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return timeKey.minTime.x == minTime.x && timeKey.minTime.y == minTime.y && timeKey.maxTime.x == maxTime.x && timeKey.maxTime.y == maxTime.y;
            }
        }
        else if (obj is int2 time)
        {
            if (time.x > minTime.x)
            {
                if (time.x < maxTime.x)
                {
                    return true;
                }
                else if (time.y < maxTime.y)
                {
                    return true;
                }
            }
            else if (time.x == minTime.x && time.y >= minTime.y)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public static bool operator ==(GameTimeKey gameTimeKey, int2 timeKey)
    {
        if (timeKey.x > gameTimeKey.minTime.x)
        {
            if (timeKey.x < gameTimeKey.maxTime.x)
            {
                return true;
            }
            else if (timeKey.y < gameTimeKey.maxTime.y)
            {
                return true;
            }
        }
        else if (timeKey.x == gameTimeKey.minTime.x && timeKey.y >= gameTimeKey.minTime.y)
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(GameTimeKey gameTimeKey, int2 timeKey)
    {
        if (timeKey.x > gameTimeKey.minTime.x)
        {
            if (timeKey.x < gameTimeKey.maxTime.x)
            {
                return false;
            }
            else if (timeKey.y < gameTimeKey.maxTime.y)
            {
                return false;
            }
        }
        else if (timeKey.x == gameTimeKey.minTime.x && timeKey.y >= gameTimeKey.minTime.y)
        {
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public bool Equals(int2 time)
    {
        if (time.x > minTime.x)
        {
            if (time.x < maxTime.x)
            {
                return true;
            }
            else if (time.y < maxTime.y)
            {
                return true;
            }
        }
        else if (time.x == minTime.x && time.y >= minTime.y)
        {
            return true;
        }
        return false;
    }

    public bool Equals(GameTimeKey timeKey)
    {
        if (timeKey.maxTime.x == timeKey.minTime.x && timeKey.maxTime.y == timeKey.minTime.y)
        {
            if (timeKey.maxTime.x > minTime.x)
            {
                if (timeKey.maxTime.x < maxTime.x)
                {
                    return true;
                }
                else if (timeKey.maxTime.y < maxTime.y)
                {
                    return true;
                }
            }
            else if (timeKey.maxTime.x == minTime.x && timeKey.maxTime.y >= minTime.y)
            {
                return true;
            }
            return false;
        }
        else
        {
            return timeKey.minTime.x == minTime.x && timeKey.minTime.y == minTime.y && timeKey.maxTime.x == maxTime.x && timeKey.maxTime.y == maxTime.y;
        }
    }
}