using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using static UnityEngine.ParticleSystem;

#if UNITY_EDITOR
#endif

public delegate void SetFootStepAction(AudioClip audioClip, Color color);

public class CharacterRuntimeObj : MonoBehaviour, IGameData
{
    public RuntimeObj runtimeObj
    {
        get
        {
            return _runtimeObj;
        }
        set
        {
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
    private Transform body, equip, shadow;

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
    private Transform DirOther;

    [SerializeField]
    private ParticleSystem footStep;

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
            body.localScale = Vector3.zero;
            equip.Translate(offset);
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

    public void SetFootStepON(bool on)
    {
        var emission = footStep.emission;
        emission.enabled = on;
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

        body.localScale = Vector3.one;
        Vector3 offset = equip.localPosition;
        offset.z = 0;
        equip.localPosition = offset;
        offset = shadow.localPosition;
        offset.z = 0;
        shadow.localPosition = offset; 

        /*if(runtimeObj!=null)
            EnvironmentManger.instance.AddCharacterGetFootStep(runtimeObj.linkId, characterGetFootStep);*/
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
        if (isDisplayFootStep && footStep && speed > 0)
        {
            if (waitFootTime <= 0)
            {
                float angel = GameCommon.VectorAngle(Vector2.up, moveDirection);
                //
                EmitParams ep = new EmitParams();
                ep.startColor = footStepColor;
                Vector3 offSetPos = isLeftFoot ? leftFootPos : rightFootPos;
                offSetPos.x *= moveDirection.y;
                offSetPos.y *= -moveDirection.x;

                ep.position = transform.position + offSetPos;
                ep.startSize = isLeftFoot ? footStep.main.startSize.constant : -footStep.main.startSize.constant;
                ep.rotation = 180 - angel;
                footStep.Emit(ep, 1);
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
        body = transform.Find("Body");
        equip = transform.Find("Equip");
        shadow = transform.Find("Shadow");
        myShadow = shadow.GetComponent<MyShadowPolygon>();
        equipRenderer = transform.GetChild(1).GetChild(1).GetComponent<MySpriteMeshRender>();
        behaviorTree = transform.GetComponent<BehaviorTree>();
        DirOther = transform.Find("Other/Dir");
        if (DirOther)
        {
            var footStepTrans = DirOther.Find("脚印");
            if (footStepTrans)
                footStep = footStepTrans.GetComponent<ParticleSystem>();
        }
    }

#endif
}