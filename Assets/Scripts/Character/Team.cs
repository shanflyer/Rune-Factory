
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics; 

public class TeamManager : Singleton<TeamManager>
{
    private Dictionary<int, Team> teams = new Dictionary<int, Team>();

    public Team GetTeam(Character character)
    {
        if(teams.TryGetValue(character.instanceId,out var team))
        {
            return team;
        }
        foreach(var t in teams)
        {
            if (t.Value.CheckCharacter(character.instanceId))
            {
                return t.Value;
            }
        }
        return null;
    }
 
    public void ChangeTeamLeader(int oldLeaderId,int newLeaderId)
    {
        if(teams.TryGetValue(oldLeaderId,out var team))
        {
            teams.Remove(oldLeaderId);
            if (newLeaderId != 0)
            {
                teams.Add(newLeaderId, team);
            }
        }
    }
    public bool LeaveTeam(int characterId)
    {
        if (teams.TryGetValue(characterId,out var team))
        {
            team.RemoveCharacter(characterId);
            return true;
        }

        foreach (var t in teams)
        {
            if (t.Value.CheckCharacter(characterId))
            {
                team = t.Value;
                break;
            }
        }
        if (team != null)
        {
            team.RemoveCharacter(characterId);
            return true;
        }
        return false;
    }
    public bool IsInTeam(int characterId)
    {
        foreach (var team in teams)
        {
            if (team.Value.CheckCharacter(characterId))
            {
                return true;
            }
        }
        return false;
    }
    public void CreatTeam(Character character)
    {
        if (!teams.ContainsKey(character.instanceId))
        {
            Team team = new Team(new List<Character> { character });
            teams.Add(character.instanceId, team);
        } 
    }
    public void CreatTeam(List<Character> characters)
    {
        if (!teams.ContainsKey(characters[0].instanceId))
        {
            Team team = new Team(characters);
            teams.Add(characters[0].instanceId, team);
        }
    }


    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<DestoryCharacter>(RemoveCharacter);
        GameActionManager.instance.AddListener<JoinTeam>(JoinTeam);
    }
    void JoinTeam(JoinTeam joinTeam)
    {
        if(teams.TryGetValue(joinTeam.teamCharacterId,out var team))
        { 
            joinTeam.setResult(team.AddCharacter(joinTeam.characterId));
            return;
        }
        foreach(var t in teams)
        {
            if (t.Value.CheckCharacter(joinTeam.teamCharacterId))
            {
                joinTeam.setResult(t.Value.AddCharacter(joinTeam.characterId)); 
                return;
            }
        }
        Character character = CharacterManager.instance.GetCharacter(joinTeam.teamCharacterId);
        if (character != null)
        {
            Team team1 = new Team(new List<Character> { character });
            teams.Add(character.instanceId, team1);
            joinTeam.setResult(team1.AddCharacter(joinTeam.characterId)); 
        }

        joinTeam.setResult(false);
    }
    void RemoveCharacter(DestoryCharacter destoryCharacter)
    {
        LeaveTeam(destoryCharacter.characterId);
    }
    protected override void Clear()
    {
        base.Clear();
    }
}
public class Team
{
    public Character leader=>characters[0];
    private List<Character> characters = new List<Character>();
    private HashSet<int> characterInstances = new HashSet<int>();
   
    public Team(List<Character> characters)
    {
        this.characters = characters;
        for(int i = 0; i < characters.Count; i++)
        {
            characterInstances.Add(characters[i].instanceId);
            StopCharacterBehavior(characters[i].instanceId);
        }
    }
    void ReStartCharacterBehavior(int characterId)
    {
        StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(startCharacterBehavior, true);
    }
    void StopCharacterBehavior(int characterId)
    {
        StopCharacterMove stopCharacterMove = new StopCharacterMove
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(stopCharacterMove, true);

        StopCharacterBehavior stopCharacterBehavior = new StopCharacterBehavior
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(stopCharacterBehavior, true);
    }
    public bool CheckCharacter(int characterId)
    {
        return characterInstances.Contains(characterId);
    }
    public bool AddCharacter(int characterId)
    {
        if (characterInstances.Add(characterId))
        {
            Character character = CharacterManager.instance.GetCharacter(characterId);
            characters.Add(character);
            if (characters.Count > 1)
            {
                Character forwardCharacter = characters[characters.Count - 2];
                SetLastCoordianteAndDircet(forwardCharacter, character);
            }
            StopCharacterBehavior(characterId);
            return true;
        }
        return false;
    }
    void SetLastCoordianteAndDircet(Character forwardCharacter,Character nowCharacter)
    {
        int2 lastCoordinate = forwardCharacter.coordinate;
        Direction direction= forwardCharacter.direction;
        switch (forwardCharacter.direction)
        {
            case Direction.UP:
                lastCoordinate.y -= 2;
                break;
            case Direction.LEFT:
                lastCoordinate.x += 2;
                break;
            case Direction.RIGHT:
                lastCoordinate.x-=2;
                break;
            case Direction.DOWN:
                lastCoordinate.y+=2;
                break;
        }
        if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
        {
            switch (forwardCharacter.direction)
            {
                case Direction.UP:
                case Direction.DOWN:
                    lastCoordinate =forwardCharacter.coordinate+new int2(-2,0);
                    direction = Direction.RIGHT;
                    break;
                case Direction.LEFT: 
                case Direction.RIGHT:
                    lastCoordinate = forwardCharacter.coordinate + new int2(0, -2);
                    direction = Direction.UP;
                    break; 
            }
            if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
            {
                switch (forwardCharacter.direction)
                {
                    case Direction.UP:
                    case Direction.DOWN:
                        lastCoordinate = forwardCharacter.coordinate + new int2(+2, 0);
                        direction = Direction.LEFT;
                        break;
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        lastCoordinate = forwardCharacter.coordinate + new int2(0, +2);
                        direction = Direction.DOWN;
                        break;
                }
                if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
                {
                    lastCoordinate = forwardCharacter.coordinate;
                    direction = forwardCharacter.direction;
                }
            }
        }
        nowCharacter.SetDirection(direction);
    }
    public void RemoveCharacter(int characterid)
    { 
        bool isLeader = leader.instanceId == characterid;
        int oldCharacterId = leader.instanceId;
        if (characterInstances.Remove(characterid))
        {
            int index = characters.FindIndex(c => c.instanceId == characterid);
            if (index >= 0)
            {
                if (index < characters.Count - 1)
                {
                    for (int i = characters.Count - 1; i > index; i--)
                    {
                        var nextCharacter = characters[i];
                        var forwardCharacter = characters[i - 1];
                        nextCharacter.SetCoordinate(forwardCharacter.ObjCoordinate);
                        CharacterManager.instance.RefreshNpcRuntimeObj(nextCharacter);
                    }
                }
                characters.RemoveAt(index);
            }
            ReStartCharacterBehavior(characterid);
        }
        if (isLeader)
        {
            TeamManager.instance.ChangeTeamLeader(oldCharacterId, characters.Count > 0 ? leader.instanceId : 0);
        }
    }
    public void TeamMove(int targetRoom,int2 targetCoordinate)
    {
        int mapId = leader.mapInstance;
        int2 coordinate = leader.coordinate;
        void ChangeCoordinate()
        {
            if (characters.Count <= 1)
            {
                return;
            }

            if (mapId != leader.mapInstance)
            {
                for(int i=0;i<characters.Count; i++)
                {
                    SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                    {
                        characterId = characters[i].instanceId,
                        coordinate = new int3(leader.coordinate.xy, leader.mapInstance)
                    };
                    GameActionManager.instance.QueueAction(setCharacterCoordinate, true);
                }
            }
            else 
            {
                int distance = GameCommon.GetCellDistance(coordinate, leader.coordinate);
                if (distance >= 2)
                {
                    coordinate = leader.coordinate; 
                    TeamCharacterMove(1, leader.coordinate);
                } 
            }
        }
        leader.MoveCrossMap(targetRoom, targetCoordinate,changeCoordinateAction: ChangeCoordinate);
    }
    int2 lastCoordinate;
    void TeamCharacterMove(int index,int2 coordinate)
    {
        Character character = characters[index];
        CharacterManager.instance.CharacterMoveTarget(character, coordinate, changeCoordinateAction: () =>
        {
            int distance = GameCommon.GetCellDistance(coordinate, character.coordinate);
            if (distance >= 2)
            {
                if (characters.Count > index + 1)
                {
                    TeamCharacterMove(index + 1, coordinate);
                }else
                {
                    lastCoordinate = coordinate;
                }
            } 
        });
    }
}