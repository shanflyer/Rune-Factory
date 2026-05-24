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
    private bool actionListenersRegistered;
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

                // 角色按钮刷新绑定当前面板生命周期，关闭后旧列表初始化不再写回 UI。
                RunLifecycleTask(token => characterButtons.InitListData(nowCharacters, SelectAction, cancellationToken: token), nameof(Awake));
            }
        });
    }

    public override void OnEnable()
    {
        base.OnEnable();
        RegisterActionListeners();
    }

    public override void OnDisable()
    {
        UnregisterActionListeners();
        base.OnDisable();
    }

    private void RegisterActionListeners()
    {
        if (actionListenersRegistered || SingletonType.Cleared)
        {
            return;
        }

        GameActionManager.instance.AddListener<RefreshOperateCharacters>(RefreshOperateCharacters);
        GameActionManager.instance.AddListener<RefreshOperateCharacter>(RefreshOperateCharacter);
        actionListenersRegistered = true;
    }

    private void UnregisterActionListeners()
    {
        if (!actionListenersRegistered || SingletonType.Cleared || !GameActionManager.HasInstance)
        {
            return;
        }

        // 面板隐藏时主动解绑，避免再次打开后同一对象重复响应角色操作刷新。
        GameActionManager.instance.RemoveListener<RefreshOperateCharacters>(RefreshOperateCharacters);
        GameActionManager.instance.RemoveListener<RefreshOperateCharacter>(RefreshOperateCharacter);
        actionListenersRegistered = false;
    }

    List<MyInt> nowCharacters = new List<MyInt>();
    HashSet<int> characters = new HashSet<int>();
    void SelectAction(MyInt seletCharacter, int index, bool selected = true)
    {
        RunLifecycleTask(token => SelectActionAsync(seletCharacter, index, selected, token), nameof(SelectAction));
    }

    async System.Threading.Tasks.Task SelectActionAsync(MyInt seletCharacter, int index, bool selected, System.Threading.CancellationToken cancellationToken)
    {
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

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
                if (ShouldStopLifecycleTask(cancellationToken))
                {
                    return;
                }
            }


        }
    }

    private void RefreshOperateCharacters(RefreshOperateCharacters refreshOperateCharacters)
    {
        RunLifecycleTask(token => RefreshOperateCharactersAsync(refreshOperateCharacters, token), nameof(RefreshOperateCharacters));
    }

    private async System.Threading.Tasks.Task RefreshOperateCharactersAsync(RefreshOperateCharacters refreshOperateCharacters, System.Threading.CancellationToken cancellationToken)
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
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

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
                    await CharacterButtonReference.InitData(nowCharacters[0], SelectAction, null, cancellationToken);
                    if (ShouldStopLifecycleTask(cancellationToken))
                    {
                        return;
                    }

                    CharacterButtonReference.enabled = true;
                    break;
                default:
                    if (characterButtonParent.gameObject.activeSelf)
                    {
                        await characterButtons.InitListData(nowCharacters, SelectAction, cancellationToken: cancellationToken);
                        if (ShouldStopLifecycleTask(cancellationToken))
                        {
                            return;
                        }
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

    private void RefreshOperateCharacter(RefreshOperateCharacter refreshOperateCharacter)
    {
        RunLifecycleTask(token => RefreshOperateCharacterAsync(refreshOperateCharacter, token), nameof(RefreshOperateCharacter));
    }

    private async System.Threading.Tasks.Task RefreshOperateCharacterAsync(RefreshOperateCharacter refreshOperateCharacter, System.Threading.CancellationToken cancellationToken)
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
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

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
                    await CharacterButtonReference.InitData(nowCharacters[0], SelectAction, null, cancellationToken);
                    if (ShouldStopLifecycleTask(cancellationToken))
                    {
                        return;
                    }
                    break;
                default:
                    if (characterButtonParent.gameObject.activeSelf)
                    {
                        await characterButtons.InitListData(nowCharacters, SelectAction, cancellationToken: cancellationToken);
                        if (ShouldStopLifecycleTask(cancellationToken))
                        {
                            return;
                        }
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
