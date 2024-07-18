using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;

public class MultiNPCBehaviorManager:Singleton<MultiNPCBehaviorManager>
{
    private MyInstance myInstance;
    public MyDic<int,List<int>> mulitNpcGroups = new MyDic<int, List<int>>();

    public override void Init()
    {
        base.Init();
        myInstance = new MyInstance();
        mulitNpcGroups.Clear();
    }
    public bool IsInMulitGroup(int characterId)
    {
        for(int i = 0; i < mulitNpcGroups.length; i++)
        {
            if (mulitNpcGroups[i].Contains(characterId))
            {
                return true;
            }
        }
        return false;
    }
    public void CreatMulitGroup(List<int> characters,bool faceCenter=true)
    {
        int groupInstance = myInstance.CreatInstanceId();
        int maxX = int.MinValue;
        int maxY = int.MinValue;
        int minX=int.MaxValue; int minY=int.MaxValue;

        for(int i = 0; i < characters.Count; i++)
        {
            int characterId = characters[i];
            Character character = CharacterManager.instance.GetCharacter(characterId);
            if (character != null)
            {
                if (maxX < character.coordinate.x)
                    maxX = character.coordinate.x;
                if (maxY < character.coordinate.y)
                    maxY = character.coordinate.y;
                if (minX > character.coordinate.x)
                    minX = character.coordinate.x;
                if (minY < character.coordinate.y)
                    minY = character.coordinate.y;

                character.StopMove();
                character.mulitGroup = groupInstance; 
                StopCharacterBehavior stopCharacterBehavior = new StopCharacterBehavior
                {
                    characterId = characterId,
                };
                GameActionManager.instance.QueueAction(stopCharacterBehavior);
            }
        }
        int2 center=new int2(minX+(maxX-minX)/2,minY+(maxY-minY)/2);
        if (faceCenter)
        {
            for(int i = 0; i < characters.Count; i++)
            {
                var characterId = characters[i];
                SetTargetDirection SetTargetDirection = new SetTargetDirection
                {
                    characterId = characterId,
                    targetCoordinate = center
                };
                GameActionManager.instance.QueueAction(SetTargetDirection, true);
            }
        }

        mulitNpcGroups.Add(groupInstance, characters);
    } 
    public void DestroyMulitGroup(int groupId,HashSet<int> exceptionCharacters)
    {
        if(mulitNpcGroups.TryGetValue(groupId,out var characters))
        {
            for (int i = 0; i < characters.Count; i++)
            {
                int characterId = characters[i];
                Character character = CharacterManager.instance.GetCharacter(characterId);
                if (character != null)
                {
                    character.mulitGroup =0;
                    if (!exceptionCharacters.Contains(characterId))
                    {
                        StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
                        {
                            characterId = characterId
                        };
                        GameActionManager.instance.QueueAction(startCharacterBehavior);
                    }
                }
            }
        } 
        mulitNpcGroups.Remove(groupId);
    }
}
