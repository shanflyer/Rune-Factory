using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class TalkManager : Singleton<TalkManager>
{
    
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<Talk>(Talk);
        GameActionManager.instance.AddListener<SimpleTalk>(SimpleTalk);
    }
    async void SimpleTalk(SimpleTalk simpleTalk)
    { 
        if (CharacterManager.instance.GetRuntimeCharacterObj(simpleTalk.characterId,out var characterRuntimeObj))
        {
            //Character character = CharacterManager.instance.GetCharacter(simpleTalk.characterId);
            //character.StopMove();
            TalkData talkData = await GameDataManager.instance.GetAsyncData<TalkData>(simpleTalk.talkId);
            CharacterResponseData characterResponseData = new CharacterResponseData
            {
                talkValue = talkData.text,
                endAction= simpleTalk.endAction
            };
            UIManager.instance.ShowGamePanel<CharacterResponsePanel, CharacterResponseData>(characterResponseData, parent: characterRuntimeObj.runtimeObj.obj as Transform);
        }
    }
    
    void Talk(Talk talk)
    {
        Talk(talk.talkId, talk.characterId,talk.displayFunction,talk.endAction);
    }
    public async void Talk(int talkId,int characterId=-1,
        bool displayFunction=false,Action endAction=null)
    {
        TalkData talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkId);
        NPCTalkOperateData NPCTalkOperateData = new NPCTalkOperateData
        {
            characterId = characterId,
            defaultTalk = talkData,
            displayFunction=displayFunction,
            npcFunctionDatas = new List<NPCFunctionData>(),
            endAction=endAction
        };
        CharacterData characterData = CharacterManager.instance.GetCharacterDataFromInstance(characterId);
        if (characterData != null)
        {
            for(int i = 0; i < characterData.functionIds.Count; i++)
            {
                int funtionId = characterData.functionIds[i];
                NPCFunctionData nPCFunctionData = await GameDataManager.instance.GetAsyncData<NPCFunctionData>(funtionId);
                NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
            }
        }

        UIManager.instance.ShowGamePanel<TalkPanel, NPCTalkOperateData>(NPCTalkOperateData);
    }
}