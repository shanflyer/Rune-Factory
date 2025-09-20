using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonPanel :GamePanel<MyListInt>
{
    [SerializeField]
    Button MultiCharacterButton;
    [SerializeField]
    CharacterButtonReference CharacterButtonReference;
    [SerializeField]
    Transform characterButtonParent;
    
    DisplayList<CharacterButtonReference,MyInt> characterButtons;
    protected override void Awake()
    {
        base.Awake();
        characterButtons = new DisplayList<CharacterButtonReference, MyInt>(CharacterButtonReference, characterButtonParent);
        MultiCharacterButton.onClick.AddListener(() =>
        {
            if (MultiCharacterButton.transform.localScale.x != 0)
            {
                MultiCharacterButton.transform.localScale = Vector3.zero;
                characterButtonParent.gameObject.SetActive(true);

                nowCharacters.Clear();
                using (var e = characters.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        nowCharacters.Add(new MyInt { value = e.Current });
                    }
                }

                characterButtons.InitListData(nowCharacters, SelectAction);
            }
        });

        GameActionManager.instance.AddListener<RefreshOperateCharacters>(RefreshOperateCharacters);
        GameActionManager.instance.AddListener<RefreshOperateCharacter>(RefreshOperateCharacter);
    }
    List<MyInt> nowCharacters = new List<MyInt>();
    HashSet<int> characters = new HashSet<int>();
    async void SelectAction(MyInt seletCharacter, int index, bool selected = true)
    {

        Character character = CharacterManager.instance.GetCharacter(seletCharacter.value);

        if (character != null)
        {
            if (character.isInTeam)
            {
               // return;
            }
           
            if (!PastureManager.instance.TalkAnimal(character.instanceId))
            {
                EventReferenceData eventReferenceData = new EventReferenceData
                {
                    name = "CharacterId",
                    value = seletCharacter.value
                };
                EventReferenceData targetReferenceData = new EventReferenceData
                {
                    name = "TargetCharacter",
                    value = CharacterManager.instance.controllerCharacter.instanceId
                };

                int nextTalkEventId = 0;
                int eventId = 0;


                if (character is TempCharacter tempCharacter)
                {
                    nextTalkEventId = tempCharacter.tempCharacterData.nextTalkEventId;
                    eventId = tempCharacter.tempCharacterData.tempTalkEventId;
                }
                else if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var NPC))
                {
                    nextTalkEventId = NPC.nextTalkEventId;
                    eventId = NPC.playerOperateEventId;
                    AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                    {
                        characterId = character.instanceId,
                        friendAddType = FriendAddType.对话,
                        value = 1
                    };
                    GameActionManager.instance.QueueAction(addFriendShipValue);
                }

                EventReferenceData NextTalkReferenceData = new EventReferenceData
                {
                    name = "NextTalkEventId",
                    value = nextTalkEventId
                };


                await GameEventManager.instance.AddGameEvent(eventId, new List<EventReferenceData>
                {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
                });
            }

           
        }
    }

    private async void RefreshOperateCharacters(RefreshOperateCharacters refreshOperateCharacters)
    {
        bool refresh = false; 
        refreshOperateCharacters.joinCharacters.ExceptWith(TeamManager.instance.playerTeam.TeamCharacters); 
        if (refreshOperateCharacters.joinCharacters != null)
        { 
            refresh = true;
            characters.UnionWith(refreshOperateCharacters.joinCharacters);
        }
        if (refreshOperateCharacters.leaveCharacters != null)
        {
            var simpleTalkPanel = await UIManager.instance.GetGamePanel<SimpleTalkPanel>();
            if (simpleTalkPanel != null)
                foreach (var leaveCharacter in refreshOperateCharacters.leaveCharacters)
                    simpleTalkPanel.TryClose(leaveCharacter);

            refresh = true;
            characters.ExceptWith(refreshOperateCharacters.leaveCharacters);
        }
        if (refresh)
        {
            nowCharacters.Clear();
            using (var e = characters.GetEnumerator())
            { 
                while (e.MoveNext())
                {
                    if (CharacterManager.instance.IsTempCharacter(e.Current))
                    {
                        continue;
                    }

                    nowCharacters.Add(new MyInt { value = e.Current });
                }
            }
            switch (nowCharacters.Count)
            {
                case 0:
                    characterButtonParent.gameObject.SetActive(false);
                    CharacterButtonReference.transform.localScale = Vector3.zero;
                    MultiCharacterButton.transform.localScale = Vector3.zero;
                    CharacterButtonReference.enabled = false;
                    break;
                case 1:
                    characterButtonParent.gameObject.SetActive(false);
                    CharacterButtonReference.transform.localScale = Vector3.one;
                    MultiCharacterButton.transform.localScale = Vector3.zero;
                    CharacterButtonReference.InitData(nowCharacters[0], SelectAction);
                    CharacterButtonReference.enabled = true; 
                    break;
                default:
                    if (characterButtonParent.gameObject.activeSelf)
                    {
                        characterButtons.InitListData(nowCharacters, SelectAction);
                    }
                    else
                    {
                        CharacterButtonReference.transform.localScale = Vector3.zero;
                        MultiCharacterButton.transform.localScale = Vector3.one;
                    }
                   
                    break;
            }
        }
    }

    private async void RefreshOperateCharacter(RefreshOperateCharacter refreshOperateCharacter)
    {
        if (CharacterManager.instance.IsTempCharacter(refreshOperateCharacter.characterId))
        {
            return;
        }
        int oldCount = characters.Count;
        if (refreshOperateCharacter.join)
        {
            //if (!PastureManager.instance.CheckAnimal(refreshOperateCharacter.characterId))
            {
                characters.Add(refreshOperateCharacter.characterId);
            }
           
        }
        else
        {
            characters.Remove(refreshOperateCharacter.characterId);
            var simpleTalkPanel = await UIManager.instance.GetGamePanel<SimpleTalkPanel>();
            if (simpleTalkPanel != null) simpleTalkPanel.TryClose(refreshOperateCharacter.characterId);
        }
        if (characters.Count != oldCount)
        { 
            nowCharacters.Clear();
            using (var e = characters.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    nowCharacters.Add(new MyInt { value = e.Current });
                }
            }
            switch (characters.Count)
            {
                case 0:
                    characterButtonParent.gameObject.SetActive(false);
                    CharacterButtonReference.transform.localScale = Vector3.zero;
                    MultiCharacterButton.transform.localScale = Vector3.zero;
                    CharacterButtonReference.enabled = false;
                    break;
                case 1:
                    characterButtonParent.gameObject.SetActive(false);
                    CharacterButtonReference.transform.localScale = Vector3.one;
                    CharacterButtonReference.enabled = true;
                    MultiCharacterButton.transform.localScale = Vector3.zero;
                    CharacterButtonReference.InitData(nowCharacters[0], SelectAction);
                    break;
                default:
                    if (characterButtonParent.gameObject.activeSelf)
                    {
                        characterButtons.InitListData(nowCharacters, SelectAction);
                    }
                    else
                    {
                        CharacterButtonReference.transform.localScale = Vector3.zero;
                        MultiCharacterButton.transform.localScale = Vector3.one;
                    }
                    break;
            } 
        }
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        MultiCharacterButton = FindChildGameObject<Button>("MultiCharacterButton");
        CharacterButtonReference = FindChildGameObject<CharacterButtonReference>("CharacterButton");
        characterButtonParent = FindChildGameObject("CharacterButtonList");
    }
    public override Task InitData(string dataKey)
    {
        CharacterButtonReference.transform.localScale = Vector3.zero;
        MultiCharacterButton.transform.localScale = Vector3.zero;
        CharacterButtonReference.enabled = false;
        characterButtonParent.gameObject.SetActive(false);
        return base.InitData(dataKey);
    }
    public override void InitReferenceData(MyListInt v)
    {
        base.InitReferenceData(v);
        CharacterButtonReference.transform.localScale = Vector3.zero;
        MultiCharacterButton.transform.localScale = Vector3.zero;
        CharacterButtonReference.enabled = false;
        characterButtonParent.gameObject.SetActive(false);
    }

}
