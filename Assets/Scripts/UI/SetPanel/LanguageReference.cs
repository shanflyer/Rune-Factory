using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageReference : UIObjReference<LanguageData>
{
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    TextMeshProUGUI text;
    public override void InitChildObjData()
    {
        base.InitChildObjData();
        toggle = GetComponent<Toggle>();
        text = FindChildGameObject<TextMeshProUGUI>("Text");

    }
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value)
            {
                if (SelectAction != null)
                {
                    SelectAction(data);
                }
            }
        });
        
    }
    public override void InitData(LanguageData t, SelectAction<LanguageData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        text.text = t.ShowName;
        text.isRightToLeftText = t.rightStart;
        toggle.SetIsOnWithoutNotify(LanguageManage.nowLanguage == t.languageType);
        base.InitData(t, SelectAction, toggleGroup);
    }
    public override void Close()
    {
        base.Close();
    }
}
