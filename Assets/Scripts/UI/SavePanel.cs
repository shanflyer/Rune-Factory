using UnityEngine;
using UnityEngine.UI;

public class SavePanel : GamePanel<UserGameSaveDataList>
{
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private SaveReference SaveReference;

    [SerializeField]
    private Button Copy,  Save, Return;

    [SerializeField]
    private Transform SaveDataParent;

    private DisplayList<SaveReference, UserGameSaveData> saveList;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Save = FindChildGameObject<Button>("SaveButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup = GetComponent<ToggleGroup>();
    }

    protected override void Awake()
    {
        base.Awake();
        Copy.onClick.AddListener(CopyData);
        Save.onClick.AddListener(SaveAction);
        Return.onClick.AddListener(() =>
        { 
            Close();
        });

        saveList = new DisplayList<SaveReference, UserGameSaveData>(SaveReference, SaveDataParent);
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

    private UserGameSaveData selectGameSaveData;

    private void SelectAction(UserGameSaveData userGameSaveData, int index, bool selected)
    {
        if (selected)
        {
            selectGameSaveData = userGameSaveData; 
            bool dataIsNull = string.IsNullOrEmpty(userGameSaveData.saveTime);
            Copy.interactable = !dataIsNull;
        }
    }

    public override void InitReferenceData(UserGameSaveDataList v)
    {
        base.InitReferenceData(v);
        // 存档面板重开时取消旧列表刷新，避免旧 SaveReference 再次抢选中态。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(UserGameSaveDataList v, System.Threading.CancellationToken cancellationToken)
    {
        //selectGameSaveData = null;
       await saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup, cancellationToken: cancellationToken);
       if (ShouldStopLifecycleTask(cancellationToken))
       {
           return;
       }

       saveList.SelectDefault();
    }

    private void SaveAction()
    {
        if (GameDataSaveManager.instance.SaveData(selectGameSaveData.index))
        {
            RefreshGameSaveData RefreshGameSaveData = new RefreshGameSaveData { };
            GameActionManager.instance.QueueAction(RefreshGameSaveData);
        }
    }

    private void CopyData()
    {
        if (!string.IsNullOrEmpty(selectGameSaveData.saveTime))
        {
            if (GameDataSaveManager.instance.CopySaveData(selectGameSaveData))
            {
            }
            else
            {
            }
        }
        else
        {
        }
    }

}
