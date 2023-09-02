using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

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
    RuntimeObj fightMapRuntime0, fightMapRuntime1;
    [SerializeField]
    List<Transform> playerPos=new List<Transform>();
    [SerializeField]
    List<Transform> monsterPos=new List<Transform>();

    Dictionary<int,FightPlayerRuntime> fightPlayerRuntimes=new Dictionary<int, FightPlayerRuntime>();
    Dictionary<int, FightPlayerRuntime> fightMonsterRuntimes = new Dictionary<int, FightPlayerRuntime>();
    private void OnEnable()
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
    }
    private void OnDestroy()
    {
        GameRuntimeObjManager.instance.ClearRuntime<FightRuntimeObjType>();
    }
    float cycleSize;
    Vector3 cyclePos;
    public void CreatFightMap(FightMapData fightMapData)
    { 
        fightMapRuntime0 = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(),
            fightMapData.id.ToString(), fightMapData.fightMapObj, 0);
        fightMapRuntime1 = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(),
            fightMapData.id.ToString(), fightMapData.fightMapObj, 1);

        Vector3 zeroPos = new Vector3(0, fightMapData.offsetY, 0);
        cyclePos = new Vector3(fightMapData.cycleSize, fightMapData.offsetY, 0);

        cycleSize = fightMapData.cycleSize;
        fightMapRuntime0.obj.transform.localPosition = zeroPos;
        fightMapRuntime1.obj.transform.localPosition = cyclePos;
    }
    public async void CreatFightPlayer(int dataId,int instanceId,int index)
    {
        index = math.clamp(index, 0, 2);
        CharacterData characterData = await GameDataManager.instance.GetAsyncData<CharacterData>(dataId);
        if (characterData != null)
        {
            var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYER.ToString(),
                characterData.objName, characterData.obj, instanceId);
            characterRuntime.obj.transform.position = playerPos[index].position;
            FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime
            {
                playerObj = characterRuntime,
                animator = characterRuntime.obj.GetComponentInChildren<Animator>(),
                playableDirector = characterRuntime.obj.GetComponentInChildren<PlayableDirector>()
            };
            fightPlayerRuntimes[instanceId] = fightPlayerRuntime;
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
                characterData.monsterName, characterData.obj, instanceId);
            characterRuntime.obj.transform.position = monsterPos[index].position;
            FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime
            {
                playerObj = characterRuntime,
                animator = characterRuntime.obj.GetComponentInChildren<Animator>(),
                playableDirector = characterRuntime.obj.GetComponentInChildren<PlayableDirector>(),
                behaviorTree=characterRuntime.obj.GetComponent<BehaviorTree>()
            };

            var ExternalBehavior = await GameSourceManager.instance.GetBehavior($"{DataPath.BehaviorPath}{characterData.behaviorId}");
            fightPlayerRuntime.behaviorTree.ExternalBehavior = ExternalBehavior;
            fightMonsterRuntimes[instanceId] = fightPlayerRuntime;
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
    public GameObject FindFightPlayer(string name)
    {
        return null;
    }
    public GameObject FindFingMoster(string name)
    {
        return null;
    }
    public void RunFightCharacter(int characterId)
    {
        if(!fightPlayerRuntimes.TryGetValue(characterId,out var fightPlayer))
        {
            fightPlayer.behaviorTree.Start();
        }else if (fightMonsterRuntimes.TryGetValue(characterId, out var fightMonster))
        {
            fightMonster.behaviorTree.Start();
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
            fightCharacter.fightStatus = FightStatus.¹¥»÷ÖÐ;
            if(fightCharacter.skillRuntimes.TryGetValue(skillEstimateData.skillId,out var skillRuntime))
            {
                var skillData = skillRuntime.skillData;
                TimeLineManger.instance.PlaySkillTimeline(characterId, skillEstimateData, skillData.myTimeLineData, fightPlayerRuntime.playableDirector
                   , () => { fightCharacter.fightStatus = FightStatus.¹¥»÷½áÊø; });
            }
        }

    }
    public void StartWalk()
    {
        StartCoroutine(MapMoving());
    }
    public void StopWalk()
    {
        StopAllCoroutines();
    }
    IEnumerator MapMoving()
    {
        var wait = new WaitForFixedUpdate();
        Vector3 late = new Vector3(-GameCommon.fightMapMovingSpeed, 0, 0);
        while (true)
        {
            fightMapRuntime0.obj.transform.Translate(late);
            fightMapRuntime1.obj.transform.Translate(late);
            if (fightMapRuntime0.obj.transform.localPosition.x <= -cycleSize)
            {
                fightMapRuntime0.obj.transform.localPosition = cyclePos;
            }
            if (fightMapRuntime1.obj.transform.localPosition.x <= -cycleSize)
            {
                fightMapRuntime1.obj.transform.localPosition = cyclePos;
            }
            yield return wait;
        }
    }
}
