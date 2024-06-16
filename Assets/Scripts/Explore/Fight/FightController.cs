using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

public struct FightPlayerRuntime
{
    public FightPlayerRuntime(RuntimeObj playerObj)
    {
        this.playerObj = playerObj;
        var transform = playerObj.obj as Transform;
        playableDirector = transform.GetComponentInChildren<PlayableDirector>();
        animator = transform.GetComponent<Animator>();
        behaviorTree = transform.GetComponent<BehaviorTree>();

        renderer = transform.gameObject.GetComponentInChildren<SpriteRenderer>();
        material = renderer.sharedMaterial;
        material.SetFloat("_LightBlend", 0.5f);
    }

    public void InitMonsterSprite(SpriteResourceRenference spriteResourceRenference)
    {
        spriteResourceRenference.SetSprite(renderer); 
    }

    public void Recycle()
    {
        material.SetFloat("_LightBlend", 1);
        material = null;
        playableDirector = null;
        animator = null;
        behaviorTree = null;
        GameRuntimeObjManager.instance.RecycleRuntimeObj(playerObj); 
        playerObj = null;
    }

    private SpriteRenderer renderer;
    private Material material;
    private RuntimeObj playerObj;
    public PlayableDirector playableDirector;
    public Animator animator;
    public BehaviorTree behaviorTree;
}

public struct FightMapRuntime
{
    public RuntimeObj runtimeObj;
    private Transform backScene, middleScene, forwardScene;

    private float cyclePos;

    public FightMapRuntime(RuntimeObj runtimeObj, float cyclePos)
    {
        this.runtimeObj = runtimeObj;
        this.cyclePos = cyclePos;
        if (runtimeObj.obj is Transform obj)
        {
            forwardScene = obj.Find("00");
            middleScene = obj.Find("02");
            backScene = obj.Find("03");
        }
        else
        {
            backScene = null;
            middleScene = null;
            forwardScene = null;
        }
        InitScene(backScene);
        InitScene(middleScene);
        InitScene(forwardScene);
    }

    private void InitScene(Transform sceneTrans)
    {
        if (sceneTrans)
        {
            Vector3 pos = sceneTrans.localPosition;
            pos.x = 0;
            sceneTrans.localPosition = pos;
        }
    }

    public void SceneMove()
    {
        if (backScene)
        {
            backScene.Translate(Vector2.left * GameCommon.fightMapMovingSpeed * 0.5f * Time.deltaTime);
            if (backScene.localPosition.x <= cyclePos)
            {
                Vector3 localPos = backScene.localPosition;
                localPos.x = 0;
                backScene.localPosition = localPos;
            }
        }

        if (middleScene)
        {
            middleScene.Translate(Vector2.left * GameCommon.fightMapMovingSpeed * Time.deltaTime);
            if (middleScene.localPosition.x <= cyclePos)
            {
                Vector3 localPos = middleScene.localPosition;
                localPos.x = 0;
                middleScene.localPosition = localPos;
            }
        }

        if (forwardScene)
        {
            forwardScene.Translate(Vector2.left * GameCommon.fightMapMovingSpeed * 1.5f * Time.deltaTime);
            if (forwardScene.localPosition.x <= cyclePos)
            {
                Vector3 localPos = forwardScene.localPosition;
                localPos.x = 0;
                forwardScene.localPosition = localPos;
            }
        }
    }

    public void Recycle()
    {
        backScene = null;
        middleScene = null;
        forwardScene = null;
        GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        runtimeObj = null;
    }
}
 

public class FightController : MonoBehaviour
{
    public static FightController instance;

    [SerializeField]
    private BehaviorTree controllerBehavior;

    [SerializeField]
    private BehaviorTree manualFightBehavior;

    private FightMapRuntime fightMapRuntime;

    [SerializeField]
    GameObject singleMaskParent, allMaskParent, horizontalMaskParent, verticalMaskParent;
    [SerializeField]
    SpriteRenderer skillMask;
    [SerializeField]
    private List<Transform> playerPos = new List<Transform>();
    [SerializeField]
    private List<Transform> monsterPos = new List<Transform>();

   

    private Dictionary<int, FightPlayerRuntime> fightPlayerRuntimes = new Dictionary<int, FightPlayerRuntime>();
    private Dictionary<int, FightPlayerRuntime> fightMonsterRuntimes = new Dictionary<int, FightPlayerRuntime>();

    private Transform monsterObj;

    private Dictionary<int2, SelectMasker> singleSelectMaskerDic = new Dictionary<int2, SelectMasker>();
    private SelectMasker allSelectMasker;
    private Dictionary<int, SelectMasker> horizontalMaskerDic = new Dictionary<int, SelectMasker>();
    private Dictionary<int, SelectMasker> verticalMaskerDic = new Dictionary<int, SelectMasker>();

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
        GameActionManager.instance.AddListener<EndPlayerRound>(EndPlayerRound);

        var behaviorTrees = GetComponents<BehaviorTree>();
        for (int i = 0; i < behaviorTrees.Length; i++)
        {
            var behaviorTree = behaviorTrees[i];
            if (behaviorTree.BehaviorName == "回合制战斗")
            {
                controllerBehavior = behaviorTree;
            }
            else
            {
                manualFightBehavior = behaviorTree;
            }
        }
        // controllerBehavior = GetComponent<BehaviorTree>();
        var sceneInfoManager = SceneInfoManager.instance;

        monsterObj = Resources.Load<GameObject>(DataPath.monsterPrefabPath).transform;

        singleSelectMaskerDic.Clear();
        var selectMaskers = singleMaskParent.GetComponentsInChildren<SelectMasker>();
        for (int i = 0; i < selectMaskers.Length; i++)
        {
            var strs = selectMaskers[i].transform.parent.name.Split(',');
            //selectMaskers[i].DisplayOrHide(false);
            singleSelectMaskerDic.Add(new int2(int.Parse(strs[0]), int.Parse(strs[1])), selectMaskers[i]);
        }
        horizontalMaskerDic.Clear();
        var horizontalMaskers = horizontalMaskParent.GetComponentsInChildren<SelectMasker>();
        for(int i = 0; i < horizontalMaskers.Length; i++)
        {
           // horizontalMaskers[i].DisplayOrHide(false);
            horizontalMaskerDic.Add(int.Parse(horizontalMaskers[i].transform.parent.name), horizontalMaskers[i]);
        }
        verticalMaskerDic.Clear();
        var verticalMaskers = verticalMaskParent.GetComponentsInChildren<SelectMasker>();
        for (int i = 0; i < verticalMaskers.Length; i++)
        {
           // verticalMaskers[i].DisplayOrHide(false);
            verticalMaskerDic.Add(int.Parse(verticalMaskers[i].transform.parent.name), verticalMaskers[i]);
        }
        allSelectMasker = allMaskParent.GetComponentInChildren<SelectMasker>();
        //allSelectMasker.DisplayOrHide(false);
    }
    void EndPlayerRound(EndPlayerRound endPlayerRound)
    {
        _chapterFight = false;
    }
    private void FightCharacterMove(FightCharacterMove fightCharacterMove)
    {
        if (fightPlayerRuntimes.TryGetValue(fightCharacterMove.characterId, out var fightPlayerRuntime))
        {
            Vector3 targetPos = playerPos[fightCharacterMove.newIndex].position;
            StartCoroutine(FightCharacterMoving(fightPlayerRuntime.animator.transform, targetPos));
        }
    }

    private void TryStartAutoBehavior(TryStartAutoBehavior tryStartAutoBehavior)
    {
        if (autoFight)
        {
            controllerBehavior.EnableBehavior();
        }
    }

    private void TryStartAutoExplore(TryStartAutoExplore tryStartAutoExplore)
    {
        if (autoExplore)
        {
            StartWalk();
            //controllerBehavior.EnableBehavior();
        }
    }

    private TargetRangeType nowDisplayTargetRangeType; 
    public void DisplayMask(TargetRangeType targetRangeType)
    {
        nowDisplayTargetRangeType = targetRangeType;
        var allFightMonsters = FightManager.instance.GetAllFightMonster();
        SelectTransform = null;
        skillMask.enabled = true;
        switch (targetRangeType)
        {
            case TargetRangeType.Null:
                skillMask.enabled = false;
                singleMaskParent.SetActive(false);
                horizontalMaskParent.SetActive(false);
                verticalMaskParent.SetActive(false);
                allMaskParent.SetActive(false);
                break;
            case TargetRangeType.单体:
                singleMaskParent.SetActive(true);
                horizontalMaskParent.SetActive(false);
                verticalMaskParent.SetActive(false);
                allMaskParent.SetActive(false);
                foreach (var selectMasker in singleSelectMaskerDic)
                {
                    selectMasker.Value.transform.parent.localScale = Vector3.zero;
                }
                for (int i = 0; i < allFightMonsters.Count; i++)
                {
                    var fightPos = allFightMonsters[i].fightPos;
                    if (singleSelectMaskerDic.TryGetValue(fightPos, out var selectMasker))
                    {
                        selectMasker.SelectAction(false);
                        if (SelectTransform == null)
                        {
                            SelectTransform= selectMasker.transform.parent;
                            selectMasker.SelectAction(true);
                        }
                        selectMasker.transform.parent.localScale = Vector3.one;
                    }
                }
                break;
            case TargetRangeType.横向:
                singleMaskParent.SetActive(false);
                horizontalMaskParent.SetActive(true);
                verticalMaskParent.SetActive(false);
                allMaskParent.SetActive(false);
                HashSet<int> allhorizontalPos = new HashSet<int>();
                for (int i = 0; i < allFightMonsters.Count; i++)
                {
                    var fightPos = allFightMonsters[i].fightPos;
                    allhorizontalPos.Add(fightPos.y);
                }
                foreach(var selectMasker in horizontalMaskerDic)
                {
                    selectMasker.Value.SelectAction(false);
                    var active = allhorizontalPos.Contains(selectMasker.Key);
                    if(active&&SelectTransform == null)
                    {
                        SelectTransform = selectMasker.Value.transform.parent;
                        selectMasker.Value.SelectAction(true);
                    }
                    selectMasker.Value.transform.parent.localScale = (active ? Vector3.one : Vector3.zero);
                }
                break;
            case TargetRangeType.纵向:
                singleMaskParent.SetActive(false);
                horizontalMaskParent.SetActive(false);
                verticalMaskParent.SetActive(true);
                allMaskParent.SetActive(false);
                HashSet<int> allVerticalPos = new HashSet<int>();
                for (int i = 0; i < allFightMonsters.Count; i++)
                {
                    var fightPos = allFightMonsters[i].fightPos;
                    allVerticalPos.Add(fightPos.x);
                }
                foreach (var selectMasker in verticalMaskerDic)
                {
                    selectMasker.Value.SelectAction(false);
                    var active = allVerticalPos.Contains(selectMasker.Key);
                    if (active && SelectTransform == null)
                    {
                        SelectTransform = selectMasker.Value.transform.parent;
                        selectMasker.Value.SelectAction(true);
                    }
                    selectMasker.Value.transform.parent.localScale=(active?Vector3.one:Vector3.zero);
                }
                break;
            case TargetRangeType.全部:
                SelectTransform = allMaskParent.transform.parent;
                singleMaskParent.SetActive(false);
                horizontalMaskParent.SetActive(false);
                verticalMaskParent.SetActive(false);
                allMaskParent.SetActive(true);
                allSelectMasker.transform.parent.localScale =Vector3.one;
                allSelectMasker.SelectAction(true);
                break;
        }
    }

    Transform SelectTransform;
    int2 selectValue;
    public void HideSelectMask()
    {
        switch (nowDisplayTargetRangeType)
        {
            case TargetRangeType.单体:
                foreach (var selectMasker in singleSelectMaskerDic)
                { 
                    selectMasker.Value.transform.parent.localScale = Vector3.zero;
                }
                break;
            case TargetRangeType.横向:
                foreach (var selectMasker in horizontalMaskerDic)
                { 
                    selectMasker.Value.transform.parent.localScale = Vector3.zero;
                }
                break;
            case TargetRangeType.纵向:
                foreach (var selectMasker in verticalMaskerDic)
                { 
                    selectMasker.Value.transform.parent.localScale = Vector3.zero;
                }
                break;
            case TargetRangeType.全部:
                allSelectMasker.transform.parent.localScale = Vector3.zero;
                break;
        }
    }
    public void SelectMask(Transform selectMask)
    {
        SelectTransform = selectMask;
        selectValue = int2.zero;
        switch (nowDisplayTargetRangeType)
        {
            case TargetRangeType.单体:
                foreach(var selectMasker in singleSelectMaskerDic)
                {
                    var active = selectMask == selectMasker.Value.transform.parent;
                    if (active)
                        selectValue = selectMasker.Key;
                    selectMasker.Value.SelectAction(active);
                }
                break;
            case TargetRangeType.横向:
                foreach (var selectMasker in horizontalMaskerDic)
                {
                    var active = selectMask == selectMasker.Value.transform.parent;
                    if (active)
                        selectValue = selectMasker.Key;
                    selectMasker.Value.SelectAction(active);
                }
                break;
            case TargetRangeType.纵向:
                foreach (var selectMasker in verticalMaskerDic)
                {
                    var active = selectMask == selectMasker.Value.transform.parent;
                    if (active)
                        selectValue = selectMasker.Key;
                    selectMasker.Value.SelectAction(active);
                }
                break;
            case TargetRangeType.全部:
                allSelectMasker.SelectAction(true);
                break;
        }
        UIManager.instance.ShowGamePanel<SkillActionPanel>(parent: selectMask);
        FightManager.instance.SetManualSelectTargets(nowDisplayTargetRangeType, selectValue);
    }

    private void SwitchAutoExplore(SwitchAutoExplore switchAutoExplore)
    {
        if (switchAutoExplore.explore)
        {
            autoExplore = !autoExplore;
            autoFight = autoExplore;
            if (switchAutoExplore.setResult != null)
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

    private void SetAutoExplore(SetAutoExplore setAutoExplore)
    {
        autoExplore = setAutoExplore.auto;
        autoFight = setAutoExplore.auto;
        if (autoExplore)
        {
            StartWalk();
        }
        else
        {
            StopWalk();
        }
    }

    private void PlayerFight(PlayerFight playerFight)
    {
        controllerBehavior.DisableBehavior();
        manualFightBehavior.EnableBehavior();
        /*
        foreach(var fightPlayer in fightPlayerRuntimes)
        {
            RunFightCharacter(fightPlayer.Key);
        }*/
    }

    private void DisplayFightScene(DisplayFightScene displayFightScene)
    {
        GameRuntimeObjManager.instance.SetObjParent(FightRuntimeObjType.FIGHTMAP.ToString(), false);
    }

    private void HideFightScene(HideFightScene hideFightScene)
    {
        GameRuntimeObjManager.instance.SetObjParent(FightRuntimeObjType.FIGHTMAP.ToString(), true);
    }

    private void ExploreEnd(ExploreEnd exploreEnd)
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

      

        int characterMap = CharacterManager.instance.controllerCharacter.mapInstance;
        if (characterMap > 0)
        {
            UIManager.instance.ShowGamePanel<PlayerTopPanel>();
            UIManager.instance.ShowGamePanel<MainPanel>();
            UIManager.instance.ShowGamePanel<ShortcutPanel>();
            UIManager.instance.ShowGamePanel<ScreenControllerPanel>();
            DisplayMap displayMap = new DisplayMap
            {
                displayMap = CharacterManager.instance.controllerCharacter.mapInstance
            };
            GameActionManager.instance.QueueAction(displayMap,true);
        }
        UIManager.instance.CloseGamePanel<FightPanel>();
        SceneManager.instance.UnloadNowScene(false);
    }

    public void RemoveFightPlayerRuntime(int characterId)
    {
        if (!fightPlayerRuntimes.TryGetValue(characterId, out var fightPlayerRuntime))
        {
            if (fightMonsterRuntimes.TryGetValue(characterId, out fightPlayerRuntime))
            {
                fightPlayerRuntime.Recycle();
                fightMonsterRuntimes.Remove(characterId);
            }
        }
        else
        {
            fightPlayerRuntime.Recycle();
            fightPlayerRuntimes.Remove(characterId);
        }
    }

    /// <summary>
    /// 展示掉落
    /// </summary>
    public async void DisplayDropItem(List<int2> items, int characterId)
    {
        if (fightMonsterRuntimes.TryGetValue(characterId, out var fightMonsterRuntime))
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
                for (int j = 0; j < itemCount; j++)
                {
                    var itemRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.OTHER.ToString(),
                      "dropItem", GameSourceManager.instance.dropItem, itemData.id);
                    SpriteRenderer spriteRenderer = itemRuntime.obj as SpriteRenderer;
                    spriteRenderer.sprite = itemData.icon;
                    spriteRenderer.transform.position = startPos;
                    spriteRenderer.transform.localScale = Vector3.zero;

                    var targetPos = finalPos + new Vector2(GameRandom.RandomFloat(dropArea.x, dropArea.z), GameRandom.RandomFloat(dropArea.y, dropArea.w));
                    var middlePos = startPos + (targetPos - startPos) * 0.5f;
                    middlePos.y += Vector2.Distance(startPos, middlePos);

                    float waitTime = GameRandom.RandomFloat(GameCommon.dropWaitTime.x, GameCommon.dropWaitTime.y);
                    float moveTime = Vector2.Distance(startPos, targetPos) / GameCommon.dropItemFlyerSpeed;

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
                GameObjectCurveController.instance.LineList(CurveMoveDatasLine, null);
            });
        }
    }

    private void DisplayHurt(DisplayHurt displayHurt)
    {
        FightPlayerRuntime fightPlayerRuntime;
        if (!fightPlayerRuntimes.TryGetValue(displayHurt.targetId, out fightPlayerRuntime))
        {
            if (fightMonsterRuntimes.TryGetValue(displayHurt.targetId, out fightPlayerRuntime))
            {
                Vector3 pos = fightPlayerRuntime.animator.transform.position;
                SceneInfoManager.instance.DisplaySceneInfo(displayHurt.hurtValue.ToString(), pos);
            }
        }
        else
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
    private bool autoFight = false;

    public bool AutoExplore => autoExplore;
    private bool autoExplore = false;

    private void EndFightRound(StartRoundFight startRoundFight)
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
        fightMapRuntime.Recycle();
        GameRuntimeObjManager.instance.ClearRuntime<FightRuntimeObjType>();
    }

    public void CreatFightMap(FightMapData fightMapData)
    {
        if (fightMapData.fightMapObj != null)
        {
            var mapRuntimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.FIGHTMAP.ToString(),
            fightMapData.id.ToString(), fightMapData.fightMapObj.transform, 0);

            fightMapRuntime = new FightMapRuntime(mapRuntimeObj, fightMapData.cycleSize);

            DisplaySky displaySky = new DisplaySky
            {
                display = fightMapData.skyDisplay,
                displaySunlight = fightMapData.displaySunlight,
            };
            GameActionManager.instance.QueueAction(displaySky);

            DisplayMask(TargetRangeType.Null);
        }
    }

    private async void CreatFightPlayer(CreatFightPlayer creatFightPlayer)
    {
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            int index = i;
            int dataId = creatFightPlayer.players[i];
            Character character;
            if (dataId == 0)
            {
                character = CharacterManager.instance.controllerCharacter;
            }
            else
            {
                character = CharacterManager.instance.GetCharacterForDataId(dataId);
            }

            index = math.clamp(index, 0, 2);

            CharacterData characterData = character.characterData;
            if (characterData != null)
            {
                var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.PLAYER.ToString(),
                    characterData.obj.name, characterData.obj.transform, character.instanceId, isActive: false);
                var transform = characterRuntime.obj as Transform;
                transform.position = playerPos[index].position;
                FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime(characterRuntime);
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


    public async void CreatFightMonster(MonsterData characterData, int instanceId, int2 pos)
    {
        int index = pos.x * 3 + pos.y + 1;
        index = math.clamp(index, 0, 5);
        if (characterData != null)
        {
            var characterRuntime = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.MONSTRT.ToString(),
                "Monster", monsterObj.transform, instanceId, isActive: false);
            var transform = characterRuntime.obj as Transform;


            transform.position = monsterPos[index].position;
            transform.gameObject.SetActive(true);
            FightPlayerRuntime fightPlayerRuntime = new FightPlayerRuntime(characterRuntime);
            fightPlayerRuntime.InitMonsterSprite(characterData.monsterSprite);
            var ExternalBehavior = await GameSourceManager.instance.GetBehavior($"{DataPath.BehaviorPath}{characterData.behaviorId}");
            fightPlayerRuntime.behaviorTree.ExternalBehavior = ExternalBehavior;
            fightMonsterRuntimes[instanceId] = fightPlayerRuntime;
            fightPlayerRuntime.behaviorTree.SetVariableValue("fightCharacter", instanceId);
        }
    }
    public GameObject GetParentObj(int instanceId, bool self)
    {
        if (fightPlayerRuntimes.ContainsKey(instanceId))
        {
            if (self)
            {
                return playerPos[0].gameObject;
            }
            else
            {
                return monsterPos[0].gameObject;
            }
        }
        else
        {
            if (!self)
            {
                return playerPos[0].gameObject;
            }
            else
            {
                return monsterPos[0].gameObject;
            }
        }
    }
   
    public Animator FindFightCharacter(int id)
    {
        if (fightPlayerRuntimes.TryGetValue(id, out var fightPlayer))
        {
            return fightPlayer.animator;
        }
        if (fightMonsterRuntimes.TryGetValue(id, out var fightMonster))
        {
            return fightMonster.animator;
        }
        return null;
    }

   public void StopFightCharacter(int characterId)
    {
        if (fightPlayerRuntimes.TryGetValue(characterId, out var fightPlayer))
        {
            fightPlayer.behaviorTree.DisableBehavior();
        }
        else if (fightMonsterRuntimes.TryGetValue(characterId, out var fightMonster))
        {
            fightMonster.behaviorTree.DisableBehavior();
        }
    }
    public void RunFightCharacter(int characterId)
    {
        if (fightPlayerRuntimes.TryGetValue(characterId, out var fightPlayer))
        {
            fightPlayer.behaviorTree.EnableBehavior();
        }
        else if (fightMonsterRuntimes.TryGetValue(characterId, out var fightMonster))
        {
            fightMonster.behaviorTree.EnableBehavior();
        }
    }
    public Transform GetSkillShowTarget(FightCharacter fightCharacter,TargetRangeType targetRangeType,bool self)
    {

        bool isPlayer = false;
        if(fightCharacter is FightPlayer)
        {
            if (self)
            {
                isPlayer = true;
            }
        }
        else
        {
            if (!self)
            {
                isPlayer = true;
            }
        }

        if(isPlayer)
        {
            switch (targetRangeType)
            {
                case TargetRangeType.全部:
                    return playerPos[0]; 
                default:
                    return playerPos[fightCharacter.fightPos.x]; 
            }
           
        }
        else
        {
            switch (targetRangeType)
            {
                case TargetRangeType.单体:

                    if (singleSelectMaskerDic.TryGetValue(fightCharacter.fightPos, out var selectMasker))
                    {
                        return selectMasker.transform.parent;
                    }
                    break;
                case TargetRangeType.横向:
                    if (horizontalMaskerDic.TryGetValue(fightCharacter.fightPos.x, out   selectMasker))
                    {
                        return selectMasker.transform.parent;
                    }
                    break;
                case TargetRangeType.纵向:
                    if (verticalMaskerDic.TryGetValue(fightCharacter.fightPos.x, out  selectMasker))
                    {
                        return selectMasker.transform.parent;
                    }
                    break;
                case TargetRangeType.全部:
                    return allSelectMasker.transform.parent;
            }
        }
        return null;
             
    }
    public void StartSkillAction(SkillEstimateData skillEstimateData, int characterId)
    {
        FightPlayerRuntime fightPlayerRuntime;
        if (!fightPlayerRuntimes.TryGetValue(characterId, out fightPlayerRuntime))
        {
            fightMonsterRuntimes.TryGetValue(characterId, out fightPlayerRuntime);
        }
        FightCharacter fightCharacter;
        if (FightManager.instance.GetFightCharacter(characterId, out fightCharacter))
        {
            fightCharacter.fightStatus = FightStatus.行动;
            if (fightCharacter.skillRuntimes.TryGetValue(skillEstimateData.skillId, out var skillRuntime))
            {
                var skillData = skillRuntime.skillData;
                TimeLineManger.instance.PlaySkillTimeline(characterId, skillEstimateData, skillData.myTimeLineData
                   , () =>
                   {
                       fightCharacter.fightStatus = FightStatus.准备;
                   });
                skillRuntime.Reset();
            }
        }
    }

    private void SetFightCharacterAnimator(SetFightCharacterAnimator SetFightCharacterAnimator)
    {
        if (SetFightCharacterAnimator.characterId == -1)
        {
            foreach (var fightCharacterobj in fightPlayerRuntimes)
            {
                Animator animator = fightCharacterobj.Value.animator;
                SetFightCharacterAnimator.SetAnimator(animator);
            }
        }
        else if (fightPlayerRuntimes.TryGetValue(SetFightCharacterAnimator.characterId, out var fightPlayerRuntime))
        {
            Animator animator = fightPlayerRuntime.animator;
            SetFightCharacterAnimator.SetAnimator(animator);
        }
    }

    public bool chapterFight => _chapterFight;
    private bool _chapterFight;

    private bool chapterMoving = false;
    private float waitTime;
    private float nowTime;

    public void StartWalk()
    {
        if (fightMapRuntime.runtimeObj.use)
        {
            SetFightCharacterAnimator(new global::SetFightCharacterAnimator
            {
                characterId = -1,
                parameter = "Speed",
                parameterType = ParameterType.FLOAT,
                floatValue = 1
            });

            chapterMoving = true;
            if (nowTime == 0)
            {
                waitTime = GameRandom.RandomInt(GameCommon.fightWalkTime.x, GameCommon.fightWalkTime.y) * 0.001f;
            }
            waitTime = 10;
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

    private IEnumerator FightCharacterMoving(Transform characterTransform, Vector3 targetPos)
    {
        Vector3 startPos = characterTransform.position;
        float timeValue = 0;
        while (timeValue > GameCommon.fightCharacterMoveTime)
        {
            timeValue += Time.deltaTime;
            characterTransform.position = (targetPos - startPos) * timeValue / GameCommon.fightCharacterMoveTime + startPos;
            yield return 0;
        }
        characterTransform.position = targetPos;
    }

    private IEnumerator MapMoving()
    {
        while (nowTime < waitTime)
        {
            nowTime += Time.deltaTime;
            fightMapRuntime.SceneMove();
            yield return 1;
        }
        nowTime = 0;
        waitTime = 0;
        StopWalk();
        ChapterStepAction chapterStepAction = new ChapterStepAction();
        GameActionManager.instance.QueueAction(chapterStepAction,true);
        _chapterFight = true;
    }


    SkillRuntime ActionSkillRuntime;
    int ActionCharacter;
    public void SelectSkill(SkillRuntime skillRuntime, int ActionCharacter)
    {
        ActionSkillRuntime = skillRuntime;
        this.ActionCharacter = ActionCharacter;
        DisplayMask(skillRuntime.skillData.targetRangeType);
    }

    public void ActionSkill()
    { 
        SkillEstimateData skillEstimateData = new SkillEstimateData
        {
            skillId = ActionSkillRuntime.instanceId,
            source = ActionCharacter,
            target = SelectTransform,
            targets = FightManager.instance.GetTargets(selectValue, ActionSkillRuntime.skillData.targetRangeType),
        };
        StartSkillAction(skillEstimateData, ActionCharacter);
        HideSelectMask();
    }
}