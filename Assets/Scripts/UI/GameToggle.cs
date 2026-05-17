using UnityEngine;
using UnityEngine.UI;

public class GameToggle : Toggle, IGuideSelectable
{
    [SerializeField] int m_GameSetUid;
    [SerializeField] int m_GameGuid;
    [SerializeField] bool m_GameHideSelected;

    bool IGuideSelectable.HideSelected
    {
        get => m_GameHideSelected;
        set => m_GameHideSelected = value;
    }

    public new void InitListSelectable(int index)
    {
        UGUISelectableUtility.InitListSelectable(this, m_GameSetUid, ref m_GameGuid, index);
    }

    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(PlayAudioWhenOn);
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

    void PlayAudioWhenOn(bool isOn)
    {
        if (isOn)
            UGUISelectableUtility.PlayTagAudio(this);
    }
}
