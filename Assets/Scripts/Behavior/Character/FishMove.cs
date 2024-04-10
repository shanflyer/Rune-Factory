using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics; 
using UnityEngine; 

[TaskCategory("Game/Character")]
[TaskName("鱼移动")]
public class FishMove : Action
{
    [SerializeField]
    public SharedInt speed;
    [SerializeField]
    private SharedInt fishId;
    public SharedInt3 target;
    public SharedInt2 nowCell;
    public SharedInt2List cells; 

    // Use this for initialization
    [SerializeField]
    private TaskStatus taskStatus;

    Transform fishTransform;
    Animator fishAnimator;
    public override void OnAwake()
    {
        base.OnAwake();
       
        
    }
     
     

    public override void OnStart()
    {
        if (fishTransform == null)
        {
            fishTransform = FishController.instance.GetFishTransform(fishId.Value, out fishAnimator);
        }
        
        taskStatus = TaskStatus.Running;
        if (fishTransform == null)
        {
            taskStatus = TaskStatus.Failure;
        }
        else
        {
            var pathCells = MapCellController.instance.FindSamplePathNode(nowCell.Value, target.Value.xy, target.Value.z, cells.Value);
            if (pathCells.Count == 0)
            {
                taskStatus = TaskStatus.Failure;
            }
            else
            {
                MoveToTarget(pathCells);
            }
        }
            
    }

    IEnumerator LineMove(Vector2 P0,Vector2 P1,Transform transform,float speed,System.Action endAction, System.Action breakAction)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            
            if(transform.gameObject.activeSelf == false)
            {
                breakAction();
                break;
            }
            else
            {
                var postion = Vector2.Lerp(P0, P1, t);
                transform.position = new Vector3(postion.x, postion.y, 150);
            }
            yield return null;
        }
        endAction();
    }

    void MoveToTarget(Stack<int2> pathNodes)
    {
        var targetCoordinate = pathNodes.Pop();
        
        Vector2 targetPos = GameCommon.GetMapPos(targetCoordinate);
        float2 directionValue= GameCommon.InitMoveDirect(targetCoordinate - nowCell.Value);
        fishAnimator.SetFloat(CharacterAnimatorParameter.Dir_X, directionValue.x);
        fishAnimator.SetFloat(CharacterAnimatorParameter.Dir_Y, directionValue.y);
        nowCell.SetValue(targetCoordinate);
        StartCoroutine(LineMove(fishTransform.position, targetPos, fishTransform,speed.Value,
        () =>
        {
            if(pathNodes.Count > 0)
            {
                MoveToTarget(pathNodes);
            }
            else
            {
                taskStatus = TaskStatus.Success;
            }
        },() =>
        {
            taskStatus = TaskStatus.Failure;
        })); 
    }
  

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}