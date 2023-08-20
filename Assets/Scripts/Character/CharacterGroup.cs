using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGroupData : ScriptableObject, IGameData
{
    public int id;
    public List<int> characters = new List<int>();
    public string GetKey()
    {
        return id.ToString();
    }
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
                friends.Add(i);
            } 
        }
        return friends;
    }
}