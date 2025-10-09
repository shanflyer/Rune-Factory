using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LanguagePanel : GamePanel<IReferenceData>
{
   
    [SerializeField]
    private Button  returnButton; 
    [SerializeField]
    private LanguageReference languageReference;
    [SerializeField]
    private Transform languageParent;
    DisplayList<LanguageReference, LanguageData> languages;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();  
        languageReference = FindChildGameObject<LanguageReference>("LanguageReference");
        languageParent = FindChildGameObject("LanguageParent");
        returnButton = FindChildGameObject<Button>("ReturnButton");
    }

    protected override void Awake()
    {
        base.Awake();
        languages = new DisplayList<LanguageReference, LanguageData>(languageReference, languageParent);

        returnButton.onClick.AddListener(() =>
        {
            Close();
        }); 
          
    }
    public override async Task InitData(string dataKey)
    {
       await base.InitData(dataKey); 
       
        RefreshLanguage();
    }
    void RefreshLanguage()
    {
        languages.InitListData(LanguageManage.instance.languageDatas, (LanguageData languageData,int index, bool selected) =>
        {
            if (selected)
            {
                LanguageManage.instance.SetLanguage(languageData.languageType);
            }
        });
    } 
}