using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SelectLoadPanel : GamePanel<UserGameSaveDataList>
{
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    SaveReference SaveReference;
    [SerializeField]
    Button Copy, Delete, Start, Return;
    [SerializeField]
    Transform SaveDataParent;

    DisplayList<SaveReference, UserGameSaveData> saveList;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Delete = FindChildGameObject<Button>("DeleteButton");
        Start = FindChildGameObject<Button>("StartButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup=GetComponent<ToggleGroup>(); 
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshGameSaveData>(RefreshGameSaveData);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshGameSaveData>(RefreshGameSaveData);
    }

    void RefreshGameSaveData(RefreshGameSaveData refreshGameSaveData)
    {
        InitReferenceData(GameDataSaveManager.instance.UserGameSaveDataList);
    }
    protected override void Awake()
    {
        base.Awake();
        Copy.onClick.AddListener(CopyData);
        Delete.onClick.AddListener(DeleteData);
        Start.onClick.AddListener(StartAction);
        Return.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<ZeroPanel>();
            Close();
        });

        saveList = new DisplayList<SaveReference, UserGameSaveData>(SaveReference, SaveDataParent);
    }
    void SelectAction(UserGameSaveData userGameSaveData,bool selected)
    {
        if (selected)
        {
            selectGameSaveData = userGameSaveData;
            bool dataIsNull= string.IsNullOrEmpty(userGameSaveData.saveTime);
            Copy.interactable = !dataIsNull;
            Delete.interactable = userGameSaveData.index > 0 && !dataIsNull;
            Start.interactable = !dataIsNull;
        }
    }
    UserGameSaveData selectGameSaveData;
    public override void InitReferenceData(UserGameSaveDataList v)
    {
        base.InitReferenceData(v);
        selectGameSaveData = default(UserGameSaveData);
        saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup) ;
    } 
    void StartAction()
    {
        if (!string.IsNullOrEmpty(selectGameSaveData.saveTime))
        { 
            ExploreManager.instance.EnterChapter(-1);
           // SceneManager.instance.SwitchScene("001");
            //UIManager.instance.ShowGamePanel<LoadingPanel>();

            Close();
        } 
    }
    void CopyData()
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
    void DeleteData()
    {
        GameDataSaveManager.instance.DeletaSaveData(selectGameSaveData);
    }  
}
