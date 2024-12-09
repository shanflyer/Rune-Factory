using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public enum HurtResultType
{
    Default = 0, 暴击 = 1, Miss = 2
}

public class FightManager : Singleton<FightManager>
{ 
    public override bool NeedUpdata => true;
    private Dictionary<int, FightCharacter> fightCharacters = new Dictionary<int, FightCharacter>();
    private List<int> fightPlayers = new List<int>();
    private List<int> fightMonsters = new List<int>();
    private Dictionary<int, int> playerDic = new Dictionary<int, int>();
    private Dictionary<int2, int> singleMonsterDic = new Dictionary<int2, int>();
    private Dictionary<int2, List<int>> horizontalMonsterDic = new Dictionary<int2, List<int>>();
    private Dictionary<int2, List<int>> verticalMonsterDic = new Dictionary<int2, List<int>>();

    private SkillRuntime useItemSkillRuntime;
    private Item nowUsedItem;
    public void TryUseItem(Item item)
    {
        nowUsedItem = item;
        SelectSkillAction selectSkillAction = new SelectSkillAction
        {
            skillRuntime = useItemSkillRuntime,
            ActionCharacter = CharacterManager.instance.controllerCharacter.instanceId,
        };
        GameActionManager.instance.QueueAction(selectSkillAction, true);
    }
    public float GetUseItemCd()
    {
        return useItemSkillRuntime.GetTimeValue();
    }
    public override async void Init()
    {
        base.Init();

        maxRoundCount = Enum.GetValues(typeof(FightRoundType)).Length;

        GameActionManager.instance.AddListener<CreatFightPlayer>(CreateFightPlayer);
        GameActionManager.instance.AddListener<ActionSkillEstimate>(ActionSkillEstimate);
        GameActionManager.instance.AddListener<NextActionSkillEstimate>(NextActionSkillEstimate);

        deathTimeLineData = await GameSourceManager.instance.GetScriptableObject<MyTimeLineData>(DataPath.MonsterDeathPath);
        GameActionManager.instance.AddListener<CharacterDeath>(CharacterDeath);
        GameActionManager.instance.AddListener<CharacterLevelUp>(CharacterLevelUp);
        GameActionManager.instance.AddListener<ExploreEnd>(ExploreEnd);
        GameActionManager.instance.AddListener<AllCharacterTryAutoFight>(AllCharacterTryAutoFight);
        GameActionManager.instance.AddListener<StopAllCharacterAutoFight>(StopAllCharacterAutoFight);
        GameActionManager.instance.AddListener<SkillPauseAction>(SkillPauseAction);
        GameActionManager.instance.AddListener<NoSelectSkillAction>(NoSelectSkillAction);
        GameActionManager.instance.AddListener<EndNowRoundFight>(EndNowRoundFight);
        GameActionManager.instance.AddListener<SwitchFunctionButton>(SwitchFunctionButton);

        fightResult = new FightResult
        {
            fighterResults = new List<FighterResult>(),
            getItems = new List<Item>()
        };
        GetItemIndexs = new Dictionary<int, int>();
    }

    public FightResult FightResult
    { get { return fightResult; } }
    private FightResult fightResult;

    public bool isFight { get; private set; }
    void SwitchFunctionButton(SwitchFunctionButton switchFunctionButton)
    {
        isFight = switchFunctionButton.fight;
    }
    void NoSelectSkillAction(NoSelectSkillAction noSelectSkillAction)
    {
        nowUsedItem = default(Item);
    }
    async void CreatUseItemSkill()
    {
        int skillId = 1000;
        useItemSkillRuntime = await SkillManager.instance.CreateSkillRuntime(skillId);
    }
    public List<FightCharacter> GetAllFightCharacters()
    {
        List<FightCharacter> _fightCharacters = new List<FightCharacter>();
        for(int i = 0; i < fightPlayers.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightPlayers[i],out var fightCharacter))
            {
                _fightCharacters.Add(fightCharacter);
            }
        }
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i], out var fightCharacter))
            {
                _fightCharacters.Add(fightCharacter);
            }
        }
        return _fightCharacters;
    }
    private void ExploreEnd(ExploreEnd exploreEnd)
    { 
        ClearCharacter();
        GetItemIndexs.Clear();
        fightResult.fighterResults.Clear();
        fightResult.getItems.Clear();

        AudioController.instance.PlayBGM(null, audioClearType: AudioClearType.All, Group: BGMGroup.Battle.ToString());
        AudioController.instance.SetBGMGroupValue(BGMGroup.Map.ToString(), 1);
        AudioController.instance.SetBGSGroupValue(BGSGroup.Map.ToString(), 1);
        AudioController.instance.SetBGSGroupValue(BGSGroup.Rain.ToString(), 1);
        AudioController.instance.SetBGSGroupValue(BGSGroup.Wind.ToString(), 1);
        AudioController.instance.SetBGSGroupValue(BGSGroup.Lightning.ToString(), 1); 
    }

    void EndNowRoundFight(EndNowRoundFight endNowRoundFight)
    {
        /*
       foreach(var fightcharacter in fightCharacters)
        {
            fightcharacter.Value.ClearBuff();
        }*/
    }
    void SkillPauseAction(SkillPauseAction skillPauseAction)
    {
        pauseBehavior = skillPauseAction.pause;
        Debug.Log($"暂停{pauseBehavior}");
    }

    protected override void Clear()
    { 
        useItemSkillRuntime = null;
        ClearCharacter();
        GetItemIndexs.Clear();
        base.Clear();
    }

    private void ClearCharacter()
    {
        fightCharacters.Clear();
        fightPlayers.Clear();
        for(int i = 0; i < fightMonsters.Count; i++)
        {
            MyInstance.instance.RemoveInstance(fightMonsters[i]);
        }
        fightMonsters.Clear();
        playerDic.Clear();
        singleMonsterDic.Clear();
        horizontalMonsterDic.Clear();
        verticalMonsterDic.Clear();
    }

    public int GetAttackType(int id)
    {
        if (fightCharacters.TryGetValue(id, out var fightCharacter))
        {
            return fightCharacter.attackType;
        }
        return 0;
    }

    private MyTimeLineData deathTimeLineData;

    private void CharacterLevelUp(CharacterLevelUp characterLevelUp)
    {
        if (fightPlayers.Contains(characterLevelUp.characterId))
        {
            for (int i = 0; i < fightResult.fighterResults.Count; i++)
            {
                var fighterResult = fightResult.fighterResults[i];
                fighterResult.levelUp = true;
                fightResult.fighterResults[i] = fighterResult;
               // break;
            }
        }
    }

    private void StopAllCharacterAutoFight(StopAllCharacterAutoFight allCharacterTryAutoFight)
    {
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.CheckAction())
            {
                FightController.instance.StopFightCharacter(fightCharacter.Key);
                fightCharacter.Value.fightStatus = FightStatus.准备;
            }
        }
    }

    private void AllCharacterTryAutoFight(AllCharacterTryAutoFight allCharacterTryAutoFight)
    {
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.CheckAction())
            {
                FightController.instance.RunFightCharacter(fightCharacter.Key);
                fightCharacter.Value.fightStatus = FightStatus.行动;
            }
        }
    }

    private void CharacterDeath(CharacterDeath characterDeath)
    {
        Debug.Log("播放死亡效果");
        //播放死亡效果
        TimeLineManger.instance.PlaySkillTimeline(characterDeath.characterId, null,
              deathTimeLineData, () =>
              {
                  if (fightMonsters.Contains(characterDeath.characterId))
                  {
                      var monster = (FightMonster)fightCharacters[characterDeath.characterId];
                      monster.DeathAction();
                      singleMonsterDic.Remove(monster.fightPos);
                      horizontalMonsterDic.Remove(monster.fightPos.y);
                      verticalMonsterDic.Remove(monster.fightPos.x);

                      if (fightMonsters.Remove(characterDeath.characterId))
                      {
                          MyInstance.instance.RemoveInstance(characterDeath.characterId);
                      }
                      else
                      {
                          fightCharacters.Remove(characterDeath.characterId);
                      }
                       
                      FightController.instance.RemoveFightPlayerRuntime(characterDeath.characterId);
                      GameActionManager.instance.QueueAction(new RefreshFightCharacterList());
                  }
                  else if (fightPlayers.Contains(characterDeath.characterId))
                  {
                      var fightCharacter = fightCharacters[characterDeath.characterId];
                      fightCharacter.DeathAction();
                      //fightPlayers.Remove(characterDeath.characterId);
                      fightCharacter.fightCharacterStaues = FightCharacterStaues.濒死;
                  }
              });

        MonsterDeathDrop(characterDeath.characterId);
    }

    private Dictionary<int, int> GetItemIndexs = new Dictionary<int, int>();

    //死亡掉落
    private void MonsterDeathDrop(int characterId)
    {
        if (fightMonsters.Contains(characterId))
        {
            var fightMonster = (FightMonster)fightCharacters[characterId]; 
            var dropResult = GameRandom.instance.GetRandomValue(fightMonster.monsterData.dropId);

            List<int2> items = new List<int2>();
            for (int i = 0; i < dropResult.Count; i++)
            {
                int itemId = dropResult[i].x;
                int count = dropResult[i].y;
                items.Add(new int2(itemId, count));

                if (GetItemIndexs.TryGetValue(itemId, out int index))
                {
                    var item = fightResult.getItems[index];
                    item.count += count;
                    fightResult.getItems[index] = item;
                }
                else
                {
                    var item = new Item
                    {
                        instanceId = -1,
                        dataId = itemId,
                        count = count
                    };
                    fightResult.getItems.Add(item);
                    GetItemIndexs.Add(itemId, fightResult.getItems.Count - 1);
                }
                /*
                AddPackageItem addPackageItem = new AddPackageItem
                {
                    packageId = 0,
                    itemDataId = itemId,
                    itemCount = count
                };
                GameActionManager.instance.QueueAction(addPackageItem);*/
            }
            AddPackageItemList addPackageItemList = new AddPackageItemList
            {
                packageId = 0,
                items = items
            };
            GameActionManager.instance.QueueAction(addPackageItemList);
            FightController.instance.DisplayDropItem(items, characterId);
            ExploreManager.instance.SetChapterFindItem(items);
            //获得经验
            int exp = fightMonster.monsterData.exp;
            for (int i = 0; i < fightPlayers.Count; i++)
            {
                int fightPlayerId = fightPlayers[i];
                var fightPlayer = (FightPlayer)fightCharacters[fightPlayerId];
                fightPlayer.character.AddExp(exp);
            }
        }
    }

    public SkillRuntime GetPlayerEquipSkill(int characterId)
    {
        if (fightCharacters.TryGetValue(characterId, out var character))
        {
            if (character is FightPlayer fightPlayer)
            {
                return fightPlayer.GetPlayerEquipSkill();
            }
        }
        return null;
    }

    public Dictionary<FightType, List<int>> GetReadySkills(int id, FightType fightType = FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        if (fightCharacters.TryGetValue(id, out var fightCharacter))
        {
            return fightCharacter.GetReadySkills(fightType);
        }

        return results;
    }

    private void CreateFightPlayer(CreatFightPlayer creatFightPlayer)
    { 
        playerDic.Clear();
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            Character character = CharacterManager.instance.GetCharacterForDataId(creatFightPlayer.players[i]);

            FightPlayer fightPlayer = new FightPlayer(character);
            fightPlayer.fightPos = i; 

            fightPlayer.CreatSkillRuntime();
            fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
            fightPlayers.Add(fightPlayer.instanceId);
            //FightController.instance.CreatFightPlayer(character.dataId, character.instanceId, i);

            FighterResult fighterResult = new FighterResult
            {
                Character = character,
            };
            fightResult.fighterResults.Add(fighterResult);

            playerDic.Add(i, fightPlayer.instanceId);
        }
        CreatUseItemSkill();
       
    }

    public bool GetFightCharacter(int id, out FightCharacter fightCharacter)
    {
        if (fightCharacters.TryGetValue(id, out fightCharacter))
        {
            return true;
        }
        return false;
    }

    public void RefreshFightPlayerInfo()
    {
        RefreshFightCharactersInfo refreshFightCharactersInfo = new RefreshFightCharactersInfo
        {
            characters = new List<int>()
        };
        refreshFightCharactersInfo.characters.AddRange(fightPlayers);
        GameActionManager.instance.QueueAction(refreshFightCharactersInfo);
    }

    
    public void CreateFightPlayer()
    {
        /*
        var player = CharacterManager.instance.player;
        FightPlayer fightPlayer = new FightPlayer
        {
            instanceId = player.instanceId,
            dataId = player.dataId,
        };
        fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
        fightPlayers.Add(fightPlayer.instanceId);
        FightController.instance.CreatFightPlayer(player.dataId, player.instanceId, 0);*/

        var playerTeam = TeamManager.instance.playerTeam;
        if (playerTeam != null)
        {
            List<int> players = new List<int>();
            for (int i = 0; i < playerTeam.Teamers.Count; i++)
            {
                var character = playerTeam.Teamers[i].character;
                players.Add(character.dataId);
            }
            CreatFightPlayer CreatFightPlayer = new CreatFightPlayer
            {
                players = players
            };
            GameActionManager.instance.QueueAction(CreatFightPlayer, true);
        }
        RefreshFightPlayerInfo();
        isFight = false;
    }

    public async Task CreateFightMonster(MonsterDeploy monsterDeploy)
    {
        var beforeAction = await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.beforeActionId);
        if (beforeAction != null)
        {
            beforeAction.Action();
        }

        var randomResults = GameRandom.instance.GetRandomValue(monsterDeploy.refreshId);
        var result = randomResults[0];
        var characterGroupData = await GameDataManager.instance.GetAsyncData<CharacterGroupData>(result.x);
        for (int i = 0; i < characterGroupData.characters.Count; i++)
        {
            int characterId = characterGroupData.characters[i];
            if (characterId == 0)
            {
                continue;
            }
            int col = i / 3;
            int raw = i - col * 3 - 1;

            MonsterData monsterData = await GameDataManager.instance.GetAsyncData<MonsterData>(characterId);
            FightMonster fightMonster = new FightMonster(monsterData, MyInstance.instance.uid, new int2(col, raw));   
            fightCharacters.Add(fightMonster.instanceId, fightMonster);
            fightMonsters.Add(fightMonster.instanceId);
            Debug.Log($"singleMonsterDic.Count{singleMonsterDic.Count}--fightMonster.instanceId}}{fightMonster.instanceId}--fightMonster.fightPos{fightMonster.fightPos}");
            singleMonsterDic.Add(fightMonster.fightPos, fightMonster.instanceId);
            if (!horizontalMonsterDic.TryGetValue(fightMonster.fightPos.y, out var horizontalMonsters))
            {
                horizontalMonsters = new List<int>();
                horizontalMonsterDic.Add(fightMonster.fightPos.y, horizontalMonsters);
            }
            horizontalMonsters.Add(fightMonster.instanceId);
            if (!verticalMonsterDic.TryGetValue(fightMonster.fightPos.x, out var verticalMonsters))
            {
                verticalMonsters = new List<int>();
                verticalMonsterDic.Add(fightMonster.fightPos.x, verticalMonsters);
            }
            verticalMonsters.Add(fightMonster.instanceId);

            FightController.instance.CreateFightMonster(monsterData, fightMonster.instanceId, fightMonster.fightPos);

            GameActionManager.instance.QueueAction(new RefreshFightCharacterList());
        }

        var afterAction = await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.afterActionId);
        if (afterAction != null)
        {
            afterAction.Action();
        }
    }

    public bool IsSurvival(int id)
    {
        if (fightCharacters.TryGetValue(id, out var fightCharacter))
        {
            if (fightCharacter.characterProperty.HP > 0)
            {
                return true;
            }
        }
        return false;
    }
    public int GetAttributeTypeRandomValue(AttributeType attributeType0, AttributeType attributeType1,int randomValue)
    {
        if (attributeType0 == AttributeType.无 || attributeType1 == AttributeType.无)
        {
            return randomValue;
        }
        int value = (int)attributeType0 - (int)attributeType1;
        if (math.abs(value) == 1)
        {
            if (value < 0)
            {
                return randomValue+(int)((100-randomValue)*0.5f);
            }
            return 0;
        }
        if (math.abs(value) == 4)
        {
            if (value < 0)
            {
                return 0;
            }
            return randomValue + (int)((100 - randomValue) * 0.5f);
        }
        return randomValue;
    }
    private float GetAttributeTypeValue(AttributeType attributeType0, AttributeType attributeType1)
    {
        if (attributeType0 == AttributeType.无 || attributeType1 == AttributeType.无)
        {
            return 1.0f;
        }
        int value = (int)attributeType0 - (int)attributeType1;
        if (math.abs(value) == 1)
        {
            if (value < 0)
            {
                return 1.25f;
            }
            return 0.75f;
        }
        if (math.abs(value) == 4)
        {
            if (value < 0)
            {
                return 0.75f;
            }
            return 1.25f;
        }
        return 1.0f;
    }

    public HurtResultType GetHurtResultType(int Lucky0, int Lucky1)
    {
        int LuckyValue = (Lucky0 - Lucky1) * 2;
        if (LuckyValue < 0)
        {
            int trueLucky = math.clamp(-LuckyValue, 0, Lucky1);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                return HurtResultType.Miss;
            }
            else
            {
                float value = -LuckyValue * 1.0f / Lucky0;
                value = 1 - math.clamp(value, 0, 1);
                // int trueCrit = Lucky0 + LuckyValue*2;
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    return HurtResultType.暴击;
                }
            }
        }
        else
        {
            int trueLucky = math.clamp(LuckyValue, 0, Lucky0);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                return HurtResultType.暴击;
            }
            else
            {
                float value = LuckyValue * 1.0f / Lucky1;
                value = 1 - math.clamp(value, 0, 1);
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    return HurtResultType.Miss;
                }
            }
        }
        return HurtResultType.Default;
    }

    public int HurtValue(int AT, int DF, int Lucky0, int Lucky1, out HurtResultType hurtResultType)
    {
        int hurt = 1;
        int ATValue = AT / 2;
        int DFValue = DF / 4;
        if (ATValue > DFValue)
        {
            hurt = ATValue - DFValue;
        }
        int hurtRandomAdd = hurt / 16 + 1;
        hurt += GameRandom.RandomInt(-hurtRandomAdd, hurtRandomAdd);
        hurt = math.clamp(hurt, 1, hurt);

        int LuckyValue = (Lucky0 - Lucky1) * 2;
        hurtResultType = HurtResultType.Default;
        if (LuckyValue < 0)
        {
            int trueLucky = math.clamp(-LuckyValue, 0, Lucky1);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                hurtResultType = HurtResultType.Miss;
                return 0;
            }
            else
            {
                float value = -LuckyValue * 1.0f / Lucky0;
                value = 1 - math.clamp(value, 0, 1);
                // int trueCrit = Lucky0 + LuckyValue*2;
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    hurtResultType = HurtResultType.暴击;
                    return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
                }
            }
        }
        else
        {
            int trueLucky = math.clamp(LuckyValue, 0, Lucky0);
            if (GameRandom.RandomInt(0, 100) < trueLucky)
            {
                hurtResultType = HurtResultType.暴击;
                return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
            }
            else
            {
                float value = LuckyValue * 1.0f / Lucky1;
                value = 1 - math.clamp(value, 0, 1);
                if (GameRandom.RandomFloat(0, 1.0f) < 0.05f * value)
                {
                    hurtResultType = HurtResultType.Miss;
                    return 0;
                }
            }
        }
        return hurt;
    }
    public void SetNextTarget(SkillData skillData, int fightCharacterId, ref SkillEstimateData skillEstimateData)
    {
        if(fightCharacters.TryGetValue(fightCharacterId,out var fightCharacter))
        {
            SetNextTarget(skillData, fightCharacter, ref skillEstimateData);
        }
    }
    void SetNextTarget(SkillData skillData,FightCharacter fightCharacter,ref SkillEstimateData skillEstimateData)
    {
        if (skillData.haveNextAction)
        {
            skillEstimateData.nextTargets = new List<int>();
            if (skillData.holdTarget)
            {
                skillEstimateData.nextTargets.AddRange(skillEstimateData.targets);
                skillEstimateData.nextTarget = skillEstimateData.target;
            }
            else
            {
                skillEstimateData.nextTargets = GetTarget(skillData.nextTargetType, fightCharacter);
                switch (skillData.nextTargetType)
                {
                    case TargetType.敌方:

                        skillEstimateData.nextTarget = FightController.instance.GetSkillShowTarget(fightCharacter, TargetRangeType.全部, false);
                        break;
                    case TargetType.我方:
                        skillEstimateData.nextTarget = FightController.instance.GetSkillShowTarget(fightCharacter, TargetRangeType.全部, true);
                        break;
                    case TargetType.自身:
                        skillEstimateData.nextTarget = FightController.instance.GetSkillShowTarget(fightCharacter, TargetRangeType.单体, true);
                        break;
                }
            }
        }
    }
    public List<SkillEstimateData> EstimateSkills(int skillId, int characterId)
    {
        List<SkillEstimateData> skillEstimateDatas = new List<SkillEstimateData>();
        if (fightCharacters.TryGetValue(characterId, out var fightCharacter))
        {
            if (fightCharacter.skillRuntimes.TryGetValue(skillId, out var skillRuntime))
            {
                var skillData = skillRuntime.skillData;

                var targets = GetTarget(skillData.targetType, fightCharacter, skillData.targetRangeType);
                for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                {
                    var targetList = targets[targetIndex];
                    SkillEstimateData SkillEstimateData = new SkillEstimateData
                    {
                        skillRuntime=skillRuntime,
                        source = characterId,
                        skillId = skillId,
                        targets = targetList,
                        utlilityValue = 1,
                    };
                    skillEstimateDatas.Add(SkillEstimateData);
                }

                switch (skillData.fightType)
                {
                    case FightType.攻击:
                        for (int i = 0; i < skillEstimateDatas.Count; i++)
                        {
                            var skillEstimateData = skillEstimateDatas[i];
                            Dictionary<int, int> targetHurtValueDic = new Dictionary<int, int>();
                            for (int j = 0; j < skillEstimateData.targets.Count; j++)
                            {
                                int targetId = skillEstimateData.targets[j];
                                FightCharacter tagetFighter = fightCharacters[targetId];
                                int hurt = 0;
                                if (skillData.skillActionType == SkillActionType.属性值)
                                {
                                    hurt = HurtValue(fightCharacter.characterProperty.AT, tagetFighter.characterProperty.DF,
                                  fightCharacter.characterProperty.Lucky, tagetFighter.characterProperty.Lucky, out var hurtResultType);
                                    hurt = (int)(hurt * GetAttributeTypeValue(fightCharacter.AttackAttributeType, tagetFighter.DefenceAttributeType));
                                }
                                else
                                {
                                    hurt = skillData.actionValue;
                                }
                                if (!targetHurtValueDic.TryGetValue(targetId, out var value))
                                {
                                    value = hurt;
                                }
                                else
                                {
                                    value += hurt;
                                }
                                if(fightCharacter is FightPlayer)
                                {
                                    var fightPos = fightCharacter.fightPos;
                                    if (fightPos.x == 0)
                                    {
                                        value *= 3;
                                    }
                                }

                                targetHurtValueDic[targetId] = value;
                                if (skillEstimateData.target == null)
                                {
                                    skillEstimateData.target = FightController.instance.GetSkillShowTarget(tagetFighter, skillData.targetRangeType, false);
                                }
                            }

                            foreach (var targetHurtValue in targetHurtValueDic)
                            {
                                FightCharacter tagetFighter = fightCharacters[targetHurtValue.Key];
                                float _hurtValue = (targetHurtValue.Value / tagetFighter.characterProperty.HP) * (1 - GameCommon.HurtUtlility) + GameCommon.HurtUtlility;
                                _hurtValue = math.clamp(_hurtValue, 0, 1);
                                skillEstimateData.utlilityValue += _hurtValue;
                            }
                            SetNextTarget(skillData, fightCharacter, ref skillEstimateData);
                            skillEstimateDatas[i] = skillEstimateData;
                        }
                        break;

                    case FightType.回复:
                        for (int i = 0; i < skillEstimateDatas.Count; i++)
                        {
                            var skillEstimateData = skillEstimateDatas[i];
                            float cureValue = 0;
                            for (int j = 0; j < skillEstimateData.targets.Count; j++)
                            {
                                int targetId = skillEstimateData.targets[j];
                                FightCharacter tagetFighter = fightCharacters[targetId];
                                var targetCureValue = 1 - 1 / 1 + math.pow(math.E * GameCommon.hpUtlility, (-tagetFighter.characterProperty.HP / tagetFighter.characterProperty.MaxHP * 12 + 6));
                                cureValue += targetCureValue;

                                if (skillEstimateData.target == null)
                                {
                                    skillEstimateData.target = FightController.instance.GetSkillShowTarget(tagetFighter, skillData.targetRangeType, true);
                                }
                            }
                            skillEstimateData.utlilityValue = cureValue;
                            skillEstimateDatas[i] = skillEstimateData;
                            SetNextTarget(skillData, fightCharacter, ref skillEstimateData);
                        }
                        break;
                }
            }
        }
        return skillEstimateDatas;
    }
    private List<int> GetTarget(TargetType targetType, FightCharacter fightCharacter)
    {
        List<int> finalTargets = new List<int>(); 
        switch (targetType)
        {
            case TargetType.敌方:

                if (fightPlayers.Contains(fightCharacter.instanceId))
                {
                    for (int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            finalTargets.Add(targetCharacter.instanceId);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fightPlayers.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightPlayers[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            finalTargets.Add(targetCharacter.instanceId);
                        }
                    }
                }
                break;

            case TargetType.我方:
                if (fightMonsters.Contains(fightCharacter.instanceId))
                {
                    for (int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            finalTargets.Add(targetCharacter.instanceId);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fightPlayers.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightPlayers[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            finalTargets.Add(targetCharacter.instanceId);
                        }
                    }
                }
                break;

            case TargetType.自身:
                finalTargets.Add(fightCharacter.instanceId);
                return finalTargets;
        } 
        return finalTargets;
    }
    private List<List<int>> GetTarget(TargetType targetType, FightCharacter fightCharacter, TargetRangeType targetRangeType)
    {
        List<List<int>> finalTargets = new List<List<int>>();
        List<FightCharacter> targetCharacters = new List<FightCharacter>();
        switch (targetType)
        {
            case TargetType.敌方:

                if (fightPlayers.Contains(fightCharacter.instanceId))
                {
                    for (int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fightPlayers.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightPlayers[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                break;

            case TargetType.我方:
                if (fightMonsters.Contains(fightCharacter.instanceId))
                {
                    for (int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < fightPlayers.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightPlayers[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targetCharacters.Add(targetCharacter);
                        }
                    }
                }
                break;

            case TargetType.自身:
                finalTargets.Add(new List<int> { fightCharacter.instanceId });
                return finalTargets;
        }

        switch (targetRangeType)
        {
            case TargetRangeType.单体:
                for (int i = 0; i < targetCharacters.Count; i++)
                {
                    var targetCharacter = targetCharacters[i];
                    finalTargets.Add(new List<int> { targetCharacter.instanceId });
                }
                break;

            case TargetRangeType.全部:
                {
                    List<int> targets = new List<int>();
                    for (int i = 0; i < targetCharacters.Count; i++)
                    {
                        var targetCharacter = targetCharacters[i];
                        targets.Add(targetCharacter.instanceId);
                    }
                    finalTargets.Add(targets);
                }
                break;

            case TargetRangeType.横向:
                {
                    Dictionary<int, List<int>> targets = new Dictionary<int, List<int>>();
                    for (int i = 0; i < targetCharacters.Count; i++)
                    {
                        var targetCharacter = targetCharacters[i];
                        if (!targets.TryGetValue(targetCharacter.fightPos.x, out var ints))
                        {
                            ints = new List<int>();
                            targets.Add(targetCharacter.fightPos.x, ints);
                        }
                        ints.Add(targetCharacter.instanceId);
                    }
                    foreach (var target in targets)
                    {
                        finalTargets.Add(target.Value);
                    }
                }
                break;

            case TargetRangeType.纵向:
                {
                    Dictionary<int, List<int>> targets = new Dictionary<int, List<int>>();
                    for (int i = 0; i < targetCharacters.Count; i++)
                    {
                        var targetCharacter = targetCharacters[i];
                        if (!targets.TryGetValue(targetCharacter.fightPos.y, out var ints))
                        {
                            ints = new List<int>();
                            targets.Add(targetCharacter.fightPos.y, ints);
                        }
                        ints.Add(targetCharacter.instanceId);
                    }
                    foreach (var target in targets)
                    {
                        finalTargets.Add(target.Value);
                    }
                }
                break;
        }
        return finalTargets;
    }

    private List<int> GetRandomValue(List<int> characters, int targetCount)
    {
        List<int> result = new List<int>();
        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<int3>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };

        for (int i = 0; i < characters.Count; i++)
        {
            RandomItem randomItem = new RandomItem
            { 
                itemValue = characters[characters.Count - 1 - i],
                randomValue = 20 * i,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        }
        gameRandomData.Pretreatment();

        var results = GameRandom.instance.GetRandomValue(gameRandomData, randomResultCount: targetCount);
        for (int i = 0; i < results.Count; i++)
        {
            result.Add(results[i].x);
        }
        return result;
    }
    private void NextActionSkillEstimate(NextActionSkillEstimate nextActionSkillEstimate)
    {
        int skillId = nextActionSkillEstimate.skillId;
        int sourceId = nextActionSkillEstimate.sourceId;
        FightCharacter source = fightCharacters[sourceId];
        var skillRuntime = source.skillRuntimes[skillId];
        var skillData = skillRuntime.skillData;
        List<FightCharacter> targets = new List<FightCharacter>();
        for(int i = 0; i < nextActionSkillEstimate.targets.Count; i++)
        { 
            SkillAction(skillData.nextFightType,skillData.nextSkillActionType,skillData.nextActionValue, source, fightCharacters[nextActionSkillEstimate.targets[i]], nextActionSkillEstimate.displayHurt);
        }
       
    }

    public  void BuffAction(BuffData buffData, int characterId, bool isDisplayHurt)
    {
        FightCharacter target = fightCharacters[characterId];
        switch (buffData.buffactionType)
        {
            case BuffActionType.伤害:
                int hurt = GameRandom.RandomInt(buffData.addActionValue.x, buffData.addActionValue.y);
                if (hurt > 0)
                {
                   int value= GameRandom.RandomInt(buffData.mulActionValue.x, buffData.mulActionValue.y);
                    hurt =(int)( target.characterProperty.MaxHP * (value * 0.01f));
                }
                FightHPChange(-hurt, target, isDisplayHurt, HurtResultType.Default);
                break;
            case BuffActionType.回复:
                int addHP = GameRandom.RandomInt(buffData.addActionValue.x, buffData.addActionValue.y);
                if (addHP > 0)
                {
                    int value = GameRandom.RandomInt(buffData.mulActionValue.x, buffData.mulActionValue.y);
                    addHP = (int)(target.characterProperty.MaxHP * (value * 0.01f));
                }
                FightHPChange(addHP, target, isDisplayHurt, HurtResultType.Default);
                break;
        }
    }
    async void SkillAction(FightType fightType, SkillActionType skillActionType, int actionValue,FightCharacter source,FightCharacter target,bool isDisplayHurt)
    {
        switch (fightType)
        {
            case FightType.使用道具:
                if (nowUsedItem.dataId != 0)
                {
                    ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(nowUsedItem.dataId);
                    target.CreateBuffRuntime(itemData.typeValue);
                    ItemUseAction itemUseAction = new ItemUseAction
                    {
                        itemId = nowUsedItem.dataId,
                        itemCount = 1,
                        packageId = nowUsedItem.packageId
                    };
                    GameActionManager.instance.QueueAction(itemUseAction, true);
                }
                break;
            case FightType.攻击:

                int hurt = 1;
                HurtResultType hurtResultType;
                if (skillActionType == SkillActionType.属性值)
                {
                    hurt = HurtValue(source.characterProperty.AT, target.characterProperty.DF,
                    source.characterProperty.Lucky, target.characterProperty.Lucky, out hurtResultType);
                    hurt = (int)(hurt * GetAttributeTypeValue(source.AttackAttributeType, target.DefenceAttributeType));
                    hurt = (int)(hurt * actionValue * 0.01f);
                }
                else
                {
                    hurt = actionValue;
                    hurtResultType = GetHurtResultType(source.characterProperty.Lucky, target.characterProperty.Lucky);
                }
                if (hurt < 1)
                {
                    hurt = 1;
                }
                FightHPChange(-hurt, target, isDisplayHurt, hurtResultType); 
                break;
            case FightType.回复:
                int addHp = 0;
                if (skillActionType == SkillActionType.属性值)
                {
                    addHp =(int)( target.characterProperty.MaxHP * (actionValue * 0.01f));  
                }
                else
                {
                    addHp = actionValue; 
                }
                FightHPChange(addHp, target, isDisplayHurt, HurtResultType.Default);
                break;
            case FightType.buff:
                target.CreateBuffRuntime(actionValue);
                break;
        }

        RefreshFightCharacterInfo refreshFightCharacterInfo1 = new RefreshFightCharacterInfo
        {
            characterId = source.instanceId
        };
        GameActionManager.instance.QueueAction(refreshFightCharacterInfo1, true);
    }

    public void FightHPChange(int changeValue,FightCharacter target,bool isDisplayHurt,HurtResultType hurtResultType)
    {
        ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
        {
            characterId=target.instanceId,
            changeValue = changeValue,
            propertyType = CharacterPropertyType.生命,
        }; 
        GameActionManager.instance.QueueAction(changeCharacterProperty, true);
        int hp = target.characterProperty.HP;
        if (isDisplayHurt)
        {
            DisplayHurt displayHurt = new DisplayHurt
            {
                targetId = target.instanceId,
                hurtValue =changeValue>0? $"<color=green>{changeValue}</color>": $"<color=red>{changeValue}</color>",
                hurtResultType = hurtResultType
            };
            GameActionManager.instance.QueueAction(displayHurt, true);
        }
        //Debug.Log($"角色HP：{target.Name}--{hp}");
        if (hp <= 0)
        {
           // Debug.Log($"角色死亡：{target is FightMonster}");
            CharacterDeath characterDeath = new CharacterDeath
            {
                characterId = target.instanceId,
            };
            GameActionManager.instance.QueueAction(characterDeath,true);
        }
        RefreshFightCharacterInfo refreshFightCharacterInfo = new RefreshFightCharacterInfo
        {
            characterId = target.instanceId
        };
        GameActionManager.instance.QueueAction(refreshFightCharacterInfo, true);
       
    }


    private void ActionSkillEstimate(ActionSkillEstimate actionSkillEstimate)
    {
        cdTimeMoving = false; 
        int skillId = actionSkillEstimate.skillId;
        int sourceId = actionSkillEstimate.sourceId;
        int targetId = actionSkillEstimate.targetId;
        int index = actionSkillEstimate.index;

        FightCharacter source = fightCharacters[sourceId];
        FightCharacter target = fightCharacters[targetId];
        SkillRuntime skillRuntime;
        if (useItemSkillRuntime != null && useItemSkillRuntime.instanceId == skillId)
        {
            skillRuntime = useItemSkillRuntime;
        }
        else
        {
            skillRuntime = source.skillRuntimes[skillId];
        }
       
        var skillData = skillRuntime.skillData;

        ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
        {
            characterId = source.instanceId,
            propertyType = CharacterPropertyType.法力,
            changeValue = -skillData.cost
        };
        GameActionManager.instance.QueueAction(changeCharacterProperty, true);
        SkillAction(skillData.fightType,skillData.skillActionType,skillData.actionValue, source, target, actionSkillEstimate.displayHurt);

    }

    private FightRoundType nowFightRound;
    private int maxRoundCount;

    public int GetActiveFightCharacterCount()
    {
        int ActiveCount = 0;
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.fightCharacterStaues == FightCharacterStaues.正常 && fightCharacter.Value.characterProperty.HP > 0)
            {
                ActiveCount++;
            }
        }
        return ActiveCount;
    }

    public int GetActiveFightCharacterCount(int characterId, Force force)
    {
        int ActiveCount = 0;
        int playerCount = 0;
        int monsterCount = 0;
        foreach (var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.fightCharacterStaues == FightCharacterStaues.正常 && fightCharacter.Value.characterProperty.HP > 0)
            {
                if (fightPlayers.Contains(fightCharacter.Key))
                {
                    playerCount++;
                }
                else
                {
                    monsterCount++;
                }
                ActiveCount++;
            }
        }
        if (force == Force.全部)
        {
            return ActiveCount;
        }
        if (force == Force.我方)
        {
            if (fightPlayers.Contains(characterId))
            {
                return playerCount;
            }
            return monsterCount;
        }
        else
        {
            if (fightMonsters.Contains(characterId))
            {
                return playerCount;
            }
            return monsterCount;
        }
    }
   
    
    public Queue<int> GetReadyFighter()
    {
        Queue<int> nowFightCharacters = new Queue<int>();
        foreach(var fightCharacter in fightCharacters)
        {
            if (fightCharacter.Value.waiteEnd)
            {
                nowFightCharacters.Enqueue(fightCharacter.Value.instanceId);
            }
        }
        return nowFightCharacters;
    }


    public  bool cdTimeMoving= false;
    bool pauseBehavior = false;

    public Queue<int> InitFightCharacter()
    {
        int roundType = (int)nowFightRound;
        roundType++;
        if (roundType >= maxRoundCount)
        {
            roundType = 0;
        }
        nowFightRound = (FightRoundType)roundType;

        Debug.Log($"回合轮转{nowFightRound}");

        Queue<int> nowFightCharacters = new Queue<int>();
        switch (nowFightRound)
        {
            case FightRoundType.Player:
                for (int i = 0; i < fightPlayers.Count; i++)
                {
                    int id = fightPlayers[i];
                    if (fightCharacters[id].CheckAction())
                    {
                        fightCharacters[id].fightStatus = FightStatus.准备;
                        nowFightCharacters.Enqueue(id);
                    }
                }
                break;

            case FightRoundType.Monster:
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    int id = fightMonsters[i];
                    if (fightCharacters[id].CheckAction())
                    {
                        fightCharacters[id].fightStatus = FightStatus.准备;
                        nowFightCharacters.Enqueue(id);
                    }
                }
                break;
        }

        return nowFightCharacters;
    }
    /*
    public void SetManualSelectTargets(TargetRangeType targetRangeType, int2 value)
    {
        List<int> targets = new List<int>();
        switch (targetRangeType)
        {
            case TargetRangeType.Player:
                if (playerDic.TryGetValue(value.x, out var player))
                {
                    var fightCharacter = fightCharacters[player];
                    //if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(player);
                    }
                }
                break;
            case TargetRangeType.全部:
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    var fightCharacter = fightCharacters[fightMonsters[i]];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(fightMonsters[i]);
                    }
                }
                break;

            case TargetRangeType.单体:
                if (singleMonsterDic.TryGetValue(value, out var monster))
                {
                    var fightCharacter = fightCharacters[monster];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(monster);
                    }
                }
                break;

            case TargetRangeType.横向:

                if (horizontalMonsterDic.TryGetValue(value, out var ints))
                {
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;

            case TargetRangeType.纵向:
                if (verticalMonsterDic.TryGetValue(value, out ints))
                {
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;
        }
    }*/
    public List<FightPlayer> GetAllFightPlayer()
    {
        List<FightPlayer> result = new List<FightPlayer>();
        for (int i = 0; i < fightPlayers.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightPlayers[i], out var fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.Add((FightPlayer)fightCharacter);
                }
            }
        }
        return result;
    }
    public List<FightMonster> GetAllFightMonster()
    {
        List<FightMonster> result = new List<FightMonster>();
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i], out var fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.Add((FightMonster)fightCharacter);
                }
            }
        }
        return result;
    }

    public bool2 IsFightEnd()
    {
        bool2 result = true;
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i], out FightCharacter fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.y = false;
                    break;
                }
            }
        }

        for (int i = 0; i < fightPlayers.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightPlayers[i], out FightCharacter fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.x = false;
                    break;
                }
            }
        }
        return result;
    }

    public void RoundPlayer()
    {
        List<int> newFightPlayers = new List<int>();
        if (fightCharacters.Count > 1)
        {
            List<int2> playersPos = new List<int2>();
            for (int i = 1; i < fightPlayers.Count; i++)
            {
                newFightPlayers.Add(fightPlayers[i]);
                if(fightCharacters.TryGetValue(fightPlayers[i],out var fightCharacter))
                {
                    playersPos.Add(fightCharacter.fightPos);
                }
            }
            newFightPlayers.Add(fightPlayers[0]);
            if (fightCharacters.TryGetValue(fightPlayers[0], out var fightCharacter1))
            {
                playersPos.Add(fightCharacter1.fightPos);
            }
            fightPlayers = newFightPlayers;

            for(int i = 0; i < fightPlayers.Count; i++)
            {
                if (fightCharacters.TryGetValue(fightPlayers[i], out var fightCharacter))
                {
                    fightCharacter.fightPos = playersPos[i];
                }
            }

            for (int i = 0; i < fightPlayers.Count; i++)
            {
                FightCharacterMove fightCharacterMove = new FightCharacterMove
                {
                    characterId = fightPlayers[i],
                    newIndex = i
                };
                GameActionManager.instance.QueueAction(fightCharacterMove, true);
            }
        }
    }

    public void EscapeAction()
    {
        int playerValue = 0;
        int monsterValue = 0;

        foreach (var fightCharacter in fightCharacters)
        {
            if (fightPlayers.Contains(fightCharacter.Key))
            {
                playerValue += fightCharacter.Value.characterProperty.Lucky;
            }
            else
            {
                monsterValue += fightCharacter.Value.characterProperty.Lucky;
            }
        }
        playerValue /= fightPlayers.Count;
        monsterValue /= fightMonsters.Count;
        var totalValue = 50 + playerValue - monsterValue;
        if (GameRandom.RandomInt(0, 100) < totalValue)
        {
            InformationController.instance.AddInformation("逃离成功!", true, true);
            for (int i = 0; i < fightMonsters.Count; i++)
            {
                int characterId = fightMonsters[i];
                fightCharacters.Remove(characterId);
                FightController.instance.RemoveFightPlayerRuntime(characterId);
                MyInstance.instance.RemoveInstance(characterId);
            } 
            fightMonsters.Clear(); 

            singleMonsterDic.Clear();
            horizontalMonsterDic.Clear();
            verticalMonsterDic.Clear();
            playerDic.Clear();
            ExploreManager.instance.StepFightSucceed();
        }
        else
        {
            InformationController.instance.AddInformation("逃离失败!", true, true);
            nowFightRound = FightRoundType.Player;
            PlayerFight playerFight = new PlayerFight();
            GameActionManager.instance.QueueAction(playerFight);
        }
    }

    public List<int> GetTargets(int2 key, TargetRangeType targetRangeType)
    {
        List<int> targets = new List<int>();
        switch (targetRangeType)
        {
            case TargetRangeType.Player:
                if (playerDic.TryGetValue(key.x, out var player))
                {
                    var fightCharacter = fightCharacters[player];
                    //if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(player);
                    }
                } 
                break;
            case TargetRangeType.全部:
                targets = new List<int>();
                for (int i = 0; i < fightMonsters.Count; i++)
                {
                    var fightCharacter = fightCharacters[fightMonsters[i]];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets.Add(fightMonsters[i]);
                    }
                }
                break;

            case TargetRangeType.单体:
                if (singleMonsterDic.TryGetValue(key, out var monster))
                {
                    var fightCharacter = fightCharacters[monster];
                    if (fightCharacter.characterProperty.HP > 0)
                    {
                        targets = new List<int> { monster };
                    }
                }
                break;

            case TargetRangeType.横向:
                if (horizontalMonsterDic.TryGetValue(key, out var ints))
                {
                    targets = new List<int>();
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;

            case TargetRangeType.纵向:
                if (verticalMonsterDic.TryGetValue(key, out ints))
                {
                    targets = new List<int>();
                    for (int i = 0; i < ints.Count; i++)
                    {
                        var fightCharacter = fightCharacters[ints[i]];
                        if (fightCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(ints[i]);
                        }
                    }
                }
                break;
        }
        return targets;
    }

    float useItemCd = 0;
    protected override void UpData()
    {
        base.UpData();
        if (cdTimeMoving&&!pauseBehavior)
        {
            foreach (var fightCharacter in fightCharacters)
            {
                if(fightCharacter.Value.characterProperty.HP > 0)
                {
                    fightCharacter.Value.UpData(Time.deltaTime);
                }
               
            }
            useItemCd += Time.deltaTime;
            if (useItemCd >= GameCommon.DefaultPerRoundCd)
            {
                useItemCd = 0;
                useItemSkillRuntime.UpData();
            }
        }
    }
}

public enum Force
{
    全部, 我方, 敌方
}

public enum FightRoundType
{
    Player, Monster
}

public struct FighterResult : IReferenceData
{
    public Character Character;
    public bool levelUp, skillUp;
}

public struct FightResult : IReferenceData
{
    public bool victory;
    public List<Item> getItems;
    public List<FighterResult> fighterResults;
}