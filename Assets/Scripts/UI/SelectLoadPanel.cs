using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SelectLoadPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    SaveReference SaveReference;
    [SerializeField]
    Button Copy, Delete, Start, Return;
    [SerializeField]
    Transform SaveDataParent;
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

        Copy.onClick.AddListener(CopyData);
        Delete.onClick.AddListener(DeleteData);
        Start.onClick.AddListener(StartAction);
        Return.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<ZeroPanel>();
            Close();
        });

        SaveReference.SetToggleGroup(toggleGroup); 
        //Delete.interactable = false;
    }

    public override Task InitData(string dataKay)
    {
        if (DataSaveAndLoadTest.LoadSaveData(0))
        {
            GameSaveData gameSaveData = DataSaveAndLoadTest.gameSaveData;
            SaveReference.Refresh(gameSaveData, 0);
        }  
        for(int i = 1; i <= 3; i++)
        {
            if (DataSaveAndLoadTest.LoadSaveData(i))
            {
                GameSaveData ManualSaveData = DataSaveAndLoadTest.gameSaveData;
                var saveReference = Instantiate(SaveReference, SaveDataParent);
                saveReference.Refresh(ManualSaveData, i);
                saveReference.SetToggleGroup(toggleGroup);
            }
           
        } 
        return base.InitData(dataKay);
    }

    void StartAction()
    {
        if (DataSaveAndLoadTest.CheckSaveData(SelectedIndex))
        {
            DataSaveAndLoadTest.LoadSaveData(SelectedIndex);
            DataSaveAndLoadTest.isJsonData = true;
            ExploreManager.instance.EnterChapter(-1);
           // SceneManager.instance.SwitchScene("001");
            //UIManager.instance.ShowGamePanel<LoadingPanel>();

            Close();
        } 
    }
    void CopyData()
    {
        if (!DataSaveAndLoadTest.CheckSaveData(SelectedIndex))
        {
            DataSaveAndLoadTest.CreatSaveData(SelectedIndex);
        }
        else
        {
            bool copySuccess = false;
            for (int target = 1; target <= 3; target++)
            {
                if (!DataSaveAndLoadTest.CheckSaveData(target))
                {
                    DataSaveAndLoadTest.CopySaveData(SelectedIndex, target);
                    copySuccess = true;
                    break;
                }
            }
            if (!copySuccess)
            {
                //zeroSceneStart.InitTipsData(LanguageManage.SwitchStr("提示"), LanguageManage.SwitchStr("没有空白存档，无法复制！"));
            }
        } 
    }
    void DeleteData()
    {
        DataSaveAndLoadTest.DeletaSaveData(SelectedIndex);
    }
    int SelectedIndex;
    public void RefreshDataFuncButton(int index,bool nullData)
    {
        SelectedIndex = index;
        Copy.interactable = !nullData;
        Delete.interactable = index>0&& !nullData;
        Start.interactable = !nullData;
    }
}
