using Unity.Mathematics;
using UnityEngine;
using BehaviorDesigner.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CharacterRuntimeObj:MonoBehaviour,IGameData
{
    public RuntimeObj runtimeObj;
    public Animator animator; 
    public MyShadowPolygon myShadow;
    public SpriteRenderer equipRenderer;
    public BehaviorTree behaviorTree;
    public void Clear()
    {
        if (runtimeObj != null)
        {  
            behaviorTree.DisableBehavior();
            behaviorTree.enabled = false;
            behaviorTree.ExternalBehavior = null;
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        }
    }

    public string GetKey()
    {
        return gameObject.name;
    }

    public void SetAnimationDirection(float2 direction)
    {
        if (direction.x == float.NaN || direction.y == float.NaN)
        {
            return;
        }
        if (animator != null)
        {
            if (direction.x == 0 && direction.y == 0)
            {
                return;
            }
            animator.SetFloat(CharacterAnimatorParameter.Dir_X, direction.x);
            animator.SetFloat(CharacterAnimatorParameter.Dir_Y, direction.y);
        }
    }

    public void SetAnimationSpeed(float speed, float animationSpeed = 1)
    {
        if (animator != null)
        {
            animator.SetFloat(CharacterAnimatorParameter.Speed, speed);
            if (speed > 0)
            {
                animator.speed = animationSpeed;
            }
        }
    }

#if UNITY_EDITOR
    public void SetReferenceData()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        myShadow = transform.Find("Shadow").GetComponent<MyShadowPolygon>();
        equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
        behaviorTree = transform.GetComponent<BehaviorTree>();
    }

#endif

}
 