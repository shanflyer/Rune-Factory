using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameGuideManager : Singleton<GameGuideManager>
{
    private Dictionary<int, Selectable> guidSelectableDic = new Dictionary<int, Selectable>();
    private HashSet<int> endGuide = new HashSet<int>();

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

    public async Task<GameGuideFilmData> GetGameGuideFilmData()
    {
        if (endGuideFilmIndex >= 0)
        {
            return await GameDataManager.instance.GetAsyncData<GameGuideFilmData>(endGuideFilmIndex);
        }
        return null;
    }

    public async void SetGameGuidFilmDataAction(int characterId, string worldName)
    {
        var data = await GetGameGuideFilmData();

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
        if (!data.displayCharacter)
        {
            SetCharacterStopCreate setCharacterStopCreate = new SetCharacterStopCreate
            {
                hide = !data.displayCharacter
            };
            GameActionManager.instance.QueueAction(setCharacterStopCreate);
        }
        else
        {
            SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
            {
                characterId = characterId,
                coordinate = data.fixedMap
            };
            GameActionManager.instance.QueueAction(setCharacterCoordinate);
        }
        GameTimerController.instance.DelayAction(1000, () =>
        {
            //GameActionDataManager.instance.Action(data.beforeEventId);
        });
    }

    public override void Init()
    {
        base.Init();
        Selectable.setIntAction = SetIntAction;
        Selectable.removeIntAction = RemoveIntAction;
        endGuide.Clear();

        GameActionManager.instance.AddListener<GameGuideAction>(GameGuideAction);
        GameActionManager.instance.AddListener<CheckGameGuideAction>(CheckGameGuideAction);
        GameActionManager.instance.AddListener<SaveGuideFilmIndexAction>(SaveGuideFilmIndexAction);
        GameActionManager.instance.AddListener<CheckGuideFilmIndex>(CheckGuideFilmIndex);
    }

    private void CheckGuideFilmIndex(CheckGuideFilmIndex checkGuideFilmIndex)
    {
        if (checkGuideFilmIndex.setResult != null)
        {
            checkGuideFilmIndex.setResult(endGuideFilmIndex > checkGuideFilmIndex.id);
        }
    }

    private void SaveGuideFilmIndexAction(SaveGuideFilmIndexAction saveGuideFilmIndexAction)
    {
        endGuideFilmIndex = saveGuideFilmIndexAction.id;
    }

    private void CheckGameGuideAction(CheckGameGuideAction CheckGameGuideAction)
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

    private async void SetIntAction(int id, Selectable selectable)
    {
        guidSelectableDic[id] = selectable;

        // if (id== nowGuideSelectableId)
        {
            // Debug.Log("等待 guid");
            //InitShowGuide();
        }
    }

    private void RemoveIntAction(int id, Selectable selectable)
    {
        guidSelectableDic.Remove(id);
    }

    private async void GameGuideAction(GameGuideAction gameGuideAction)
    {
        nowGameGuideData = await GameDataManager.instance.GetAsyncData<GameGuideData>(gameGuideAction.guidKey);
        nowGameGuideData.Zero();
        ShowGuide();
    }

    private void ShowGuide()
    {
        if (nowGameGuideData == null)
        {
            return;
        }
        if (nowGameGuideData.GetGuidStepData(out guidStepData))
        {
            UIManager.instance.ShowGamePanel<GameGuidePanel, GuidStepData>(guidStepData);
        }
        else
        {
            endGuide.Add(nowGameGuideData.id);
            nowGameGuideData = null;
            guidStepData = null;
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    private GameGuideData nowGameGuideData;
    private GuidStepData guidStepData;

    private int nowGuideSelectableId;

    public void GuideButtonAction(PointerEventData eventData)
    {
        Debug.Log($"指引点击00!!--{eventData.button}");
        if (guidStepData.waitTime > 0)
        {
            GameTimerController.instance.DelayAction(guidStepData.waitTime, () =>
            {
                InitShowGuide();
            });
        }
        else
        {
            InitShowGuide();
        }
    }

    public void GuideButtonAction()
    {
        if (guidStepData.waitTime > 0)
        {
            GameTimerController.instance.DelayAction(guidStepData.waitTime, () =>
            {
                InitShowGuide();
            });
        }
        else
        {
            InitShowGuide();
        }
    }

    private void InitShowGuide()
    {
        if (guidSelectableDic.TryGetValue(nowGuideSelectableId, out var selectable))
        {
            selectable.HideSelected = true;
            //Debug.Log($"指引点击01!!-");
            if (selectable is Button button)
            {
                button.OnPointerClick();
            }
            else if (selectable is Toggle toggle)
            {
                toggle.OnPointerClick();
            }

            ShowGuide();
        }
        else
        {
            Debug.Log("指引未命中！");
        }
    }

    public bool GetSelectableSize(int guid, out Vector3 pos, out Vector2 size)
    {
        pos = Vector3.zero;
        size = Vector3.zero;
        if (guidSelectableDic.TryGetValue(guid, out var selectable))
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