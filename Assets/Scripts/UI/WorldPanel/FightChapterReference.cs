using System.Threading.Tasks;
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
    private TextMeshProUGUI chapterNameText0, chapterNameText1;

    [SerializeField]
    private Toggle selected;

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

    public bool TrySelect()
    {
        if (activeObj.localScale != Vector3.zero)
        {
            selected.SetIsOnWithoutNotify(true);
            if (SelectAction != null)
            {
                SelectAction(data, true);
            }
            return true;
        }
        return false;
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

    public override async Task InitData(UIFightChapterData t, SelectAction<UIFightChapterData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await base.InitData(t, SelectAction, toggleGroup);
        FightChapter fightChapter = ExploreManager.instance.GetFightChapter(data.fightChapterId);
        if (data.season)
        {
            activeObj.localScale = Vector3.one;
            unActiveObj.localScale = Vector3.zero;
            selected.interactable = true;
            if (
#if UNITY_EDITOR
                !GameController.instance.test &&
#endif
                !fightChapter.open)
            {
                selected.interactable = false;
                //activeObj.localScale = Vector3.zero;
                //unActiveObj.localScale = Vector3.one;
            }
        }
        else
        {
            activeObj.localScale = Vector3.zero;
            unActiveObj.localScale = Vector3.one;
        }
        chapterNameText0.SetSWText(fightChapter.mapName);
        chapterNameText1.SetSWText(fightChapter.mapName);
    }
}

public struct UIFightChapterData : IReferenceData
{
    public int fightChapterId;
    public bool season;
}