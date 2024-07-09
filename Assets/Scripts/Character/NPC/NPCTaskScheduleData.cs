using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Unity.Mathematics; 
using UnityEngine; 
[CreateAssetMenu(menuName = "Data/Npc任务计划表数据")]
public class NPCTaskScheduleData : ScriptableObject, IGameData
{
    public int id;
    public string taskName;
    public NPCTaskScheduleType type;
    public int[] gameTimeRange;
    private string behaviorName;
    public ExternalBehaviorTree externalBehavior;
    public bool loopBehavior;
    
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}
public enum NPCTaskScheduleType
{
    时间, 事件,
}
public struct GameTimeKey 
{
    public int minHour,minMinute,maxHour,maxMinute;
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
        if(obj is GameTimeKey timeKey)
        {
            return timeKey.minHour == minHour && timeKey.minMinute == minMinute && timeKey.maxHour == maxHour && timeKey.maxMinute == maxMinute;
        }else if(obj is int2 time)
        {
            return time.x<=maxHour && time.x>=minHour && time.y<=maxMinute && time.y>=minMinute;
        }
        return false;
    }
    public static bool operator ==(GameTimeKey gameTimeKey,int2 timeKey)
    {
        return timeKey.x>= gameTimeKey.minHour && timeKey.x >= gameTimeKey.minMinute &&
            timeKey.y <= gameTimeKey.maxHour && timeKey.y<= gameTimeKey.maxMinute;
    }
    public static bool operator !=(GameTimeKey gameTimeKey, int2 timeKey)
    {
        return !(timeKey.x >= gameTimeKey.minHour && timeKey.x >= gameTimeKey.minMinute &&
            timeKey.y <= gameTimeKey.maxHour && timeKey.y <= gameTimeKey.maxMinute);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

}