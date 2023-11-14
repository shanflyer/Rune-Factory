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
    private BehaviorTree controllerBehavior;
      
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
        GameActionManager.instance.AddListener((HideFightScene hideFightScene) =>
        {
            GameRuntimeObjManager.instance.SetObjParent(FightRuntimeObjType.FIGHTMAP.ToString(), true);
        });
        GameActionManager.instance.AddListener((DisplayFightScene displayFightScene) =>
        {
            GameRuntimeObjManager.instance.SetObjParent(FightRuntimeObjType.FIGHTMAP.ToString(), false);
        });
        GameActionManager.instance.AddListener<StartRoundFight>(EndFightRound);
        GameActionManager.instance.AddListener<DisplayHurt>(DisplayHurt);
        controllerBehavior = GetComponent<BehaviorTree>();

        

        var sceneInfoManager = SceneInfoManager.instance;
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
                    (itemRuntime.obj as SpriteRenderer).sprite = itemData.icon;
                    var targetPos = new Vector2(GameRandom.RandomFloat(dropArea.x, dropArea.z),GameRandom.RandomFloat(dropArea.y, dropArea.w));

                    float waitTime = GameRandom.RandomFloat(GameCommon.dropWaitTime.x, GameCommon.dropWaitTime.y);
                    float moveTime = Vector2.Distance(startPos, targetPos)/GameCommon.dropItemFlyerSpeed;

                    CurveMoveData curveMoveData = new CurveMoveData
                    {
                        waitTime = waitTime,
                        moveTime = moveTime,
                        startPos = startPos,
                        targetPos = targetPos,
                        middlePos = startPos + (targetPos - startPos) * 0.5f
                    };
                    CurveMoveDatas.Add(curveMoveData);


                    CurveMoveData curveMoveDataLine = new CurveMoveData
                    {
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
    void EndFightRound(StartRoundFight startRoundFight)
    {
        if (controllerBehavior.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Inactive)
        {
            controllerBehavior.EnableBehavior();
        }
        else
        {
           // BehaviorManager.instance.RestartBehavior(controllerBehavior);
        }

        Debug.Log("回合结束...."); 
    }
   

    private void OnDestroy()
    {
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
    public async void CreatFightPlayer(int dataId,int instanceId,int index)
    {
        index = math.clamp(index, 0, 2);
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(dataId);
        if (characterData != null)
        {
            var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYER.ToString(),
                characterData.objName, characterData.obj.transform, instanceId);
            var transform = characterRuntime.obj as Transform;
            transform.position = playerPos[index].position;
            FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime
            {
                playerObj = characterRuntime,
                animator = transform.GetComponentInChildren<Animator>(),
                playableDirector = transform.GetComponentInChildren<PlayableDirector>(),
                behaviorTree = transform.GetComponent<BehaviorTree>()
            };
           
            var ExternalBehavior = await GameSourceManager.instance.GetBehavior($"{DataPath.BehaviorPath}{characterData.behavior}");
            fightPlayerRuntime.behaviorTree.ExternalBehavior = ExternalBehavior;
            fightPlayerRuntimes[instanceId] = fightPlayerRuntime;
            fightPlayerRuntime.behaviorTree.SetVariableValue("fightCharacter", instanceId);
        }
    }
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
                characterData.monsterName, characterData.obj.transform, instanceId);
            var transform = characterRuntime.obj as Transform;
            transform.position = monsterPos[index].position;
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
    public void StartWalk()
    {
        if (fightMapRuntime0.use && fightMapRuntime1.use)
        {
            StartCoroutine(MapMoving());
        }
        
    }
    public void StopWalk()
    {
        StopAllCoroutines();
    }
    IEnumerator MapMoving()
    {
        var wait = new WaitForFixedUpdate();
        Vector3 late = new Vector3(-GameCommon.fightMapMovingSpeed, 0, 0);
        var transform0 = fightMapRuntime0.obj as Transform;
        var transform1 = fightMapRuntime1.obj as Transform;
        while (true)
        {
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
    }


}
