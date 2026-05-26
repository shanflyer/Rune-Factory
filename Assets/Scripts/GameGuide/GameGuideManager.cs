using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameGuideManager:Singleton<GameGuideManager>
{
    Dictionary<int, Selectable> guidSelectableDic = new Dictionary<int, Selectable>();
    HashSet<int> endGuide = new HashSet<int>();
    ActiveGuideSession activeGuideSession;
    int guideSessionVersion;

    class ActiveGuideSession
    {
        public int Version;
        public GameGuideData Data;
        public int StepIndex;
        public GuidStepData CurrentStep;
        public int WaitingSelectableId;
        public bool IsAdvancing;
    }

    public int endGuideFilmIndex
    {
        get
        {
            return GameDataSaveManager.instance.UserGameSaveData.endGuideFilmIndex;
        }
        set
        {
            GameDataSaveManager.instance.UserGameSaveData.endGuideFilmIndex = value;
        }
    }

    public bool IsEndGuide()
    {
        return endGuideFilmIndex >= GameDataManager.instance.GlobalData.endGuideIndex;
    }
    public async Task<GameGuideFilmData> GetGameGuideFilmData()
    {
        if (endGuideFilmIndex >= 0)
        {
            return await GameDataManager.instance.GetAsyncData<GameGuideFilmData>(endGuideFilmIndex);
        }
        return null;
    }
    public void SetGameGuidFilmDataAction(int characterId,string worldName)
    {
        AsyncTaskRunner.Run(() => SetGameGuidFilmDataActionAsync(characterId, worldName), nameof(SetGameGuidFilmDataAction));
    }

    public async System.Threading.Tasks.Task SetGameGuidFilmDataActionAsync(int characterId,string worldName)
    {
        var data=await GetGameGuideFilmData();
        if (data == null)
        {
            Debug.LogWarning($"Game guide film data not found: {endGuideFilmIndex}");
            return;
        }

        GameActionManager.instance.QueueAction(new ChangeWorld
        {
            worldName = worldName,
            displayMap = data.fixedMap.z
        }, true);

        SetFixedTime setFixedTime = new SetFixedTime
        {
            date = data.fixedDate,
            hour = data.fixedHour,
        };
        GameActionManager.instance.QueueAction(setFixedTime);
        var setCharacterCoordinate = new SetCharacterCoordinate
        {
            characterId = characterId,
            coordinate = data.fixedMap
        };
        GameActionManager.instance.QueueAction(setCharacterCoordinate);

        var setCharacterStopCreate = new SetCharacterStopCreate
        {
            hide = !data.displayCharacter
        };
        GameActionManager.instance.QueueAction(setCharacterStopCreate);

        GameTimerController.instance.DelayAction(1000, () =>
        {
            GameActionDataManager.instance.Action(data.beforeEventId);
        });

    }
    public override void Init()
    {
        base.Init();
        SelectableGuideRegistry.SetIntAction = SetIntAction;
        SelectableGuideRegistry.RemoveIntAction = RemoveIntAction;
        endGuide.Clear();

        GameActionManager.instance.AddAsyncListener<GameGuideAction>(GameGuideActionAsync, nameof(GameGuideAction));
        GameActionManager.instance.AddListener<CheckGameGuideAction>(CheckGameGuideAction);
        GameActionManager.instance.AddListener<SaveGuideFilmIndexAction>(SaveGuideFilmIndexAction);
        GameActionManager.instance.AddListener<CheckGuideFilmIndex>(CheckGuideFilmIndex);
    }
    void CheckGuideFilmIndex(CheckGuideFilmIndex checkGuideFilmIndex)
    {
        if (checkGuideFilmIndex.setResult != null)
        {
            checkGuideFilmIndex.setResult(endGuideFilmIndex > checkGuideFilmIndex.id);
        }
    }
    void SaveGuideFilmIndexAction(SaveGuideFilmIndexAction saveGuideFilmIndexAction)
    {
        endGuideFilmIndex = saveGuideFilmIndexAction.id;
    }
    void CheckGameGuideAction(CheckGameGuideAction CheckGameGuideAction)
    {
        if (CheckGameGuideAction.setResult != null)
        {
            if (CheckGameGuideAction.isEnd)
            {
                CheckGameGuideAction.setResult(endGuide.Contains(CheckGameGuideAction.guidKey));
            }
            else
            {
                CheckGameGuideAction.setResult(!endGuide.Contains(CheckGameGuideAction.guidKey));
            }

        }
    }

    private void SetIntAction(int id, Selectable selectable)
    {
        guidSelectableDic[id] = selectable;
        if (activeGuideSession != null &&
            activeGuideSession.WaitingSelectableId == id &&
            activeGuideSession.CurrentStep != null &&
            activeGuideSession.CurrentStep.selectableId == id)
        {
            activeGuideSession.WaitingSelectableId = 0;
            TryShowCurrentStep(activeGuideSession, nameof(SetIntAction));
        }

    }
    void RemoveIntAction(int id,Selectable selectable)
    {
        bool removedCurrent = false;
        if (guidSelectableDic.TryGetValue(id, out var current) && current == selectable)
        {
            guidSelectableDic.Remove(id);
            removedCurrent = true;
        }

        if (hiddenSelectable == selectable)
        {
            RestoreHiddenSelectable();
        }

        if (removedCurrent &&
            activeGuideSession != null &&
            activeGuideSession.CurrentStep != null &&
            activeGuideSession.CurrentStep.selectableId == id)
        {
            activeGuideSession.WaitingSelectableId = id;
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    async System.Threading.Tasks.Task GameGuideActionAsync(GameGuideAction gameGuideAction)
    {
        int sessionVersion = ++guideSessionVersion;
        ClearActiveGuide(true);

        var gameGuideData = await GameDataManager.instance.GetAsyncData<GameGuideData>(gameGuideAction.guidKey);
        if (sessionVersion != guideSessionVersion)
        {
            return;
        }

        if (gameGuideData == null)
        {
            Debug.LogWarning($"Game guide data not found: {gameGuideAction.guidKey}");
            return;
        }

        activeGuideSession = new ActiveGuideSession
        {
            Version = sessionVersion,
            Data = gameGuideData
        };
        AdvanceToNextStep(activeGuideSession);
    }

    void AdvanceToNextStep(ActiveGuideSession session)
    {
        if (!IsActiveSession(session))
        {
            return;
        }

        session.WaitingSelectableId = 0;
        if(session.Data.GetGuidStepData(session.StepIndex, out var stepData))
        {
            session.StepIndex++;
            session.CurrentStep = stepData;
            TryShowCurrentStep(session, nameof(AdvanceToNextStep));
        }
        else
        {
            CompleteGuide(session);
        }
    }

    void TryShowCurrentStep(ActiveGuideSession session, string sourceName)
    {
        if (!IsActiveSession(session) || session.CurrentStep == null)
        {
            return;
        }

        int selectableId = session.CurrentStep.selectableId;
        if (guidSelectableDic.ContainsKey(selectableId))
        {
            session.WaitingSelectableId = 0;
            AsyncTaskRunner.Run(ShowGuidePanelAsync(session.Version, session.CurrentStep, sourceName), sourceName);
        }
        else
        {
            session.WaitingSelectableId = selectableId;
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    async Task ShowGuidePanelAsync(int sessionVersion, GuidStepData stepData, string sourceName)
    {
        if (!IsActiveSession(sessionVersion, stepData))
        {
            return;
        }

        await UIManager.instance.ShowGamePanel<GameGuidePanel, GuidStepData>(stepData);
        if (IsActiveSession(sessionVersion, stepData))
        {
            return;
        }

        var currentSession = activeGuideSession;
        if (currentSession != null && currentSession.CurrentStep != null)
        {
            TryShowCurrentStep(currentSession, sourceName);
        }
        else
        {
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    void CompleteGuide(ActiveGuideSession session)
    {
        if (!IsActiveSession(session))
        {
            return;
        }

        session.Data.TriggerEndAction();
        endGuide.Add(session.Data.id);
        ClearActiveGuide(true);
    }

    public void GuideButtonAction()
    {
        AdvanceFromGuideButton();
    }

    void AdvanceFromGuideButton()
    {
        var session = activeGuideSession;
        if (!IsActiveSession(session) || session.CurrentStep == null || session.IsAdvancing)
        {
            return;
        }

        int sessionVersion = session.Version;
        int selectableId = session.CurrentStep.selectableId;
        if (!guidSelectableDic.TryGetValue(selectableId, out var selectable))
        {
            session.WaitingSelectableId = selectableId;
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
            return;
        }

        session.IsAdvancing = true;
        SetHiddenSelectable(selectable);
        try
        {
            selectable.InvokeClick();
        }
        finally
        {
            RestoreHiddenSelectable();
            if (IsActiveSession(sessionVersion))
            {
                session.IsAdvancing = false;
            }
        }

        if (IsActiveSession(sessionVersion))
        {
            AdvanceToNextStep(session);
        }
    }

    bool IsActiveSession(ActiveGuideSession session)
    {
        return session != null && activeGuideSession == session && session.Version == guideSessionVersion;
    }

    bool IsActiveSession(int sessionVersion)
    {
        return activeGuideSession != null && activeGuideSession.Version == sessionVersion && guideSessionVersion == sessionVersion;
    }

    bool IsActiveSession(int sessionVersion, GuidStepData stepData)
    {
        return IsActiveSession(sessionVersion) && activeGuideSession.CurrentStep == stepData;
    }

    void ClearActiveGuide(bool closePanel)
    {
        activeGuideSession = null;
        RestoreHiddenSelectable();
        if (closePanel)
        {
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    Selectable hiddenSelectable;
    void SetHiddenSelectable(Selectable selectable)
    {
        RestoreHiddenSelectable();
        hiddenSelectable = selectable;
        hiddenSelectable.SetHideSelected(true);
    }

    void RestoreHiddenSelectable()
    {
        if (hiddenSelectable != null)
        {
            hiddenSelectable.SetHideSelected(false);
            hiddenSelectable = null;
        }
    }

    public bool GetSelectRectTransform(int guid, out RectTransform guidRect)
    {
        if (guidSelectableDic.TryGetValue(guid, out var selectable))
        {
            guidRect = selectable.transform as RectTransform;

            return true;
        }

        guidRect = null;
        return false;
    }
    public bool GetSelectableSize(int guid,out Vector3 pos,out Vector2 size)
    {
        pos = Vector3.zero;
        size = Vector3.zero;
        if(guidSelectableDic.TryGetValue(guid,out var selectable))
        {
            RectTransform rectTransform = selectable.transform as RectTransform;
            pos = rectTransform.position;
            size = rectTransform.sizeDelta;
            return true;
        }
        return false;
    }
}
