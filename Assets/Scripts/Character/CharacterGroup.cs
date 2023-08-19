using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacterGroup 
{ 
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