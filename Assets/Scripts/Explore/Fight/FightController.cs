using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;

public struct FightPlayerRuntime
{ 
    public RuntimeObj playerObj;
    public PlayableDirector playableDirector;
    public Animator animator;
    public BehaviorTree behaviorTree;
}
public class FightController : MonoBehaviour
{
    public static FightController instance;
    [SerializeField]
    private BehaviorTree controllerBehavior;
    [SerializeField]
    private BehaviorTree manualFightBehavior;
      
    RuntimeObj fightMapRuntime0, fightMapRuntime1;
    [SerializeField]
    List<Transform> playerPos=new List<Transform>();
    [SerializeField]
    List<Transform> monsterPos=new List<Transform>();

    Dictionary<int,FightPlayerRuntime> fightPlayerRuntimes=new Dictionary<int, FightPlayerRuntime>();
    Dictionary<int, FightPlayerRuntime> fightMonsterRuntimes = new Dictionary<int, FightPlayerRuntime>();

     
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        GameRuntimeObjManager.instance.CreatParent<FightRuntimeObjType>(transform);
        GameActionManager.instance.AddListener<HideFightScene>(HideFightScene);
        GameActionManager.instance.AddListener<DisplayFightScene>(DisplayFightScene);
        GameActionManager.instance.AddListener<StartRoundFight>(EndFightRound);
        GameActionManager.instance.AddListener<DisplayHurt>(DisplayHurt);
        GameActionManager.instance.AddListener<ExploreEnd>(ExploreEnd);
        GameActionManager.instance.AddListener<SetFightCharacterAnimator>(SetFightCharacterAnimator);
        GameActionManager.instance.AddListener<PlayerFight>(PlayerFight);
        GameActionManager.instance.AddListener<CreatFightPlayer>(CreatFightPlayer);
        GameActionManager.instance.AddListener<SetAutoExplore>(SetAutoExplore);
        GameActionManager.instance.AddListener<SwitchAutoExplore>(SwitchAutoExplore);
        GameActionManager.instance.AddListener<TryStartAutoBehavior>(TryStartAutoBehavior);
        GameActionManager.instance.AddListener<TryStartAutoExplore>(TryStartAutoExplore);
        GameActionManager.instance.AddListener<FightCharacterMove>(FightCharacterMove);

        var behaviorTrees = GetComponents<BehaviorTree>();
        for(int i = 0; i < behaviorTrees.Length; i++)
        {
            var behaviorTree = behaviorTrees[i];
            if (behaviorTree.BehaviorName == "回合制战斗")
            {
                controllerBehavior = behaviorTree;
            }
            else
            {
                manualFightBehavior=behaviorTree;
            }
        }
       // controllerBehavior = GetComponent<BehaviorTree>(); 
        var sceneInfoManager = SceneInfoManager.instance;
    }
    void FightCharacterMove(FightCharacterMove fightCharacterMove)
    {
        if(fightPlayerRuntimes.TryGetValue(fightCharacterMove.characterId,out var fightPlayerRuntime))
        {
            Vector3 targetPos = playerPos[fightCharacterMove.newIndex].position;
            StartCoroutine(FightCharacterMoving(fightPlayerRuntime.animator.transform, targetPos));
        }
    }
    void TryStartAutoBehavior(TryStartAutoBehavior tryStartAutoBehavior)
    {
        if (autoFight)
        {
            controllerBehavior.EnableBehavior();
        }
    }
    void TryStartAutoExplore(TryStartAutoExplore tryStartAutoExplore)
    {
        if (autoExplore)
        {
            StartWalk();
            //controllerBehavior.EnableBehavior();
        }
    }


    void SwitchAutoExplore(SwitchAutoExplore switchAutoExplore)
    {
        if (switchAutoExplore.explore)
        {
            autoExplore = !autoExplore;
            autoFight = autoExplore;
            if (switchAutoExplore.setResult!=null)
            {
                switchAutoExplore.setResult(autoExplore);
            }
            if (autoExplore)
            {
                StartWalk();
            }
            else
            {
                StopWalk();
            }
        }
        else
        {
            autoFight = !autoFight;
            if (autoFight)
            {
                if (controllerBehavior.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Inactive)
                {
                    controllerBehavior.EnableBehavior();
                }
            }

            if (switchAutoExplore.setResult != null)
            {
                switchAutoExplore.setResult(autoFight);
            }
        }
    }
  
    void SetAutoExplore(SetAutoExplore setAutoExplore)
    {
        autoExplore = setAutoExplore.auto;
        autoFight=setAutoExplore.auto;
        if (autoExplore)
        {
            StartWalk();
        }
        else
        {
            StopWalk();
        }
    }
    void PlayerFight(PlayerFight playerFight)
    {
        controllerBehavior.DisableBehavior();
        manualFightBehavior.EnableBehavior();
        /*
        foreach(var fightPlayer in fightPlayerRuntimes)
        {
            RunFightCharacter(fightPlayer.Key);
        }*/
    }
    void DisplayFightScene(DisplayFightScene displayFightScene)
    {
        GameRuntimeObjManager.instance.SetObjParent(FightRuntimeObjType.FIGHTMAP.ToString(), false);
    }
    void HideFightScene(HideFightScene hideFightScene)
    {
        GameRuntimeObjManager.instance.SetObjParent(FightRuntimeObjType.FIGHTMAP.ToString(), true);
    }
 
    void ExploreEnd(ExploreEnd exploreEnd)
    {
        GameActionManager.instance.RemoveListener<HideFightScene>(HideFightScene);
        GameActionManager.instance.RemoveListener<DisplayFightScene>(DisplayFightScene);
        GameActionManager.instance.RemoveListener<StartRoundFight>(EndFightRound);
        GameActionManager.instance.RemoveListener<DisplayHurt>(DisplayHurt);
        GameActionManager.instance.RemoveListener<ExploreEnd>(ExploreEnd);
        GameActionManager.instance.RemoveListener<SetFightCharacterAnimator>(SetFightCharacterAnimator);
        GameActionManager.instance.RemoveListener<PlayerFight>(PlayerFight);
        GameActionManager.instance.RemoveListener<CreatFightPlayer>(CreatFightPlayer);
        GameActionManager.instance.RemoveListener<SetAutoExplore>(SetAutoExplore);
        GameActionManager.instance.RemoveListener<SwitchAutoExplore>(SwitchAutoExplore);
        GameActionManager.instance.RemoveListener<TryStartAutoBehavior>(TryStartAutoBehavior);
        GameActionManager.instance.RemoveListener<TryStartAutoExplore>(TryStartAutoExplore);
        GameActionManager.instance.RemoveListener<FightCharacterMove>(FightCharacterMove);

         
        UIManager.instance.CloseGamePanel<FightPanel>();
        SceneManager.instance.UnloadNowScene();
    }
     
    public void RemoveFightPlayerRuntime(int characterId)
    {
        if (!fightPlayerRuntimes.TryGetValue(characterId, out var fightPlayerRuntime))
        {
            if (fightMonsterRuntimes.TryGetValue(characterId, out fightPlayerRuntime))
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(fightPlayerRuntime.playerObj);
                fightMonsterRuntimes.Remove(characterId);
            }
        }
        else
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(fightPlayerRuntime.playerObj);
            fightPlayerRuntimes.Remove(characterId);
        }
    }

    /// <summary>
    /// 展示掉落
    /// </summary>
    public async void DisplayDropItem(List<int2> items,int characterId)
    { 
        if(fightMonsterRuntimes.TryGetValue(characterId,out var fightMonsterRuntime))
        {
            Vector2 startPos = fightMonsterRuntime.animator.transform.position;
            float4 dropArea = GameCommon.dropArea;
            Vector2 finalPos = playerPos[0].position;
            using (var e = fightPlayerRuntimes.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    finalPos = e.Current.Value.animator.transform.position;
                }
            }

            List<CurveMoveData> CurveMoveDatas = new List<CurveMoveData>();
            List<CurveMoveData> CurveMoveDatasLine = new List<CurveMoveData>();
            for (int i = 0; i < items.Count; i++)
            {
                int itemId = items[i].x;
                int itemCount = items[i].y;

                var itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId);
                for(int j = 0; j < itemCount; j++)
                {
                    var itemRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.OTHER.ToString(),
                      "dropItem", GameSourceManager.instance.dropItem, itemData.id); 
                    SpriteRenderer spriteRenderer = itemRuntime.obj as SpriteRenderer;
                    spriteRenderer.sprite = itemData.icon;
                    spriteRenderer.transform.position = startPos;
                    spriteRenderer.transform.localScale = Vector3.zero;

                    var targetPos = finalPos+ new Vector2(GameRandom.RandomFloat(dropArea.x, dropArea.z),GameRandom.RandomFloat(dropArea.y, dropArea.w));
                    var middlePos = startPos + (targetPos - startPos) * 0.5f;
                    middlePos.y += Vector2.Distance(startPos, middlePos);

                    float waitTime = GameRandom.RandomFloat(GameCommon.dropWaitTime.x, GameCommon.dropWaitTime.y);
                    float moveTime = Vector2.Distance(startPos, targetPos)/GameCommon.dropItemFlyerSpeed;

                    CurveMoveData curveMoveData = new CurveMoveData
                    {
                        transform = spriteRenderer.transform,
                        waitTime = waitTime,
                        moveTime = moveTime,
                        startPos = startPos,
                        targetPos = targetPos,
                        middlePos = middlePos
                    };
                    CurveMoveDatas.Add(curveMoveData);


                    CurveMoveData curveMoveDataLine = new CurveMoveData
                    {
                        transform = spriteRenderer.transform,
                        waitTime = 0,
                        moveTime = Vector2.Distance(targetPos, finalPos) / GameCommon.dropItemFlyerSpeed,
                        startPos = targetPos,
                        targetPos = finalPos,
                        middlePos = targetPos + (finalPos - targetPos) * 0.5f,
                        CurveEndAction = () => { GameRuntimeObjManager.instance.RecycleRuntimeObj(itemRuntime); }
                    };
                    CurveMoveDatasLine.Add(curveMoveDataLine);
                }
            }

            GameObjectCurveController.instance.CurveList(CurveMoveDatas, () =>
            {
               GameObjectCurveController.instance.LineList(CurveMoveDatasLine,null);
            });
        } 
    }

    void DisplayHurt(DisplayHurt displayHurt)
    {
        FightPlayerRuntime fightPlayerRuntime;
        if (!fightPlayerRuntimes.TryGetValue(displayHurt.targetId, out fightPlayerRuntime))
        {
            if(fightMonsterRuntimes.TryGetValue(displayHurt.targetId,out fightPlayerRuntime))
            {
                Vector3 pos = fightPlayerRuntime.animator.transform.position;
                SceneInfoManager.instance.DisplaySceneInfo(displayHurt.hurtValue.ToString(), pos);
            }
        }else
        {

            Vector3 pos = fightPlayerRuntime.animator.transform.position;
            SceneInfoManager.instance.DisplaySceneInfo(displayHurt.hurtValue.ToString(), pos);
        }
    }

    private void OnApplicationQuit()
    {
        instance = null;
    }

    public bool AutoFight => autoFight;
    bool autoFight = false;

    public bool AutoExplore => autoExplore;
    bool autoExplore = false;
    void EndFightRound(StartRoundFight startRoundFight)
    {  
        if (autoFight)
        {
            if (controllerBehavior.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Inactive)
            {
                controllerBehavior.EnableBehavior();
            }
            else
            {
                // BehaviorManager.instance.RestartBehavior(controllerBehavior);
            }
            EndPlayerRound endPlayerRound = new EndPlayerRound();
            GameActionManager.instance.QueueAction(endPlayerRound);

        }
        else
        {
            controllerBehavior.DisableBehavior();
            StopAutoFight stopAutoFight = new StopAutoFight();
            GameActionManager.instance.QueueAction(stopAutoFight);

        }
       

        Debug.Log("回合结束...."); 
    } 

    private void OnDestroy()
    {
        instance = null;
        GameRuntimeObjManager.instance.ClearRuntime<FightRuntimeObjType>();
    }
    float cycleSize;
    Vector3 cyclePos;
    public void CreatFightMap(FightMapData fightMapData)
    {
        if (fightMapData.fightMapObj != null)
        {
            fightMapRuntime0 = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(),
           fightMapData.id.ToString(), fightMapData.fightMapObj.transform, 0);
            fightMapRuntime1 = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(),
                fightMapData.id.ToString(), fightMapData.fightMapObj.transform, 1);

            Vector3 zeroPos = new Vector3(0, fightMapData.offsetY, 0);
            cyclePos = new Vector3(fightMapData.cycleSize, fightMapData.offsetY, 0);

            cycleSize = fightMapData.cycleSize;
            var transform = fightMapRuntime0.obj as Transform;
            var transform1 = fightMapRuntime1.obj as Transform;
            transform.localPosition = zeroPos;
            transform1.localPosition = cyclePos;

            DisplaySky displaySky = new DisplaySky
            {
                display = fightMapData.skyDisplay
            };
            GameActionManager.instance.QueueAction(displaySky);
        }
        else
        {
            if (fightMapRuntime0.use)
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(fightMapRuntime0);
            }
            if (fightMapRuntime1.use)
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(fightMapRuntime1);
            }
        }
    }

     async void CreatFightPlayer(CreatFightPlayer creatFightPlayer)
    {
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            int index = i;
            int dataId = creatFightPlayer.players[i];

            index = math.clamp(index, 0, 2);

            Character character = CharacterManager.instance.GetCharacterForDataId(dataId);
            CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(dataId);
            if (characterData != null)
            {
                var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYER.ToString(),
                    characterData.objName, characterData.obj.transform, character.instanceId, isActive: false);
                var transform = characterRuntime.obj as Transform;
                transform.position = playerPos[index].position;
                FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime
                {
                    playerObj = characterRuntime,
                    animator = transform.GetComponentInChildren<Animator>(),
                    playableDirector = transform.GetComponentInChildren<PlayableDirector>(),
                    behaviorTree = transform.GetComponent<BehaviorTree>()
                };
                transform.gameObject.SetActive(true);
                var ExternalBehavior = await GameSourceManager.instance.GetBehavior($"{DataPath.BehaviorPath}{characterData.fightBehavior}");
                fightPlayerRuntime.behaviorTree.ExternalBehavior = ExternalBehavior;
                fightPlayerRuntimes[character.instanceId] = fightPlayerRuntime;
                fightPlayerRuntime.behaviorTree.SetVariableValue("fightCharacter", character.instanceId);
            }
        }

            
    }
    /*
    public async void CreatFightPlayer(int dataId,int instanceId,int index)
    {
        index = math.clamp(index, 0, 2);
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(dataId);
        if (characterData != null)
        {
            var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYER.ToString(),
                characterData.objName, characterData.obj.transform, instanceId,isActive:false);
            var transform = characterRuntime.obj as Transform;
            transform.position = playerPos[index].position;
            FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime
            {
                playerObj = characterRuntime,
                animator = transform.GetComponentInChildren<Animator>(),
                playableDirector = transform.GetComponentInChildren<PlayableDirector>(),
                behaviorTree = transform.GetComponent<BehaviorTree>()
            };
            transform.gameObject.SetActive(true);
            var ExternalBehavior = await GameSourceManager.instance.GetBehavior($"{DataPath.BehaviorPath}{characterData.fightBehavior}");
            fightPlayerRuntime.behaviorTree.ExternalBehavior = ExternalBehavior;
            fightPlayerRuntimes[instanceId] = fightPlayerRuntime;
            fightPlayerRuntime.behaviorTree.SetVariableValue("fightCharacter", instanceId);
        }
    }*/
    public async void CreatFightMonster(int dataId, int instanceId, int index)
    {
        MonsterData monsterData = await GameDataManager.instance.GetAsyncData<MonsterData>(dataId);
        CreatFightMonster(monsterData, instanceId, index);
    }
    public async void CreatFightMonster(MonsterData characterData, int instanceId, int index)
    {
        index = math.clamp(index, 0, 5); 
        if (characterData != null)
        {
            var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.MONSTRT.ToString(),
                characterData.monsterName, characterData.obj.transform, instanceId,isActive:false);
            var transform = characterRuntime.obj as Transform;
            transform.position = monsterPos[index].position;
            transform.gameObject.SetActive(true);
            FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime
            {
                playerObj = characterRuntime,
                animator = transform.GetComponentInChildren<Animator>(),
                playableDirector = transform.GetComponentInChildren<PlayableDirector>(),
                behaviorTree= transform.GetComponent<BehaviorTree>()
            };
           
            var ExternalBehavior = await GameSourceManager.instance.GetBehavior($"{DataPath.BehaviorPath}{characterData.behaviorId}");
            fightPlayerRuntime.behaviorTree.ExternalBehavior = ExternalBehavior;
            fightMonsterRuntimes[instanceId] = fightPlayerRuntime;
            fightPlayerRuntime.behaviorTree.SetVariableValue("fightCharacter", instanceId);
        }
    }


    public Animator FindFightCharacter(int id)
    {
        if(fightPlayerRuntimes.TryGetValue(id,out var fightPlayer))
        {
            return fightPlayer.animator;
        }
        if (fightMonsterRuntimes.TryGetValue(id, out var fightMonster))
        {
            return fightMonster.animator;
        }
        return null;
    }
 
    public void RunFightCharacter(int characterId)
    {
        if(fightPlayerRuntimes.TryGetValue(characterId,out var fightPlayer))
        {
            fightPlayer.behaviorTree.EnableBehavior();
        }else if (fightMonsterRuntimes.TryGetValue(characterId, out var fightMonster))
        {
            fightMonster.behaviorTree.EnableBehavior();
        }
    }
    public void StartSkillAction(SkillEstimateData skillEstimateData,int characterId)
    {
        FightPlayerRuntime fightPlayerRuntime;
        if (!fightPlayerRuntimes.TryGetValue(characterId, out fightPlayerRuntime))
        {
            fightMonsterRuntimes.TryGetValue(characterId, out fightPlayerRuntime);
        }
        FightCharacter fightCharacter;
        if(FightManager.instance.GetFightCharacter(characterId,out fightCharacter))
        {
            fightCharacter.fightStatus = FightStatus.攻击;
            if(fightCharacter.skillRuntimes.TryGetValue(skillEstimateData.skillId,out var skillRuntime))
            {
                var skillData = skillRuntime.skillData;
                TimeLineManger.instance.PlaySkillTimeline(characterId, skillEstimateData, skillData.myTimeLineData
                   , () => {
                       fightCharacter.fightStatus = FightStatus.准备; 

                   });
            }
        }

    } 

    void SetFightCharacterAnimator(SetFightCharacterAnimator SetFightCharacterAnimator)
    {
        if (SetFightCharacterAnimator.characterId == -1)
        {
            foreach(var fightCharacterobj in fightPlayerRuntimes)
            {
                Animator animator = fightCharacterobj.Value.animator;
                SetFightCharacterAnimator.SetAnimator(animator);
            }
        }
        else if(fightPlayerRuntimes.TryGetValue(SetFightCharacterAnimator.characterId,out var fightPlayerRuntime))
        {
            Animator animator = fightPlayerRuntime.animator;
            SetFightCharacterAnimator.SetAnimator(animator);
        } 
    }

    public bool chapterMoving = false;
    float waitTime;
    float nowTime;
    public void StartWalk()
    {
        if (fightMapRuntime0.use && fightMapRuntime1.use)
        {
            SetFightCharacterAnimator(new global::SetFightCharacterAnimator
            {
                characterId = -1,
                parameter = "Speed",
                parameterType = ParameterType.FLOAT,
                floatValue = 1
            }) ;
          
            chapterMoving = true;
            if (nowTime == 0)
            {
                waitTime = GameRandom.RandomInt(GameCommon.fightWalkTime.x, GameCommon.fightWalkTime.y) * 0.001f;
            }
            StopCoroutine("MapMoving");
            StartCoroutine("MapMoving");

            ExploreManager.instance.LerpExploreTime(waitTime);
        }
        
    }
    public void StopWalk()
    {
        StopCoroutine("MapMoving");
        SetFightCharacterAnimator(new global::SetFightCharacterAnimator
        {
            characterId = -1,
            parameter = "Speed",
            parameterType = ParameterType.FLOAT,
            floatValue = 0
        });
        chapterMoving = false;
        GameTimeManager.instance.StopTimeRun();
    }

    IEnumerator FightCharacterMoving(Transform characterTransform,Vector3 targetPos)
    {
        Vector3 startPos = characterTransform.position;
        float timeValue = 0;
        while (timeValue>GameCommon.fightCharacterMoveTime)
        {
            timeValue += Time.deltaTime;
            characterTransform.position = (targetPos - startPos) * timeValue / GameCommon.fightCharacterMoveTime + startPos;
            yield return 0;
        }
        characterTransform.position = targetPos;
    }
    IEnumerator MapMoving()
    {
        var wait = new WaitForFixedUpdate();
        Vector3 late = new Vector3(-GameCommon.fightMapMovingSpeed, 0, 0);
        var transform0 = fightMapRuntime0.obj as Transform;
        var transform1 = fightMapRuntime1.obj as Transform; 
        while (nowTime<waitTime)
        {
            nowTime += Time.fixedDeltaTime;
            transform0.Translate(late);
            transform1.Translate(late);
            if (transform0.localPosition.x <= -cycleSize)
            {
                transform0.localPosition = cyclePos;
            }
            if (transform1.localPosition.x <= -cycleSize)
            {
                transform1.localPosition = cyclePos;
            }
            yield return wait;
        }
        nowTime = 0;
        waitTime = 0;
        StopWalk();
        ChapterStepAction chapterStepAction = new ChapterStepAction();
        GameActionManager.instance.QueueAction(chapterStepAction);
    }

    

}
