using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName ="Data/角色群")]
public class CharacterGroupData : ScriptableObject, IGameData
{
    public int id;
#if UNITY_EDITOR
    [NonSerialized]
    public int character0, character1, character2, character3, character4, character5;
#endif
    public List<int> characters = new List<int>();
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        characters.Clear();
        characters.Add(character0);
        characters.Add(character1);
        characters.Add(character2);
        characters.Add(character3);
        characters.Add(character4);
        characters.Add(character5);
    }
#endif
}
public struct CharacterGroup
{
    public int id;
    public List<int> characters;

    public List<int> GetFriends(int id)
    {
        List<int> friends = new List<int>();
        for(int i=0;i<characters.Count;i++)
        {
            if (characters[i] != id)
            {
                friends.Add(characters[i]);
            }
        }
        return friends;
    }
}
