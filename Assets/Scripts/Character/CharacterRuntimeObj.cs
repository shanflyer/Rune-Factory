using Unity.Mathematics;
using UnityEngine;
using BehaviorDesigner.Runtime;
using static UnityEngine.ParticleSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CharacterRuntimeObj:MonoBehaviour,IGameData
{
    public RuntimeObj runtimeObj;
    public Animator Animator => animator;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private MyShadowPolygon myShadow;
    [SerializeField]
    private SpriteRenderer equipRenderer;
    [SerializeField]
    private BehaviorTree behaviorTree;
    [SerializeField]
    private Transform DirOther;
    [SerializeField]
    private ParticleSystem footStep;
    [SerializeField]
    bool isDisplayFootStep;
    [SerializeField]
    Vector3 leftFootPos, rightFootPos;
    [SerializeField]
    float FootTime;
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
    public void SetAnimationFloat(int hashParameter,float value)
    {
        if (animator)
        {
            animator.SetFloat(hashParameter, value);
        }
    }
    public void SetEquipSprite(Sprite sprite)
    {
        equipRenderer.sprite = sprite;
        equipRenderer.enabled = sprite != null;
    }
    public void SetEnableBehavior(int instanceId, ExternalBehaviorTree externalBehaviorTree)
    {
        behaviorTree.ExternalBehavior = externalBehaviorTree;
        behaviorTree.enabled = true;
        behaviorTree.SetVariable("CharacterId", new SharedInt { Value = instanceId });
        behaviorTree.EnableBehavior();
    }
    public string GetKey()
    {
        return gameObject.name;
    }
    public void SetFootStepON(bool on)
    {
        var emission= footStep.emission;
        emission.enabled = on;
    }
    Vector2 moveDirection;
    public void SetMoveDirection(float2 direction)
    {
        moveDirection = direction;
    }
    float speed; 
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
        this.speed = animationSpeed;
    }

    float waitFootTime = 0;
    bool isLeftFoot;
    void OnEnable()
    {
        waitFootTime = 0;
        isLeftFoot = false;
    }
     
    void Update()
    {
        if (isDisplayFootStep&& footStep&& speed>0)
        {
            if (waitFootTime <= 0)
            {
                float angel = GameCommon.VectorAngle(Vector2.up, moveDirection);

                EmitParams ep = new EmitParams();
                Vector3 offSetPos = isLeftFoot ? leftFootPos :rightFootPos;
                offSetPos.x *= moveDirection.y;
                offSetPos.y *= -moveDirection.x;

                ep.position = transform.position + offSetPos;
                ep.startSize = isLeftFoot ? footStep.main.startSize.constant:-footStep.main.startSize.constant;
                ep.rotation =180- angel;
                footStep.Emit(ep, 1);
                waitFootTime = FootTime;
                isLeftFoot=!isLeftFoot;
            }
            waitFootTime -= Time.deltaTime;
        }
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        myShadow = transform.Find("Shadow").GetComponent<MyShadowPolygon>();
        equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
        behaviorTree = transform.GetComponent<BehaviorTree>();
        DirOther = transform.Find("Other/Dir");
        if (DirOther)
        {
            var footStepTrans = DirOther.Find("脚印");
            if(footStepTrans)
                footStep = footStepTrans.GetComponent<ParticleSystem>();
        }
    }

#endif

}
 