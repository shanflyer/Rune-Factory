using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;

public class Team
{
    public Character leader=>characters[0];
    private List<Character> characters = new List<Character>();

    public Team(List<Character> characters)
    {
        this.characters = characters;
    }
    public void AddCharacter(Character character)
    {
        characters.Add(character);
    }
    public void RemoveCharacter(Character character)
    {
        int index = characters.FindIndex(c=>c==character);
        if (index >= 0)
        {
            if (characters.Count > 1)
            {

            }
            characters.RemoveAt(index);
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
                }
            } 
        });
    }
}