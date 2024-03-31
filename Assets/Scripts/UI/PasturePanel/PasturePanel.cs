using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class PasturePanel : GamePanel<MyListInt>
{
    [SerializeField]
    PastureInfo[] pastureInfos;
    [SerializeField]
    TextMeshProUGUI pastureNameText;
    [SerializeField]
    TextMeshProUGUI animalCaseText;
    [SerializeField]
    TextMeshProUGUI foodCaseText;
    [SerializeField]
    TextMeshProUGUI waterCaseText;
    [SerializeField]
    TextMeshProUGUI productCaseText;
    [SerializeField]
    Image animalIcon;
    [SerializeField]
    TextMeshProUGUI animalNameText;
    [SerializeField]
    Button SetButton;
    [SerializeField]
    Button CloseButton;
    [SerializeField]
    Transform animalParent;
    [SerializeField]
    AnimalReference animalReference;
    DisplayList<AnimalReference, MyInt> animals;


    protected override void Awake()
    {
        base.Awake();
        SetButton.onClick.AddListener(() =>
        {
            SetAnimalToPasture setAnimalToPasture = new SetAnimalToPasture
            {
                animalId = animalId,
                pastureId = selectPasture.instanceId,
                refreshPos=!TeamManager.instance.playerTeam.CheckCharacter(animalId),
                setResult=SetAnimalToPastureResult
            };
            GameActionManager.instance.QueueAction(setAnimalToPasture);

        });
        CloseButton.onClick.AddListener(Close);
        animals = new DisplayList<AnimalReference, MyInt>(animalReference, animalParent);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPasture>(RefreshPasture);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshPasture>(RefreshPasture);
    }


    void SetAnimalToPastureResult(bool result)
    {
        if (result)
        { 
            LeaveTeam leaveTeam = new LeaveTeam
            {
                teamCharacterId = animalId
            };
            GameActionManager.instance.QueueAction(leaveTeam);
            animalList.intList.Remove(animalId);
            RefreshAnimalList();
        } 
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        pastureInfos = gameObject.GetComponentsInChildren<PastureInfo>(true);
        pastureNameText = FindChildGameObject<TextMeshProUGUI>("PastureName");
        animalCaseText = FindChildGameObject<TextMeshProUGUI>("AnimalCase");
        foodCaseText = FindChildGameObject<TextMeshProUGUI>("ProductCase");
        waterCaseText = FindChildGameObject<TextMeshProUGUI>("WaterCase");
        productCaseText = FindChildGameObject<TextMeshProUGUI>("ProductCase");
        animalIcon = FindChildGameObject<Image>("AnimalIcon");
        animalNameText = FindChildGameObject<TextMeshProUGUI>("AnimalName");
        SetButton = FindChildGameObject<Button>("SetButton");
        CloseButton = FindChildGameObject<Button>("Close");
        animalReference = FindChildGameObject<AnimalReference>("AnimalReference");
        animalParent = FindChildGameObject("Animals");
    }
    Pasture selectPasture;
    void SelectPasture(Pasture pasture,bool selected)
    {
        if (selected)
        {
            selectPasture = pasture;
            if (pasture.instanceId != 0)
            {
                pastureNameText.text = pasture.name.ToString();

                int foodCase = PackageManager.instance.GetPackageCaseCount(pasture.foodPackage);
                int foodCount = PackageManager.instance.GetPackageItems(pasture.foodPackage).Count;
                foodCaseText.text = $"{foodCount}/{foodCase}";

                int waterCase = PackageManager.instance.GetPackageCaseCount(pasture.waterPackage);
                int waterCount = PackageManager.instance.GetPackageItems(pasture.waterPackage).Count;
                waterCaseText.text = $"{waterCount}/{waterCase}";

                int productCase = PackageManager.instance.GetPackageCaseCount(pasture.productPackage);
                int productCount = PackageManager.instance.GetPackageItems(pasture.productPackage).Count;
                productCaseText.text = $"{productCount}/{productCase}";

                animalCaseText.text = $"{pasture.animals.Count}/{pasture.animalCase}";
                SetButton.interactable = true;
            }
            else
            {
                pastureNameText.text = "»Ù»µµÄÄÁ³¡";
                foodCaseText.text = "--/--";
                waterCaseText.text= "--/--";
                productCaseText.text = "--/--";
                animalCaseText.text = "--/--";
                SetButton.interactable = false;
            }
         
        }
        else if(selectPasture.instanceId==pasture.instanceId)
        {
            SetButton.interactable = false;
        }
       

    }
    public override Task InitData(string dataKey)
    {
        return base.InitData(dataKey);
    }
    
    void SelectAnimal(MyInt myInt,bool select)
    {
        if (select)
        {
            animalId = myInt.value;
            if (animalId != 0)
            {
                Character character = CharacterManager.instance.GetCharacter(animalId);
                animalIcon.sprite = character.characterData.icon;
                animalNameText.text = character.name;
                SetButton.transform.localScale = Vector3.one;
                animalIcon.enabled = true;
                animalNameText.enabled = true;
            }
            else
            {
                SetButton.transform.localScale = Vector3.zero;
                animalIcon.enabled = false;
                animalNameText.enabled = false;
            }
        }
        else if(animalId==myInt.value)
        {
            animalId = 0;
            SetButton.transform.localScale = Vector3.zero;
            animalIcon.enabled = false;
            animalNameText.enabled = false;
        }
    }

    int animalId = 0;
    MyListInt animalList;
    void RefreshAnimalList()
    {
        List<MyInt> animalDatas = new List<MyInt>();
        for (int i = 0; i < animalList.intList.Count; i++)
        {
            int id = animalList.intList[i];
            if (PastureManager.instance.CheckAnimal(id))
            {
                MyInt myInt = new MyInt
                {
                    value = animalList.intList[i]
                };
                animalDatas.Add(myInt);
            } 
        }
        animals.InitListData(animalDatas, SelectAnimal);

        if (animalList.intList.Count == 0)
        {
            SetButton.transform.localScale = Vector3.zero;
            animalIcon.enabled = false;
            animalNameText.enabled = false;
        }
        else
        {
            animals.ClearSelect();
            animals.SelectDefault();
        }
    }

    void RefreshPasture(RefreshPasture refreshPasturee)
    {
        if (selectPasture.instanceId == refreshPasturee.instanceId)
        {
           if(PastureManager.instance.GetPasture(selectPasture.instanceId,out selectPasture))
            {
                SelectPasture(selectPasture, true);
            }
        }
    }

    public override void InitReferenceData(MyListInt v)
    {
        base.InitReferenceData(v);
        animalList = v;
        RefreshAnimalList();


        for (int i = 0; i < pastureInfos.Length; i++)
        {
            pastureInfos[i].InitData(default(Pasture), SelectPasture);
        }
        var allPastures = PastureManager.instance.GetAllPasture();
        for(int i = 0; i < allPastures.Count; i++)
        {
            var p = allPastures[i];
            if (p.index != 0)
            {
                pastureInfos[p.index].InitData(p, SelectPasture); 
            }
        }  
        pastureInfos[0].SelectDefault();
    }
}
