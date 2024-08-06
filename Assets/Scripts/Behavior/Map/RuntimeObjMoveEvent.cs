using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Map")]
[TaskName("物体运动表现")]
public class RuntimeObjMoveEvent : Action
{
    public SharedInt objId;
    public EntityType entityType;

    public bool coordinatePos;
    public bool offsetMiddle;

    public SharedVector2 startPos, targetPos,middlePos;
    public float costTime;

    TaskStatus taskStatus = TaskStatus.Success;

    Transform runtimeObjTransform;
    // Start is called before the first frame update
    public override void OnStart()
    {
        if (objId==null|| objId.IsNull())
        {
            taskStatus = TaskStatus.Failure;
            return;
        }
         
        switch (entityType)
        {
            case EntityType.地图道具:
                if (WorldMapObjManager.instance.GetRuntimeMapItemObj(objId.Value, out var runtimeObj))
                {
                    runtimeObjTransform = runtimeObj.transform;
                    taskStatus = TaskStatus.Running;
                }
                else
                {
                    taskStatus = TaskStatus.Failure;
                }
               
                break;
            case EntityType.角色:
                
                if (CharacterManager.instance.GetRuntimeCharacterObj(objId.Value, out CharacterRuntimeObj characterRuntimeObj))
                {
                    runtimeObjTransform = characterRuntimeObj.transform;
                    taskStatus = TaskStatus.Running;
                }
                else
                {
                    taskStatus = TaskStatus.Failure;
                }
               // runtimeObj = characterRuntimeObj.runtimeObj;
                break;
        }
        if(taskStatus== TaskStatus.Running)
        {
            Vector2 startPos = this.startPos.Value;
            Vector2 targetPos = this.targetPos.Value;
            Vector2 middlePos = this.middlePos.Value;

            if (coordinatePos)
            {
                startPos = GameCommon.GetMapPos(startPos);
                targetPos = GameCommon.GetMapPos(targetPos);
                middlePos = GameCommon.GetMapPos(middlePos);
                if (offsetMiddle)
                {
                    middlePos = startPos + (targetPos - startPos) * 0.5f + middlePos;
                }

                float speed = 1.0f / costTime;

                GameObjectCurveController.instance.Curve(speed, startPos, targetPos, middlePos, SetObjPos, MoveEnd);
            }
        }
    }
    void MoveEnd()
    {
        taskStatus = TaskStatus.Success;
    }
    void SetObjPos(Vector2 pos)
    {
        if (transform != null && transform.gameObject.activeSelf)
        { 
            if(transform!= null)
            {
                float z = transform.position.z;
                transform.position = new Vector3(pos.x, pos.y, z);
            }
            
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }

    // Update is called once per frame
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
