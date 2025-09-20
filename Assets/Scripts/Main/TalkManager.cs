using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Talk(talk.talkId, talk.characterId, talk.displayFunction, talk.endAction, talk.nextTalkEventId,talk.fixedFunctions);
    }

    async void Talk(int talkId, int characterId = -1,
        bool displayFunction = false, Action endAction = null, int nextTalkEventId = 0,List<int> fixedFunctions=null)
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
          
        }else if (NPCManager.instance.GetNPC(characterId, out  NPC))
        {
            functionIds = NPC.functions;
        }
        else if(PastureManager.instance.GetAnimal(characterId,out var animal))
        {
            functionIds = animal.animalData.functionIds;
        }

        bool[] functionCheckResult = null; 
        if (fixedFunctions != null)
        {
            functionCheckResult = null;
            for (int i = 0; i < fixedFunctions.Count; i++)
            {
                NPCFunctionData nPCFunctionData = await GameDataManager.instance.GetAsyncData<NPCFunctionData>(fixedFunctions[i]);
                NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
            }
        }else
        if (functionIds != null)
        {
            functionCheckResult = new bool[functionIds.Count];
            for (int i = 0; i < functionIds.Count; i++)
            {
                int functionId = functionIds[i]; 
                if (functionId == GameCommon.setTeamerFunctionId && TeamManager.instance.playerTeam.CheckCharacter(characterId))
                {
                   // continue;
                    //NPCFunctionData nPCFunctionData = await GameDataManager.instance.GetAsyncData<NPCFunctionData>(functionId);
                   // NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
                    functionCheckResult[i] = true;
                    ShowTalkAsync();
                }
                else
                {
                    NPCFunctionData nPCFunctionData = await GameDataManager.instance.GetAsyncData<NPCFunctionData>(functionId);
                    int index = i;
                    if (nPCFunctionData.GameActionData != null)
                    {
                        nPCFunctionData.GameActionData.Action(characterId, setResult: (bool value) =>
                        { 
                            if (value)
                            {
                                NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
                            }
                            functionCheckResult[index] = true;
                            ShowTalkAsync();

                        },immediately:true);
                    }
                    else
                    {
                        NPCTalkOperateData.npcFunctionDatas.Add(nPCFunctionData);
                        functionCheckResult[index] = true;
                        ShowTalkAsync();
                    }
                   
                }
            }
        }
        async Task ShowTalkAsync()
        {
            for(int i = 0; i < functionCheckResult.Length; i++)
            {
                if (!functionCheckResult[i])
                {
                    return;
                }
            }

            await UIManager.instance.ShowGamePanel<SimpleTalkPanel, NPCTalkOperateData>(NPCTalkOperateData);
        }

        if (functionCheckResult == null)
        {
            await UIManager.instance.ShowGamePanel<SimpleTalkPanel, NPCTalkOperateData>(NPCTalkOperateData);
        }
        
       
    }
}