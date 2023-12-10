using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;

public enum FightStatus
{
    准备,攻击,
}
public class FightCharacter
{
    public FightStatus fightStatus = FightStatus.准备;
    public virtual CharacterProperty characterProperty { get; }
    public int instanceId { get; set; } 
    public int behaviorId { get; set; }
    public Dictionary<int,SkillRuntime> skillRuntimes { get; set; }
    public virtual bool CheckAction() { return false; }
    public virtual void CreatSkillRuntime(IGameData gameData = null)
    {

    }

    public virtual void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    { 
    }
    public virtual  Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All) 
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        return results;
    }
}

public class FightPlayer : FightCharacter
{ 
     
    public override CharacterProperty characterProperty 
    {
        get
        {
            return character.CharacterProperty;
        }
    }

    public Character character
    {
        get
        {
            if (_character == null)
            {
                _character = CharacterManager.instance.GetCharacter(instanceId);
            }
            return _character;
        }
    }
    private Character _character;
     

    public int dataId;

    public override bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0)
        {
            return true;
        }

        return false;
    }
    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        var character = CharacterManager.instance.GetCharacter(instanceId); 
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>(); 
        using(var e = skillRuntimes.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var skillRuntime = e.Current.Value;
                if (fightType != FightType.All && skillRuntime.fightType != fightType)
                {
                    continue;
                }
                if (skillRuntime.skillCd == 0 && character.CharacterProperty.MP > skillRuntime.cost)
                {

                    if (!results.TryGetValue(skillRuntime.fightType, out var skills))
                    {
                        skills = new List<int>();
                        results.Add(skillRuntime.fightType, skills);
                    }
                    skills.Add(skillRuntime.instanceId);
                }
            }
        }
        
        
        return results;
    }
    public async override void CreatSkillRuntime(IGameData gameData= null)
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        skillRuntimes=new Dictionary<int, SkillRuntime>();
        for (int i = 0; i < character.skills.Count; i++)
        {
            int skillId = character.skills[i];
            SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
            skillRuntimes.Add(skillRuntime.instanceId,skillRuntime);
        }
    }
    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        character.SetProperty(setCharacterProperty);
    }
}
public class FightMonster : FightCharacter
{  
    public int dataId;  
    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    { 
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();

        using(var e = skillRuntimes.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var skillRuntime = e.Current.Value;
                if (fightType != FightType.All && skillRuntime.fightType != fightType)
                {
                    continue;
                }
                if (skillRuntime.skillCd == 0)
                {
                    if (!results.TryGetValue(skillRuntime.fightType, out var skills))
                    {
                        skills = new List<int>();
                        results.Add(skillRuntime.fightType, skills);
                    }
                    skills.Add(skillRuntime.instanceId);
                }
            }
        }
       
        return results;
    }
    public override bool CheckAction()
    { 
        if (characterProperty.HP > 0)
        {
            return true;
        }

        return false;
    }
    public async override void CreatSkillRuntime(IGameData gameData)
    {
        MonsterData monsterData=gameData as MonsterData;
        if(monsterData!=null)
        {
            skillRuntimes = new Dictionary<int, SkillRuntime>();
            for (int i = 0; i < monsterData.skills.Count; i++)
            {
                int skillId = monsterData.skills[i];
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId,skillRuntime);
            }
        }
    }

    public override CharacterProperty characterProperty
    {
        get
        {
            return _characterProperty;
        }
    }
    private CharacterProperty _characterProperty;

    public void InitCharacterProperty(MonsterData monsterData)
    {
        _characterProperty.HP = monsterData.HP;
        _characterProperty.AT = monsterData.AT;
        _characterProperty.DF = monsterData.DF;
        _characterProperty.Crit = monsterData.Crit;
        _characterProperty.Dodge = monsterData.Dodge;
    }

    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        _characterProperty.SetProperty(setCharacterProperty);
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = _characterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger);
    }
}

public enum HurtResultType
{
    Default=0,暴击=1,Miss=2
}
public class FightManager :Singleton<FightManager>
{
    MyInstance myInstance = new MyInstance(); 

    Dictionary<int, FightCharacter> fightCharacters = new Dictionary<int, FightCharacter>();
    List<int> fightPlayers = new List<int>();
    List<int> fightMonsters = new List<int>();

    public override async void Init()
    {
        base.Init();

        maxRoundCount = Enum.GetValues(typeof(FightRoundType)).Length;

        GameActionManager.instance.AddListener<CreatFightPlayer>(CreatFightPlayer);
        GameActionManager.instance.AddListener<ActionSkillEstimate>(ActionSkillEstimate);

        deathTimeLineData = await GameSourceManager.instance.GetScriptableObject<MyTimeLineData>(DataPath.MonsterDeathPath);
        GameActionManager.instance.AddListener<CharacterDeath>(CharacterDeath);
        GameActionManager.instance.AddListener<CharacterLevelUp>(CharacterLevelUp);
        GameActionManager.instance.AddListener<ExploreEnd>(ExploreEnd);

        fightResult = new FightResult
        {
            fighterResults=new List<FighterResult>(),
            getItems=new List<Item>()
        };
        GetItemIndexs = new Dictionary<int, int>();
    }
    public FightResult FightResult { get { return fightResult; } }
    FightResult fightResult;

    void ExploreEnd(ExploreEnd exploreEnd)
    {
        myInstance.Clear();
        fightCharacters.Clear();
        fightPlayers.Clear();
        fightMonsters.Clear();
        GetItemIndexs.Clear();
    }
    protected override void Clear()
    {
        myInstance.Clear();
        fightCharacters.Clear();
        fightPlayers.Clear();
        fightMonsters.Clear();
        GetItemIndexs.Clear();
        base.Clear();
    }
    MyTimeLineData deathTimeLineData;
    void CharacterLevelUp(CharacterLevelUp characterLevelUp)
    {
        if (fightPlayers.Contains(characterLevelUp.characterId))
        {
            for(int i = 0; i < fightResult.fighterResults.Count; i++)
            {
                var fighterResult = fightResult.fighterResults[i];
                fighterResult.levelUp = true;
                fightResult.fighterResults[i] = fighterResult;
                break;
            }
        }
    }
    void CharacterDeath(CharacterDeath characterDeath)
    {
        //播放死亡效果
        TimeLineManger.instance.PlaySkillTimeline(characterDeath.characterId, default(SkillEstimateData),
              deathTimeLineData, () =>
              {
                  if (fightMonsters.Contains(characterDeath.characterId))
                  {
                      fightMonsters.Remove(characterDeath.characterId); 
                  }else if (fightPlayers.Contains(characterDeath.characterId))
                  {
                      fightPlayers.Remove(characterDeath.characterId);
                  }
                  fightCharacters.Remove(characterDeath.characterId);
                  FightController.instance.RemoveFightPlayerRuntime(characterDeath.characterId);
              });

        MonsterDeathDrop(characterDeath.characterId);
     
    }

    Dictionary<int, int> GetItemIndexs = new Dictionary<int, int>();
    //死亡掉落
    async void MonsterDeathDrop(int characterId)
    {
        if (fightMonsters.Contains(characterId))
        {
            var fightMonster = (FightMonster)fightCharacters[characterId]; 
            var monsterData = await GameDataManager.instance.GetAsyncData<MonsterData>(fightMonster.dataId); 
            var dropResult = GameRandom.instance.GetRandomValue(monsterData.dropId);

            List<int2> items = new List<int2>();
            for (int i = 0; i < dropResult.Count; i++)
            {
                int itemId =int.Parse(dropResult[i].result);
                int count = dropResult[i].count;
                items.Add(new int2(itemId, count));

                if(GetItemIndexs.TryGetValue(itemId,out int index))
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
  
                AddPackageItem addPackageItem = new AddPackageItem
                {
                    packageId = 0,
                    itemDataId = itemId,
                    itemCount = count
                };
                GameActionManager.instance.QueueAction(addPackageItem);
            } 
            FightController.instance.DisplayDropItem(items, characterId);

            //获得经验
            int exp = monsterData.exp;
            for(int i = 0; i < fightPlayers.Count; i++)
            {
                int fightPlayerId = fightPlayers[i];
                var fightPlayer = (FightPlayer)fightCharacters[fightPlayerId];
                fightPlayer.character.AddExp(exp);
            }
        }
    }
     

    public Dictionary<FightType, List<int>> GetReadySkills(int id,FightType fightType=FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        if(fightCharacters.TryGetValue(id,out var fightCharacter))
        {
            return fightCharacter.GetReadySkills(fightType);
        }

        return results;
    }
    public void CreatFightPlayer(CreatFightPlayer creatFightPlayer)
    { 
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            Character character = CharacterManager.instance.GetCharacterForDataId(creatFightPlayer.players[i]);
             
            FightPlayer fightPlayer = new FightPlayer
            {
                instanceId = character.instanceId,
                dataId = character.dataId,
            };
            fightPlayer.CreatSkillRuntime();
            fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
            fightPlayers.Add(fightPlayer.instanceId);
            FightController.instance.CreatFightPlayer(character.dataId, character.instanceId, i);

            FighterResult fighterResult = new FighterResult
            {
                Character = character,
            };
            fightResult.fighterResults.Add(fighterResult);
        }

    }

    public bool GetFightCharacter(int id,out FightCharacter fightCharacter)
    {
        if(fightCharacters.TryGetValue(id,out fightCharacter))
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
    public async void CreatFightPlayer()
    {
        var player = CharacterManager.instance.player;
        FightPlayer fightPlayer = new FightPlayer
        {
            instanceId = player.instanceId,
            dataId = player.dataId,
        };
        fightCharacters.Add(fightPlayer.instanceId, fightPlayer);
        fightPlayers.Add(fightPlayer.instanceId); 
        FightController.instance.CreatFightPlayer(player.dataId, player.instanceId, 0);

        var teamPlayers = CharacterManager.instance.teamPlayers;
        if (teamPlayers != null && teamPlayers.Count > 0)
        {
            for(int i = 0; i < teamPlayers.Count; i++)
            {
                FightPlayer fightTeamPlayer = new FightPlayer
                {
                    instanceId = teamPlayers[i].instanceId,
                    dataId = teamPlayers[i].dataId,
                }; 

                fightCharacters.Add(fightTeamPlayer.instanceId, fightTeamPlayer);
                fightPlayers.Add(fightTeamPlayer.instanceId);

                FightController.instance.CreatFightPlayer(teamPlayers[i].dataId, teamPlayers[i].instanceId, i+1);
            }
        } 
        await  UIManager.instance.ShowGamePanel<FightPanel>(ExploreManager.instance.NowCharpter.ToString(),layer:2);
        RefreshFightPlayerInfo();
    }

    
    public async void CreatFightMonster(MonsterDeploy monsterDeploy)
    {
        var beforeAction =await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.beforeActionId);
        if (beforeAction != null)
        {
            beforeAction.Action();
        }

        List<int> monsterIds = new List<int>();
        var randomResults = GameRandom.instance.GetRandomValue(monsterDeploy.refreshId);
        for(int i = 0; i < randomResults.Count; i++)
        {
            var result = randomResults[i];
            var characterGroupData = await GameDataManager.instance.GetAsyncData<CharacterGroupData>(result.result);
            if(characterGroupData != null)
            {
                monsterIds.AddRange(characterGroupData.characters);
            }
        }
        for(int i = 0; i < 6; i++)
        {
            if (monsterIds.Count <= i)
            {
                break;
            }
            MonsterData monsterData = await GameDataManager.instance.GetAsyncData<MonsterData>(monsterIds[i]);
            FightMonster fightMonster = new FightMonster
            {
                instanceId = myInstance.CreatInstanceId(),
                dataId = monsterData.id,
                behaviorId = monsterData.behaviorId,
               
            };
            fightMonster.InitCharacterProperty(monsterData); 
            fightMonster.CreatSkillRuntime(monsterData);
            fightCharacters.Add(fightMonster.instanceId, fightMonster);
            fightMonsters.Add(fightMonster.instanceId);

            FightController.instance.CreatFightMonster(monsterData, fightMonster.instanceId, i);
        }

        var afterAction = await GameDataManager.instance.GetAsyncData<GameActionData>(monsterDeploy.afterActionId);
        if (afterAction != null)
        {
            afterAction.Action();
        }
    }
    public int HurtValue(int AT,int DF,int Crit, int Dodge0, int Dodge1,out HurtResultType hurtResultType)
    {
        int hurt = AT - DF;
        hurt = math.clamp(hurt, 1, hurt);

        int dodgeValue = Dodge0 - Dodge1;
        hurtResultType = HurtResultType.Default;
        if (dodgeValue < 0)
        {
            int trueDodge =math.clamp( dodgeValue * 2,0,Dodge1);
            if (GameRandom.RandomInt(0, 100) < trueDodge)
            {
                hurtResultType = HurtResultType.Miss;
                return 0;
            }
            else
            {
                int trueCrit =  Crit- dodgeValue*2;
                if (GameRandom.RandomInt(0, 100) < trueCrit)
                {
                    hurtResultType = HurtResultType.暴击;
                    return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
                }
            }
        }
        else
        {
            int trueDodge = math.clamp(math.abs(dodgeValue /2), 0, Dodge1);
            if (GameRandom.RandomInt(0, 100) < trueDodge)
            {
                hurtResultType = HurtResultType.Miss;
                return 0;
            }
            else
            {
                int trueCrit = math.abs(dodgeValue / 2)+Crit;
                if (GameRandom.RandomInt(0, 100) < trueCrit)
                {
                    hurtResultType = HurtResultType.暴击;
                    return (int)(hurt * GameRandom.RandomFloat(1.5f, 2.0f));
                }

            }
        }
        return hurt;
    }


    public SkillEstimateData EstimateSkill(int skillId, int characterId)
    {
        SkillEstimateData SkillEstimateData = new SkillEstimateData
        {
            source=characterId,
            skillId = skillId,
            target = new List<List<int>>(),
            utlilityValue = 1
        };
        if(fightCharacters.TryGetValue(characterId,out var fightCharacter))
        {
            if(fightCharacter.skillRuntimes.TryGetValue(skillId,out var skillRuntime))
            {
                var skillData = skillRuntime.skillData;
                for (int j = 0; j < skillData.actionCount; j++)
                {
                    var targets = GetTarget(skillData.targetType, fightCharacter, skillData.targetCount);
                    SkillEstimateData.target.Add(targets);
                }


                switch (skillData.skillActionType)
                {
                    case SkillActionType.伤害:
                        float hurtValue = 0;
                        for (int x = 0; x < SkillEstimateData.target.Count; x++)
                        {
                            for (int y = 0; y < SkillEstimateData.target[x].Count; y++)
                            {
                                int t = SkillEstimateData.target[x][y];
                                FightCharacter tagetFighter = fightCharacters[t];
                                int hurt = HurtValue(fightCharacter.characterProperty.AT, tagetFighter.characterProperty.DF,
                                    fightCharacter.characterProperty.Crit, fightCharacter.characterProperty.Dodge,
                                    tagetFighter.characterProperty.DF, out var hurtResultType);
                                float _hurtValue = (hurt / tagetFighter.characterProperty.HP) * (1 - GameCommon.HurtUtlility) + GameCommon.HurtUtlility;
                                _hurtValue = math.clamp(_hurtValue, 0, 1);
                                hurtValue += _hurtValue;
                            }
                            hurtValue = math.clamp(hurtValue, 0, 1);
                        }
                        SkillEstimateData.utlilityValue = hurtValue;
                        break;
                    case SkillActionType.待机:
                        break;
                } 
            }
             
        }
        return SkillEstimateData;
    }

    List<int> GetTarget(TargetType targetType,FightCharacter fightCharacter,int targetCount)
    { 
        switch (targetType)
        {
            case TargetType.敌方:
                List<int> targets = new List<int>();
                if (fightPlayers.Contains(fightCharacter.instanceId))
                { 
                    for(int i = 0; i < fightMonsters.Count; i++)
                    {
                        var targetCharacter = fightCharacters[fightMonsters[i]];
                        if (targetCharacter.characterProperty.HP > 0)
                        {
                            targets.Add(targetCharacter.instanceId);
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
                            targets.Add(targetCharacter.instanceId);
                        }
                    } 
                }
                return GetRandomValue(targets, targetCount);
            case TargetType.我方:
                if (fightPlayers.Contains(fightCharacter.instanceId))
                {
                    return GetRandomValue(fightPlayers, targetCount);
                }
                else
                {
                    return GetRandomValue(fightMonsters, targetCount);
                } 
            case TargetType.自身:
                List<int> result = new List<int>();
                result.Add(fightCharacter.instanceId);
                return result;
        }
        return null;
    }

    List<int> GetRandomValue(List<int> characters,int targetCount)
    {
        List<int> result = new List<int>();
        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<WeightBarrel>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };

        for (int i = 0; i < characters.Count; i++)
        {
            RandomItem randomItem = new RandomItem
            {
                itemId = characters.Count - i,
                itemValue = characters[characters.Count-1 - i].ToString(),
                randomValue = 20 * i,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);

        }
        gameRandomData.Pretreatment();

        var results = GameRandom.instance.GetRandomValue(gameRandomData,randomResultCount:targetCount);
        for (int i = 0; i < results.Count; i++)
        {
            result.Add(int.Parse(results[i].result));
        }
        return result;
    }


    void ActionSkillEstimate(ActionSkillEstimate actionSkillEstimate)
    {
        int skillId = actionSkillEstimate.skillId;
        int sourceId = actionSkillEstimate.sourceId;
        int targetId = actionSkillEstimate.targetId;
        int index = actionSkillEstimate.index;

        FightCharacter source = fightCharacters[sourceId]; 
        FightCharacter target= fightCharacters[targetId];
        var skillRuntime = source.skillRuntimes[skillId];
        var skillData = skillRuntime.skillData;

        ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
        {
            characterId = source.instanceId,
            propertyType = CharacterPropertyType.法力,
            changeValue = -skillData.cost
        };
        GameActionManager.instance.QueueAction(changeCharacterProperty, true);

        switch (skillData.skillActionType)
        {
            case SkillActionType.伤害:

                int hurt = HurtValue(source.characterProperty.AT, target.characterProperty.DF,
                    source.characterProperty.Crit, source.characterProperty.Dodge,
                    target.characterProperty.DF, out var hurtResultType);
                int hp = target.characterProperty.HP - hurt;
                hp = math.clamp(hp, 0, hp);
                SetCharacterProperty setCharacterProperty = new SetCharacterProperty
                {
                    characterId = target.instanceId,
                    propertyType = CharacterPropertyType.生命,
                    Value = hp
                };
                GameActionManager.instance.QueueAction(setCharacterProperty, true);
                if (actionSkillEstimate.displayHurt)
                {
                    DisplayHurt displayHurt = new DisplayHurt
                    {
                        targetId = targetId,
                        hurtValue = hurt,
                        hurtResultType = hurtResultType
                    };
                    GameActionManager.instance.QueueAction(displayHurt, true);
                }
                if (hp <= 0)
                {
                    CharacterDeath characterDeath = new CharacterDeath
                    {
                        characterId = targetId
                    };
                    GameActionManager.instance.QueueAction(characterDeath);
                }

                break;
        }
        RefreshFightCharacterInfo refreshFightCharacterInfo = new RefreshFightCharacterInfo
        {
            characterId = target.instanceId
        };
        GameActionManager.instance.QueueAction(refreshFightCharacterInfo, true);
        RefreshFightCharacterInfo refreshFightCharacterInfo1 = new RefreshFightCharacterInfo
        {
            characterId = source.instanceId
        };
        GameActionManager.instance.QueueAction(refreshFightCharacterInfo1, true);
    }


    FightRoundType nowFightRound;
    int maxRoundCount;
  
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
                for(int i = 0; i < fightPlayers.Count; i++)
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

    public bool2 IsFightEnd()
    { 
        bool2 result=true;
        for (int i = 0; i < fightMonsters.Count; i++)
        {
            if (fightCharacters.TryGetValue(fightMonsters[i],out FightCharacter fightCharacter))
            {
                if (fightCharacter.characterProperty.HP > 0)
                {
                    result.y = false;
                    break;
                }
            }
        }
        if (!result.y)
        {
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
        }
        
        return result;
    }
}

public enum FightRoundType
{
    Player,Monster
}
public struct FighterResult:IReferenceData
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