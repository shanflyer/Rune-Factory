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
          await  UIManager.instance.ShowGamePanel<CharacterResponsePanel, CharacterResponseData>(characterResponseData, parent: characterRuntimeObj.transform);
        }
    }

    private void Talk(Talk talk)
    {
        Talk(talk.talkId, talk.characterId, talk.displayFunction, talk.endAction, talk.nextTalkEventId);
    }

    async void Talk(int talkId, int characterId = -1,
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
        List<int> functionIds=null;
        if(NPCManager.instance.GetNPCFormInstance(characterId,out var NPC))
        {
            functionIds = NPC.functions;
          
        }
        else if(PastureManager.instance.GetAnimal(characterId,out var animal))
        {
            functionIds = animal.animalData.functionIds;
        }
        if (functionIds != null)
        {
            for (int i = 0; i < functionIds.Count; i++)
            {
                int funtionId = functionIds[i];
                if (funtionId == GameCommon.setTeamerFunctionId && TeamManager.instance.playerTeam.CheckCharacter(characterId))
                {

                }
                else
                {
                    NPCFunctionData nPCFunctionData = await GameDataManager.instance.GetAsyncData<NPCFunctionData>(funtionId);
                    if (nPCFunctionData.GameActionData != null)
                    {
                        nPCFunctionData.GameActionData.Action(characterId, setResult: (bool value) =>
                        {
                            if(value)
                                NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
                        },immediately:true);
                    }
                    else
                    {
                        NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
                    }
                   
                }
            }
        }

       await UIManager.instance.ShowGamePanel<TalkPanel, NPCTalkOperateData>(NPCTalkOperateData);
    }
}