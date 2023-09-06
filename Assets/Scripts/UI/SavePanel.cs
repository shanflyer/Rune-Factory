using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SavePanel : GamePanel<IReferenceData>
{
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    SaveReference SaveReference;
    [SerializeField]
    Button Copy, Delete, Save, Return;
    [SerializeField]
    Transform SaveDataParent;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Delete = FindChildGameObject<Button>("DeleteButton");
        Save = FindChildGameObject<Button>("SaveButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup=GetComponent<ToggleGroup>();

        Copy.onClick.AddListener(CopyData);
        Delete.onClick.AddListener(DeleteData);
        Save.onClick.AddListener(SaveAction);
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

    void SaveAction()
    {
        if (DataSaveAndLoadTest.CheckSaveData(SelectedIndex))
        {
            DataSaveAndLoadTest.LoadSaveData(SelectedIndex);
            DataSaveAndLoadTest.isJsonData = true; 

            Close();
        } 
    }
    void CopyData()
    {
        DataSaveAndLoadTest.gameSaveData.SaveData();
        DataSaveAndLoadTest.CreatSaveData(SelectedIndex);
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
        Save.interactable = !nullData;
    }
}
