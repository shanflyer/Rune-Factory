using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState
{
    修养中=0,正常=1
}
public struct CharacterInformationData:IReferenceData
{
    public string name;
    public Sprite head;
    public int characterId;
    public bool isNpc;
    public NPCState NPCState;
    public CharacterProperty characterProperty;
    public int level;
    public Exp exp;
    public Equip equip;
}
public partial class Character
{
    public CharacterInformationData GetInformation()
    {
        CharacterInformationData characterInformationData = new CharacterInformationData();
        characterInformationData.characterId = instanceId; 
        characterInformationData.characterProperty = characterProperty;
        characterInformationData.level = level;
        characterInformationData.exp = exp;
        characterInformationData.equip = equip;
        characterInformationData.name=name;
        characterInformationData.head = characterData.head;

        if(this is NPC)
        {
            NPC npc = this as NPC;
            characterInformationData.isNpc = true;
            characterInformationData.NPCState = npc.npcState;
        }

        return characterInformationData;
    }
}
public struct NPCList : IReferenceData
{
    public List<NPC> npcs;
}
public class NPC:Character,IReferenceData
{
    public static Color GetStateColor(NPCState state)
    {
        
        if (state == NPCState.正常)
        {
            return new Color(0, 0.5f, 0, 1);
        }
        return new Color(0.5f, 0, 0, 1);
    }
    public new string name => NPCData.npcName;

    public NPCData NPCData;   
    public NPCState npcState;
    public NPC(NPCData NPCData,CharacterData characterData, int instanceId) 
        :base(characterData,instanceId, NPCData.overridePackage)
    {
        this.NPCData = NPCData;
        npcState = NPCData.zeroState;
    }
}
public class NPCManager : Singleton<NPCManager>
{  
    
}
