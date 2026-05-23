using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectLoadPanel : GamePanel<UserGameSaveDataList>
{
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private SaveReference SaveReference;

    [SerializeField]
    private Button Copy, Delete, Start, Return;

    [SerializeField]
    private Transform SaveDataParent;

    private DisplayList<SaveReference, UserGameSaveData> saveList;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Delete = FindChildGameObject<Button>("DeleteButton");
        Start = FindChildGameObject<Button>("StartButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup = GetComponent<ToggleGroup>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshGameSaveData>(RefreshGameSaveData);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshGameSaveData>(RefreshGameSaveData);
    }

    private void RefreshGameSaveData(RefreshGameSaveData refreshGameSaveData)
    {
        InitReferenceData(GameDataSaveManager.instance.UserGameSaveDataList);
    }

    protected override void Awake()
    {
        base.Awake();
        Copy.onClick.AddListener(CopyDataAsync);
        Delete.onClick.AddListener(DeleteData);
        Start.onClick.AddListener(StartAction);
        Return.onClick.AddListener(async () =>
        {
           await UIManager.instance.ShowGamePanel<ZeroPanel>();
            Close();
        });
        SaveReference.index = -1;
        saveList = new DisplayList<SaveReference, UserGameSaveData>(SaveReference, SaveDataParent);
    }

    private int selectedIndex = -1;
    private void SelectAction(UserGameSaveData userGameSaveData,int index, bool selected)
    {
        if (selected)
        {
            selectedIndex = index;
            selectGameSaveData = userGameSaveData;
            var dataIsNull = userGameSaveData == null || string.IsNullOrEmpty(userGameSaveData.saveTime) ||
                             userGameSaveData.playerData.gender == Gender.animal;
            Copy.interactable = !dataIsNull;
            Delete.interactable = !dataIsNull && userGameSaveData.index >= 0; 
            Start.interactable = !dataIsNull;
        }
    }

    private UserGameSaveData selectGameSaveData;

    public override void InitReferenceData(UserGameSaveDataList v)
    {
        base.InitReferenceData(v);
        // 面板引用数据入口保持同步，存档列表加载异常统一进入日志。
        AsyncTaskRunner.Run(InitReferenceDataAsync(v), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(UserGameSaveDataList v)
    {
        selectGameSaveData = v.nowSaveData;
        await SaveReference.InitData(v.nowSaveData, SelectAction, toggleGroup);
        await saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup);
        SaveReference.SelectDefault();
    }

    private void StartAction()
    {
        if (selectGameSaveData == null)
        {
            GameDataSaveManager.instance.loadingIndex = selectedIndex;
            if (selectedIndex == -1)
                data.nowSaveData = UserGameSaveData.CreatSaveData(-1);
            else
                data.userGameSaveDatas[selectedIndex] = UserGameSaveData.CreatSaveData(selectedIndex);
        }
        else
        { 
            GameDataSaveManager.instance.loadingIndex = selectGameSaveData.index;
        }

        var startWorldInit = new StartWorldInit();
        GameActionManager.instance.QueueAction(startWorldInit);
        NPCManager.instance.CreateZeroNPC();
        // SceneManager.instance.SwitchScene("World");
        // UIManager.instance.ShowGamePanel<LoadingPanel>();

        Close();

        var types = new List<Type>
        {
            typeof(CharacterSelectInformationPanel),
            typeof(ZeroPanel),
            typeof(SelectCharacterPanel),
            typeof(SelectLoadPanel)
        };
        UIManager.instance.UnLoadPanel(types);
    }

    private async void CopyDataAsync()
    {
        if (selectGameSaveData != null && !string.IsNullOrEmpty(selectGameSaveData.saveTime))
        {
            GameDataSaveManager.instance.CopySaveData(selectGameSaveData);
            await SaveReference.InitData(data.nowSaveData, SelectAction, toggleGroup);
            await saveList.InitListData(data.userGameSaveDatas, SelectAction, toggleGroup);
            SaveReference.SelectDefault();
        }
        else
        {
        }
    }

    private async void DeleteData()
    {
        GameDataSaveManager.instance.DeleteSaveData(selectGameSaveData);
        await SaveReference.InitData(data.nowSaveData, SelectAction, toggleGroup);
        await saveList.InitListData(data.userGameSaveDatas, SelectAction, toggleGroup);
        SaveReference.SelectDefault();
    }
}
