using Unity.Mathematics;
using UnityEngine;
using BehaviorDesigner.Runtime;
using static UnityEngine.ParticleSystem;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif
public delegate void SetFootStepAction(AudioClip audioClip,Color color);
public class CharacterRuntimeObj:MonoBehaviour,IGameData
{
    public RuntimeObj runtimeObj;
    [SerializeField]
    private Transform body,equip,shadow; 
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
    private AudioSource audioSource;
    [SerializeField]
    bool isDisplayFootStep;
    [SerializeField]
    Vector3 leftFootPos, rightFootPos;
    [SerializeField]
    float FootTime;
    [SerializeField]
    float2 direction;

    public Collider2D collider => myShadow.PolygonCollider;
    public void Clear()
    {
        if (runtimeObj != null)
        {  
            behaviorTree.DisableBehavior();
            behaviorTree.enabled = false;
            behaviorTree.ExternalBehavior = null;

            this.enabled = false;
            Vector3 offset = new Vector3(0, 0, -99999);
            body.localScale = Vector3.zero;
            equip.Translate(offset);
            shadow.Translate(offset);

            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj,false);
        }
    }
    public void SetAnimationFloat(int hashParameter,float value)
    {
        if (animator)
        {
            animator.SetFloat(hashParameter, value);
            if(hashParameter== CharacterAnimatorParameter.Speed)
            {
                this.speed = value;
            }
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
    [SerializeField]
    Vector2 moveDirection;
    [SerializeField]
    float speed; 
     
    public void SetAnimationDirection(float2 moveDirection, Direction direction)
    {
        this.moveDirection = moveDirection;
        this.direction = GameCommon.GetDirectValue(direction);
        if (this.direction.x == float.NaN || this.direction.y == float.NaN)
        {
            return;
        }
        if (animator != null)
        {
            if (this.direction.x == 0 && this.direction.y == 0)
            {
                return;
            }
            animator.SetFloat(CharacterAnimatorParameter.Dir_X, this.direction.x);
            animator.SetFloat(CharacterAnimatorParameter.Dir_Y, this.direction.y);
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


        body.localScale = Vector3.one; 
        Vector3 offset = equip.localPosition;
        offset.z = 0;
        equip.localPosition = offset;
        offset = shadow.localPosition;
        offset.z = 0;
        shadow.localPosition = offset;  
    }
    PlayableGraph playableGraph;
    AudioPlayableOutput playableOutput;
    AudioClipPlayable audioClipPlayable;
     void Awake()
    {
        playableGraph = PlayableGraph.Create($"{gameObject.name}-FootStep");
        playableOutput = AudioPlayableOutput.Create(playableGraph, $"{gameObject.name}-FootStep", audioSource);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        if (gameObject.activeSelf)
        {
            EnvironmentManger.instance.AddCharacterGetFootStep(
                new CharacterGetFootStep
                {
                    pos = pos,
                    SetFootStepAction = SetFootStepAction
                }
                );
        }
    }
    AudioClip stepAudioClip;
    Color footStepColor;
    void SetFootStepAction(AudioClip audioClip,Color color)
    {
        footStepColor = color;
        if (stepAudioClip != audioClip)
        { 
            audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, false);
            stepAudioClip = audioClip;
        }
    }
   
    void PlayFootStep()
    {
        if (!audioClipPlayable.IsNull())
        {
            playableOutput.SetSourcePlayable(audioClipPlayable);
            playableGraph.Play();
        } 
    } 
    void Update()
    {
        if (isDisplayFootStep&& footStep&& speed>0)
        {
            if (waitFootTime <= 0)
            {
                float angel = GameCommon.VectorAngle(Vector2.up, moveDirection);
                //
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

                PlayFootStep();


            }
            waitFootTime -= Time.deltaTime;
        }
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        body = transform.Find("Body");
        equip = transform.Find("Equip");
        shadow = transform.Find("Shadow");
        myShadow = shadow.GetComponent<MyShadowPolygon>();
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
 