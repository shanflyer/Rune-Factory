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
        if (!SingletonType.Cleared && GameActionManager.HasInstance)
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
        Return.onClick.AddListener(() =>
        {
            RunLifecycleTask(_ => ReturnToZeroAsync(), nameof(Return));
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
        // 存档列表刷新绑定面板生命周期，关闭或重开后旧结果不再覆盖新状态。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task ReturnToZeroAsync()
    {
        await UIManager.instance.ShowGamePanel<ZeroPanel>();
        Close();
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(UserGameSaveDataList v, System.Threading.CancellationToken cancellationToken)
    {
        selectGameSaveData = v.nowSaveData;
        await SaveReference.InitData(v.nowSaveData, SelectAction, toggleGroup, cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        await saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

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

    private void CopyDataAsync()
    {
        RunLifecycleTask(CopyDataTaskAsync, nameof(CopyDataAsync));
    }

    private async System.Threading.Tasks.Task CopyDataTaskAsync(System.Threading.CancellationToken cancellationToken)
    {
        if (selectGameSaveData != null && !string.IsNullOrEmpty(selectGameSaveData.saveTime))
        {
            GameDataSaveManager.instance.CopySaveData(selectGameSaveData);
            await SaveReference.InitData(data.nowSaveData, SelectAction, toggleGroup, cancellationToken);
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

            await saveList.InitListData(data.userGameSaveDatas, SelectAction, toggleGroup, cancellationToken: cancellationToken);
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

            SaveReference.SelectDefault();
        }
        else
        {
        }
    }

    private void DeleteData()
    {
        RunLifecycleTask(DeleteDataAsync, nameof(DeleteData));
    }

    private async System.Threading.Tasks.Task DeleteDataAsync(System.Threading.CancellationToken cancellationToken)
    {
        GameDataSaveManager.instance.DeleteSaveData(selectGameSaveData);
        await SaveReference.InitData(data.nowSaveData, SelectAction, toggleGroup, cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        await saveList.InitListData(data.userGameSaveDatas, SelectAction, toggleGroup, cancellationToken: cancellationToken);
        if (ShouldStopLifecycleTask(cancellationToken))
        {
            return;
        }

        SaveReference.SelectDefault();
    }
}
