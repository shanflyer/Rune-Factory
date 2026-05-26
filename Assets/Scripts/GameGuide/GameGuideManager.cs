using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameGuideManager:Singleton<GameGuideManager>
{
    readonly GameGuideFilmService filmService = new GameGuideFilmService();
    readonly GameGuideUiFlow uiFlow = new GameGuideUiFlow();

    public int endGuideFilmIndex
    {
        get => filmService.EndGuideFilmIndex;
        set => filmService.EndGuideFilmIndex = value;
    }

    public bool IsEndGuide()
    {
        return filmService.IsEndGuide();
    }

    public Task<GameGuideFilmData> GetGameGuideFilmData()
    {
        return filmService.GetGameGuideFilmData();
    }

    public void SetGameGuidFilmDataAction(int characterId,string worldName)
    {
        AsyncTaskRunner.Run(() => filmService.SetGameGuidFilmDataActionAsync(characterId, worldName), nameof(SetGameGuidFilmDataAction));
    }

    public override void Init()
    {
        base.Init();
        SelectableGuideRegistry.SetIntAction = uiFlow.RegisterSelectable;
        SelectableGuideRegistry.RemoveIntAction = uiFlow.UnregisterSelectable;
        uiFlow.ClearCompletedGuides();

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

    void CheckGameGuideAction(CheckGameGuideAction checkGameGuideAction)
    {
        uiFlow.SetCheckResult(checkGameGuideAction);
    }

    Task GameGuideActionAsync(GameGuideAction gameGuideAction)
    {
        return uiFlow.StartGuideAsync(gameGuideAction.guidKey);
    }

    public void GuideButtonAction()
    {
        uiFlow.AdvanceFromGuideButton();
    }

    public bool GetSelectRectTransform(int guid, out RectTransform guidRect)
    {
        return uiFlow.GetSelectRectTransform(guid, out guidRect);
    }

    public bool GetSelectableSize(int guid,out Vector3 pos,out Vector2 size)
    {
        return uiFlow.GetSelectableSize(guid, out pos, out size);
    }
}

class GameGuideFilmService
{
    public int EndGuideFilmIndex
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
        return EndGuideFilmIndex >= GameDataManager.instance.GlobalData.endGuideIndex;
    }

    public async Task<GameGuideFilmData> GetGameGuideFilmData()
    {
        if (EndGuideFilmIndex >= 0)
        {
            return await GameDataManager.instance.GetAsyncData<GameGuideFilmData>(EndGuideFilmIndex);
        }
        return null;
    }

    public async Task SetGameGuidFilmDataActionAsync(int characterId, string worldName)
    {
        var data = await GetGameGuideFilmData();
        if (data == null)
        {
            Debug.LogWarning($"Game guide film data not found: {EndGuideFilmIndex}");
            return;
        }

        GameActionManager.instance.QueueAction(new ChangeWorld
        {
            worldName = worldName,
            displayMap = data.fixedMap.z
        }, true);

        GameActionManager.instance.QueueAction(new SetFixedTime
        {
            date = data.fixedDate,
            hour = data.fixedHour,
        });

        GameActionManager.instance.QueueAction(new SetCharacterCoordinate
        {
            characterId = characterId,
            coordinate = data.fixedMap
        });

        GameActionManager.instance.QueueAction(new SetCharacterStopCreate
        {
            hide = !data.displayCharacter
        });

        GameTimerController.instance.DelayAction(1000, () =>
        {
            GameActionDataManager.instance.Action(data.beforeEventId);
        });
    }
}

class GameGuideUiFlow
{
    readonly Dictionary<int, Selectable> guideSelectableById = new Dictionary<int, Selectable>();
    readonly HashSet<int> completedGuides = new HashSet<int>();

    ActiveGuideSession activeGuideSession;
    Selectable hiddenSelectable;
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

    public void ClearCompletedGuides()
    {
        completedGuides.Clear();
    }

    public void SetCheckResult(CheckGameGuideAction action)
    {
        if (action.setResult == null)
        {
            return;
        }

        bool isCompleted = completedGuides.Contains(action.guidKey);
        action.setResult(action.isEnd ? isCompleted : !isCompleted);
    }

    public void RegisterSelectable(int id, Selectable selectable)
    {
        guideSelectableById[id] = selectable;
        if (activeGuideSession != null &&
            activeGuideSession.WaitingSelectableId == id &&
            activeGuideSession.CurrentStep != null &&
            activeGuideSession.CurrentStep.selectableId == id)
        {
            activeGuideSession.WaitingSelectableId = 0;
            TryShowCurrentStep(activeGuideSession, nameof(RegisterSelectable));
        }
    }

    public void UnregisterSelectable(int id, Selectable selectable)
    {
        bool removedCurrent = false;
        if (guideSelectableById.TryGetValue(id, out var current) && current == selectable)
        {
            guideSelectableById.Remove(id);
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

    public async Task StartGuideAsync(int guideId)
    {
        int sessionVersion = ++guideSessionVersion;
        ClearActiveGuide(true);

        var gameGuideData = await GameDataManager.instance.GetAsyncData<GameGuideData>(guideId);
        if (sessionVersion != guideSessionVersion)
        {
            return;
        }

        if (gameGuideData == null)
        {
            Debug.LogWarning($"Game guide data not found: {guideId}");
            return;
        }

        activeGuideSession = new ActiveGuideSession
        {
            Version = sessionVersion,
            Data = gameGuideData
        };
        AdvanceToNextStep(activeGuideSession);
    }

    public void AdvanceFromGuideButton()
    {
        var session = activeGuideSession;
        if (!IsActiveSession(session) || session.CurrentStep == null || session.IsAdvancing)
        {
            return;
        }

        int sessionVersion = session.Version;
        int selectableId = session.CurrentStep.selectableId;
        if (!guideSelectableById.TryGetValue(selectableId, out var selectable))
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

    public bool GetSelectRectTransform(int guideId, out RectTransform guideRect)
    {
        if (guideSelectableById.TryGetValue(guideId, out var selectable))
        {
            guideRect = selectable.transform as RectTransform;
            return true;
        }

        guideRect = null;
        return false;
    }

    public bool GetSelectableSize(int guideId, out Vector3 pos, out Vector2 size)
    {
        pos = Vector3.zero;
        size = Vector3.zero;
        if(guideSelectableById.TryGetValue(guideId, out var selectable))
        {
            RectTransform rectTransform = selectable.transform as RectTransform;
            pos = rectTransform.position;
            size = rectTransform.sizeDelta;
            return true;
        }
        return false;
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
        if (guideSelectableById.ContainsKey(selectableId))
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
        completedGuides.Add(session.Data.id);
        ClearActiveGuide(true);
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
}
