using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldPanel : GamePanel<MyInt>
{ 
    [SerializeField]
    List<FightChapterReference> seasonFightChapterList;
    [SerializeField]
    List<Season> seasons;
    [SerializeField]
    Transform itemParent;
    [SerializeField]
    FightMapItemReference itemReference;
    [SerializeField]
    TextMeshProUGUI exploreValue;
    [SerializeField]
    Button exploreButton;
    [SerializeField]
    Button closeButton;

    DisplayList<FightMapItemReference, MapItemReferenceData> fightMapItems;
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

        seasons = new List<Season>();
        seasonFightChapterList = new List<FightChapterReference>();
        foreach (var s in Enum.GetValues(typeof(Season)))
        {
            int index = (int)s - 1;
            if (index >= 0)
            {
                Season season = (Season)s;
                Transform child = FindChildGameObject(season.ToString());
                foreach(Transform _child in child)
                {
                    FightChapterReference fightChapterReference = _child.GetComponent<FightChapterReference>();
                    seasonFightChapterList.Add(fightChapterReference);
                    seasons.Add(season);
                }
                //seasonWorlds.Add(child);
            }
        }
       
    }
    int selectFightChapterId;

    void ExploreMap()
    {
        EnterChapter enterChapter = new EnterChapter
        {
            id = selectFightChapterId
        };
        GameActionManager.instance.QueueAction(enterChapter);
    }
    async void SelectFightChapter(UIFightChapterData uIFightChapterData,bool selected)
    {
        if (selected)
        {
            selectFightChapterId = uIFightChapterData.fightChapterId;
            var chapterData = ExploreManager.instance.GetFigehtChapter(uIFightChapterData.fightChapterId);

            exploreValue.text = $"Ì½Ë÷¶È:{chapterData.completeValue}%";

            List<MapItemReferenceData> list = new List<MapItemReferenceData>();
            for (int i = 0; i < chapterData.haveItems.Length; i++)
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
            fightMapItems.InitListData(list);
            exploreButton.interactable = chapterData.open;
        }
        else
        {
            if (selectFightChapterId == uIFightChapterData.fightChapterId)
            {
                selectFightChapterId = -1;
            }
            fightMapItems.ClearAll();
            exploreValue.text = $"Ì½Ë÷¶È:--%";
            exploreButton.interactable = false;
        }  
    }
   
    public override async void InitReferenceData(MyInt v)
    {
        base.InitReferenceData(v);

        var fightMapDatas=await GameDataManager.instance.GetAllAsyncData<FightMapData>();

        Dictionary<Season, Queue<int>> seasonQueue = new Dictionary<Season, Queue<int>>();
        Queue<int> seasonIndex = new Queue<int>();
        for (int i = 0; i < seasons.Count; i++)
        {
            Season season = seasons[i];
            if(!seasonQueue.TryGetValue(season,out seasonIndex))
            {
                seasonIndex = new Queue<int>();
                seasonQueue.Add(season,seasonIndex);
            }
            seasonIndex.Enqueue(i);
        }

        for(int i = 0; i < fightMapDatas.Count; i++)
        {
            var fightMapData = fightMapDatas[i];
            if (fightMapData.season != Season.Default)
            {
                if (seasonQueue.TryGetValue(fightMapData.season, out var ints))
                {
                    int index = ints.Dequeue();
                    seasonFightChapterList[i].InitData(new UIFightChapterData
                    {
                        fightChapterId = fightMapData.id,
                        season = fightMapData.season == (Season)v.value
                    }, SelectFightChapter) ;
                }
            }
        }

        for(int i = 0; i < seasons.Count; i++)
        {
            Season season = seasons[i];
            FightChapterReference fightChapterReference = seasonFightChapterList[i];

        }
    }
    public override Task InitData(string dataKey)
    {
        return base.InitData(dataKey);
    }
}
