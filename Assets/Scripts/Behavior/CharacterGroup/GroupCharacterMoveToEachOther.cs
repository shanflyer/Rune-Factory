using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks; 
using Unity.Mathematics; 

[TaskCategory("Game/CharacterGroup")]
[TaskName("组团角色彼此靠近")]
public class GroupCharacterMoveToEachOther : Action
{
    [SerializeField]
    private SharedIntList characterIds;
    [SerializeField]
    private int distance;
    [SerializeField]
    private SharedInt multiNPCGroupId;
    [SerializeField]
    private float waitDuration=1.0f;
    MyDic<int, Character> characters = new MyDic<int, Character>();
    private float startTime;
    // Remember the time that the task is paused so the time paused doesn't contribute to the wait time.
    private float pauseTime;
    public override void OnStart()
    {
        int2 target = GetGroupCenter();

        for(int i = 0; i < characters.length; i++)
        {
            var character = characters[i]; character.StopMove();
            if (NPCTaskScheduleManager.instance.GetNPCHoldPos(character.instanceId))
            {
               
               
            }
            else
            {
                if (!character.TryMove(target))
                {

                }
            }
            
        }
        startTime = Time.time;
    }
    TaskStatus taskStatus;
    int2 oldCenter = new int2(int.MinValue,int.MinValue);
    int2 nowCenter;
    int2 GetGroupCenter()
    {
        int maxX = int.MinValue;
        int maxY = int.MinValue;
        int minX = int.MaxValue; int minY = int.MaxValue;
         
        for (int i = 0; i < characters.length; i++)
        { 
            Character character = characters[i];
            if (character != null)
            { 
                if (maxX < character.coordinate.x)
                    maxX = character.coordinate.x;
                if (maxY < character.coordinate.y)
                    maxY = character.coordinate.y;
                if (minX > character.coordinate.x)
                    minX = character.coordinate.x;
                if (minY < character.coordinate.y)
                    minY = character.coordinate.y;
                 
            }
        }
        int2 center = new int2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
        return center;
    }

    void CheckCharacterMoveResult()
    {
        var center = GetGroupCenter();
        nowCenter = center;
        for (int i = 0; i < characters.length; i++)
        {
            var distance = math.abs(characters[i].coordinate.x - center.x) + 
                math.abs(characters[i].coordinate.y - center.y);
            if (distance > this.distance)
            {
                taskStatus = TaskStatus.Running;
                return;
            }
        }
        taskStatus = TaskStatus.Success;
    }
    
    void UpDataCharacter()
    {
        var keys = characters.GetKeyList();
        for(int i = 0; i < keys.Count; i++)
        { 
            if (!characterIds.Value.Contains(keys[i]))
            { 
                LeaveMultiNPCBehaviorGroup leaveMultiNPCBehaviorGroup = new LeaveMultiNPCBehaviorGroup
                {
                    characterId = keys[i],
                    groupId = multiNPCGroupId.Value
                };
                GameActionManager.instance.QueueAction(leaveMultiNPCBehaviorGroup, true);
                characters.Remove(keys[i]);
            }
        }
        for(int i = 0; i < characterIds.Value.Count; i++)
        {
            int characterId = characterIds.Value[i];
            if (!characters.ContainsKey(characterId))
            {
                JoinInMultiNPCBehaviorGroup joinInMultiNPCBehaviorGroup = new JoinInMultiNPCBehaviorGroup
                {
                    characterId = characterId,
                    groupId = multiNPCGroupId.Value
                };
                GameActionManager.instance.QueueAction(joinInMultiNPCBehaviorGroup,true);
                Character character = CharacterManager.instance.GetCharacter(characterId);
                characters.Add(character.instanceId,character);
            }
        }
    }

    void CharacterMoveToCenter()
    {
        if (!oldCenter.Equals(nowCenter))
        {
            for(int i = 0; i < characters.length; i++)
            {
                var character = characters[i];
                if(NPCTaskScheduleManager.instance.GetNPCHoldPos(character.instanceId))
                {
                    character.RemoveMove(); 
                }
                else
                {
                    character.RemoveMove();
                    character.TryMove(nowCenter);
                } 
            }
            oldCenter = nowCenter;
        }
        startTime = Time.time;
    }
    public override void OnPause(bool paused)
    {
        if (paused)
        {
            // Remember the time that the behavior was paused.
            pauseTime = Time.time;
        }
        else
        {
            // Add the difference between Time.time and pauseTime to figure out a new start time.
            startTime += (Time.time - pauseTime);
        }
    }
    public override TaskStatus OnUpdate()
    {
        UpDataCharacter();
        CheckCharacterMoveResult();
        if(taskStatus==TaskStatus.Running&&
            startTime + waitDuration < Time.time)
        {
            CharacterMoveToCenter();
        }
        return taskStatus;
    }
}