using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
#if UNITY_EDITOR
#endif

public delegate void SetFootStepAction(AudioClip audioClip, Color color);

public class CharacterRuntimeObj : MonoBehaviour, IGameData
{
    public new Transform transform
    {
        get
        {
            return base.transform;
        } 
    }
   
    public RuntimeObj runtimeObj
    {
        get
        {
            return _runtimeObj;
        }
        set
        {
            if (value != null)
            {
                value.dispose = Dispose;
            }
            /*if (value != null)
            {
                EnvironmentManger.instance.AddCharacterGetFootStep(value.linkId, characterGetFootStep);
            }
            else
            {
                EnvironmentManger.instance.RemoveCharacterGetFootStep(runtimeObj.linkId);
            }*/
            _runtimeObj = value;
        }
    }

    private RuntimeObj _runtimeObj;

    [SerializeField]
    private Transform  equip, shadow;

    public Animator Animator => animator;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private MyShadowPolygon myShadow;

    [SerializeField]
    private MySpriteMeshRender equipRenderer;

    [SerializeField]
    private BehaviorTree behaviorTree;


    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private bool isDisplayFootStep;

    [SerializeField]
    private Vector3 leftFootPos, rightFootPos;

    [SerializeField]
    private float FootTime;

    [SerializeField]
    private float2 direction;

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
            if (equip)
                equip.Translate(offset);
            if (shadow)
                shadow.Translate(offset);

            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj, true);
        }
    }

    public void SetAnimationFloat(int hashParameter, float value)
    {
        if (animator)
        {
            animator.SetFloat(hashParameter, value);
            if (hashParameter == CharacterAnimatorParameter.Speed)
            {
                this.speed = value;
            }
        }
    }

    public void SetEquipSprite(Sprite sprite)
    {
        equipRenderer.m_Sprite = sprite;
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



    [SerializeField]
    private Vector2 moveDirection;

    [SerializeField]
    private float speed;

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
                //animator.SetBool(CharacterAnimatorParameter.Set, false);
               // animator.SetBool(CharacterAnimatorParameter.Fish,false);
            }
        }
        this.speed = animationSpeed;
    }

    private float waitFootTime = 0;
    private bool isLeftFoot;

    private void OnEnable()
    {
        waitFootTime = 0;
        isLeftFoot = false;

        if (equip)
        {
            Vector3 offset = equip.localPosition;
            offset.z = 0;
            equip.localPosition = offset;
        }
        if (shadow)
        {
            Vector3 offset = shadow.localPosition;
            offset.z = 0;
            shadow.localPosition = offset;
        }
       

        /*if(runtimeObj!=null)
            EnvironmentManger.instance.AddCharacterGetFootStep(runtimeObj.linkId, characterGetFootStep);*/
    }
    public void Dispose()
    {
        if (singlePlayableGraph.IsValid())
        {
            singlePlayableGraph.Destroy();
        } 
    }
    private void OnDisable()
    {
        /*iif (runtimeObj != null&&!SingletonType.Cleared)
            EnvironmentManger.instance.RemoveCharacterGetFootStep(runtimeObj.linkId); */
    }

    private AudioPlayableOutput audioPlayableOutput;
    private AudioMixerPlayable audioMixerPlayable, leftMixerPlayable, rightMixerPlayable;
    private PlayableGraph singlePlayableGraph;

    private void Awake()
    {
        gameObject.TryGetComponent(out audioSource);
        characterGetFootStep = new CharacterGetFootStep
        {
            transform = transform,
            SetFootStepAction = SetFootStepAction
        };

        singlePlayableGraph = PlayableGraph.Create($"{gameObject.name}-FootStep");
        audioPlayableOutput = AudioPlayableOutput.Create(singlePlayableGraph, $"{gameObject.name}_Footstep", audioSource);
        audioMixerPlayable = AudioMixerPlayable.Create(singlePlayableGraph);
        leftMixerPlayable = AudioMixerPlayable.Create(singlePlayableGraph);
        audioMixerPlayable.AddInput(leftMixerPlayable, 0, 1);
        rightMixerPlayable = AudioMixerPlayable.Create(singlePlayableGraph);
        audioMixerPlayable.AddInput(rightMixerPlayable, 0, 1);

        audioPlayableOutput.SetSourcePlayable(audioMixerPlayable);

        singlePlayableGraph.Play();
    }

    public void SetPosition(Vector3 pos)
    {
         
        transform.position = pos;
    }

    public void SetCoordinateAction(int2 coordinate, int mapInstance, int defaultGround)
    {
        EnvironmentManger.instance.GetNowFootStepData(characterGetFootStep, coordinate, mapInstance, defaultGround);
    }

    private CharacterGetFootStep characterGetFootStep;

    private AudioClip stepAudioClip;
    private Color footStepColor;
    private Dictionary<AudioClip, int> audioClipIndex = new Dictionary<AudioClip, int>();

    private void SetFootStepAction(AudioClip audioClip, Color color)
    {
        footStepColor = color;
        if (stepAudioClip != audioClip)
        {
            stepAudioClip = audioClip;
            if (!audioClipIndex.ContainsKey(audioClip))
            {
                int inputCount = leftMixerPlayable.GetInputCount();
                var leftAudioClipPlayable = AudioClipPlayable.Create(singlePlayableGraph, stepAudioClip, false);
                leftMixerPlayable.AddInput(leftAudioClipPlayable, 0, 0);

                var rightAudioClipPlayable = AudioClipPlayable.Create(singlePlayableGraph, stepAudioClip, false);
                rightMixerPlayable.AddInput(rightAudioClipPlayable, 0, 0);
                audioClipIndex[audioClip] = inputCount;
            }
        }
    }
     
    private void PlayFootStep(bool isLeft)
    {
        if (stepAudioClip == null)
        {
            return;
        }
        //Debug.Log($"播放:{stepAudioClip.name}");
        if (audioClipIndex.TryGetValue(stepAudioClip, out var index))
        {
            if (isLeft)
            {
                leftMixerPlayable.SetInputWeight(index, 1);
                var audioClipPlayable = leftMixerPlayable.GetInput(index);
                audioClipPlayable.SetTime(0);
            }
            else
            {
                rightMixerPlayable.SetInputWeight(index, 1);
                var audioClipPlayable = rightMixerPlayable.GetInput(index);
                audioClipPlayable.SetTime(0);
            }
        }
    }
     
    private void LateUpdate()
    { 
        if (isDisplayFootStep && speed > 0)
        {
            if (waitFootTime <= 0)
            {
                float angle = GameCommon.VectorAngle(Vector2.up, moveDirection);
                //
              
                Vector3 offSetPos = isLeftFoot ? leftFootPos : rightFootPos;
                offSetPos.x *= moveDirection.y;
                offSetPos.y *= -moveDirection.x; 
                var position = transform.position + offSetPos;

                GameVolumeManager.instance.EmitFootParticle(angle, position, footStepColor, isLeftFoot);
                waitFootTime = FootTime;
                PlayFootStep(isLeftFoot);
                isLeftFoot = !isLeftFoot;
            }
            waitFootTime -= Time.deltaTime;
        }
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        equip = transform.Find("Equip");
        shadow = transform.Find("Shadow");
        myShadow = shadow.GetComponent<MyShadowPolygon>();
        equipRenderer = equip.GetChild(1).GetComponent<MySpriteMeshRender>(); 
        behaviorTree = transform.GetComponent<BehaviorTree>();
      
    }

#endif
}