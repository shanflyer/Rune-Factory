using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SerializeField]
public class FightChapterReference : UIObjReference<UIFightChapterData>
{
    [SerializeField]
    private Transform activeObj;
    [SerializeField]
    private Transform unActiveObj;
    [SerializeField]
    TextMeshProUGUI chapterNameText0, chapterNameText1;
    [SerializeField]
    Toggle selected;
    private void Awake()
    {
        selected.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, value);
            }
        });
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        activeObj = FindChildGameObject("Active");
        unActiveObj = FindChildGameObject("UnActive");
        chapterNameText0 = FindChildGameObject<TextMeshProUGUI>("chapterNameText0");
        chapterNameText1 = FindChildGameObject<TextMeshProUGUI>("chapterNameText1");
        selected = FindChildGameObject<Toggle>("Active");
    }

    public override void InitData(UIFightChapterData t, SelectAction<UIFightChapterData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        FightChapter fightChapter = ExploreManager.instance.GetFigehtChapter(data.fightChapterId);
        if (data.season)
        {
            activeObj.localScale = Vector3.one;
            unActiveObj.localScale = Vector3.zero; 
            if (!fightChapter.open)
            {
                activeObj.localScale = Vector3.zero;
                unActiveObj.localScale = Vector3.one;
            }
        }
        else
        {
            activeObj.localScale = Vector3.zero;
            unActiveObj.localScale = Vector3.one;
        } 
        chapterNameText0.text = chapterNameText1.text = fightChapter.mapName.ToString(); 
    }
}

public struct UIFightChapterData : IReferenceData
{
    public int fightChapterId;
    public bool season;
}