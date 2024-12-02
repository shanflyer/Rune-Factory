using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameGuideManager:Singleton<GameGuideManager>
{
    Dictionary<int, Selectable> guidSelectableDic = new Dictionary<int, Selectable>();
    public override void Init()
    {
        base.Init();
        Selectable.setIntAction = SetIntAction;
        Selectable.removeIntAction = RemoveIntAction;

        GameActionManager.instance.AddListener<GameGuideAction>(GameGuideAction);
    }
    void SetIntAction(int id,Selectable selectable)
    {
        guidSelectableDic[id] = selectable;
    }
    void RemoveIntAction(int id,Selectable selectable)
    {
        guidSelectableDic.Remove(id);
    }

    async void GameGuideAction(GameGuideAction gameGuideAction)
    {
        nowGameGuideData = await GameDataManager.instance.GetAsyncData<GameGuideData>(gameGuideAction.guidKey);
        nowGameGuideData.Zero();
        ShowGuide();
    }
    void ShowGuide()
    {
        if(nowGameGuideData.GetGuidStepData(out var guidStepData))
        {
            if (guidStepData.waitTime > 0)
            {
                GameTimerController.instance.DelayAction(guidStepData.waitTime, () =>
                {
                    UIManager.instance.ShowGamePanel<GameGuidePanel, GuidStepData>(guidStepData);
                });
            }
            else
            {
                UIManager.instance.ShowGamePanel<GameGuidePanel, GuidStepData>(guidStepData);
            }
        }
        else
        {
            nowGameGuideData = null;
            UIManager.instance.CloseGamePanel<GameGuidePanel>();
        }
    }

    GameGuideData nowGameGuideData;
    
    int nowGuideSelectableId;
    public void GuideButtonAction(PointerEventData eventData) 
    { 
        if(guidSelectableDic.TryGetValue(nowGuideSelectableId,out var selectable))
        {
            if(selectable is Button button)
            {
                button.OnPointerClick(eventData);
            }else if(selectable is Toggle toggle)
            {
                toggle.OnPointerClick(eventData);
            }
             
            ShowGuide();
        } 
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