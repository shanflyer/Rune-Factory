using TMPro;
using UnityEngine;

public class GameTextMeshProUGUI : TextMeshProUGUI
{
    [SerializeField] bool m_GameSwLanguage = true;

    protected override void Awake()
    {
        base.Awake();
        TMPTextLocalization.InitTextState(this);
    }

    protected override void OnEnable()
    {
        if (m_GameSwLanguage)
            this.FixedSwitchString();
        else
            TMPTextLocalization.InitTextState(this);

        base.OnEnable();
    }
}
