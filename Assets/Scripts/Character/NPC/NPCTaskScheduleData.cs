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
    public bool loopBehavior;
    public bool canBreak;
    public bool PauseWhenDisabled;
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
    public int minHour, minMinute, maxHour, maxMinute;

    public static explicit operator GameTimeKey(string str)
    {
        var strs = str.Split(',');
        GameTimeKey gameTimeKey = new GameTimeKey
        {
            minHour = int.Parse(strs[0]),
            minMinute = int.Parse(strs[1]),
            maxHour = int.Parse(strs[2]),
            maxMinute = int.Parse(strs[3])
        };
        return gameTimeKey;
    }

    public static explicit operator GameTimeKey(int2 value)
    {
        GameTimeKey gameTimeKey = new GameTimeKey
        {
            minHour = value.x,
            minMinute = value.y,
            maxHour = value.x,
            maxMinute = value.y
        };
        return gameTimeKey;
    }

    public override string ToString()
    {
        return $"{minHour},{minMinute},{maxHour},{maxMinute}";
    }

    public GameTimeKey(int[] timeArray)
    {
        this.minHour = timeArray[0];
        this.minMinute = timeArray[1];
        this.maxHour = timeArray[2];
        this.maxMinute = timeArray[3];
    }

    public GameTimeKey(int minHour, int minMinute, int maxHour, int maxMinute)
    {
        this.minHour = minHour;
        this.minMinute = minMinute;
        this.maxHour = maxHour;
        this.maxMinute = maxMinute;
    }

    public override bool Equals(object obj)
    {
        if (obj is GameTimeKey timeKey)
        {
            if (timeKey.maxHour == timeKey.minHour && timeKey.maxMinute == timeKey.minMinute)
            {
                if (timeKey.maxHour > minHour)
                {
                    if (timeKey.maxHour < maxHour)
                    {
                        return true;
                    }
                    else if (timeKey.maxMinute < maxMinute)
                    {
                        return true;
                    }
                }
                else if (timeKey.maxHour == minHour && timeKey.maxMinute >= minMinute)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return timeKey.minHour == minHour && timeKey.minMinute == minMinute && timeKey.maxHour == maxHour && timeKey.maxMinute == maxMinute;
            }
        }
        else if (obj is int2 time)
        {
            if (time.x > minHour)
            {
                if (time.x < maxHour)
                {
                    return true;
                }
                else if (time.y < maxMinute)
                {
                    return true;
                }
            }
            else if (time.x == minHour && time.y >= minMinute)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public static bool operator ==(GameTimeKey gameTimeKey, int2 timeKey)
    {
        if (timeKey.x > gameTimeKey.minHour)
        {
            if (timeKey.x < gameTimeKey.maxHour)
            {
                return true;
            }
            else if (timeKey.y < gameTimeKey.maxMinute)
            {
                return true;
            }
        }
        else if (timeKey.x == gameTimeKey.minHour && timeKey.y >= gameTimeKey.minMinute)
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(GameTimeKey gameTimeKey, int2 timeKey)
    {
        if (timeKey.x > gameTimeKey.minHour)
        {
            if (timeKey.x < gameTimeKey.maxHour)
            {
                return false;
            }
            else if (timeKey.y < gameTimeKey.maxMinute)
            {
                return false;
            }
        }
        else if (timeKey.x == gameTimeKey.minHour && timeKey.y >= gameTimeKey.minMinute)
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
        if (time.x > minHour)
        {
            if (time.x < maxHour)
            {
                return true;
            }
            else if (time.y < maxMinute)
            {
                return true;
            }
        }
        else if (time.x == minHour && time.y >= minMinute)
        {
            return true;
        }
        return false;
    }

    public bool Equals(GameTimeKey timeKey)
    {
        if (timeKey.maxHour == timeKey.minHour && timeKey.maxMinute == timeKey.minMinute)
        {
            if (timeKey.maxHour > minHour)
            {
                if (timeKey.maxHour < maxHour)
                {
                    return true;
                }
                else if (timeKey.maxMinute < maxMinute)
                {
                    return true;
                }
            }
            else if (timeKey.maxHour == minHour && timeKey.maxMinute >= minMinute)
            {
                return true;
            }
            return false;
        }
        else
        {
            return timeKey.minHour == minHour && timeKey.minMinute == minMinute && timeKey.maxHour == maxHour && timeKey.maxMinute == maxMinute;
        }
    }
}