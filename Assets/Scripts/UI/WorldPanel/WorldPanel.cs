using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldPanel : GamePanel<MyInt>
{
    [SerializeField]
    private List<FightChapterReference> seasonFightChapterList;

    [SerializeField]
    private List<Season> seasons;

    [SerializeField]
    private Transform itemParent;

    [SerializeField]
    private FightMapItemReference itemReference;

    [SerializeField]
    private TextMeshProUGUI exploreValue;
    [SerializeField]
    private TextMeshProUGUI infoText;
    [SerializeField]
    private Button exploreButton;

    [SerializeField]
    private Button closeButton;

    private DisplayList<FightMapItemReference, MapItemReferenceData> fightMapItems;

    protected override void Awake()
    {
        base.Awake();
        fightMapItems = new DisplayList<FightMapItemReference, MapItemReferenceData>(itemReference, itemParent);
        exploreButton.onClick.AddListener(ExploreMap);
        closeButton.onClick.AddListener(Close);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

        itemParent = FindChildGameObject("ItemParent");
        itemReference = FindChildGameObject<FightMapItemReference>("itemReference");
        exploreValue = FindChildGameObject<TextMeshProUGUI>("ExplorerValue");
        exploreButton = FindChildGameObject<Button>("ExplorerButton");
        closeButton = FindChildGameObject<Button>("Close");
        infoText = FindChildGameObject<TextMeshProUGUI>("Info");
        seasons = new List<Season>();
        seasonFightChapterList = new List<FightChapterReference>();
        foreach (var s in Enum.GetValues(typeof(Season)))
        {
            int index = (int)s - 1;
            if (index >= 0)
            {
                Season season = (Season)s;
                Transform child = FindChildGameObject(season.ToString());
                foreach (Transform _child in child)
                {
                    FightChapterReference fightChapterReference = _child.GetComponent<FightChapterReference>();
                    seasonFightChapterList.Add(fightChapterReference);
                    seasons.Add(season);
                }
                //seasonWorlds.Add(child);
            }
        }
    }

    private int selectFightChapterId;
    private FightChapter chapterData;
    private void ExploreMap()
    {
        if (chapterData.fightMapData.checkBeforeChapter != 0)
        {
            var beforeChapter = ExploreManager.instance.GetFightChapter(chapterData.fightMapData.checkBeforeChapter);
            if (beforeChapter != null && !beforeChapter.completed)
            {
                InformationController.instance.AddInformation(string.Format(LanguageManage.SwitchStr("需要先探索完成{0},才能解锁！"), LanguageManage.SwitchStr(beforeChapter.fightMapData.mapName)), true, true);
                return;
            }
        }

        if (CharacterManager.instance.controllerCharacter.CharacterProperty.Power < GameCommon.exploreCostPower)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("体力不足，无法进行探索！"), true, true);
            return;
        }

        EnterChapter enterChapter = new EnterChapter
        {
            id = selectFightChapterId
        };
        GameActionManager.instance.QueueAction(enterChapter);
        Close();
    }

    private void SelectFightChapter(UIFightChapterData uIFightChapterData, int index, bool selected)
    {
        RunLifecycleTask(token => SelectFightChapterAsync(uIFightChapterData, index, selected, token), nameof(SelectFightChapter));
    }

    private async System.Threading.Tasks.Task SelectFightChapterAsync(UIFightChapterData uIFightChapterData, int index, bool selected, CancellationToken cancellationToken)
    {
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        if (selected)
        {
            selectFightChapterId = uIFightChapterData.fightChapterId;
            chapterData = ExploreManager.instance.GetFightChapter(uIFightChapterData.fightChapterId);
            infoText.SetSWText(chapterData.fightMapData.info);
            exploreValue.SetSWText("探索度:{0}%", chapterData.completeValue);

            List<MapItemReferenceData> list = new List<MapItemReferenceData>();
            for (int i = 0; i < chapterData.haveItems.Count; i++)
            {
                int itemId = chapterData.haveItems[i];
                bool open = chapterData.findItems.Contains(itemId);
                MapItemReferenceData mapItemReferenceData = new MapItemReferenceData
                {
                    itemData = chapterData.haveItems[i],
                    open = open
                };
                list.Add(mapItemReferenceData);
            }
            await fightMapItems.InitListData(list, cancellationToken: cancellationToken);
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }
            exploreButton.interactable =
#if UNITY_EDITOR
                GameController.instance.test ? true :
#endif
                chapterData.open;
        }
        else
        {
            if (selectFightChapterId == uIFightChapterData.fightChapterId)
            {
                selectFightChapterId = -1;
            }
            fightMapItems.ClearAll();
            exploreValue.SetSWText("探索度:{0}%","--");
            exploreButton.interactable = false;
        }
    }

    private void RefreshUI(Season selectSeason)
    {
        RunLifecycleTask(token => RefreshUIAsync(selectSeason, token), nameof(RefreshUI));
    }

    private async System.Threading.Tasks.Task RefreshUIAsync(Season selectSeason, CancellationToken cancellationToken)
    {
        var fightMapDatas = await GameDataManager.instance.GetAllAsyncData<FightMapData>();
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        Dictionary<Season, Queue<int>> seasonQueue = new Dictionary<Season, Queue<int>>();
        Queue<int> seasonIndex = new Queue<int>();
        for (int i = 0; i < seasons.Count; i++)
        {
            Season season = seasons[i];
            if (!seasonQueue.TryGetValue(season, out seasonIndex))
            {
                seasonIndex = new Queue<int>();
                seasonQueue.Add(season, seasonIndex);
            }
            seasonIndex.Enqueue(i);
        }

        for (int i = 0; i < fightMapDatas.Count; i++)
        {
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

            var fightMapData = fightMapDatas[i];
            if (string.IsNullOrEmpty(fightMapData.fightMapObjName))
            {
                continue;
            }
            if (fightMapData.season != Season.Default)
            {
                if (seasonQueue.TryGetValue(fightMapData.season, out var ints))
                {
                    int index = ints.Dequeue();
                    var data = new UIFightChapterData
                    {
                        fightChapterId = fightMapData.id,
                        season =
#if UNITY_EDITOR
                       GameController.instance.test ? true :
#endif
                        fightMapData.season == selectSeason
                    };
                    await seasonFightChapterList[index].InitData(data, SelectFightChapter, null, cancellationToken);
#if UNITY_EDITOR
                    if (GameController.instance.test)
                    {
                       // SelectFightChapter(data, true);

                    }
                    else
#endif
                    if (fightMapData.season == selectSeason && fightMapData.isOpen)
                    {
                       // SelectFightChapter(data, true);
                    }
                }
            }
        }
        for(int i = 0; i < seasonFightChapterList.Count; i++)
        {
            if (seasonFightChapterList[i].TrySelect())
            {
                break;
            }
        }

        for (int i = 0; i < seasons.Count; i++)
        {
            Season season = seasons[i];
            FightChapterReference fightChapterReference = seasonFightChapterList[i];
        }
    }

    public override void InitReferenceData(MyInt v)
    {
        base.InitReferenceData(v);
        RefreshUI((Season)v.value);
    }

    public override Task InitData(string dataKey)
    {
        try
        {
            int index = int.Parse(dataKey);
            RefreshUI((Season)index);

        }
        catch { }
        return base.InitData(dataKey);
    }
}
