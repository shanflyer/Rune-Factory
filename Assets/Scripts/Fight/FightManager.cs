using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;

interface IFightCharacter
{
    public int instanceId { get; set; } 
    public int behaviorId { get; set; }
}

public struct FightPlayer : IFightCharacter
{
    public int instanceId { get ; set ; }
    public int behaviorId { get; set; }
    public int dataId;
}
public struct FightMonster : IFightCharacter
{
    public int instanceId { get; set; }
    public int behaviorId { get; set; }
    public int dataId;

    public int HP, AT, DF, Crit, Dodge;
}

public enum HurtResultType
{
    Default=0,暴击=1,Miss=2
}
public class FightManager :Singleton<FightManager>
{
    HashSet<int> instanceIds = new HashSet<int>();
    List<FightPlayer> fightPlayers = new List<FightPlayer>();
    List<FightMonster> fightMonsters = new List<FightMonster>();


    public FightController fightController;
    public int CreatFightCharacter()
    {
        Guid guid = Guid.NewGuid();
        int instanceId = guid.GetHashCode();
        while (instanceIds.Contains(instanceId))
        {
            guid = Guid.NewGuid();
            instanceId = guid.GetHashCode();
        }
        instanceIds.Add(instanceId);
        return instanceId;
    }
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<CreatFightPlayer>(CreatFightPlayer);
    }
    protected override void Clear()
    {
        instanceIds.Clear();
        base.Clear();
    }
    public void CreatFightPlayer(CreatFightPlayer creatFightPlayer)
    { 
        for (int i = 0; i < creatFightPlayer.players.Count; i++)
        {
            Character character = CharacterManager.instance.GetCharacter(creatFightPlayer.players[i]);
             
            FightPlayer fightTeamPlayer = new FightPlayer
            {
                instanceId = character.instanceId,
                dataId = character.dataId,
            };
            fightPlayers.Add(fightTeamPlayer);
            FightController.instance.CreatFightPlayer(character.dataId, character.instanceId, i);
        }

    }
    public void CreatFightPlayer()
    {
        var player = CharacterManager.instance.player;
        FightPlayer fightPlayer = new FightPlayer
        {
            instanceId = player.instanceId,
            dataId = player.dataId,
        };
        fightPlayers.Add(fightPlayer);
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
                fightPlayers.Add(fightTeamPlayer);
                FightController.instance.CreatFightPlayer(teamPlayers[i].dataId, teamPlayers[i].instanceId, i+1);
            }
        }

    }
    public async void CreatFightMonster(MonsterDeploy monsterDeploy)
    {
        var beforeAction =await GameDataManager.instance.GetAsyncObjectData<GameActionData>(monsterDeploy.beforeActionId);
        if (beforeAction != null)
        {
            beforeAction.Action();
        }

        List<int> monsterIds = new List<int>();
        var randomResults = GameRandom.instance.GetRandomValue(monsterDeploy.refreshId);
        for(int i = 0; i < randomResults.Count; i++)
        {
            var result = randomResults[i];
            var characterGroupData = await GameDataManager.instance.GetAsyncObjectData<CharacterGroupData>(result.result);
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
            MonsterData monsterData = await GameDataManager.instance.GetAsyncObjectData<MonsterData>(monsterIds[i]);
            FightMonster fightMonster = new FightMonster
            {
                instanceId = CreatFightCharacter(),
                dataId = monsterData.id,
                behaviorId = monsterData.behaviorId,
                HP = monsterData.HP,
                AT = monsterData.AT,
                DF = monsterData.DF,
                Crit = monsterData.Crit,
                Dodge = monsterData.Dodge
            };
            fightMonsters.Add(fightMonster);

            FightController.instance.CreatFightMonster(monsterData, fightMonster.instanceId, i);
        }

        var afterAction = await GameDataManager.instance.GetAsyncObjectData<GameActionData>(monsterDeploy.afterActionId);
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
}