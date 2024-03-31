using System;
using System.Collections.Generic;
using UnityEngine;

public class TalkManager : Singleton<TalkManager>
{
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<Talk>(Talk);
        GameActionManager.instance.AddListener<SimpleTalk>(SimpleTalk);
    }

    private async void SimpleTalk(SimpleTalk simpleTalk)
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(simpleTalk.characterId, out var characterRuntimeObj))
        {
            //Character character = CharacterManager.instance.GetCharacter(simpleTalk.characterId);
            //character.StopMove();
            TalkData talkData = await GameDataManager.instance.GetAsyncData<TalkData>(simpleTalk.talkId);
            CharacterResponseData characterResponseData = new CharacterResponseData
            {
                talkValue = talkData.text,
                endAction = simpleTalk.endAction
            };
            UIManager.instance.ShowGamePanel<CharacterResponsePanel, CharacterResponseData>(characterResponseData, parent: characterRuntimeObj.runtimeObj.obj as Transform);
        }
    }

    private void Talk(Talk talk)
    {
        Talk(talk.talkId, talk.characterId, talk.displayFunction, talk.endAction, talk.nextTalkEventId);
    }

    public async void Talk(int talkId, int characterId = -1,
        bool displayFunction = false, Action endAction = null, int nextTalkEventId = 0)
    {
        TalkData talkData = await GameDataManager.instance.GetAsyncData<TalkData>(talkId);
        NPCTalkOperateData NPCTalkOperateData = new NPCTalkOperateData
        {
            characterId = characterId,
            defaultTalk = talkData,
            displayFunction = displayFunction,
            npcFunctionDatas = new List<NPCFunctionData>(),
            nextTalkEventId = nextTalkEventId,
            endAction = endAction
        };
        CharacterData characterData = CharacterManager.instance.GetCharacterDataFromInstance(characterId);
        if (characterData != null)
        {
            for (int i = 0; i < characterData.functionIds.Count; i++)
            {
                int funtionId = characterData.functionIds[i];
                if (funtionId == GameCommon.setTeamerFunctionId&&TeamManager.instance.playerTeam.CheckCharacter(characterId))
                {

                }
                else
                {
                    NPCFunctionData nPCFunctionData = await GameDataManager.instance.GetAsyncData<NPCFunctionData>(funtionId);
                    NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
                }
                
            }
        }

        UIManager.instance.ShowGamePanel<TalkPanel, NPCTalkOperateData>(NPCTalkOperateData);
    }
}