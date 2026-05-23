using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameButton : Button, IGuideSelectable
{
    [SerializeField] int m_GameSetUid;
    [SerializeField] int m_GameGuid;
    [SerializeField] bool m_GameHideSelected;

    bool IGuideSelectable.HideSelected
    {
        get => m_GameHideSelected;
        set => m_GameHideSelected = value;
    }

    public void InitListSelectable(int index)
    {
        UGUISelectableUtility.InitListSelectable(this, m_GameSetUid, ref m_GameGuid, index);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UGUISelectableUtility.Register(this, m_GameSetUid, ref m_GameGuid);
    }

    protected override void OnDisable()
    {
        UGUISelectableUtility.Unregister(this, m_GameGuid);
        base.OnDisable();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        UGUISelectableUtility.PlayTagAudio(this);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        UGUISelectableUtility.PlayTagAudio(this);
    }
}
