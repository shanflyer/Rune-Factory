
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

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
            Team team = new Team(character);
            teams.Add(character.instanceId, team);
        } 
    } 


    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<DestoryCharacter>(RemoveCharacter);
        GameActionManager.instance.AddListener<JoinTeam>(JoinTeam);
         GameActionManager.instance.AddListener<TryTeamLeaderMove>(TryTeamLeaderMove);
        GameActionManager.instance.AddListener<TryTeamLeaderSetCoordinate>(TryTeamLeaderSetCoordinate);
    }
    void TryTeamLeaderSetCoordinate(TryTeamLeaderSetCoordinate tryTeamLeaderSetCoordinate)
    {
        if (teams.TryGetValue(tryTeamLeaderSetCoordinate.characterId, out var team))
        {
            team.TeamLeaderSetCoordinate();
        }
    }
    void TryTeamLeaderMove(TryTeamLeaderMove tryTeamLeaderMove)
    {
        if(teams.TryGetValue(tryTeamLeaderMove.characterId,out var team))
        {
            team.TeamLeaderMove(tryTeamLeaderMove.length);
        }
    }
    void JoinTeam(JoinTeam joinTeam)
    {
        if(teams.TryGetValue(joinTeam.teamCharacterId,out var team))
        {
            bool result = team.AddCharacter(joinTeam.characterId);
            if (joinTeam.setResult != null)
                joinTeam.setResult(result);
            return;
        }
        foreach(var t in teams)
        {
            if (t.Value.CheckCharacter(joinTeam.teamCharacterId))
            {
                bool result = t.Value.AddCharacter(joinTeam.characterId);
                if (joinTeam.setResult != null)
                    joinTeam.setResult(result); 
                return;
            }
        }
        Character character = CharacterManager.instance.GetCharacter(joinTeam.teamCharacterId);
        if (character != null)
        {
            Team team1 = new Team(character);
            teams.Add(character.instanceId, team1);

            bool result = team1.AddCharacter(joinTeam.characterId);
            if (joinTeam.setResult != null)
                joinTeam.setResult(result); 
            return;
        }
        if (joinTeam.setResult != null)
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
    private List<int2> targets = new List<int2>();
    private List<Vector2> targetPos = new List<Vector2>();
    private List<Character> characters = new List<Character>();
    private HashSet<int> characterInstances = new HashSet<int>();
     
    public Team(Character leader)
    {
        this.characters.Clear();
        characters.Add(leader);
        StopCharacterBehavior(leader.instanceId);
        lastCoordinate = leader.coordinate;
        lastMapInstance = leader.mapInstance;
        targets.Add(leader.coordinate);
        if(CharacterManager.instance.GetRuntimeCharacterObj(leader.instanceId,out var characterRuntimeObj))
        {
            Transform transform = characterRuntimeObj.runtimeObj.obj as Transform;
            targetPos.Add(transform.localPosition);
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
        RemoveCharacterMove removeCharacterMove = new RemoveCharacterMove
        {
            characterId = characterId
        };
        GameActionManager.instance.QueueAction(removeCharacterMove, true);

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
        int2 coordinate1= forwardCharacter.coordinate;
        int2 lastCoordinate = forwardCharacter.coordinate;
        Direction direction= forwardCharacter.direction;
        switch (forwardCharacter.direction)
        {
            case Direction.UP:
                coordinate1.y -= 1;
                lastCoordinate.y -= 2;
                break;
            case Direction.LEFT:
                coordinate1.x += 1;
                lastCoordinate.x += 2;
                break;
            case Direction.RIGHT:
                coordinate1.x -= 1;
                lastCoordinate.x-=2;
                break;
            case Direction.DOWN:
                coordinate1.y += 1;
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
                    coordinate1= forwardCharacter.coordinate + new int2(-1, 0);
                    direction = Direction.RIGHT;
                    break;
                case Direction.LEFT: 
                case Direction.RIGHT: 
                    lastCoordinate = forwardCharacter.coordinate + new int2(0, -2);
                    coordinate1 = forwardCharacter.coordinate + new int2(0, -1);
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
                        coordinate1 = forwardCharacter.coordinate + new int2(1, 0);
                        direction = Direction.LEFT;
                        break;
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        lastCoordinate = forwardCharacter.coordinate + new int2(0, +2);
                        coordinate1 = forwardCharacter.coordinate + new int2(0, 1);
                        direction = Direction.DOWN;
                        break;
                }
                if (!MapCellController.instance.CheckIsWalk(lastCoordinate, forwardCharacter.mapInstance))
                {
                    lastCoordinate = forwardCharacter.coordinate;
                    coordinate1 = lastCoordinate;
                    direction = forwardCharacter.direction;
                }
            }
        }
        targets.Add(coordinate1);
        targets.Add(lastCoordinate);
        nowCharacter.SetObjCoordinate(forwardCharacter.mapInstance, lastCoordinate.xy);
        nowCharacter.SetDirection(direction);
        CharacterManager.instance.RefreshNpcRuntimeObj(nowCharacter);

        
        if (CharacterManager.instance.GetRuntimeCharacterObj(nowCharacter.instanceId, out var characterRuntimeObj))
        {
            Transform transform = characterRuntimeObj.runtimeObj.obj as Transform;
            Vector2 pos = transform.position;
            Vector2 pos0 = targetPos[targetPos.Count - 1];
            pos0 = pos0 + (pos - pos0) * 0.5f;

            targetPos.Add(pos0);
            targetPos.Add(pos);
        }
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
                targets.RemoveAt(targets.Count - 1);
                targets.RemoveAt(targets.Count - 2);
                if (targetPos.Count > 0)
                {
                    targetPos.RemoveAt(targetPos.Count - 1);
                    targetPos.RemoveAt(targetPos.Count - 2); 
                }
               
            }
            ReStartCharacterBehavior(characterid);
        }
        if (isLeader)
        {
            if (characters.Count > 0)
            {
                lastCoordinate = leader.coordinate;
                lastMapInstance = leader.mapInstance;
            }
            TeamManager.instance.ChangeTeamLeader(oldCharacterId, characters.Count > 0 ? leader.instanceId : 0);
        }
    }
    public void TeamLeaderMove(float length)
    {
        if (characters.Count > 1)
        {
            for(int i = 1; i < characters.Count; i++)
            {

                if (length > 0)
                {
                    if (targetPos.Count > 0)
                    {
                        
                        float2 direction = targetPos[i * 2 - 1] - targetPos[i * 2];
                        if (!direction.Equals(float2.zero))
                        {
                            direction = math.normalize(direction);

                            CharacterManager.instance.MoveCharacterObj(characters[i], length * direction);
                        }
                       
                    }
                    else
                    {
                        float2 direction = targets[i * 2 - 1] - targets[i * 2];
                        direction = math.normalize(direction);

                        CharacterManager.instance.MoveCharacterObj(characters[i], length * direction);
                    }
                }
                else
                {
                    CharacterManager.instance.SetCharacterAnimationSpeed(0, characters[i]);
                }  
            }
        }
    } 
    public void TeamLeaderSetCoordinate()
    {
        if (characters.Count > 1)
        {
            if (lastMapInstance != leader.mapInstance)
            {
                lastMapInstance = leader.mapInstance;
                for (int i = 0; i < characters.Count; i++)
                {
                    SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                    {
                        characterId = characters[i].instanceId,
                        coordinate = new int3(leader.coordinate.xy, leader.mapInstance)
                    };
                    GameActionManager.instance.QueueAction(setCharacterCoordinate, true);
                }
                for(int i = 0; i < targets.Count; i++)
                {
                    targets[i] = leader.coordinate;
                }
                if (CharacterManager.instance.GetRuntimeCharacterObj(leader.instanceId, out var characterRuntimeObj))
                {
                    Transform transform = characterRuntimeObj.runtimeObj.obj as Transform;
                    Vector2 pos = transform.position;
                    for (int i = 0; i < targetPos.Count; i++)
                    {
                        targetPos[i] = pos;
                    }
                }
            }
            else
            {
               targets.Insert(0,leader.coordinate);
                if (CharacterManager.instance.GetRuntimeCharacterObj(leader.instanceId, out var characterRuntimeObj))
                {
                    Transform transform = characterRuntimeObj.runtimeObj.obj as Transform;
                    Vector2 pos = transform.position;  
                    targetPos.Insert(0, pos);
                }
                else
                {
                    targetPos.Clear();
                }
                for (int i = 1; i < characters.Count; i++)
                {
                    var character = characters[i];  
                    character.SetObjCoordinate(character.mapInstance, targets[i*2]);
                    float2 direction = targets[i * 2 - 1] - targets[i * 2];
                    direction = math.normalize(direction);
                    character.SetDataDirection(direction);
                  //  CharacterManager.instance.RefreshNpcRuntimeObj(character);

                    if (targetPos.Count > 0)
                    {
                       float2 displayDirection = targetPos[i * 2 - 2] - targetPos[i * 2];
                       displayDirection = math.normalize(displayDirection);
                       character.SetAnimationDirection(displayDirection);
                       CharacterManager.instance.SetCharacterObj(characters[i], targetPos[i * 2]);
                    } 
                }
                targets.RemoveAt(targets.Count - 1);
                if (targetPos.Count > 0)
                {
                    targetPos.RemoveAt(targetPos.Count - 1);
                }
            } 
        }
    }
    int2 lastCoordinate;
    int lastMapInstance;
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
                    if (character.nowSpeed != 0)
                    {
                        TeamCharacterMove(index + 1, coordinate);
                    }
                    
                } 
            } 
        },overrideSpeed:CharacterManager.updataMoveSpeed*2);
    }
}