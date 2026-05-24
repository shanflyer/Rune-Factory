using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameGuideManager:Singleton<GameGuideManager>
{
    Dictionary<int, Selectable> guidSelectableDic = new Dictionary<int, Selectable>();
    HashSet<int> endGuide = new HashSet<int>(); 
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
        if (waitGuide != 0 && waitGuide == id)
        {
            // 引导控件注册是同步入口，面板加载异常统一记录。
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<GameGuidePanel, GuidStepData>(guidStepData), nameof(SetIntAction));
        }
        
    }
    void RemoveIntAction(int id,Selectable selectable)
    {
        guidSelectableDic.Remove(id);
    }

    async System.Threading.Tasks.Task GameGuideActionAsync(GameGuideAction gameGuideAction)
    {
        nowGameGuideData = await GameDataManager.instance.GetAsyncData<GameGuideData>(gameGuideAction.guidKey);
        nowGameGuideData.Zero();
        ShowGuide();
    }
    void ShowGuide()
    {
        waitGuide = 0;
        if (nowGameGuideData == null)
        {
            return;
        }
        if(nowGameGuideData.GetGuidStepData(out guidStepData))
        {
            nowGuideSelectableId = guidStepData.selectableId;
            if (guidSelectableDic.TryGetValue(nowGuideSelectableId, out var selectable))
                AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<GameGuidePanel, GuidStepData>(guidStepData), nameof(ShowGuide));
            else
                waitGuide = nowGuideSelectableId;
        }
        else
        {
            endGuide.Add(nowGameGuideData.id);
            nowGameGuideData = null;
            guidStepData = null;
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    GameGuideData nowGameGuideData;
    GuidStepData guidStepData;


    int nowGuideSelectableId;
 
    public void GuideButtonAction()
    {
        InitShowGuide();
    }

    private int waitGuide;
    void InitShowGuide()
    {
        if (guidSelectableDic.TryGetValue(nowGuideSelectableId, out var selectable))
        { 
            selectable.SetHideSelected(true);
            Debug.Log("指引点击01!!-");
            selectable.InvokeClick();

            ShowGuide();
        }
         
    }

    public bool GetSelectRectTransform(int guid, out RectTransform guidRect)
    {
        if (guidSelectableDic.TryGetValue(guid, out var selectable))
        {
            nowGuideSelectableId = guid;
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
            nowGuideSelectableId = guid;
            RectTransform rectTransform = selectable.transform as RectTransform;
            pos = rectTransform.position;
            size = rectTransform.sizeDelta; 
            return true;
        }
        return false;
    }
}
