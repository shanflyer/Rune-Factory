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
                refreshPos = true,
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
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshPasture>(RefreshPasture);
    }


    void SetAnimalToPastureResult(bool result)
    {
        if (result)
        {

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
    void SelectPasture(Pasture pasture,int index,bool selected)
    {
        if (selected)
        {
            selectPasture = pasture;
            if (pasture!=null)
            {
                pastureNameText.SetSWText(pasture.name);

                int foodCase = PackageManager.instance.GetPackageCaseCount(pasture.foodPackage);
                int foodCount = PackageManager.instance.GetPackageItems(pasture.foodPackage).Count;
                foodCaseText.text = $"{foodCount}/{foodCase}";

               // int waterCase = PackageManager.instance.GetPackageCaseCount(pasture.waterPackage);
                //int waterCount = PackageManager.instance.GetPackageItems(pasture.waterPackage).Count;
                //waterCaseText.text = $"{waterCount}/{waterCase}";

                int productCase = PackageManager.instance.GetPackageCaseCount(pasture.productPackage);
                int productCount = PackageManager.instance.GetPackageItems(pasture.productPackage).Count;
                productCaseText.text = $"{productCount}/{productCase}";

                animalCaseText.text = $"{pasture.animals.Count}/{pasture.animalCase}";
                SetButton.gameObject.SetActive(pasture.animals.Count < pasture.animalCase);
            }
            else
            {
                pastureNameText.SetSWText("毁坏的牧场");
                foodCaseText.text = "--/--";
                waterCaseText.text= "--/--";
                productCaseText.text = "--/--";
                animalCaseText.text = "--/--";
                SetButton.gameObject.SetActive(false);
            }

        }
        else if(selectPasture==pasture)
        {
            SetButton.gameObject.SetActive(false);
        }


    }
    public override Task InitData(string dataKey)
    {
        return base.InitData(dataKey);
    }
    Vector2 animalIconSize = new Vector2(48, 48);
    void SelectAnimal(MyInt myInt,int index,bool select)
    {
        if (select)
        {
            animalId = myInt.value;
            if (animalId != 0)
            {
                Character character = CharacterManager.instance.GetCharacter(animalId);
                character.characterData.head.SetImageSprite(animalIcon, animalIconSize,Vector2.zero);
                //animalIcon.sprite = character.characterData.icon.sprite;
                animalNameText.SetSWText(character.name);
                SetButton.transform.localScale = Vector3.one;
                animalIcon.enabled = false;
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
        // 动物列表刷新绑定牧场面板生命周期，关闭后旧刷新不再选中条目。
        RunLifecycleTask(async token =>
        {
            await animals.InitListData(animalDatas, SelectAnimal, cancellationToken: token);
            if (ShouldStopLifecycleTask(token))
            {
                return;
            }

            if (animalDatas.Count == 0)
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
        }, nameof(RefreshAnimalList));
    }

    void RefreshPasture(RefreshPasture refreshPasturee)
    {
        if (selectPasture.instanceId == refreshPasturee.instanceId)
        {
           if(PastureManager.instance.GetPasture(selectPasture.instanceId,out selectPasture))
            {
                SelectPasture(selectPasture,selectPasture.index, true);
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
            pastureInfos[i].InitData(null, SelectPasture);
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
